using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backtimetracker.Data;
using backtimetracker.Models;

namespace backtimetracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimeRecordsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TimeRecordsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var records = _context.TimeRecords.Where(r => r.UserId == userId).ToList();
        return Ok(records);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var record = _context.TimeRecords.FirstOrDefault(r => r.Id == id && r.UserId == userId);
        if (record == null)
            return NotFound();
        return Ok(record);
    }

    [HttpGet("ByActivity/{activityId}")]
    public IActionResult GetByActivity(int activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var records = _context.TimeRecords
            .Where(r => r.ActivityId == activityId && r.UserId == userId)
            .OrderByDescending(r => r.Id)
            .ToList();
        return Ok(records);
    }

    [HttpPost]
    public IActionResult Post(TimeRecord record)
    {
        record.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _context.TimeRecords.Add(record);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var record = _context.TimeRecords.FirstOrDefault(r => r.Id == id && r.UserId == userId);
        if (record == null)
            return NotFound();

        _context.TimeRecords.Remove(record);
        _context.SaveChanges();
        return NoContent();
    }
}
