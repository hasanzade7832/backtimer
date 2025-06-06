using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Dtos.PettyCashes
{
    public class EditExpenseDto
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public long Amount { get; set; }

        [MaxLength(500)]
        public string ReceiptUrl { get; set; }
    }
}
