---
title: Codebase Concerns
created: 2026-05-15
focus: concerns
---

# Codebase Concerns

## Technical Debt

### Hardcoded Database Credentials in Three Locations
- **Files:** `PRN232.LAB_1.API/appsettings.json:10`, `PRN232.LAB_1.Repositories/Data/LmsDbContextFactory.cs:12`, `docker-compose.yml:7,23`
- **Issue:** The SQL Server SA password `Lab1_Pass123` is hardcoded in `appsettings.json`, `LmsDbContextFactory.cs` (design-time factory), and `docker-compose.yml` (both the SQL Server env var and the API connection string override). This is a secret-in-source anti-pattern.
- **Impact:** Any commit to a public repository exposes database credentials. Rotating the password requires updating three separate locations.
- **Fix approach:** Use `User Secrets` in development, environment variables in production/Docker, and remove the connection string from `LmsDbContextFactory.cs` (pass args instead).

### Double `GetAllAsync()` Overload Creates Dead Code
- **Files:** All 5 service interfaces + implementations (`SemesterService.cs:19`, `CourseService.cs:19`, `EnrollmentService.cs:19`, `StudentService.cs:19`, `SubjectService.cs:19`)
- **Issue:** Every service interface defines both `GetAllAsync()` (no params) and `GetAllAsync(PagedQuery query)`. The parameterless overload is never called — all controllers use the `PagedQuery` overload. This adds unnecessary maintenance surface.
- **Impact:** 5 unused methods across the codebase. Any future changes to query logic must be duplicated if kept, or risk divergence.
- **Fix approach:** Remove the parameterless `GetAllAsync()` from all service interfaces and implementations.

### `ConditionalJsonPropertyAttribute` Is Never Used
- **Files:** `PRN232.LAB_1.API/Attributes/ConditionalJsonPropertyAttribute.cs`
- **Issue:** This custom attribute class exists but has zero references anywhere in the codebase (no class uses `[ConditionalJsonProperty]`).
- **Impact:** Dead code adds confusion during onboarding and wastes maintenance effort.
- **Fix approach:** Remove the file unless the expand/field-selection feature is planned for future implementation.

### `Fields` Property in `PagedQuery` Is Never Used
- **File:** `PRN232.LAB_1.Services/Models/PagedQuery.cs:10`
- **Issue:** The `Fields` property exists on the query model but is never read by any service or mapper. The intended field-selection feature is not implemented.
- **Impact:** Dead query parameter that will silently be accepted but ignored by the API, producing misleading client behavior.
- **Fix approach:** Either implement field selection or remove the property (breaking change if clients already send it — though it currently does nothing).

### Business Model Layer Is Entirely Unused
- **Files:** `PRN232.LAB_1.Services/Models/SemesterBusiness.cs`, `CourseBusiness.cs`, `EnrollmentBusiness.cs`, `StudentBusiness.cs`, `SubjectBusiness.cs` + conversion methods in all 5 mapper classes
- **Issue:** The architecture specification defines 4 model types: Entity (Repository), Business (Services), Request (API), Response (API). However, no service method ever instantiates or uses a `Business` model. All conversions jump directly from `Request`/`Entity` to `Entity`/`Response`. The Business model and its mapper methods (e.g., `ToBusinessModel()`, `ToEntity(Business)`, etc.) are completely dead code.
- **Impact:** ~60 lines of dead code per mapper file (~300 lines total). Confusing for new developers who wonder why Business models exist but are never used.
- **Fix approach:** Remove Business models and their associated mapper methods, or introduce actual business logic that validates/transforms through them (e.g., enrollment capacity validation, duplicate enrollment checks).

### `Repository.GetByIdAsync()` Uses Tracked Entity While `GetQueryable()` Uses `AsNoTracking()`
- **Files:** `PRN232.LAB_1.Repositories/Repositories/Repository.cs:22-25` vs `Repository.cs:46-49`
- **Issue:** `GetByIdAsync()` uses `FindAsync()` (tracked query), but `GetQueryable()` prepends `AsNoTracking()`. This inconsistency means entities retrieved via `GetByIdAsync()` during `UpdateAsync` may already be tracked, causing `_dbSet.Update(entity)` to attach a duplicate or throw when called on an already-tracked entity.
- **Impact:** In `UpdateAsync`, the service calls `GetByIdAsync(id)` (entity is now tracked), then calls `_repository.UpdateAsync(entity)` which calls `_dbSet.Update(entity)` on an already-tracked entity. EF Core may throw `InvalidOperationException` for duplicate tracking.
- **Fix approach:** Either use `AsNoTracking()` in `GetByIdAsync()` as well, or change `UpdateAsync` to simply call `SaveChangesAsync()` without `_dbSet.Update()` when the entity is already tracked.

