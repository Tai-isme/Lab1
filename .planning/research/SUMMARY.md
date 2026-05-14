# Project Research Summary

**Project:** PRN232 Lab 1 — LMS REST API
**Domain:** Learning Management System — Academic Management (Semesters, Courses, Subjects, Students, Enrollments)
**Researched:** 2026-05-14
**Confidence:** HIGH

## Executive Summary

This is a 3-layer ASP.NET Core 8 REST API for academic management — handling Semesters, Courses, Subjects, Students, and Enrollments. The industry-standard approach is a strict layered architecture (API → Services → Repositories) with Entity Framework Core for data access, SQL Server for storage, and Swashbuckle for API documentation, all containerized via Docker Compose.

**The recommended approach:**
1. **Fix the scaffold first** — the current codebase has all 3 projects as standalone web API executables (`Microsoft.NET.Sdk.Web`) with zero project references, no interfaces, and no layering. Convert Services and Repositories to class libraries and wire up the dependency chain before writing any business code.
2. **Build bottom-up** — Repositories → Services → API, with clear interface boundaries at each layer.
3. **Deliver features in dependency order** — CRUD first (Semester → Subject → Student → Course → Enrollment), then collection features (paging/sort/search), then differentiators (field selection, expansion, bulk enrollment), then polish (soft delete, aggregation).

