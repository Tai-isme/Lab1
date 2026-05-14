# Domain Pitfalls: ASP.NET Core 3-Layer LMS REST API

**Domain:** Learning Management System REST API — ASP.NET Core 8, 3-layer architecture, EF Core, Docker
**Researched:** 2026-05-14
**Overall confidence:** HIGH

---

## Critical Pitfalls

Mistakes that cause rewrites, data corruption, or complete failure to meet the lab spec.

### Pitfall 1: Circular Reference → JSON Serialization Crash

**What goes wrong:** Navigation properties between entities (e.g., `Enrollment.Student` and `Student.Enrollments`) create object cycles. When System.Text.Json serializes the response, it throws:
```
System.Text.Json.JsonException: A possible object cycle was detected.
```

The default `ReferenceHandler.IgnoreCycles` does NOT fix this adequately — it sets cycle-starting properties to `null`, producing unpredictable JSON where some related data appears and some doesn't. The JSON output becomes non-deterministic and confusing to consumers.

**Why it happens:**
- EF Core's relationship fix-up automatically populates both sides of a relationship. Loading `Student` with `.Include(s => s.Enrollments)` gives each `Enrollment` a back-reference to `Student`.
- The lab spec requires 5 entity types (Semester, Course, Subject, Student, Enrollment) with navigation properties between them. Every bidirectional relationship is a potential cycle.
- Since controllers are forbidden from directly returning entity models (per DATA-04: Response models required), the cycle should be broken at the response-DTO boundary — but this is commonly missed.

**Consequences:**
- API returns 500 errors on any endpoint that loads related data.
- `ReferenceHandler.Preserve` adds `$id`/`$values` metadata to JSON, breaking clients that expect clean objects.
- Debugging is confusing because the error points to serialization, not to the actual root cause (cyclic entity graph).

**Prevention:**
1. **Never return entity models from API endpoints.** Always map to Response DTOs that omit reverse navigation properties. This is the only clean fix — it breaks cycles structurally.
2. If you must keep navigation properties on DTOs, add `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` on the reverse navigation:
   ```csharp
   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   public ICollection<EnrollmentResponse>? Enrollments { get; set; }
   ```
3. As a last resort, configure System.Text.Json to ignore cycles (but be aware of the `null`-insertion behavior):
   ```csharp
   builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
       });
   ```

**Detection:**
- Run ANY GET endpoint that loads related data via `.Include()`. If you get a 500 with "A possible object cycle was detected", you have this pitfall.
- Check every Response DTO for bidirectional navigation properties.

**Which phase should address it:** Phase 1–2 (when building entity models and response DTOs). Not addressing it here means a 500-error surprise when API-02 (GET by ID with related data) is tested.

---

### Pitfall 2: N+1 Query Problem with EF Core

**What goes wrong:** Loading a list of entities triggers one query for the list, then N additional queries for related data — one per row. With 500+ enrollments, this means 501+ database round-trips for a single request.

**Why it happens:**
- Lazy loading is disabled by default in EF Core 8, which is good. But eager loading via `.Include()` is used naively without considering query shape.
- Worse: Service-layer methods return entity models, and the controller or mapping layer accesses navigation properties after the `DbContext` is disposed (or after the query has executed without `Include`).
- The classic pattern: `foreach (var enrollment in enrollments) { var studentName = enrollment.Student?.Name; }` — each iteration triggers a separate query if `Student` wasn't eagerly loaded.

**Consequences:**
- A GET /api/enrollments endpoint that should execute 1 SQL query fires 501+ queries.
- Response time goes from ~50ms to several seconds with 500+ enrollment records.
- SQL Server CPU spikes. Docker containers on a dev machine may timeout.

**Prevention:**
1. **Use `.Include()` / `.ThenInclude()` for all eagerly needed navigation data:**
   ```csharp
   var enrollments = await _context.Enrollments
       .Include(e => e.Student)
       .Include(e => e.Course).ThenInclude(c => c.Subject)
       .ToListAsync();
   ```
