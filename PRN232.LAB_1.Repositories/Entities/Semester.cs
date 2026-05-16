using System.Text.Json.Serialization;

namespace PRN232.LAB_1.Repositories.Entities;

public class Semester
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    [JsonIgnore]
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
