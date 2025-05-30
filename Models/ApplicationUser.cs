using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public override string? Email { get; set; }
    public override string? PhoneNumber { get; set; }
    public string Role { get; set; } = "User"; 

}
