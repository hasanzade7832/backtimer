using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.User
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // اگر می‌خواهید Email و PhoneNumber را اورراید کنید:
        public override string? Email { get; set; }
        public override string? PhoneNumber { get; set; }

        // نقش کاربر
        public string Role { get; set; } = "User";

        // کالکشن تخصیص تسک‌ها؛ رابطهٔ یک‌به‌چند به UserTask
        public ICollection<backtimetracker.Models.Task.UserTask>? UserTasks { get; set; }
    }
}
