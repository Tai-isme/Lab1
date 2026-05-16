# Roadmap: PRN232 Lab 1 — LMS REST API

## Overview

A 3-layer ASP.NET Core 8 REST API for Learning Management System (LMS) academic management. This roadmap builds from the broken scaffold (3 standalone web projects with no layering) to a fully containerized REST API with CRUD, search, sort, paging, field selection, and expansion across 5 entities (Semester, Course, Subject, Student, Enrollment). The 4 phases follow a strict bottom-up dependency chain: fix the project structure → build data foundation → add business logic → expose via API → wrap in Docker.

## Phases

- [x] **Phase 1: Foundation** — Fix scaffold, build data layer with entities, DbContext, and repositories
- [x] **Phase 2: Business Logic** — Build service layer with domain models, DTOs, and model mapping
- [x] **Phase 3: API** — Implement full RESTful controllers with CRUD and collection features
- [ ] **Phase 4: Deployment** — Containerize with Docker Compose and seed production-quality data

## Phase Details

### Phase 1: Foundation — Scaffold Fix & Data Layer
**Goal**: Clean 3-layer project structure with correct SDK types, project references, and a complete data access layer (entities, DbContext, EF Core mappings, code-first migrations, repository pattern)
**Depends on**: Nothing (first phase)
**Requirements**: SCF-01, SCF-02, SCF-03, SCF-04, SCF-05, DAT-01, DAT-02, DAT-03, DAT-04, DAT-05, DAT-06
**Success Criteria** (what must be TRUE):
  1. Services and Repositories projects are class libraries (`Microsoft.NET.Sdk`) with API → Services → Repositories project references wired
  2. All WeatherForecast boilerplate, misplaced controllers, and standalone entry points are removed from Services/Repositories
  3. Entity models exist for Semester, Course, Subject, Student, Enrollment with EF Core Fluent API mappings, relationships, and cascade behavior configured
  4. DbContext registered with Scoped lifetime, DbSets for all entities, and model configurations applied
  5. Repository interfaces (`IRepository<T>`) and implementations exist per entity with full CRUD returning materialized collections; code-first migration generates correct database schema
**Plans**: 1 plan
- [ ] 01-PLAN.md — Create entities, Fluent API configs, DbContext, generic repository, DI wiring, and initial migration (DAT-01 through DAT-06)

### Phase 2: Business Logic — Services Layer
**Goal**: Complete service layer with business/domain models, request/response DTOs, service interfaces and implementations, and model mapping between all four model types
**Depends on**: Phase 1
**Requirements**: SVC-01, SVC-02, SVC-03, SVC-04, SVC-05, SVC-06
**Success Criteria** (what must be TRUE):
  1. Business/domain models exist for each entity for internal processing (no EF Core or JSON attributes)
  2. Request DTOs with data annotations exist for client input per entity
  3. Response DTOs exist per entity as flat shapes (no navigation properties) — entities never leak to API layer
  4. Service interfaces and implementations exist per entity with Entity → Response mapping and Request → Entity mapping logic
  5. Model mapping layer converts cleanly between entity ↔ business ↔ request ↔ response types (manual mapping, no AutoMapper)
**Plans**: 2 plans
- [x] 02-01-PLAN.md — Create Business models, Request/Response DTOs, mapper classes, and service interfaces (SVC-01, SVC-02, SVC-03, SVC-04, SVC-06)
- [x] 02-02-PLAN.md — Implement service classes, DI extension, and updated API wiring (SVC-05, D-04)

### Phase 3: API — Full RESTful Controllers
**Goal**: All 5 RESTful controllers with full CRUD, consistent response envelope, proper HTTP status codes, Swagger docs, and advanced collection features (search, sort, paging, field selection, expansion)
**Depends on**: Phase 2
**Requirements**: API-01, API-02, API-03, API-04, API-05, API-06, API-07, API-08, API-09, API-10, API-11, API-12, API-13, API-14, API-15, API-16, API-17
**Success Criteria** (what must be TRUE):
  1. All 5 entities have RESTful CRUD endpoints (GET collection, GET by ID, POST, PUT, DELETE) with plural resource-based URLs (`/api/semesters`, `/api/students`, etc.)
  2. GET by ID returns complete related data without circular references; returns HTTP 404 when resource not found
  3. All responses use consistent envelope `{success, message, data, errors}` with proper HTTP status codes (200, 201, 400, 404, 500)
  4. GET collection supports all query features: `?search=keyword`, `?sort=field,-field`, `?page=1&size=20`, `?fields=id,name,email`, `?expand=student,course`; pagination metadata returned as `{page, pageSize, totalItems, totalPages}`
  5. Swagger UI lists all endpoints with request/response schemas and HTTP status code documentation
