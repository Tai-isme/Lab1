# Technology Stack

**Project:** PRN232 Lab 1 — LMS REST API
**Researched:** 2026-05-14
**Mode:** Ecosystem
**Overall confidence:** HIGH

---

## Recommended Stack

### Core Framework

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET SDK | 8.0.x | Runtime & build tooling | LTS release supported until Nov 2026; lab spec requires `net8.0` |
| ASP.NET Core | 8.0 (via `Microsoft.NET.Sdk.Web`) | Web API framework | Industry standard for REST APIs on .NET; built-in DI, middleware, model binding |
| C# | 12 | Application language | Matches .NET 8; supports primary constructors, collection expressions, `required` members |

### Database & ORM

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Entity Framework Core | 8.0.26 | ORM / data access | Native .NET 8 support; code-first migrations; LINQ queries; fits repository pattern |
| EF Core SQL Server Provider | 8.0.26 | SQL Server database provider | Required by EF Core to communicate with SQL Server; wraps `Microsoft.Data.SqlClient` |
| SQL Server | 2022 (Docker image `mcr.microsoft.com/mssql/server:2022-latest`) | Relational database | Matches lab requirements; well-tested Docker image; Ubuntu 22.04 based for stability |

**Version rationale for EF Core 8.0.26:** As of May 2026, `Microsoft.EntityFrameworkCore.SqlServer` latest stable is 8.0.26 (published Feb 2026). All `Microsoft.EntityFrameworkCore.*` packages must use identical versions per Microsoft guidance. The 8.0.27 patch exists for the base `Microsoft.EntityFrameworkCore` package but `SqlServer` provider remains at 8.0.26 — pin all to 8.0.26 for consistency.

**Why SQL Server 2022 over 2025:** The `mcr.microsoft.com/mssql/server:2025-latest` image exists but uses Ubuntu 24.04 and is newer with less production validation. SQL Server 2022 is the current mainstream supported release, well-documented for Docker Compose patterns, and fully compatible with EF Core 8.0. The lab has zero SQL Server 2025-specific requirements.

### API Documentation

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Swashbuckle.AspNetCore | 6.6.2 | Swagger/OpenAPI 3.0 generation | Already in project; version included with .NET 8 templates; stable and well-tested for `net8.0` |

**Why NOT upgrade Swashbuckle:** Swashbuckle v8.0.0+ drops .NET 6 and targets newer TFMs. v10.0.0 requires ASP.NET Core 10 and has major breaking changes (OpenAPI.NET v2 migration). For `net8.0`, 6.6.2 is the correct pinned version. Upgrading would add churn with zero lab benefit — the lab only requires Swagger UI with endpoint listing, testing, and docs.

### Docker & Containerization

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Docker Engine | 24+ | Container runtime | Docker Desktop available on dev machine per lab context |
| Docker Compose | v2 (comes with Docker Desktop) | Multi-container orchestration | Defines API + SQL Server as linked services; healthcheck sequencing |
| SQL Server image | `mcr.microsoft.com/mssql/server:2022-latest` | Database in container | Official Microsoft image; Ubuntu 22.04 based; well-tested |
| .NET SDK image | `mcr.microsoft.com/dotnet/sdk:8.0` | Build stage in Dockerfile | Multi-stage build uses this for `dotnet restore/build/publish` |
| .NET Runtime image | `mcr.microsoft.com/dotnet/aspnet:8.0` | Runtime stage in Dockerfile | Smaller image size (~200MB vs ~1GB for SDK); production-optimized |

### Project SDK Decisions

| Project | Current SDK | Correct SDK | Reason |
|---------|-------------|-------------|--------|
| `PRN232.LAB_1.API` | `Microsoft.NET.Sdk.Web` | `Microsoft.NET.Sdk.Web` | ✅ Correct — exposes endpoints, has `Program.cs` entry point |
| `PRN232.LAB_1.Services` | `Microsoft.NET.Sdk.Web` | `Microsoft.NET.Sdk` | ❌ **Must change** — should be a class library, not a web app |
| `PRN232.LAB_1.Repositories` | `Microsoft.NET.Sdk.Web` | `Microsoft.NET.Sdk` | ❌ **Must change** — should be a class library, not a web app |

