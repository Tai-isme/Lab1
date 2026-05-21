using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.API.Filters;
using PRN232.LAB_1.Repositories.Data;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ── Unit of Work ──
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Services ──
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();

// ── Controllers + Swagger ──
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResponseEnvelopeFilter>();
}).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressMapClientErrors = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Auto-apply migrations in development and Docker ──
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(3);
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await db.Database.MigrateAsync();
            break;
        }
        catch when (i < maxRetries - 1)
        {
            await Task.Delay(delay);
        }
    }

    await DataSeeder.SeedAsync(db);
}

// ── Middleware pipeline ──
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
