---
title: Directory Structure
created: 2026-05-15
focus: arch
---

# Directory Structure

## Top-Level Layout

```
Lab1/
├── .planning/                        # GSD project management artifacts
│   ├── codebase/                     # Codebase map documents (this file)
│   ├── phases/                       # Phase plans
│   ├── research/                     # Research findings
│   ├── config.json                   # GSD configuration
│   ├── PROJECT.md                    # Project overview
│   ├── REQUIREMENTS.md               # Full requirement traceability
│   ├── ROADMAP.md                    # Phase roadmap
│   └── STATE.md                      # Current project state
│
├── PRN232.LAB_1.API/                 # API Layer (ASP.NET Core 8.0 Web App)
│   ├── Attributes/                   # Custom attributes (field selection)
│   ├── Controllers/                  # REST API controllers (5 resources)
│   ├── Filters/                      # Action filters (response envelope)
│   ├── Models/                       # API-specific models (ApiResponse)
│   ├── Properties/                   # launchSettings.json
│   ├── appsettings.json              # Connection strings, logging config
│   ├── appsettings.Development.json  # Dev overrides
│   ├── Dockerfile                    # Multi-stage Docker build
│   ├── Program.cs                    # App entry point + DI + pipeline
│   └── PRN232.LAB_1.API.csproj       # Project file (SDK: Web)
│
├── PRN232.LAB_1.Services/            # Services Layer (Class Library)
│   ├── Interfaces/                   # Service interfaces (5 files)
│   ├── Mappings/                     # Entity↔DTO mapper extensions (5 files)
│   ├── Models/                       # 17 model files (4 model types)
│   │   ├── *Business.cs             # Business/domain models (5)
│   │   ├── *Request.cs              # Client input DTOs (5)
│   │   ├── *Response.cs             # API output DTOs (5)
│   │   ├── PagedQuery.cs            # Query parameter model
│   │   └── PagedResult.cs           # Paginated result model
│   ├── Services/                     # Service implementations (5 files)
│   ├── DependencyInjection.cs        # DI registration extension method
│   └── PRN232.LAB_1.Services.csproj  # Project file (SDK: class library)
│
├── PRN232.LAB_1.Repositories/        # Repositories Layer (Class Library)
│   ├── Data/
│   │   ├── Configurations/           # EF Fluent API configs (5 files)
│   │   ├── DataSeeder.cs             # Seed data (~500 enrollments)
│   │   ├── LmsDbContext.cs           # EF Core DbContext
│   │   └── LmsDbContextFactory.cs    # Design-time factory for CLI
│   ├── Entities/                     # EF Core entity models (5 files)
│   ├── Migrations/                   # Code-first migrations (1 initial)
│   ├── Repositories/                 # Generic repository + interface
│   │   ├── IRepository.cs            # Repository interface
│   │   └── Repository.cs             # Repository implementation
│   └── PRN232.LAB_1.Repositories.csproj # Project file (SDK: class library)
│
├── .dockerignore                     # Docker build exclusions
├── .gitignore                        # Git exclusions
├── AGENTS.md                         # GSD agent instructions
├── docker-compose.yml                # SQL Server + API containers
└── Lab1.sln                          # .NET solution file
```

## Directory Purposes

### `PRN232.LAB_1.API/` — API Layer
- **Purpose:** HTTP boundary — receives requests, delegates to services, formats responses. No business logic.
- **Contains:** Controllers, filters, attributes, API models, configuration.
- **Key files:**
  - `Program.cs` — Application bootstrap, DI configuration, middleware pipeline, DB migration/seeding.
  - `Controllers/SemesterController.cs` — Template controller; all 5 controllers follow identical CRUD pattern.
  - `Filters/ResponseEnvelopeFilter.cs` — Auto-wraps all responses in `ApiResponse<T>` envelope.
  - `Models/ApiResponse.cs` — Generic response shape with `Ok`, `Created`, `Fail` factory methods.
  - `appsettings.json` — Contains `ConnectionStrings:DefaultConnection` for SQL Server.

