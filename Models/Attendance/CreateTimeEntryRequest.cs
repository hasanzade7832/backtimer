namespace YourProjectNamespace.Models.Attendance
{
    public class CreateTimeEntryRequest
    {
        public DateTime CheckIn { get; set; }       // فقط ساعت (ولی همچنان DateTime می‌مونه)
        public DateTime? CheckOut { get; set; }     // فقط ساعت (اختیاری)
        public string Duration { get; set; }        // مدت زمان به صورت رشته
        public string Tasks { get; set; }           // کارهای انجام‌شده
        public string ShamsiDate { get; set; }      // تاریخ شمسی به صورت رشته
    }
}