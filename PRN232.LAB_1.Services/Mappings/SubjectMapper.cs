using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Mappings;

public static class SubjectMapper
{
    public static SubjectResponse ToResponseDto(this Subject entity)
    {
        return new SubjectResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            Credits = entity.Credits
        };
    }

    public static Subject ToEntity(this SubjectRequest request)
    {
        return new Subject
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Credits = request.Credits
        };
    }

    public static void UpdateEntity(this SubjectRequest request, Subject entity)
    {
        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Credits = request.Credits;
    }

    public static SubjectBusiness ToBusinessModel(this Subject entity)
    {
        return new SubjectBusiness
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            Credits = entity.Credits,
            Courses = entity.Courses?.Select(c => c.ToBusinessModel()).ToList() ?? []
        };
    }

    public static Subject ToEntity(this SubjectBusiness model)
    {
        return new Subject
        {
            Id = model.Id,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            Credits = model.Credits
        };
    }

    public static SubjectBusiness ToBusinessModel(this SubjectRequest request)
    {
        return new SubjectBusiness
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Credits = request.Credits
        };
    }

    public static SubjectResponse ToResponseDto(this Subject entity, string[] expand)
    {
        return entity.ToResponseDto(); // Subject has no forward nav to expand
    }

    public static List<SubjectResponse> ToResponseDtoList(this IEnumerable<Subject> entities)
    {
        return entities.Select(e => e.ToResponseDto()).ToList();
    }
}
