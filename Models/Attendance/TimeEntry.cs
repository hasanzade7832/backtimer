namespace YourProjectNamespace.Models.Attendance
{
    public class TimeEntry
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Duration { get; set; }
        public string Tasks { get; set; }
        public string ShamsiDate { get; set; }
    }
}