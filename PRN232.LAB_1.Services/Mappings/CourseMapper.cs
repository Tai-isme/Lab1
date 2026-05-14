using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Mappings;

public static class CourseMapper
{
    public static CourseResponse ToResponseDto(this Course entity)
    {
        return new CourseResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            SubjectId = entity.SubjectId,
            SemesterId = entity.SemesterId,
            Instructor = entity.Instructor,
            Room = entity.Room,
            MaxStudents = entity.MaxStudents,
            Schedule = entity.Schedule
        };
    }

    public static Course ToEntity(this CourseRequest request)
    {
        return new Course
        {
            Code = request.Code,
            SubjectId = request.SubjectId,
            SemesterId = request.SemesterId,
            Instructor = request.Instructor,
            Room = request.Room,
            MaxStudents = request.MaxStudents,
            Schedule = request.Schedule
        };
    }

    public static void UpdateEntity(this CourseRequest request, Course entity)
    {
        entity.Code = request.Code;
        entity.SubjectId = request.SubjectId;
        entity.SemesterId = request.SemesterId;
        entity.Instructor = request.Instructor;
        entity.Room = request.Room;
        entity.MaxStudents = request.MaxStudents;
        entity.Schedule = request.Schedule;
    }

    public static CourseBusiness ToBusinessModel(this Course entity)
    {
        return new CourseBusiness
        {
            Id = entity.Id,
            Code = entity.Code,
            SubjectId = entity.SubjectId,
            SemesterId = entity.SemesterId,
            Instructor = entity.Instructor,
            Room = entity.Room,
            MaxStudents = entity.MaxStudents,
            Schedule = entity.Schedule,
            Semester = entity.Semester?.ToBusinessModel(),
            Subject = entity.Subject?.ToBusinessModel(),
            Enrollments = entity.Enrollments?.Select(e => e.ToBusinessModel()).ToList() ?? []
        };
    }

    public static Course ToEntity(this CourseBusiness model)
    {
        return new Course
        {
            Id = model.Id,
            Code = model.Code,
            SubjectId = model.SubjectId,
            SemesterId = model.SemesterId,
            Instructor = model.Instructor,
            Room = model.Room,
            MaxStudents = model.MaxStudents,
            Schedule = model.Schedule
        };
    }

    public static CourseBusiness ToBusinessModel(this CourseRequest request)
    {
        return new CourseBusiness
        {
            Code = request.Code,
            SubjectId = request.SubjectId,
            SemesterId = request.SemesterId,
            Instructor = request.Instructor,
            Room = request.Room,
            MaxStudents = request.MaxStudents,
            Schedule = request.Schedule
        };
    }

    public static List<CourseResponse> ToResponseDtoList(this IEnumerable<Course> entities)
    {
        return entities.Select(e => e.ToResponseDto()).ToList();
    }
}
