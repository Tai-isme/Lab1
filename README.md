# PRN232 Lab 1 — LMS REST API

**v1.0 — Hoàn thành**

Hệ thống **Learning Management System (LMS) REST API** được xây dựng với kiến trúc 3 lớp (3-Layer Architecture) trên nền tảng **ASP.NET Core 8** và **Entity Framework Core**, đóng gói hoàn chỉnh bằng **Docker Compose**.

---

## Mục lục

- [Tổng quan](#tổng-quan)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Kiến trúc](#kiến-trúc)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Các thực thể & Quan hệ](#các-thực-thể--quan-hệ)
- [API Endpoints](#api-endpoints)
- [Truy vấn nâng cao](#truy-vấn-nâng-cao)
- [Cài đặt & Chạy](#cài-đặt--chạy)
- [Docker](#docker)
- [Seed Data](#seed-data)
- [Kiểm tra API](#kiểm-tra-api)
- [Yêu cầu đã đạt](#yêu-cầu-đã-đạt)

---

## Tổng quan

Dự án cung cấp **RESTful CRUD** cho **5 thực thể học thuật**:

| # | Thực thể | Mô tả |
|---|----------|-------|
| 1 | **Student** | Quản lý sinh viên |
| 2 | **Course** | Quản lý khóa học |
| 3 | **Subject** | Quản lý môn học |
| 4 | **Semester** | Quản lý học kỳ |
| 5 | **Enrollment** | Quản lý đăng ký khóa học |

Mỗi endpoint đều hỗ trợ đầy đủ: **tìm kiếm**, **sắp xếp đa trường**, **phân trang**, **chọn trường**, và **mở rộng dữ liệu liên quan**.

- **Môn học:** PRN232 — REST API Design
- **GitHub:** [https://github.com/Tai-isme/Lab1.git](https://github.com/Tai-isme/Lab1.git)

---

## Công nghệ sử dụng

| Thành phần | Công nghệ | Phiên bản |
|------------|-----------|-----------|
| Runtime | .NET / ASP.NET Core | 8.0 |
| Ngôn ngữ | C# | 12 |
| ORM | Entity Framework Core | 8.0.11 |
| Database | SQL Server | 2022 (Docker) |
| API Documentation | Swagger (Swashbuckle) | 6.6.2 |
| Serialization | System.Text.Json | Built-in |
| Mapping | Manual (Extension Methods) | — |
| Validation | Data Annotations | Built-in |
| Containerization | Docker / Docker Compose | v2 |

### NuGet Packages

**PRN232.LAB_1.API**
- `Swashbuckle.AspNetCore` 6.6.2
- `Microsoft.EntityFrameworkCore.Design` 8.0.11

**PRN232.LAB_1.Services**
- `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2

**PRN232.LAB_1.Repositories**
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.11
- `Microsoft.EntityFrameworkCore.Tools` 8.0.11
- `Microsoft.EntityFrameworkCore.Design` 8.0.11

---

## Kiến trúc

### Strict 3-Layer Pattern

```
┌─────────────────────────────────────────────────────┐
│                   API Layer                          │
│    Controllers  →  ApiResponse  →  Middleware        │
│    (Routing, HTTP, Swagger, Response Envelope)       │
├─────────────────────────────────────────────────────┤
│                 Services Layer                        │
│    GenericService<T>  →  Mappers  →  Business Models │
│    (Business Logic, Validation, Orchestration)       │
├─────────────────────────────────────────────────────┤
│               Repositories Layer                     │
│    Repository<T>  →  DbContext  →  Entities          │
│    (EF Core, Data Access, Migrations)                │
└─────────────────────────────────────────────────────┘
│
┌─────────────────────────────────────────────────────┐
│                   SQL Server                          │
│              (Docker Container)                       │
└─────────────────────────────────────────────────────┘
```

### Nguyên tắc kiến trúc

| Nguyên tắc | Mô tả |
|------------|-------|
| **Controllers chứa 0 business logic** | Chỉ gọi service và trả về HTTP response |
| **Repositories chứa 0 business logic** | Chỉ thực hiện CRUD qua EF Core |
| **4 loại Model** | Entity (Repository) → Business (Service) → Request/Response (Service) |
| **Entity không bao giờ trả về từ API** | Luôn map qua Business → Response chain |
| **Request/Response không dùng ở Repository** | Chỉ dùng ở Service Layer |
| **Manual Mapping** | Extension methods tĩnh, không AutoMapper |
| **Unit of Work** | Phối hợp ghi dữ liệu qua nhiều repository với transaction |
| **Response Envelope** | Global filter tự động bọc mọi response trong `ApiResponse<T>` |
| **Exception Handling** | Middleware bắt mọi exception, trả về 500 nhất quán |

### Mô hình dữ liệu qua các tầng

```
Repository Layer           Services Layer            API Layer
┌──────────────┐    ┌──────────────────────┐    ┌──────────────────┐
│  Student     │ →  │  StudentBusiness     │ →  │  StudentResponse  │
│  (Entity)    │    │  StudentRequest      │    │  ApiResponse<T>   │
└──────────────┘    │  StudentResponse     │    └──────────────────┘
                    └──────────────────────┘
```

---

## Cấu trúc thư mục

```
Lab1/
│
├── PRN232.LAB_1.API/                   # API Layer (Web project)
│   ├── Program.cs                      # Composition root (DI, middleware, Swagger)
│   ├── Controllers/                    # 5 REST controllers
│   │   ├── StudentController.cs
│   │   ├── CourseController.cs
│   │   ├── SubjectController.cs
│   │   ├── SemesterController.cs
│   │   └── EnrollmentController.cs
│   ├── Models/
│   │   └── ApiResponse.cs              # Response envelope pattern
│   ├── Filters/
│   │   └── ResponseEnvelopeFilter.cs   # Global response wrapper
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Attributes/
│       └── ConditionalJsonPropertyAttribute.cs
│
├── PRN232.LAB_1.Services/              # Business Logic Layer
│   ├── DependencyInjection.cs          # DI registration extension
│   ├── Interfaces/                     # Service contracts
│   │   ├── IGenericService.cs
│   │   ├── IUnitOfWork.cs
│   │   ├── IStudentService.cs
│   │   ├── ICourseService.cs
│   │   ├── ISubjectService.cs
│   │   ├── ISemesterService.cs
│   │   └── IEnrollmentService.cs
│   ├── Services/                       # Service implementations
│   │   ├── GenericService.cs           # Abstract generic base
│   │   ├── UnitOfWork.cs
│   │   ├── StudentService.cs
│   │   ├── CourseService.cs
│   │   ├── SubjectService.cs
│   │   ├── SemesterService.cs
│   │   └── EnrollmentService.cs
│   ├── Models/                         # Business/Request/Response models
│   │   ├── StudentBusiness.cs
│   │   ├── StudentRequest.cs
│   │   ├── StudentResponse.cs
│   │   ├── (tương tự cho Course, Subject, Semester, Enrollment)
│   │   ├── PagedQuery.cs               # Query params DTO
│   │   └── PagedResult.cs              # Paginated result
│   ├── Mappings/                       # Entity ↔ Business ↔ Request/Response
│   └── Helpers/
│       └── QueryableExtensions.cs      # Sort, field selection helpers
│
├── PRN232.LAB_1.Repositories/          # Data Access Layer
│   ├── Entities/                       # EF Core entity classes
│   ├── Data/
│   │   ├── LmsDbContext.cs
│   │   ├── LmsDbContextFactory.cs
│   │   ├── DataSeeder.cs               # Seeds 500+ records
│   │   └── Configurations/             # Fluent API configs
│   ├── Repositories/
│   │   ├── IRepository.cs              # Generic repository interface
│   │   └── Repository.cs               # EF Core implementation
│   └── Migrations/                     # EF Core migrations
│
├── Lab1.sln
├── docker-compose.yml                  # SQL Server + Adminer + API
├── .gitignore
└── AGENTS.md                           # Agent instructions
```

---

## Các thực thể & Quan hệ

### Student

| Trường | Kiểu | Ràng buộc |
|--------|------|-----------|
| Id | int | PK, auto-increment |
| Code | string | — |
| FullName | string | — |
| Email | string | — |
| Phone | string | — |
| DateOfBirth | DateTime | — |
| Address | string | — |

**Navigation:** `ICollection<Enrollment>` (JsonIgnore)

### Course

| Trường | Kiểu | Ràng buộc |
|--------|------|-----------|
| Id | int | PK, auto-increment |
| Code | string | — |
| SubjectId | int | FK → Subject |
| SemesterId | int | FK → Semester |
| Instructor | string | — |
| Room | string | — |
| MaxStudents | int | — |
| Schedule | string | — |

**Navigation:** `Subject`, `Semester`, `ICollection<Enrollment>` (JsonIgnore)

### Subject

| Trường | Kiểu | Ràng buộc |
|--------|------|-----------|
| Id | int | PK, auto-increment |
| Code | string | — |
| Name | string | — |
| Description | string | — |
| Credits | int | — |

**Navigation:** `ICollection<Course>` (JsonIgnore)

### Semester

| Trường | Kiểu | Ràng buộc |
|--------|------|-----------|
| Id | int | PK, auto-increment |
| Code | string | — |
| Name | string | — |
| StartDate | DateTime | — |
| EndDate | DateTime | — |
| IsActive | bool | — |

**Navigation:** `ICollection<Course>` (JsonIgnore)

### Enrollment

| Trường | Kiểu | Ràng buộc |
|--------|------|-----------|
| Id | int | PK, auto-increment |
| StudentId | int | FK → Student |
| CourseId | int | FK → Course |
| EnrollmentDate | DateTime | — |
| Status | string | — |
| Grade | double? | Nullable |

**Navigation:** `Student`, `Course`

### Sơ đồ quan hệ

```
Semester ──< Course >── Subject
               │
               │
        Enrollment
               │
               │
           Student
```

- Course → Semester: N-1 (Restrict)
- Course → Subject: N-1 (Restrict)
- Enrollment → Student: N-1 (Restrict)
- Enrollment → Course: N-1 (Restrict)

---

## API Endpoints

### Response Envelope

Mọi response đều được bọc trong định dạng nhất quán:

```json
{
  "success": true,
  "message": "OK",
  "data": { ... },
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 50,
    "totalPages": 5
  },
  "errors": null
}
```

### Student — `/api/students`

| Method | Endpoint | Mô tả | Status |
|--------|----------|-------|--------|
| GET | `/api/students` | Danh sách sinh viên (có query) | 200 |
| GET | `/api/students/{id}` | Chi tiết sinh viên | 200 / 404 |
| POST | `/api/students` | Tạo sinh viên mới | 201 / 400 |
| PUT | `/api/students/{id}` | Cập nhật sinh viên | 200 / 404 / 400 |
| DELETE | `/api/students/{id}` | Xóa sinh viên | 200 / 404 |

**Expand:** `enrollments`
**Tìm kiếm:** `search=keyword` (Code, FullName, Email, Phone, Address)
**Sắp xếp:** `sort=id`, `sort=code`, `sort=fullname`, `sort=email`

### Course — `/api/courses`

| Method | Endpoint | Mô tả | Status |
|--------|----------|-------|--------|
| GET | `/api/courses` | Danh sách khóa học | 200 |
| GET | `/api/courses/{id}` | Chi tiết khóa học | 200 / 404 |
| POST | `/api/courses` | Tạo khóa học mới | 201 / 400 |
| PUT | `/api/courses/{id}` | Cập nhật khóa học | 200 / 404 / 400 |
| DELETE | `/api/courses/{id}` | Xóa khóa học | 200 / 404 |

**Expand:** `subject`, `semester`
**Tìm kiếm:** `search=keyword` (Code, Instructor, Room, Schedule)
**Sắp xếp:** `sort=id`, `sort=code`, `sort=instructor`, `sort=room`, `sort=maxstudents`

### Subject — `/api/subjects`

| Method | Endpoint | Mô tả | Status |
|--------|----------|-------|--------|
| GET | `/api/subjects` | Danh sách môn học | 200 |
| GET | `/api/subjects/{id}` | Chi tiết môn học | 200 / 404 |
| POST | `/api/subjects` | Tạo môn học mới | 201 / 400 |
| PUT | `/api/subjects/{id}` | Cập nhật môn học | 200 / 404 / 400 |
| DELETE | `/api/subjects/{id}` | Xóa môn học | 200 / 404 |

**Tìm kiếm:** `search=keyword` (Code, Name, Description)
**Sắp xếp:** `sort=id`, `sort=code`, `sort=name`, `sort=credits`

### Semester — `/api/semesters`

| Method | Endpoint | Mô tả | Status |
|--------|----------|-------|--------|
| GET | `/api/semesters` | Danh sách học kỳ | 200 |
| GET | `/api/semesters/{id}` | Chi tiết học kỳ | 200 / 404 |
| POST | `/api/semesters` | Tạo học kỳ mới | 201 / 400 |
| PUT | `/api/semesters/{id}` | Cập nhật học kỳ | 200 / 404 / 400 |
| DELETE | `/api/semesters/{id}` | Xóa học kỳ | 200 / 404 |

**Tìm kiếm:** `search=keyword` (Code, Name)
**Sắp xếp:** `sort=id`, `sort=code`, `sort=name`, `sort=startdate`, `sort=enddate`

### Enrollment — `/api/enrollments`

| Method | Endpoint | Mô tả | Status |
|--------|----------|-------|--------|
| GET | `/api/enrollments` | Danh sách đăng ký | 200 |
| GET | `/api/enrollments/{id}` | Chi tiết đăng ký | 200 / 404 |
| POST | `/api/enrollments` | Tạo đăng ký mới | 201 / 400 |
| PUT | `/api/enrollments/{id}` | Cập nhật đăng ký | 200 / 404 / 400 |
| DELETE | `/api/enrollments/{id}` | Xóa đăng ký | 200 / 404 |

**Expand:** `student`, `course`
**Tìm kiếm:** `search=keyword` (Status, Grade)
**Sắp xếp:** `sort=id`, `sort=enrollmentdate`, `sort=status`, `sort=grade`

---

## Truy vấn nâng cao

Tất cả endpoint `GET /api/{resource}` đều hỗ trợ các query parameter sau:

### Tìm kiếm (`?search=`)

Tìm kiếm không phân biệt hoa/thường trên nhiều trường:

```
GET /api/students?search=john
GET /api/courses?search=math
```

### Sắp xếp đa trường (`?sort=`)

Sắp xếp tăng dần (mặc định) hoặc giảm dần (tiền tố `-`):

```
GET /api/students?sort=fullname
GET /api/courses?sort=-instructor,code
```

### Phân trang (`?page=` & `?pageSize=`)

Phân trang với giới hạn 1–100 bản ghi/trang:

```
GET /api/students?page=1&pageSize=20
GET /api/enrollments?page=3&pageSize=50
```

### Chọn trường (`?fields=`)

Chỉ lấy các trường mong muốn (luôn bao gồm `id`):

```
GET /api/students?fields=id,fullName,email
GET /api/courses?fields=id,code,instructor,room
```

### Mở rộng dữ liệu liên quan (`?expand=`)

Tải kèm dữ liệu từ bảng liên quan:

```
GET /api/courses?expand=subject,semester
GET /api/enrollments?expand=student,course
GET /api/students/1?expand=enrollments
```

### Kết hợp tất cả

```
GET /api/courses?search=math&sort=-instructor,code&page=1&pageSize=10&fields=id,code,instructor,room&expand=subject,semester
```

---

## Cài đặt & Chạy

### Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (cho Docker mode)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (cho local development, hoặc dùng Docker)

### Chạy Local (Development)

1. **Clone repository:**

```bash
git clone https://github.com/Tai-isme/Lab1.git
cd Lab1
```

2. **Đảm bảo SQL Server đang chạy** (có thể dùng Docker):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Lab1_Pass123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

3. **Chạy ứng dụng:**

```bash
dotnet run --project PRN232.LAB_1.API
```

4. **Truy cập Swagger UI:** [https://localhost:5001/swagger](https://localhost:5001/swagger)

### Chạy với Docker Compose (Khuyến nghị)

```bash
docker compose up -d
```

Lệnh này sẽ khởi động 3 containers:

| Container | Port | Mô tả |
|-----------|------|-------|
| `lms-db` | 1433 | SQL Server 2022 |
| `lms-api` | 5000 | API (map từ port 8080) |
| `lms-adminer` | 8081 | Adminer (quản lý DB qua web) |

**Truy cập:**
- **API:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Adminer:** [http://localhost:8081](http://localhost:8081) (Server: `sqlserver`, User: `sa`, Password: `Lab1_Pass123`, Database: `PRN232_Lab1`)

### Dừng Docker

```bash
docker compose down
```

Xóa cả volume data:

```bash
docker compose down -v
```

---

## Seed Data

Khi ứng dụng khởi động trong môi trường `Development` hoặc `Docker`, **DataSeeder** tự động chạy và tạo dữ liệu mẫu:

| Bảng | Số lượng |
|------|----------|
| Semesters | 5 |
| Subjects | 10 |
| Students | 50 |
| Courses | 20 |
| Enrollments | 500+ |

Seeder sử dụng `Random(42)` để tạo dữ liệu tái lập được (reproducible) và kiểm tra `AnyAsync()` để đảm bảo không chạy trùng lặp.

---

## Kiểm tra API

### Với Swagger UI

Mở [http://localhost:5000/swagger](http://localhost:5000/swagger) để test trực tiếp tất cả endpoints.

### Với curl (ví dụ)

```bash
# Lấy danh sách sinh viên
curl -X GET "http://localhost:5000/api/students?page=1&pageSize=5" -H "accept: application/json"

# Lấy chi tiết khóa học với expand
curl -X GET "http://localhost:5000/api/courses/1?expand=subject,semester" -H "accept: application/json"

# Tạo sinh viên mới
curl -X POST "http://localhost:5000/api/students" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "STU001",
    "fullName": "Nguyen Van A",
    "email": "nguyenvana@example.com",
    "phone": "0123456789",
    "dateOfBirth": "2000-01-15",
    "address": "Ha Noi"
  }'
```

---

## Yêu cầu đã đạt

| Mã | Yêu cầu | Trạng thái |
|----|---------|-----------|
| **SCF-01→05** | 3-layer architecture (API → Services → Repositories) | ✅ |
| **DATA-01→04** | Entity, Business, Request, Response models | ✅ |
| **DATA-05** | Seed data 500+ records | ✅ |
| **API-01→06** | RESTful endpoints cho 5 entities | ✅ |
| **API-02** | GET by ID với expand support | ✅ |
| **API-03** | GET collection với search, sort, page, fields, expand | ✅ |
| **API-04→06** | Consistent response format & HTTP status codes | ✅ |
| **DOCKER-01→03** | Docker Compose (API + SQL Server) | ✅ |
| **SWAGGER-01** | Swagger UI | ✅ |
| **BONUS** | Global exception handling (500 errors) | ✅ |

---

## Giấy phép

Dự án này là bài tập môn PRN232 — REST API Design.
