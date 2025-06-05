using backtimetracker.Models.Activities;
using backtimetracker.Models.Internet;
using backtimetracker.Models.Task;
using backtimetracker.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // جدول‌های قبلی:
        public DbSet<Activity> Activities { get; set; }
        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Download> Downloads { get; set; }

        // جدول‌های جدید برای مدیریت تسک:
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // پیکربندی رابطهٔ TaskItem ↔ UserTask:
            builder.Entity<TaskItem>()
                .HasMany(t => t.UserTasks)
                .WithOne(ut => ut.TaskItem)
                .HasForeignKey(ut => ut.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // پیکربندی رابطهٔ ApplicationUser ↔ UserTask:
            builder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
