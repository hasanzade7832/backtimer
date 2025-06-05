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
using backtimetracker.Hubs; // فرض کنید TaskHub در این namespace است

namespace backtimetracker.Controllers.Tasks
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminTaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TaskHub> _hubContext;

        public AdminTaskController(
            ApplicationDbContext context,
            IHubContext<TaskHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ── 1) دریافت همهٔ تسک‌ها (با تخصیص‌ها) ──
        [HttpGet("All")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _context.TaskItems
                .Include(t => t.UserTasks)
                    .ThenInclude(ut => ut.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        // ── 2) ایجاد تسک جدید و تخصیص آن به چند کاربر ──
        [HttpPost("Create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            // ۲.۱) بسازیم TaskItem جدید را
            var taskItem = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                CreatedAt = DateTime.UtcNow,
                CreatedByAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            // ۲.۲) تخصیص به کاربران (UserTask)
            var userTasks = dto.UserIds.Select(userId => new UserTask
            {
                TaskItemId = taskItem.Id,
                UserId = userId,
                IsCompletedByUser = false,
                IsConfirmedByAdmin = false,
                IsSeenByUser = false,
                IsSeenByAdmin = false
            }).ToList();

            _context.UserTasks.AddRange(userTasks);
            await _context.SaveChangesAsync();

            // ۲.۳) ارسال اعلان بی‌درنگ به هر کاربر اختصاص‌یافته
            foreach (var ut in userTasks)
            {
                await _hubContext.Clients.User(ut.UserId)
                    .SendAsync("TaskAssigned", taskItem.Id);
            }

            return Ok(taskItem);
        }

        // ── 3) ویرایش یک تسک (عنوان و مهلت و توضیحات) ──
        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> EditTask(int id, [FromBody] EditTaskDto dto)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Deadline = dto.Deadline;
            // (در صورت نیاز می‌توانید منطق تغییر تخصیص کاربران را اینجا اضافه کنید)

            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // ── 4) حذف یک تسک (و تمام تخصیص‌های مربوطه) ──
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.UserTasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            // ابتدا تخصیص‌ها را حذف می‌کنیم
            _context.UserTasks.RemoveRange(task.UserTasks);
            // سپس خودِ تسک را حذف می‌کنیم
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ── 5) تأیید تسک تکمیل‌شده توسط کاربر ──
        [HttpPost("Confirm/{userTaskId}")]
        public async Task<IActionResult> ConfirmTask(int userTaskId)
        {
            var ut = await _context.UserTasks
                .Include(x => x.User)
                .Include(x => x.TaskItem)
                .FirstOrDefaultAsync(x => x.Id == userTaskId);

            if (ut == null)
                return NotFound();

            ut.IsConfirmedByAdmin = true;
            ut.ConfirmedAt = DateTime.UtcNow;
            ut.IsSeenByAdmin = true;
            await _context.SaveChangesAsync();

            // ارسال اعلان به کاربر مبنی بر اینکه ادمین تأیید کرده
            await _hubContext.Clients.User(ut.UserId)
                .SendAsync("TaskConfirmed", userTaskId);

            return Ok(ut);
        }
    }
}
