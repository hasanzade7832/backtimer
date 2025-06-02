// backtimetracker/Controllers/InternetController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using backtimetracker.Data;
using backtimetracker.Models;

namespace backtimetracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InternetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public InternetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────── دریافت همهٔ خریدها به همراه دانلودها ───────────
        [HttpGet("All")]
        public async Task<IActionResult> GetAllPurchases()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = await _context.Purchases
                .Where(p => p.UserId == userId)
                .Include(p => p.Downloads)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(data);
        }

        // ─────────── افزودن خرید جدید ───────────
        [HttpPost("AddPurchase")]
        public async Task<IActionResult> AddPurchase([FromBody] Purchase model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = userId;

            // ذخیرهٔ تاریخ جاری به‌صورت رشته‌ی "yyyy-MM-dd"
            model.Date = DateTime.Now.ToString("yyyy-MM-dd");

            // در ابتدا باقی‌مانده = حجم کل
            model.RemainingVolume = model.TotalVolume;

            _context.Purchases.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ─────────── ویرایش خرید ───────────
        [HttpPut("EditPurchase/{id}")]
        public async Task<IActionResult> EditPurchase(int id, [FromBody] Purchase updated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var purchase = await _context.Purchases
                .Include(p => p.Downloads)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (purchase == null)
                return NotFound();

            // به‌روزرسانی مبلغ و حجم کل
            purchase.Amount = updated.Amount;
            purchase.TotalVolume = updated.TotalVolume;

            // بازمحاسبه RemainingVolume: حجم کل منهای مجموع حجم دانلودها
            var totalDownloadedSoFar = purchase.Downloads.Sum(d => d.Volume);
            purchase.RemainingVolume = purchase.TotalVolume - totalDownloadedSoFar;

            await _context.SaveChangesAsync();
            return Ok(purchase);
        }

        // ─────────── حذف خرید ───────────
        [HttpDelete("DeletePurchase/{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var purchase = await _context.Purchases
                .Include(p => p.Downloads)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (purchase == null)
                return NotFound();

            // اول همهٔ دانلودهای مرتبط را حذف می‌کنیم
            _context.Downloads.RemoveRange(purchase.Downloads);
            // سپس خرید را حذف می‌کنیم
            _context.Purchases.Remove(purchase);

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ─────────── افزودن دانلود جدید ───────────
        [HttpPost("AddDownload/{purchaseId}")]
        public async Task<IActionResult> AddDownload(int purchaseId, [FromBody] Download model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var purchase = await _context.Purchases
                .Include(p => p.Downloads)
                .FirstOrDefaultAsync(p => p.Id == purchaseId && p.UserId == userId);

            if (purchase == null)
                return NotFound();

            if (purchase.RemainingVolume < model.Volume)
                return BadRequest("حجم کافی نیست.");

            // ذخیرهٔ تاریخ جاری (میلادی) به‌صورت رشته‌ی "yyyy-MM-dd"
            model.Date = DateTime.Now.ToString("yyyy-MM-dd");

            model.PurchaseId = purchaseId;

            // کم کردن از RemainingVolume
            purchase.RemainingVolume -= model.Volume;
            purchase.Downloads.Add(model);

            await _context.SaveChangesAsync();
            return Ok(model);
        }

        // ─────────── ویرایش دانلود ───────────
        [HttpPut("EditDownload/{id}")]
        public async Task<IActionResult> EditDownload(int id, [FromBody] Download editedDownload)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var download = await _context.Downloads
                .Include(d => d.Purchase)
                .FirstOrDefaultAsync(d => d.Id == id
                    && d.Purchase.UserId == userId);

            if (download == null)
                return NotFound(new { Message = $"Download با شناسهٔ {id} پیدا نشد." });

            // به‌روز کردن فیلدهای تغییرپذیر: حجم، توضیحات و تاریخ
            download.Volume = editedDownload.Volume;
            download.Desc = editedDownload.Desc;
            download.Date = editedDownload.Date; // انتظار: "yyyy-MM-dd" از کلاینت

            // بازمحاسبه RemainingVolume برای خرید والد:
            var parentPurchase = await _context.Purchases
                .Include(p => p.Downloads)
                .FirstOrDefaultAsync(p => p.Id == download.PurchaseId);

            if (parentPurchase != null)
            {
                var totalDownloaded = parentPurchase.Downloads.Sum(d => d.Volume);
                parentPurchase.RemainingVolume = parentPurchase.TotalVolume - totalDownloaded;
            }

            _context.Downloads.Update(download);
            await _context.SaveChangesAsync();

            // برگرداندن دانلود ویرایش‌شده (هر زمان نیاز بود می‌توانستیم parentPurchase را هم بفرستیم)
            return Ok(download);
        }

        // ─────────── حذف یک دانلود ───────────
        [HttpDelete("DeleteDownload/{id}")]
        public async Task<IActionResult> DeleteDownload(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var download = await _context.Downloads
                .Include(d => d.Purchase)
                .FirstOrDefaultAsync(d => d.Id == id && d.Purchase.UserId == userId);

            if (download == null)
                return NotFound(new { Message = $"دانلود با شناسه {id} یافت نشد." });

            // افزایش دوبارهٔ حجم باقیمانده در خرید والد
            var purchase = download.Purchase;
            if (purchase != null)
                purchase.RemainingVolume += download.Volume;

            _context.Downloads.Remove(download);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }

}
