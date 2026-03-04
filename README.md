# Nexus – Full-Stack Task Tracker

A small **Task Tracker** with a **C# ASP.NET Core 8 Web API** and a **TypeScript React SPA** that consumes it.

## Tech stack

| Layer   | Technology |
|--------|------------|
| Backend | .NET 8, ASP.NET Core, EF Core InMemory, Swagger |
| Frontend | React 19, TypeScript, Vite 7, React Router 7 |
| Tests   | xUnit (API), Vitest (frontend) |

## Quick start

### Prerequisites

- **.NET SDK 8.x** – [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** – [Download](https://nodejs.org/)
- **npm** (comes with Node)

### Run the API

```bash
cd backend/Nexus.Api
dotnet run
```

- API: **http://localhost:5000**
- Swagger: **http://localhost:5000/swagger**

**JetBrains Rider:** Open **`Nexus.sln`** (in the repo root). Set **Nexus.Api** as the startup project (right‑click → Set as Startup Project), then Run (Shift+F10) or Debug (F5). **Before rebuilding:** stop the running app (Stop button or Shift+F5). Otherwise MSBuild will report "file is locked by Nexus.Api" because the process holds the DLLs—stop the run, then build again.

### Run the SPA

```bash
cd frontend
npm install
npm run dev
```

- App: **http://localhost:5173**

Configure the API base URL (optional): copy `frontend/.env.example` to `frontend/.env` and set `VITE_API_BASE_URL=http://localhost:5000` if your API runs on a different port.

### CORS

The API allows origins from **appsettings.json** → `Cors:AllowedOrigins` (default: `http://localhost:5173`, `http://localhost:3000`). Adjust if your SPA runs on another port.

## Project layout

```
nexus/
├── backend/
│   ├── Nexus.Api/           # ASP.NET Core API (controllers, options, validation, middleware)
│   ├── Nexus.Application/   # DTOs, contracts (ITaskService, ITaskRepository), TaskService
│   ├── Nexus.Domain/         # TaskItem, TaskStatus, TaskPriority
│   ├── Nexus.Infrastructure/ # EF Core DbContext, migrations, TaskRepository (InMemory)
│   └── Nexus.Api.Tests/      # xUnit integration tests
├── frontend/                 # React + TypeScript SPA
│   ├── src/
│   │   ├── api/              # API client
│   │   ├── config/           # API base URL from env
│   │   ├── context/          # TasksContext (Context API)
│   │   ├── hooks/            # useTask, useCreateTask, useUpdateTask, useApiError
│   │   ├── pages/            # Home, CreateTask, EditTask
│   │   ├── components/       # TaskList, TaskForm
│   │   ├── types/            # Task, ProblemDetails
│   │   └── utils/            # mapApiErrorToMessage (+ test)
│   └── ...
├── README.md
└── SOLUTION.md               # Design, trade-offs, debugging notes
```

## API overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET    | `/api/tasks` | List tasks. Query: `q` (search), `sort=dueDate:asc\|desc` (default asc). |
| GET    | `/api/tasks/{id}` | Get one task. |
| POST   | `/api/tasks` | Create task. Body: title, description, status, priority, dueDate (optional). |
| PUT    | `/api/tasks/{id}` | Update task. |
| DELETE | `/api/tasks/{id}` | Delete task. |

Validation and errors return **RFC 7807 ProblemDetails** (e.g. 400 with `detail` and `errors`).

## Tests

**Backend (xUnit)**

```bash
cd backend/Nexus.Api.Tests
dotnet test
```

- Happy path: `GET /api/tasks` returns seeded data.
- Validation: invalid `POST` returns 400 with ProblemDetails.

**Frontend (Vitest)**

```bash
cd frontend
npm run test:run
```

- Unit test: `mapApiErrorToMessage` maps API errors to user-facing messages.

## Versions (as of this implementation)

- .NET SDK: 8.0.x
- Node: 20+
- React: 19.x
- Vite: 7.x
- React Router: 7.x

See `frontend/package.json` and `backend/Nexus.Api/Nexus.Api.csproj` for exact versions.

## EF Core migrations

Migrations are enabled and live in **Nexus.Infrastructure/Migrations**. The app currently uses the **InMemory** provider (no migration is applied at runtime). To switch to a real database (e.g. SQLite or SQL Server):

1. Change `DatabaseConfiguration.cs` in Nexus.Api to use `UseSqlite(...)` or `UseSqlServer(...)`.
2. From the repo root, run:
   ```bash
   dotnet ef database update --project backend/Nexus.Infrastructure --startup-project backend/Nexus.Api
   ```
   (Install the [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) if needed: `dotnet tool install --global dotnet-ef`.)

## Docker and Kubernetes (Rancher / Argo CD)

The app is containerized and deployable to Kubernetes with **scalability** and **high availability** (multiple replicas, resource limits, liveness/readiness probes, PodDisruptionBudgets).

### Build images

From the **repository root**, build with Docker (or Rancher Desktop / nerdctl):

```bash
# Backend (context: backend/, Dockerfile: backend/Nexus.Api/Dockerfile)
docker build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend

# Frontend (context: frontend/)
docker build -t nexus-web:latest frontend
```

For a **registry** (so Rancher/Argo CD can pull):

```bash
docker tag nexus-api:latest YOUR_REGISTRY/nexus-api:latest
docker tag nexus-web:latest YOUR_REGISTRY/nexus-web:latest
docker push YOUR_REGISTRY/nexus-api:latest
docker push YOUR_REGISTRY/nexus-web:latest
```

### Deploy with Kubernetes

Manifests are in **`kubernetes/`** (Kustomize-based).

| Resource | Purpose |
|----------|---------|
| `namespace.yaml` | Namespace `nexus` |
| `configmap.yaml` | API env (e.g. `ASPNETCORE_URLS`) |
| `backend-deployment.yaml` | API: 2 replicas, resources, probes, **PodDisruptionBudget** |
| `frontend-deployment.yaml` | Web: 2 replicas, resources, probes, **PodDisruptionBudget** |
| `backend-service.yaml` / `frontend-service.yaml` | ClusterIP services |
| `ingress.yaml` | Optional; host `nexus.local` → web service |
| `kustomization.yaml` | Kustomize base; override `images` for your registry |

#### Kubernetes infrastructure

- **Namespace** (`nexus`) – Isolates all app resources.
- **ConfigMap** (`nexus-config`) – Non-secret config for the API: `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`. Injected into API pods via `envFrom`.
- **Backend Deployment** (`nexus-api`) – Runs the API image (port 5000). Two replicas for availability. Resource requests/limits (128Mi–512Mi memory, 100m–500m CPU). Liveness and readiness probes on `/health`. **PodDisruptionBudget** keeps at least one API pod available during voluntary disruptions (e.g. node drain).
- **Frontend Deployment** (`nexus-web`) – Runs the web image (port 80, nginx). Two replicas. Probes on `/`. **PodDisruptionBudget** keeps at least one web pod available. Nginx proxies `/api` and `/health` to the `nexus-api` service.
- **Services** – `nexus-api` (ClusterIP 5000) and `nexus-web` (ClusterIP 80) give stable DNS and load-balancing to the pods. Web pods call the API via `http://nexus-api:5000`.
- **Ingress** – Optional. Routes host `nexus.local` (or your host) to `nexus-web:80`. Requires an Ingress controller (e.g. nginx-ingress). TLS can be added via annotations or a TLS block.
- **Kustomization** – Declares all resources and default image names/tags; override `images` for your registry so `kubectl apply -k` or Argo CD use the correct images.

**Apply (after pushing images to a registry the cluster can pull):**

```bash
# Override images for your registry
cd kubernetes
kustomize edit set image nexus-api=YOUR_REGISTRY/nexus-api:latest nexus-web=YOUR_REGISTRY/nexus-web:latest

# Apply
kubectl apply -k .
```

Set **`imagePullPolicy`** and image names in the deployments if your cluster pulls from a private registry (e.g. `imagePullPolicy: IfNotPresent` or `Always` and image like `your-registry.io/nexus-api:v1.0.0`).

### Argo CD

To deploy via **Argo CD**, point an Application at the `kubernetes` directory (Kustomize):

1. Push this repo (with `kubernetes/`) to Git.
2. Edit **`kubernetes/argocd-application.yaml`**: set `spec.source.repoURL`, `targetRevision`, and `spec.source.kustomize.images` to your registry and tags.
3. Apply: `kubectl apply -f kubernetes/argocd-application.yaml`.

Argo CD will sync the Kustomize base; keep image tags in sync (e.g. with Argo CD Image Updater or CI that updates the Application/kustomization images).

### Local Kubernetes (e.g. Rancher Desktop)

If you build with **nerdctl** and load images into the cluster (no registry), use **imagePullPolicy: Never** in the deployments and the default image names `nexus-api:latest`, `nexus-web:latest`. Then:

```bash
nerdctl build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend
nerdctl build -t nexus-web:latest frontend
nerdctl save nexus-api:latest -o api.tar && nerdctl -n k8s.io load -i api.tar
nerdctl save nexus-web:latest -o web.tar && nerdctl -n k8s.io load -i web.tar
kubectl apply -k kubernetes
kubectl port-forward -n nexus svc/nexus-web 3000:80
# Open http://localhost:3000
```
