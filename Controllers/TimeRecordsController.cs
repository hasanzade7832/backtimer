using backtimetracker.Data;
using Microsoft.AspNetCore.Mvc;
using TrackerAPI.Models;

namespace TrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TimeRecords
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.TimeRecords.ToList());
        }

        // GET: api/TimeRecords/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var record = _context.TimeRecords.Find(id);
            if (record == null)
                return NotFound();
            return Ok(record);
        }

        // GET: api/TimeRecords/ByActivity/3
        [HttpGet("ByActivity/{activityId}")]
        public IActionResult GetByActivity(int activityId)
        {
            var records = _context.TimeRecords
                .Where(r => r.ActivityId == activityId)
                .OrderByDescending(r => r.Id)
                .ToList();
            return Ok(records);
        }

        // POST: api/TimeRecords
        [HttpPost]
        public IActionResult Post(TimeRecord record)
        {
            _context.TimeRecords.Add(record);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }

        // DELETE: api/TimeRecords/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _context.TimeRecords.Find(id);
            if (record == null)
                return NotFound();

            _context.TimeRecords.Remove(record);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
