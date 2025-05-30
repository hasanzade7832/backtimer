// RoleInitializer.cs
using backtimetracker.Models;
using Microsoft.AspNetCore.Identity;

namespace backtimetracker.Services
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var anyAdminExists = userManager.Users.Any(u => u.Role == "Admin");
            if (!anyAdminExists)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    FullName = "Super Admin",
                    Email = "admin@example.com",
                    Role = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                await userManager.CreateAsync(admin, "Admin123!");
                Console.WriteLine("🟢 ادمین پیش‌فرض ساخته شد.");
            }
        }
    }
}
