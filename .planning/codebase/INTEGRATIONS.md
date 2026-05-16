---
title: External Integrations
created: 2026-05-15
focus: tech
---

# External Integrations

## Databases

### SQL Server 2022
- **Purpose:** Primary data store for the LMS. Stores semesters, courses, subjects, students, and enrollments.
- **Connection:** Entity Framework Core 8.0.11 with `Microsoft.EntityFrameworkCore.SqlServer` provider.
- **Connection string location:**
  - `PRN232.LAB_1.API/appsettings.json` — `ConnectionStrings:DefaultConnection` (Development/localhost)
  - `docker-compose.yml` — `ConnectionStrings__DefaultConnection` environment variable (Docker container-to-container)
  - `PRN232.LAB_1.Repositories/Data/LmsDbContextFactory.cs` — Hardcoded design-time connection string for EF Core CLI migrations
- **Database name:** `PRN232_Lab1`
- **Auth:** SQL Server login (`sa` user with password `Lab1_Pass123`).
- **Schema:** Code-first with EF Core migrations. 5 entity tables seeded via `DataSeeder.cs`:
  - `Semesters` (5 rows)
  - `Subjects` (10 rows)
  - `Students` (50 rows)
  - `Courses` (20 rows)
  - `Enrollments` (variable, ~400–700 rows)
- **Connection method:** `DbContext` injected via DI into `Repository<T>`, used with `AsNoTracking()` for reads and `SaveChangesAsync()` for writes.

### Key Configuration Files
| File | Role |
|------|------|
| `PRN232.LAB_1.Repositories/Data/LmsDbContext.cs` | Core `DbContext` with `DbSet<>` properties for each entity and `OnModelCreating` |
| `PRN232.LAB_1.Repositories/Data/Configurations/` | Fluent API entity configurations (5 files: `SemesterConfiguration.cs`, `CourseConfiguration.cs`, etc.) |
| `PRN232.LAB_1.Repositories/Migrations/` | EF Core migrations (1 initial migration + model snapshot) |
| `PRN232.LAB_1.Repositories/Data/DataSeeder.cs` | Seed data generation with deterministic random (`seed: 42`) |

## External APIs

**None detected.** The application does not consume any external REST/SOAP/gRPC APIs. All data is self-contained within the LMS database.

## Auth Providers

**None detected.** The application has no authentication or authorization middleware configured. The pipeline in `Program.cs` calls `app.UseAuthorization()` but no authentication schemes, JWT bearer, or identity providers are registered. The API is fully open/anonymous.

## Webhooks

**None detected.** No webhook endpoints, outgoing webhook calls, or event-driven integrations to external services.

## Observability & Monitoring

**Logging:**
- **ASP.NET Core built-in `ILogger`** — Default console logging via `Microsoft.AspNetCore` and application-level categories.
- **Log levels:** `Information` default, `Warning` for `Microsoft.AspNetCore`.
- **No structured logging** (Serilog, NLog, etc.) — plain console output.

**Health checks:** Not configured.

## Environment Configuration

### Critical Environment Variables
| Variable | Where Set | Purpose |
|----------|-----------|---------|
| `ASPNETCORE_ENVIRONMENT` | `launchSettings.json` / docker-compose | Sets `Development` or `Docker` environment |
| `ConnectionStrings__DefaultConnection` | docker-compose.yml (line 23) | Overrides connection string in container |

### Application Environments
- **Development** (`launchSettings.json`) — Auto-migrates and seeds DB on startup, enables Swagger, enables HTTPS redirect.
- **Docker** (`docker-compose.yml`) — Auto-migrates and seeds DB, enables Swagger, disables HTTPS redirect (runs on port 80).
- **Production/other** — Only `app.UseAuthorization()` + `MapControllers()` + `app.Run()` (no migration, no Swagger).

## CI/CD

**None detected.** No CI pipeline configuration files (GitHub Actions, Azure DevOps, Jenkins, etc.) exist in the repository.

## Deployment

- **Containerized:** Docker Compose (`docker-compose.yml`) for local development and testing.
- **Single-instance:** API and database each run in one container. No load balancer, reverse proxy, or orchestration (Kubernetes) configuration.

---

*Integration audit: 2026-05-15*
