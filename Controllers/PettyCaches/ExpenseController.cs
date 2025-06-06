using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using backtimetracker.Data;
using backtimetracker.Dtos.PettyCashes;
using backtimetracker.Models.PettyCashes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace backtimetracker.Controllers.PettyCashes
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // در صورت نیاز به احراز هویت
    public class ExpenseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ExpenseController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// GET: api/Expense
        /// دریافت لیست همهٔ هزینه‌های جاری (PettyCashId == null)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseItemDto>>> GetCurrentExpenses()
        {
            var list = await _context.Expenses
                .Where(e => e.PettyCashId == null)
                .Select(e => new ExpenseItemDto
                {
                    Id = e.Id,
                    Date = e.Date,
                    Description = e.Description,
                    Amount = e.Amount,
                    ReceiptUrl = e.ReceiptUrl
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// GET: api/Expense/5
        /// دریافت جزئیات یک هزینهٔ جاری
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseItemDto>> GetExpense(long id)
        {
            var e = await _context.Expenses.FindAsync(id);
            if (e == null)
            {
                return NotFound();
            }

            if (e.PettyCashId != null)
            {
                return BadRequest("این هزینه آرشیو شده و قابل دسترسی از این مسیر نیست.");
            }

            var dto = new ExpenseItemDto
            {
                Id = e.Id,
                Date = e.Date,
                Description = e.Description,
                Amount = e.Amount,
                ReceiptUrl = e.ReceiptUrl
            };
            return Ok(dto);
        }

        /// <summary>
        /// POST: api/Expense
        /// ایجاد یک هزینهٔ جدید (جاری)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ExpenseItemDto>> CreateExpense(CreateExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = new Expense
            {
                Date = dto.Date,
                Description = dto.Description,
                Amount = dto.Amount,
                ReceiptUrl = dto.ReceiptUrl,
                PettyCashId = null // هنوز آرشیو نشده
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            var resultDto = new ExpenseItemDto
            {
                Id = expense.Id,
                Date = expense.Date,
                Description = expense.Description,
                Amount = expense.Amount,
                ReceiptUrl = expense.ReceiptUrl
            };

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, resultDto);
        }

        /// <summary>
        /// PUT: api/Expense/5
        /// ویرایش یک هزینهٔ جاری
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditExpense(long id, EditExpenseDto dto)
        {
            if (id != dto.Id)
                return BadRequest("شناسهٔ ارسالی با پارامتر URL مطابقت ندارد.");

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            if (expense.PettyCashId != null)
            {
                return BadRequest("این هزینه آرشیو شده و قابل ویرایش نیست.");
            }

            expense.Date = dto.Date;
            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.ReceiptUrl = dto.ReceiptUrl;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// DELETE: api/Expense/5
        /// حذف یک هزینهٔ جاری
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(long id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            if (expense.PettyCashId != null)
            {
                return BadRequest("این هزینه آرشیو شده و قابل حذف نیست.");
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// POST: api/Expense/UploadReceipt
        /// دریافت یک فایل (نوع multipart/form-data) و ذخیره در wwwroot/uploads
        /// سپس مسیر نسبی فایل را برمی‌گرداند.
        /// </summary>
        [HttpPost("UploadReceipt")]
        [AllowAnonymous] // در صورت نیاز به اجازهٔ عمومی (یا می‌توانید Authorize نگه دارید)
        public async Task<ActionResult> UploadReceipt(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("فایلی انتخاب نشده یا اندازهٔ 0 است.");
            }

            // مسیر wwwroot/uploads
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // نام یکتا
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // مسیر نسبی برای نمایش در فرانت (مثلاً "/uploads/abc123.jpg")
            var relativePath = $"/uploads/{uniqueFileName}";
            return Ok(new { receiptUrl = relativePath });
        }
    }
}
