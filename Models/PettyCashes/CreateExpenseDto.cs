using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Dtos.PettyCashes
{
    public class CreateExpenseDto
    {
        [Required]
        [MaxLength(20)]
        public string Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public long Amount { get; set; }

        /// <summary>
        /// آدرس نسبی فایل رسید (مثلاً "/uploads/abc123.jpg")
        /// </summary>
        [MaxLength(500)]
        public string ReceiptUrl { get; set; }
    }
}

