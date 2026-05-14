# PRN232 Lab 1 — LMS REST API

## What This Is

An ASP.NET Core 8.0 RESTful API for a Learning Management System (LMS) using a 3-layer architecture. Built as a lab assignment for PRN232 course. The API manages semesters, courses, subjects, students, and enrollments with full CRUD, search, sort, paging, field selection, and related-data expansion.

## Core Value

All LMS resources (semesters, courses, subjects, students, enrollments) can be CRUD-managed via RESTful endpoints with search, sort, paging, field selection, and expansion — meeting the lab evaluation checklist.

## Requirements

### Validated

- ✓ ASP.NET Core 8.0 Web API project structure — existing scaffold
- ✓ Three-project solution (API, Services, Repositories) — existing scaffold
- ✓ Swagger/OpenAPI with Swashbuckle 6.6.2 — existing

### Active

- [ ] **ARCH-01**: 3-layer architecture with clear separation — API → Services → Repositories, no business logic in controllers or repositories
- [ ] **DATA-01**: Entity models for Semester, Course, Subject, Student, Enrollment mapped to database
- [ ] **DATA-02**: Business models for domain processing
- [ ] **DATA-03**: Request models for client input
- [ ] **DATA-04**: Response models for API output (entity models never returned directly)
- [ ] **DATA-05**: Seed data — 5+ semesters, 50+ students, 10+ subjects, 20+ courses, 500+ enrollments
- [ ] **API-01**: RESTful resource-based endpoints (plural nouns, no verbs in URLs)
- [ ] **API-02**: GET resource by ID with complete related data, 404 if not found
- [ ] **API-03**: GET collection with search, sort, paging, field selection, and expansion
- [ ] **API-04**: Pagination metadata in responses
- [ ] **API-05**: Consistent response format (success, message, data, errors)
- [ ] **API-06**: Proper HTTP status codes (200, 201, 400, 404, 500)
- [ ] **DOCKER-01**: Database runs in Docker container
- [ ] **DOCKER-02**: API runs in Docker container
- [ ] **DOCKER-03**: Dockerfile and docker-compose.yml configured
- [ ] **SWAGGER-01**: Swagger UI with endpoint listing, API testing, request/response docs, status code docs

### Out of Scope

- Authentication / Authorization — explicitly excluded by lab spec
- JWT Security — explicitly excluded by lab spec
- Advanced Validation — explicitly excluded by lab spec
- Global Exception Handling — explicitly excluded by lab spec
- Unit Testing / Integration Testing — explicitly excluded by lab spec

## Context

- **Course**: PRN232 — REST API Design
- **Existing code**: Default ASP.NET Core 8.0 Web API scaffold in 3 projects (API, Services, Repositories) with no actual layering, no database, and only WeatherForecast boilerplate
- **Existing issues**: Repositories and Services projects incorrectly contain Controllers; no project references between layers; no interfaces or DI wiring
- **Docker**: Docker Desktop ready for use
- **Database**: SQL Server or compatible RDBMS via Docker
- **Deadline**: Due this week

## Constraints

- **Tech stack**: ASP.NET Core 8.0, C# 12
- **Architecture**: Strict 3-layer (API → Services → Repositories)
- **Models**: 4 model types required (Entity, Business, Request, Response)
- **Naming convention**: `PRN232.[ProjectName].{API|Services|Repositories}`
- **Docker**: Both DB and API must run via Docker Compose
- **No auth**: Authentication, JWT, advanced validation, global exception handling, and unit tests are out of scope

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Convert Repositories and Services to class libraries | They should not be standalone web apps — only API layer exposes endpoints | — Pending |
| Add project references (API → Services → Repositories) | Required for proper 3-layer dependency chain | — Pending |
| Use Entity Framework Core for data access | Standard ORM for ASP.NET Core, fits repository pattern | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-05-14 after initialization*
