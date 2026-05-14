using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PRN232.LAB_1.API.Filters;
using PRN232.LAB_1.Repositories.Data;
using PRN232.LAB_1.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Application Services (repositories + services) ──
builder.Services.AddApplicationServices();

// ── Controllers + Response Envelope Filter + Swagger ──
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResponseEnvelopeFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS REST API",
        Version = "v1",
        Description = "Learning Management System REST API — Lab 1 for PRN232 course. Manages semesters, courses, subjects, students, and enrollments."
    });
});

var app = builder.Build();

// ── Auto-apply migrations in development ──
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware pipeline ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS REST API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
