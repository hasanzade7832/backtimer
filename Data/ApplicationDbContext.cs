using backtimetracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using backtimetracker.Models;

namespace backtimetracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Download> Downloads { get; set; }
    }
}
