using Microsoft.Extensions.DependencyInjection;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Services;

namespace PRN232.LAB_1.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        return services;
    }
}
