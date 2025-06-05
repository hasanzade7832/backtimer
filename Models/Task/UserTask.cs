using System;
using System.ComponentModel.DataAnnotations;
using backtimetracker.Models.User;

namespace backtimetracker.Models.Task
{
    public class UserTask
    {
        public int Id { get; set; }

        #region ارتباط با تسک اصلی (TaskItem)
        [Required]
        public int TaskItemId { get; set; }

        public TaskItem TaskItem { get; set; }
        #endregion

        #region ارتباط با کاربر (ApplicationUser)
        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
        #endregion

        #region وضعیت تسک برای کاربر
        /// <summary>
        /// آیا کاربر تسک را تکمیل کرده است؟
        /// </summary>
        public bool IsCompletedByUser { get; set; }

        /// <summary>
        /// آیا ادمین تکمیل‌شدن کاربر را تایید کرده؟
        /// </summary>
        public bool IsConfirmedByAdmin { get; set; }

        /// <summary>
        /// تاریخ و زمان انجام‌شدن تسک توسط کاربر
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// تاریخ و زمان تایید انجام‌شدن تسک توسط ادمین
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }
        #endregion

        #region علامت‌گذاری اعلان‌ها (Seen Flags)
        /// <summary>
        /// آیا پیام اعلان مربوط به این تسک را کاربر دیده است؟
        /// </summary>
        public bool IsSeenByUser { get; set; }

        /// <summary>
        /// آیا پیام اعلان مربوط به این تسک را ادمین دیده است؟
        /// </summary>
        public bool IsSeenByAdmin { get; set; }
        #endregion

        #region درصد انجام‌شده توسط کاربر
        /// <summary>
        /// درصد پیشرفت یا تکمیل کار توسط کاربر (از 0 تا 100)
        /// </summary>
        [Range(0, 100, ErrorMessage = "درصد انجام‌شده باید بین 0 تا 100 باشد.")]
        public int PercentComplete { get; set; } = 0;
        #endregion
    }
}
