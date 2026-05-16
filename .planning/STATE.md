---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: executing
stopped_at: Completed 04-01 plan — Docker containerization + seed data
last_updated: "2026-05-16T10:00:00Z"
last_activity: 2026-05-16
progress:
  total_phases: 8
  completed_phases: 6
  total_plans: 15
  completed_plans: 12
  percent: 80
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-05-14)

**Core value:** All LMS resources (semesters, courses, subjects, students, enrollments) can be CRUD-managed via RESTful endpoints with search, sort, paging, field selection, and expansion — meeting the lab evaluation checklist.
**Current focus:** Phase 08 — thêm phase fix lỗi này

## Current Position

Phase: 04 (deployment-docker-seed-data) — COMPLETE
Plan: 04-01 completed
Status: Phase 04 complete (1/1 plans done)
Last activity: 2026-05-16

Progress: [██████████] 80%

## Performance Metrics

**Velocity:**

- Total plans completed: 11
- Average duration: ~5 min
- Total execution time: ~55 min

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 1. Foundation | 2 | 2 | ~1 min |
| 2. Business Logic | 1 | 1 | ~1 min |
| 3. API | 3 | 3 | ~2 min |
| 4. Deployment | 1 | 1 | ~2 min |
| 7. List API Fix | 2 | 2 | ~2 min |
| 8. Fix lỗi | 2 | 2 | ~11 min |

*Updated after each plan completion*

## Accumulated Context

### Roadmap Evolution

- Phase 5 added: Fix project chưa đúng theo yêu cầu Business models được sử dụng đúng mục đích
- Phase 6 added: Fix the fail rule
- Phase 7 added: Fix the missing/gaps/issues to match the requirement 5. GET Collection Resource (List API)
- Phase 8 added: Fix lỗi 500 Internal Server Error — thêm global exception handler cho consistent response format

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Phase 1]: Convert Services and Repositories from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` (class libraries)
- [Phase 1]: Add project references API → Services → Repositories
- [Phase 1]: Use Entity Framework Core for data access with code-first migrations
- [All Phases]: Use manual model mapping (no AutoMapper) for 5 entity types
- [Phase 1]: int auto-increment PKs for all 5 entities
- [Phase 1]: Single generic `IRepository<T>` with CRUD methods (GetAll, GetById, Add, Update, Delete)
- [Phase 1]: Async-only repository methods, hard delete, null on not-found
- [Phase 1]: Fluent API via `IEntityTypeConfiguration` — one file per entity, assembly scan registration
- [Phase 1]: Plural table names, Restrict/NoAction cascade
- [Phase 1]: Connection string from appsettings.json + env var override, DbContext in API Program.cs
- [Phase 2]: Keep 4 model types (Entity, Business, Request, Response) including Business models
- [Phase 2]: Per-entity service interfaces — ISemesterService, ICourseService, etc.
- [Phase 2]: Static mapper classes per entity for manual model mapping
- [Phase 2]: Rewire API DI to go through Services layer in this phase
- [Phase 2]: Basic data annotations ([Required], [StringLength], [Range]) on Request DTOs
- [Phase 2]: Service implementations inject IRepository<T> and use static mapper classes
- [Phase 2]: DependencyInjection.cs with AddApplicationServices — all service + repository registrations centralized
- [Phase 2]: API Program.cs uses AddApplicationServices(), removes direct repository references
- [Phase 4]: Separate DataSeeder class in Repositories/Data/ — static one-off call, no DI
- [Phase 4]: 500+ seed records with idempotent check (AnyAsync) and single transaction
- [Phase 4]: Hardcoded realistic arrays — no Bogus or external packages
- [Phase 4]: 2-stage Dockerfile (sdk:8.0 → aspnet:8.0), port 80, API project only
- [Phase 4]: docker-compose.yml with sqlserver + api, env var connection string (Server=sqlserver)
- [Phase 4]: Retry loop in Program.cs (5 attempts, exponential backoff) for DB readiness
- [Phase 4]: ASPNETCORE_ENVIRONMENT=Docker, conditional HTTPS redirect, Docker-aware Swagger

### Pending Todos

- Phase 5: Refactor Services to use Business models explicitly (Entity → Business → Response mapping)
- Phase 6: Fix the fail rule — add expand support to GetByIdAsync for complete related data
- Phase 7: Implement fields selection, multi-field sorting, and update sort parameter format for List APIs
- Phase 8: Add global exception handler to ensure 500 errors return consistent ApiResponse format

## Session Continuity

Last session: 2026-05-16T10:00:00Z
Stopped at: Completed 04-01 plan — Docker containerization + seed data
Resume file: None

### Blockers/Concerns

- Phase 5 concern: Business models exist but Services bypass them — map Entity → Response directly
- Phase 6 concern: GetByIdAsync returns only scalar properties — no navigation properties included, no ?expand support