2. **Use `.AsSplitQuery()` when including multiple collections** to avoid Cartesian explosion:
   ```csharp
   var enrollments = await _context.Enrollments
       .Include(e => e.Student)
       .Include(e => e.Course)
       .AsSplitQuery()
       .ToListAsync();
   ```
3. **Use `.AsNoTracking()` for read-only queries** to eliminate change-tracker overhead:
   ```csharp
   await _context.Enrollments.AsNoTracking().Include(...).ToListAsync();
   ```
4. **Use projections (`Select()`) to DTOs** instead of loading full entities — EF Core translates projections to SQL JOINs, eliminating N+1 entirely:
   ```csharp
   var dtos = await _context.Enrollments
       .Select(e => new EnrollmentResponse {
           Id = e.Id,
           StudentName = e.Student.Name,
           CourseName = e.Course.Name
       })
       .ToListAsync();
   ```
5. **Set `AsSplitQuery()` as default in DbContext configuration** if the project regularly includes multiple collections:
   ```csharp
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.UseSqlServer(connectionString, 
           o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
   }
   ```

**Detection:**
- Enable EF Core query logging in `appsettings.Development.json`:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
  ```
- Run a list endpoint and count the SQL queries. If you see the same SELECT with different `@p__` values repeated N times, you have N+1.
- Use MiniProfiler or the `DbCommandInterceptor` pattern to assert query count per endpoint.

**Which phase should address it:** Phase 4 (when building GET collection endpoints). Must be verified before the 500+ enrollment seed data is loaded.

---

### Pitfall 3: Incorrect Layer Separation (Leaky Architecture)

**What goes wrong:** The 3-layer architecture (API → Services → Repositories) is violated — business logic leaks into controllers, ORM concepts leak into services, or repositories expose EF Core internals.

**Why it happens (specific to this codebase):**
- The existing scaffold has Repositories and Services projects that *are* web API projects (with `Controllers/` folders, `Program.cs`). They need to be converted to class libraries.
- Project references don't exist: API doesn't reference Services, Services doesn't reference Repositories.
- No interfaces (`IRepository`, `IService`) defined — layers couple directly to concrete implementations.

**More subtle manifestations:**
- **Repository returns `IQueryable<T>`:** This leaks EF Core abstractions into the service layer. The caller can append `.Where()`, `.Include()`, `.OrderBy()` — but these are EF Core-specific LINQ providers, not standard .NET. If the ORM is swapped, everything breaks.
- **Repository returns `IEnumerable<T>` but the query hasn't executed yet:** Deferred execution means the database connection may be closed by the time the service iterates.
- **Service layer is anemic transaction script:** Services become method dumps where every method reimplements validation that belongs on the entity model.
- **Entity Framework entities are returned directly from controllers:** The lab spec explicitly requires Response DTOs (DATA-04), but the easiest path is to skip mapping and return entities.

**Consequences:**
- Cannot unit-test business logic without a database.
- Swapping from SQL Server to another provider requires rewriting repository internals.
- Controllers grow fat with business logic, violating separation of concerns.
- The solution structure is misleading (projects named Repositories/Services that are actually web apps).

**Prevention:**
1. **Convert Repositories and Services to class libraries:**
   - Change `<Project Sdk="Microsoft.NET.Sdk.Web">` to `<Project Sdk="Microsoft.NET.Sdk">`
   - Remove `Program.cs`, `appsettings.json`, `Controllers/` folders, `Properties/launchSettings.json`
   - Add project references: API → Services, Services → Repositories

2. **Define interfaces for each layer:**
   ```csharp
   // Repositories layer
   public interface IEnrollmentRepository
   {
       Task<Enrollment?> GetByIdAsync(int id);
       Task<PagedResult<Enrollment>> GetPagedAsync(PagingParams paging);
       Task AddAsync(Enrollment enrollment);
       Task UpdateAsync(Enrollment enrollment);
       Task DeleteAsync(int id);
   }
   
   // Services layer
   public interface IEnrollmentService
   {
       Task<EnrollmentResponse?> GetByIdAsync(int id);
       Task<PagedResponse<EnrollmentResponse>> GetPagedAsync(EnrollmentQueryParams parameters);
       Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request);
   }
   ```

3. **Repository methods return materialized collections (`IReadOnlyList<T>`, `List<T>`, `T?`), NOT `IQueryable<T>`:**
   ```csharp
   // GOOD
   public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId)
   {
       return await _context.Enrollments
           .AsNoTracking()
           .Where(e => e.StudentId == studentId)
           .ToListAsync();
   }
   
   // BAD — leaks IQueryable
   public IQueryable<Enrollment> GetAll()
   {
       return _context.Enrollments;
   }
   ```

4. **Enforce strict dependency direction:** API knows about Services. Services knows about Repositories. Repositories knows about nothing. No circular or sideways references.

5. **Wiring via DI in `Program.cs`:**
   ```csharp
   builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
   builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
   ```

**Detection:**
- Check if any `using` statement in the API layer references EF Core (`Microsoft.EntityFrameworkCore`). It shouldn't.
- Check if any service method calls `ToListAsync()`, `FirstOrDefaultAsync()`, or any EF Core async method — that work belongs in repositories.
- Check if any repository method returns `IQueryable<T>` or `DbSet<T>`.
- Verify project references match the dependency direction: API → Services → Repositories.

**Which phase should address it:** Phase 0 (project structure fix). This is a prerequisite for everything else — building on top of a broken layer structure multiplies technical debt.

---

### Pitfall 4: Docker Networking — API Cannot Connect to SQL Server

**What goes wrong:** The Dockerized API container cannot connect to the SQL Server container, throwing:
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
The server was not found or was not accessible.
```

