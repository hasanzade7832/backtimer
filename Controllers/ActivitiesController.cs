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

        activity.UserId = userId;
        activity.User = null;

        _context.Entry(activity).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
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
}