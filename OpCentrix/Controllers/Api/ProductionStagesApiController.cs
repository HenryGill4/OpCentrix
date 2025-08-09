using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Controllers.Api
{
    [ApiController]
    [Route("api/production-stages")]
    [Authorize(Policy = "AdminOnly")]
    public class ProductionStagesApiController : ControllerBase
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<ProductionStagesApiController> _logger;

        public ProductionStagesApiController(SchedulerContext context, ILogger<ProductionStagesApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all available production stages
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableStages()
        {
            try
            {
                var stages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.Name)
                    .Select(ps => new
                    {
                        ps.Id,
                        ps.Name,
                        ps.Description,
                        ps.DefaultHourlyRate,
                        ps.DefaultSetupMinutes,
                        DefaultTeardownMinutes = 0, // This field doesn't exist in the current schema
                        ps.IsActive,
                        DefaultDurationHours = 1.0, // Default if not in database
                        DefaultMaterialCost = 0.0m
                    })
                    .ToListAsync();

                return Ok(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available production stages");
                return StatusCode(500, "Error retrieving production stages");
            }
        }

        /// <summary>
        /// Get production stage by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStageById(int id)
        {
            try
            {
                var stage = await _context.ProductionStages
                    .Where(ps => ps.Id == id && ps.IsActive)
                    .Select(ps => new
                    {
                        ps.Id,
                        ps.Name,
                        ps.Description,
                        ps.DefaultHourlyRate,
                        ps.DefaultSetupMinutes,
                        DefaultTeardownMinutes = 0, // This field doesn't exist in the current schema
                        ps.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (stage == null)
                {
                    return NotFound($"Production stage with ID {id} not found");
                }

                return Ok(stage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving production stage {StageId}", id);
                return StatusCode(500, "Error retrieving production stage");
            }
        }
    }
}