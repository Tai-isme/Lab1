# Technology Stack

**Analysis Date:** 2026-05-14

## Languages

**Primary:**
- C# 12 (.NET 8.0) — All application code in three ASP.NET Core projects

**Secondary:**
- Not detected — No JavaScript, TypeScript, SQL, or other languages present

## Runtime

**Environment:**
- .NET 8.0 (`net8.0` target framework in all `.csproj` files)

**Package Manager:**
- NuGet (via `dotnet restore`)
- Lockfile: Not present (no `packages.lock.json`)

## Frameworks

**Core:**
- ASP.NET Core 8.0 — Web API framework used by all 3 projects
  - Entry pattern: Minimal-style `WebApplication.CreateBuilder(args)` in `Program.cs`

**Testing:**
- Not detected — No test projects, no xUnit/NUnit/MSTest references

**Build/Dev:**
- `dotnet build` / `dotnet run` — Standard .NET CLI
- IIS Express — Configured in `launchSettings.json` for local development

## Key Dependencies

**Critical:**
- `Swashbuckle.AspNetCore` v6.6.2 — Swagger/OpenAPI UI for API documentation (present in all 3 projects)
- `Microsoft.NET.Sdk.Web` — ASP.NET Core SDK used by all 3 projects

**Infrastructure:**
- `Microsoft.AspNetCore.Mvc` (implicit via SDK) — Controller and API attribute support
- `Microsoft.Extensions.Logging` (implicit via SDK) — Logging abstractions

## Configuration

**Environment:**
- `appsettings.json` — Base configuration per project (Logging, AllowedHosts)
- `appsettings.Development.json` — Development overrides per project (same Logging settings)
- `ASPNETCORE_ENVIRONMENT` environment variable — Set to `Development` in launch profiles

**Build:**
- `Lab1.sln` — Solution file (Visual Studio 2022 v17.14)
- No `Directory.Build.props` or `Directory.Build.targets` detected
- No `.editorconfig` detected

## Platform Requirements

**Development:**
- .NET 8.0 SDK
- Visual Studio 2022 (v17.14) or compatible IDE
- IIS Express (optional, for IIS Express profile)

**Production:**
- Not defined — No deployment configuration detected

---

*Stack analysis: 2026-05-14*
