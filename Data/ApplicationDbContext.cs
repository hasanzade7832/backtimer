using System.Collections.Generic;
using System.Reflection.Emit;
using backtimetracker.Models.Activities;
using backtimetracker.Models.Internet;
using backtimetracker.Models.Task;
using backtimetracker.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Data
{
    /// <summary>
    /// کانتکست اصلی EF Core که از IdentityDbContext مشتق می‌شود.
    /// در این کلاس، تمام DbSetهای پروژه (از جمله جدول‌های تسک) تعریف شده‌اند.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region جدول‌های مربوط به ماژول‌های قبلی پروژه
        public DbSet<Activity> Activities { get; set; }
        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Download> Downloads { get; set; }
        #endregion

        #region جدول‌های جدید برای مدیریت تسک
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── تنظیم رابطهٔ TaskItem ↔ UserTask (یک TaskItem می‌تواند چند UserTask داشته باشد) ──
            builder.Entity<TaskItem>()
                .HasMany(t => t.UserTasks)
                .WithOne(ut => ut.TaskItem)
                .HasForeignKey(ut => ut.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── تنظیم رابطهٔ ApplicationUser ↔ UserTask (یک کاربر می‌تواند چند UserTask داشته باشد) ──
            builder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // در صورت نیاز، ایندکس یا قوانین دیگری نیز می‌توانید اضافه کنید.
        }
    }
}