**Why it happens:**
- The connection string in `appsettings.json` uses `localhost` or `127.0.0.1`: `"Server=localhost;Database=LMS;User Id=sa;Password=..."`
- Inside a Docker container, `localhost` refers to the container itself, NOT the host machine or other containers.
- Even when both containers are on the same Docker network, the connection string must use the **service name** (as defined in `docker-compose.yml`) as the hostname.
- Missing `TrustServerCertificate=True` because SQL Server Docker images use self-signed certificates by default.
- The `sa` password must meet SQL Server password complexity rules (8+ chars, upper/lower/digit/special) — simple passwords fail silently.

**Consequences:**
- `docker-compose up` succeeds but the API returns 500 errors on every endpoint.
- `dotnet ef database update` inside the API container fails.
- Developer spends hours debugging "connection refused" messages.
- The API startup throws `System.Data.SqlClient.SqlException` and exits.

**Prevention:**
1. **Use Docker Compose service name in connection string, not localhost:**
   ```yaml
   # docker-compose.yml
   services:
     lms-db:
       image: mcr.microsoft.com/mssql/server:2022-latest
       container_name: lms-sqlserver
       environment:
         ACCEPT_EULA: "Y"
         MSSQL_SA_PASSWORD: "LMS_P@ssw0rd_2024"
         MSSQL_PID: "Developer"
       ports:
         - "1433:1433"
       healthcheck:
         test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "LMS_P@ssw0rd_2024" -C -Q "SELECT 1"
         interval: 10s
         retries: 10
   
     lms-api:
       build: .
       container_name: lms-api
       ports:
         - "5000:8080"
       depends_on:
         lms-db:
           condition: service_healthy
       environment:
         - ConnectionStrings__DefaultConnection=Server=lms-db;Database=LMS;User Id=sa;Password=LMS_P@ssw0rd_2024;TrustServerCertificate=True;MultipleActiveResultSets=True
   ```

2. **Override connection string via environment variable in Docker Compose** (as shown above) rather than relying on `appsettings.json`. The environment variable syntax `ConnectionStrings__DefaultConnection` uses the double-underscore convention for hierarchy.

