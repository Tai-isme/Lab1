# Codebase Structure

**Analysis Date:** 2026-05-14

## Directory Layout

```
Lab1/
├── Lab1.sln                              # Visual Studio solution file (empty — no projects listed)
├── PRN232.LAB_1.API/                     # Intended presentation/API layer (standalone Web API)
│   ├── Program.cs                        # ASP.NET Core bootstrapping & middleware
│   ├── WeatherForecast.cs                # Model class
│   ├── Controllers/
│   │   └── WeatherForecastController.cs  # GET /weatherforecast endpoint
│   ├── Properties/
│   │   ├── launchSettings.json           # Dev profiles (port 5004/7242)
│   ├── appsettings.json                  # Logging config
│   ├── appsettings.Development.json      # Dev overrides
│   └── PRN232.LAB_1.API.csproj           # SDk: Microsoft.NET.Sdk.Web, net8.0
│
├── PRN232.LAB_1.Services/                # Intended business logic layer (standalone Web API)
│   ├── Program.cs                        # ASP.NET Core bootstrapping & middleware
│   ├── WeatherForecast.cs                # Model class (duplicate)
│   ├── Controllers/
│   │   └── WeatherForecastController.cs  # GET /weatherforecast endpoint (duplicate)
│   ├── Properties/
│   │   ├── launchSettings.json           # Dev profiles (port 5096/7106)
│   ├── appsettings.json                  # Logging config
│   ├── appsettings.Development.json      # Dev overrides
│   └── PRN232.LAB_1.Services.csproj      # Sdk: Microsoft.NET.Sdk.Web, net8.0
│
├── PRN232.LAB_1.Repositories/            # Intended data access layer (standalone Web API)
│   ├── Program.cs                        # ASP.NET Core bootstrapping & middleware
│   ├── WeatherForecast.cs                # Model class (duplicate)
│   ├── Controllers/
│   │   └── WeatherForecastController.cs  # GET /weatherforecast endpoint (duplicate)
│   ├── Properties/
│   │   ├── launchSettings.json           # Dev profiles (port 5187/7055)
│   ├── appsettings.json                  # Logging config
│   ├── appsettings.Development.json      # Dev overrides
│   └── PRN232.LAB_1.Repositories.csproj  # Sdk: Microsoft.NET.Sdk.Web, net8.0
│
├── .planning/
│   └── codebase/                         # Codebase analysis documents
└── .vs/                                  # Visual Studio local settings (git-ignored)
```

## Directory Purposes

**`PRN232.LAB_1.API/`:**
- Purpose: Intended as the presentation/API entry point for HTTP requests
- Contains: `Program.cs`, model class, controller, config files
- Key files: `Program.cs` (app bootstrap), `Controllers/WeatherForecastController.cs` (API endpoint)

**`PRN232.LAB_1.Services/`:**
- Purpose: Intended as the business logic / service layer
- Contains: `Program.cs`, model class, controller, config files (All identical to API project)
- Key files: `Program.cs` (app bootstrap), `Controllers/WeatherForecastController.cs` (API endpoint)

**`PRN232.LAB_1.Repositories/`:**
- Purpose: Intended as the data access / repository layer
- Contains: `Program.cs`, model class, controller, config files (All identical to API and Services projects)
- Key files: `Program.cs` (app bootstrap), `Controllers/WeatherForecastController.cs` (API endpoint)

**`Properties/` (inside each project):**
- Purpose: Visual Studio / .NET launch profiles and IIS Express configuration
- Contains: `launchSettings.json` (port bindings, environment variables for dev profiles)

## Key File Locations

**Entry Points:**
- `PRN232.LAB_1.API/Program.cs`: API layer application bootstrap
- `PRN232.LAB_1.Services/Program.cs`: Services layer application bootstrap
- `PRN232.LAB_1.Repositories/Program.cs`: Repositories layer application bootstrap

**Configuration:**
- `PRN232.LAB_1.API/Properties/launchSettings.json`: Port 5004 (HTTP) / 7242 (HTTPS)
- `PRN232.LAB_1.Services/Properties/launchSettings.json`: Port 5096 (HTTP) / 7106 (HTTPS)
- `PRN232.LAB_1.Repositories/Properties/launchSettings.json`: Port 5187 (HTTP) / 7055 (HTTPS)
- `appsettings.json` (in each project): Logging level configuration
- `PRN232.LAB_1.*.csproj` (in each project): .NET SDK, framework target, NuGet dependencies

**Core Logic:**
- `PRN232.LAB_1.*/Controllers/WeatherForecastController.cs`: The only endpoint handler in each project
- `PRN232.LAB_1.*/WeatherForecast.cs`: The only model class (duplicated in each project)

## Naming Conventions

**Projects:**
- Pattern: `PRN232.LAB_1.{Layer}` — e.g., `PRN232.LAB_1.API`, `PRN232.LAB_1.Services`, `PRN232.LAB_1.Repositories`
- Style: PascalCase, company prefix (`PRN232`), course prefix (`LAB_1`), layer suffix

**Files:**
- Pattern: PascalCase — e.g., `Program.cs`, `WeatherForecast.cs`, `WeatherForecastController.cs`
- Controllers follow the naming convention `{Entity}Controller.cs`

**Namespaces:**
- Pattern: `PRN232.LAB_1.{Layer}` — e.g., `PRN232.LAB_1.API.Controllers`
- Style: Mirror the project name and folder structure

**Classes:**
- Pattern: PascalCase — e.g., `WeatherForecast`, `WeatherForecastController`, `Program`
- Controllers inherit from `ControllerBase`

## Where to Add New Code

**New Feature (across all projects as wired layers):**
1. Shared models/interfaces: Create `PRN232.LAB_1.Shared/` class library project
2. Repository logic: Add to `PRN232.LAB_1.Repositories/`
3. Service/business logic: Add to `PRN232.LAB_1.Services/`
4. API endpoint: Add to `PRN232.LAB_1.API/`

**New Controller:**
- Implementation: `PRN232.LAB_1.API/Controllers/{Name}Controller.cs`
- Tests: Not yet configured (no test project exists)

**New Model:**
- Shared model: Ideally in `PRN232.LAB_1.Shared/` (doesn't exist yet — currently duplicated per project)

**New Interface/Contract:**
- Ideally in `PRN232.LAB_1.Shared/` (doesn't exist yet — currently no interfaces exist)

## Special Directories

**`.vs/`:**
- Purpose: Visual Studio user-specific settings, cache, and document state
- Generated: Yes (by Visual Studio)
- Committed: No (git-ignored)

**`.planning/`:**
- Purpose: Codebase analysis documents for GSD workflow
- Generated: Yes (by mapping commands)
- Committed: Yes (project workspace docs)

**`obj/`:**
- Purpose: Compiled intermediate build artifacts (one per project)
- Generated: Yes (by `dotnet build`)
- Committed: No (git-ignored)

---

*Structure analysis: 2026-05-14*
