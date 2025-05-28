using Microsoft.EntityFrameworkCore;
using backtimetracker.Models;

namespace backtimetracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Activity> Activities { get; set; }
    }
}
