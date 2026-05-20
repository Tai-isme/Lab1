using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;

namespace PRN232.LAB_1.Services.Interfaces;

public interface IUnitOfWork
{
    IRepository<Semester> Semesters { get; }
    IRepository<Course> Courses { get; }
    IRepository<Subject> Subjects { get; }
    IRepository<Student> Students { get; }
    IRepository<Enrollment> Enrollments { get; }

    Task<int> SaveChangesAsync();
}
