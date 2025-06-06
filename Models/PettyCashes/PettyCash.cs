using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.PettyCashes
{
    public class PettyCash
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// هزینه‌های آرشیوشده در این تنخواه
        /// </summary>
        public ICollection<Expense> Expenses { get; set; }
    }
}
