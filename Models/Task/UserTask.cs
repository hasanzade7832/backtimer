using System;
using backtimetracker.Models.User;

namespace backtimetracker.Models.Task
{
    public class UserTask
    {
        public int Id { get; set; }

        // کلید خارجی به تسک اصلی
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        // کلید خارجی به کاربر
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // وضعیت تسک برای کاربر
        public bool IsCompletedByUser { get; set; }
        public bool IsConfirmedByAdmin { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        // برای علامت‌گذاری اعلان‌ها
        public bool IsSeenByUser { get; set; }
        public bool IsSeenByAdmin { get; set; }
    }
}
