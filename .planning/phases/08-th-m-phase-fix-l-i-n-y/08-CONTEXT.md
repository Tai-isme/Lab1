# Phase 08: Error Handling & Response Consistency - Context

**Gathered:** 2026-05-16
**Status:** Ready for planning

<domain>
## Phase Boundary

Add global exception handling middleware to ensure all unhandled exceptions return the consistent `{success, message, data, errors}` ApiResponse envelope. Fix Delete endpoint responses to use explicit ApiResponse<object> instead of anonymous objects. Document 500 status code in Swagger for all endpoints. This phase does NOT add new endpoints, change business logic, or add authentication/validation.

</domain>

<decisions>
## Implementation Decisions

### Exception Handling Mechanism
- **D-01:** Use custom exception handling middleware (`app.UseExceptionHandler`) placed early in the pipeline, before Swagger and controllers. Catches all exceptions including those from middleware, model binding, and controllers. Returns `ApiResponse<object>` with 500 status code.

### Error Detail Exposure
- **D-02:** Dev/Prod split — In Development environment, include exception type, message, and stack trace in the `errors` field. In Production (and Docker), return only `"An internal server error occurred"` with no stack trace. Use `IWebHostEnvironment.IsDevelopment()` to branch.

### Delete Response Consistency
- **D-03:** All 5 Delete endpoints return `Ok(ApiResponse<object>.Ok(new { message = "Deleted successfully" }))` explicitly instead of anonymous objects. This ensures the ResponseEnvelopeFilter skips re-wrapping (already detects ApiResponse<T>) and response types are consistent across all endpoints.

### Swagger 500 Documentation
- **D-04:** Add `[ProducesResponseType(StatusCodes.Status500InternalServerError)]` attribute to every controller action (all 5 entities × 5 actions = 25 endpoints). Shows 500 response schema in Swagger UI matching the ApiResponse envelope format.

### the agent's Discretion
- Middleware class naming and file organization within the API project
- Exact stack trace formatting (single string vs structured fields)
- Whether to log exceptions to console or use ILogger

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Exception Handling
- `.planning/REQUIREMENTS.md` — API-09 (consistent response envelope), API-10 (proper HTTP status codes)
- `PRN232.LAB_1.API/Program.cs` — Current middleware pipeline, where UseExceptionHandler must be inserted
- `PRN232.LAB_1.API/Filters/ResponseEnvelopeFilter.cs` — Existing envelope wrapping logic that exception middleware must complement (not duplicate)

### Response Format
- `PRN232.LAB_1.API/Models/ApiResponse.cs` — ApiResponse<T> class with Ok/Created/Fail factory methods
- `PRN232.LAB_1.API/Controllers/*.cs` — All 5 controllers that need Delete endpoint fixes and 500 status code attributes

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ApiResponse<T>` — Already has `Fail(string, Dictionary<string, string[]>)` factory method that can be reused for error responses
- `ResponseEnvelopeFilter` — Already handles 400/404 wrapping; exception middleware should produce the same shape so filter can skip re-wrapping
- `IWebHostEnvironment` — Already injected via `builder.Environment` in Program.cs, can be used for dev/prod branching

### Established Patterns
- Manual model mapping (no AutoMapper) — Phase 2 decision
- ApiResponse envelope with `{success, message, data, errors}` — Phase 3 decision
- ResponseEnvelopeFilter as IResultFilter — Phase 3, already wraps ObjectResult responses
- Controllers return IActionResult, not direct ApiResponse — filter handles wrapping

### Integration Points
- Exception middleware must be placed BEFORE `app.UseSwagger()` and `app.MapControllers()` in Program.cs middleware pipeline
- Exception middleware response must match ApiResponse<T> shape so ResponseEnvelopeFilter recognizes it and skips double-wrapping
- Delete endpoints in all 5 controllers need return value changes

</code_context>

<specifics>
## Specific Ideas

- Exception middleware should log the exception (Console.WriteLine or ILogger) before returning the response
- Stack trace in dev mode should be useful for debugging — include `ex.ToString()` or at least `ex.Message` + `ex.StackTrace`
- The middleware should handle the case where the response has already started (can't write body) — check `context.Response.HasStarted`

</specifics>

<deferred>
## Deferred Ideas

- Global exception handler was previously marked as "Out of Scope" in PROJECT.md — this phase brings it back in scope for response consistency only
- None — analysis stayed within phase scope

</deferred>

---

*Phase: 08-them-phase-fix-loi-nay*
*Context gathered: 2026-05-16*
