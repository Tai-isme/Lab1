# Coding Conventions

**Analysis Date:** 2026-05-14

## Naming Patterns

**Files:**
- PascalCase for all `.cs` files matching the class name they contain (e.g., `WeatherForecast.cs`, `WeatherForecastController.cs`, `Program.cs`)
- Project-level `Program.cs` uses the default ASP.NET Core convention

**Classes:**
- PascalCase: `WeatherForecast`, `WeatherForecastController`
- Controller classes use the suffix `Controller`: `WeatherForecastController`

**Methods:**
- PascalCase for all methods: `Get()`, `AddControllers()`, `AddEndpointsApiExplorer()`, `AddSwaggerGen()`
- ASP.NET Core pipeline methods: `UseSwagger()`, `UseHttpsRedirection()`, `UseAuthorization()`, `MapControllers()`

**Variables:**
- `_camelCase` for private instance fields: `_logger` (`WeatherForecastController.cs:15`)
- `camelCase` for local variables and method parameters: `builder`, `app`, `args`, `index`
- `PascalCase` for public properties: `Date`, `TemperatureC`, `TemperatureF`, `Summary`
- `PascalCase` for static readonly arrays: `Summaries` (`WeatherForecastController.cs:9`)

**Types:**
- PascalCase for all classes: `WeatherForecast`, `WeatherForecastController`, `Program`
- Interfaces (when used) — none present in the codebase currently

## Code Style

**Formatting:**
- No `.editorconfig` or `Directory.Build.props` detected
- No explicit formatting rules or style analyzers configured
- **Key settings from `.csproj` files:**
  - `<Nullable>enable</Nullable>` — nullable reference types enabled project-wide
  - `<ImplicitUsings>enable</ImplicitUsings>` — implicit using directives enabled
- All 3 projects target `net8.0`

**Indentation:**
- 4 spaces per indent level (no tabs visible)
- Opening braces on new line (Allman style): consistent across all files

**Linting:**
- No linting/analysis tools configured (no `.ruleset`, no `Editorconfig`, no `StyleCop.Analyzers` NuGet package)
- No Roslyn analyzers detected beyond default .NET 8 SDK behavior

## Import Organization

**Order:**
- `using Microsoft.AspNetCore.Mvc;` — always the first and only import in controllers
- Implicit global usings (from `<ImplicitUsings>enable</ImplicitUsings>`) provide standard `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading.Tasks`, and ASP.NET Core namespaces automatically

**No path aliases detected.**

## Namespace Convention

**Pattern:** `PRN232.LAB_1.{ProjectName}[.SubNamespace]`
- `PRN232.LAB_1.API` — API project
- `PRN232.LAB_1.Repositories` — Repositories project
- `PRN232.LAB_1.Repositories.Controllers` — Controllers within Repositories
- `PRN232.LAB_1.Services` — Services project
- `PRN232.LAB_1.Services.Controllers` — Controllers within Services

Each project has its own top-level namespace matching the project name.

## Project Structure Convention

**3-layered solution structure:**
| Project | Role |
|---------|------|
| `PRN232.LAB_1.API` | Entry layer (Web API host) |
| `PRN232.LAB_1.Repositories` | Data access layer |
| `PRN232.LAB_1.Services` | Business logic layer |

All 3 projects are currently identical scaffold code — no cross-project references are configured.

## Error Handling

**Strategy:** No custom error handling implemented. Relies entirely on ASP.NET Core defaults:
- `app.UseHttpsRedirection()` for HTTPS enforcement
- `app.UseAuthorization()` for auth middleware
- No try-catch blocks in any code
- No custom exception filters or middleware
- No `ProblemDetails` or custom error response shaping
- No validation attributes present on models

**Current gap:** All controllers lack try-catch, guard clauses, input validation, and structured error responses.

## Logging

**Framework:** `ILogger<T>` via ASP.NET Core's built-in DI container

**Patterns:**
- Constructor-injected logger in every controller: `ILogger<WeatherForecastController> _logger`
- Logger stored as private readonly field with `_logger` naming
- Logger is injected but never actually used to log anything in the current code — the `_logger` field is assigned but no logging calls (`_logger.LogInformation`, etc.) are made

**Configuration:**
- `appsettings.json`: Default `"Information"`, `Microsoft.AspNetCore` at `"Warning"`

## Comments

**When to Comment:**
- Minimal comments — only the default ASP.NET Core template comments in `Program.cs`
- Comments describe pipeline sections: `"// Add services to the container."`, `"// Configure the HTTP request pipeline."`
- No XML/JSDoc comments on any public members
- No inline code comments explaining logic

## Function Design

**Size:**
- Small methods — `Get()` is 8 lines, `Program.cs` top-level statements are ~25 lines
- No method exceeds 10 meaningful lines of logic

**Parameters:**
- Minimal parameters — `Get()` takes none
- All parameters flow through DI (constructor injection) for cross-cutting concerns

**Return Values:**
- Controllers return `IEnumerable<WeatherForecast>` directly — no `IActionResult` or `ActionResult<T>` wrapper
- Expressed computed property: `TemperatureF => 32 + (int)(TemperatureC / 0.5556)`

## Module Design

**Exports:**
- `public class` is the only access modifier used — no `internal` or `private` classes
- Properties use `public get; set;` for all model properties
- No explicit interfaces defined

**Barrel Files:**
- Not applicable — no index/barrel files used

## Controller Convention

**Pattern:** All controllers follow this structure:

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[] { ... };
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get() { ... }
}
```

Key conventions:
- `[ApiController]` attribute on every controller
- `[Route("[controller]")]` convention-based routing (controller name derives route)
- Inherit from `ControllerBase` (not `Controller`)
- Constructor injection for dependencies
- `[HttpGet(Name = "...")]` with named route

## Model Convention

```csharp
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}
```

- Public auto-properties with `{ get; set; }`
- Computed/expression-bodied properties for derived values (`TemperatureF`)
- Nullable reference types used: `string?` (enabled by `<Nullable>enable</Nullable>`)
- `DateOnly` type used (instead of `DateTime`) — .NET 6+ feature

---

*Convention analysis: 2026-05-14*
