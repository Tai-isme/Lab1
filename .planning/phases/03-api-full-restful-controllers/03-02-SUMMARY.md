---
phase: 03-api-full-restful-controllers
plan: 02
subsystem: api
tags: [aspnet-core, repository, service-layer, paging, expand, iqueryable]

requires:
  - phase: 02-business-logic-services-layer
    provides: Service interfaces, DTOs, mapper classes, DI wiring
  - phase: 03-api-full-restful-controllers
    plan: 01
    provides: PagedQuery model, ApiResponse envelope, ResponseEnvelopeFilter

provides:
  - IRepository<T>.GetQueryable() for service-level IQueryable<T> composition
  - PagedResult<T> generic paginated result model
  - GetAllAsync(PagedQuery) overload on all 5 service interfaces
  - Expand navigation properties on CourseResponse and EnrollmentResponse
  - Expand-aware mapping methods (ToResponseDto overloads with string[] expand)
  - Full query pipeline implementations (filter → sort → include → count → page → project) in all 5 services

affects: [03-plan-03-controllers]

tech-stack:
  added: []
  patterns:
    - IQueryable<T> composition pattern: service calls _repository.GetQueryable(), chains Where/OrderBy/Skip/Take, then materializes
    - Expand pipeline: parse query.Expand → string[] → Entity Framework Include() → expand-aware mapper
    - Pagination: CountAsync before Skip/Take, Math.Clamp(PageSize, 1, 100) for safety, Math.Ceiling for TotalPages
    - SortBy whitelist: switch expression with unknown-field fallback to Id sort (T-03-06 mitigation)
    - Search: string.Contains with ToLower() — EF Core parameterizes, preventing SQL injection (T-03-07 mitigation)

key-files:
  created:
    - PRN232.LAB_1.Services/Models/PagedResult.cs
  modified:
    - PRN232.LAB_1.Repositories/Repositories/IRepository.cs
    - PRN232.LAB_1.Repositories/Repositories/Repository.cs
    - PRN232.LAB_1.Services/Interfaces/ISemesterService.cs
    - PRN232.LAB_1.Services/Interfaces/ICourseService.cs
    - PRN232.LAB_1.Services/Interfaces/ISubjectService.cs
    - PRN232.LAB_1.Services/Interfaces/IStudentService.cs
    - PRN232.LAB_1.Services/Interfaces/IEnrollmentService.cs
    - PRN232.LAB_1.Services/Models/CourseResponse.cs
    - PRN232.LAB_1.Services/Models/EnrollmentResponse.cs
    - PRN232.LAB_1.Services/Mappings/SemesterMapper.cs
    - PRN232.LAB_1.Services/Mappings/CourseMapper.cs
    - PRN232.LAB_1.Services/Mappings/SubjectMapper.cs
    - PRN232.LAB_1.Services/Mappings/StudentMapper.cs
    - PRN232.LAB_1.Services/Mappings/EnrollmentMapper.cs
    - PRN232.LAB_1.Services/Services/SemesterService.cs
    - PRN232.LAB_1.Services/Services/CourseService.cs
    - PRN232.LAB_1.Services/Services/SubjectService.cs
    - PRN232.LAB_1.Services/Services/StudentService.cs
    - PRN232.LAB_1.Services/Services/EnrollmentService.cs

key-decisions:
  - "GetQueryable() added to IRepository<T> for service-level query composition instead of pushing all query logic into repository methods"
  - "PagedResult<T> modeled as simple POCO with Items, Page, PageSize, TotalItems, TotalPages — no behavior, just data transfer"
  - "Expand uses EF Core Include() on IQueryable<T> before materialization — one SQL query per Include instead of N+1"
  - "ToLower().Contains() for search — case-insensitive matching at database level; sufficient for this lab's scale"

patterns-established:
  - "Service layer owns query composition via IQueryable<T> from repository: filter → sort → include → count → page → project"
  - "Expand-aware mappers accept string[] and call ToResponseDto() on each navigation property conditionally"
  - "PageSize clamped to [1, 100] as DoS mitigation (T-03-04); SortBy uses whitelist switch (T-03-06); Search uses parameterized EF queries (T-03-07)"

