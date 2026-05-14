using PRN232.LAB_1.Repositories.Entities;
using PRN232.LAB_1.Services.Models;

namespace PRN232.LAB_1.Services.Mappings;

public static class StudentMapper
{
    public static StudentResponse ToResponseDto(this Student entity)
    {
        return new StudentResponse
        {
            Id = entity.Id,
            Code = entity.Code,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            DateOfBirth = entity.DateOfBirth,
            Address = entity.Address
        };
    }

    public static Student ToEntity(this StudentRequest request)
    {
        return new Student
        {
            Code = request.Code,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address
        };
    }

    public static void UpdateEntity(this StudentRequest request, Student entity)
    {
        entity.Code = request.Code;
        entity.FullName = request.FullName;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.DateOfBirth = request.DateOfBirth;
        entity.Address = request.Address;
    }

    public static StudentBusiness ToBusinessModel(this Student entity)
    {
        return new StudentBusiness
        {
            Id = entity.Id,
            Code = entity.Code,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            DateOfBirth = entity.DateOfBirth,
            Address = entity.Address,
            Enrollments = entity.Enrollments?.Select(e => e.ToBusinessModel()).ToList() ?? []
        };
    }

    public static Student ToEntity(this StudentBusiness model)
    {
        return new Student
        {
            Id = model.Id,
            Code = model.Code,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            DateOfBirth = model.DateOfBirth,
            Address = model.Address
        };
    }

    public static StudentBusiness ToBusinessModel(this StudentRequest request)
    {
        return new StudentBusiness
        {
            Code = request.Code,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address
        };
    }

    public static StudentResponse ToResponseDto(this Student entity, string[] expand)
    {
        return entity.ToResponseDto(); // Student has no forward nav to expand
    }

    public static List<StudentResponse> ToResponseDtoList(this IEnumerable<Student> entities)
    {
        return entities.Select(e => e.ToResponseDto()).ToList();
    }
}
