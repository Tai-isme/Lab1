---
title: Architecture
created: 2026-05-15
focus: arch
---

# Architecture

## Architectural Pattern
- **Strict 3-layer (Layered Architecture)** — The codebase enforces a dependency chain `API → Services → Repositories` where each layer only depends on the layer directly below it. The API layer contains only controllers and HTTP concerns; the Services layer contains all business logic; the Repositories layer handles data access via Entity Framework Core. No cross-layer shortcuts are permitted.

## Layer Diagram

```
┌───────────────────────────────────────────────────────────────┐
│                    API Layer (Web Project)                      │
│    Controllers · Filters · Attributes · Request/Response DTOs  │
│     `PRN232.LAB_1.API/`                                       │
│     Depends on → `PRN232.LAB_1.Services`                       │
├───────────────────────────────────────────────────────────────┤
│                   Services Layer (Class Library)                 │
│    Service Interfaces · Service Implementations · Mappers       │
│    Business Models · Request/Response Models · PagedQuery       │
│     `PRN232.LAB_1.Services/`                                   │
│     Depends on → `PRN232.LAB_1.Repositories`                   │
│     Used by    → API Layer (project ref)                       │
├───────────────────────────────────────────────────────────────┤
│                 Repositories Layer (Class Library)               │
│    Entity Models · DbContext · EF Configurations · Migrations   │
│    Generic Repository (IRepository<T>) · Data Seeder            │
│     `PRN232.LAB_1.Repositories/`                               │
│     Depends on → nothing (leaf layer)                          │
│     Used by    → Services Layer (project ref)                  │
└───────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │  SQL Server DB   │
                    │ (Docker container│
                    │  or localhost)   │
                    └─────────────────┘
```

## Layer Details

### API Layer (`PRN232.LAB_1.API`)
- **Location:** `PRN232.LAB_1.API/`
- **Project type:** `Microsoft.NET.Sdk.Web` (ASP.NET Core 8.0 Web Application)
- **Responsibility:** HTTP concerns only — routing, request deserialization, response serialization, status codes, Swagger documentation, response envelope wrapping.
- **Contains no business logic** — controllers delegate all work to service layer.
- **Key types:**
  - `Controllers/*Controller` — 5 controllers (Semester, Course, Subject, Student, Enrollment), each with `[ApiController]`, `[Route("api/<plural>")]`, and CRUD actions (GET, GET/{id}, POST, PUT, DELETE).
  - `Models/ApiResponse.cs` — Generic response envelope `{ success, message, data, pagination, errors }` with static factory methods `Ok`, `Created`, `Fail`.
  - `Filters/ResponseEnvelopeFilter.cs` — `IResultFilter` that automatically wraps all action results into `ApiResponse<T>`, extracts pagination metadata from `HttpContext.Items["Pagination"]`, handles 404 and 400 responses.
  - `Attributes/ConditionalJsonPropertyAttribute.cs` — Custom attribute for field-selection support (marks properties that can be conditionally included/excluded).

### Services Layer (`PRN232.LAB_1.Services`)
- **Location:** `PRN232.LAB_1.Services/`
- **Project type:** `Microsoft.NET.Sdk` (class library)
- **Responsibility:** All business logic — entity-to-response mapping, request-to-entity mapping, search/filter/sort/paging logic, expand (eager loading) orchestration.
- **Key types:**
  - `Interfaces/I*Service.cs` — 5 service interfaces (`ISemesterService`, `ICourseService`, `ISubjectService`, `IStudentService`, `IEnrollmentService`), each with methods: `GetAllAsync()`, `GetAllAsync(PagedQuery)`, `GetByIdAsync(id)`, `AddAsync(Request)`, `UpdateAsync(id, Request)`, `DeleteAsync(id)`.
  - `Services/*Service.cs` — 5 concrete implementations using `IRepository<T>` for data access, implementing search/sort/paging/expand logic via `IQueryable<T>` composition.
  - `Mappings/*Mapper.cs` — 5 static extension-method mappers, each with: `ToResponseDto(entity)`, `ToEntity(request)`, `UpdateEntity(request, entity)`, `ToBusinessModel(entity)`, `ToBusinessModel(request)`, `ToResponseDto(entity, string[] expand)`, `ToResponseDtoList(entities)`.
  - `Models/` — 17 model files across 4 model types:
    - **Business** (`*Business.cs`): Internal domain models used for processing (5 files)
    - **Request** (`*Request.cs`): Client input DTOs with `[Required]`, `[StringLength]` data annotations (5 files)
    - **Response** (`*Response.cs`): API output DTOs (5 files)
    - **Shared** (`PagedQuery.cs`, `PagedResult.cs`): Reusable paging/shared models
  - `DependencyInjection.cs` — Extension method `AddApplicationServices()` that registers `IRepository<>` and all 5 service interfaces with `AddScoped`.

