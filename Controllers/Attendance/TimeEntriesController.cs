using backtimetracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using YourProjectNamespace.Models.Attendance;

namespace YourProjectNamespace.Controllers.Attendance
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimeEntry([FromBody] CreateTimeEntryRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            if (request.CheckIn == default || string.IsNullOrEmpty(request.ShamsiDate))
                return BadRequest("CheckIn و ShamsiDate الزامی هستند.");

            if (request.CheckOut.HasValue && request.CheckIn > request.CheckOut.Value)
                return BadRequest("زمان خروج نمی‌تواند قبل از زمان ورود باشد.");

            var entry = new TimeEntry
            {
                UserId = userId,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                Duration = request.Duration ?? CalculateDuration(request.CheckIn, request.CheckOut),
                Tasks = request.Tasks ?? string.Empty,
                ShamsiDate = request.ShamsiDate
            };

            _context.TimeEntries.Add(entry);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Id = entry.Id,
                CheckIn = entry.CheckIn.ToString("o"),
                CheckOut = entry.CheckOut?.ToString("o"),
                Duration = entry.Duration,
                Tasks = entry.Tasks,
                ShamsiDate = entry.ShamsiDate
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTimeEntries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var entries = await _context.TimeEntries
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CheckIn)
                .Select(t => new
                {
                    t.Id,
                    CheckIn = t.CheckIn.ToString("o"),
                    CheckOut = t.CheckOut.HasValue ? t.CheckOut.Value.ToString("o") : null,
                    t.Duration,
                    t.Tasks,
                    ShamsiDate = t.ShamsiDate ?? ""
                })
                .ToListAsync();

            Console.WriteLine($"Entries retrieved: {entries.Count}, First ShamsiDate: {(entries.Any() ? entries.First().ShamsiDate : "No entries")}");
            return Ok(entries);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeEntry(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var entry = await _context.TimeEntries.FindAsync(id);
            if (entry == null || entry.UserId != userId)
                return NotFound();

            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string CalculateDuration(DateTime checkIn, DateTime? checkOut)
        {
            if (!checkOut.HasValue) return "00:00:00";
            var diff = checkOut.Value - checkIn;
            var h = (int)diff.TotalHours;
            var m = (int)(diff.TotalMinutes % 60);
            var s = (int)(diff.TotalSeconds % 60);
            return $"{h:D2}:{m:D2}:{s:D2}";
        }
    }

    public class CreateTimeEntryRequest
    {
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Duration { get; set; }
        public string Tasks { get; set; }
        public string ShamsiDate { get; set; }
    }
}