### `Repository.UpdateAsync()` Marks All Properties as Modified
- **File:** `PRN232.LAB_1.Repositories/Repositories/Repository.cs:34-38`
- **Issue:** `UpdateAsync` calls `_dbSet.Update(entity)` which marks ALL properties as modified, even unchanged ones. This generates UPDATE SQL for every column, not just changed columns.
- **Impact:** Unnecessary database writes, potential concurrency issues, and no partial update support. If the entity was already tracked (from `FindAsync`), `_dbSet.Update()` will fail or cause duplicate tracking.
- **Fix approach:** Use `_context.Entry(entity).State = EntityState.Modified` for untracked entities, or rely on change tracking for tracked ones and just call `SaveChangesAsync()`.

### Pagination String "Pagination" Is a Magic String
- **Files:** `PRN232.LAB_1.API/Controllers/SemesterController.cs:32`, `CourseController.cs:32`, `EnrollmentController.cs:32`, `StudentController.cs:32`, `SubjectController.cs:32` + `ResponseEnvelopeFilter.cs:21`
- **Issue:** The key `"Pagination"` for `HttpContext.Items` is a string literal repeated in 6 files. A typo in any controller or filter breaks pagination metadata silently.
- **Impact:** Brittle coupling between controllers and filter. No compile-time safety.
- **Fix approach:** Define a `public const string PaginationKey = "Pagination"` in a shared location (e.g., `ApiResponse.cs` or a new `Constants.cs`).

### No Unique Constraints on Entity Code Fields
- **Files:** All entity configuration files (`SemesterConfiguration.cs`, `CourseConfiguration.cs`, `SubjectConfiguration.cs`, `StudentConfiguration.cs`)
- **Issue:** Entity `Code` fields (e.g., `SP2025`, `STU001`, `PRN232-1`) have no unique indexes defined in EF configurations. The database will allow duplicate codes.
- **Impact:** Data integrity risk — the application can create two semesters with code `SP2025`. The layer has no protection against this.
- **Fix approach:** Add `builder.HasIndex(e => e.Code).IsUnique()` to each entity configuration for entities with Code fields.

### Seed Data Enrollment Dates Limited to 14 Days After Semester Start
- **File:** `PRN232.LAB_1.Repositories/Data/DataSeeder.cs:121`
- **Issue:** All enrollment dates are generated within 14 days of the semester start date: `semester.StartDate.AddDays(random.Next(0, 14))`. This creates unrealistic enrollment data where no student ever enrolls late.
- **Impact:** Seed data doesn't reflect real-world enrollment patterns. Makes demo/testing less representative.
- **Fix approach:** Use a wider date window (e.g., `random.Next(-7, 30)` for pre- and post-start enrollments).

## Known Bugs

### `EnrollmentService.GetAllAsync` Sort by Grade Throws on Null Grades
- **File:** `PRN232.LAB_1.Services/Services/EnrollmentService.cs:54`
- **Issue:** When sorting by `grade` with `SortDesc=true`, the expression `q.OrderByDescending(e => e.Grade!)` uses the null-forgiving operator but doesn't filter out nulls. If any enrollment has `Grade == null`, the `ORDER BY` clause in SQL Server will handle nulls as lowest, but the `null-forgiving` operator is misleading and the query might produce unexpected sort order mixing nulls and values.
- **Trigger:** Request `GET /api/enrollments?sortBy=grade` with enrollments that have null grades.
- **Severity:** Low. SQL Server sorts nulls first for `ASC`, last for `DESC`. The behavior matches what most would expect, but the `!` operator is incorrect and masks the intent.
- **Fix approach:** Use explicit ordering: `q.OrderBy(e => e.Grade ?? "")` or add `.Where(e => e.Grade != null)` before sorting.

