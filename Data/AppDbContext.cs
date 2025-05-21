using ConsoleProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleProject.Data;

public class AppDbContext : DbContext
{
    public DbSet<Users> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=localhost,1433;Database=ConsoleProject;User Id=sa;Password=YourStrong@Passw0rd;" +
                             "TrustServerCertificate=True;");

    }
}