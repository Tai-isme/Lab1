# Roadmap: PRN232 Lab 1 — LMS REST API

## Overview

A 3-layer ASP.NET Core 8 REST API for Learning Management System (LMS) academic management. This roadmap builds from the broken scaffold (3 standalone web projects with no layering) to a fully containerized REST API with CRUD, search, sort, paging, field selection, and expansion across 5 entities (Semester, Course, Subject, Student, Enrollment). The 4 phases follow a strict bottom-up dependency chain: fix the project structure → build data foundation → add business logic → expose via API → wrap in Docker.

## Phases

- [x] **Phase 1: Foundation** — Fix scaffold, build data layer with entities, DbContext, and repositories
- [ ] **Phase 2: Business Logic** — Build service layer with domain models, DTOs, and model mapping
- [ ] **Phase 3: API** — Implement full RESTful controllers with CRUD and collection features
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
- [ ] 02-02-PLAN.md — Implement service classes, DI extension, and updated API wiring (SVC-05, D-04)

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
**Plans**: TBD

### Phase 4: Deployment — Docker & Seed Data
**Goal**: Fully containerized application running via Docker Compose (SQL Server + API) with 500+ realistic seed records using batched `UseSeeding()`
**Depends on**: Phase 3
**Requirements**: DAT-07, DCK-01, DCK-02, DCK-03, DCK-04
**Success Criteria** (what must be TRUE):
  1. Multi-stage Dockerfile builds the API project efficiently (SDK build → runtime publish)
  2. `docker-compose.yml` orchestrates SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`) and API services with correct networking (service name as hostname)
  3. SQL Server healthcheck ensures API container waits for DB readiness before starting
  4. Both API and database containers start successfully via `docker-compose up`; API responds to requests on configured port
  5. Application starts with 500+ seed records: 5+ semesters, 50+ students, 10+ subjects, 20+ courses, 500+ enrollments (via `UseSeeding()` with batched inserts)
**Plans**: TBD

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 1/1 | Complete | 2026-05-14 |
| 2. Business Logic | 1/2 | In Progress | - |
| 3. API | 0/0 | Not started | - |
| 4. Deployment | 0/0 | Not started | - |
