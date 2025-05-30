using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    [JsonIgnore]
    public ApplicationUser? User { get; set; }

    public List<Download> Downloads { get; set; } = new();
}
