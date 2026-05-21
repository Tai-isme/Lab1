# Requirements: PRN232 Lab 1 — LMS REST API

**Defined:** 2026-05-14
**Core Value:** All LMS resources (semesters, courses, subjects, students, enrollments) can be CRUD-managed via RESTful endpoints with search, sort, paging, field selection, and expansion — meeting the lab evaluation checklist.

## v1 Requirements

### Scaffold Fix

- [x] **SCF-01**: Services project converted from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` (class library)
- [x] **SCF-02**: Repositories project converted from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk` (class library)
- [x] **SCF-03**: Project references added: API → Services → Repositories
- [x] **SCF-04**: WeatherForecast boilerplate removed from all projects
- [x] **SCF-05**: Controllers deleted from Services and Repositories projects (only API layer has controllers)

### Data Layer (Repositories)

- [x] **DAT-01**: Entity models defined for Semester, Course, Subject, Student, Enrollment with EF Core mappings
- [x] **DAT-02**: DbContext created with DbSets for all entities and model configurations
- [x] **DAT-03**: Entity relationships configured (foreign keys, navigation properties, cascade behavior)
- [x] **DAT-04**: Repository interfaces defined per entity (IRepository<T> pattern)
- [x] **DAT-05**: Repository implementations with full CRUD operations
- [x] **DAT-06**: Code-first migrations for database schema
- [x] **DAT-07**: Seed data — 5+ semesters, 50+ students, 10+ subjects, 20+ courses, 500+ enrollments (via DataSeeder)

### Business Layer (Services)

- [x] **SVC-01**: Business/domain models for internal processing
- [x] **SVC-02**: Request models for client input validation
- [x] **SVC-03**: Response models for API output (entity models never returned directly)
- [x] **SVC-04**: Service interfaces defined per entity
- [x] **SVC-05**: Service implementations with business logic (Entity → Response mapping, Request → Entity mapping)
- [x] **SVC-06**: Model mapping between entity, business, request, and response types

### API Layer (Controllers)

- [x] **API-01**: RESTful CRUD endpoints for Semester (GET, GET/{id}, POST, PUT, DELETE)
- [x] **API-02**: RESTful CRUD endpoints for Course
- [x] **API-03**: RESTful CRUD endpoints for Subject
- [x] **API-04**: RESTful CRUD endpoints for Student
- [x] **API-05**: RESTful CRUD endpoints for Enrollment
- [x] **API-06**: Resource-based URLs with plural nouns (e.g., /api/students, /api/enrollments)
- [x] **API-07**: GET by ID returns complete related data without circular references
- [x] **API-08**: HTTP 404 returned when resource not found
- [x] **API-09**: Consistent response envelope: {success, message, data, errors}
- [x] **API-10**: HTTP 200 on success, 201 on created, 400 on bad request, 404 on not found, 500 on server error
- [x] **API-11**: GET collection supports search (?search=keyword)
- [x] **API-12**: GET collection supports sorting (?sort=field,-field)
- [x] **API-13**: GET collection supports paging (?page=1&size=20)
- [x] **API-14**: GET collection supports field selection (?fields=id,name,email)
- [x] **API-15**: GET collection supports expansion (?expand=student,course)
- [x] **API-16**: Pagination metadata in responses: {page, pageSize, totalItems, totalPages}
- [x] **API-17**: Swagger/OpenAPI integration with endpoint listing, API testing, request/response docs, and HTTP status code documentation

### Docker Deployment

- [x] **DCK-01**: Multi-stage Dockerfile for API project
- [x] **DCK-02**: docker-compose.yml with SQL Server service + API service
- [x] **DCK-03**: Docker healthcheck on SQL Server container (API waits for DB)
- [x] **DCK-04**: Both API and database run successfully via Docker Compose

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| SCF-01 | Phase 1 | Complete |
| SCF-02 | Phase 1 | Complete |
| SCF-03 | Phase 1 | Complete |
| SCF-04 | Phase 1 | Complete |
| SCF-05 | Phase 1 | Complete |
| DAT-01 | Phase 1 | Complete |
| DAT-02 | Phase 1 | Complete |
| DAT-03 | Phase 1 | Complete |
| DAT-04 | Phase 1 | Complete |
| DAT-05 | Phase 1 | Complete |
| DAT-06 | Phase 1 | Complete |
| DAT-07 | Phase 4 | Complete |
| SVC-01 | Phase 2 | Complete |
| SVC-02 | Phase 2 | Complete |
| SVC-03 | Phase 2 | Complete |
| SVC-04 | Phase 2 | Complete |
| SVC-05 | Phase 2 | Complete |
| SVC-06 | Phase 2 | Complete |
| API-01 | Phase 3 | Complete |
| API-02 | Phase 3 | Complete |
| API-03 | Phase 3 | Complete |
| API-04 | Phase 3 | Complete |
| API-05 | Phase 3 | Complete |
| API-06 | Phase 3 | Complete |
| API-07 | Phase 6 | Complete |
| API-08 | Phase 3 | Complete |
| API-09 | Phase 8 | Complete |
| API-10 | Phase 8 | Complete |
| API-11 | Phase 3 | Complete |
| API-12 | Phase 7 | Complete |
| API-13 | Phase 3 | Complete |
| API-14 | Phase 7 | Complete |
| API-15 | Phase 7 | Complete |
| API-16 | Phase 3 | Complete |
| API-17 | Phase 3 | Complete |
| DCK-01 | Phase 4 | Complete |
| DCK-02 | Phase 4 | Complete |
| DCK-03 | Phase 4 | Complete |
| DCK-04 | Phase 4 | Complete |

---
*Last updated: 2026-05-16*