### Delete on Parent Entity With Children Throws Unhandled Exception
- **Files:** `PRN232.LAB_1.API/Controllers/SemesterController.cs:108-113`, `CourseController.cs:97-102`, `StudentController.cs:97-102` + `PRN232.LAB_1.Repositories/Repositories/Repository.cs:40-44`
- **Issue:** Deleting a `Semester`, `Course`, or `Student` that has dependent `Course` or `Enrollment` records will throw a `DbUpdateException` because all foreign keys use `DeleteBehavior.Restrict`. The API has no exception handler or try-catch around `DeleteAsync`, so this results in a 500 error with no useful response.
- **Trigger:** `DELETE /api/semesters/1` when semester has courses. `DELETE /api/students/1` when student has enrollments.
- **Severity:** Medium. The API returns an unhandled 500 error instead of a proper 409 Conflict or 400 Bad Request.
- **Fix approach:** Wrap delete in try-catch in the service layer. Catch `DbUpdateException` and return a meaningful error (e.g., "Cannot delete semester with existing courses").

## Security Considerations

### Plaintext Secret in Docker Compose and Source
- **Files:** `docker-compose.yml:6-7,22-23`, `PRN232.LAB_1.API/appsettings.json:10`, `PRN232.LAB_1.Repositories/Data/LmsDbContextFactory.cs:12`
- **Risk:** Database SA password is stored in plaintext in three locations, two of which are committed to version control (`appsettings.json`, `docker-compose.yml`, `LmsDbContextFactory.cs`).
- **Current mitigation:** The `.gitignore` excludes `.planning/` but not the project files containing secrets.
- **Recommendations:** Move to environment variables for production, User Secrets for development, and remove the hardcoded string from `LmsDbContextFactory.cs`.

### Swagger Enabled in Docker (Production-Like) Environment
- **File:** `PRN232.LAB_1.API/Program.cs:72-77`
- **Risk:** Swagger UI is exposed when `ASPNETCORE_ENVIRONMENT=Docker`. If this image is deployed in any production-like scenario, the full API documentation including endpoint signatures is publicly accessible.
- **Current mitigation:** None. The condition checks for `Development` or `Docker`.
- **Recommendations:** Add an explicit configuration flag (e.g., `Swagger:Enabled`) that defaults to false in production. Never auto-enable Swagger based solely on environment name.

### No Global Exception Handler
- **File:** `PRN232.LAB_1.API/Program.cs` (missing)
- **Risk:** Unhandled exceptions (e.g., `DbUpdateException` from FK violations, `FormatException`, `NullReferenceException`) will return the ASP.NET Core default 500 error HTML page, which in development/Docker may include stack traces. No `UseExceptionHandler()` or custom middleware is configured.
- **Current mitigation:** None. The `ResponseEnvelopeFilter` only handles 400 and 404 results, not 500 errors.
- **Recommendations:** Add `app.UseExceptionHandler()` to the middleware pipeline with a custom handler that returns a JSON `ApiResponse<object>` failure envelope.

### Missing CORS Policy
- **File:** `PRN232.LAB_1.API/Program.cs` (missing)
- **Risk:** No CORS is configured. If this API is consumed by a frontend application (common for LMS systems), cross-origin requests will be blocked by the browser.
- **Current mitigation:** `appsettings.json` has `"AllowedHosts": "*"` but this only affects host header validation, not CORS.
- **Recommendations:** Add CORS policy if frontend consumption is expected. Even for development, `AllowAnyOrigin()` for localhost testing.

### No Rate Limiting or Request Throttling
- **File:** Not configured anywhere in the codebase.
- **Risk:** No protection against brute force or DoS attacks on any endpoint. An attacker can call `POST /api/enrollments` or search endpoints repeatedly without restriction.
- **Current mitigation:** PageSize is clamped to [1, 100] in the service layer (T-03-10 mitigation), but this is a soft limit, not enforced by middleware.
- **Recommendations:** Add ASP.NET Core rate limiting middleware with per-endpoint or per-IP policies.

## Performance Concerns

### Service-Layer Search Uses Client-Side `ToLower().Contains()` Pattern
- **Files:** All 5 service classes (e.g., `CourseService.cs:33-36`, `StudentService.cs:32-36`, `EnrollmentService.cs:33-34`)
- **Problem:** Search filtering uses `e => e.Code.ToLower().Contains(s)` which EF Core translates to `CHARINDEX` or `LOWER` in SQL. While EF Core does translate this server-side for SQL Server, it prevents efficient index usage because the column is wrapped in `LOWER()`, making any index on `Code` unusable.
- **Cause:** The pattern forces a full table scan + scalar operation for every search.
- **Improvement path:** Use `EF.Functions.Like()` for pattern-based searches, or use a case-insensitive database collation and compare directly with `.Contains()`.