**Why change Services and Repositories to `Microsoft.NET.Sdk`:**
- `Microsoft.NET.Sdk.Web` implicitly adds `OutputType` as `Exe`, requiring a `Main()` entry point. Class libraries should produce `.dll` only.
- `Microsoft.NET.Sdk.Web` pulls in IIS Express, launch profiles, and web-specific targets irrelevant to class libraries.
- Neither Services nor Repositories contain controllers, middleware, or startup configuration — they have zero need for the web SDK.
- If these projects need ASP.NET Core types (e.g., `[FromBody]` attributes in Service interfaces — which they shouldn't), add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` instead. But in a clean 3-layer architecture, Services and Repositories should NOT reference ASP.NET Core MVC attributes.

---

## Supporting Libraries

| Library | Version | Purpose | When to Use | Confidence |
|---------|---------|---------|-------------|------------|
| `Microsoft.EntityFrameworkCore` | 8.0.26 | Core EF functionality | All projects using EF (Repositories) | HIGH |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.26 | SQL Server provider | Repositories project only (DbContext) | HIGH |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.26 | CLI migrations (`dotnet ef`) | Installed globally or in API project for migration commands | HIGH |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.26 | Design-time EF tools (migration scaffolding) | API project only (needed for `dotnet ef migrations add`) | HIGH |
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger UI + OpenAPI JSON | API project only | HIGH |
| `Microsoft.Data.SqlClient` | 5.1.7+ | SQL Server data access (transitive from EF provider) | Pinned via EF SqlServer dependency; no direct reference needed unless newer features required | MEDIUM |

---

## Libraries NOT Used — And Why

| Library | Why We're Not Using It | What We Do Instead |
|---------|------------------------|-------------------|
| **AutoMapper** | Adds complexity for simple DTO mapping; lab has 5 entity types only. Mapping is trivially done with manual mapper methods or implicit operators. AutoMapper's convention-based mapping causes debugging difficulty in small projects. | Manual mapping in service layer between Entity ↔ Business ↔ Request/Response models |
| **FluentValidation** | Excluded by lab spec ("Advanced validation out of scope"). Would add package dependency for minimal gain given lab constraints. | Data annotations on Request models (built-in, no extra package) |
| **Serilog** | Out of scope — lab spec excludes global exception handling and advanced logging. Console logging via `Microsoft.Extensions.Logging` (built-in) is sufficient. | Built-in `ILogger<T>` from `Microsoft.Extensions.Logging` |
| **MediatR / CQRS** | Overkill for simple CRUD. Adds in-process messaging, handlers, pipelines — unnecessary indirection for 5 resources with no read/write segregation need. | Direct service → repository calls |
| **Dapper** | EF Core already selected. Adding another data access library hybridizes the DAL with no benefit for this project scope. | EF Core exclusively |
| **Newtonsoft.Json** | ASP.NET Core 8 uses `System.Text.Json` by default (built-in, faster, no dependency). Newtonsoft is only needed if you require specific serialization features it has that STJ lacks — not the case here. | `System.Text.Json` (built-in) |
| **Swashbuckle v8+ / v10+** | Requires newer .NET TFMs or introduces breaking API changes. v6.6.2 is the correct version for .NET 8. | Swashbuckle 6.6.2 |

---

## Project Structure & Layer Dependencies

### Dependency Chain

```
  PRN232.Lab1.API                     (Presentation Layer — Controllers, DTOs, Startup)
       │
       ▼
  PRN232.Lab1.Services                (Business Logic Layer — Services, Business Models)
       │
       ▼
  PRN232.Lab1.Repositories            (Data Access Layer — DbContext, Entities, Repository implementations)
```

### Layer Responsibilities

```
┌─────────────────────────────────────────────────┐
│                 API Layer                        │
│  Controllers  →  Request Models  →  Response    │
│  Program.cs   →  Middleware  →  Swagger Config   │
│  HTTP only — no business logic, no data access   │
├─────────────────────────────────────────────────┤
│               Services Layer                     │
│  Business Logic  →  Mapping  →  Orchestration    │
│  Business Models  →  Service Interfaces/Impls    │
│  No HTTP awareness, no EF references             │
├─────────────────────────────────────────────────┤
│             Repositories Layer                   │
│  DbContext  →  Entity Models  →  Migrations      │
│  Repository Interfaces/Impls  →  DB access       │
│  EF Core lives here — no business logic          │
└─────────────────────────────────────────────────┘
```

### What Each Project References

| Project | References | NuGet Packages |
|---------|-----------|----------------|
| `PRN232.Lab1.API` | Services project | `Swashbuckle.AspNetCore` 6.6.2, `Microsoft.EntityFrameworkCore.Design` 8.0.26 |
| `PRN232.Lab1.Services` | Repositories project | *(none — business models only)* |
| `PRN232.Lab1.Repositories` | *(nothing — bottom layer)* | `Microsoft.EntityFrameworkCore` 8.0.26, `Microsoft.EntityFrameworkCore.SqlServer` 8.0.26 |

The API project must also reference `Microsoft.EntityFrameworkCore.Design` (8.0.26), NOT the Repositories project, because `dotnet ef migrations` commands run from the startup project (the API).

---

## EF Core Configuration

### DbContext Setup (Repositories Layer)

```csharp
// PRN232.Lab1.Repositories/Data/LmsDbContext.cs
public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity mappings, relationships, constraints
        // Seed data in OnModelCreating via modelBuilder.Entity<T>().HasData(...)
    }
}
```

### Registration in API Layer (Program.cs)

```csharp
// In PRN232.Lab1.API/Program.cs
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Connection String Pattern

```json
// PRN232.Lab1.API/appsettings.json (development — Docker Compose)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PRN232_Lab1;User Id=sa;Password=Lab1_Pass123;TrustServerCertificate=True;"
  }
}
```

**Docker Compose override:** When running in Docker Compose, the API container connects using the *service name* as hostname:
```
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=PRN232_Lab1;User Id=sa;Password=Lab1_Pass123;TrustServerCertificate=True
```

This is set as an environment variable in the `docker-compose.yml` or in `appsettings.Docker.json`.

---

## Docker Compose Configuration

### docker-compose.yml

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: lms-db
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=Lab1_Pass123
    ports:
      - "1433:1433"
    volumes:
      - lms-sql-data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Lab1_Pass123" -C -Q "SELECT 1" || exit 1
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 30s

  api:
    build:
      context: .
      dockerfile: PRN232.Lab1.API/Dockerfile
    container_name: lms-api
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=PRN232_Lab1;User Id=sa;Password=Lab1_Pass123;TrustServerCertificate=True
    depends_on:
      sqlserver:
        condition: service_healthy

volumes:
  lms-sql-data:
    name: lms-sql-data
```