requirements-completed: [API-07, API-11, API-12, API-13, API-15, API-16]

duration: 5min
completed: 2026-05-14
---

# Phase 3 Plan 2: Service Extensions — Query Pipeline with Search, Sort, Paging, and Expand

**IQueryable<T> query pipeline in all 5 services: search, sort, paging, and expand via IRepository.GetQueryable(), PagedResult<T>, expand-aware mapping**

## Performance

- **Duration:** 6 min
- **Started:** 2026-05-14T11:15:02Z
- **Completed:** 2026-05-14T11:21:41Z
- **Tasks:** 3
- **Files modified:** 20

## Accomplishments

- Extended `IRepository<T>` with `IQueryable<T> GetQueryable()` and implemented in `Repository<T>` returning `_dbSet.AsNoTracking().AsQueryable()`
- Created `PagedResult<T>` model in Services/Models with Items, Page, PageSize, TotalItems, TotalPages properties for pagination metadata
- Added `Task<PagedResult<TResponse>> GetAllAsync(PagedQuery query)` overload to all 5 service interfaces (ISemesterService, ICourseService, ISubjectService, IStudentService, IEnrollmentService)
- Added nullable expand navigation properties: `SubjectResponse? Subject` and `SemesterResponse? Semester` to CourseResponse; `StudentResponse? Student` and `CourseResponse? Course` to EnrollmentResponse
- Extended all 5 mapper classes with `ToResponseDto(Entity, string[] expand)` overloads — CourseMapper and EnrollmentMapper conditionally map navigation properties based on expand flag; SemesterMapper, SubjectMapper, and StudentMapper have placeholder overloads (no forward navigation to expand)
- Implemented `GetAllAsync(PagedQuery)` in all 5 services with full query pipeline:
  - **Search**: field-specific string.Contains with ToLower() on searchable columns
  - **Sort**: whitelist switch expression with Id fallback and SortDesc support
  - **Expand**: EF Core Include() for Course (subject, semester) and Enrollment (student, course)
  - **Paging**: CountAsync for total, Math.Max for page safety, Math.Clamp(PageSize, 1, 100), Skip/Take
  - **Return**: PagedResult<TResponse> with Items, Page, PageSize, TotalItems, TotalPages

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend repository + create PagedResult + add interface overloads** - `4eb520c` (feat: repository, PagedResult, interface methods)
2. **Task 2: Add expand props to DTOs + expand-aware mapper overloads** - `cf4cc96` (feat: expand props and mapper overloads)
3. **Task 3: Implement GetAllAsync(PagedQuery) in all 5 services** - `efb7c9d` (feat: service implementations with query pipeline)

**Additional commits:**
- `cecc372` (docs: XML doc comments for PagedResult and CourseResponse)

## Files Created/Modified

### Created
- `PRN232.LAB_1.Services/Models/PagedResult.cs` — Generic paginated result model with Items, Page, PageSize, TotalItems, TotalPages

