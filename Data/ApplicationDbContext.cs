using Microsoft.EntityFrameworkCore;
using backtimetracker.Models;
using TrackerAPI.Models;

namespace backtimetracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Activity> Activities { get; set; }

        public DbSet<TimeRecord> TimeRecords { get; set; }

        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Download> Downloads { get; set; }


    }
}