### `Repository.GetAllAsync()` Loads All Rows Into Memory
- **File:** `PRN232.LAB_1.Repositories/Repositories/Repository.cs:17-20`
- **Problem:** `GetAllAsync()` calls `_dbSet.AsNoTracking().ToListAsync()` which loads every row of the table into memory. For a table with thousands of rows, this is a significant memory and performance issue.
- **Cause:** This method is not called by controllers (they use the paged overload via the service layer), but it's still exposed and could be accidentally used by any future code.
- **Improvement path:** Remove the method entirely or add a `maxResult` parameter with a default limit.

### Pagination Causes Double Query (Count + Data)
- **Files:** All 5 service classes (e.g., `SemesterService.cs:55-63`)
- **Problem:** Every paged query executes two SQL queries: one `COUNT(*)` and one `SELECT ... OFFSET...FETCH`. For simple endpoints this is negligible, but it doubles database round-trips.
- **Cause:** The pattern is standard for pagination and not necessarily a bug, but for high-traffic endpoints it doubles query load.
- **Improvement path:** Use window functions in a single query for count + data (EF Core 8 supports this with `EF.Functions`), or cache total counts with periodic invalidation.

## Fragile Areas

### `Program.cs` Initialization — Blocking Startup With Retry Loop
- **File:** `PRN232.LAB_1.API/Program.cs:44-67`
- **Why fragile:** The migration and seeding logic runs synchronously during startup inside `Program.cs`, not in a background task. The exponential backoff retry loop (up to 5 attempts, max 32s) blocks the application from serving requests. If the database connection fails after all retries, the exception is unhandled and the process crashes.
- **What breaks:** Entire application is unavailable during migration/seed. If `DataSeeder.SeedAsync` throws after the transaction starts but before the catch, the `catch` block rolls back, but any partial `SaveChangesAsync` within the seed before the transaction would already be committed.
- **Safe modification:** Extract migration/seed into a separate hosted service (`IHostedService` or background task) so the application starts and health checks can report readiness separately.
- **Test coverage:** No tests exist for the startup retry logic or the seed transaction atomicity.

### `ResponseEnvelopeFilter` — Reflection-Heavy With No Caching
- **File:** `PRN232.LAB_1.API/Filters/ResponseEnvelopeFilter.cs:26-29,50-53,57-63,81-84,88-91,95-98`
- **Why fragile:** The filter uses `typeof(ApiResponse<>).MakeGenericType(...).GetMethod(...)` reflection on every single API response. This is both slow (reflection overhead per request) and fragile — if `ApiResponse` static methods are renamed or signatures change, this breaks at runtime with no compile-time error.
- **What breaks:** Any change to the `ApiResponse<T>` class method signatures (e.g., `Ok` -> `Success`, adding a parameter) will cause `GetMethod`/`Invoke` to return null or throw `TargetInvocationException` at runtime.
- **Safe modification:** Use an interface-based approach (e.g., `IApiResponse` non-generic interface) or inject a factory service instead of using reflection. Cache the `MethodInfo` instances in a static dictionary.
- **Test coverage:** No unit tests for the filter's reflection logic.

### No `ILogger<T>` Used Anywhere
- **Files:** Entire codebase — no `ILogger<T>` injection in any controller, service, or filter.
- **Why fragile:** When any operation fails (database timeout, FK violation, validation error), there is no diagnostic output. The `Program.cs` retry loop uses `Console.WriteLine` instead of structured logging. Debugging production issues will require reproducing the exact scenario.
- **What breaks:** Silent failures in the service layer, unhelpful 500 responses with no traceability.
- **Safe modification:** Inject `ILogger<T>` into each service class and log entry/exit/error. Use structured logging properties for correlation IDs.

### No Tests Anywhere in the Codebase
- **Files:** No test projects exist at all — no `.Tests` directory, no test files.
- **Why fragile:** Zero test coverage on all layers: repository logic, service search/sort/paging/expand logic, mapper conversions, controller routing, filter response wrapping.
- **What breaks:** Any refactoring or enhancement risks silent regressions. There is no safety net.
- **Safe modification:** The most critical tests are: (1) service paged query logic (search, sort, paging boundary), (2) mapper conversions with expand arrays, (3) `ResponseEnvelopeFilter` wrapping for all HTTP status codes, (4) controller action-to-service method wiring.
- **Priority:** High — this is the single biggest risk in the codebase.