### Repositories Layer (`PRN232.LAB_1.Repositories`)
- **Location:** `PRN232.LAB_1.Repositories/`
- **Project type:** `Microsoft.NET.Sdk` (class library)
- **Responsibility:** Data access only — EF Core DbContext, entity definitions, configurations, migrations, generic CRUD operations. Contains no business logic.
- **Key types:**
  - `Entities/*.cs` — 5 entity classes: `Semester`, `Course`, `Subject`, `Student`, `Enrollment` with Id, scalar properties, and navigation properties. Navigation properties marked `[JsonIgnore]` to prevent serialization cycles.
  - `Data/LmsDbContext.cs` — `DbContext` with `DbSet<T>` for each entity, loads configurations via `ApplyConfigurationsFromAssembly`.
  - `Data/LmsDbContextFactory.cs` — `IDesignTimeDbContextFactory<LmsDbContext>` for EF Core CLI tooling.
  - `Data/Configurations/*Configuration.cs` — 5 `IEntityTypeConfiguration<T>` classes using Fluent API: table mapping, keys, column constraints (`HasMaxLength`, `IsRequired`), foreign keys with `DeleteBehavior.Restrict`.
  - `Data/DataSeeder.cs` — Static seed method with deterministic random seed (42), creates 5 semesters, 10 subjects, 50 students, 20 courses, ~500 enrollments. Runs inside a transaction with rollback on failure.
  - `Repositories/IRepository.cs` — Generic interface: `GetAllAsync()`, `GetByIdAsync(int)`, `AddAsync(T)`, `UpdateAsync(T)`, `DeleteAsync(T)`, `GetQueryable()`.
  - `Repositories/Repository.cs` — Generic implementation using `DbSet<T>`, uses `AsNoTracking()` for read operations, calls `SaveChangesAsync()` on writes.
  - `Migrations/` — EF Core code-first migrations (1 initial migration already created).

## Data Flow

### Primary Request Path (GET collection with search/sort/paging)

```
HTTP GET /api/semesters?search=2025&sortBy=code&page=1&size=10
  │
  ▼
[1] Program.cs routes to SemesterController.GetAll(PagedQuery)
  │  via ASP.NET Core routing
  │
  ▼
[2] SemesterController.GetAll()
  │  - No business logic
  │  - Delegates to _service.GetAllAsync(query)
  │  - Sets HttpContext.Items["Pagination"] from result metadata
  │  - Returns Ok(result.Items)
  │
  ▼
[3] SemesterService.GetAllAsync(PagedQuery)
  │  - Gets IQueryable<Semester> from _repository.GetQueryable()
  │  - Applies search filter (WHERE Code/Name LIKE '%search%')
  │  - Applies sort (ORDER BY code ASC/DESC)
  │  - Counts total items
  │  - Applies paging (OFFSET/FETCH via Skip/Take)
  │  - Maps entities → List<SemesterResponse> via mapper
  │  - Returns PagedResult<SemesterResponse>
  │
  ▼
[4] Repository<Semester>.GetQueryable()
  │  - Returns _dbSet.AsNoTracking().AsQueryable()
  │  - No business logic
  │
  ▼
[5] ResponseEnvelopeFilter.OnResultExecuting()
  │  - Intercepts the ObjectResult
  │  - Wraps data in ApiResponse<T> with pagination metadata
  │  - Returns JSON: { success, message, data: [...], pagination: {...} }
  │
  ▼
  HTTP 200 OK
  { success: true, message: "Success",
    data: [...], pagination: { page, pageSize, totalItems, totalPages } }
```

### Secondary Flow (GET by ID with expand)

```
HTTP GET /api/courses/5?expand=subject,semester
  │
  ▼
CourseController.GetById(5) → CourseService.GetByIdAsync(5)
  │
  ▼
Service calls _repository.GetByIdAsync(5):
  - Uses _dbSet.FindAsync(5) — no eager loading by default
  - Returns entity with null navigation properties
  │
  ▼ (expand is applied only in the paged path, not in GetById)
  Service maps entity → CourseResponse
  (GetByIdAsync currently lacks expand support — only the
   GetAllAsync(PagedQuery) overload handles expand/Include)
  │
  ▼
ResponseEnvelopeFilter wraps in ApiResponse
```

