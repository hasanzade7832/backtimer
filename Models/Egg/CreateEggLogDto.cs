// File: Models/Egg/CreateEggLogDto.cs

using System.ComponentModel.DataAnnotations;

namespace backtimetracker.Models.Egg
{
    /// <summary>
    /// برای درخواست POST: api/Egg
    /// </summary>
    public class CreateEggLogDto
    {
        [Required]
        [MaxLength(20)]
        public string Date { get; set; }
    }
}
