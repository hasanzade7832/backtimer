using System;

namespace backtimetracker.Models.Task
{
    /// <summary>
    /// دادهٔ ورودی برای ویرایش تسک (عنوان، توضیحات، مهلت)
    /// </summary>
    public class EditTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
    }
}
