using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Runtime;

namespace OpCentrix.Controllers.Api
{
    [ApiController]
    [Route("api/scheduler")] 
    public class SchedulerController : ControllerBase
    {
        private readonly SchedulerContext _context;
        private readonly ISchedulerRuntimeService _runtime;
        private readonly ILogger<SchedulerController> _logger;

        public SchedulerController(SchedulerContext context, ISchedulerRuntimeService runtime, ILogger<SchedulerController> logger)
        {
            _context = context;
            _runtime = runtime;
            _logger = logger;
        }

        [HttpPost("jobs/{id}/start")] 
        public async Task<IActionResult> StartJob(int id, [FromBody] StartJobRequest request)
        {
            var userName = User?.Identity?.Name ?? "System";
            int? userId = null; // Extend with user resolution later
            var result = await _runtime.StartJobAsync(id, request, userName, userId);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(ToDto(result.Job));
        }

        [HttpPost("jobs/{id}/complete")] 
        public async Task<IActionResult> CompleteJob(int id, [FromBody] CompleteJobRequest request)
        {
            var userName = User?.Identity?.Name ?? "System";
            int? userId = null;
            var result = await _runtime.CompleteJobAsync(id, request, userName, userId);
            if (!result.Success)
                return BadRequest(new { error = result.Error });
            return Ok(ToDto(result.Job));
        }

        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _context.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return NotFound();
            return Ok(ToDto(job));
        }

        private object ToDto(Job j) => new
        {
            j.Id,
            j.PartNumber,
            j.MachineId,
            j.Status,
            j.ScheduledStart,
            j.ScheduledEnd,
            j.ActualStart,
            j.ActualEnd,
            j.PlannedEndUtc,
            j.StackLevel,
            j.ActualUnitsPlanned,
            j.PrototypeUnitsPlanned,
            j.PowderAddedKg,
            j.PowderMaterial,
            j.PlannedStackDurationHours,
            j.EstimatedHours,
            DurationHours = j.DurationHours
        };
    }
}