### `PRN232.LAB_1.Services/` — Services Layer
- **Purpose:** Business logic — mapping, validation, search/sort/paging computation, expand/include orchestration.
- **Contains:** Service interfaces and implementations, mapping extensions, model types (Business, Request, Response), DI registration.
- **Key files:**
  - `DependencyInjection.cs` — Single extension method `AddApplicationServices()` registers all services and repositories for DI. Called from `Program.cs`.
  - `Services/SemesterService.cs` — Reference implementation showing the search → sort → count → page → map → return pattern.
  - `Mappings/SemesterMapper.cs` — Reference mapper showing all conversion extension methods.
  - `Models/PagedQuery.cs` — Central query parameter model; all collection endpoints accept this via `[FromQuery]`.
  - `Models/PagedResult.cs` — Generic paginated result model with `Page`, `PageSize`, `TotalItems`, `TotalPages`.
  - `Interfaces/ISemesterService.cs` — Reference interface; all 5 follow identical CRUD signature pattern.

### `PRN232.LAB_1.Repositories/` — Repositories Layer
- **Purpose:** Data access — EF Core context, entity models, migrations, generic CRUD. No business logic.
- **Contains:** Entities, DbContext, EF configurations, migrations, generic repository, data seeder.
- **Key files:**
  - `Repositories/IRepository.cs` — Generic `IRepository<T>` with CRUD + `GetQueryable()` for LINQ composition.
  - `Repositories/Repository.cs` — Concrete `Repository<T>` using `DbSet<T>`. Uses `AsNoTracking()` for reads, calls `SaveChangesAsync()` per write operation.
  - `Data/LmsDbContext.cs` — DbContext with `DbSet<>` for each entity. Loads configurations via assembly scanning.
  - `Data/Configurations/SemesterConfiguration.cs` — Reference EF configuration showing Fluent API mapping pattern.
  - `Data/DataSeeder.cs` — Deterministic seed data generator (random seed: 42).
  - `Migrations/20260514023828_InitialCreate.cs` — Initial migration creating all 5 tables.

## Key File Locations

| Purpose | Path |
|---------|------|
| Solution file | `Lab1.sln` |
| Application entry point | `PRN232.LAB_1.API/Program.cs` |
| DI registration | `PRN232.LAB_1.Services/DependencyInjection.cs` |
| DbContext | `PRN232.LAB_1.Repositories/Data/LmsDbContext.cs` |
| Generic repository interface | `PRN232.LAB_1.Repositories/Repositories/IRepository.cs` |
| Generic repository impl | `PRN232.LAB_1.Repositories/Repositories/Repository.cs` |
| Response envelope implementation | `PRN232.LAB_1.API/Filters/ResponseEnvelopeFilter.cs` |
| API response model | `PRN232.LAB_1.API/Models/ApiResponse.cs` |
| Paging query model | `PRN232.LAB_1.Services/Models/PagedQuery.cs` |
| Paging result model | `PRN232.LAB_1.Services/Models/PagedResult.cs` |
| Docker compose | `docker-compose.yml` |
| API Dockerfile | `PRN232.LAB_1.API/Dockerfile` |
| Seed data | `PRN232.LAB_1.Repositories/Data/DataSeeder.cs` |
| Connection string config | `PRN232.LAB_1.API/appsettings.json` |
| Migration (initial) | `PRN232.LAB_1.Repositories/Migrations/20260514023828_InitialCreate.cs` |

## Naming Conventions

### Files
- **C# source files:** PascalCase per class name (e.g., `SemesterController.cs`, `CourseConfiguration.cs`, `PagedQuery.cs`).
- **Configuration files:** camelCase (e.g., `appsettings.json`, `docker-compose.yml`).
- **Dockerfile:** No extension (capitalized `Dockerfile`).

