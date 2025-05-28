namespace TrackerAPI.Models
{
    public class TimeRecord
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string Title { get; set; } = null!;
        public string Duration { get; set; } = null!;
        public string Date { get; set; } = null!;
        public string Time { get; set; } = null!;
    }
}
