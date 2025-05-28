// Models/Purchase.cs
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models;

public class Purchase
{
    public int Id { get; set; }

    [Required]
    public int Amount { get; set; }

    [Required]
    public int TotalVolume { get; set; }

    public int RemainingVolume { get; set; }

    public string Date { get; set; } = string.Empty;

    public List<Download> Downloads { get; set; } = new();
}
