using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232.LAB_1.Repositories.Entities;

namespace PRN232.LAB_1.Repositories.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Instructor).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Room).HasMaxLength(50).IsRequired();
        builder.Property(e => e.MaxStudents).IsRequired();
        builder.Property(e => e.Schedule).HasMaxLength(200).IsRequired();

        builder.HasOne(e => e.Semester)
               .WithMany(s => s.Courses)
               .HasForeignKey(e => e.SemesterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Subject)
               .WithMany(s => s.Courses)
               .HasForeignKey(e => e.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
