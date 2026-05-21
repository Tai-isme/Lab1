using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class StudentService
    : GenericService<Student, StudentBusiness, StudentRequest, StudentResponse>,
      IStudentService
{
    public StudentService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    // ── Mapping hooks ─────────────────────────────────────────────────────────

    protected override StudentBusiness MapToBusiness(Student entity)
        => entity.ToBusinessModel();

    protected override StudentResponse MapToResponse(StudentBusiness business, string[] expand)
        => business.ToResponseDto();   // StudentBusiness carries all needed data; expand ignored at response level

    protected override Student MapToEntity(StudentRequest request)
        => request.ToEntity();

    protected override void UpdateEntityFromRequest(StudentRequest request, Student entity)
        => request.UpdateEntity(entity);

    // ── Search hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Student> ApplySearch(IQueryable<Student> q, string search)
    {
        var s = search.ToLower();
        return q.Where(e => e.Code.ToLower().Contains(s)
                         || e.FullName.ToLower().Contains(s)
                         || e.Email.ToLower().Contains(s)
                         || e.Phone.ToLower().Contains(s)
                         || e.Address.ToLower().Contains(s));
    }

    // ── Expand hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Student> ApplyExpand(IQueryable<Student> q, string[] expand)
    {
        if (expand.Contains("enrollments"))
            q = q.Include(e => e.Enrollments);
        return q;
    }

    // ── Sort hook ─────────────────────────────────────────────────────────────

    protected override Dictionary<string, (Func<IQueryable<Student>, IOrderedQueryable<Student>> Asc,
                                           Func<IQueryable<Student>, IOrderedQueryable<Student>> Desc)>
        GetSortFields() => new()
        {
            { "id",       (q => q.OrderBy(e => e.Id),       q => q.OrderByDescending(e => e.Id)) },
            { "code",     (q => q.OrderBy(e => e.Code),     q => q.OrderByDescending(e => e.Code)) },
            { "fullname", (q => q.OrderBy(e => e.FullName), q => q.OrderByDescending(e => e.FullName)) },
            { "email",    (q => q.OrderBy(e => e.Email),    q => q.OrderByDescending(e => e.Email)) },
        };
}
