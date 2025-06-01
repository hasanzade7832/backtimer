using backtimetracker.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Download
{
    public int Id { get; set; }

    [Required]
    public int Volume { get; set; }

    public string? Desc { get; set; }

    public string Date { get; set; } = "";   // 👈 فقط این

    [ForeignKey("Purchase")]
    public int PurchaseId { get; set; }

    public Purchase? Purchase { get; set; }
}
