using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;

namespace OpCentrix.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(SchedulerContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            _logger.LogInformation("?? [TEST-API] Test controller called");
            return Ok(new { 
                message = "Test API is working!", 
                timestamp = DateTime.UtcNow,
                controllerName = nameof(TestController)
            });
        }

        [HttpGet("stages")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStages()
        {
            try
            {
                _logger.LogInformation("?? [TEST-API] Getting stages via test controller");
                
                var stages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .Select(ps => new { ps.Id, ps.Name, ps.DefaultHourlyRate })
                    .ToListAsync();

                _logger.LogInformation("? [TEST-API] Found {Count} stages", stages.Count);
                return Ok(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [TEST-API] Error getting stages");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}