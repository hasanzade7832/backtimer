using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backtimetracker.Data;
using backtimetracker.Models.Task;
using backtimetracker.Hubs;

namespace backtimetracker.Controllers.Tasks
{
    [Authorize(Roles = "User")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserTaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TaskHub> _hubContext;

        public UserTaskController(
            ApplicationDbContext context,
            IHubContext<TaskHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ── 1) دریافت همهٔ تسک‌هایی که به این کاربر تخصیص یافته ──
        [HttpGet("MyTasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myTasks = await _context.UserTasks
                .Include(ut => ut.TaskItem)
                .Where(ut => ut.UserId == userId)
                .OrderBy(ut => ut.TaskItem.Deadline)
                .ToListAsync();

            return Ok(myTasks);
        }

        // ── 2) کاربر وقتی یک تسک را کامل می‌کند (تیک می‌زند و درصد خود را ارسال می‌کند) ──
        [HttpPost("Complete/{userTaskId}")]
        public async Task<IActionResult> CompleteTask(int userTaskId, [FromBody] int percentComplete)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ut = await _context.UserTasks
                .Include(x => x.TaskItem)
                .FirstOrDefaultAsync(x => x.Id == userTaskId && x.UserId == userId);

            if (ut == null)
                return NotFound(new { message = "تسک تخصیص‌یافته یافت نشد." });

            if (ut.IsCompletedByUser)
                return BadRequest(new { message = "این تسک قبلاً تکمیل شده است." });

            if (percentComplete < 0 || percentComplete > 100)
                return BadRequest(new { message = "درصد پیشرفت باید بین 0 تا 100 باشد." });

            // به‌روز کردن وضعیت و درصد پیشرفت
            ut.PercentComplete = percentComplete;
            ut.IsCompletedByUser = (percentComplete == 100);

            if (ut.IsCompletedByUser)
                ut.CompletedAt = DateTime.UtcNow;

            ut.IsSeenByUser = true;
            await _context.SaveChangesAsync();

            // ارسال پیام آنی به ادمینِ سازندهٔ تسک
            await _hubContext.Clients.User(ut.TaskItem.CreatedByAdminId)
                .SendAsync("TaskCompletedByUser", new
                {
                    UserTaskId = ut.Id,
                    Percent = ut.PercentComplete
                });

            return Ok(ut);
        }
    }
}
