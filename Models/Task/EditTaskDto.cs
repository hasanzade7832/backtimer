using System;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.Task
{
    /// <summary>
    /// دادهٔ ورودی برای ویرایش تسک (عنوان، توضیحات، مهلت)
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
    }
}
