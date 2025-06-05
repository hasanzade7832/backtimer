// File: backtimetracker/Controllers/Tasks/AdminTaskController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backtimetracker.Data;
using backtimetracker.Hubs;
using backtimetracker.Models.Task;

namespace backtimetracker.Controllers.Tasks
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminTaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TaskHub> _hub;

        public AdminTaskController(ApplicationDbContext context, IHubContext<TaskHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // ── 1) دریافت همهٔ تسک‌ها (به‌همراه تخصیص‌ها و اطلاعات کاربر مربوطه) ──
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
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
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var taskItem = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                CreatedAt = DateTime.UtcNow,
                CreatedByAdminId = adminId
            };
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            var userTasks = dto.UserIds.Select(uid => new UserTask
            {
                TaskItemId = taskItem.Id,
                UserId = uid,
                PercentComplete = 0
            }).ToList();

            _context.UserTasks.AddRange(userTasks);
            await _context.SaveChangesAsync();

            // پیام «TaskAssigned» برای هر کاربر جدید
            foreach (var ut in userTasks)
            {
                await _hub.Clients.User(ut.UserId).SendAsync("TaskAssigned", taskItem.Id);
            }

            return Ok(taskItem);
        }

        // ── 3) ویرایش یک تسک (عنوان، توضیحات، مهلت و تخصیص کاربران) ──
        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditTaskDto dto)
        {
            var task = await _context.TaskItems
                .Include(t => t.UserTasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound(new { message = "تسک موردنظر یافت نشد." });

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Deadline = dto.Deadline;

            // تغییر کاربران تخصیص‌یافته
            if (dto.UserIds != null && dto.UserIds.Any())
            {
                var currentIds = task.UserTasks.Select(ut => ut.UserId).ToList();

                // حذف کاربرانی که دیگر در لیست نیستند
                var toRemove = task.UserTasks.Where(ut => !dto.UserIds.Contains(ut.UserId)).ToList();
                _context.UserTasks.RemoveRange(toRemove);

                // اضافه کردن کاربرانی که جدیدند
                var toAdd = dto.UserIds.Except(currentIds);
                foreach (var uid in toAdd)
                {
                    _context.UserTasks.Add(new UserTask
                    {
                        TaskItemId = task.Id,
                        UserId = uid,
                        PercentComplete = 0
                    });
                }

                // پیام حذف برای کاربرانی که دیگر تسک را ندارند
                foreach (var uid in toRemove.Select(r => r.UserId))
                {
                    await _hub.Clients.User(uid).SendAsync("TaskDeleted", task.Id);
                }

                // پیام تخصیص برای کاربرانی که تازه اضافه شده‌اند
                foreach (var uid in toAdd)
                {
                    await _hub.Clients.User(uid).SendAsync("TaskAssigned", task.Id);
                }
            }

            await _context.SaveChangesAsync();

            // پیام «TaskUpdated» برای همهٔ کاربران فعلی
            var affectedUserIds = await _context.UserTasks
                .Where(ut => ut.TaskItemId == task.Id)
                .Select(ut => ut.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var uid in affectedUserIds)
            {
                await _hub.Clients.User(uid).SendAsync("TaskUpdated", task.Id);
            }

            return Ok(task);
        }

        // ── ۴) حذف یک تسک (و تمام تخصیص‌های مربوطه) ──
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.UserTasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound(new { message = "تسک موردنظر یافت نشد." });

            var userIds = task.UserTasks.Select(ut => ut.UserId).Distinct().ToList();

            _context.UserTasks.RemoveRange(task.UserTasks);
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            // پیام «TaskDeleted» برای هر کاربر
            foreach (var uid in userIds)
            {
                await _hub.Clients.User(uid).SendAsync("TaskDeleted", task.Id);
            }

            return Ok(new { message = "تسک و تمامی تخصیص‌های آن حذف شد." });
        }

        // ── 5) تأیید تسک تکمیل‌شده توسط کاربر ──
        [HttpPost("Confirm/{userTaskId}")]
        public async Task<IActionResult> Confirm(int userTaskId)
        {
            var ut = await _context.UserTasks
                .Include(x => x.TaskItem)
                .FirstOrDefaultAsync(x => x.Id == userTaskId);

            if (ut == null)
                return NotFound(new { message = "تسک تخصیص‌یافته یافت نشد." });

            ut.IsConfirmedByAdmin = true;
            ut.ConfirmedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // ارسال اعلان «TaskConfirmed» برای کاربر
            await _hub.Clients.User(ut.UserId).SendAsync("TaskConfirmed", ut.Id);

            return Ok(ut);
        }

        // ── 6) دریافت تسک‌های تأییدشده توسط ادمین برای یک کاربر خاص ──
        [HttpGet("Approved/ByUser/{userId}")]
        public async Task<IActionResult> GetApproved(string userId)
        {
            var userTasks = await _context.UserTasks
                .Where(ut => ut.UserId == userId && ut.IsConfirmedByAdmin)
                .Include(ut => ut.TaskItem)
                .ToListAsync();

            var approvedTasks = userTasks.Select(ut => new
            {
                ut.TaskItem.Id,
                ut.TaskItem.Title,
                ut.TaskItem.Description,
                ut.TaskItem.Deadline,
                IsApprovedByAdmin = true
            });

            return Ok(approvedTasks);
        }
    }
}
