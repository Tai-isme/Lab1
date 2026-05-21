# Phase 11: Add Docker AppSettings - Context

**Gathered:** 2026-05-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Create `appsettings.Docker.json` for the Docker environment configuration and clean up the redundant env var override in docker-compose.yml. This is a configuration cleanup task — no functional changes to the API, services, or repositories.

</domain>

<decisions>
## Implementation Decisions

### Connection string strategy
- **D-01:** Create `appsettings.Docker.json` in the API project — ASP.NET Core auto-loads it when `ASPNETCORE_ENVIRONMENT=Docker`. Config lives in the right file, not as an env var override.

### File content scope
- **D-02:** `appsettings.Docker.json` contains only the connection string — minimal and focused. No logging overrides or other config. File structure mirrors `appsettings.json` with Docker-specific connection string (`Server=sqlserver,1433`).

### Cleanup
- **D-03:** Remove `ConnectionStrings__DefaultConnection` env var from `docker-compose.yml` — `appsettings.Docker.json` is the single source of truth. Keep `ASPNETCORE_ENVIRONMENT=Docker` env var (this triggers the file load).

### the agent's Discretion
- Exact connection string value (should use `Server=sqlserver,1433` to match docker-compose service name)
- File formatting and JSON structure details

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

- `PRN232.LAB_1.API/appsettings.json` — Base config with connection string template
- `docker-compose.yml` — Current env var override to be removed, service names, port mappings
- `.planning/ROADMAP.md` — Phase 11 goal and requirements

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `appsettings.json` — existing config file with connection string format to mirror
- `appsettings.Development.json` — example of environment-specific config file structure

### Established Patterns
- ASP.NET Core 8 environment-based config loading (`appsettings.{Environment}.json`)
- `ASPNETCORE_ENVIRONMENT=Docker` already set in docker-compose.yml — triggers auto-load of `appsettings.Docker.json`

### Integration Points
- `docker-compose.yml` api service — remove `ConnectionStrings__DefaultConnection` env var
- `PRN232.LAB_1.API/` project directory — add new `appsettings.Docker.json` file

</code_context>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 11-add-docker-appsettings*
*Context gathered: 2026-05-21*
