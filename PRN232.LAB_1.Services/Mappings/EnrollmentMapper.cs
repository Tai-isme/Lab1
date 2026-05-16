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

    public static EnrollmentResponse ToResponseDto(this Enrollment entity, string[] expand)
    {
        var dto = entity.ToResponseDto();
        if (expand.Contains("student") && entity.Student != null)
            dto.Student = entity.Student.ToResponseDto();
        if (expand.Contains("course") && entity.Course != null)
            dto.Course = entity.Course.ToResponseDto();
        return dto;
    }

    public static EnrollmentResponse ToResponseDto(this EnrollmentBusiness model)
    {
        return new EnrollmentResponse
        {
            Id = model.Id,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollmentDate = model.EnrollmentDate,
            Status = model.Status,
            Grade = model.Grade
        };
    }

    public static EnrollmentResponse ToResponseDto(this EnrollmentBusiness model, string[] expand)
    {
        var dto = model.ToResponseDto();
        if (expand.Contains("student") && model.Student != null)
            dto.Student = model.Student.ToResponseDto();
        if (expand.Contains("course") && model.Course != null)
            dto.Course = model.Course.ToResponseDto();
        return dto;
    }

    public static List<EnrollmentResponse> ToResponseDtoList(this IEnumerable<EnrollmentBusiness> models, string[] expand)
    {
        return models.Select(m => m.ToResponseDto(expand)).ToList();
    }

    public static List<EnrollmentResponse> ToResponseDtoList(this IEnumerable<EnrollmentBusiness> models)
    {
        return models.Select(m => m.ToResponseDto()).ToList();
    }

    public static List<EnrollmentResponse> ToResponseDtoList(this IEnumerable<Enrollment> entities, string[] expand)
    {
        return entities.Select(e => e.ToResponseDto(expand)).ToList();
    }

    public static List<EnrollmentResponse> ToResponseDtoList(this IEnumerable<Enrollment> entities)
    {
        return entities.Select(e => e.ToResponseDto()).ToList();
    }
}
