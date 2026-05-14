# Feature Landscape: LMS REST API for Academic Management

**Domain:** Learning Management System — Academic Management (Semesters, Courses, Subjects, Students, Enrollments)
**Researched:** 2026-05-14
**Source APIs consulted:** Canvas LMS, edX/Open edX, Ed-Fi, IMS Edu-API, Oracle Student Management, Moodle, LifterLMS
**Confidence:** HIGH (cross-referenced 6+ production LMS APIs + academic data standards)

---

## Table Stakes

Features that users (front-end consumers, grading dashboards, SIS integrations) expect. Missing any of these and the API feels broken or amateur.

| # | Feature | Why Expected | Complexity | Notes |
|---|---------|-------------|------------|-------|
| 1 | **Full CRUD for all 5 entities** — Semester, Course, Subject, Student, Enrollment | Every LMS API (Canvas, Ed-Fi, Moodle, Oracle SIS) exposes create/read/update/delete for core entities. Missing CRUD = not a usable API. | Low | POST + GET/{id} + PUT/PATCH + DELETE per resource |
| 2 | **Consistent JSON response envelope** — `{ success, data, message, errors }` | Industry standard (Canvas, Ed-Fi, and every major ASP.NET guide use an `ApiResponse<T>` wrapper). Clients need a single parse path. | Low | `ApiResponse<T>` envelope with `JsonIgnore(Condition.WhenWritingNull)` for compact payloads |
| 3 | **Collection GET with paging** — `?page=1&size=10` with pagination metadata | Without paging, a DB of 500+ enrollments returns an unusable blob. All major LMS APIs (Canvas `per_page`, Ed-Fi `limit/offset`) require it. | Low | Return `page`, `size`, `total`, `totalPages` in response metadata |
| 4 | **Sort on collections** — `?sort=LastName desc` or `?sortBy=LastName&sortDir=desc` | Users expect to sort students by name, enrollments by date. Canvas, Ed-Fi all support `$orderby`. | Low | Map to LINQ `OrderBy` / `OrderByDescending` |
| 5 | **Search/filter on collections** — `?search=text`, `?status=active` | Filtering students by name, courses by semester, enrollments by status. Canvas `enrollment_state`, Ed-Fi `filter` equivalents. | Low-Medium | Text search on string fields; exact match on foreign keys/enums |
| 6 | **GET by ID with full expansion** — `GET /students/5` returns complete student with nested related data | Consumers fetch one entity to see everything about it. Canvas, Ed-Fi all return full objects on singular GET. | Low | Include navigation properties inline |
| 7 | **404 on missing resource** — `GET /students/9999` → 404 | Non-negotiable REST convention. Indicates absence vs. error. | Low | Standard ASP.NET Core `NotFound()` |
| 8 | **Proper HTTP status codes** — 200 OK, 201 Created, 204 No Content, 400 Bad Request, 404 Not Found, 409 Conflict, 500 Internal Server Error | REST fundamental. Each status communicates a distinct semantic. | Low | 201 for POST creates, 204 for successful DELETE, 400 for validation errors, 409 for duplicate enrollment |
| 9 | **Swagger/OpenAPI docs** — interactive endpoint listing, request/response schemas, status codes | Developers discover and test the API without reading source. Swashbuckle is already in the project. | Low | Already scaffolded — needs proper XML comments + `[ProducesResponseType]` attributes |
| 10 | **Resource-based URL pattern** — `api/semesters`, `api/courses`, `api/students/{id}/enrollments` | REST convention: plural nouns, no verbs in URLs. Canvas `/api/v1/courses`, Ed-Fi `/students`, `/courses`. | Low | Must avoid `/api/GetStudents` or `/api/DeleteCourse` anti-patterns |
| 11 | **Validation error details** — which fields failed and why | 400 responses must tell the client *which* field is wrong. Canvas and Ed-Fi both return field-level validation arrays. | Low | ASP.NET `[ApiController]` model validation + custom validation logic |
| 12 | **POST returns 201 Created with Location header** | Standard REST creation response. Client needs the new resource's URL. | Low | `CreatedAtAction(nameof(GetById), new { id }, createdResource)` |
| 13 | **DELETE returns 204 No Content** | Standard REST deletion response. No body needed. | Low | `return NoContent()` |
| 14 | **Empty collection returns 200 with `[]`** — not 404 | An empty result is a valid response, not a missing resource. | Low | Return `ApiResponse<List<T>>.Ok(new List<T>())` |
| 15 | **PUT/PATCH for full/partial updates** — support both | Some consumers want full replacement (PUT), others want partial (PATCH). Canvas supports both. | Low-Medium | PUT → replace full resource; PATCH → update only provided fields |

---

## Differentiators

Features that add genuine value for this lab's scope. Not strictly required by the rubric, but they demonstrate architectural maturity and make the API substantially more useful.