**Key decisions:**
1. **Healthcheck on SQL Server:** Prevents the API from starting before the DB accepts connections. Without this, `dotnet ef` or EF initialization fails on first deploy.
2. **Named volume:** `lms-sql-data` persists database files across container restarts. Without this, every `docker compose down` destroys all data.
3. **`depends_on` with condition:** Only starts the API container when SQL Server healthcheck passes.
4. **Port mapping 5000:8080:** Maps host port 5000 to container's internal 8080 (set via `ASPNETCORE_URLS`). Avoids port conflicts with other SQL Server instances.
5. **Connection string override via env var:** Docker Compose passes the connection string as an environment variable using the double-underscore syntax (`ConnectionStrings__DefaultConnection`), which .NET Configuration reads as a hierarchical key.

### Dockerfile (Multi-stage Build)

```dockerfile
# PRN232.Lab1.API/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore (layer caching)
COPY PRN232.Lab1.API/*.csproj PRN232.Lab1.API/
COPY PRN232.Lab1.Services/*.csproj PRN232.Lab1.Services/
COPY PRN232.Lab1.Repositories/*.csproj PRN232.Lab1.Repositories/
COPY Lab1.sln .
RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish PRN232.Lab1.API/PRN232.Lab1.API.csproj -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PRN232.Lab1.API.dll"]
```

**Multi-stage benefits:**
- **Build stage** uses the SDK image (~1GB) for restore/build/publish.
- **Runtime stage** uses the smaller aspnet image (~200MB) for running.
- **Layer caching:** `.csproj` files are copied and restored first. Docker caches this layer — subsequent builds skip restore unless project files change.

### Environment-Specific Configuration

