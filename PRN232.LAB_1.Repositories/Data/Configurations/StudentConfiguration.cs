using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN232.LAB_1.Repositories.Entities;

namespace PRN232.LAB_1.Repositories.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(20).IsRequired();
        builder.Property(e => e.DateOfBirth).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(500).IsRequired();
    }
}
