using System;
using System.Collections.Generic;

namespace backtimetracker.Models.Task
{
    /// <summary>
    /// دادهٔ ورودی برای ایجاد تسک توسط ادمین:
    /// عنوان، توضیحات، مهلت و لیست کاربران.
    /// </summary>
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public List<string> UserIds { get; set; }
    }
}
