# SOLUTION.md – Design and trade-offs

## Architecture (overview)

### System

```mermaid
flowchart LR
  subgraph Client
    Browser[Browser]
  end
  subgraph Frontend
    SPA[React SPA]
  end
  subgraph Backend
    API[ASP.NET API]
    Store[(InMemory)]
  end
  Browser --> SPA
  SPA -->|REST /api/tasks| API
  API --> Store
```

### Backend layers (dependency direction)

```mermaid
flowchart TB
  subgraph Api
    Controllers[Controllers]
    Config[Configuration]
    Middleware[Middleware]
  end
  subgraph Application
    Service[TaskService]
    Contracts[ITaskService, ITaskRepository]
  end
  subgraph Domain
    Entity[TaskItem, Enums]
  end
  subgraph Infrastructure
    Repo[TaskRepository]
    Db[AppDbContext]
  end
  Api --> Application
  Application --> Domain
  Application --> Infrastructure
  Infrastructure --> Domain
```

### Frontend (simplified)

```mermaid
flowchart LR
  subgraph UI
    Pages[Pages]
    Components[Components]
  end
  subgraph State
    Context[TasksContext]
    Hooks[useTask, useCreateTask, ...]
  end
  subgraph Data
    Client[API client]
  end
  Pages --> Components
  Pages --> Context
  Pages --> Hooks
  Context --> Client
  Hooks --> Client
  Client -->|fetch| API[/api/tasks]
```

### Kubernetes (deploy)

```mermaid
flowchart LR
  subgraph nexus
    Web[nexus-web]
    Api[nexus-api]
    Web --> Api
  end
  User[User] --> Ingress[Ingress]
  Ingress --> Web
```

---

## Design decisions

### Backend (C# / ASP.NET Core)

- **Layered structure (separation of concerns)**  
  - **Domain**: `TaskItem`, `TaskStatus`, `TaskPriority` – no dependencies.  
  - **Application**: DTOs, `ITaskRepository`, `ITaskService`, `TaskService` – orchestration and use cases.  
  - **Infrastructure**: EF Core `AppDbContext`, `TaskRepository` – persistence.  
  - **Api**: Controllers, validation, options, global exception middleware – HTTP and configuration only.

- **Options pattern and config sections**  
  - `ApiOptions` and `CorsOptions` are bound from **appsettings.json** sections `Api` and `Cors`.  
  - CORS origins and Swagger toggle are driven by config, not hard-coded.

- **SOLID**  
  - **S** – Controllers handle HTTP; `TaskRequestValidator` only validates; `TaskService` only implements use cases.  
  - **O** – Sort behaviour is a simple parameter (`sortDueDateAsc`); repository can be extended with more strategies without changing callers.  
  - **L** – N/A (no inheritance hierarchy).  
  - **I** – `ITaskRepository` and `ITaskService` are focused interfaces.  
  - **D** – API and Application depend on `ITaskRepository` / `ITaskService`; Infrastructure implements them.

- **Validation and errors**  
  - Request validation is done in a dedicated validator; controllers return **RFC 7807 ProblemDetails** (including `errors` for validation).  
  - A **global exception handler middleware** turns unhandled exceptions into ProblemDetails and enables consistent HTTP logging.

- **EF Core InMemory**  
  - Used as required; seed data is applied on startup so `GET /api/tasks` returns example tasks.  
  - Search uses case-insensitive string checks compatible with InMemory (no SQL-specific `EF.Functions.Like`).

### Frontend (React + TypeScript)

- **Context API**  
  - `TasksProvider` holds list state (tasks, loading, error, search, sort) and exposes `refetch`, `setSearchQuery`, `setSortDueDateAsc`.  
  - List view and create/edit pages use the same context where needed (e.g. refetch after create/update).

- **Custom hooks**  
  - `useTasksContext()` – access tasks context.  
  - `useTask(id)` – load a single task (e.g. for edit).  
  - `useCreateTask`, `useUpdateTask` – submit create/update and handle loading/error.  
  - `useApiError` – map API errors to a single message (used with create/update).

- **Config**  
  - API base URL comes from **environment**: `VITE_API_BASE_URL` (see `frontend/.env.example`). Defaults to `http://localhost:5000` in code if unset.

- **Strong types**  
  - `Task`, `ProblemDetails`, and request/response shapes are typed; `mapApiErrorToMessage` works with the ProblemDetails shape from the API.

- **Routing**  
  - `/` – task list.  
  - `/tasks/new` – create task.  
  - `/tasks/:id/edit` – edit task.  
  - Loading and error states are shown; validation/error messages surface `detail` or validation `errors` from ProblemDetails.

---

## Trade-offs

- **InMemory** – No real DB; data is lost on restart. Acceptable for the assignment; production would use a persistent store.  
- **Search/sort** – Implemented in the API (query params `q` and `sort`). Keeps the SPA simple and avoids loading all data client-side.  
- **Client-side validation** – Mirrored where practical (required title, status/priority enums, optional due date). Full rules stay on the server; 400 + ProblemDetails drive error display.  
- **Single list context** – One global list state. For this scope it’s enough; a larger app might split list vs. detail contexts or use a data library.

---

## Debugging issue: InMemory and `EF.Functions.Like`

**What happened**  
In the repository, task search was first implemented with `EF.Functions.Like(t.Title, "%" + q + "%")` for case-insensitive search. With the **InMemory** provider, this caused a runtime exception because InMemory does not support `EF.Functions.Like` in the same way as SQL providers.

**How it was diagnosed**  
- Running the API and calling `GET /api/tasks?q=test` produced an error.  
- The stack trace pointed to the repository’s LINQ query.  
- Checking EF Core docs and InMemory behaviour confirmed that not all SQL-oriented functions are supported.

**How it was resolved**  
The query was changed to use in-memory–friendly, case-insensitive logic: normalize with `ToLowerInvariant()` and use `string.Contains` (e.g. `t.Title.ToLower().Contains(q)` with `q` lowercased). This works with the InMemory provider and still respects the “case-insensitive search” requirement. For a real SQL database, you could switch back to `EF.Functions.Like` or a provider-specific equivalent and keep the same repository interface.

---

## Commit / branching approach

- **main** – stable, runnable solution.  
- Commits are small and focused: e.g. “Add ApiOptions and CorsOptions”, “Add TasksContext and useTasksContext”, “Add mapApiErrorToMessage and unit test”.  
- No long-lived feature branches for this scope; optional: a single `dev` branch for WIP before merging to `main`.
