// File: backtimetracker/Models/Task/EditTaskDto.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.Task
{
    /// <summary>
    /// دادهٔ ورودی برای ویرایش تسک:
    /// شامل عنوان، توضیحات، مهلت و فهرست شناسه‌های کاربران اختصاصی‌شده.
    /// </summary>
    public class EditTaskDto
    {
        [Required(ErrorMessage = "عنوان تسک الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان تسک نمی‌تواند بیشتر از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات تسک الزامی است.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "مهلت اجرا (Deadline) الزامی است.")]
        public DateTime Deadline { get; set; }

        /// <summary>
        /// لیست شناسهٔ کاربران (ApplicationUser.Id) که باید
        /// این تسک به آنها اختصاص یابد (به‌روز رسانی‌شده).
        /// اگر این لیست پر باشد، کاربرانی که اکنون در لیست نیستند حذف می‌شوند
        /// و کاربران جدید اضافه خواهند شد.
        /// </summary>
        [Required(ErrorMessage = "باید حداقل یک کاربر را انتخاب کنید.")]
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
