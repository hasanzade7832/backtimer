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
public class InternetController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public InternetController(ApplicationDbContext context)
    {
        _context = context;
    }

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

    [HttpPost("AddPurchase")]
    public async Task<IActionResult> AddPurchase([FromBody] Purchase model)
    {
        model.Date = DateTime.Now.ToString("yyyy/MM/dd");
        model.RemainingVolume = model.TotalVolume;
        model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _context.Purchases.Add(model);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("EditPurchase/{id}")]
    public async Task<IActionResult> EditPurchase(int id, [FromBody] Purchase updated)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var purchase = await _context.Purchases
            .Include(p => p.Downloads)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (purchase == null) return NotFound();

        purchase.Amount = updated.Amount;
        purchase.TotalVolume = updated.TotalVolume;
        purchase.RemainingVolume = updated.TotalVolume - purchase.Downloads.Sum(d => d.Volume);

        await _context.SaveChangesAsync();
        return Ok(purchase);
    }

    [HttpDelete("DeletePurchase/{id}")]
    public async Task<IActionResult> DeletePurchase(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var purchase = await _context.Purchases.Include(p => p.Downloads)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (purchase == null) return NotFound();

        _context.Downloads.RemoveRange(purchase.Downloads);
        _context.Purchases.Remove(purchase);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("AddDownload/{purchaseId}")]
    public async Task<IActionResult> AddDownload(int purchaseId, [FromBody] Download model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var purchase = await _context.Purchases.Include(p => p.Downloads)
            .FirstOrDefaultAsync(p => p.Id == purchaseId && p.UserId == userId);

        if (purchase == null) return NotFound();
        if (purchase.RemainingVolume < model.Volume)
            return BadRequest("حجم کافی نیست.");

        model.Date = DateTime.Now.ToString("yyyy-MM-dd");   // 👈 فقط تاریخ و بدون ساعت

        model.PurchaseId = purchaseId;
        purchase.RemainingVolume -= model.Volume;
        purchase.Downloads.Add(model);

        await _context.SaveChangesAsync();
        return Ok(model);
    }

    // ─────────── ویرایش دانلود ───────────
    [HttpPut("EditDownload/{id}")]
    public async Task<IActionResult> EditDownload(int id, [FromBody] Download editedDownload)
    {
        // ابتدا رکورد دانلودِ مربوطه را از دیتابیس بخوانیم
        var download = await _context.Downloads
            .Include(d => d.Purchase) // در صورت نیاز به اطلاعات والد
            .FirstOrDefaultAsync(d => d.Id == id && d.Purchase.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (download == null)
            return NotFound(new { Message = $"Download با شناسهٔ {id} پیدا نشد." });

        // فرض می‌کنیم فیلدهای قابل ویرایش فقط حجم دانلود (Volume) و تاریخ آن است
        download.Volume = editedDownload.Volume;
        download.Date = editedDownload.Date;

        // اگر لازم است باقی منطق مثل به‌روز کردن RemainingVolume انجام شود:
        var parentPurchase = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == download.PurchaseId);
        if (parentPurchase != null)
        {
            // دوباره محاسبه RemainingVolume بر اساس مجموع دانلودهای جدید
            var totalDownloaded = await _context.Downloads
                .Where(d => d.PurchaseId == parentPurchase.Id)
                .SumAsync(d => d.Volume);
            parentPurchase.RemainingVolume = parentPurchase.TotalVolume - totalDownloaded;
        }

        _context.Downloads.Update(download);
        await _context.SaveChangesAsync();
        return Ok(download);
    }
}
