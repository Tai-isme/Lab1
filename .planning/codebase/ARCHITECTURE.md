<!-- refreshed: 2026-05-14 -->
# Architecture

**Analysis Date:** 2026-05-14

## System Overview

This codebase is a **multi-project ASP.NET Core 8 solution** divided into three independently-deployable Web API projects. Despite the names suggesting a classic n-tier layered architecture (`API` → `Services` → `Repositories`), **no project-to-project references, interfaces, or dependency injections connect the layers**. Each project is a standalone Web API with identical structure.

```text
┌──────────────────────────────────────────────────────────────────┐
│                     PRN232.LAB_1.API                             │
│                   http://localhost:5004                           │
│  Program.cs → WeatherForecastController → inline WeatherForecast  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                    PRN232.LAB_1.Services                          │
│                   http://localhost:5096                           │
│  Program.cs → WeatherForecastController → inline WeatherForecast  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                   PRN232.LAB_1.Repositories                      │
│                   http://localhost:5187                           │
│  Program.cs → WeatherForecastController → inline WeatherForecast  │
└──────────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| `PRN232.LAB_1.API` | Exposes WeatherForecast HTTP endpoint (intended presentation/API layer) | `PRN232.LAB_1.API/` |
| `PRN232.LAB_1.Services` | Exposes WeatherForecast HTTP endpoint (intended business logic layer) | `PRN232.LAB_1.Services/` |
| `PRN232.LAB_1.Repositories` | Exposes WeatherForecast HTTP endpoint (intended data access layer) | `PRN232.LAB_1.Repositories/` |

**All three projects have identical responsibilities — no actual layering is implemented.**

## Pattern Overview

**Overall:** Intended: N-Tier / Layered Architecture. Actual: 3 independent Web API projects.

**Key Characteristics:**
- All three projects are `Microsoft.NET.Sdk.Web` (standalone executables), not class libraries
- No project references exist between any of the three projects (`.csproj` files contain no `<ProjectReference>`)
- No interfaces, abstract base classes, or shared contracts exist
- Each project duplicates the same `WeatherForecast` model and `WeatherForecastController` verbatim
- No database, ORM, repository pattern, or service-layer abstraction is present
- Only dependency: `Swashbuckle.AspNetCore 6.6.2` for Swagger UI

## Layers

**PRN232.LAB_1.API:**
- Purpose: Intended as the presentation/API entry point
- Location: `PRN232.LAB_1.API/`
- Contains: `Program.cs`, `WeatherForecast.cs` (model), `Controllers/WeatherForecastController.cs`
- Depends on: `Swashbuckle.AspNetCore 6.6.2`
- Used by: External HTTP callers
- Port: `5004` (HTTP) / `7242` (HTTPS)

**PRN232.LAB_1.Services:**
- Purpose: Intended as the business logic layer
- Location: `PRN232.LAB_1.Services/`
- Contains: `Program.cs`, `WeatherForecast.cs` (model), `Controllers/WeatherForecastController.cs`
- Depends on: `Swashbuckle.AspNetCore 6.6.2`
- Used by: (None — no API project references it)
- Port: `5096` (HTTP) / `7106` (HTTPS)

**PRN232.LAB_1.Repositories:**
- Purpose: Intended as the data access layer
- Location: `PRN232.LAB_1.Repositories/`
- Contains: `Program.cs`, `WeatherForecast.cs` (model), `Controllers/WeatherForecastController.cs`
- Depends on: `Swashbuckle.AspNetCore 6.6.2`
- Used by: (None — no Services project references it)
- Port: `5187` (HTTP) / `7055` (HTTPS)

## Data Flow

### Primary Request Path (identical in all 3 projects)

1. HTTP GET request arrives at `/{controller}` (e.g. `/weatherforecast`)
2. `Program.cs` middleware pipeline runs: Swagger (dev) → HTTPS redirect → Authorization → MapControllers
3. `WeatherForecastController.Get()` generates 5 random `WeatherForecast` objects inline using `Random.Shared`
4. JSON response is returned with the generated array

**State Management:**
- No persistent state — all data is generated in-memory on each request
- No database, cache, or external storage

### [Currently missing] Intended Data Flow (based on naming convention)

1. API layer receives HTTP request → validates input
2. API layer calls Services layer (via interface) → business logic processing
3. Services layer calls Repositories layer (via interface) → data access
4. Data flows back through the chain

## Key Abstractions

**WeatherForecast (model):**
- Purpose: Data transfer object representing a weather forecast entry
- Files: `PRN232.LAB_1.API/WeatherForecast.cs`, `PRN232.LAB_1.Services/WeatherForecast.cs`, `PRN232.LAB_1.Repositories/WeatherForecast.cs`
- Pattern: Duplicated POCO class in each project (not shared via class library)
- Properties: `Date` (DateOnly), `TemperatureC` (int), `TemperatureF` (int, computed), `Summary` (string?)

**WeatherForecastController:**
- Purpose: API controller handling weather forecast requests
- Files: `PRN232.LAB_1.API/Controllers/WeatherForecastController.cs`, `PRN232.LAB_1.Services/Controllers/WeatherForecastController.cs`, `PRN232.LAB_1.Repositories/Controllers/WeatherForecastController.cs`
- Pattern: Duplicated controller in each project
- Route: `[controller]` (i.e., `/weatherforecast`)
- Endpoints: `GET /weatherforecast` (name: `"GetWeatherForecast"`)

## Entry Points

**Each project is an independent entry point:**
- Location: `PRN232.LAB_1.API/Program.cs`, `PRN232.LAB_1.Services/Program.cs`, `PRN232.LAB_1.Repositories/Program.cs`
- Triggers: `dotnet run` or IIS Express launch
- Responsibilities: Configure services (controllers, Swagger), build pipeline, map controllers, run web host

## Architectural Constraints

- **Standalone executables:** All three projects are `Microsoft.NET.Sdk.Web`, not class libraries. They cannot be referenced as dependencies by other projects without producing circular-execution issues.
- **No inter-project coupling:** Zero `<ProjectReference>` elements across all `.csproj` files. The projects are completely isolated.
- **Duplicate code:** The same model and controller code is copy-pasted across all three projects — no shared/contracts project exists.
- **Thread safety:** `Random.Shared` is thread-safe, but `Summaries` array is a `private static readonly` field — safe for concurrent reads.
- **Global state:** No module-level mutable singletons detected.

## Anti-Patterns

### [1] Aspirational Layering Without Wiring

**What happens:** Three projects named API, Services, and Repositories suggest an n-tier architecture, but there are zero project references, interfaces, or DI registrations connecting them. Each project is a standalone web app with identical code.

**Why it's wrong:** The naming creates a false architectural surface that implies separation of concerns, but the actual implementation provides zero layering. Adding code to these projects without wiring them together would create confusion.

**Do this instead:** Either (a) create a shared contracts library (`PRN232.LAB_1.Shared`) for models/interfaces and add `<ProjectReference>` elements so `API` → `Services` → `Repositories` depend on each other, or (b) rename the projects to reflect that they are independent microservices.

### [2] Code Duplication Across Projects

**What happens:** `WeatherForecast.cs` and `WeatherForecastController.cs` are duplicated identically in all three projects.

**Why it's wrong:** Any change to the model or API surface must be replicated in 3 places. Violates DRY.

**Do this instead:** Extract shared models and interfaces into a class library project (e.g., `PRN232.LAB_1.Shared`) referenced by all three.

## Error Handling

**Strategy:** Default ASP.NET Core error handling. No custom middleware, exception filters, or error responses implemented.

**Patterns:**
- No explicit try/catch blocks in controllers
- No validation attributes on models
- No custom error response shapes

## Cross-Cutting Concerns

**Logging:** Default `ILogger<T>` injected via constructor — uses ASP.NET Core's built-in logging pipeline. Only logs `Information` and `Warning` levels via `appsettings.json`.

**Validation:** Not implemented. No data annotations on models, no `[FromBody]` validation, no FluentValidation.

**Authentication:** Standard ASP.NET Core `UseAuthorization()` middleware call, but no authentication scheme is configured.

---

*Architecture analysis: 2026-05-14*
