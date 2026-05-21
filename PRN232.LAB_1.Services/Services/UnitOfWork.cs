using PRN232.LAB_1.Repositories.Data;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Repositories.Repositories;
using PRN232.LAB_1.Services.Interfaces;

namespace PRN232.LAB_1.Services.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly LmsDbContext _context;
    private IRepository<Semester>? _semesters;
    private IRepository<Course>? _courses;
    private IRepository<Subject>? _subjects;
    private IRepository<Student>? _students;
    private IRepository<Enrollment>? _enrollments;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(LmsDbContext context)
    {
        _context = context;
    }

    public IRepository<Semester> Semesters => _semesters ??= new Repository<Semester>(_context);
    public IRepository<Course> Courses => _courses ??= new Repository<Course>(_context);
    public IRepository<Subject> Subjects => _subjects ??= new Repository<Subject>(_context);
    public IRepository<Student> Students => _students ??= new Repository<Student>(_context);
    public IRepository<Enrollment> Enrollments => _enrollments ??= new Repository<Enrollment>(_context);

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new Repository<TEntity>(_context);
            _repositories[type] = repo;
        }
        return (IRepository<TEntity>)repo;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
