using System.Text.Json.Serialization;

namespace PRN232.LAB_1.Repositories.Entities;

public class Subject
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Credits { get; set; }

    [JsonIgnore]
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
