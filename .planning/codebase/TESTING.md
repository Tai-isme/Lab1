---
title: Testing Strategy
created: 2026-05-15
focus: quality
---

# Testing Strategy

## Test Framework

**Not configured.** No test projects exist in the solution. The `.sln` file (`Lab1.sln`) contains only three projects:

- `PRN232.LAB_1.API` — ASP.NET Core Web API
- `PRN232.LAB_1.Services` — Business logic (class library)
- `PRN232.LAB_1.Repositories` — Data access (class library)

No test SDK packages (xUnit, NUnit, MSTest) are referenced in any `.csproj`.

**No test runner configuration files exist:**
- No `.runsettings`
- No `xunit.runner.json`
- No `Directory.Build.props` with test settings

**No test files exist:**
- No `*.Tests.*` files
- No `*.Test.*` files
- No `*.Spec.*` files
- No `Tests/` or `Test/` directories

## Test Structure

**Not applicable** — no test projects exist.

The expected structure for a .NET 8 solution would be a separate test project (e.g., `PRN232.LAB_1.Tests` or `Lab1.Tests`) in the solution directory:

```
Lab1/
├── PRN232.LAB_1.API/          # System Under Test
├── PRN232.LAB_1.Services/     # System Under Test
├── PRN232.LAB_1.Repositories/ # System Under Test
└── Lab1.Tests/                # Missing — needs creation
    ├── Controllers/
    ├── Services/
    ├── Repositories/
    └── Integration/
```

## Mocking

**Not configured.** No mocking library (Moq, NSubstitute, FakeItEasy, NMock3) is referenced.

If tests are added, the recommended mocking approach based on the architecture:

- **Repository mocking:** Since all services depend on `IRepository<T>`, mock this generic interface
- **Service mocking:** For controller tests, mock `IStudentService`, `ICourseService`, etc.
- **DbContext mocking:** For repository tests, consider using an in-memory SQLite or EF Core InMemory provider rather than mocking `DbSet`

## Coverage

**Not configured.** No coverage tool (Coverlet, dotCover, NCover) is referenced in any project file.

No coverage targets or thresholds are defined.

## Recommendations for Test Implementation

### Project Setup
Create a test project targeting `net8.0`:
```
dotnet new xunit -n Lab1.Tests
dotnet add Lab1.Tests/Lab1.Tests.csproj reference PRN232.LAB_1.API
dotnet add Lab1.Tests/Lab1.Tests.csproj reference PRN232.LAB_1.Services
dotnet add Lab1.Tests/Lab1.Tests.csproj reference PRN232.LAB_1.Repositories
```

Add NuGet packages:
- `xunit` (2.9.x) — test framework
- `xunit.runner.visualstudio` — VS test runner
- `Moq` (4.20.x) — mocking framework
- `FluentAssertions` (7.x) — optional, for fluent assertions
- `Microsoft.EntityFrameworkCore.InMemory` — for repository tests
- `Coverlet.collector` — for code coverage

### What to Test (Priority Order)

1. **Services** (highest value) — Business logic, search/sort/pagination, null handling
   - `StudentService.GetAllAsync(PagedQuery)` — verify search filtering, sorting, paging math
   - `StudentService.GetByIdAsync(int)` — verify null return for missing entity
   - `StudentService.AddAsync(StudentRequest)` — verify entity creation and response mapping
   - `StudentService.UpdateAsync(int, StudentRequest)` — verify update and null for missing
   - `StudentService.DeleteAsync(int)` — verify return true/false

2. **Controllers** (medium value) — Route mapping, status codes, pagination header
   - `StudentController.GetAll` — verify 200 + pagination in HttpContext.Items
   - `StudentController.GetById` — verify 200 for found, 404 for missing
   - `StudentController.Create` — verify 201 + CreatedAtAction
   - `StudentController.Update` — verify 200 for success, 404 for missing
   - `StudentController.Delete` — verify 200 + message, 404 for missing

3. **Mappers** (low effort, high confidence) — Extension method correctness
   - `StudentMapper.ToResponseDto(Student)` — verify all fields map correctly
   - `StudentMapper.ToEntity(StudentRequest)` — verify field mapping
   - `StudentMapper.UpdateEntity(StudentRequest, Student)` — verify mutation

4. **Repositories** (integration-level, with InMemory provider) — EF Core behavior
   - CRUD operations
   - `GetQueryable()` behavior

5. **ResponseEnvelopeFilter** (integration-level) — HTTP response wrapping
   - Verify 200 responses wrapped in `ApiResponse<T>.Ok`
   - Verify 400 responses include validation errors
   - Verify 404 responses use `ApiResponse.Fail`

6. **Validation** (on Request models) — DataAnnotation behavior
   - Verify `[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]` attributes

### Common Test Patterns

**Service unit test pattern (with Moq):**
```csharp
// Arrange
var mockRepo = new Mock<IRepository<Student>>();
mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Student { Id = 1, /* ... */ });
var service = new StudentService(mockRepo.Object);

// Act
var result = await service.GetByIdAsync(1);

// Assert
Assert.NotNull(result);
Assert.Equal(1, result.Id);
```

**Controller unit test pattern (with Moq):**
```csharp
// Arrange
var mockService = new Mock<IStudentService>();
mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new StudentResponse { Id = 1 });
var controller = new StudentController(mockService.Object);

// Act
var result = await controller.GetById(1);

// Assert
var okResult = Assert.IsType<OkObjectResult>(result);
var response = Assert.IsType<StudentResponse>(okResult.Value);
Assert.Equal(1, response.Id);
```

**Pagination search test pattern:**
```csharp
[Fact]
public async Task GetAllAsync_WithSearch_FiltersByName()
{
    var data = new List<Student>
    {
        new() { Code = "STU001", FullName = "Alice", Email = "a@test.com" },
        new() { Code = "STU002", FullName = "Bob", Email = "b@test.com" },
    }.AsQueryable();

    var mockRepo = new Mock<IRepository<Student>>();
    mockRepo.Setup(r => r.GetQueryable()).Returns(data);
    var service = new StudentService(mockRepo.Object);

    var result = await service.GetAllAsync(new PagedQuery { Search = "alice" });

    Assert.Single(result.Items);
    Assert.Equal("Alice", result.Items[0].FullName);
}
```

### Test Architecture Constraints
- **Never mock Entity Framework** — use InMemory provider for repo tests
- **Mapper tests should be pure unit tests** — no dependencies to mock
- **Controller tests should only test HTTP concerns** — route selection, status codes, headers
- **Service tests should verify business logic** — search/filter/sort/paging correctness, null handling
- **Test data should use real model types** — avoid anonymous objects or tuples for assertions

---

*Testing analysis: 2026-05-15*