3. **Key rules for the connection string inside Docker:**
   - `Server` = Docker Compose service name (e.g., `lms-db`), NOT `localhost` or `127.0.0.1`
   - `TrustServerCertificate=True` is required for SQL Server Docker images
   - `MultipleActiveResultSets=True` allows multiple EF Core queries on a single connection
   - Port is the internal port (`1433`), not the mapped host port

4. **For local development** (API running on host, DB in Docker), use `localhost` + mapped port:
   ```
   Server=localhost,1433;Database=LMS;User Id=sa;Password=LMS_P@ssw0rd_2024;TrustServerCertificate=True;
   ```

5. **Add a health check on the DB service** and use `depends_on: condition: service_healthy` so the API doesn't start before SQL Server is ready.

**Detection:**
- Run `docker-compose up` and check API logs for "Could not open a connection to SQL Server".
- Check the connection string — does it use a Docker service name or `localhost`?
- Does the docker-compose have `depends_on` without a health check condition?
- Test by exec'ing into the API container: `docker exec -it lms-api ping lms-db`

**Which phase should address it:** Phase 5 (Docker setup). Must be solved before the API can be tested end-to-end.

---

### Pitfall 5: Seed Data Generation for 500+ Records

**What goes wrong:** The seed data approach (500+ enrollments, 50+ students, 20+ courses) either:
- Causes EF Core migration snapshots to bloat to MB-sized `.Designer.cs` files.
- Times out because EF Core's `AddRange` + `SaveChanges` is slow for bulk inserts.
- Runs out of memory because the `DbContext` tracks all 500+ entities.
- Fails because `HasData()` requires hardcoded primary keys and is inflexible.

**Why it happens:**
- Using `HasData()` in `OnModelCreating` for 500+ records embeds every INSERT as migration code. The `.Designer.cs` file grows by hundreds of KBs. Regenerating migrations becomes slow.
- Calling `SaveChangesAsync()` once for all 500 records: EF Core tracks every entity in the change tracker, memory grows, and the single transaction may timeout.
- Seed data references depend on foreign keys (enrollments need valid student/course IDs) — generating them in the wrong order causes FK violations.
- Using `DateTime.Now` in seed data is non-deterministic — EF Core may detect "changes" on every migration.

**Consequences:**
- `dotnet ef migrations add InitialSeed` takes minutes and produces a 5+ MB designer file.
- `dotnet ef database update` hangs or times out.
- Memory spikes to 500+ MB on seed execution.
- FK violations when enrollment seed runs before student/course seed.

**Prevention:**
1. **DO NOT use `HasData()` for large seed datasets.** Use `HasData()` only for small reference data (e.g., 2-3 lookup records). For 500+ records, use `UseSeeding()` / `UseAsyncSeeding()` (available in EF Core 8, recommended by Microsoft since EF Core 9).

2. **Use the `UseSeeding` / `UseAsyncSeeding` pattern** in your `DbContext`:
   ```csharp
   public class AppDbContext : DbContext
   {
       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       {
           optionsBuilder.UseSeeding((context, _) =>
           {
               SeedData(context);
           });
           optionsBuilder.UseAsyncSeeding(async (context, _, ct) =>
           {
               await SeedDataAsync(context, ct);
           });
       }
   }
   ```

3. **Batch the inserts** — create, add, save, and dispose in batches of ~100:
   ```csharp
   private async Task SeedDataAsync(AppDbContext context, CancellationToken ct)
   {
       if (await context.Students.AnyAsync(ct)) return; // Idempotent
       
       var students = GenerateStudents(50);
       for (int i = 0; i < students.Count; i += 100)
       {
           var batch = students.Skip(i).Take(100);
           context.Students.AddRange(batch);
           await context.SaveChangesAsync(ct);
       }
   }
   ```

4. **Alternative: Use a standalone seed script/class** that runs via `IHostedService` or `WebApplication.Services` at startup:
   ```csharp
   public static async Task SeedDatabaseAsync(this WebApplication app)
   {
       using var scope = app.Services.CreateScope();
       var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
       await context.Database.MigrateAsync();
       
       if (!await context.Students.AnyAsync())
       {
           // Generate and insert seed data
       }
   }
   ```

