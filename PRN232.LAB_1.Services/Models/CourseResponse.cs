namespace PRN232.LAB_1.Services.Models;

/// <summary>
/// Response DTO for a Course resource.
/// </summary>
public class CourseResponse
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Course code (e.g., PRN232).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Foreign key to the subject.</summary>
    public int SubjectId { get; set; }

    /// <summary>Foreign key to the semester.</summary>
    public int SemesterId { get; set; }

    /// <summary>Instructor name.</summary>
    public string Instructor { get; set; } = string.Empty;

    /// <summary>Room location.</summary>
    public string Room { get; set; } = string.Empty;

    /// <summary>Maximum number of students allowed.</summary>
    public int MaxStudents { get; set; }

    /// <summary>Schedule description (e.g., Mon-Wed 13:00-15:00).</summary>
    public string Schedule { get; set; } = string.Empty;

    /// <summary>Expanded subject data (available when ?expand=subject).</summary>
    public SubjectResponse? Subject { get; set; }

    /// <summary>Expanded semester data (available when ?expand=semester).</summary>
    public SemesterResponse? Semester { get; set; }
}
