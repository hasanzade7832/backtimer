using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backtimetracker.Models.PettyCashes
{
    public class Expense
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// اگر هزینه آرشیو نشده باشد، null؛ پس از آرشیو (اضافه‌شدن به تنخواه)، برابر Id آن تنخواه می‌شود.
        /// </summary>
        public long? PettyCashId { get; set; }

        [ForeignKey(nameof(PettyCashId))]
        public PettyCash PettyCash { get; set; }

        [Required]
        [MaxLength(20)]
        public string Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public long Amount { get; set; }

        /// <summary>
        /// مسیر یا URL نسبی تصویر رسید (مثلاً "/uploads/abc123.jpg").
        /// </summary>
        [MaxLength(500)]
        public string ReceiptUrl { get; set; }
    }
}