```json
// appsettings.Docker.json (loaded when ASPNETCORE_ENVIRONMENT=Docker)
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

---

## DI Registration Pattern

### What Gets Registered Where

```csharp
// PRN232.Lab1.API/Program.cs

// 1. EF Core
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Repositories
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// 3. Services
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// 4. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
```

**Lifetime rationale:**
- **`AddScoped`** for repositories and services — creates one instance per HTTP request. This is the standard for EF Core + Web API because `DbContext` is also scoped.
- **`AddDbContext` defaults to scoped** — matches repository/service lifetimes.
- **`AddSingleton` would be wrong** — would cause stale data across requests.
- **`AddTransient` would be wasteful** — creates new instances per injection point.

---

## Swagger/OpenAPI Configuration

### Current (Stays as-is)

Swashbuckle 6.6.2 with default settings is sufficient for the lab requirements:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**What this gives you (lab checklist):**
- ✅ Swagger UI at `/swagger` (dev/Docker only)
- ✅ OpenAPI 3.0 JSON at `/swagger/v1/swagger.json`
- ✅ Endpoint listing with HTTP methods and paths
- ✅ Request/response schemas from .NET model classes
- ✅ Status code documentation from `[ProducesResponseType]` attributes
- ✅ "Try it out" for testing endpoints

**XML documentation comments (optional enhancement):**
To get richer Swagger docs from `<summary>` XML comments:

```xml
<!-- PRN232.Lab1.API.csproj -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

```csharp
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

This is **not required** for the lab but improves Swagger UI readability.

---

## Installation & Setup Commands

### Package Installation

```powershell
# API project
dotnet add PRN232.Lab1.API/PRN232.Lab1.API.csproj package Swashbuckle.AspNetCore --version 6.6.2
dotnet add PRN232.Lab1.API/PRN232.Lab1.API.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.26

# Repositories project (change SDK to Microsoft.NET.Sdk first)
dotnet add PRN232.Lab1.Repositories/PRN232.Lab1.Repositories.csproj package Microsoft.EntityFrameworkCore --version 8.0.26
dotnet add PRN232.Lab1.Repositories/PRN232.Lab1.Repositories.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.26

# Services project — NO packages needed (pure C# business logic)
```

### DotNet CLI EF Migrations

```powershell
# Install EF tools globally (one-time)
dotnet tool install --global dotnet-ef --version 8.0.26

# Create initial migration
dotnet ef migrations add InitialCreate --project PRN232.Lab1.Repositories --startup-project PRN232.Lab1.API

# Apply migration to database (local dev without Docker)
dotnet ef database update --project PRN232.Lab1.Repositories --startup-project PRN232.Lab1.API

# Generate SQL script (for Docker deployment)
dotnet ef migrations script --project PRN232.Lab1.Repositories --startup-project PRN232.Lab1.API -o script.sql
```

### Docker Commands

```powershell
# Build and start containers
docker compose up --build -d

# View logs
docker compose logs -f

# Stop and remove containers (preserves volume data)
docker compose down

