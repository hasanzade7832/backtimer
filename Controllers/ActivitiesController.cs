using Microsoft.AspNetCore.Mvc;
using backtimetracker.Data;
using backtimetracker.Models;
using Microsoft.EntityFrameworkCore;

namespace backtimetracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            return await _context.Activities.ToListAsync();
        }
    }
}
