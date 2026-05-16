namespace PRN232.LAB_1.Repositories.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Grade { get; set; }

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
