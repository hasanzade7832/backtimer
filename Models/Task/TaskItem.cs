using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.Task
{
    public class TaskItem
    {
        public TaskItem()
        {
            // مقداردهی اولیهٔ کالکشن تخصیص‌ها به کاربران 
            UserTasks = new HashSet<UserTask>();

            // مقداردهی خودکارِ CreatedAt هنگام ساخت شیء
            CreatedAt = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان تسک الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان تسک نمی‌تواند بیشتر از 200 کاراکتر باشد.")]
        public string Title { get; set; }            // عنوان تسک

        [Required(ErrorMessage = "توضیحات تسک الزامی است.")]
        public string Description { get; set; }      // توضیحات کامل

        [Required(ErrorMessage = "مهلت اجرا (Deadline) الزامی است.")]
        public DateTime Deadline { get; set; }       // مهلت اجرا

        // شناسهٔ ادمینی که تسک را ساخت
        [Required]
        public string CreatedByAdminId { get; set; }

        // زمان ایجاد (به صورت خودکار در سازنده مقداردهی می‌شود)
        public DateTime CreatedAt { get; set; }

        // کالکشن تخصیص‌ها به کاربران
        public ICollection<UserTask> UserTasks { get; set; }
    }
}
