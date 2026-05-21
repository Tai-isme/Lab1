using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Interfaces;
using PRN232.LAB_1.Services.Mappings;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Services;

public class CourseService
    : GenericService<Course, CourseBusiness, CourseRequest, CourseResponse>,
      ICourseService
{
    public CourseService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

    // ── Mapping hooks ─────────────────────────────────────────────────────────

    protected override CourseBusiness MapToBusiness(Course entity)
        => entity.ToBusinessModel();

    protected override CourseResponse MapToResponse(CourseBusiness business, string[] expand)
        => business.ToResponseDto(expand);   // CourseMapper has expand-aware overload

    protected override Course MapToEntity(CourseRequest request)
        => request.ToEntity();

    protected override void UpdateEntityFromRequest(CourseRequest request, Course entity)
        => request.UpdateEntity(entity);

    // ── Search hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Course> ApplySearch(IQueryable<Course> q, string search)
    {
        var s = search.ToLower();
        return q.Where(e => e.Code.ToLower().Contains(s)
                         || e.Instructor.ToLower().Contains(s)
                         || e.Room.ToLower().Contains(s)
                         || e.Schedule.ToLower().Contains(s));
    }

    // ── Expand hook ───────────────────────────────────────────────────────────

    protected override IQueryable<Course> ApplyExpand(IQueryable<Course> q, string[] expand)
    {
        if (expand.Contains("subject"))
            q = q.Include(e => e.Subject);
        if (expand.Contains("semester"))
            q = q.Include(e => e.Semester);
        return q;
    }

    // ── Sort hook ─────────────────────────────────────────────────────────────

    protected override Dictionary<string, (Func<IQueryable<Course>, IOrderedQueryable<Course>> Asc,
                                           Func<IQueryable<Course>, IOrderedQueryable<Course>> Desc)>
        GetSortFields() => new()
        {
            { "id",          (q => q.OrderBy(e => e.Id),          q => q.OrderByDescending(e => e.Id)) },
            { "code",        (q => q.OrderBy(e => e.Code),        q => q.OrderByDescending(e => e.Code)) },
            { "instructor",  (q => q.OrderBy(e => e.Instructor),  q => q.OrderByDescending(e => e.Instructor)) },
            { "room",        (q => q.OrderBy(e => e.Room),        q => q.OrderByDescending(e => e.Room)) },
            { "maxstudents", (q => q.OrderBy(e => e.MaxStudents), q => q.OrderByDescending(e => e.MaxStudents)) },
        };
}
