using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using backtimetracker.Data;
using backtimetracker.Dtos;            // برای UploadPhotoRequest
using backtimetracker.Models.User;
using Microsoft.AspNetCore.Http;

namespace backtimetracker.Controllers
{
    [Authorize]                    // فقط کاربران لاگین‌شده می‌توانند دسترسی داشته باشند
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // مسیر فیزیکی ذخیرهٔ عکس‌ها: wwwroot/uploads/profiles
        private readonly string _uploadRoot =
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");

        public ProfileController(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        /// <summary>
        /// GET api/Profile
        /// بازگرداندن اطلاعات پروفایلِ کاربرِ فعلی (شامل URL عکس)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            // شناسهٔ کاربر از توکن JWT
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جستجوی کاربر در دیتابیس
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // اگر مسیر عکس ذخیره شده باشد، آن را همراه با Base URL بازمی‌گردانیم
            string? photoUrl = null;
            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                // به فرض اینکه PhotoUrl مانند "/uploads/profiles/abc123.jpg" باشد
                var request = HttpContext.Request;
                string baseUrl = $"{request.Scheme}://{request.Host}";
                photoUrl = $"{baseUrl}{user.PhotoUrl}";
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.CreatedAt,
                photoUrl        // lowercase مطابق فرانت
            });
        }

        /// <summary>
        /// PUT api/Profile
        /// ویرایشِ اطلاعات متنیِ پروفایل (FullName, Email, PhoneNumber)
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();

            // اگر dto دارای فیلدی باشد، آن را به‌روزرسانی کن؛ در غیر این صورت مقدار فعلی بماند
            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// POST api/Profile/UploadPhoto
        /// آپلود عکس پروفایل (multipart/form-data با فیلد "file")
        /// </summary>
        [HttpPost("UploadPhoto")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoRequest request)
        {
            var file = request.file;
            if (file == null || file.Length == 0)
                return BadRequest("هیچ فایلی ارسال نشده.");

            // اگر پوشهٔ مقصد وجود ندارد، آن را بساز
            if (!Directory.Exists(_uploadRoot))
                Directory.CreateDirectory(_uploadRoot);

            // استخراج پسوند فایل و تولید نام یکتا
            var ext = Path.GetExtension(file.FileName).ToLower();      // مثلاً ".jpg"
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            if (Array.IndexOf(allowed, ext) < 0)
                return BadRequest("فقط فرمت JPG/PNG مجاز است.");

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(_uploadRoot, fileName);

            // اگر فایلی با این نام وجود داشت، حذفش کن
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);

            // ذخیرهٔ فیزیکی فایل روی سرور
            await using (var stream = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(stream);
            }

            // مسیر نسبی برای ذخیره در دیتابیس و ارسال به کلاینت
            var relativeUrl = $"/uploads/profiles/{fileName}";

            // به‌روزرسانی فیلد PhotoUrl در جدول AspNetUsers
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            // اگر عکس قبلی وجود داشت، ابتدا آن‌ را حذف کن
            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            user.PhotoUrl = relativeUrl;
            await _context.SaveChangesAsync();

            // بازگرداندن مسیر عکس (relative) به کلاینت
            return Ok(new { photoUrl = relativeUrl });
        }

        /// <summary>
        /// DELETE api/Profile/DeletePhoto
        /// حذف عکس فعلی پروفایلِ کاربر
        /// </summary>
        [HttpDelete("DeletePhoto")]
        public async Task<IActionResult> DeletePhoto()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            if (string.IsNullOrEmpty(user.PhotoUrl))
                return BadRequest("عکسی برای حذف وجود ندارد.");

            // مسیر فیزیکی عکس فعلی
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            // حذف فایل از سرور اگر وجود داشته باشد
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            // پاک‌کردن لینک عکس در دیتابیس
            user.PhotoUrl = null!;
            await _context.SaveChangesAsync();

            return Ok(new { message = "عکس حذف شد." });
        }
    }
}
