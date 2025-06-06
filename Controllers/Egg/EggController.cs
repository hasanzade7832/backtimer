// File: Controllers/Egg/EggController.cs

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backtimetracker.Data;
using backtimetracker.Models.Egg;
using backtimetracker.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Controllers.Egg
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // فقط کاربران لاگین‌شده دسترسی دارند
    public class EggController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EggController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// GET: api/Egg/Counts
        /// دریافت تعداد کل تخم‌مرغ‌های مصرف‌شده برای همهٔ کاربران
        /// </summary>
        [HttpGet("Counts")]
        public async Task<ActionResult<IEnumerable<UserEggCountDto>>> GetEggCountsPerUser()
        {
            // ۱) دریافت همهٔ کاربران
            var users = await _userManager.Users.ToListAsync();

            // ۲) برای هر کاربر، تعداد لاگ‌ها را بشماریم
            var result = new List<UserEggCountDto>();

            foreach (var user in users)
            {
                var count = await _context.EggLogs
                    .Where(el => el.UserId == user.Id)
                    .CountAsync();

                result.Add(new UserEggCountDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    TotalEggs = count
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// GET: api/Egg/MyLogs
        /// دریافت لیست لاگ‌های کاربر جاری (برای حذف/مشاهده جزئیات)
        /// </summary>
        [HttpGet("MyLogs")]
        public async Task<ActionResult<IEnumerable<EggLogDto>>> GetMyEggLogs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var logs = await _context.EggLogs
                .Where(el => el.UserId == userId)
                .OrderByDescending(el => el.CreatedAt)
                .Select(el => new EggLogDto
                {
                    Id = el.Id,
                    Date = el.Date
                })
                .ToListAsync();

            return Ok(logs);
        }

        /// <summary>
        /// POST: api/Egg
        /// ثبت لاگ جدید (برای کاربر جاری)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateEggLog([FromBody] CreateEggLogDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var newLog = new EggLog
            {
                UserId = userId,
                Date = dto.Date
            };

            _context.EggLogs.Add(newLog);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, id = newLog.Id });
        }

        /// <summary>
        /// DELETE: api/Egg/{id}
        /// حذف یک لاگ (فقط اگر متعلق به کاربر جاری باشد)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEggLog(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var log = await _context.EggLogs.FindAsync(id);
            if (log == null) return NotFound();

            if (log.UserId != userId)
            {
                return Forbid(); // فقط مالک لاگ اجازهٔ حذف دارد
            }

            _context.EggLogs.Remove(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("Decrement/{userId}")]
        public async Task<IActionResult> DecrementEgg(string userId)
        {
            // ۱. پیدا کردن آخرین EggLog برای کاربر با آن userId
            var lastLog = await _context.EggLogs
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastLog == null)
            {
                return BadRequest("هیچ تخم‌مرغی برای حذف وجود ندارد.");
            }

            // ۲. حذف آن لاگ
            _context.EggLogs.Remove(lastLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