**Key risks:**
- **Circular references in JSON serialization** (Entities have bidirectional navigation properties). Solution: Never return entity models from API — always map to response DTOs that omit reverse navigation.
- **N+1 queries at scale** (500+ enrollment seed data). Solution: Use `.Include()`, `.AsSplitQuery()`, `.AsNoTracking()`, and projection queries.
- **Docker networking** (API can't reach SQL Server). Solution: Use Docker Compose service names in connection strings, not `localhost`.
- **Seed data bloat** (500+ records via `HasData()` breaks migrations). Solution: Use `UseSeeding()`/`UseAsyncSeeding()` with batched inserts (100 per batch), not `HasData()`.

## Key Findings

### Recommended Stack

.NET 8 / ASP.NET Core 8 / C# 12 with EF Core 8.0.26 + SQL Server 2022, containerized with Docker Compose. Swashbuckle 6.6.2 stays as-is (v8+ requires newer .NET TFMs). **Critical fix needed:** Services and Repositories projects must be converted from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` — they are class libraries, not web apps.

**Core technologies:**
- **.NET 8 SDK / ASP.NET Core 8**: LTS release supported until Nov 2026; lab spec requires `net8.0`; built-in DI, middleware, and model binding.
- **EF Core 8.0.26 + SQL Server 2022**: Code-first migrations, LINQ queries, repository pattern. SQL Server 2022 via Docker (`mcr.microsoft.com/mssql/server:2022-latest`) is more stable than 2025-latest.
- **Swashbuckle 6.6.2**: Swagger/OpenAPI 3.0 generation — already in the project; correct version for `net8.0`.
- **Docker Compose v2**: Multi-container orchestration with SQL Server healthcheck sequencing.
- **Libraries NOT used**: AutoMapper (manual mapping for 5 entity types), FluentValidation (data annotations suffice), Serilog (out of scope per lab spec), MediatR (overkill for simple CRUD), Dapper (EF Core exclusively).

### Expected Features

**Must have (table stakes) — all 15 from FEATURES.md:**
- Full CRUD for all 5 entities (Semester, Course, Subject, Student, Enrollment)
- Consistent JSON response envelope: `{ success, data, message, errors }`
- Collection GET with paging (`?page=1&size=10`), sort (`?sort=LastName desc`), search/filter (`?search=text`, `?status=active`)
- GET by ID with full expansion, 404 on missing resource
- Proper HTTP status codes (200, 201, 204, 400, 404, 409, 500)
- Swagger/OpenAPI docs, resource-based URL patterns (`api/semesters`)
- Validation error details, 201 Created with Location header, 204 No Content on DELETE
- Empty collection returns 200 with `[]`, PUT/PATCH support

**Should have (differentiators):**
- Field selection (`?fields=id,firstName,lastName`) — medium complexity
- Related data expansion (`?expand=enrollments` or `?include=courses`) — medium complexity
- Bulk enrollment (`POST api/enrollments/bulk`) — medium complexity
- Semester context filtering (`?semesterId=5`) — low complexity
- Enrollment status management (Active, Dropped, Completed, Withdrawn) — low-medium
- Enrollment count aggregation (`GET /courses/5/stats`) — low complexity
- Multi-field search (`?search=john` matches firstName+lastName+email) — low-medium
- Soft delete (`IsDeleted` flag, `?includeDeleted=true`) — medium complexity

**Defer (not building):** Auth/JWT, global exception handling middleware, tests, webhooks, file upload, RBAC, grades, caching, LTI/SCORM, rate limiting, API versioning, HATEOAS, ETags, CSV/PDF export — all explicitly out of scope per lab spec.

### Architecture Approach

Strict 3-layer architecture where **dependencies flow downward** and **no layer skips another**:

```
API (Controllers, Program.cs, Swagger)
  → references Services (interfaces + implementations)
    → references Repositories (DbContext, entities, EF Core)
```

**Four model types with strict boundaries:**
1. **Entity models** (Repositories/Entities) — 1:1 with DB tables, EF Core attributes, navigation properties.
2. **Business models** (Services/Models/Business) — domain processing, validation rules, no EF/JSON attributes.
3. **Request DTOs** (Services/Models/Request) — client input contracts with data annotations.
4. **Response DTOs** (Services/Models/Response) — API output contracts, flat shape, no navigation properties.

**Key patterns:**
- Generic `IRepository<T>` + entity-specific repository interfaces — but repositories return materialized collections, NOT `IQueryable<T>`
- Service layer handles search/sort/paging on `IQueryable<T>` returned from repositories
- Consistent `ApiResponse<T>` envelope wrapping all responses
- `QueryParameters` model for all queryable collection endpoints
- `PagedResult<T>` with page/pageSize/totalCount/totalPages metadata
- Scoped DI lifetime for DbContext, repositories, and services (one per HTTP request)
- Dependency Injection via `Program.cs` or extension methods per layer

### Critical Pitfalls

1. **Circular Reference → JSON Serialization Crash**: Entity navigation properties create object cycles. **Prevention:** Never return entities from API — always map to Response DTOs that omit reverse navigation properties.

2. **N+1 Query Problem**: Loading 500+ enrollments triggers 501+ SQL queries. **Prevention:** Use `.Include()`, `.AsSplitQuery()`, `.AsNoTracking()`, and projection `.Select()` to DTOs.

3. **Leaky Architecture**: Business logic in controllers, ORM leaks in services, repositories returning `IQueryable<T>`. **Prevention:** Convert Services/Repos to class libraries, enforce interface boundaries, return materialized collections from repos.

4. **Docker Networking**: API container uses `localhost` in connection string → can't reach SQL Server. **Prevention:** Use Docker Compose service name as hostname, add `TrustServerCertificate=True`, use healthcheck on DB service.

5. **Seed Data Bloat**: 500+ records via `HasData()` produces MB-sized migration files and times out. **Prevention:** Use `UseSeeding()`/`UseAsyncSeeding()` with batched inserts (100 per batch) and Bogus for realistic data. Seed in FK dependency order.

## Implications for Roadmap

Based on research, the following phase structure is recommended. This combines the bottom-up build order from ARCHITECTURE.md with the feature prioritization from FEATURES.md, while avoiding pitfalls from PITFALLS.md.

### Phase 0: Project Scaffold Fix (Pre-requisite)
**Rationale:** The current scaffold has all 3 projects as standalone web APIs with zero layering. Building features on top of this multiplies technical debt. This must be addressed before any business code.
**Delivers:** Clean 3-layer project structure with correct SDKs, project references, and interface stubs.
**Addresses:** ARCHITECTURE needs (correct dependency chain)
**Avoids:** Pitfall #3 (leaky architecture — incorrect layer separation)
**Steps:** Fix SDKs, add project references, remove scaffold boilerplate (Controllers, Program.cs, WeatherForecast, launchSettings from Services/Repos), add NuGet packages, create interface stubs.

### Phase 1: Repositories Layer (Foundation)
**Rationale:** Everything depends on this layer — entities, DbContext, and data access must exist before anything else can work.
**Delivers:** Entity models, DbContext, entity configurations (Fluent API), generic + specific repository interfaces and implementations, initial EF migration.
**Addresses:** Table-stakes features DATA-01 (entity models), supports all CRUD operations.
**Avoids:** Pitfall #6 (DbContext lifetime — register as Scoped), Pitfall #8 (generic repository over-abstraction — use entity-specific repos that return materialized collections, not IQueryable).
**Standard patterns:** Well-documented EF Core + Repository pattern. Skip research-phase.

### Phase 2: Services Layer (Business Logic)
**Rationale:** Services depend on Repositories being complete. This layer provides the abstraction boundary the API controllers call into.
**Delivers:** Service interfaces + implementations, Request/Response DTOs, AutoMapper profiles, business validation logic, `PagedResult<T>` model, `QueryParameters` model, `ApiResponse<T>` envelope.
**Addresses:** Table-stakes features DATA-02/03/04 (model types), API-05 (consistent response format).
**Avoids:** Pitfall #7 (inconsistent response envelope — define ApiResponse<T> upfront), Pitfall #10 (over-fetching — response DTOs are lean by design).
**Standard patterns:** Well-established service layer + DTO mapping pattern. Skip research-phase.

### Phase 3: API Layer — Core CRUD (MVP)
**Rationale:** Controllers depend on Services being complete. Start with the simplest entities (no FKs) and progress to Enrollment (3 FKs). This phase delivers a working, demonstrable API.
**Delivers:** 5 controllers with full CRUD (GET by ID, GET collection, POST, PUT, PATCH, DELETE), consistent `ApiResponse<T>` wrapping, proper HTTP status codes (200, 201, 204, 400, 404, 409, 500), Swagger docs with `[ProducesResponseType]` annotations.
**Build order within phase:** Semester → Subject → Student → Course → Enrollment (FK dependency order).
**Addresses:** Table-stakes features #1-15 (FEATURES.md) — all must-have features.
**Avoids:** Pitfall #1 (circular references — response DTOs omit reverse nav properties), Pitfall #9 (404 vs empty — correct by-id vs collection semantics).
**Standard patterns:** Well-documented ASP.NET Core controller patterns. Skip research-phase.

### Phase 4: Collection Power & Differentiators
**Rationale:** Paging, sorting, and search transform the API from "toy CRUD" into something usable. These build on the `IQueryable` pattern established in the Services layer. Differentiators (field selection, expansion, bulk enrollment) follow once collection basics work.
**Delivers:** Paging with metadata (`?page=1&size=10`), sort (`?sortBy=LastName&sortDir=desc`), search/filter (`?search=text`, `?status=active`), field selection (`?fields=id,name`), related data expansion (`?expand=enrollments`), bulk enrollment, enrollment status management, semester context filtering, enrollment count aggregation, multi-field search.
**Addresses:** Differentiators #1-9 (FEATURES.md) — field selection, expansion, bulk enrollment, status management, aggregation.
**Avoids:** Pitfall #2 (N+1 queries — use Include/SplitQuery/AsNoTracking/Select projections), Pitfall #11 (pagination without metadata — always return totalCount).
**Research flags:**
- **Field selection (`?fields=`)** requires dynamic expression tree building for EF Core `Select()` — non-trivial. Consider a simplified approach (property inclusion/exclusion at the DTO level) or deeper research during planning.
- **String comparison culture** in search — need to decide case-insensitive vs case-sensitive behavior for SQL Server.

### Phase 5: Docker + Seed Data (Deployment)
**Rationale:** Containerization wraps the finished API. Seed data must be carefully handled to avoid migration bloat.
**Delivers:** `docker-compose.yml` (SQL Server + API services with healthcheck), multi-stage Dockerfile, appsettings.Docker.json, 500+ realistic seed records (Bogus), `UseSeeding()`/`UseAsyncSeeding()` batch insert logic.
**Addresses:** DATA-05 (seed data requirement), deployment readiness.
**Avoids:** Pitfall #4 (Docker networking — use service name, TrustServerCertificate=True, healthcheck), Pitfall #5 (seed data bloat — use batched UseSeeding, not HasData), Pitfall #12 (Docker image size — multi-stage build, runtime image not SDK).
**Research flags:**
- **Docker ARM compatibility:** If the dev machine uses ARM (Apple Silicon, some Windows ARM), use `mcr.microsoft.com/azure-sql-edge` instead of `mcr.microsoft.com/mssql/server`. Verify architecture before writing Docker Compose.
- **SQL Server startup time:** The container can take 30-60 seconds to initialize. Health check configuration is essential.
- **EF Core migration strategy in Docker:** Decide whether to auto-migrate at startup or apply migrations manually.

### Phase 6: Polish (if time permits)
**Rationale:** Soft delete and course stats are nice-to-have differentiators that build on working CRUD.
**Delivers:** Soft delete with `IsDeleted` flag, global query filter, `?includeDeleted=true` support, course enrollment stats endpoint.
**Addresses:** Differentiators #10 (soft delete from FEATURES.md).
**Avoids:** Pitfall #1 (soft delete responses must still use DTOs, not entities).

### Phase Ordering Rationale

- **Bottom-up dependency chain** (Phase 0→1→2→3→4→5): Entities → Repos → Services → Controllers → Collection features → Docker. This matches the architecture dependency direction and ensures nothing is built on an incomplete foundation.
- **Entity order within phases** (Semester → Subject → Student → Course → Enrollment): FK dependency dictates this — Courses reference Semester+Subject, Enrollments reference Student+Course. Building entities in FK order prevents circular blocking.
- **Features grouped by layer and dependency**: CRUD per entity (Phase 3) before collection features (Phase 4) before cross-cutting differentiators (Phase 4). Field selection and expansion build on the collection query infrastructure.
- **Docker/seed last**: Containerization is wrapping, not building. Seed data needs all entities and relationships to exist.
- **Pitfall avoidance is built into the phase order**: Phase 0 prevents leaky architecture, Phase 1/2 set up DI lifetimes and response envelopes correctly before controllers are written, Phase 3 uses DTOs to prevent circular reference crashes, Phase 4 addresses N+1 before the 500-record dataset is loaded, Phase 5 uses `UseSeeding` instead of `HasData`.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 4 (Field Selection):** Dynamic EF Core `Select()` with expression trees is needed for `?fields=id,name`. This is non-trivial and may need a dedicated spike or library evaluation.
- **Phase 5 (Docker ARM Compatibility):** If the dev machine is ARM-based, the SQL Server image must be `azure-sql-edge` instead of `mssql/server`. Verify before writing compose files.
- **Phase 5 (Seed Data Strategy):** `UseSeeding()` has specific EF Core requirements. The batch size (100), idempotency check, and FK ordering need careful implementation.

Phases with standard patterns (skip research-phase):
- **Phase 1 (EF Core + Repositories):** Well-documented, official Microsoft guidance, thousands of examples.
- **Phase 2 (Services + DTO Mapping):** Standard layering pattern, well-established in .NET ecosystem.
- **Phase 3 (Controllers + REST):** Standard ASP.NET Core controller patterns, widely documented.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Verified NuGet package versions, Microsoft docs, MCR image tags. Project SDK fix (Web→Sdk) confirmed by Microsoft documentation. |
| Features | HIGH | Cross-referenced 6+ production LMS APIs (Canvas, Ed-Fi, Moodle, Oracle SIS, Open edX, IMS Edu-API). Lab spec confirms scope boundaries. |
| Architecture | HIGH | Microsoft official guidance on n-layer patterns, EF Core configuration, DI lifetime. Codebase audit confirms current broken state. |
| Pitfalls | HIGH | Verified against Microsoft docs (circular refs, N+1, DbContext lifetime, seed data). Community sources with multiple independent confirmations. Docker networking from verified Stack Overflow solutions. |

**Overall confidence:** HIGH

### Gaps to Address

- **AutoMapper vs Manual Mapping Decision:** ARCHITECTURE.md includes AutoMapper in project references, but STACK.md recommends manual mapping for 5 entity types to avoid complexity. Must decide during Phase 2 implementation. **Recommendation:** Manual mapping for simplicity at this scale.
- **`IQueryable<T>` Debate:** STACK and ARCHITECTURE suggest services build queries on `IQueryable<T>` from repositories (applying Where/OrderBy/Skip/Take). PITFALLS.md warns against leaking `IQueryable<T>`. **Recommendation:** For this lab, returning `IQueryable<T>` from the generic repository's `GetQueryable()` method (with the DB still open in the scoped context) is acceptable — the alternative of parameterizing every filter/sort/page combination creates an explosion of method overloads. Document that this is a tradeoff.
- **Field Selection Implementation:** No consensus on the best approach for dynamic `Select()` in EF Core. **Recommendation:** Flag for a dedicated spike during Phase 4 planning. Fallback: return all fields and filter at the API serialization level.
- **Docker ARM Architecture:** Cannot verify the dev machine's processor architecture from here. **Recommendation:** Add a validation step at Phase 5 to check `docker info | findstr Architecture` and adjust SQL Server image accordingly.

## Sources

### Primary (HIGH confidence)
- Microsoft Learn — EF Core NuGet packages and releases: learn.microsoft.com/en-us/ef/core/what-is-new/
- Microsoft Learn — Common web application architectures: learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- Microsoft Learn — EF Core Data Seeding: learn.microsoft.com/en-us/ef/core/modeling/data-seeding
- Microsoft Learn — EF Core Efficient Querying: learn.microsoft.com/en-us/ef/core/performance/efficient-querying
- Microsoft Learn — DbContext Configuration: learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
- Microsoft Learn — Swashbuckle + ASP.NET Core 8: learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle
- MCR — SQL Server Container Images: mcr.microsoft.com/product/mssql/server/about
- Canvas LMS REST API Docs (Instructure)
- Open edX LMS API Docs
- Ed-Fi API Design Guidelines v2.0
- IMS Edu-API v1.0 spec
- PROJECT.md (primary lab spec)

### Secondary (MEDIUM confidence)
- NuGet Gallery — EF Core SqlServer 8.0.26: nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/8.0.26
- Woodruff — Layered Architecture in ASP.NET Core: woodruff.dev
- Docker Compose SQL Server Template: github.com/kakashidota/docker-compose-dotnet-sql-template
- Enterprise Craftsmanship — Which Collection Interface to Use
- ByteCrafted — Why Most Service Layers Make Code Worse
- ByteCrafted — DRY API Responses
- no dogma blog — Seeding Large Database with EF
- CodeOpinion — Avoiding Repository Pattern with ORM
- Stack Overflow — Docker Compose SQL Server Connection
- Oracle Student Management REST API
- Moodle Web Services Docs

### Tertiary (LOW confidence)
- (None — all major decisions are backed by official Microsoft documentation or multiple independent community sources.)

---

*Research completed: 2026-05-14*
*Ready for roadmap: yes*
