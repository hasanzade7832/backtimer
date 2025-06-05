using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using backtimetracker.Models.Task;

namespace backtimetracker.Models.User
{
    /// <summary>
    /// کلاس ApplicationUser که از IdentityUser مشتق می‌شود.
    /// اینجا می‌توانید فیلدهای دلخواه کاربر (مثل FullName یا CreatedAt) را نیز اضافه کنید.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            // مقداردهی اولیهٔ کالکشن UserTasks تا هیچ‌گاه null نباشد
            UserTasks = new HashSet<UserTask>();
        }

        /// <summary>
        /// نام و نام‌خانوادگی (اختیاری)
        /// </summary>
        [MaxLength(100)]
        public string? FullName { get; set; }

        /// <summary>
        /// تاریخ و زمان ایجاد حساب کاربری
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// اگر خواستید ایمیل یا تلفن را اورراید کنید:
        /// برای مثال برای اضافه کردن اعتبارسنجی یا فرمت متفاوت.
        /// </summary>
        public override string? Email { get; set; }
        public override string? PhoneNumber { get; set; }

        /// <summary>
        /// نقش کاربر (برای نمایش ساده در UI یا منطق دلخواه شما). 
        /// توجه کنید که خود Identity هم نقش (Role) را مدیریت می‌کند.
        /// این فیلد صرفاً یک مقدار کمکی است.
        /// </summary>
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        /// <summary>
        /// مسیر نسبی عکس پروفایل (مثلاً "uploads/{UserId}.jpg")
        /// این فیلد به کمک UploadPhoto در ProfileController پر می‌شود.
        /// </summary>
        public string? PhotoUrl { get; set; }

        /// <summary>
        /// کالکشن تخصیص تسک‌ها به این کاربر
        /// (رابطهٔ یک کاربر ⇆ چند UserTask)
        /// </summary>
        public ICollection<UserTask> UserTasks { get; set; }
    }
}
