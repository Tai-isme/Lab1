---
phase: 07
plan: 01
subsystem: services
tags: [query-helpers, multi-field-sort, field-selection]
dependency_graph:
  requires: []
  provides: [ApplyMultiFieldSort, ApplyFieldSelection, Sort-parameter]
  affects: [07-02]
tech_stack:
  added: [QueryableExtensions helper class]
  patterns: [extension methods, reflection-based field selection, IOrderedQueryable chaining]
key_files:
  created:
    - PRN232.LAB_1.Services/Helpers/QueryableExtensions.cs
  modified:
    - PRN232.LAB_1.Services/Models/PagedQuery.cs
decisions:
  - Used Dictionary<string, (LambdaExpression, bool)> for sort field mappings — type-safe, validated at compile time
  - ApplyFieldSelection operates on IEnumerable (post-materialization) — avoids EF Core translation issues
  - Id property always preserved in field selection — never nulled out
metrics:
  duration: ~2 min
  completed_date: "2026-05-15T16:14:00Z"
---

# Phase 07 Plan 01: Replace PagedQuery Sort and Create Query Helpers Summary

**One-liner:** Replaced SortBy+SortDesc with single Sort parameter and created shared QueryableExtensions helpers for multi-field sort and field selection.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Replace SortBy+SortDesc with Sort in PagedQuery | `3b91826` | PagedQuery.cs |
| 2 | Create QueryableExtensions with ApplyMultiFieldSort and ApplyFieldSelection | `53f8b9b` | QueryableExtensions.cs |

## What Was Built

**PagedQuery.cs** — Replaced two properties (`SortBy` string, `SortDesc` bool) with single `Sort` string? parameter. Enables comma-separated multi-field sort format (`?sort=name,-id`). All other properties (Search, Page, PageSize, Fields, Expand) unchanged.

**QueryableExtensions.cs** — New helper class in `PRN232.LAB_1.Services.Helpers` namespace with two extension methods:
1. `ApplyMultiFieldSort<T>` — Accepts IQueryable, sort string, Dictionary of known field mappings, and default order key. Uses OrderBy/OrderByDescending for first field, ThenBy/ThenByDescending for subsequent. Falls back to Id ascending if no valid fields.
2. `ApplyFieldSelection<T>` — Accepts IEnumerable (post-materialized), fields string. Nulls out properties NOT in requested list via reflection. Always preserves Id. Handles value types (default) and reference types (null).

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check

- [x] PagedQuery.cs exists with `public string? Sort { get; set; }`
- [x] PagedQuery.cs does NOT contain SortBy or SortDesc
- [x] QueryableExtensions.cs exists in Helpers/ folder
- [x] Contains ApplyMultiFieldSort method with correct signature
- [x] Contains ApplyFieldSelection method with correct signature
- [x] Namespace is PRN232.LAB_1.Services.Helpers
- [x] Commits: 3b91826, 53f8b9b

## Self-Check: PASSED
