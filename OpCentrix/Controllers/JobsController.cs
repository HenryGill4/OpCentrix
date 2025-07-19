using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly SchedulerContext _context;

        public JobsController(SchedulerContext context)
        {
            _context = context;
        }

        // GET: /api/jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        // POST: /api/jobs
        [HttpPost]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetJobs), new { id = job.Id }, job);
        }

        // PUT: /api/jobs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJob(int id, Job updatedJob)
        {
            if (id != updatedJob.Id)
                return BadRequest();

            _context.Entry(updatedJob).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: /api/jobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
