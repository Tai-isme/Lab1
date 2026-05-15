# Requirements: PRN232 Lab 1 — LMS REST API

**Defined:** 2026-05-14
**Core Value:** All LMS resources (semesters, courses, subjects, students, enrollments) can be CRUD-managed via RESTful endpoints with search, sort, paging, field selection, and expansion — meeting the lab evaluation checklist.

## v1 Requirements

### Scaffold Fix

- [ ] **SCF-01**: Services project converted from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` (class library)
- [ ] **SCF-02**: Repositories project converted from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` (class library)
- [ ] **SCF-03**: Project references added: API → Services → Repositories
- [ ] **SCF-04**: WeatherForecast boilerplate removed from all projects
- [ ] **SCF-05**: Controllers deleted from Services and Repositories projects (only API layer has controllers)

### Data Layer (Repositories)

- [ ] **DAT-01**: Entity models defined for Semester, Course, Subject, Student, Enrollment with EF Core mappings
- [ ] **DAT-02**: DbContext created with DbSets for all entities and model configurations
- [ ] **DAT-03**: Entity relationships configured (foreign keys, navigation properties, cascade behavior)
- [ ] **DAT-04**: Repository interfaces defined per entity (IRepository<T> pattern)
- [ ] **DAT-05**: Repository implementations with full CRUD operations
- [ ] **DAT-06**: Code-first migrations for database schema
- [ ] **DAT-07**: Seed data — 5+ semesters, 50+ students, 10+ subjects, 20+ courses, 500+ enrollments (via UseSeeding)

### Business Layer (Services)

- [x] **SVC-01**: Business/domain models for internal processing
- [x] **SVC-02**: Request models for client input validation
- [x] **SVC-03**: Response models for API output (entity models never returned directly)
- [x] **SVC-04**: Service interfaces defined per entity
- [ ] **SVC-05**: Service implementations with business logic (Entity → Response mapping, Request → Entity mapping)
- [x] **SVC-06**: Model mapping between entity, business, request, and response types

### API Layer (Controllers)

- [ ] **API-01**: RESTful CRUD endpoints for Semester (GET, GET/{id}, POST, PUT, DELETE)
- [ ] **API-02**: RESTful CRUD endpoints for Course
- [ ] **API-03**: RESTful CRUD endpoints for Subject
- [ ] **API-04**: RESTful CRUD endpoints for Student
- [ ] **API-05**: RESTful CRUD endpoints for Enrollment
- [ ] **API-06**: Resource-based URLs with plural nouns (e.g., /api/students, /api/enrollments)
- [ ] **API-07**: GET by ID returns complete related data without circular references
- [ ] **API-08**: HTTP 404 returned when resource not found
- [ ] **API-09**: Consistent response envelope: {success, message, data, errors}
- [ ] **API-10**: HTTP 200 on success, 201 on created, 400 on bad request, 404 on not found, 500 on server error
- [ ] **API-11**: GET collection supports search (?search=keyword)
- [x] **API-12**: GET collection supports sorting (?sort=field,-field)
- [ ] **API-13**: GET collection supports paging (?page=1&size=20)
- [x] **API-14**: GET collection supports field selection (?fields=id,name,email)
- [x] **API-15**: GET collection supports expansion (?expand=student,course)
- [ ] **API-16**: Pagination metadata in responses: {page, pageSize, totalItems, totalPages}
- [ ] **API-17**: Swagger/OpenAPI integration with endpoint listing, API testing, request/response docs, and HTTP status code documentation

### Docker Deployment

- [ ] **DCK-01**: Multi-stage Dockerfile for API project
- [ ] **DCK-02**: docker-compose.yml with SQL Server service + API service
- [ ] **DCK-03**: Docker healthcheck on SQL Server container (API waits for DB)
- [ ] **DCK-04**: Both API and database run successfully via Docker Compose

## v2 Requirements

None — all requirements are v1 for this lab assignment.

## Out of Scope

| Feature | Reason |
|---------|--------|
| Authentication / Authorization | Explicitly excluded by lab spec |
| JWT Security | Explicitly excluded by lab spec |
| Advanced Validation | Explicitly excluded by lab spec |
| Global Exception Handling | Explicitly excluded by lab spec |
| Unit Testing / Integration Testing | Explicitly excluded by lab spec |
| Real-time features (WebSockets) | Not part of lab requirements |
| File upload (avatars, documents) | Not part of lab requirements |
| Role-based access control | Not applicable (no auth) |
| Rate limiting | Not required for lab |
| API versioning | Single-version API sufficient for lab |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| SCF-01 | Phase 1 | Pending |
| SCF-02 | Phase 1 | Pending |
| SCF-03 | Phase 1 | Pending |
| SCF-04 | Phase 1 | Pending |
| SCF-05 | Phase 1 | Pending |
| DAT-01 | Phase 1 | Pending |
| DAT-02 | Phase 1 | Pending |
| DAT-03 | Phase 1 | Pending |
| DAT-04 | Phase 1 | Pending |
| DAT-05 | Phase 1 | Pending |
| DAT-06 | Phase 1 | Pending |
| DAT-07 | Phase 4 | Pending |
| SVC-01 | Phase 2 | Pending |
| SVC-02 | Phase 2 | Pending |
| SVC-03 | Phase 2 | Pending |
| SVC-04 | Phase 2 | Pending |
| SVC-05 | Phase 2 | Pending |
| SVC-06 | Phase 2 | Pending |
| API-01 | Phase 3 | Pending |
| API-02 | Phase 3 | Pending |
| API-03 | Phase 3 | Pending |
| API-04 | Phase 3 | Pending |
| API-05 | Phase 3 | Pending |
| API-06 | Phase 3 | Pending |
| API-07 | Phase 3 | Pending |
| API-08 | Phase 3 | Pending |
| API-09 | Phase 3 | Pending |
| API-10 | Phase 3 | Pending |
| API-11 | Phase 3 | Pending |
| API-12 | Phase 3 | Complete |
| API-13 | Phase 3 | Pending |
| API-14 | Phase 3 | Complete |
| API-15 | Phase 3 | Complete |
| API-16 | Phase 3 | Pending |
| API-17 | Phase 3 | Pending |
| DCK-01 | Phase 4 | Pending |
| DCK-02 | Phase 4 | Pending |
| DCK-03 | Phase 4 | Pending |
| DCK-04 | Phase 4 | Pending |

**Coverage:**
- v1 requirements: 39 total
- Mapped to phases: 39
- Unmapped: 0 ✅

---
*Requirements defined: 2026-05-14*
*Last updated: 2026-05-14 after initial definition*
