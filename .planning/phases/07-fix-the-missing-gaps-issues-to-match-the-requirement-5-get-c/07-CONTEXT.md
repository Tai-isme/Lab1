# Phase 7: Fix the missing/gaps/issues to match the requirement 5. GET Collection Resource (List API) - Context

**Gathered:** 2026-05-15
**Status:** Ready for planning
**Source:** Codebase gap analysis

<domain>
## Phase Boundary

Fix three gaps in GET Collection (List API) endpoints across all 5 entities:
1. **Multi-field sort** — Replace `SortBy`/`SortDesc` with `Sort` parameter (`?sort=field,-field`)
2. **Field selection** — Implement `?fields=id,name,email` to return only requested properties
3. **Expand in GetAllAsync** — Add EF Core Include() support to SemesterService, StudentService, SubjectService (already in CourseService + EnrollmentService)

Search and paging are already working correctly across all services.
</domain>

<decisions>
## Implementation Decisions

### Sort Parameter Format (D-01)
- Replace `SortBy` (string) + `SortDesc` (bool) with single `Sort` (string?) in PagedQuery
- Format: comma-separated field names, `-` prefix for descending (e.g., `?sort=name,-id`)
- Parsing: split by `,`, trim, check `-` prefix per field
- Sorting: use `IOrderedQueryable` chaining — first field uses `OrderBy`/`OrderByDescending`, subsequent fields use `ThenBy`/`ThenByDescending`
- Default sort: `Id` ascending when no sort parameter or unrecognized field
- Unknown field names: silently fall back to `Id` ascending (no error)

### Field Selection Implementation (D-02)
- Apply field selection AFTER mapping Entity → Business → Response DTO (in-memory)
- Use reflection to null out properties NOT in the requested fields list
- Always preserve `Id` field (never null it out)
- Property name matching: case-insensitive comparison against Response DTO property names
- If `fields` parameter is empty or null: return all fields (no filtering)
- Field selection operates on top-level scalar properties only (not navigation properties from expand)

### Expand in GetAllAsync (D-03)
- Apply EF Core `Include()` BEFORE ordering and paging in the query pipeline
- Follow pattern already established in CourseService (lines 40-47) and EnrollmentService (lines 38-45)
- SemesterService: no forward navigation to expand — add expand handling stub (accepts parameter, no includes needed)
- StudentService: add `Include(e => e.Enrollments)` when expand contains "enrollments"
- SubjectService: no forward navigation to expand — add expand handling stub (accepts parameter, no includes needed)

### Shared Query Helper (D-04)
- Create `PRN232.LAB_1.Services/Helpers/QueryableExtensions.cs` with static extension methods
- `ApplySort<T>(IQueryable<T>, string? sort)` — multi-field sort with IOrderedQueryable chaining
- `ApplyFieldSelection<T>(IEnumerable<T>, string? fields)` — post-mapping field filtering via reflection
- All 5 services use these helpers instead of inline switch statements

### the agent's Discretion
- Exact property names for field selection matching (use case-insensitive comparison)
- Whether to log unrecognized sort fields (no — silently fall back to default)
- Response DTO property nulling approach (use reflection, set non-Id properties to default)
</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Structure
- `.planning/ROADMAP.md` — Phase goal and dependency chain
- `.planning/REQUIREMENTS.md` — API-12 (sort), API-14 (field selection), API-15 (expand)
- `.planning/STATE.md` — Project decisions and pending todos

### Existing Code Patterns (MUST follow)
- `PRN232.LAB_1.Services/Models/PagedQuery.cs` — Current query parameter model (will be modified)
- `PRN232.LAB_1.Services/Services/EnrollmentService.cs` — Reference pattern for expand in GetAllAsync (lines 38-45, 75-81)
- `PRN232.LAB_1.Services/Services/CourseService.cs` — Reference pattern for expand in GetAllAsync (lines 40-47, 78-84)
- `PRN232.LAB_1.Services/Mappings/*.cs` — Mapper patterns (Entity → Business → Response)
- `PRN232.LAB_1.Repositories/Repositories/Repository.cs` — GetQueryable() implementation
- `PRN232.LAB_1.Repositories/Entities/*.cs` — Entity models with navigation properties
</canonical_refs>

<specifics>
## Specific Implementation Details

### PagedQuery Changes
**Before:**
```csharp
public string? SortBy { get; set; }
public bool SortDesc { get; set; }
```
**After:**
```csharp
public string? Sort { get; set; }
```

### Multi-field Sort Examples
- `?sort=name` → OrderBy(e => e.Name)
- `?sort=-name` → OrderByDescending(e => e.Name)
- `?sort=name,id` → OrderBy(e => e.Name).ThenBy(e => e.Id)
- `?sort=name,-id` → OrderBy(e => e.Name).ThenByDescending(e => e.Id)
- `?sort=-name,id` → OrderByDescending(e => e.Name).ThenBy(e => e.Id)

### Field Selection Example
Request: `GET /api/students?fields=id,fullname,email`
Response: Each student object has only `Id`, `FullName`, `Email` populated; `Code`, `Phone`, `DateOfBirth`, `Address` are null/default.

### Expand Support Matrix for GetAllAsync
| Service | Expandable Properties | Already Implemented? |
|---------|----------------------|---------------------|
| SemesterService | (none — backward nav only) | No (add stub) |
| CourseService | Subject, Semester | Yes |
| SubjectService | (none — backward nav only) | No (add stub) |
| StudentService | Enrollments | No |
| EnrollmentService | Student, Course | Yes |

### Response DTO Properties (for field selection)
- **SemesterResponse**: Id, Code, Name, StartDate, EndDate, IsActive
- **CourseResponse**: Id, Code, Instructor, Room, Schedule, MaxStudents, SubjectId, SemesterId, Subject (nav), Semester (nav)
- **SubjectResponse**: Id, Code, Name, Credits, Description
- **StudentResponse**: Id, Code, FullName, Email, Phone, DateOfBirth, Address
- **EnrollmentResponse**: Id, StudentId, CourseId, EnrollmentDate, Status, Grade, Student (nav), Course (nav)
</specifics>

<deferred>
## Deferred Ideas

None — phase scope is fully defined by the three gaps identified.
</deferred>

---

*Phase: 07-fix-the-missing-gaps-issues-to-match-the-requirement-5-get-c*
*Context gathered: 2026-05-15 via codebase gap analysis*