5. **Use a library like Bogus** for realistic seed data instead of hardcoded values:
   ```csharp
   var faker = new Faker<Student>()
       .RuleFor(s => s.Name, f => f.Name.FullName())
       .RuleFor(s => s.Email, f => f.Internet.Email())
       .RuleFor(s => s.BirthDate, f => f.Date.Past(20));
   ```

6. **Order matters** — seed in FK dependency order: Semesters → Subjects → Courses → Students → Enrollments.

**Detection:**
- Check if `HasData()` is called in `OnModelCreating` — if so, count the seeded rows. Over 20 rows in `HasData()` is a warning sign.
- Run `dotnet ef migrations list` and check if any migration produces a large `.Designer.cs` file.
- Run the seed and monitor memory usage in Docker Desktop.

**Which phase should address it:** Phase 5 (when DATA-05 seed data requirement is implemented). Critical to solve before demo day.

---

## Moderate Pitfalls

### Pitfall 6: DbContext Lifetime Mismanagement

**What goes wrong:** `DbContext` is registered with the wrong lifetime, causing data corruption, connection exhaustion, or `ObjectDisposedException`.

**Why it happens:**
- **Singleton DbContext:** Multiple requests share one `DbContext` instance. EF Core change tracker is not thread-safe — concurrent requests corrupt tracked entities, throw `InvalidOperationException`.
- **Transient DbContext:** Service A and Service B in the same request get DIFFERENT `DbContext` instances. `SaveChangesAsync()` on Service A's context cannot see changes from Service B's context.
- **DbContext injected into background service** (singleton) without `IServiceScopeFactory`: The scoped `DbContext` outlives its intended HTTP request scope.

**Prevention:**
1. **Always register DbContext as Scoped** (this is the default with `AddDbContext`):
   ```csharp
   builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(connectionString));
   // Scoped is the default — one context per HTTP request
   ```
2. Never inject `AppDbContext` directly into a singleton (e.g., middleware constructor, background services). Use `IServiceScopeFactory` or `IDbContextFactory` instead.
3. For seeding at startup, create an explicit scope:
   ```csharp
   using var scope = app.Services.CreateScope();
   var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
   ```

**Detection:**
- Look for `services.AddDbContext` calls. Verify the lifetime parameter (if omitted, defaults to `Scoped` — which is correct).
- Check any `IHostedService` or middleware class: does it inject `DbContext` in its constructor? That's wrong.
- Check for `using var db = new AppDbContext(...)` inside controllers — that creates unmanaged instances.

**Which phase should address it:** Phase 1 (DbContext setup in DI). Easy to fix early, hard to untangle later.

---

### Pitfall 7: Inconsistent Response Envelope Format

**What goes wrong:** The lab spec requires a consistent response format (success, message, data, errors) but different endpoints return different shapes — some return raw data, some wrap it, some use different property names.

**Why it happens:**
- No shared response DTO or envelope class is defined upfront.
- Each controller action independently decides what to return.
- Some endpoints return `Ok(data)`, others return `Ok(new { Success = true, Data = data })`, others return just `data` directly.
- Error responses vary: `BadRequest("message")` vs `BadRequest(new { error = "message" })`.

**Consequences:**
- Client code cannot write a generic response parser.
- The lab evaluation checklist item "consistent response format" is not met.
- Frontend developers have to special-case each endpoint.

**Prevention:**
1. **Define a shared response envelope** early in the project:
   ```csharp
   public class ApiResponse<T>
   {
       public bool Success { get; set; }
       public string? Message { get; set; }
       public T? Data { get; set; }
       public List<string>? Errors { get; set; }
       
       public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
       public static ApiResponse<T> Fail(List<string> errors, string? message = null) 
           => new() { Success = false, Errors = errors, Message = message };
   }
   ```
