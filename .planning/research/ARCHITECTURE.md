# Architecture: 3-Layer LMS REST API

**Project:** PRN232 Lab 1 — LMS REST API  
**Researched:** 2026-05-14  
**Overall confidence:** HIGH — based on Microsoft official guidance and widely-adopted ASP.NET Core layered architecture patterns

---

## 1. Current State (Before)

All three projects (`API`, `Services`, `Repositories`) are **standalone Web API executables** — each has its own `Program.cs`, Controllers folder, and `Microsoft.NET.Sdk.Web` SDK. **No project references exist.** No interfaces. No DI wiring. Each project duplicates `WeatherForecast` boilerplate.

```text
┌──────────────────────────────────────┐    ┌──────────────────────────────────────┐    ┌──────────────────────────────────────┐
│          PRN232.LAB_1.API            │    │       PRN232.LAB_1.Services          │    │      PRN232.LAB_1.Repositories       │
│          Sdk="Microsoft.NET.Sdk.Web"  │    │       Sdk="Microsoft.NET.Sdk.Web"    │    │       Sdk="Microsoft.NET.Sdk.Web"    │
│                                      │    │                                      │    │                                      │
│  Program.cs                          │    │  Program.cs                          │    │  Program.cs                          │
│  Controllers/WeatherForecastCtrl.cs   │    │  Controllers/WeatherForecastCtrl.cs   │    │  Controllers/WeatherForecastCtrl.cs   │
│  WeatherForecast.cs (model)          │    │  WeatherForecast.cs (model)          │    │  WeatherForecast.cs (model)          │
│  appsettings.json                    │    │  appsettings.json                    │    │  appsettings.json                    │
│  (port 5004)                         │    │  (port 5096)                         │    │  (port 5187)                         │
└──────────────────────────────────────┘    └──────────────────────────────────────┘    └──────────────────────────────────────┘
         No references →                    No references →                    (bottom)
```

**Problems found in codebase audit:**
1. All three projects are `Microsoft.NET.Sdk.Web` — they cannot be referenced as class libraries
2. Zero `<ProjectReference>` elements across all `.csproj` files
3. Each project hosts its own HTTP server (ports 5004, 5096, 5187) — Services and Repositories should NOT be web servers
4. No interfaces, no DI registrations, no actual layering
5. `WeatherForecast` model and controller are copy-pasted identically in all 3 projects

---

## 2. Target Architecture (After)

A strict 3-layer architecture where **dependencies flow downward** (API → Services → Repositories) and **no layer skips another**.

