// Models/Download.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backtimetracker.Models;

public class Download
{
    public int Id { get; set; }

    [Required]
    public int Volume { get; set; }

    public string? Desc { get; set; }

    public string Time { get; set; } = "";

    [ForeignKey("Purchase")]
    public int PurchaseId { get; set; }

    public Purchase? Purchase { get; set; }
}