2. **Create a paginated response variant** for collection endpoints:
   ```csharp
   public class PagedResponse<T> : ApiResponse<List<T>>
   {
       public int Page { get; set; }
       public int PageSize { get; set; }
       public int TotalCount { get; set; }
       public int TotalPages { get; set; }
   }
   ```
3. **Use an `ActionFilter` or base controller class** to standardize the wrapping, so individual actions don't have to remember it.

**Detection:**
- GET `/api/enrollments` — what shape does it return?
- GET `/api/enrollments/1` — same shape?
- POST `/api/enrollments` with bad data — same shape?
- If three endpoints return three different JSON shapes, you have this pitfall.

**Which phase should address it:** Phase 0 (before any controller actions are written). Retrofitting response consistency across 20+ endpoints is painful.

---

### Pitfall 8: Generic Repository Over-Abstraction

**What goes wrong:** Creating a single `IRepository<T>` with methods like `GetAll()`, `GetById()`, `Add()`, `Update()`, `Delete()` that all entities share.

**Why it happens:**
- It looks clean and DRY on the surface.
- Tutorials often demonstrate generic repositories.
- The developer doesn't realize that business operations rarely map to simple CRUD.

**Consequences:**
- The generic repository returns `IQueryable<T>` (or else needs parameters for every possible filter/Include — an explosion of method overloads).
- Adding `Include()` chains to a generic method is impossible without either: (a) passing expression trees, or (b) accepting string-based navigation paths.
- The abstraction leaks ORM details (expressions, `IQueryable`, navigation property strings).
- Each entity has different query needs — a single generic interface cannot express them cleanly.

**Prevention:**
1. **Use entity-specific repository interfaces** that express real data access needs:
   ```csharp
   public interface IEnrollmentRepository
   {
       Task<PagedResult<Enrollment>> GetPagedWithDetailsAsync(
           int? studentId, int? courseId, string? sortBy, 
           bool descending, int page, int pageSize);
       Task<Enrollment?> GetWithDetailsAsync(int id);
       // No generic Add<T>/Update<T> — those are DbContext responsibilities
   }
   ```
2. The pattern above is acceptable **given the lab constraint** that requires a "Repository" layer. For production scenarios, many architects argue the repository pattern is redundant over EF Core's `DbSet` directly.
3. Keep repository methods focused on retrieval queries. Let `DbContext.SaveChangesAsync()` handle persistence.

**Detection:**
- Look for a single `IRepository<T>` interface in the Repositories project — if it exists, evaluate whether it's adding value or just wrapping `DbSet`.
- Check if repository methods return `IQueryable<T>` — that's the strongest signal of leaky abstraction.

**Which phase should address it:** Phase 2 (when creating repository interfaces). Harder to refactor once services depend on a generic interface.

---

## Minor Pitfalls

### Pitfall 9: Not Handling 404 vs Empty Collection

**What goes wrong:** `GET /api/enrollments/9999` (non-existent) returns 200 with `null` data instead of 404. Or `GET /api/enrollments?studentId=9999` (no matches) returns 404 instead of 200 with empty array.

**Why it happens:**
- The lab spec says "404 if not found" for GET by ID, but developers conflate "resource not found" (404) with "query returned no results" (200 with empty list).
- A null check returns `NotFound()` in the wrong places.

**Prevention:**
- GET by ID: if entity is null → 404 NotFound
- GET collection: always return 200 Ok with the list (may be empty)
- POST: 201 Created with the new resource
- PUT/PATCH with invalid ID: 404 NotFound
- DELETE with invalid ID: 404 NotFound

**Which phase should address it:** Phase 4 (building controller actions).

---

### Pitfall 10: Over-fetching with No Field Selection Support

**What goes wrong:** When the client requests field selection (a lab requirement), the API always returns every column, including potentially large text fields.

**Why it happens:**
- The lab spec requires field selection (`?fields=id,name`), but implementing dynamic field selection in EF Core requires `Select()` with a dynamic expression — which is non-trivial.
- The easy path is to return the full object and ignore the `fields` parameter.