### FK Cascade Restrict on All Relationships — Silent Delete Failures
- **Files:** `CourseConfiguration.cs:23,28`, `EnrollmentConfiguration.cs:21,26`
- **Why fragile:** All foreign keys use `DeleteBehavior.Restrict`. While this prevents accidental cascading deletes, the error surfaces as an unhandled `DbUpdateException` at the database layer. The controller returns `Ok(new { message = "Deleted successfully" })` but the delete actually fails.
- **What breaks:** `DELETE /api/semesters/1` returns 200 with "Deleted successfully" but the semester is NOT deleted (the FK violation rolls back the entire SaveChanges). The client gets a false success.
- **Safe modification:** Check for dependent records before attempting delete, and return a 409 Conflict with a meaningful message. Wrap delete operations in try-catch.

### Migration Retry Loop Has No Timeout on `DataSeeder.SeedAsync()`
- **File:** `PRN232.LAB_1.API/Program.cs:44-67`
- **Why fragile:** The retry loop only guards `db.Database.MigrateAsync()`. If `MigrateAsync()` succeeds but `DataSeeder.SeedAsync(db)` throws (e.g., duplicate data, timeout), there is no retry — the exception propagates unhandled.
- **What breaks:** If seed data insertion fails, the database is migrated but empty, and the application crashes. On restart, the idempotency check `if (await context.Semesters.AnyAsync())` will be false (no semesters because seed failed), so it retries seeding — but if the failure persists, the application will never start.
- **Safe modification:** Wrap the entire `MigrateAsync` + `SeedAsync` block in a single try-catch with proper error logging, or seed inside the retry loop as well.

## Missing Critical Features

### No Health Check Endpoint
- **Problem:** Docker Compose has `depends_on: sqlserver` but no health check for the API. Docker has no way to know when the API is ready to serve traffic (after migrations + seeding). Kubernetes liveness/readiness probes cannot be configured.
- **Blocks:** Reliable container orchestration, zero-downtime deployments.
- **Files affected:** `docker-compose.yml`, `Program.cs`
- **Fix:** Add `MapHealthChecks()` or a simple `GET /health` endpoint that verifies database connectivity.

### No Validation That FK Entities Exist Before Creating/Updating
- **Problem:** `CourseRequest` accepts `SubjectId` and `SemesterId` but `CourseService.AddAsync` never checks that the referenced `Subject` and `Semester` actually exist. If a client sends an invalid FK, the database will throw `DbUpdateException` (FK violation), resulting in an unhandled 500.
- **Files affected:** All service `AddAsync` and `UpdateAsync` methods (e.g., `CourseService.cs:98-103`, `EnrollmentService.cs:95-100`)
- **Fix:** Validate that referenced entities exist before creating/updating. Return a 400 Bad Request with appropriate error message.

### No Enrollment Capacity Enforcement
- **Problem:** `Course.MaxStudents` is stored in the database but never enforced. `EnrollmentService.AddAsync` will enroll a student even if the course is already at capacity.
- **Files affected:** `PRN232.LAB_1.Services/Services/EnrollmentService.cs:95-100`
- **Fix:** Before adding an enrollment, count existing enrollments for the course and reject if `>= MaxStudents`.

### No Duplicate Enrollment Check
- **Problem:** `EnrollmentService.AddAsync` does not check if a student is already enrolled in the same course. Duplicate enrollments are allowed at the application layer.
- **Files affected:** `PRN232.LAB_1.Services/Services/EnrollmentService.cs:95-100`
- **Fix:** Check for existing enrollment with matching `StudentId` + `CourseId` before creating a new one. Return 409 Conflict on duplicate.

## Test Coverage Gaps

- **What's not tested:** Entire application — 0 test projects, 0 test files across all 3 layers (Repository, Service, API).
- **Files:** All `.cs` files in the solution.
- **Risk:** Any change may break existing functionality without detection. The search/sort/paging/expand logic in services, the reflection-based `ResponseEnvelopeFilter`, and the FK constraint handling are particularly high-risk untested areas.
- **Priority:** High

---

*Concerns audit: 2026-05-15*