# Stop and remove everything including volumes (DESTRUCTIVE — deletes DB data)
docker compose down -v
```

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| **Database** | SQL Server 2022 | SQL Server 2025 | 2025 image is newer with less community Docker Compose documentation; 2022 is proven stable |
| **Database** | SQL Server 2022 | PostgreSQL 16 | Lab spec says SQL Server. Npgsql provider exists but would violate requirements. |
| **OR Mapping** | EF Core 8 | Dapper | Dapper lacks migrations, change tracking, and LINQ — more code for the same result in a CRUD API. EF Core is the standard for this lab pattern. |
| **DI** | Built-in Microsoft DI | Autofac / Scrutor | Built-in DI in .NET 8 is fully sufficient for < 20 service registrations. Third-party containers add complexity for no benefit at this scale. |
| **API Docs** | Swashbuckle 6.6.2 | NSwag / Scalar | Swashbuckle is already in the project and is the standard for .NET 8. NSwag adds code generation tooling; Scalar requires separate setup. Neither improves lab outcomes. |
| **Project SDK** (Services/Repos) | `Microsoft.NET.Sdk` | `Microsoft.NET.Sdk.Web` | Current state is WRONG. Web SDK creates executables instead of libraries. Must be fixed. |

---

## Confidence Assessment

| Decision | Confidence | Basis |
|----------|------------|-------|
| EF Core 8.0.26 | **HIGH** | Verified on NuGet gallery: `Microsoft.EntityFrameworkCore.SqlServer` latest 8.x is 8.0.26 |
| Swashbuckle 6.6.2 | **HIGH** | Official Microsoft doc for .NET 8; confirmed in existing project |
| SQL Server 2022 Docker | **HIGH** | Official MCR documentation shows 2022-latest tag with Ubuntu 22.04 |
| Project SDK change needed | **HIGH** | Microsoft docs confirm `Microsoft.NET.Sdk.Web` produces executables; class libraries must use `Microsoft.NET.Sdk` |
| Docker Compose healthcheck pattern | **MEDIUM** | Community pattern (not official Microsoft). Multiple sources agree; standard practice for container sequencing. |
| 3-layer dependency chain | **HIGH** | Confirmed by multiple architecture sources (Microsoft Learn, Woodruff, common web architecture patterns) |
| Scoped DI lifetime | **HIGH** | Standard EF Core guidance — `AddDbContext` defaults to scoped; repositories and services must match |

---

## Sources

| Source | URL | What It Confirms | Confidence |
|--------|-----|-------------------|------------|
| Microsoft Learn — EF Core NuGet packages | https://learn.microsoft.com/en-us/ef/core/what-is-new/nuget-packages | EF Core package architecture, version consistency requirement | HIGH |
| NuGet Gallery — EF Core SqlServer 8.0.26 | https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/8.0.26 | Current latest 8.x version | HIGH |
| Microsoft Learn — EF Core releases | https://learn.microsoft.com/en-us/ef/core/what-is-new/ | .NET 8 / EF Core 8 support until Nov 2026 | HIGH |
| GitHub — Swashbuckle v6.6.2 | https://github.com/dotnet/aspnetcore/pull/56266 | 6.6.2 is the version bundled with .NET 8 templates | HIGH |
| GitHub — Swashbuckle v8.0.0 | https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v8.0.0 | v8 drops .NET 6, targets newer TFMs — not for .NET 8 | HIGH |
| MCR — SQL Server container images | https://mcr.microsoft.com/product/mssql/server/about | Available tags: 2022-latest and 2025-latest | HIGH |
| Microsoft Learn — Common web application architectures | https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures | Clean/layered architecture patterns for ASP.NET Core | HIGH |
| Woodruff — Layered Architecture in ASP.NET Core | https://woodruff.dev/stop-letting-your-controllers-talk-to-sql-layered-architecture-in-asp-net-core/ | 3-layer pattern with explicit boundaries | MEDIUM |
| Docker Compose SQL Server template | https://github.com/kakashidota/docker-compose-dotnet-sql-template | Docker Compose + SQL Server + .NET 8 working template | MEDIUM |
| Microsoft Learn — Swashbuckle + ASP.NET Core | https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0 | Swashbuckle 6.6.2 with .NET 8 setup | HIGH |

---

## Migration Steps (From Current State)

The current scaffold has several issues that must be fixed before building features:

1. **Fix project SDKs:**
   - `PRN232.LAB_1.Repositories.csproj`: Change SDK from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk`
   - `PRN232.LAB_1.Services.csproj`: Change SDK from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk`

2. **Add project references:**
   - `PRN232.LAB_1.API` → reference `PRN232.LAB_1.Services`
   - `PRN232.LAB_1.Services` → reference `PRN232.LAB_1.Repositories`

3. **Remove Swashbuckle from Repositories and Services projects:**
   - Not needed — they don't expose endpoints

4. **Add required NuGet packages** (see Installation section above)

5. **Remove Controllers folder from Repositories and Services:**
   - The scaffold incorrectly created controller stubs in all 3 projects. Only the API project should have Controllers.

6. **Create DbContext and entity models** in Repositories project

7. **Create Service interfaces and implementations** in Services project

8. **Wire up DI and Swagger** in API/Program.cs

9. **Create Dockerfile** in API project (not repo root, but context from repo root for Docker Compose)

10. **Create docker-compose.yml** in solution root

---

*Stack analysis completed for Phase 6 — Research. Feeds into roadmap for Phase 1 implementation.*
