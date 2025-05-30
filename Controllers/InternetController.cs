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

        model.Time = DateTime.Now.ToString("HH:mm:ss");
        model.PurchaseId = purchaseId;
        purchase.RemainingVolume -= model.Volume;
        purchase.Downloads.Add(model);

        await _context.SaveChangesAsync();
        return Ok(model);
    }
}
