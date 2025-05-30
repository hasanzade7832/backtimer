using backtimetracker.Data;
using backtimetracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Controllers.Account;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // فقط ادمین‌ها به این کنترلر دسترسی دارند
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // 🟢 دریافت همه کاربران
    [HttpGet("AllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users
            .Select(u => new {
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // 🟡 ارتقا به Admin
    [HttpPut("PromoteToAdmin/{id}")]
    public async Task<IActionResult> PromoteToAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("کاربر یافت نشد");

        user.Role = "Admin";
        await _userManager.UpdateAsync(user);

        return Ok("کاربر به ادمین ارتقا یافت");
    }

    // 🔴 حذف کاربر
    [HttpDelete("DeleteUser/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound("کاربر یافت نشد");

        await _userManager.DeleteAsync(user);
        return Ok("کاربر حذف شد");
    }
}
