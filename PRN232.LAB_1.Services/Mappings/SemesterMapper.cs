using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Mappings;

public static class SemesterMapper
{
    public static SemesterResponse ToResponseDto(this Semester entity)
    {
        return new SemesterResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive
        };
    }

    public static Semester ToEntity(this SemesterRequest request)
    {
        return new Semester
        {
            Code = request.Code,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };
    }

    public static void UpdateEntity(this SemesterRequest request, Semester entity)
    {
        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.IsActive = request.IsActive;
    }

    public static SemesterBusiness ToBusinessModel(this Semester entity)
    {
        return new SemesterBusiness
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            Courses = entity.Courses?.Select(c => c.ToBusinessModel()).ToList() ?? []
        };
    }

    public static Semester ToEntity(this SemesterBusiness model)
    {
        return new Semester
        {
            Id = model.Id,
            Code = model.Code,
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive
        };
    }

    public static SemesterBusiness ToBusinessModel(this SemesterRequest request)
    {
        return new SemesterBusiness
        {
            Code = request.Code,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };
    }

    public static SemesterResponse ToResponseDto(this Semester entity, string[] expand)
    {
        return entity.ToResponseDto(); // Semester has no forward nav to expand
    }

    public static List<SemesterResponse> ToResponseDtoList(this IEnumerable<Semester> entities)
    {
        return entities.Select(e => e.ToResponseDto()).ToList();
    }
}
