using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Dtos.PettyCashes
{
    public class CreatePettyCashDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
    }
}