### Directories
- **PascalCase** for all project directories and source subdirectories: `PRN232.LAB_1.API/`, `Controllers/`, `Repositories/`, `Mappings/`.
- **Special files root:** `.planning/`, `.opencode/` use dot-prefix convention.

### Projects (`.csproj`)
- **Pattern:** `PRN232.<Layer>.{API|Services|Repositories}` — e.g., `PRN232.LAB_1.API`, `PRN232.LAB_1.Services`, `PRN232.LAB_1.Repositories`. Maps directly to the 3-tier layer name.

### Namespaces
- **Pattern:** Mirror directory structure — e.g., `namespace PRN232.LAB_1.API.Controllers`, `namespace PRN232.LAB_1.Repositories.Data.Configurations`.
- File-scoped namespaces used throughout (`namespace X.Y.Z;` syntax).

### Classes
- **Controllers:** `<Resource>Controller` — `SemesterController`, `CourseController`, `StudentController`, `SubjectController`, `EnrollmentController`.
- **Services:** `I<Resource>Service` / `<Resource>Service` — `ISemesterService` / `SemesterService`.
- **Repositories:** `IRepository<T>` / `Repository<T>` (single generic pair, not per-entity).
- **Entities:** Entity name only — `Semester`, `Course`, `Student`, `Subject`, `Enrollment`.
- **Mappers:** `<Resource>Mapper` — `SemesterMapper`, `CourseMapper`.
- **Models:** `<Resource><Type>` — `SemesterBusiness`, `SemesterRequest`, `SemesterResponse`.
- **Configurations:** `<Resource>Configuration` — `SemesterConfiguration`, `CourseConfiguration`.

### Methods
- **Public API methods:** PascalCase with Async suffix — `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
- **Controller actions:** `GetAll`, `GetById`, `Create`, `Update`, `Delete`.
- **Mapper extension methods:** `ToResponseDto`, `ToEntity`, `UpdateEntity`, `ToBusinessModel`, `ToResponseDtoList`.

## Where to Add New Code

### New Feature (new resource, e.g., "Department")
1. **Entity:** `PRN232.LAB_1.Repositories/Entities/Department.cs`
2. **EF Configuration:** `PRN232.LAB_1.Repositories/Data/Configurations/DepartmentConfiguration.cs`
3. **DbContext:** Add `DbSet<Department> Departments` to `LmsDbContext.cs`
4. **Migration:** `dotnet ef migrations add AddDepartment` (from Repositories project)
5. **Business model:** `PRN232.LAB_1.Services/Models/DepartmentBusiness.cs`
6. **Request model:** `PRN232.LAB_1.Services/Models/DepartmentRequest.cs`
7. **Response model:** `PRN232.LAB_1.Services/Models/DepartmentResponse.cs`
8. **Mapper:** `PRN232.LAB_1.Services/Mappings/DepartmentMapper.cs`
9. **Service interface:** `PRN232.LAB_1.Services/Interfaces/IDepartmentService.cs`
10. **Service implementation:** `PRN232.LAB_1.Services/Services/DepartmentService.cs`
11. **Controller:** `PRN232.LAB_1.API/Controllers/DepartmentController.cs`
12. **DI registration:** Add `services.AddScoped<IDepartmentService, DepartmentService>()` in `DependencyInjection.cs`
13. **Seed data:** Add department seed data in `DataSeeder.cs`

### New Endpoint on Existing Resource
- **Controller action:** Add to existing controller in `PRN232.LAB_1.API/Controllers/`.
- **Service method:** Add to service interface and implementation in `PRN232.LAB_1.Services/`.
- **No repository changes needed** unless the query requires new data access patterns (then use `GetQueryable()` or extend repository).

### New Utility / Shared Code
- **Services-layer helpers:** `PRN232.LAB_1.Services/` (e.g., new model or helper class).
- **API-layer helpers:** `PRN232.LAB_1.API/` (e.g., new filter or attribute).

---

*Structure analysis: 2026-05-15*
