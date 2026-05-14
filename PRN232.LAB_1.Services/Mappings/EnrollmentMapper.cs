using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Mappings;

public static class EnrollmentMapper
{
    public static EnrollmentResponse ToResponseDto(this Enrollment entity)
    {
        return new EnrollmentResponse
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            CourseId = entity.CourseId,
            EnrollmentDate = entity.EnrollmentDate,
            Status = entity.Status,
            Grade = entity.Grade
        };
    }

    public static Enrollment ToEntity(this EnrollmentRequest request)
    {
        return new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollmentDate = request.EnrollmentDate,
            Status = request.Status,
            Grade = request.Grade
        };
    }

    public static void UpdateEntity(this EnrollmentRequest request, Enrollment entity)
    {
        entity.StudentId = request.StudentId;
        entity.CourseId = request.CourseId;
        entity.EnrollmentDate = request.EnrollmentDate;
        entity.Status = request.Status;
        entity.Grade = request.Grade;
    }

    public static EnrollmentBusiness ToBusinessModel(this Enrollment entity)
    {
        return new EnrollmentBusiness
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            CourseId = entity.CourseId,
            EnrollmentDate = entity.EnrollmentDate,
            Status = entity.Status,
            Grade = entity.Grade,
            Student = entity.Student?.ToBusinessModel(),
            Course = entity.Course?.ToBusinessModel()
        };
    }

    public static Enrollment ToEntity(this EnrollmentBusiness model)
    {
        return new Enrollment
        {
            Id = model.Id,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollmentDate = model.EnrollmentDate,
            Status = model.Status,
            Grade = model.Grade
        };
    }

    public static EnrollmentBusiness ToBusinessModel(this EnrollmentRequest request)
    {
        return new EnrollmentBusiness
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollmentDate = request.EnrollmentDate,
            Status = request.Status,
            Grade = request.Grade
        };
    }

    public static List<EnrollmentResponse> ToResponseDtoList(this IEnumerable<Enrollment> entities)
    {
        return entities.Select(e => e.ToResponseDto()).ToList();
    }
}
