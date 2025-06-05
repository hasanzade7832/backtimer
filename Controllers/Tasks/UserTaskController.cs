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

        /*──────────────── 1) دریافت همهٔ تسک‌های کاربر ───────────────*/
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

        /*──────────────── DTO برای ثبت درصد ────────────────────────*/
        public class CompleteTaskDto
        {
            public int PercentComplete { get; set; } = 100;   // پیش‌فرض = تکمیل
        }

        /*──────────────── 2) ثبت پیشرفت یا تکمیل ────────────────────*/
        [HttpPost("Complete/{userTaskId}")]
        public async Task<IActionResult> CompleteTask(
            int userTaskId,
            [FromBody] CompleteTaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ut = await _context.UserTasks
                .Include(x => x.TaskItem)
                .FirstOrDefaultAsync(x => x.Id == userTaskId && x.UserId == userId);

            if (ut == null)
                return NotFound();

            // فقط جلو برو اگر درصد جدید بزرگ‌تر از قبلی باشد
            if (dto.PercentComplete < ut.PercentComplete)
                return BadRequest("درصد جدید نمی‌تواند کمتر از قبلی باشد.");

            // اگر پیش‌تر 100% شده، دیگر اجازه ثبت مجدد نیست
            if (ut.IsCompletedByUser)
                return BadRequest("این تسک قبلاً به‌طور کامل تکمیل شده است.");

            /* به‌روزرسانی فیلدها */
            ut.PercentComplete = dto.PercentComplete;
            ut.IsCompletedByUser = dto.PercentComplete == 100;
            ut.CompletedAt = DateTime.UtcNow;
            ut.IsSeenByUser = true;

            await _context.SaveChangesAsync();

            /* پیام آنی به ادمین سازندهٔ تسک */
            await _hubContext.Clients.User(ut.TaskItem.CreatedByAdminId)
                .SendAsync("TaskCompletedByUser", new
                {
                    userTaskId = ut.Id,
                    percent = ut.PercentComplete
                });

            return Ok(ut);
        }
    }
}
