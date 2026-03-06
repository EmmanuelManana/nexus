# How to run the Nexus Task Tracker on Rancher Desktop (simple guide)

This guide is for anyone who wants to run the app **locally** using **Rancher Desktop** and **Kubernetes**. You will use a **terminal** (command line) to type or paste commands. No prior experience with Kubernetes is required—just follow each step in order.

---

## What you need before you start

1. **Rancher Desktop** installed on your computer.  
   - If you don’t have it: download from [rancherdesktop.io](https://rancherdesktop.io/) and install it.  
   - Open Rancher Desktop and wait until it says **Kubernetes is running** (or similar). This may take a minute the first time.

2. **The Nexus project** on your computer.  
   - You should have a folder (e.g. `nexus`) that contains:
     - a `backend` folder  
     - a `frontend` folder  
     - a `kubernetes` folder  

3. **A terminal (command prompt).**  
   - **Windows:** Open **PowerShell** or **Windows Terminal**.  
   - **Mac/Linux:** Open **Terminal**.  
   - You will run all commands from the **project folder** (the one that contains `backend`, `frontend`, and `kubernetes`).

---

## Step 1: Open the terminal in the project folder

**Do this:**  
Open your terminal and go to the Nexus project folder.

- **Windows (PowerShell):**  
  - Press `Win + R`, type `powershell`, press Enter.  
  - Type: `cd` followed by a space, then **drag the project folder** into the window and press Enter.  
  - Or type the path, e.g. `cd C:\Users\YourName\Documents\code\nexus`
- **Mac/Linux:**  
  - Open Terminal, then type: `cd` followed by a space, then drag the project folder into the window and press Enter.

**Check:**  
Type:

```powershell
dir
```

(On Mac/Linux use `ls` instead of `dir`.)

You should see folders like `backend`, `frontend`, and `kubernetes`. If you do, you’re in the right place.

---

## Step 2: Build the backend (API) image

**What this does:**  
The app has two parts: a **backend** (API) and a **frontend** (website). Each part is packaged into an **image**—a file that Kubernetes can run. This step builds the backend image.

**Do this:**  
Copy and paste this command into the terminal and press Enter:

```powershell
nerdctl build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend
```

**You should see:**  
A lot of lines of output while it downloads and builds. At the end you should see something like “exporting to docker image format” and “Loaded image” for `nexus-api:latest`.  
It can take **2–5 minutes** the first time. Wait until the command finishes and you get your prompt back.

**If you see an error about “dockerDesktopLinuxEngine”:**  
You’re using Rancher Desktop with **nerdctl**. That’s correct—keep using `nerdctl` (don’t use `docker`). If the error persists, in Rancher Desktop go to **Settings → Container Engine** and try switching to **dockerd (moby)**, then run the command again using `docker` instead of `nerdctl`.

---

## Step 3: Build the frontend (website) image

**What this does:**  
This step builds the image for the website (frontend) that you will see in the browser.

**Do this:**  
Copy and paste this command and press Enter:

```powershell
nerdctl build -t nexus-web:latest frontend
```

**You should see:**  
Again, lots of output, then at the end something like “Loaded image: nexus-web:latest”. This may also take **2–5 minutes** the first time.

---

## Step 4: Save the backend image to a file

**What this does:**  
Kubernetes needs the images to be in a special place. We save the backend image to a file so we can load it into Kubernetes in the next step.

**Do this:**  
Run these two commands **one at a time** (press Enter after each). Make sure you are still in the **project folder** (the one that contains `backend`, `frontend`, and `kubernetes`), otherwise the second command may say “no such file or directory” because it can’t find `api.tar`.

```powershell
nerdctl save nexus-api:latest -o api.tar
```

```powershell
nerdctl -n k8s.io load -i api.tar
```

**You should see:**  
After the first command, nothing much. After the second, something like “Loaded image: nexus-api:latest” or “unpacking … nexus-api:latest”.

---

## Step 5: Save and load the frontend image

**What this does:**  
Same as Step 4, but for the website (frontend) image.

**Do this:**  
Run these two commands **one at a time**. Stay in the **project folder** so `web.tar` is found.

```powershell
nerdctl save nexus-web:latest -o web.tar
```

```powershell
nerdctl -n k8s.io load -i web.tar
```

**You should see:**  
After the second command, something like “Loaded image: nexus-web:latest”.

---

## Step 6: Deploy the app to Kubernetes

**What this does:**  
This tells Kubernetes to create everything it needs to run the app: a namespace, the backend and frontend apps (each with 2 copies for reliability), and internal networking.

**Do this:**  
First, go into the `kubernetes` folder:

```powershell
cd kubernetes
```

Then run:

```powershell
kubectl apply -k .
```

**You should see:**  
A list of lines saying “created” for things like:

- namespace/nexus created  
- configmap/nexus-config created  
- service/nexus-api created  
- service/nexus-web created  
- deployment.apps/nexus-api created  
- deployment.apps/nexus-web created  
- (and a few more)

If you see “created” or “configured” for these, the deploy worked.

---

## Step 7: Wait until the app is running

**What this does:**  
Kubernetes needs a short time to start the backend and frontend. This step lets you watch until both are “Running”.

**Do this:**  
Run:

```powershell
kubectl get pods -n nexus
```

**You should see:**  
A table with rows for pods named something like `nexus-api-xxxxx` and `nexus-web-xxxxx`. In the **STATUS** column you want to see **Running**, and in the **READY** column something like **2/2** or **1/1**.

- If you see **Pending** or **ContainerCreating**, wait **30–60 seconds** and run the same command again.  
- If after 2–3 minutes some pods are still not **Running**, see [If something goes wrong](#if-something-goes-wrong) below.

When all pods show **Running** and are ready, go to the next step.

---

## Step 8: Connect your browser to the app

**What this does:**  
The app is running inside Kubernetes, but it’s not visible on the internet. **Port-forwarding** creates a tunnel so that when you open a address on your computer (localhost), the traffic goes to the app inside Kubernetes.

**Do this:**  
Run this command and **leave it running** (do not close the terminal):

```powershell
kubectl port-forward -n nexus svc/nexus-web 3000:80
```

**You should see:**  
Something like “Forwarding from 127.0.0.1:3000 -> 80” or “Forwarding from [::1]:3000 -> 80”. There will be no new prompt—that’s normal. Leave this window open.

---

## Step 9: Open the app in your browser

**Do this:**  
Open your web browser (Chrome, Edge, Firefox, etc.) and go to:

**http://localhost:3000**

**You should see:**  
The Nexus Task Tracker: a page with “Task Tracker” at the top, a search box, a list of tasks, and a “New task” link or button. You can click around, create a task, and edit tasks. The app is running locally on your machine via Rancher and Kubernetes.

**When you’re done:**  
Go back to the terminal where `kubectl port-forward` is running and press **Ctrl+C** to stop the tunnel. The app will still be running in Kubernetes, but you won’t be able to reach it at localhost:3000 until you run the port-forward command again.

---

## Summary: what we did

1. **Built** two images (backend API and frontend website) using `nerdctl`.  
2. **Loaded** those images into Kubernetes’ image store so the cluster could use them.  
3. **Deployed** the app with `kubectl apply -k .` so Kubernetes started the backend and frontend (with 2 copies of each).  
4. **Waited** until the pods were Running.  
5. **Port-forwarded** port 3000 on your computer to the website running in Kubernetes.  
6. **Opened** http://localhost:3000 in the browser to use the app.

---

## If something goes wrong

### “Pods stay Pending”

- Make sure you ran **Step 4 and Step 5** (save and load both images with `nerdctl -n k8s.io load`).  
- Make sure you did **not** change the Kubernetes deployment files to use a different `imagePullPolicy`; they should use `Never` for this local setup.  
- Run: `kubectl describe pod -n nexus` and then the name of one of the Pending pods (e.g. `kubectl describe pod -n nexus nexus-web-xxxxx`). Scroll to **Events** at the bottom and read the message (e.g. “image not found”).

### “Port-forward says pod is not running”

- The pods are not ready yet. Go back to **Step 7** and wait until `kubectl get pods -n nexus` shows all pods as **Running** and ready. Then run the port-forward command again.

### “I get an error when I run nerdctl or kubectl”

- **nerdctl:** Make sure Rancher Desktop is open and says Kubernetes is running.  
- **kubectl:** Rancher Desktop installs `kubectl` for you. If the command is not found, try opening a **new** terminal after Rancher Desktop has started, or check Rancher Desktop’s documentation for how to add its tools to your PATH.

### “The page at localhost:3000 doesn’t load”

- Make sure the **port-forward** command from Step 8 is still running in a terminal.  
- Try refreshing the page or opening http://127.0.0.1:3000 instead.

---

## Example: what you’ll see when it works

Below is typical output for each step so you can confirm things are working.

**Step 2 (build backend)** – last lines:

```
=> exporting to docker image format
=> => sending tarball
Loaded image: docker.io/library/nexus-api:latest
```

**Step 3 (build frontend)** – last lines:

```
=> exporting to docker image format
Loaded image: docker.io/library/nexus-web:latest
```

**Step 4 (load backend)** – after the second command:

```
unpacking docker.io/library/nexus-api:latest ...
Loaded image: nexus-api:latest
```

**Step 5 (load frontend)** – after the second command:

```
unpacking docker.io/library/nexus-web:latest ...
Loaded image: nexus-web:latest
```

**Step 6 (deploy)** – after `kubectl apply -k .`:

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

**Step 7 (pods)** – when ready, `kubectl get pods -n nexus` shows something like:

```
NAME                         READY   STATUS    RESTARTS   AGE
nexus-api-xxxxxxxxxx-xxxxx   2/2     Running   0          1m
nexus-api-xxxxxxxxxx-xxxxx   2/2     Running   0          1m
nexus-web-xxxxxxxxxx-xxxxx   1/1     Running   0          1m
nexus-web-xxxxxxxxxx-xxxxx   1/1     Running   0          1m
```

**Step 8 (port-forward)** – after running the command:

```
Forwarding from 127.0.0.1:3000 -> 80
Forwarding from [::1]:3000 -> 80
```

(No prompt appears; leave this running and open the browser.)

---

## Documented run: steps that were executed

The following is a **record of an actual run** with Rancher Desktop running on Windows. Use it as a reference for the exact commands and the output you can expect.

**Environment:** Windows PowerShell, project folder `c:\Users\emana\Documents\code\nexus`, Rancher Desktop with Kubernetes running.

### 1. Build backend image

```powershell
cd c:\Users\emana\Documents\code\nexus
nerdctl build -t nexus-api:latest -f backend/Nexus.Api/Dockerfile backend
```

**Result:** Exit code 0. Final lines of output:

```
#19 exporting to docker image format
#19 exporting layers done
#19 sending tarball 5.5s done
unpacking docker.io/library/nexus-api:latest (sha256:2d54fbc...)
Loaded image: docker.io/library/nexus-api:latest
```

*(First-time build took ~75 seconds; later builds use cache and are faster.)*

### 2. Build frontend image

```powershell
nerdctl build -t nexus-web:latest frontend
```

**Result:** Exit code 0. Final lines:

```
#17 exporting to docker image format
#17 sending tarball 2.0s done
unpacking docker.io/library/nexus-web:latest (sha256:39ac98...)
Loaded image: docker.io/library/nexus-web:latest
```

*(First-time build can take ~2 minutes due to npm and build context.)*

### 3. Load images into Kubernetes

**Important:** Run these from the **project folder** so `api.tar` and `web.tar` are found. In PowerShell you can use a single line with a semicolon:

```powershell
cd c:\Users\emana\Documents\code\nexus
nerdctl save nexus-api:latest -o api.tar
nerdctl -n k8s.io load -i api.tar
```

**Result:** After the load command: `Loaded image: nexus-api:latest`

```powershell
nerdctl save nexus-web:latest -o web.tar
nerdctl -n k8s.io load -i web.tar
```

**Result:** After the load command: `Loaded image: nexus-web:latest`

*(If you see "no such file or directory" for api.tar or web.tar, make sure you ran the commands from the project folder, or use the full path to the .tar file.)*

### 4. Deploy to Kubernetes

```powershell
cd c:\Users\emana\Documents\code\nexus\kubernetes
kubectl apply -k .
```

**Result:** Exit code 0. Output:

```
namespace/nexus unchanged
configmap/nexus-config unchanged
service/nexus-api unchanged
service/nexus-web unchanged
deployment.apps/nexus-api unchanged
deployment.apps/nexus-web unchanged
poddisruptionbudget.policy/nexus-api-pdb configured
poddisruptionbudget.policy/nexus-web-pdb configured
ingress.networking.k8s.io/nexus created
```

*(“unchanged” means the resources were already there from a previous run; “configured” or “created” means they were updated or created.)*

### 5. Verify pods are running

```powershell
kubectl get pods -n nexus
```

**Result:** All pods Running and ready (1/1 or 2/2):

```
NAME                         READY   STATUS    RESTARTS        AGE
nexus-api-74d74d8576-gk58c   1/1     Running   2 (8m25s ago)   38h
nexus-api-74d74d8576-jff8q   1/1     Running   2 (8m25s ago)   38h
nexus-web-6c575f5b58-9qxv7   1/1     Running   4 (7m27s ago)   38h
nexus-web-6c575f5b58-gm966   1/1     Running   4 (7m25s ago)   38h
```

### 6. Port-forward and open the app

```powershell
kubectl port-forward -n nexus svc/nexus-web 3000:80
```

**Result:** Command runs and shows “Forwarding from 127.0.0.1:3000 -> 80”. Leave it running.

**Then:** Open a browser and go to **http://localhost:3000**. You should see the Nexus Task Tracker (task list, search, New task, etc.). To stop the tunnel, press **Ctrl+C** in the terminal where port-forward is running.

---

## Running the app again later

If you’ve already built and loaded the images and deployed once, you usually don’t need to repeat Steps 2–6 unless you changed the app code or deleted the Kubernetes resources.

To start using the app again:

1. Open a terminal in the **project folder**.  
2. Run: `kubectl get pods -n nexus` and check that the pods are **Running**. If not, run: `cd kubernetes` then `kubectl apply -k .`  
3. Run: `kubectl port-forward -n nexus svc/nexus-web 3000:80`  
4. Open **http://localhost:3000** in your browser.

When you’re done, press **Ctrl+C** in the terminal to stop the port-forward.
