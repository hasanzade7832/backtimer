using backtimetracker.Data;
using backtimetracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Controllers.Account;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]          // دسترسی فقط برای ادمین‌ها
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // ───────────────────────────────────────────────
    // 1. همهٔ کاربران
    // GET: /api/Admin/AllUsers
    // ───────────────────────────────────────────────
    [HttpGet("AllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users
            .Select(u => new {
                u.Id,
                u.UserName,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // ───────────────────────────────────────────────
    // 2. جزئیات کامل یک کاربر
    // GET: /api/Admin/UserDetails/{id}
    // ───────────────────────────────────────────────
    [HttpGet("UserDetails/{id}")]
    public async Task<IActionResult> GetUserDetails(string id)
    {
        // ✔️ مشخصات پایه کاربر
        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new {
                u.Id,
                u.UserName,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user is null) return NotFound("کاربر یافت نشد");

        // ✔️ فعالیت‌ها و زمان‌ها
        var activities = await _context.Activities
                                 .Where(a => a.UserId == id)
                                 .ToListAsync();

        var timeRecords = await _context.TimeRecords
                                 .Where(t => t.UserId == id)
                                 .ToListAsync();

        // ✔️ خریدهای اینترنت + دانلودها
        var purchases = await _context.Purchases
                                 .Where(p => p.UserId == id)
                                 .Include(p => p.Downloads)
                                 .ToListAsync();

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.CreatedAt,
            Activities = activities,
            TimeRecords = timeRecords,
            Purchases = purchases
        });
    }

    // ───────────────────────────────────────────────
    // 3. ارتقاء به Admin
    // PUT: /api/Admin/Promote/{id}
    // ───────────────────────────────────────────────
    [HttpPut("Promote/{id}")]
    public async Task<IActionResult> PromoteToAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound("کاربر یافت نشد");

        if (user.Role == "Admin") return BadRequest("کاربر از قبل ادمین است");

        user.Role = "Admin";
        await _userManager.UpdateAsync(user);

        return Ok("کاربر به ادمین ارتقا یافت");
    }

    // ───────────────────────────────────────────────
    // 4. حذف کاربر
    // DELETE: /api/Admin/DeleteUser/{id}
    // ───────────────────────────────────────────────
    [HttpDelete("DeleteUser/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound("کاربر یافت نشد");

        await _userManager.DeleteAsync(user);
        return Ok("کاربر حذف شد");
    }
}
