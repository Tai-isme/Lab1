# Phase 6: Fix the fail rule - Context

**Gathered:** 2026-05-15
**Status:** Implemented — verified in codebase
**Source:** Derived from STATE.md + codebase analysis

<domain>
## Phase Boundary

Fix `GetByIdAsync` across all 5 entities to return complete related data (navigation properties included) via `?expand=` query parameter. The lab evaluation checklist requires "GET by ID returns complete related data without circular references" (API-07).

### Implementation Verification

All decisions from this CONTEXT.md are confirmed implemented in the codebase:

| Layer | Entity | File | Line |
|-------|--------|------|------|
| Repository | Generic | `Repository.cs` | 27-38 — `GetByIdAsync(id, includes)` with Include/ThenInclude |
| Service | Semester | `SemesterService.cs` | 85-92 — expand parsing → repository includes |
| Service | Course | `CourseService.cs` | 99 — expand-aware GetByIdAsync |
| Service | Subject | `SubjectService.cs` | 85 — expand-aware GetByIdAsync |
| Service | Student | `StudentService.cs` | 93 — expand-aware GetByIdAsync |
| Service | Enrollment | `EnrollmentService.cs` | 96 — expand-aware GetByIdAsync |
| Controller | Semester | `SemesterController.cs` | 55 — `GetById(id, [FromQuery] string? expand)` |
| Controller | Course | `CourseController.cs` | 50 — `GetById(id, [FromQuery] string? expand)` |
| Controller | Subject | `SubjectController.cs` | 50 — `GetById(id, [FromQuery] string? expand)` |
| Controller | Student | `StudentController.cs` | 50 — `GetById(id, [FromQuery] string? expand)` |
| Controller | Enrollment | `EnrollmentController.cs` | 50 — `GetById(id, [FromQuery] string? expand)` |

</domain>

<decisions>
## Implementation Decisions

### Repository Layer
- Add `GetByIdAsync(int id, string[]? includes = null)` to `IRepository<T>` — optional includes parameter for navigation property names
- Implement in `Repository<T>` using `Include()`/`ThenInclude()` when includes are specified, fall back to `FindAsync` when not
- Method signature: `Task<T?> GetByIdAsync(int id, string[]? includes = null)`

### Service Layer
- Add `GetByIdAsync(int id, string? expand = null)` overload to all 5 service interfaces
- Implement expand parsing (comma-separated string → string array) in all 5 service implementations
- Pass includes to repository's `GetByIdAsync`

### API Layer
- Update all 5 controllers' `GetById` endpoint to accept optional `?expand=` query parameter
- Pass expand parameter to service's `GetByIdAsync`

### the agent's Discretion
- Expand parameter format: comma-separated navigation property names (e.g., `?expand=courses`, `?expand=semester,subject`)
- EF Core `Include()` uses string-based navigation property names
- Circular references already handled by `[JsonIgnore]` on collection navigation properties in entities
- No changes to Response DTOs needed — they already have flat shapes; expand loads data into entity, mapping still works

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Repository Layer
- `PRN232.LAB_1.Repositories/Repositories/IRepository.cs` — Interface to extend
- `PRN232.LAB_1.Repositories/Repositories/Repository.cs` — Implementation to extend

### Service Layer
- `PRN232.LAB_1.Services/Interfaces/ISemesterService.cs` — Service interface pattern
- `PRN232.LAB_1.Services/Interfaces/ICourseService.cs`
- `PRN232.LAB_1.Services/Interfaces/ISubjectService.cs`
- `PRN232.LAB_1.Services/Interfaces/IStudentService.cs`
- `PRN232.LAB_1.Services/Interfaces/IEnrollmentService.cs`
- `PRN232.LAB_1.Services/Services/SemesterService.cs` — Service implementation pattern
- `PRN232.LAB_1.Services/Services/CourseService.cs`
- `PRN232.LAB_1.Services/Services/SubjectService.cs`
- `PRN232.LAB_1.Services/Services/StudentService.cs`
- `PRN232.LAB_1.Services/Services/EnrollmentService.cs`

### API Layer
- `PRN232.LAB_1.API/Controllers/SemesterController.cs` — Controller pattern
- `PRN232.LAB_1.API/Controllers/CourseController.cs`
- `PRN232.LAB_1.API/Controllers/SubjectController.cs`
- `PRN232.LAB_1.API/Controllers/StudentController.cs`
- `PRN232.LAB_1.API/Controllers/EnrollmentController.cs`

### Entity Navigation Properties
- `PRN232.LAB_1.Repositories/Entities/Semester.cs` — nav: Courses
- `PRN232.LAB_1.Repositories/Entities/Course.cs` — nav: Semester, Subject, Enrollments
- `PRN232.LAB_1.Repositories/Entities/Subject.cs` — nav: Courses
- `PRN232.LAB_1.Repositories/Entities/Student.cs` — nav: Enrollments
- `PRN232.LAB_1.Repositories/Entities/Enrollment.cs` — nav: Student, Course

</canonical_refs>

<specifics>
## Specific Requirements

- `GetByIdAsync` must support loading navigation properties via EF Core `Include()`
- Expand parameter format: `?expand=property1,property2`
- When no expand specified, behavior unchanged (backward compatible)
- All 5 entities must support this: Semester, Course, Subject, Student, Enrollment
- Entity navigation properties already have `[JsonIgnore]` on collections to prevent circular references
- No changes to Response DTOs — they are flat shapes; expand affects entity loading, not response shape

</specifics>

<deferred>
## Deferred Ideas

None — phase scope is focused on GetById expand support only.

</deferred>

---

*Phase: 06-fix-the-fail-rule*
*Context gathered: 2026-05-15 via codebase analysis*
