using Microsoft.EntityFrameworkCore;
using TrekingTIme.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TrekingTIme
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkHour> WorkHours { get; set; }
    }
}
