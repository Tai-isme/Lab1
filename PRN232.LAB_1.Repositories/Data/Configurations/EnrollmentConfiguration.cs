using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232.LAB_1.Repositories.Entities;

namespace PRN232.LAB_1.Repositories.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.EnrollmentDate).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Grade).HasMaxLength(10);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.Enrollments)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
               .WithMany(c => c.Enrollments)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
