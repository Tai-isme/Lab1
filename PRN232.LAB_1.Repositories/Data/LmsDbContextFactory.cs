using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PRN232.LAB_1.Repositories.Data;

public class LmsDbContextFactory : IDesignTimeDbContextFactory<LmsDbContext>
{
    public LmsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LmsDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=PRN232_Lab1;User Id=sa;Password=Lab1_Pass123;TrustServerCertificate=True;");
        return new LmsDbContext(optionsBuilder.Options);
    }
}