```text
┌───────────────────────────────────────────────────────────────────────────┐
│                      PRN232.LAB_1.API (Web API)                          │
│               Sdk="Microsoft.NET.Sdk.Web"      port: 5004                 │
│                                                                           │
│  Program.cs (composition root — DI wiring, Swagger, middleware)           │
│  Controllers/                                                             │
│    SemestersController.cs                                                   │
│    CoursesController.cs                                                    │
│    SubjectsController.cs                                                    │
│    StudentsController.cs                                                    │
│    EnrollmentsController.cs                                                 │
│                                                                           │
│  Dependencies:                                                             │
│    → PRN232.LAB_1.Services (project reference)                             │
│    → Swashbuckle.AspNetCore 6.6.2                                          │
│    → Microsoft.EntityFrameworkCore.Design (for migrations, dev only)        │
└───────────────────────┬───────────────────────────────────────────────────┘
                        │ references (compile-time)
                        ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                   PRN232.LAB_1.Services (Class Library)                   │
│               Sdk="Microsoft.NET.Sdk"     NOT a web project                │
│                                                                           │
│  This layer contains:                                                      │
│  - Service interfaces (contracts the API calls)                            │
│  - Service implementations (business logic, validation, orchestration)     │
│  - Business models (domain models for business processing)                 │
│  - Request DTOs (input models — what the API receives)                     │
│  - Response DTOs (output models — what the API returns)                    │
│  - Model mapping / transformation logic                                    │
│                                                                           │
│  /Interfaces/                                                              │
│    ISemesterService.cs                                                      │
│    ICourseService.cs                                                        │
│    ISubjectService.cs                                                       │
│    IStudentService.cs                                                       │
│    IEnrollmentService.cs                                                    │
│  /Implementations/                                                         │
│    SemesterService.cs                                                       │
│    CourseService.cs                                                         │
│    SubjectService.cs                                                        │
│    StudentService.cs                                                        │
│    EnrollmentService.cs                                                     │
│  /Models/                                                                   │
│    /Business/  Semester.cs, Course.cs, Subject.cs, Student.cs, Enrollment.cs │
│    /Request/   SemesterRequest.cs, CourseRequest.cs, ...                     │
│    /Response/  SemesterResponse.cs, CourseResponse.cs, ...                   │
│                                                                           │
│  Dependencies:                                                             │
│    → PRN232.LAB_1.Repositories (project reference)                         │
└───────────────────────┬───────────────────────────────────────────────────┘
                        │ references (compile-time)
                        ▼
┌───────────────────────────────────────────────────────────────────────────┐
│                 PRN232.LAB_1.Repositories (Class Library)                 │
│               Sdk="Microsoft.NET.Sdk"     NOT a web project                │
│                                                                           │
│  This layer contains:                                                      │
│  - Entity models (EF Core entity classes, 1:1 with DB tables)              │
│  - DbContext (LmsDbContext)                                                 │
│  - EF Core migrations                                                      │
│  - Repository interfaces (abstractions for data access)                    │
│  - Repository implementations (EF Core queries)                            │
│  - Entity configurations (Fluent API mappings)                             │
│  - Seed data logic                                                         │
│                                                                           │
│  /Entities/                                                                │
│    Semester.cs                                                              │
│    Course.cs                                                                │
│    Subject.cs                                                               │
│    Student.cs                                                               │
│    Enrollment.cs                                                            │
│  /Interfaces/                                                               │
│    IRepository.cs (generic)                                                 │
│    ISemesterRepository.cs (entity-specific)                                 │
│    ICourseRepository.cs                                                     │
│    ISubjectRepository.cs                                                    │
│    IStudentRepository.cs                                                    │
│    IEnrollmentRepository.cs                                                 │
│  /Implementations/                                                          │
│    Repository.cs (generic base)                                             │
│    SemesterRepository.cs                                                    │
│    CourseRepository.cs                                                      │
│    SubjectRepository.cs                                                     │
│    StudentRepository.cs                                                     │
│    EnrollmentRepository.cs                                                  │
│  Data/LmsDbContext.cs                                                       │
│  Data/Configurations/                                                       │
│    SemesterConfiguration.cs                                                 │
│    CourseConfiguration.cs                                                   │
│    SubjectConfiguration.cs                                                  │
│    StudentConfiguration.cs                                                  │
│    EnrollmentConfiguration.cs                                               │
│                                                                           │
│  Dependencies:                                                              │
│    → Microsoft.EntityFrameworkCore 8.0.x                                    │
│    → Microsoft.EntityFrameworkCore.SqlServer 8.0.x (or target DB provider)  │
│    → (NO reference to Services or API)                                     │
└───────────────────────────────────────────────────────────────────────────┘
```

### Key Architectural Rule

**Dependency direction is strictly one-way:** `API → Services → Repositories`.

- ❌ API must NOT reference Repositories directly
- ❌ Services must NOT reference API
- ❌ Repositories must NOT reference Services or API
- ❌ Controllers must NOT instantiate DbContext or run EF queries

---

## 3. Component Boundaries

### 3.1 API Layer (PRN232.LAB_1.API)

| Component | Responsibility | Strictly Forbidden |
|-----------|----------------|--------------------|
| **Controllers** | Receive HTTP requests, validate input, call service interfaces, return HTTP responses | Business logic, data access, EF Core calls, `DbContext`, instantiating services |
| **Filters** (optional) | Cross-cutting concerns (logging, validation) | Business decisions |
| **Middleware** (optional) | Request/response pipeline concerns | Data access |
| **Program.cs** | Composition root: register DI, configure middleware, Swagger | Business logic |
| **appsettings.json** | Connection strings, configuration | Secrets, credentials |

