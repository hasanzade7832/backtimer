using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.Task
{
    /// <summary>
    /// دادهٔ ورودی برای ایجاد تسک توسط ادمین:
    /// عنوان، توضیحات، مهلت و لیست کاربران.
    /// </summary>
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "عنوان تسک الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان تسک نمی‌تواند بیشتر از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات تسک الزامی است.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "مهلت اجرا (Deadline) الزامی است.")]
        public DateTime Deadline { get; set; }

        /// <summary>
        /// شناسه‌های کاربران (ApplicationUser.Id) که قرار است تسک به آنها اختصاص یابد.
        /// </summary>
        [Required(ErrorMessage = "باید حداقل یک کاربر برای اختصاص تسک انتخاب شود.")]
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
