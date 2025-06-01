using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backtimetracker.Data;
using backtimetracker.Models;

namespace backtimetracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ActivitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await _context.Activities.Where(a => a.UserId == userId).ToListAsync();
    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null) return NotFound();

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("کاربر معتبر نیست");
        }

        activity.UserId = userId;
        activity.User = null;

        // اگر از فرانت مقدار totalSeconds فرستاده شده، استفاده کن، اگر نه مقدار صفر باشد.
        if (activity.TotalSeconds < 0) activity.TotalSeconds = 0;

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActivities), new { id = activity.Id }, activity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(int id, Activity activity)
    {
        if (id != activity.Id) return BadRequest("شناسه فعالیت نامعتبر است");

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("کاربر معتبر نیست");
        }

        // رکورد اصلی را بگیر تا totalSeconds را هم بتوانی درست بروزرسانی کنی
        var entity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (entity == null) return NotFound();

        entity.Title = activity.Title;
        entity.TotalSeconds = activity.TotalSeconds; // ← ← این خط مهم است

        await _context.SaveChangesAsync();

        return NoContent();
    }

}