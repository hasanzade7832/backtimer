using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backtimetracker.Models;

public class TimeRecord
{
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    [JsonIgnore]
    public ApplicationUser? User { get; set; }
}
