namespace backtimetracker.Models.Auth
{
    public class RegisterDto
    {
        public string? UserName { get; set; }   // ✅ اضافه شده
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    }
}
