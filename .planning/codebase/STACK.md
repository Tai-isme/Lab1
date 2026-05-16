---
title: Technology Stack
created: 2026-05-15
focus: tech
---

# Technology Stack

## Languages

- **C# 12** — All application code (API controllers, service layer, repositories, entity models, mappings, migrations). Leverages .NET 8's implicit usings and nullable reference types.
- **SQL** — Migrations and seed data via Entity Framework Core (`PRN232.LAB_1.Repositories/Migrations/`); raw SQL not used directly in application code.

## Runtime

- **.NET 8.0** (SDK `8.0.x` / ASP.NET Core `8.0`) — Target framework for all three projects (`net8.0`).
  - API project uses `Microsoft.NET.Sdk.Web` (ASP.NET Core).
  - Services and Repositories use `Microsoft.NET.Sdk` (class libraries).

## Frameworks & Libraries

### Web Framework
- **ASP.NET Core 8.0** — REST API host with controllers (`[ApiController]` attribute), model binding, routing, middleware pipeline, and DI container.
- **Swashbuckle.AspNetCore 6.6.2** — Swagger/OpenAPI spec generation and Swagger UI. Configured in `Program.cs` (lines 24–39) with XML doc comments enabled.

### ORM / Data Access
- **Entity Framework Core 8.0.11** — ORM for SQL Server. Code-first approach with `DbContext` (`PRN232.LAB_1.Repositories/Data/LmsDbContext.cs`), fluent configuration classes, and EF Core migrations.
- **EF Core SqlServer Provider 8.0.11** — SQL Server database provider for EF Core.
- **EF Core Tools 8.0.11** — Design-time tools for migrations (`dotnet ef migrations`).
- **EF Core Design 8.0.11** — Design-time DbContext factory support (`LmsDbContextFactory.cs`).

### Dependency Injection
- **ASP.NET Core built-in DI** — All services and repositories registered via `IServiceCollection` extensions in `PRN232.LAB_1.Services/DependencyInjection.cs`.
- **Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2** — DI abstractions used by the Services project.

### Serialization
- **System.Text.Json** (built-in ASP.NET Core) — JSON serialization for API request/response bodies. Includes a custom `ConditionalJsonPropertyAttribute` in `PRN232.LAB_1.API/Attributes/`.

## Dependencies

### Production

| Package | Version | Purpose |
|---------|---------|---------|
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger/OpenAPI documentation and UI |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.11 | SQL Server EF Core provider |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.2 | DI abstractions for the Services layer |

### Development / Design-Time

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore.Design` | 8.0.11 | EF Core design-time tools (migrations) |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.11 | EF Core CLI tooling for migrations |

## Project Structure (Solutions)

### `Lab1.sln` — 3 projects:
| Project | Path | Type | Dependencies |
|---------|------|------|-------------|
| `PRN232.LAB_1.API` | `PRN232.LAB_1.API/` | ASP.NET Core Web API | → `PRN232.LAB_1.Services` |
| `PRN232.LAB_1.Services` | `PRN232.LAB_1.Services/` | Class Library | → `PRN232.LAB_1.Repositories` |
| `PRN232.LAB_1.Repositories` | `PRN232.LAB_1.Repositories/` | Class Library | None |

## Configuration

- **`PRN232.LAB_1.API/appsettings.json`** — Primary configuration: logging levels, connection strings (SQL Server `DefaultConnection`), allowed hosts.
- **`PRN232.LAB_1.API/appsettings.Development.json`** — Development overrides (logging level only).
- **`PRN232.LAB_1.API/appsettings.Docker.json`** *(referenced by docker-compose env)* — Docker-specific config. The `Docker` environment is set via `ASPNETCORE_ENVIRONMENT=Docker` in `docker-compose.yml`.
- **`PRN232.LAB_1.API/Properties/launchSettings.json`** — Dev launch profiles: `http` (port 5004), `https` (port 7242/5004), `IIS Express` (port 25412/44352). All launch Swagger UI on startup.
- **Connection string format:** `Server=localhost,1433;Database=PRN232_Lab1;User Id=sa;Password=...;TrustServerCertificate=True;`

## Build & Deploy

### Build
- **`dotnet build`** / `dotnet publish` — Standard .NET CLI. Release build configured in Dockerfile: `dotnet publish -c Release -o /app/publish`.
- XML documentation file generation enabled (`GenerateDocumentationFile=true`) with warning 1591 suppressed.
- **Solution:** `Lab1.sln` contains all 3 projects.

### Docker Deployment
- **Multi-stage Dockerfile** at `PRN232.LAB_1.API/Dockerfile`:
  - **Build stage:** `mcr.microsoft.com/dotnet/sdk:8.0` — Restores, then publishes.
  - **Runtime stage:** `mcr.microsoft.com/dotnet/aspnet:8.0` — Runs on port 80 with `dotnet PRN232.LAB_1.API.dll`.
- **docker-compose.yml** — Orchestrates two services:
  - `sqlserver` — `mcr.microsoft.com/mssql/server:2022-latest` on port 1433, named volume `lms-sql-data`.
  - `api` — Built from the Dockerfile, port 5000:80, depends on `sqlserver`, passes connection string via environment variable.
- **`.dockerignore`** — Excludes `bin/`, `obj/`, `.git/`, `.vs/`, `.planning/`, and env-specific appsettings.

### Languages (Code)
- C# 12: All project source code
- SQL: Embedded in EF Core migrations (`PRN232.LAB_1.Repositories/Migrations/`)

---

*Stack analysis: 2026-05-15*
