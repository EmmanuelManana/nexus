# Kubernetes deployment guide

This document explains how to deploy the Nexus Task Tracker to Kubernetes, step by step. The app runs as two workloads: **nexus-api** (ASP.NET Core) and **nexus-web** (React SPA served by nginx, with `/api` and `/health` proxied to the API).

---

## Table of contents

1. [Prerequisites](#1-prerequisites)
2. [What gets deployed](#2-what-gets-deployed)
3. [Step 1: Build container images](#3-step-1-build-container-images)
4. [Step 2: Make images available to the cluster](#4-step-2-make-images-available-to-the-cluster)
5. [Step 3: Apply the manifests](#5-step-3-apply-the-manifests)
6. [Step 4: Verify pods are running](#6-step-4-verify-pods-are-running)
7. [Step 5: Access the application](#7-step-5-access-the-application)
8. [Optional: Deploy with Argo CD](#8-optional-deploy-with-argo-cd)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. Prerequisites

- **kubectl** – [Install kubectl](https://kubernetes.io/docs/tasks/tools/) and ensure it targets your cluster (`kubectl cluster-info`).
- **Container runtime** – Either:
  - **Docker** or **nerdctl** to build images.
  - For **Rancher Desktop**: use nerdctl (default) or switch to dockerd in Settings → Container Engine so the `docker` CLI works.
- **Kubernetes cluster** – Any cluster (e.g. Rancher Desktop, minikube, cloud). The manifests use standard Kubernetes resources (Deployments, Services, optional Ingress).

---

## 2. What gets deployed

All manifests live in the **`kubernetes/`** directory and are wired together with **Kustomize**. The following resources are created in the **`nexus`** namespace.

| File | Resource | Purpose |
|------|----------|---------|
| `namespace.yaml` | Namespace | Creates the `nexus` namespace so all app resources are isolated. |
| `configmap.yaml` | ConfigMap | Holds non-secret config for the API: `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`. Injected into API pods via `envFrom`. |
| `backend-deployment.yaml` | Deployment + PodDisruptionBudget | Runs the **nexus-api** image (port 5000). Two replicas, resource requests/limits, liveness and readiness probes on `/health`. PDB keeps at least one API pod available during voluntary disruptions. |
| `frontend-deployment.yaml` | Deployment + PodDisruptionBudget | Runs the **nexus-web** image (port 80, nginx). Two replicas, probes on `/`. Nginx serves the SPA and proxies `/api` and `/health` to the API service. PDB keeps at least one web pod available. |
| `backend-service.yaml` | Service | ClusterIP service `nexus-api` on port 5000. Gives stable DNS and load-balancing to API pods. |
| `frontend-service.yaml` | Service | ClusterIP service `nexus-web` on port 80. Traffic to the web pods goes through this service. |
| `ingress.yaml` | Ingress | Optional. Routes host `nexus.local` (or your host) to `nexus-web:80`. Requires an Ingress controller (e.g. nginx-ingress). TLS can be enabled via the commented block. |
| `kustomization.yaml` | Kustomization | Lists all resources and default image names/tags. Override `images` here (or via `kustomize edit set image`) when using a registry. |

**Flow:** User → (optional Ingress) → **nexus-web** (nginx) → static files or proxy `/api` and `/health` to **nexus-api** (ASP.NET Core). The API uses in-memory storage (data does not persist across restarts).

---

## 3. Step 1: Build container images

From the **repository root** (the folder that contains `backend/` and `frontend/`), build both images.

### Option A: Using Docker

```bash
docker build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend
docker build -t nexus-web:latest frontend
```

- **Backend:** Build context is `backend/`; the Dockerfile is at `backend/Nexus.Api/Dockerfile`. The image runs the published ASP.NET Core app on port 5000.
- **Frontend:** Build context is `frontend/`. The image is a multi-stage build: Node builds the SPA, then nginx serves `dist` and proxies `/api` and `/health` to the backend (in K8s, nginx uses the service name `nexus-api:5000`).

### Option B: Using nerdctl (e.g. Rancher Desktop)

If you see `error during connect ... dockerDesktopLinuxEngine ... The system cannot find the file specified`, the `docker` CLI is targeting Docker Desktop, which is not running. Use **nerdctl** instead:

```bash
nerdctl build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend
nerdctl build -t nexus-web:latest frontend
```

**PowerShell:** Run each command on its own line; `&&` is not supported in Windows PowerShell 5.x.

---

## 4. Step 2: Make images available to the cluster

The cluster must be able to run the images. Choose one of the following.

### Path A: Local cluster with no registry (e.g. Rancher Desktop)

You load the images directly into the cluster’s image store so no pull from a registry is needed. The manifests use **`imagePullPolicy: Never`** so the kubelet only uses images already on the node.

1. **Save** each image to a tarball.
2. **Load** each tarball into the **k8s.io** namespace (used by the cluster).

**Bash / Git Bash:**

```bash
nerdctl save nexus-api:latest -o api.tar
nerdctl -n k8s.io load -i api.tar

nerdctl save nexus-web:latest -o web.tar
nerdctl -n k8s.io load -i web.tar
```

**PowerShell (run one command per line):**

```powershell
nerdctl save nexus-api:latest -o api.tar
nerdctl -n k8s.io load -i api.tar

nerdctl save nexus-web:latest -o web.tar
nerdctl -n k8s.io load -i web.tar
```

- **Why `-n k8s.io`?** Rancher Desktop’s Kubernetes uses the `k8s.io` containerd namespace for cluster images. Loading there makes the images visible to the scheduler.
- **Why `imagePullPolicy: Never`?** So the cluster does not try to pull from a registry; it only uses the images you just loaded.

### Path B: Registry (for remote clusters or Argo CD)

If your cluster pulls images from a registry (e.g. Docker Hub, GHCR, a private registry):

1. **Tag** the images with your registry URL:

   ```bash
   docker tag nexus-api:latest YOUR_REGISTRY/nexus-api:latest
   docker tag nexus-web:latest YOUR_REGISTRY/nexus-web:latest
   ```

2. **Push** them:

   ```bash
   docker push YOUR_REGISTRY/nexus-api:latest
   docker push YOUR_REGISTRY/nexus-web:latest
   ```

3. When applying manifests (Step 5), override the image names in Kustomize and set **`imagePullPolicy`** to `IfNotPresent` or `Always` in the deployment YAMLs (the repo default is `Never` for local load). Example override:

   ```bash
   cd kubernetes
   kustomize edit set image nexus-api=YOUR_REGISTRY/nexus-api:latest nexus-web=YOUR_REGISTRY/nexus-web:latest
   ```

   Then edit `backend-deployment.yaml` and `frontend-deployment.yaml` and change `imagePullPolicy: Never` to `imagePullPolicy: IfNotPresent`.

---

## 5. Step 3: Apply the manifests

From the repository root:

```bash
cd kubernetes
kubectl apply -k .
```

- **`kubectl apply -k .`** uses Kustomize to build the final manifests from the `kustomization.yaml` and the listed resource files, then applies them to the current cluster. All resources are created in the **nexus** namespace.

You should see output like:

```
namespace/nexus created
configmap/nexus-config created
service/nexus-api created
service/nexus-web created
deployment.apps/nexus-api created
deployment.apps/nexus-web created
poddisruptionbudget.policy/nexus-api-pdb created
poddisruptionbudget.policy/nexus-web-pdb created
ingress.networking.k8s.io/nexus created
```

---

## 6. Step 4: Verify pods are running

1. **List pods** in the `nexus` namespace:

   ```bash
   kubectl get pods -n nexus
   ```

   Wait until both `nexus-api` and `nexus-web` pods show status **Running** and ready (e.g. `2/2` in the READY column).

2. **If a pod stays Pending or CrashLoopBackOff**, inspect it:

   ```bash
   kubectl describe pod -n nexus -l app=nexus-web
   kubectl describe pod -n nexus -l app=nexus-api
   ```

   Check the **Events** section at the bottom. Common causes:
   - **Image not found** – For local load: ensure you ran `nerdctl -n k8s.io load` and that the deployment uses `imagePullPolicy: Never`. For registry: ensure image names and tags match and the cluster can pull.
   - **Scheduling** – Not enough CPU/memory, or node taints. Adjust `resources` in the deployments if needed.

3. **Optional: restart deployments** after fixing image or config:

   ```bash
   kubectl rollout restart deployment/nexus-api deployment/nexus-web -n nexus
   kubectl get pods -n nexus -w
   ```

   Press Ctrl+C when all pods are Running.

---

## 7. Step 5: Access the application

The frontend is served on **port 80** inside the cluster. You can access it in two ways.

### Option A: Port-forward (no Ingress)

Forward the **nexus-web** service port 80 to a port on your machine (e.g. 3000):

```bash
kubectl port-forward -n nexus svc/nexus-web 3000:80
```

Then open **http://localhost:3000** in your browser. The SPA will load; API calls go to the same origin and nginx proxies `/api` to the backend.

Leave the command running; stop with Ctrl+C when done.

### Option B: Ingress (cluster has an Ingress controller)

The `ingress.yaml` routes host **nexus.local** to `nexus-web:80`. To use it:

1. **Install an Ingress controller** if needed (e.g. [nginx-ingress](https://kubernetes.github.io/ingress-nginx/deploy/), or use Rancher’s built-in).
2. **Point the host** to the Ingress controller’s address. For a local cluster you can add to your hosts file:
   - Windows: `C:\Windows\System32\drivers\etc\hosts`
   - Add a line: `127.0.0.1 nexus.local` (replace with the actual LB or node IP if different).
3. Open **http://nexus.local** in your browser.

To enable TLS, uncomment and fill the `tls` block in `kubernetes/ingress.yaml` and create the referenced secret (e.g. with cert-manager or a manual TLS secret).

---

## 8. Optional: Deploy with Argo CD

To deploy and keep the app in sync with Git using **Argo CD**:

1. **Push the repo** (including the `kubernetes/` directory) to a Git remote (e.g. GitHub).
2. **Edit** `kubernetes/argocd-application.yaml`:
   - Set `spec.source.repoURL` to your repo URL (e.g. `https://github.com/YOUR_ORG/nexus.git`).
   - Set `spec.source.targetRevision` (e.g. `main`).
   - Under `spec.source.kustomize.images`, set your registry and tags, e.g.:
     - `nexus-api:YOUR_REGISTRY/nexus-api:latest`
     - `nexus-web:YOUR_REGISTRY/nexus-web:latest`
3. **Apply** the Application (Argo CD must be installed in the cluster):

   ```bash
   kubectl apply -f kubernetes/argocd-application.yaml
   ```

Argo CD will sync the `kubernetes/` directory (Kustomize) into the `nexus` namespace. Use **Argo CD Image Updater** or CI to update image tags when you push new builds.

---

## 9. Troubleshooting

| Issue | What to do |
|------|------------|
| **Pods stuck in Pending** | Run `kubectl describe pod -n nexus <pod-name>` and read **Events**. Fix image name / imagePullPolicy or resource/taint issues, then re-apply or restart deployments. |
| **`docker` connect error (Rancher Desktop)** | Use **nerdctl** for builds and follow Path A (load into `k8s.io`), or switch Rancher Desktop to **dockerd (moby)** in Settings → Container Engine. |
| **Port-forward says “pod is not running”** | Pods are still Pending or not ready. Wait for `kubectl get pods -n nexus` to show Running, then run port-forward again. |
| **Frontend loads but API calls fail** | Ensure you are hitting the app through **nexus-web** (port-forward or Ingress). The SPA expects same-origin; nginx in nexus-web proxies `/api` to the nexus-api service. |
| **Ingress returns 404 or no route** | Confirm an Ingress controller is installed and that the Ingress’s `ingressClassName` matches it. Check `kubectl get ingress -n nexus` and controller logs. |
| **Need to change API config** | Edit `kubernetes/configmap.yaml` (e.g. `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), then run `kubectl apply -k kubernetes` and restart the API deployment: `kubectl rollout restart deployment/nexus-api -n nexus`. |

---

## Quick reference: local Rancher Desktop (full sequence)

From the repo root, using **nerdctl** and no registry:

```bash
# Build
nerdctl build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend
nerdctl build -t nexus-web:latest frontend

# Load into cluster
nerdctl save nexus-api:latest -o api.tar
nerdctl -n k8s.io load -i api.tar
nerdctl save nexus-web:latest -o web.tar
nerdctl -n k8s.io load -i web.tar

# Deploy
cd kubernetes
kubectl apply -k .

# Wait for pods
kubectl get pods -n nexus -w
# Ctrl+C when Running

# Access (in another terminal)
kubectl port-forward -n nexus svc/nexus-web 3000:80
# Open http://localhost:3000
```

**PowerShell:** Run the save/load commands as separate lines (no `&&`).