| # | Feature | Value Proposition | Complexity | Notes |
|---|---------|-------------------|------------|-------|
| 1 | **Field selection** — `?fields=id,firstName,lastName` | Lets clients request only the properties they need. Slashes payload size. Canvas `?include[]`, Ed-Fi `fields` parameter. | Medium | Implement via `IDataShaper<T>` pattern — select properties from expression tree |
| 2 | **Related data expansion** — `?expand=enrollments` or `?include=courses` | Clients fetch parent + children in one round trip instead of N+1. Canvas `$expand`, Ed-Fi nested resources. | Medium | Use `IQueryable.Include()` on navigation properties based on expand param. Must support: `Student → Enrollments`, `Course → Subject`, `Enrollment → Student + Course` |
| 3 | **Bulk enrollment** — `POST api/enrollments/bulk` to enroll multiple students in one course in one call | Directly addresses the "500+ enrollments" seed data requirement. Open edX `bulk_enroll`, Canvas bulk enrollment API both support this. | Medium | Accept `{ courseId, studentIds: [...] }`. Validate all students exist before any insert to avoid partial failures. |
| 4 | **Semester context filtering** — `GET /courses?semesterId=5` and `GET /enrollments?semesterId=5` | Academic management is inherently semester-scoped. The most common query is "what's happening this semester." Ed-Fi, Oracle SIS all have semester/term filters. | Low | Add `semesterId` as a standard filter param on all endpoints that have a semester relationship |
| 5 | **Enrollment status management** — `status` field (Active, Dropped, Completed, Withdrawn) with filtering | Real academic systems track enrollment lifecycle. Canvas uses `enrollment_state`. Separates current enrollments from history. | Low-Medium | Add `status` enum to Enrollment entity. Filter with `?status=active`. Status changes via PATCH. |
| 6 | **Enrollment count aggregation** — `GET /courses/5/stats` or `?include=enrollmentCount` | Dashboards need "enrolled students" counts. Avoids clients having to count paginated results. | Low | Return count from DB query — cheap operation on indexed FK columns |
| 7 | **Pagination metadata in envelope** — `meta: { page, size, total, totalPages }` | Clients need to build UIs with "Page 2 of 17." Standard pagination info. Canvas returns `per_page`, `total`. | Low | Include in response `meta` object alongside `data` |
| 8 | **Semester-based validation on enrollment** — reject enrollment if course semester ≠ student's current semester | Prevents data inconsistency. A real academic system would enforce this. | Medium | Service-layer validation: check course.SemesterId matches expectation |
| 9 | **Search across multiple fields** — `?search=john` matches firstName, lastName, email simultaneously | Better UX than single-field search. Students may be searched by name, ID, or email. | Low-Medium | Build dynamic OR predicate across searchable string fields |
| 10 | **Soft delete** — entities have `IsDeleted` flag, deleted items excluded by default, `?includeDeleted=true` to see them | Prevents accidental data loss and maintains referential integrity (enrollments still point to valid students). | Medium | Add `IsDeleted` + `DeletedAt` to entities. Global query filter `.IgnoreQueryFilters()` for includeDeleted queries. |

---

## Anti-Features

Features to explicitly NOT build for *this lab project* — they add scope without earning marks.

| Anti-Feature | Why Avoid | What to Do Instead |
|-------------|-----------|-------------------|
| **Authentication / Authorization** | Explicitly excluded by lab spec (PROJECT.md line 40). Auth adds complexity to every endpoint, every test, every error path — for zero grading value. | Skip entirely. Document as "future enhancement" in docs. |
| **JWT tokens / OAuth2 / API Keys** | Explicitly excluded by lab spec (line 41). Would require middleware, token validation, user management. | Not needed. |
| **Global exception handling middleware** | Explicitly excluded by lab spec (line 43). Would require `ExceptionFilterAttribute` or middleware. | Let ASP.NET Core `[ApiController]` default behavior handle it. Return 500 with minimal info. |
| **Unit tests / Integration tests** | Explicitly excluded by lab spec (line 44). Tests are valuable but outside scope. | Not needed. |
| **Webhooks / event-driven notifications** | No consumer to receive them. LMS webhooks require external subscribers. Open edX, Canvas both support webhooks — but only useful with integration consumers. | Document as future enhancement. |
| **File upload (course materials, assignments)** | Not in the 5-entity scope. Requires blob storage, multipart handling, file type validation. | Only needed if a course material entity is added later. |
| **Role-based access control (admin vs. student vs. teacher)** | Out of scope. Would require user entity, roles, authorization policies on every endpoint. Canvas `TeacherEnrollment` vs `StudentEnrollment` is a different entity. | Our `Enrollment` has a simple student→course link. No roles needed. |
| **Grade tracking / scoring / assessment** | Not in the 5-entity scope. Requires grades entity, calculation logic, letter grade mapping. | Add as a separate feature if scope expands. |
| **Caching layer (Redis, in-memory)** | Premature optimization for seed data (500 enrollments). Adds deployment complexity. | Keep simple. Add caching only if performance measurements show need. |
| **LTI (Learning Tools Interoperability) compliance** | LTI is for embedding external tools in an LMS — irrelevant for a backend API. IMS Edu-API is a different standard. | Not applicable. |
| **SCORM / xAPI runtime support** | These are e-learning content standards, not REST API management features. | Not applicable. |
| **Rate limiting** | No public consumers hitting the API. Adds middleware complexity for no benefit. | Add only if deploying to production with untrusted clients. |
| **API versioning (`/v1/` prefix)** | Single lab assignment, single version. Versioning adds routing complexity. Canvas doesn't version in URL (uses `Accept` header). | Omit version prefix. Add if breaking changes are ever needed. |
| **HATEOAS / hypermedia links** | No consumer benefits from link discovery. Canvas doesn't use HATEOAS — it returns flat JSON. | Standard REST envelope is sufficient. |
| **ETags / conditional requests** | No concurrent edit conflicts expected in a lab dataset. | Skip. |
| **Export endpoints (CSV, PDF)** | Not in scope. Requires file generation libraries. | Only if explicitly requested by evaluator. |

