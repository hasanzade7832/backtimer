using System;
using System.Collections.Generic;

namespace backtimetracker.Models.Task
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; }            // عنوان تسک
        public string Description { get; set; }      // توضیحات کامل
        public DateTime Deadline { get; set; }       // مهلت اجرا

        public string CreatedByAdminId { get; set; } // شناسهٔ ادمینی که تسک را ساخت
        public DateTime CreatedAt { get; set; }      // زمان ایجاد

        // کالکشن تخصیص‌ها به کاربران
        public ICollection<UserTask> UserTasks { get; set; }
    }
}
