// File: Models/Egg/UserEggCountDto.cs

namespace backtimetracker.Models.Egg
{
    /// <summary>
    /// برای بازگرداندن تعداد کل تخم‌مرغ‌های هر کاربر (GET: api/Egg/Counts)
    /// </summary>
    public class UserEggCountDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TotalEggs { get; set; }
    }
}
