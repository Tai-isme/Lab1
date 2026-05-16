using System.Text.Json.Serialization;

namespace PRN232.LAB_1.Repositories.Entities;

public class Student
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