**Consequences:**
- The lab evaluation checklist item "field selection" is not met.
- With 500+ enrollments, sending all fields over the wire increases response size.

**Prevention:**
- Implement field selection via a helper that builds a dynamic `Select` expression, or use a library like `GraphQL.NET` (out of scope for this project).
- Simpler approach: define a response DTO per endpoint that includes only the fields most commonly needed, and document that `fields` parameter selects from those.

**Which phase should address it:** Phase 4 (GET collection endpoint). Flag as needing deeper research on dynamic `Select()` implementation.

---

### Pitfall 11: Pagination Without Metadata

**What goes wrong:** The API paginates results but returns only the items, omitting `totalCount`, `page`, `pageSize`, and `totalPages` — making it impossible for the client to build pagination UI.

**Why it happens:**
- The developer implements `.Skip()/.Take()` on the query but doesn't also execute a `COUNT(*)`.
- The response body contains just the array, with no envelope to hold pagination metadata.

**Consequences:**
- Client cannot display "Page 3 of 25" or "Showing 41-60 of 500".
- The lab spec requirement for "pagination metadata in responses" is not met.

**Prevention:**
- Always execute `COUNT(*)` alongside `.Skip()/.Take()`:
  ```csharp
  var totalCount = await query.CountAsync();
  var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
  ```
- Return a `PagedResponse<T>` that wraps items + metadata.

**Which phase should address it:** Phase 4 (GET collection endpoint).

---

### Pitfall 12: Docker Image Too Large / Slow Builds

**What goes wrong:** The Docker image for the API is based on `mcr.microsoft.com/dotnet/sdk:8.0` (the SDK image, ~700MB) instead of the runtime image, or doesn't use multi-stage builds.

**Why it happens:**
- Using the default Dockerfile from the ASP.NET Core template without optimization.
- The SDK image includes compilers, NuGet, and tools not needed at runtime.

**Consequences:**
- Docker image is >700MB instead of ~200MB for the runtime image.
- `docker-compose up` takes 5+ minutes for the initial build.
- CI/CD pipelines are slow.

