---
title: Coding Conventions
created: 2026-05-15
focus: quality
---

# Coding Conventions

## Code Style

**Language:** C# 12 (.NET 8) with nullable reference types enabled and implicit usings.

**Formatting:** Default Visual Studio formatting. No `.editorconfig`, no StyleCop, no `.ruleset`, no `Directory.Build.props` — no explicit formatting or analysis rules are configured or enforced.

**Linting/Analysis:** Not configured. Code analysis is limited to the built-in .NET SDK warnings. `<NoWarn>1591</NoWarn>` suppresses the "missing XML comment" warning (`PRN232.LAB_1.API.csproj:8`).

**Indentation:** 4 spaces (default C#). `System.Text.Json.Serialization` used for JSON in entities.

## Naming Conventions

| Convention | Usage | Examples |
|------------|-------|---------|
| **PascalCase** | Classes, methods, properties, public fields, records | `StudentController`, `GetAllAsync`, `PagedResult<T>`, `Code` |
| **camelCase** | Private fields (underscore-prefixed), local variables, parameters | `_service`, `_repository`, `_context`, `_dbSet`, `query`, `entity`, `id` |
| **PascalCase + "I" prefix** | Interfaces | `IStudentService`, `IRepository<T>`, `ICourseService` |
| **PascalCase (file match)** | Files named exactly after the class they contain | `StudentController.cs` → `StudentController`, `Program.cs` → `Program` |
| **"Async" suffix** | Async methods (all service/controller methods) | `GetAllAsync`, `AddAsync`, `GetByIdAsync`, `DeleteAsync` |

## File Organization

- **One class per file** — consistent across all three projects. No multi-class files.
- **File-scoped namespaces** — C# 10/12 style consistently used everywhere:
  ```csharp
  namespace PRN232.LAB_1.Services.Services;
  ```
- **Imports at top** — `using` statements precede namespace declaration, sorted alphabetically within groups (System first, then NuGet, then project references).
- **No regions used** — class bodies are un-sectioned.

## Patterns in Use

### 1. Repository Pattern
- **Interface:** `IRepository<T>` (`PRN232.LAB_1.Repositories\Repositories\IRepository.cs`) with generic CRUD + `GetQueryable()`
- **Implementation:** `Repository<T>` (`PRN232.LAB_1.Repositories\Repositories\Repository.cs`) using `LmsDbContext`
- **Usage:** Services depend on `IRepository<TEntity>` via constructor injection

### 2. Service Pattern
- **Convention:** One interface + one implementation per domain entity
- **Location:** Interfaces in `PRN232.LAB_1.Services\Interfaces\`, implementations in `PRN232.LAB_1.Services\Services\`
- **Standard methods across all 5 services:**
  - `GetAllAsync()` — returns `List<TResponse>`
  - `GetAllAsync(PagedQuery)` — returns `PagedResult<TResponse>` with search, sort, pagination
  - `GetByIdAsync(int id)` — returns `TResponse?` (null if not found)
  - `AddAsync(TRequest)` — returns `TResponse`
  - `UpdateAsync(int id, TRequest)` — returns `TResponse?` (null if not found)
  - `DeleteAsync(int id)` — returns `bool`
- **Service constructor pattern:**
  ```csharp
  private readonly IRepository<TEntity> _repository;

  public XxxService(IRepository<TEntity> repository)
  {
      _repository = repository;
  }
  ```

### 3. Controller (API) Pattern
- All controllers use `[ApiController]` attribute, inherit from `ControllerBase`
- Route attribute: `[Route("api/{plural}")]`
- **Standard 5 actions per controller:** `GetAll`, `GetById`, `Create`, `Update`, `Delete`
- **No business logic in controllers** — all delegate to service, matching AGENTS.md rule
- **Pagination metadata** set via `HttpContext.Items["Pagination"]`, consumed by `ResponseEnvelopeFilter`
- **Idiomatic body:**
  ```csharp
  [HttpGet("{id:int}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetById(int id)
  {
      var entity = await _service.GetByIdAsync(id);
      if (entity == null)
          return NotFound();
      return Ok(entity);
  }
  ```

### 4. Mapper Pattern (Extension Methods)
- **Static extension method classes** in `PRN232.LAB_1.Services\Mappings\`
- **Standard methods per mapper:**
  - `ToResponseDto(this Entity)` — Entity → Response
  - `ToResponseDto(this Entity, string[] expand)` — Entity → Response with expand support
  - `ToEntity(this Request)` — Request → Entity (for creates)
  - `UpdateEntity(this Request, Entity)` — Request → updates existing Entity (for updates)
  - `ToBusinessModel(this Entity)` — Entity → Business
  - `ToEntity(this Business)` — Business → Entity
  - `ToBusinessModel(this Request)` — Request → Business
  - `ToResponseDtoList(this IEnumerable<Entity>)` — bulk mapping
- **No AutoMapper or third-party mapper** — all mapping is hand-written

### 5. 4-Model Type Pattern
Enforced per AGENTS.md rule:
| Layer | Model Type | Example | Location |
|-------|-----------|---------|----------|
| Repositories | Entity | `Student` | `PRN232.LAB_1.Repositories\Entities\` |
| Services (internal) | Business | `StudentBusiness` | `PRN232.LAB_1.Services\Models\` |
| API Input | Request | `StudentRequest` | `PRN232.LAB_1.Services\Models\` |
| API Output | Response | `StudentResponse` | `PRN232.LAB_1.Services\Models\` |

### 6. Pagination Pattern
- **Input:** `PagedQuery` (`PRN232.LAB_1.Services\Models\PagedQuery.cs`) with `Search`, `SortBy`, `SortDesc`, `Page`, `PageSize`, `Fields`, `Expand`
- **Output:** `PagedResult<T>` (`PRN232.LAB_1.Services\Models\PagedResult.cs`) with `Items`, `Page`, `PageSize`, `TotalItems`, `TotalPages`
- **Default sort fallback:** `q.OrderBy(e => e.Id)` for all entities
- **Page size clamp:** `Math.Clamp(query.PageSize, 1, 100)`

### 7. Expand Pattern
- `?expand=subject,semester` query string support in Course and Enrollment services
- Uses `string[]` split from comma-separated value
- Entity Framework `.Include()` calls before materialization
- Response DTOs have nullable nav properties (`SubjectResponse? Subject`)

### 8. Response Envelope Filter Pattern
- `ResponseEnvelopeFilter` (`PRN232.LAB_1.API\Filters\ResponseEnvelopeFilter.cs`) registered as global filter
- Wraps all `ObjectResult` responses in `ApiResponse<T>`
- Handles 2xx (Ok/Created), 400 (validation errors), 404 (not found)
- Pagination metadata injected into envelope from `HttpContext.Items`
- Uses reflection (`MakeGenericType`) to create typed `ApiResponse<T>` instances

### 9. Entity Configuration Pattern
- Separate `IEntityTypeConfiguration<T>` classes per entity in `PRN232.LAB_1.Repositories\Data\Configurations\`
- Applied via `modelBuilder.ApplyConfigurationsFromAssembly()` in `LmsDbContext`
- All use: `ToTable()`, `HasKey()`, `ValueGeneratedOnAdd()`, `HasMaxLength()`, `IsRequired()`
- Foreign keys use `OnDelete(DeleteBehavior.Restrict)`

### 10. Dependency Injection Registration
- Centralized `DependencyInjection.AddApplicationServices()` extension method in `PRN232.LAB_1.Services\DependencyInjection.cs`
- Registers generic `IRepository<>` and all 5 service interfaces as Scoped

## Error Handling

**Approach:** Minimal error handling with null-propagating pattern.

- **Controllers:** Null check → early return `NotFound()`:
  ```csharp
  var entity = await _service.GetByIdAsync(id);
  if (entity == null) return NotFound();
  return Ok(entity);
  ```
- **Services:** Return `null` for "not found" cases. No custom exception types used.
- **Repositories:** No try/catch. Exceptions (e.g., DbUpdateConcurrencyException, SQL errors) propagate to ASP.NET Core middleware unhandled.
- **Global Filter:** `ResponseEnvelopeFilter` catches model validation errors (400) and maps them to structured `ApiResponse<T>.Fail()` with per-field error arrays.
- **Transaction handling:** Only in `DataSeeder.SeedAsync()` (`PRN232.LAB_1.Repositories\Data\DataSeeder.cs:12`) — uses manual transaction with catch/rollback.
- **Database retry:** In `Program.cs:49-64` — exponential backoff retry (5 attempts, 2s base) for migration.

**Missing:**
- No global exception middleware (no `UseExceptionHandler`, no custom `IExceptionHandler`)
- No `try/catch` in services or controllers for business errors
- No `FluentValidation` — relies solely on `DataAnnotations` on Request models
- No structured error logging (no `ILogger<T>` anywhere)

## Logging

**Framework:** `Console.WriteLine` only — in `Program.cs:60` for migration retry messages.

**Structured logging:** Not configured. No Serilog, no `ILogger<T>` in any service, repository, or controller.

## Comments

**XML Doc Comments:**
- Controllers: Summary + `<param>` + `<response>` tags on every action method and on the controller class
- `PagedResult<T>` and `CourseResponse`: Have `<summary>` and `<typeparam>` XML comments
- Mappers, Services, Repositories, Entities, Request/Response/Business models: **No XML comments** (except CourseResponse and PagedResult)
- Comments suppressed via `<NoWarn>1591</NoWarn>` in API csproj (only XML doc warnings suppressed)

**Inline comments:**
- Sparse, used primarily as section markers in longer methods (e.g., `// Search`, `// Sort`, `// Expand — apply Include before ordering`, `// Count`, `// Page`)
- `// ── Section ──` style used in `Program.cs` (e.g., `// ── Database ──`)
- DataSeeder has no inline comments (data is self-documenting)

---

*Convention analysis: 2026-05-15*
