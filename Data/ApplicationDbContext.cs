using System.Collections.Generic;
using System.Reflection.Emit;
using backtimetracker.Models.Activities;
using backtimetracker.Models.Internet;
using backtimetracker.Models.PettyCashes;
using backtimetracker.Models.Task;
using backtimetracker.Models.User;
using backtimetracker.Models.Egg;           // برای EggLog


// اضافه کردن فضای نام مربوط به مدل‌های PettyCash و Expense

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Data
{
    /// <summary>
    /// کانتکست اصلی EF Core که از IdentityDbContext مشتق می‌شود.
    /// در این کلاس، تمام DbSetهای پروژه (از جمله جدول‌های تسک و PettyCash/Expense) تعریف شده‌اند.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region جدول‌های مربوط به ماژول‌های قبلی پروژه

        /// <summary>
        /// جداول مربوط به ماژول Activities
        /// </summary>
        public DbSet<Activity> Activities { get; set; }
        public DbSet<TimeRecord> TimeRecords { get; set; }

        /// <summary>
        /// جداول مربوط به ماژول Internet (خرید و دانلود)
        /// </summary>
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Download> Downloads { get; set; }
        public DbSet<EggLog> EggLogs { get; set; }
        #endregion

        #region جدول‌های مربوط به ماژول تسک (Task)

        /// <summary>
        /// TaskItem: نمایندهٔ یک تسک عمومی
        /// UserTask: نگاشت تسک به کاربر (کاربر/تاریخ/وضعیت و …)
        /// </summary>
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }

        #endregion

        #region جدول‌های جدید برای مدیریت هزینه و تنخواه

        /// <summary>
        /// Expense: نمایندهٔ یک هزینهٔ منفرد (وضعیت جاری یا آرشیو شده)
        /// PettyCash: نمایندهٔ یک دستهٔ هزینه‌های آرشیوشده (تنخواه)
        /// </summary>
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<PettyCash> PettyCashes { get; set; }

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

            // ───────────── جدول‌های PettyCash/Expense ─────────────

            // ۱) تنظیم رابطهٔ PettyCash ↔ Expense (یک PettyCash می‌تواند چند Expense داشته باشد)
            builder.Entity<PettyCash>()
                .HasMany(pc => pc.Expenses)
                .WithOne(e => e.PettyCash)
                .HasForeignKey(e => e.PettyCashId)
                .OnDelete(DeleteBehavior.Cascade);

            // ۲) تنظیم رفتار حذف Cascade:
            //    اگر یک رکورد PettyCash حذف شود، همهٔ هزینه‌های (Expense) مرتبط با آن نیز حذف خواهند شد.

            // ۳) (اختیاری) می‌توانید ایندکس یا محدودیت بیشتری بگذارید؛ برای مثال:
            //    builder.Entity<Expense>()
            //        .HasIndex(e => new { e.PettyCashId, e.Date });
        }
    }
}