### State Management
- **Stateless architecture** — No in-memory state, session, or cache between requests.
- **Unit of Work** — EF Core `DbContext` manages change tracking per request (scoped lifetime).
- **Pagination metadata** — Passed through `HttpContext.Items` (per-request dictionary) from controller to the response envelope filter.

## Key Abstractions

- **`IRepository<T>` / `Repository<T>`** — Generic repository pattern (`PRN232.LAB_1.Repositories/Repositories/IRepository.cs`). The only abstraction for data access. Provides `GetQueryable()` for LINQ composition, enabling the service layer to build queries without leaking EF Core to the API layer. Id-based primary key assumed.

- **`I*Service` / `*Service`** — Per-entity service interfaces (`PRN232.LAB_1.Services/Interfaces/`). Each service owns the business logic for one resource type. Decouples controllers from data access and allows the controller to call methods like `AddAsync(Request)` without knowing how mapping or persistence works.

- **`PagedQuery` / `PagedResult<T>`** — Standardized query/result pair (`PRN232.LAB_1.Services/Models/`). `PagedQuery` aggregates search, sort, paging, field selection, and expand parameters. `PagedResult<T>` provides a consistent paginated response shape across all endpoints.

- **`ApiResponse<T>`** — Response envelope (`PRN232.LAB_1.API/Models/ApiResponse.cs`). All API responses are wrapped in `{ success, message, data, pagination, errors }` via the `ResponseEnvelopeFilter`. Eliminates manual wrapping in controller actions.

- **Static mapper extension methods** — `*Mapper.cs` classes (`PRN232.LAB_1.Services/Mappings/`). Extension methods on entities/requests for type conversion. Each resource has `ToResponseDto(entity)`, `ToEntity(request)`, `UpdateEntity(request, entity)`, `ToBusinessModel(entity)`, `ToBusinessModel(request)`, and a parameterized `ToResponseDto(entity, string[] expand)` overload.

- **Four-model taxonomy** — The codebase uses 4 distinct model types per resource:
  - **Entity** (`PRN232.LAB_1.Repositories/Entities/`): EF Core data models, never exposed to API.
  - **Business** (`PRN232.LAB_1.Services/Models/*Business.cs`): Domain models for internal processing.
  - **Request** (`PRN232.LAB_1.Services/Models/*Request.cs`): Client input with validation attributes.
  - **Response** (`PRN232.LAB_1.Services/Models/*Response.cs`): API output DTOs, never return entities.

## Entry Points

- **`Program.cs`** (`PRN232.LAB_1.API/Program.cs`) — Application entry point. Configures DI (DbContext, application services, controllers), Swagger/OpenAPI, and the middleware pipeline. In development/Docker mode: auto-migrates the database with retry logic (5 attempts, exponential backoff), then runs seed data via `DataSeeder.SeedAsync(db)`. Skips HTTPS redirection in Docker mode.

- **Controllers (`PRN232.LAB_1.API/Controllers/`)** — 5 API entry points per resource:
  - `SemesterController` → `api/semesters`
  - `CourseController` → `api/courses`
  - `SubjectController` → `api/subjects`
  - `StudentController` → `api/students`
  - `EnrollmentController` → `api/enrollments`

- **`DependencyInjection.cs`** (`PRN232.LAB_1.Services/DependencyInjection.cs`) — Registration entry point for all application services. Called by `Program.cs` via `builder.Services.AddApplicationServices()`.

- **`LmsDbContextFactory`** (`PRN232.LAB_1.Repositories/Data/LmsDbContextFactory.cs`) — Design-time factory for EF Core CLI (`dotnet ef migrations`).

## Architectural Constraints

- **Dependency direction:** API → Services → Repositories (enforced via project references). API references Services; Services references Repositories. Repositories reference nothing.
- **No business logic in controllers:** Controllers only extract parameters, call service methods, and return HTTP results.
- **No business logic in repositories:** Repositories only provide raw data access (CRUD + `IQueryable`). Filtering, sorting, paging, and mapping happen in the service layer.
- **Entity models never returned directly:** All data passed to the API layer is converted to `*Response` DTOs before leaving the service layer.
- **Request/Response models never used in repositories:** Request and Response types live only in the Services layer and above.
- **Single generic repository:** There is no per-entity repository. `Repository<T>` handles all entities. Per-entity customization would need to extend the generic or use the `GetQueryable()` escape hatch.

---

*Architecture analysis: 2026-05-15*
