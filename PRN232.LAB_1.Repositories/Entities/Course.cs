using System.Text.Json.Serialization;

namespace PRN232.LAB_1.Repositories.Entities;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public int SemesterId { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public string Schedule { get; set; } = string.Empty;

    public Semester Semester { get; set; } = null!;
    public Subject Subject { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
