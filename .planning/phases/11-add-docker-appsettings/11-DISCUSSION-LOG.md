# Phase 11: Add Docker AppSettings - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions captured in CONTEXT.md — this log preserves the analysis.

**Date:** 2026-05-21T08:58:00Z
**Phase:** 11-add-docker-appsettings
**Mode:** discuss
**Areas analyzed:** Connection string strategy, File content scope, Cleanup

## Areas Discussed

### Connection string strategy
- **Options:** A) appsettings.Docker.json (recommended), B) Keep env var only, C) Both
- **User selection:** A — Create `appsettings.Docker.json` with Docker connection string
- **Rationale:** ASP.NET Core auto-loads when `ASPNETCORE_ENVIRONMENT=Docker`. Config lives in the right file, easier to maintain.

### File content scope
- **Options:** A) Connection string only (recommended), B) Connection string + logging, C) Full config mirror
- **User selection:** A — Minimal file with only the connection string
- **Rationale:** Keep file focused. Logging config stays in appsettings.json or appsettings.Production.json.

### Cleanup
- **Options:** A) Remove env var (recommended), B) Keep env var, C) Comment out env var
- **User selection:** A — Remove `ConnectionStrings__DefaultConnection` from docker-compose.yml
- **Rationale:** Single source of truth. Avoid conflicts and debugging confusion.

## Deferred Ideas

None — discussion stayed within phase scope.

---

*Phase: 11-add-docker-appsettings*
*Discussion logged: 2026-05-21*
