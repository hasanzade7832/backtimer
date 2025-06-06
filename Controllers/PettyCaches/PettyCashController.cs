using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backtimetracker.Data;
using backtimetracker.Dtos.PettyCashes;
using backtimetracker.Models.PettyCashes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Controllers.PettyCashes
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // در صورت نیاز به احراز هویت
    public class PettyCashController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PettyCashController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/PettyCash
        /// دریافت لیست همهٔ تنخواه‌ها به همراه هزینه‌های آرشیوشده
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PettyCashItemDto>>> GetAllPettyCashes()
        {
            var list = await _context.PettyCashes
                .Include(pc => pc.Expenses)
                .Select(pc => new PettyCashItemDto
                {
                    Id = pc.Id,
                    Title = pc.Title,
                    CreatedAt = pc.CreatedAt,
                    Expenses = pc.Expenses.Select(e => new ExpenseItemDto
                    {
                        Id = e.Id,
                        Date = e.Date,
                        Description = e.Description,
                        Amount = e.Amount,
                        ReceiptUrl = e.ReceiptUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// GET: api/PettyCash/5
        /// دریافت جزئیات یک تنخواه به همراه هزینه‌های آرشیوشدهٔ آن
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PettyCashItemDto>> GetPettyCash(long id)
        {
            var pc = await _context.PettyCashes
                .Include(p => p.Expenses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pc == null)
                return NotFound();

            var dto = new PettyCashItemDto
            {
                Id = pc.Id,
                Title = pc.Title,
                CreatedAt = pc.CreatedAt,
                Expenses = pc.Expenses.Select(e => new ExpenseItemDto
                {
                    Id = e.Id,
                    Date = e.Date,
                    Description = e.Description,
                    Amount = e.Amount,
                    ReceiptUrl = e.ReceiptUrl
                }).ToList()
            };
            return Ok(dto);
        }

        /// <summary>
        /// POST: api/PettyCash
        /// ایجاد یک تنخواه جدید (آرشیو کردن همهٔ هزینه‌های جاری)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PettyCashItemDto>> CreatePettyCash(CreatePettyCashDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ۱) همهٔ هزینه‌های جاری را بگیر
            var currentExpenses = await _context.Expenses
                .Where(e => e.PettyCashId == null)
                .ToListAsync();

            // ۲) شیء جدید PettyCash را بساز و هزینه‌ها را به آن نسبت بده
            var newPetty = new PettyCash
            {
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow,
                Expenses = currentExpenses
            };

            // ۳) برای هر هزینه، PettyCashId را ست کن
            foreach (var e in currentExpenses)
            {
                e.PettyCash = newPetty;
            }

            // ۴) در دیتابیس اضافه‌اش کن
            _context.PettyCashes.Add(newPetty);
            await _context.SaveChangesAsync();

            // ۵) آماده‌سازی DTO خروجی
            var resultDto = new PettyCashItemDto
            {
                Id = newPetty.Id,
                Title = newPetty.Title,
                CreatedAt = newPetty.CreatedAt,
                Expenses = newPetty.Expenses.Select(e => new ExpenseItemDto
                {
                    Id = e.Id,
                    Date = e.Date,
                    Description = e.Description,
                    Amount = e.Amount,
                    ReceiptUrl = e.ReceiptUrl
                }).ToList()
            };

            return CreatedAtAction(nameof(GetPettyCash), new { id = newPetty.Id }, resultDto);
        }

        /// <summary>
        /// DELETE: api/PettyCash/5
        /// حذف یک تنخواه و همهٔ هزینه‌های آرشیوشدهٔ آن (Cascade)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePettyCash(long id)
        {
            var pc = await _context.PettyCashes
                .Include(p => p.Expenses)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pc == null)
                return NotFound();

            _context.PettyCashes.Remove(pc);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
