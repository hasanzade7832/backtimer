namespace backtimetracker.Models.User
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        /* اگر خواستی رمز را هم اینجا بگیری و تغییر دهی، می‌توانی فیلد Password اضافه کنی */
    }
}