**Prevention:**
- **Always use multi-stage builds** — SDK stage for build/publish, runtime stage for execution:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish -c Release -o /app/publish
  
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
  WORKDIR /app
  COPY --from=build /app/publish .
  ENTRYPOINT ["dotnet", "PRN232.LAB_1.API.dll"]
  ```
- Use `dotnet publish` with `--self-contained false` (uses shared framework from the runtime image).

**Which phase should address it:** Phase 5 (Docker setup).

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| **Phase 0: Project Structure Fix** | Incorrect layer separation — API, Services, Repositories not properly decoupled | Convert Services and Repositories to class libraries. Remove `Program.cs`, controllers, web SDK. Add proper project references. |
| **Phase 1: Entity Models & DbContext** | Circular references causing JSON serialization failure | Design Response DTOs that omit reverse-navigation properties. Never return entities from API. |
| **Phase 1: DI Setup** | `DbContext` lifetime wrong | Register `DbContext` as Scoped (default). Never inject into singletons. |
| **Phase 2: Repository Interfaces** | Generic `IRepository<T>` leaking `IQueryable` | Use entity-specific repositories. Return `Task<List<T>>` or `Task<T?>`, never `IQueryable<T>`. |
| **Phase 3: Service Layer** | Anemic service layer / transaction script anti-pattern | Put validation and business rules in entity models, not services. Services coordinate, entities decide. |
| **Phase 4: Controller Actions** | Inconsistent response format across endpoints | Define `ApiResponse<T>` envelope upfront. Use factory methods. |
| **Phase 4: GET Collection** | N+1 queries due to missing `.Include()` | Add query logging in dev. Count queries. Use `.Include()`, `.AsSplitQuery()`, `.AsNoTracking()`. |
| **Phase 4: GET by ID** | 404 not handled for missing entities | Check for null, return `NotFound()`, not 200 with null data. |
| **Phase 4: Field Selection** | Over-fetching — ignoring `fields` parameter | Implement dynamic `Select()` or predefine minimal response DTOs. |
| **Phase 4: Pagination** | Missing pagination metadata (totalCount, pages) | Always run `COUNT(*)` alongside `.Skip()/.Take()`. Wrap in `PagedResponse<T>`. |
| **Phase 5: Docker Compose** | API cannot connect to DB container | Use service name in connection string, not `localhost`. Add `TrustServerCertificate=True`. |
| **Phase 5: Dockerfile** | Bloated SDK image in production | Use multi-stage build: SDK for build, `aspnet:8.0` runtime for execution. |
| **Phase 5: Seed Data (DATA-05)** | 500+ records via `HasData()` bloats migration files | Use `UseSeeding()` / `UseAsyncSeeding()` or a standalone seed script with batch inserts. Use Bogus for generation. |
| **Phase 5: Seed Data Order** | FK violations during seeding | Seed in dependency order: Semesters → Subjects → Courses → Students → Enrollments. |

---

## Research Flags for Phases

- **Phase 4 (Field Selection):** Implementing dynamic field selection (`?fields=id,name`) with EF Core's `Select()` requires building expression trees at runtime. This is non-trivial and may need deeper research or a library. Consider limiting field selection to response DTO properties only, or using a simple approach like manually switching on field names.
- **Phase 5 (Docker):** SQL Server Docker compatibility with ARM-based Macs/Windows requires `mcr.microsoft.com/azure-sql-edge` instead of `mcr.microsoft.com/mssql/server`. Check the development machine architecture before writing the Docker Compose file.
- **Phase 5 (Health Checks):** SQL Server Docker containers can take 30-60 seconds to initialize. Health check configuration in Docker Compose is essential for reliable startup ordering.

---

## Sources

- **Circular References:** Microsoft Learn — Related Data and Serialization (learn.microsoft.com/en-us/ef/core/querying/related-data/serialization) [HIGH confidence — official docs]
- **N+1 Queries:** Microsoft Learn — Efficient Querying (learn.microsoft.com/en-us/ef/core/performance/efficient-querying) [HIGH confidence — official docs]
- **N+1 Detection:** EF Core 11 N+1 Detection Guide (startdebugging.net) [MEDIUM confidence — multiple sources agree]
- **Layer Separation / IQueryable Leak:** Enterprise Craftsmanship — Which Collection Interface to Use (enterprisecraftsmanship.com) [MEDIUM confidence — authoritative blog, verified by code review]
- **Service Layer Anti-Pattern:** ByteCrafted — Why Most Service Layers Make Code Worse (bytecrafted.dev) [MEDIUM confidence — multiple sources verify pattern]
- **DbContext Lifetime:** Microsoft Learn — DbContext Configuration (learn.microsoft.com/en-us/ef/core/dbcontext-configuration) [HIGH confidence — official docs]
- **Docker Networking:** Stack Overflow — Docker Compose SQL Server Connection (stackoverflow.com/questions/78273183) [MEDIUM confidence — verified community knowledge]
- **Seed Data:** Microsoft Learn — Data Seeding (learn.microsoft.com/en-us/ef/core/modeling/data-seeding) [HIGH confidence — official docs]
- **Bulk Seed Performance:** no dogma blog — Seeding Large Database with EF (nodogmablog.bryanhogan.net) [MEDIUM confidence — blog with reproducible benchmarks]
- **Response Envelope:** ByteCrafted — DRY API Responses (bytecrafted.dev) [MEDIUM confidence — multiple sources agree on pattern]
- **Generic Repository Critique:** CodeOpinion — Avoiding Repository Pattern with ORM (codeopinion.com) [MEDIUM confidence — community expert, verified against EF Core documentation]
- **Response Format Standards:** REST API Naming Guide (botneve.com) [MEDIUM confidence — multiple sources agree]