### Modified
- `PRN232.LAB_1.Repositories/Repositories/IRepository.cs` — Added `IQueryable<T> GetQueryable()` method
- `PRN232.LAB_1.Repositories/Repositories/Repository.cs` — Implemented `GetQueryable()` returning `_dbSet.AsNoTracking().AsQueryable()`
- `PRN232.LAB_1.Services/Interfaces/ISemesterService.cs` — Added `GetAllAsync(PagedQuery)` overload
- `PRN232.LAB_1.Services/Interfaces/ICourseService.cs` — Added `GetAllAsync(PagedQuery)` overload
- `PRN232.LAB_1.Services/Interfaces/ISubjectService.cs` — Added `GetAllAsync(PagedQuery)` overload
- `PRN232.LAB_1.Services/Interfaces/IStudentService.cs` — Added `GetAllAsync(PagedQuery)` overload
- `PRN232.LAB_1.Services/Interfaces/IEnrollmentService.cs` — Added `GetAllAsync(PagedQuery)` overload
- `PRN232.LAB_1.Services/Models/CourseResponse.cs` — Added `SubjectResponse? Subject` and `SemesterResponse? Semester`
- `PRN232.LAB_1.Services/Models/EnrollmentResponse.cs` — Added `StudentResponse? Student` and `CourseResponse? Course`
- `PRN232.LAB_1.Services/Mappings/SemesterMapper.cs` — Added `ToResponseDto(Semester, string[])` placeholder
- `PRN232.LAB_1.Services/Mappings/CourseMapper.cs` — Added `ToResponseDto(Course, string[])` with expand-aware subject/semester mapping + list overload
- `PRN232.LAB_1.Services/Mappings/SubjectMapper.cs` — Added `ToResponseDto(Subject, string[])` placeholder
- `PRN232.LAB_1.Services/Mappings/StudentMapper.cs` — Added `ToResponseDto(Student, string[])` placeholder
- `PRN232.LAB_1.Services/Mappings/EnrollmentMapper.cs` — Added `ToResponseDto(Enrollment, string[])` with expand-aware student/course mapping + list overload
- `PRN232.LAB_1.Services/Services/SemesterService.cs` — Implemented GetAllAsync(PagedQuery): search Code/Name, sort 4 fields, paging
- `PRN232.LAB_1.Services/Services/CourseService.cs` — Implemented GetAllAsync(PagedQuery): search 4 fields, sort 4 fields, expand subject/semester
- `PRN232.LAB_1.Services/Services/SubjectService.cs` — Implemented GetAllAsync(PagedQuery): search Code/Name/Description, sort 3 fields
- `PRN232.LAB_1.Services/Services/StudentService.cs` — Implemented GetAllAsync(PagedQuery): search 5 fields, sort 3 fields
- `PRN232.LAB_1.Services/Services/EnrollmentService.cs` — Implemented GetAllAsync(PagedQuery): search Status/Grade, sort 3 fields, expand student/course

## Decisions Made

- **GetQueryable() on IRepository<T>**: Chose to expose `IQueryable<T>` at the repository boundary rather than pushing query methods into the repository. This keeps the repository simple (CRUD + IQueryable access) and lets the service layer handle query composition (search, sort, paging, expand) where it belongs per the 3-layer architecture.
- **String.Contains for search**: Using `ToLower().Contains()` is not the most performant approach (no full-text search capabilities), but it's sufficient for this lab's scale and avoids adding a third-party search library.
- **Expand as Include before materialization**: Applies EF Core `Include()` calls on the IQueryable chain before `ToListAsync()`, producing a single SQL query with JOINs rather than N+1 separate queries.
- **Separate backward-compatible overloads**: The existing parameterless `GetAllAsync()` and `ToResponseDto(Entity)` stay unchanged. The new `GetAllAsync(PagedQuery)` and `ToResponseDto(Entity, string[])` are additive overloads — no breaking changes.

## Deviations from Plan

None - plan executed exactly as written.

**Note:** Build verification between Task 1 and Task 3 could not pass individually because adding the interface methods without service implementations causes compile errors. The plan's sequential task structure had this cross-task dependency, so build verification was validated cumulatively at plan completion.

## Issues Encountered

None

## Threat Flags

None — threat register mitigations are fully covered:
- T-03-04 (PageSize DoS): `Math.Clamp(PageSize, 1, 100)` applied in all 5 services ✅
- T-03-05 (Expand info disclosure): Only forward navigation properties, one level deep ✅
- T-03-06 (SortBy injection): Switch expression with whitelist, unknown fields fallback to Id ✅
- T-03-07 (Search injection): String.Contains with EF parameterized queries ✅

## Next Phase Readiness

- Repository, service interfaces, DTOs, mappers, and service implementations all extended with search, sort, paging, and expand support
- All 5 services now have the full `GetAllAsync(PagedQuery)` implementation ready for controller consumption
- Ready for Plan 03 (Controllers — creating the 5 CRUD RESTful controllers using this infrastructure)

---
*Phase: 03-api-full-restful-controllers*
*Completed: 2026-05-14*
