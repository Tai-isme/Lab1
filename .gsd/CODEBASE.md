# Codebase Map

Generated: 2026-05-21T00:54:43Z | Files: 79 | Described: 0/79
<!-- gsd:codebase-meta {"generatedAt":"2026-05-21T00:54:43Z","fingerprint":"b4229812241899a1f1e05090045fef9805c8d85b","fileCount":79,"truncated":false} -->

### (root)/
- `.dockerignore`
- `.gitignore`
- `AGENTS.md`
- `ARCHITECTURE-COMPLIANCE.md`
- `docker-compose.yml`
- `DOCKER-DEPLOYMENT-AUDIT.md`
- `Lab1.sln`
- `RESPONSE-FORMAT-AUDIT.md`

### PRN232.LAB_1.API/
- `PRN232.LAB_1.API/appsettings.Development.json`
- `PRN232.LAB_1.API/appsettings.json`
- `PRN232.LAB_1.API/Dockerfile`
- `PRN232.LAB_1.API/PRN232.LAB_1.API.csproj`
- `PRN232.LAB_1.API/Program.cs`

### PRN232.LAB_1.API/Attributes/
- `PRN232.LAB_1.API/Attributes/ConditionalJsonPropertyAttribute.cs`

### PRN232.LAB_1.API/Controllers/
- `PRN232.LAB_1.API/Controllers/CourseController.cs`
- `PRN232.LAB_1.API/Controllers/EnrollmentController.cs`
- `PRN232.LAB_1.API/Controllers/SemesterController.cs`
- `PRN232.LAB_1.API/Controllers/StudentController.cs`
- `PRN232.LAB_1.API/Controllers/SubjectController.cs`

### PRN232.LAB_1.API/Filters/
- `PRN232.LAB_1.API/Filters/ResponseEnvelopeFilter.cs`

### PRN232.LAB_1.API/Middleware/
- `PRN232.LAB_1.API/Middleware/ExceptionHandlingMiddleware.cs`

### PRN232.LAB_1.API/Models/
- `PRN232.LAB_1.API/Models/ApiResponse.cs`

### PRN232.LAB_1.API/Properties/
- `PRN232.LAB_1.API/Properties/launchSettings.json`

### PRN232.LAB_1.Repositories/
- `PRN232.LAB_1.Repositories/PRN232.LAB_1.Repositories.csproj`

### PRN232.LAB_1.Repositories/Data/
- `PRN232.LAB_1.Repositories/Data/DataSeeder.cs`
- `PRN232.LAB_1.Repositories/Data/LmsDbContext.cs`
- `PRN232.LAB_1.Repositories/Data/LmsDbContextFactory.cs`

### PRN232.LAB_1.Repositories/Data/Configurations/
- `PRN232.LAB_1.Repositories/Data/Configurations/CourseConfiguration.cs`
- `PRN232.LAB_1.Repositories/Data/Configurations/EnrollmentConfiguration.cs`
- `PRN232.LAB_1.Repositories/Data/Configurations/SemesterConfiguration.cs`
- `PRN232.LAB_1.Repositories/Data/Configurations/StudentConfiguration.cs`
- `PRN232.LAB_1.Repositories/Data/Configurations/SubjectConfiguration.cs`

### PRN232.LAB_1.Repositories/Entities/
- `PRN232.LAB_1.Repositories/Entities/Course.cs`
- `PRN232.LAB_1.Repositories/Entities/Enrollment.cs`
- `PRN232.LAB_1.Repositories/Entities/Semester.cs`
- `PRN232.LAB_1.Repositories/Entities/Student.cs`
- `PRN232.LAB_1.Repositories/Entities/Subject.cs`

### PRN232.LAB_1.Repositories/Migrations/
- `PRN232.LAB_1.Repositories/Migrations/20260514023828_InitialCreate.cs`
- `PRN232.LAB_1.Repositories/Migrations/20260514023828_InitialCreate.Designer.cs`
- `PRN232.LAB_1.Repositories/Migrations/LmsDbContextModelSnapshot.cs`

### PRN232.LAB_1.Repositories/Repositories/
- `PRN232.LAB_1.Repositories/Repositories/IRepository.cs`
- `PRN232.LAB_1.Repositories/Repositories/Repository.cs`

### PRN232.LAB_1.Services/
- `PRN232.LAB_1.Services/DependencyInjection.cs`
- `PRN232.LAB_1.Services/PRN232.LAB_1.Services.csproj`

### PRN232.LAB_1.Services/Helpers/
- `PRN232.LAB_1.Services/Helpers/QueryableExtensions.cs`

### PRN232.LAB_1.Services/Interfaces/
- `PRN232.LAB_1.Services/Interfaces/ICourseService.cs`
- `PRN232.LAB_1.Services/Interfaces/IEnrollmentService.cs`
- `PRN232.LAB_1.Services/Interfaces/ISemesterService.cs`
- `PRN232.LAB_1.Services/Interfaces/IStudentService.cs`
- `PRN232.LAB_1.Services/Interfaces/ISubjectService.cs`
- `PRN232.LAB_1.Services/Interfaces/IUnitOfWork.cs`

### PRN232.LAB_1.Services/Mappings/
- `PRN232.LAB_1.Services/Mappings/CourseMapper.cs`
- `PRN232.LAB_1.Services/Mappings/EnrollmentMapper.cs`
- `PRN232.LAB_1.Services/Mappings/SemesterMapper.cs`
- `PRN232.LAB_1.Services/Mappings/StudentMapper.cs`
- `PRN232.LAB_1.Services/Mappings/SubjectMapper.cs`

### PRN232.LAB_1.Services/Models/
- `PRN232.LAB_1.Services/Models/CourseBusiness.cs`
- `PRN232.LAB_1.Services/Models/CourseRequest.cs`
- `PRN232.LAB_1.Services/Models/CourseResponse.cs`
- `PRN232.LAB_1.Services/Models/EnrollmentBusiness.cs`
- `PRN232.LAB_1.Services/Models/EnrollmentRequest.cs`
- `PRN232.LAB_1.Services/Models/EnrollmentResponse.cs`
- `PRN232.LAB_1.Services/Models/PagedQuery.cs`
- `PRN232.LAB_1.Services/Models/PagedResult.cs`
- `PRN232.LAB_1.Services/Models/SemesterBusiness.cs`
- `PRN232.LAB_1.Services/Models/SemesterRequest.cs`
- `PRN232.LAB_1.Services/Models/SemesterResponse.cs`
- `PRN232.LAB_1.Services/Models/StudentBusiness.cs`
- `PRN232.LAB_1.Services/Models/StudentRequest.cs`
- `PRN232.LAB_1.Services/Models/StudentResponse.cs`
- `PRN232.LAB_1.Services/Models/SubjectBusiness.cs`
- `PRN232.LAB_1.Services/Models/SubjectRequest.cs`
- `PRN232.LAB_1.Services/Models/SubjectResponse.cs`

### PRN232.LAB_1.Services/Services/
- `PRN232.LAB_1.Services/Services/CourseService.cs`
- `PRN232.LAB_1.Services/Services/EnrollmentService.cs`
- `PRN232.LAB_1.Services/Services/SemesterService.cs`
- `PRN232.LAB_1.Services/Services/StudentService.cs`
- `PRN232.LAB_1.Services/Services/SubjectService.cs`
- `PRN232.LAB_1.Services/Services/UnitOfWork.cs`