---

## Feature Dependencies

### Hard Dependencies (B must exist before A can work)

| Feature | Depends On | Rationale |
|---------|-----------|-----------|
| POST `/api/enrollments` | Student + Course + Semester entities must exist | Enrollment references three foreign keys |
| GET `/api/students/{id}/enrollments` | Enrollment entity exists | Nested route requires enrollment data |
| Bulk enrollment | Individual enrollment creation works | Same logic, just iterated |
| Semester context filtering | Semester entity + relationships wired | Filter parameter is meaningless without semester FK |
| Soft delete | All CRUD operations | Must not break existing GET/PUT behavior |
| Field selection | All GET collection endpoints | Applied as a post-processing step after query |
| Related data expansion | All GET by-id and GET collection endpoints | EF Core `.Include()` needs navigation properties configured |

### Soft Dependencies (B is easier with A, but not strictly blocked)

| Feature | Better With | Rationale |
|---------|-------------|-----------|
| Search across multiple fields | Single-field search first | Start simple, then generalize |
| Enrollment status management | Basic Enrollment CRUD | Status is just a field until you add lifecycle logic |
| Enrollment count aggregation | Pagination metadata pattern | Use same `meta` object approach for both |

---

## MVP Recommendation (Phase Ordering)

Based on the lab rubric and dependency analysis, prioritize in this order:

### Phase 1: Foundation (must have for any demo)
1. CRUD for **Semester** — simplest entity, no FKs
2. CRUD for **Subject** — second simplest, no FKs
3. CRUD for **Student** — single FK to... well, none in this model
4. CRUD for **Course** — FK to Semester + Subject
5. CRUD for **Enrollment** — FKs to Student + Course
6. Consistent response envelope across all endpoints
7. Proper HTTP status codes + validation errors

### Phase 2: Collection Power (makes it usable)
8. Paging on all GET-collection endpoints
9. Sort on all GET-collection endpoints
10. Search/filter on collection endpoints
11. Pagination metadata in responses

### Phase 3: Differentiators (makes it impressive)
12. Field selection (`?fields=`)
13. Related data expansion (`?expand=`)
14. Bulk enrollment endpoint
15. Enrollment status management
16. Semester context filtering
17. Enrollment count aggregation

### Phase 4: Polish (belt-and-suspenders)
18. Soft delete with `?includeDeleted=true`
19. Search across multiple fields
20. Course enrollment stats endpoint

**Deferred (not building):** All anti-features listed above — auth, tests, caching, file upload, webhooks, roles, grades, export.

---

## Sources

| Source | Type | Confidence | What It Informed |
|--------|------|-----------|-----------------|
| Canvas LMS REST API docs (Instructure) | Production API docs | HIGH | Enrollment model, expansion patterns, status lifecycle |
| Open edX LMS API docs | Production API docs | HIGH | Bulk enrollment endpoints, course management |
| Ed-Fi API Design Guidelines v2.0 | Industry standard | HIGH | Resource hierarchy, field selection, pagination conventions |
| IMS Edu-API v1.0 spec | Industry standard | HIGH | Course offering / enrollment models, academic session handling |
| Oracle Student Management REST API | Production API docs | MEDIUM | Course selection, academic program structures |
| Moodle Web Services docs | Production API docs | MEDIUM | Error handling patterns, CRUD patterns |
| MS Learn: ASP.NET Core OData 8 docs | Official Microsoft | HIGH | `$select`, `$expand`, `$filter`, `$orderby` implementation patterns |
| MS Learn: EF Core sort/filter/page tutorial | Official Microsoft | HIGH | Paging + search implementation patterns |
| Zuplo: Common REST API pitfalls | Industry blog | MEDIUM | Anti-patterns: 200 with errors, verbs in URLs |
| Edlink: LMS integration pitfalls | Industry blog | MEDIUM | Anti-patterns: email as user identifier, partial failure handling |
| PROJECT.md (lab spec) | Primary source | HIGH | Out-of-scope items, entity list, naming conventions |
