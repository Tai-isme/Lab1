# Project: PRN232 Lab 1 — LMS REST API

**v1.0 Complete**

A 3-layer ASP.NET Core 8 REST API for Learning Management System (LMS) academic management. Supports semesters, courses, subjects, students, and enrollments with full CRUD, search, sort, paging, field selection, and related-data expansion.

## Core Value

All LMS resources (semesters, courses, subjects, students, enrollments) can be CRUD-managed via RESTful endpoints with search, sort, paging, field selection, and expansion — meeting the lab evaluation checklist.

## Requirements

### Validated

- ✓ **SCF-01 to 05**: 3-layer architecture with clear separation — API → Services → Repositories
- ✓ **DATA-01 to 04**: Entity, Business, Request, and Response models implemented
- ✓ **DATA-05**: Seed data — 500+ records generated and seeded on startup
- ✓ **API-01 to 06**: RESTful resource-based endpoints for all 5 entities
- ✓ **API-02**: GET resource by ID with complete related data and expansion support
- ✓ **API-03**: GET collection with search, multi-field sort, paging, field selection, and expansion
- ✓ **API-04 to 06**: Consistent response format and proper HTTP status codes
- ✓ **DOCKER-01 to 03**: Fully containerized with Docker Compose (API + SQL Server)
- ✓ **SWAGGER-01**: Swagger UI with full documentation and testing capabilities
- ✓ **BONUS**: Global exception handling for consistent 500 Internal Server Error responses

### Active

None — all requirements are complete.

### Out of Scope

- Authentication / Authorization — explicitly excluded by lab spec
- JWT Security — explicitly excluded by lab spec
- Advanced Validation — explicitly excluded by lab spec
- Unit Testing / Integration Testing — explicitly excluded by lab spec

## Context

- **Course**: PRN232 — REST API Design
- **Final State**: 100% complete, containerized, and documented.
- **Docker**: Both DB and API run via Docker Compose on port 5000.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| 3-Layer Architecture | Separation of concerns (API/Services/Repositories) | ✓ Success |
| Manual Model Mapping | Full control over model transformations without AutoMapper | ✓ Success |
| Generic Repository | Reduced boilerplate for common CRUD operations | ✓ Success |
| Queryable Extensions | Centralized logic for sort/fields/expand across all entities | ✓ Success |
| Global Exception Middleware | Consistent error response format for unexpected failures | ✓ Success |

---
*Last updated: 2026-05-16*
