// File: Models/Egg/EggLogDto.cs

namespace backtimetracker.Models.Egg
{
    /// <summary>
    /// برای بازگرداندن جزئیات لاگ (GET: api/Egg/MyLogs)
    /// </summary>
    public class EggLogDto
    {
        public long Id { get; set; }
        public string Date { get; set; }
    }
}
