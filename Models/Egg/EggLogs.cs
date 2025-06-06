// File: Models/Egg/EggLog.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backtimetracker.Models.User;

namespace backtimetracker.Models.Egg
{
    /// <summary>
    /// هر بار که کاربر یک تخم‌مرغ مصرف کرد، یک رکورد در این جدول ایجاد می‌شود.
    /// </summary>
    public class EggLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// کلید خارجی به جدول AspNetUsers (Identity)
        /// </summary>
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// تاریخ شمسی به صورت رشته (مثلاً "۱۴۰۲/۰۳/۲۵")
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Date { get; set; }

        /// <summary>
        /// زمان دقیق UTC ثبت لاگ (اختیاری)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