**Plans**: 3 plans
- [x] 03-01-PLAN.md — API Infrastructure: ApiResponse, ResponseEnvelopeFilter, PagedQuery, Swagger XML docs (API-09, API-10, API-14, API-17)
- [x] 03-02-PLAN.md — Service Extensions: GetQueryable, PagedResult, service query overloads, expand support (API-07, API-11, API-12, API-13, API-15, API-16)
- [x] 03-03-PLAN.md — All 5 Controllers: RESTful CRUD with search, sort, paging, expand, field selection (API-01, API-02, API-03, API-04, API-05, API-06, API-08)

### Phase 4: Deployment — Docker & Seed Data
**Goal**: Fully containerized application running via Docker Compose (SQL Server + API) with 500+ realistic seed records using a separate DataSeeder class
**Depends on**: Phase 3
**Requirements**: DAT-07, DCK-01, DCK-02, DCK-03, DCK-04
**Success Criteria** (what must be TRUE):
  1. Multi-stage Dockerfile builds the API project efficiently (SDK build → runtime publish) and exposes port 80
  2. `docker-compose.yml` orchestrates SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`) and API services with env var connection string using service name (sqlserver) as hostname
  3. API container retries DB connection with exponential backoff (no Docker HEALTHCHECK — retry in Program.cs)
  4. Both API and database containers start successfully via `docker-compose up`; API responds on http://localhost:5000
  5. Application seeds 500+ records on first startup: 5+ semesters, 10+ subjects, 50+ students, 20+ courses, 500+ enrollments (via DataSeeder class with nested loops, single transaction, idempotent check)
**Plans**: 1 plan
- [ ] 04-01-PLAN.md — DataSeeder.cs, Program.cs (retry + seed + HTTPS guard), Dockerfile, docker-compose.yml, .dockerignore (DAT-07, DCK-01, DCK-02, DCK-03, DCK-04)

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 1/1 | Complete | 2026-05-14 |
| 2. Business Logic | 2/2 | Complete | 2026-05-14 |
| 3. API | 3/3 | Complete | 2026-05-14 |
| 4. Deployment | 0/1 | Planned | - |
| 5. Business Model Fix | 0/2 | Planned | - |

### Phase 5: Fix project chưa đúng theo yêu cầu Business models được sử dụng đúng mục đích

**Goal:** Refactor all services to use Business models as internal domain representation (Entity → Business → Response flow)
**Requirements**: SVC-05, SVC-06
**Depends on:** Phase 4
**Plans:** 2 plans

Plans:
- [ ] 05-01-PLAN.md — Add Business → Response mapping methods to all 5 entity mappers
- [ ] 05-02-PLAN.md — Refactor all 5 services to use Entity → Business → Response chain

### Phase 6: Fix the fail rule

**Goal:** GetByIdAsync returns complete related data with expand support across all 5 entities
**Requirements**: API-07
**Depends on:** Phase 5
**Plans:** 2 plans

Plans:
- [ ] 06-01-PLAN.md — Repository layer: expand-aware GetByIdAsync with EF Core Include() support (API-07)
- [ ] 06-02-PLAN.md — Services + Controllers: wire expand parameter through all 5 entities (API-07)

### Phase 7: Fix the missing/gaps/issues to match the requirement 5. GET Collection Resource (List API)

**Goal:** All 5 List APIs support multi-field sort (`?sort=field,-field`), field selection (`?fields=x,y,z`), and expand (`?expand=related`) in GET collection endpoints
**Requirements**: API-12, API-14, API-15
**Depends on:** Phase 6
**Plans:** 2/2 plans complete

Plans:
- [x] 07-01-PLAN.md — Replace SortBy+SortDesc with Sort in PagedQuery; create QueryableExtensions helper (ApplyMultiFieldSort, ApplyFieldSelection)
- [x] 07-02-PLAN.md — Refactor all 5 services to use shared query helpers, add field selection, add expand support where missing

### Phase 8: Error Handling & Response Consistency

**Goal:** Add global exception handling middleware and fix Delete endpoint responses for consistent ApiResponse envelope
**Requirements**: API-09, API-10
**Depends on:** Phase 7
**Plans:** 2 plans

Plans:
- [ ] 08-01-PLAN.md — ExceptionHandlingMiddleware + Program.cs wiring (API-09, API-10)
- [ ] 08-02-PLAN.md — Fix Delete responses + add 500 status code to all 25 actions (API-09, API-10)
