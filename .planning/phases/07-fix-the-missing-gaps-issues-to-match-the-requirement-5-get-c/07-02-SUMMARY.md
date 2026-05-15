---
phase: 07
plan: 02
subsystem: services
tags: [list-api, multi-field-sort, field-selection, expand]
dependency_graph:
  requires: [07-01]
  provides: [GetAllAsync-multi-field-sort, GetAllAsync-field-selection, GetAllAsync-expand]
  affects: []
tech_stack:
  added: [reflection-based ThenBy in QueryableExtensions]
  patterns: [pre-built sort functions, EF Core Include, field selection via reflection]
key_files:
  created: []
  modified:
    - PRN232.LAB_1.Services/Helpers/QueryableExtensions.cs
    - PRN232.LAB_1.Services/Services/SemesterService.cs
    - PRN232.LAB_1.Services/Services/SubjectService.cs
    - PRN232.LAB_1.Services/Services/CourseService.cs
    - PRN232.LAB_1.Services/Services/StudentService.cs
    - PRN232.LAB_1.Services/Services/EnrollmentService.cs
decisions:
  - Changed sort dictionary from LambdaExpression tuples to pre-built (asc, desc) functions — LambdaExpression cannot be used with LINQ generic methods due to type inference
  - Used reflection for ThenBy/ThenByDescending to extract TKey at runtime from the asc function's lambda expression
  - Applied field selection after ToResponseDtoList() and before ToList() — operates on materialized data
metrics:
  duration: ~5 min
  completed_date: "2026-05-15T16:20:00Z"
---

# Phase 07 Plan 02: Refactor All 5 Service GetAllAsync Methods Summary

**One-liner:** Refactored all 5 service GetAllAsync methods to use shared ApplyMultiFieldSort and ApplyFieldSelection helpers, adding expand support where missing.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Refactor SemesterService + SubjectService | `fb4cd1c` | SemesterService.cs, SubjectService.cs, QueryableExtensions.cs |
| 2 | Refactor CourseService + StudentService + EnrollmentService | `792f5df` | CourseService.cs, StudentService.cs, EnrollmentService.cs |

## What Was Built

**All 5 services** now use a consistent GetAllAsync(PagedQuery query) pattern:
1. **Search** — unchanged from before
2. **Expand** — EF Core Include() applied BEFORE ordering (CourseService: subject/semester, StudentService: enrollments, EnrollmentService: student/course, SemesterService/SubjectService: stub comments)
3. **Sort** — ApplyMultiFieldSort with entity-specific sort function dictionaries replacing inline switch statements
4. **Count + Page** — unchanged
5. **Map + Field Selection** — ApplyFieldSelection applied after ToResponseDtoList() to null out non-requested properties

**QueryableExtensions.cs** — Fixed from plan's LambdaExpression approach to use pre-built sort functions `(Func<IQueryable<T>, IOrderedQueryable<T>> asc, Func<IQueryable<T>, IOrderedQueryable<T>> desc)`. Uses reflection for ThenBy/ThenByDescending to handle generic TKey at runtime.

## Deviations from Plan

### [Rule 1 - Bug] Fixed LambdaExpression type inference in QueryableExtensions
- **Found during:** Plan 07-02 Task 1 build verification
- **Issue:** Plan specified `Dictionary<string, (LambdaExpression, bool)>` for sort fields, but LINQ's OrderBy/OrderByDescending/ThenBy/ThenByDescending require `Expression<Func<T, TKey>>` where TKey is inferred at compile time. LambdaExpression (non-generic base type) cannot provide this.
- **Fix:** Changed to `Dictionary<string, (Func<IQueryable<T>, IOrderedQueryable<T>> asc, Func<IQueryable<T>, IOrderedQueryable<T>> desc)>` — pre-built sort functions that work with any property type. Used reflection for ThenBy/ThenByDescending to extract TKey from the lambda at runtime.
- **Files modified:** QueryableExtensions.cs, all 5 service files (sort dictionary format)

## Known Stubs

None — all functionality is wired and operational.

## Self-Check

- [x] All 5 service files do NOT contain query.SortBy or query.SortDesc
- [x] All 5 services contain ApplyMultiFieldSort(query.Sort, sortFunctions)
- [x] All 5 services contain .ApplyFieldSelection(query.Fields) after ToResponseDtoList()
- [x] All 5 services have using PRN232.LAB_1.Services.Helpers
- [x] CourseService has expand handling for "subject" and "semester" BEFORE sort
- [x] StudentService has expand handling for "enrollments" BEFORE sort
- [x] EnrollmentService retains expand handling for "student" and "course" BEFORE sort
- [x] SemesterService, SubjectService have expand stub comments
- [x] dotnet build succeeds with 0 errors, 0 warnings
- [x] Commits: fb4cd1c, 792f5df

## Self-Check: PASSED
