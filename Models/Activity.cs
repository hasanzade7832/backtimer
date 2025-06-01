using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backtimetracker.Models;

public class Activity
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public int TotalSeconds { get; set; } = 0; 

    [ForeignKey("UserId")]
    [JsonIgnore]
    public ApplicationUser? User { get; set; }
}
