using backtimetracker.Data;
using backtimetracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YourAppNamespace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InternetController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public InternetController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🟢 1. دریافت لیست خریدها با دانلودها
    [HttpGet("All")]
    public async Task<IActionResult> GetAllPurchases()
    {
        var data = await _context.Purchases
            .Include(p => p.Downloads)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return Ok(data);
    }

    // 🟢 2. افزودن خرید جدید
    [HttpPost("AddPurchase")]
    public async Task<IActionResult> AddPurchase([FromBody] Purchase model)
    {
        model.Date = DateTime.Now.ToString("yyyy/MM/dd");
        model.RemainingVolume = model.TotalVolume;
        _context.Purchases.Add(model);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    // 🟡 3. ویرایش خرید
    [HttpPut("EditPurchase/{id}")]
    public async Task<IActionResult> EditPurchase(int id, [FromBody] Purchase updated)
    {
        var purchase = await _context.Purchases.Include(p => p.Downloads).FirstOrDefaultAsync(p => p.Id == id);
        if (purchase == null) return NotFound();

        purchase.Amount = updated.Amount;
        purchase.TotalVolume = updated.TotalVolume;

        // محاسبه مجدد حجم باقی‌مانده
        var used = purchase.Downloads.Sum(d => d.Volume);
        purchase.RemainingVolume = updated.TotalVolume - used;

        await _context.SaveChangesAsync();
        return Ok(purchase);
    }

    // 🔴 4. حذف خرید
    [HttpDelete("DeletePurchase/{id}")]
    public async Task<IActionResult> DeletePurchase(int id)
    {
        var purchase = await _context.Purchases.Include(p => p.Downloads).FirstOrDefaultAsync(p => p.Id == id);
        if (purchase == null) return NotFound();

        _context.Downloads.RemoveRange(purchase.Downloads);
        _context.Purchases.Remove(purchase);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // 🟢 5. افزودن دانلود برای یک خرید خاص
    [HttpPost("AddDownload/{purchaseId}")]
    public async Task<IActionResult> AddDownload(int purchaseId, [FromBody] Download model)
    {
        var purchase = await _context.Purchases.Include(p => p.Downloads).FirstOrDefaultAsync(p => p.Id == purchaseId);
        if (purchase == null) return NotFound();

        if (purchase.RemainingVolume < model.Volume)
            return BadRequest("حجم کافی نیست.");

        model.Time = DateTime.Now.ToString("HH:mm:ss");
        model.PurchaseId = purchaseId;

        purchase.RemainingVolume -= model.Volume;
        purchase.Downloads.Add(model);

        await _context.SaveChangesAsync();
        return Ok(model);
    }
}