**What controllers do:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService; // Only depends on interface from Services
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SemesterResponse>> GetById(int id)
    {
        var result = await _semesterService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SemesterResponse>>> GetAll(
        [FromQuery] QueryParameters parameters)
    {
        var result = await _semesterService.GetAllAsync(parameters);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SemesterResponse>> Create(
        [FromBody] SemesterRequest request)
    {
        var result = await _semesterService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

### 3.2 Services Layer (PRN232.LAB_1.Services)

| Component | Responsibility | Strictly Forbidden |
|-----------|----------------|--------------------|
| **Service Interfaces** | Define the contract API layer depends on — `ISemesterService`, etc. | Implementation details |
| **Service Implementations** | Business logic, validation, orchestration, model mapping | HTTP concerns, `HttpContext`, controller logic, direct `DbContext` access |
| **Business Models** | Domain objects with business rules | EF Core attributes, JSON serialization attributes |
| **Request DTOs** | Input contracts for service operations | Business logic |
| **Response DTOs** | Output contracts from service operations | EF Core navigation properties |

**What services do:**
```csharp
public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repository;
    private readonly IMapper _mapper;

    public SemesterService(ISemesterRepository repository, IMapper mapper)
    {
        _repository = repository;   // Only depends on repository interface
        _mapper = mapper;
    }

    public async Task<SemesterResponse?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<SemesterResponse>(entity);
    }

    public async Task<PagedResult<SemesterResponse>> GetAllAsync(QueryParameters parameters)
    {
        var query = _repository.GetQueryable(); // Returns IQueryable<Semester>
        // Apply search, sort, paging via IQueryable
        // Return paged result mapped to Response DTOs
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        return new PagedResult<SemesterResponse>
        {
            Items = _mapper.Map<List<SemesterResponse>>(items),
            TotalCount = await query.CountAsync(),
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }
}
```

### 3.3 Repositories Layer (PRN232.LAB_1.Repositories)

| Component | Responsibility | Strictly Forbidden |
|-----------|----------------|--------------------|
| **Entity Models** | EF Core entity classes — 1:1 with database tables | Business logic, validation, HTTP concerns |
| **DbContext** | EF Core `DbContext` subclass — `DbSet` properties, entity configurations | Business logic |
| **Entity Configurations** | Fluent API config — table mapping, keys, relationships, constraints | Business logic |
| **Repository Interfaces** | Data access contract — `IRepository<T>`, `ISemesterRepository`, etc. | Implementation details |
| **Repository Implementations** | EF Core query execution — `_context.Semesters.Where(...)` | Business logic, HTTP |

**What repositories do:**
```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly LmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(LmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
```

---

## 4. The Four Model Types

This is a critical architectural constraint. Each model type lives in a specific layer and has specific rules.

| Model Type | Lives In | Purpose | Rules |
|-----------|----------|---------|-------|
| **Entity** | Repositories/Entities | Database mapping — one C# class per DB table | `[Table]`, `[Column]`, `[Key]` attributes; navigation properties for FK relationships; no business logic; no JSON serialization attributes |
| **Business** | Services/Models/Business | Domain processing — validation rules, computed properties | No EF Core attributes; no `[Key]` or `[Column]`; plain C# objects; may contain business methods |
| **Request** | Services/Models/Request | Client input contract — what the API accepts | Data annotations for input validation (`[Required]`, `[StringLength]`, `[Range]`); no entity navigation properties |
| **Response** | Services/Models/Response | API output contract — what the API returns | Read-only shape; flat (no circular references); may aggregate data from multiple entities; no navigation properties |

### Example: Entity → Response Flow

```
[Semester Entity]                         [SemesterResponse]
┌────────────────────────┐                ┌──────────────────────────┐
│ Id: int                │                │ Id: int                  │
│ Code: string           │  Service       │ Code: string             │
│ Name: string           │  maps using    │ Name: string             │
│ StartDate: DateTime    │  AutoMapper    │ StartDate: DateTime      │
│ EndDate: DateTime      │  ──────────►   │ EndDate: DateTime        │
│ Status: string         │                │ Status: string           │
│ IsDeleted: bool        │                │ CourseCount: int  ← computed |
│ CreatedAt: DateTime    │                │ (no IsDeleted/           │
│ Courses: ICollection   │                │  CreatedAt/Courses)      │
└────────────────────────┘                └──────────────────────────┘

[SemesterRequest]
┌──────────────────────────┐
│ Code: string      [Req]  │
│ Name: string      [Req]  │
│ StartDate: DateTime [Req]│
│ EndDate: DateTime   [Req]│
│ Status: string           │
└──────────────────────────┘
```

### Mapping strategy

Use **AutoMapper** (or manual mapping in the service layer) to transform between model types:

```csharp
// AutoMapper Profile in Services layer
public class SemesterMappingProfile : Profile
{
    public SemesterMappingProfile()
    {
        CreateMap<Semester, SemesterResponse>()
            .ForMember(dest => dest.CourseCount,
                       opt => opt.MapFrom(src => src.Courses.Count));

        CreateMap<SemesterRequest, Semester>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Courses, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
```

---

## 5. Data Flow

### 5.1 Primary Request Path (GET by ID)

```
Client                          API Layer                   Services Layer              Repositories Layer          Database
  │                               │                            │                            │                       │
  │  GET /api/semesters/5         │                            │                            │                       │
  │ ─────────────────────────►    │                            │                            │                       │
  │                               │  SemesterController        │                            │                       │
  │                               │  .GetById(5)               │                            │                       │
  │                               │    ────►                   │                            │                       │
  │                               │                            │  ISemesterService           │                       │
  │                               │                            │  .GetByIdAsync(5)           │                       │
  │                               │                            │    ────►                   │                       │
  │                               │                            │                            │  ISemesterRepository   │
  │                               │                            │                            │  .GetByIdAsync(5)      │
  │                               │                            │                            │    ────►               │
  │                               │                            │                            │                       │  SELECT * FROM
  │                               │                            │                            │                       │  Semesters WHERE Id=5
  │                               │                            │                            │                       │ ◄──── /
  │                               │                            │                            │  ◄── Semester entity  │
  │                               │                            │  ◄── SemesterResponse     │                       │
  │                               │  ◄── SemesterResponse     │                            │                       │
  │  ◄── 200 OK {data}           │                            │                            │                       │
  │ ◄─────────────────────────    │                            │                            │                       │
```

### 5.2 Collection Request Path (GET with search, sort, paging)

```
Client                          API Layer                   Services Layer                Repositories Layer
  │                               │                            │                            │
  │  GET /api/semesters?          │                            │                            │
  │  search=2024&sort=code        │                            │                            │
  │  &page=1&pageSize=10          │                            │                            │
  │ ─────────────────────────►    │                            │                            │
  │                               │  Controller validates      │                            │
  │                               │  query parameters          │                            │
  │                               │    ────►                   │                            │
  │                               │                            │  Service receives          │
  │                               │                            │  QueryParameters            │
  │                               │                            │    ────►                   │
  │                               │                            │                            │  Repo.GetQueryable()
  │                               │                            │                            │  returns IQueryable<T>  ──►
  │                               │                            │  Service applies:          │
  │                               │                            │  1. Where(search filter)    │
  │                               │                            │  2. OrderBy(sort)          │
  │                               │                            │  3. Skip/Take(paging)      │
  │                               │                            │  4. Count(total)           │
  │                               │                            │    ────► repositories      │
  │                               │                            │  ◄── entities              │
  │                               │  ◄── PagedResult<>         │  maps to Response DTOs     │
  │  ◄── 200 OK {                │                            │                            │
  │    data: [...],              │                            │                            │
  │    pagination: {             │                            │                            │
  │      page, pageSize,         │                            │                            │
  │      totalCount,             │                            │                            │
  │      totalPages              │                            │                            │
  │    }                         │                            │                            │
  │  }                           │                            │                            │
```

### 5.3 Create Request Path (POST)

```
Client                          API Layer                   Services Layer                Repositories Layer
  │                               │                            │                            │
  │  POST /api/semesters          │                            │                            │
  │  {code, name, startDate,      │                            │                            │
  │   endDate, status}            │                            │                            │
  │ ─────────────────────────►    │                            │                            │
  │                               │  [FromBody] binding         │                            │
  │                               │  ModelState.IsValid check   │                            │
  │                               │  (400 if invalid)           │                            │
  │                               │    ────►                   │                            │
  │                               │                            │  Validate business rules:   │
  │                               │                            │  - Dates coherent?         │
  │                               │                            │  - Code unique?            │
  │                               │                            │  - Status valid?           │
  │                               │                            │  Map Request → Entity      │
  │                               │                            │    ────►                   │
  │                               │                            │                            │  Repo.AddAsync(entity)
  │                               │                            │                            │  SaveChanges()
  │                               │                            │  Map Entity → Response      │
  │                               │  ◄── SemesterResponse      │                            │
  │  ◄── 201 Created              │                            │                            │
  │  Location: /api/semesters/6   │                            │                            │
```

---

## 6. Project Reference Conversion

### 6.1 The Conversion

**Critical action:** Services and Repositories must be converted from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk`.

This means:
1. Change the `<Project Sdk="...">` in their `.csproj` files
2. Delete `Program.cs` (they are no longer entry points)
3. Delete `Controllers/` folder (they are not web projects)
4. Delete `appsettings.json`, `appsettings.Development.json`, `Properties/launchSettings.json`
5. Remove `Swashbuckle.AspNetCore` package reference (only API needs it)
6. Add `<ProjectReference>` elements

### 6.2 PRN232.LAB_1.Repositories.csproj (converted)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Changed from Sdk="Microsoft.NET.Sdk.Web" -->

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

### 6.3 PRN232.LAB_1.Services.csproj (converted)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Changed from Sdk="Microsoft.NET.Sdk.Web" -->

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="13.0.1" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PRN232.LAB_1.Repositories\PRN232.LAB_1.Repositories.csproj" />
  </ItemGroup>
</Project>
```

### 6.4 PRN232.LAB_1.API.csproj (updated)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PRN232.LAB_1.Services\PRN232.LAB_1.Services.csproj" />
  </ItemGroup>

</Project>
```

### 6.5 Solution file update

The `.sln` file needs the project GUIDs properly registered. When projects are converted, re-add them to the solution:

```
dotnet sln Lab1.sln add PRN232.LAB_1.Repositories\PRN232.LAB_1.Repositories.csproj
dotnet sln Lab1.sln add PRN232.LAB_1.Services\PRN232.LAB_1.Services.csproj
dotnet sln Lab1.sln add PRN232.LAB_1.API\PRN232.LAB_1.API.csproj
```

---

## 7. Interface & DI Registration Patterns

### 7.1 Interface Location

| Interface | Defined In | Implemented In |
|-----------|-----------|----------------|
| `IRepository<T>` | Repositories/Interfaces | Repositories/Implementations |
| `ISemesterRepository` | Repositories/Interfaces | Repositories/Implementations |
| `ICourseRepository` | Repositories/Interfaces | Repositories/Implementations |
| `ISubjectRepository` | Repositories/Interfaces | Repositories/Implementations |
| `IStudentRepository` | Repositories/Interfaces | Repositories/Implementations |
| `IEnrollmentRepository` | Repositories/Interfaces | Repositories/Implementations |
| `ISemesterService` | Services/Interfaces | Services/Implementations |
| `ICourseService` | Services/Interfaces | Services/Implementations |
| `ISubjectService` | Services/Interfaces | Services/Implementations |
| `IStudentService` | Services/Interfaces | Services/Implementations |
| `IEnrollmentService` | Services/Interfaces | Services/Implementations |

### 7.2 DI Registration (in API's Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// ── Services ──
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// ── AutoMapper ──
builder.Services.AddAutoMapper(typeof(SemesterMappingProfile).Assembly);

// ── Controllers + Swagger ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LMS API", Version = "v1" });
});
```

**Alternative: Use extension methods** to keep Program.cs clean:

```csharp
// In Services layer: DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<ICourseService, CourseService>();
        // ... etc
        services.AddAutoMapper(typeof(SemesterMappingProfile).Assembly);
        return services;
    }
}

// In Repositories layer: DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LmsDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        // ... etc
        return services;
    }
}

// In API's Program.cs:
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddApplicationServices();
```

---

## 8. Entity Relationship Model

```
┌───────────────┐       ┌──────────────────┐       ┌───────────────┐
│   Semester    │       │     Course       │       │   Subject     │
│───────────────│       │──────────────────│       │───────────────│
│ Id (PK)       │──┐    │ Id (PK)          │    ┌──│ Id (PK)       │
│ Code          │  │    │ Code             │    │  │ Code          │
│ Name          │  │    │ Name             │    │  │ Name          │
│ StartDate     │  │    │ Credits          │    │  │ Description   │
│ EndDate       │  │    │ SemesterId (FK)  │◄───┘  │ Department    │
│ Status        │  │    │ SubjectId (FK)   │◄───────│ IsActive      │
│ IsDeleted     │  └──► │ IsActive         │       └───────────────┘
│ CreatedAt     │       │ Capacity         │
└───────────────┘       │ EnrolledCount    │
                        └───────┬──────────┘
                                │
                                │ 1:N
                                │
                        ┌───────▼──────────┐       ┌───────────────┐
                        │   Enrollment     │       │   Student     │
                        │──────────────────│       │───────────────│
                        │ Id (PK)          │    ┌──│ Id (PK)       │
                        │ StudentId (FK)   │◄───┘  │ FirstName     │
                        │ CourseId (FK)    │◄──┐   │ LastName      │
                        │ EnrollmentDate   │   │   │ Email         │
                        │ Grade (nullable) │   │   │ Phone         │
                        │ Status           │   │   │ DateOfBirth   │
                        └──────────────────┘   │   │ Address       │
                                                │   │ IsActive      │
                                                │   └───────────────┘
                                                │
                                                └─── (Course is the
                                                      other FK side)
```

### Entity Configuration (Fluent API)

All entity configurations belong in `Repositories/Data/Configurations/`. Example:

```csharp
// Repositories/Data/Configurations/EnrollmentConfiguration.cs
public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EnrollmentDate).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Grade).HasMaxLength(2);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.Enrollments)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
               .WithMany(c => c.Enrollments)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
```

Applied in `DbContext.OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new SemesterConfiguration());
    modelBuilder.ApplyConfiguration(new CourseConfiguration());
    modelBuilder.ApplyConfiguration(new SubjectConfiguration());
    modelBuilder.ApplyConfiguration(new StudentConfiguration());
    modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());
}
```

---

## 9. Query Features Architecture

The API must support: **search, sort, paging, field selection, and expansion**.

### 9.1 Query Parameters Model (in Services/Models/Request)

```csharp
// Services/Models/Request/QueryParameters.cs
public class QueryParameters
{
    // ── Paging ──
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // ── Search ──
    public string? Search { get; set; }

    // ── Sort ──
    public string? SortBy { get; set; }        // e.g., "code"
    public bool SortDescending { get; set; }   // default ascending

    // ── Field Selection ──
    public string? Fields { get; set; }        // comma-separated: "id,code,name"

    // ── Expansion ──
    public string? Expand { get; set; }        // comma-separated: "courses,subject"
}
```

### 9.2 PagedResult Model (in Services/Models/Response)

```csharp
// Services/Models/Response/PagedResult.cs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### 9.3 Where Query Features Live

| Query Feature | Responsibility Layer | Implementation |
|--------------|---------------------|----------------|
| **Paging** (Skip/Take) | Services layer | Applied on `IQueryable<T>` returned from repository |
| **Search** (Where) | Services layer | Applied on `IQueryable<T>` before paging |
| **Sort** (OrderBy) | Services layer | Applied on `IQueryable<T>` before paging |
| **Field Selection** | API layer → Services | `Expand` param passed through; field selection applied in AutoMapper or via Expression |
| **Expansion** (Include) | Repositories layer | Repository returns `IQueryable<T>` with `.Include()` for requested navigation properties |

**Why expansion is in repos:** Only the repository knows how to `.Include()` navigation properties. The service passes the `Expand` parameter to the repository.

```csharp
// Repository
public IQueryable<Course> GetQueryableWithIncludes(string? expand)
{
    var query = _dbSet.AsQueryable();
    if (expand?.Contains("subject", StringComparison.OrdinalIgnoreCase) == true)
        query = query.Include(c => c.Subject);
    if (expand?.Contains("semester", StringComparison.OrdinalIgnoreCase) == true)
        query = query.Include(c => c.Semester);
    if (expand?.Contains("enrollments", StringComparison.OrdinalIgnoreCase) == true)
        query = query.Include(c => c.Enrollments);
    return query;
}
```

---

## 10. Build Order & Phase Dependencies

### Phase 1: Repositories Layer (Foundation)

**Why first:** Everything depends on it. Must be built before anything else.

| Step | Task | Depends On |
|------|------|------------|
| 1.1 | Convert .csproj to `Microsoft.NET.Sdk`, remove Web artifacts | — |
| 1.2 | Create Entity classes (Semester, Course, Subject, Student, Enrollment) | — |
| 1.3 | Create `LmsDbContext` with `DbSet<>` properties | 1.2 |
| 1.4 | Create entity configurations (Fluent API) | 1.2, 1.3 |
| 1.5 | Create `IRepository<T>` generic interface | 1.2 |
| 1.6 | Create `Repository<T>` generic implementation | 1.3, 1.5 |
| 1.7 | Create entity-specific repository interfaces (`ISemesterRepository`, etc.) | 1.2, 1.5 |
| 1.8 | Create entity-specific repository implementations | 1.3, 1.6, 1.7 |
| 1.9 | Add EF Core packages + SQL Server provider | — |
| 1.10 | Create EF migration for initial schema | 1.3, 1.4 |
| 1.11 | Add seed data in migration or DbContext | 1.10 |

### Phase 2: Services Layer (Business Logic)

| Step | Task | Depends On |
|------|------|------------|
| 2.1 | Convert .csproj to `Microsoft.NET.Sdk`, remove Web artifacts | — |
| 2.2 | Add project reference to Repositories | Phase 1 |
| 2.3 | Create Business models (if needed for domain logic) | — |
| 2.4 | Create Request DTOs | — |
| 2.5 | Create Response DTOs | — |
| 2.6 | Create AutoMapper profiles | 2.3, 2.4, 2.5, Phase 1 Entities |
| 2.7 | Create service interfaces (`ISemesterService`, etc.) | 2.4, 2.5 |
| 2.8 | Create `PagedResult<T>` model | 2.5 |
| 2.9 | Implement service classes with business logic | 2.6, 2.7, 2.8, Phase 1 repos |

### Phase 3: API Layer (Presentation)

| Step | Task | Depends On |
|------|------|------------|
| 3.1 | Add project reference to Services | Phase 2 |
| 3.2 | Add `Microsoft.EntityFrameworkCore.Design` (for migrations at runtime) | Phase 1 |
| 3.3 | Wire up DI in Program.cs | 3.1 |
| 3.4 | Create Controllers (one per entity) | 3.3, Phase 2 service interfaces |
| 3.5 | Implement GET by ID with 404 | 3.4 |
| 3.6 | Implement GET collection with search, sort, paging, fields, expand | 3.4 |
| 3.7 | Implement POST (create) | 3.4 |
| 3.8 | Implement PUT (update) | 3.4 |
| 3.9 | Implement DELETE | 3.4 |
| 3.10 | Add consistent response format wrapper | 3.4 |
| 3.11 | Configure Swagger with response type documentation | 3.3 |

### Phase 4: Docker + Deployment

| Step | Task | Depends On |
|------|------|------------|
| 4.1 | Create `docker-compose.yml` with SQL Server + API containers | Phase 3 |
| 4.2 | Create Dockerfile for API | Phase 3 |
| 4.3 | Configure connection string via environment variable | 4.1 |
| 4.4 | Test full stack with Docker Compose | 4.1, 4.2 |

---

## 11. Folder Structure (Final)

```
Lab1/
├── Lab1.sln
├── docker-compose.yml
│
├── PRN232.LAB_1.API/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── PRN232.LAB_1.API.csproj      (Microsoft.NET.Sdk.Web)
│   ├── Controllers/
│   │   ├── SemestersController.cs
│   │   ├── CoursesController.cs
│   │   ├── SubjectsController.cs
│   │   ├── StudentsController.cs
│   │   └── EnrollmentsController.cs
│   ├── Middleware/                    (optional)
│   └── Properties/
│       └── launchSettings.json
│
├── PRN232.LAB_1.Services/
│   ├── PRN232.LAB_1.Services.csproj  (Microsoft.NET.Sdk)
│   ├── Interfaces/
│   │   ├── ISemesterService.cs
│   │   ├── ICourseService.cs
│   │   ├── ISubjectService.cs
│   │   ├── IStudentService.cs
│   │   └── IEnrollmentService.cs
│   ├── Implementations/
│   │   ├── SemesterService.cs
│   │   ├── CourseService.cs
│   │   ├── SubjectService.cs
│   │   ├── StudentService.cs
│   │   └── EnrollmentService.cs
│   ├── Models/
│   │   ├── Business/                  (domain models)
│   │   ├── Request/
│   │   │   ├── SemesterRequest.cs
│   │   │   ├── CourseRequest.cs
│   │   │   ├── SubjectRequest.cs
│   │   │   ├── StudentRequest.cs
│   │   │   ├── EnrollmentRequest.cs
│   │   │   └── QueryParameters.cs
│   │   └── Response/
│   │       ├── SemesterResponse.cs
│   │       ├── CourseResponse.cs
│   │       ├── SubjectResponse.cs
│   │       ├── StudentResponse.cs
│   │       ├── EnrollmentResponse.cs
│   │       └── PagedResult.cs
│   ├── Mapping/
│   │   └── MappingProfile.cs
│   └── DependencyInjection.cs         (extension method for DI registration)
│
├── PRN232.LAB_1.Repositories/
│   ├── PRN232.LAB_1.Repositories.csproj (Microsoft.NET.Sdk)
│   ├── Entities/
│   │   ├── Semester.cs
│   │   ├── Course.cs
│   │   ├── Subject.cs
│   │   ├── Student.cs
│   │   └── Enrollment.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs             (generic)
│   │   ├── ISemesterRepository.cs
│   │   ├── ICourseRepository.cs
│   │   ├── ISubjectRepository.cs
│   │   ├── IStudentRepository.cs
│   │   └── IEnrollmentRepository.cs
│   ├── Implementations/
│   │   ├── Repository.cs              (generic base)
│   │   ├── SemesterRepository.cs
│   │   ├── CourseRepository.cs
│   │   ├── SubjectRepository.cs
│   │   ├── StudentRepository.cs
│   │   └── EnrollmentRepository.cs
│   ├── Data/
│   │   ├── LmsDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── SemesterConfiguration.cs
│   │   │   ├── CourseConfiguration.cs
│   │   │   ├── SubjectConfiguration.cs
│   │   │   ├── StudentConfiguration.cs
│   │   │   └── EnrollmentConfiguration.cs
│   │   └── Seed/
│   │       └── DataSeeder.cs
│   ├── Migrations/
│   └── DependencyInjection.cs         (extension method for DI registration)
```

---

## 12. Consistent Response Envelope

Every API response should follow a consistent shape (requirement API-05):

```csharp
// In Services/Models/Response/
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(List<string> errors, string? message = null) =>
        new() { Success = false, Errors = errors, Message = message };
}
```

Used in controllers:

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById(int id)
{
    var result = await _semesterService.GetByIdAsync(id);
    if (result == null)
        return NotFound(ApiResponse<SemesterResponse>.Fail(
            new List<string> { $"Semester with ID {id} not found." }));
    return Ok(ApiResponse<SemesterResponse>.Ok(result));
}
```

---

## 13. Anti-Patterns to Avoid

### Anti-Pattern 1: Skiping Layers

**What happens:** Controller calls Repository directly (API → Repositories), bypassing Services.

```csharp
// ❌ BAD
public class StudentsController : ControllerBase
{
    private readonly IStudentRepository _repo; // API should not know about repositories
}
```

**Why it's wrong:** Breaks the 3-layer architecture constraint. Business logic leaks into controllers. Changes to data access affect API.

**Instead:** Always go through the service interface.

### Anti-Pattern 2: Business Logic in Repositories

**What happens:** Repository methods contain validation rules, calculations, or orchestration.

**Why it's wrong:** Repositories should only translate between entities and database. Business rules change independently of data persistence.

**Instead:** Keep repositories purely for CRUD and query logic. Business logic lives in Services.

### Anti-Pattern 3: Returning Entities from API

**What happens:** Controller returns `Semester` entity directly as JSON.

```csharp
// ❌ BAD
[HttpGet("{id}")]
public async Task<ActionResult<Semester>> GetById(int id) // Returns entity!
```

**Why it's wrong:** Exposes internal data (e.g., `IsDeleted`, `CreatedAt`). Creates tight coupling between API contract and database schema. Can cause circular reference serialization errors with navigation properties.

**Instead:** Always map to Response DTOs. Entities never leave the Repositories layer boundary.

### Anti-Pattern 4: DbContext Lifetime Outside DI

**What happens:** Repository manually instantiates `DbContext` with `new LmsDbContext()` or accesses `HttpContext` to get scoped instances.

**Why it's wrong:** EF Core manages context lifecycle through DI scoping. Manual instantiation causes connection leaks, stale data tracking, and concurrency issues.

**Instead:** Inject `LmsDbContext` via constructor — ASP.NET Core DI handles scoped lifetime per request.

---

## 14. Consistency Requirements Compliance

| Requirement | Architectural Implementation |
|------------|---------------------------|
| **ARCH-01**: 3-layer separation | Project references enforce compile-time dependency direction; interfaces enforce runtime abstraction; no layer skips another |
| **DATA-01**: Entity models | All entities in Repositories/Entities as POCOs with EF Core attributes |
| **DATA-02**: Business models | Services/Models/Business for domain processing |
| **DATA-03**: Request models | Services/Models/Request with data annotations |
| **DATA-04**: Response models | Services/Models/Response — entities never returned directly |
| **API-01**: RESTful endpoints | Controllers use `[Route("api/[controller]")]` with plural nouns |
| **API-02**: GET by ID + 404 | Service returns null → Controller returns 404 |
| **API-03**: Collection with features | `QueryParameters` → Service applies on `IQueryable` |
| **API-04**: Pagination metadata | `PagedResult<T>` includes page/pageSize/totalCount/totalPages |
| **API-05**: Consistent response | `ApiResponse<T>` envelope wraps all responses |

---

## Sources

- **Microsoft docs — Common web application architectures:** https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures (HIGH confidence — official Microsoft guidance on n-layer patterns)
- **Microsoft docs — Infrastructure persistence layer design:** https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design (HIGH confidence — official guidance on Repository pattern with EF Core)
- **EF Core 8.0 documentation — DbContext lifetime:** https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ (HIGH confidence — DI scoping guidance)
- **Ardalis Clean Architecture template v8:** https://github.com/ardalis/CleanArchitecture (MEDIUM confidence — community standard template, well-known in .NET ecosystem)
- **Codebase audit (2026-05-14):** verified all three projects are `Microsoft.NET.Sdk.Web` with zero project references and duplicate boilerplate (HIGH confidence — direct file reads)
