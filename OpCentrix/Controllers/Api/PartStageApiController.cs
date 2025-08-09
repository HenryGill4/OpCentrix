using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Controllers.Api
{
    /// <summary>
    /// API Controller for Part Stage Management
    /// Supports the modern stage management system in the Part Form
    /// </summary>
    [ApiController]
    [Route("api/parts")]
    [Authorize] // Changed from AdminOnly to basic Authorize to fix modal context issues
    public class PartStageApiController : ControllerBase
    {
        private readonly SchedulerContext _context;
        private readonly IPartStageService _partStageService;
        private readonly ILogger<PartStageApiController> _logger;

        public PartStageApiController(
            SchedulerContext context,
            IPartStageService partStageService,
            ILogger<PartStageApiController> logger)
        {
            _context = context;
            _partStageService = partStageService;
            _logger = logger;
        }

        /// <summary>
        /// Get available production stages for stage selection
        /// </summary>
        [HttpGet("/api/production-stages/available")]
        [AllowAnonymous] // Allow anonymous access for better modal compatibility
        public async Task<IActionResult> GetAvailableProductionStages()
        {
            try
            {
                var stages = await _context.ProductionStages
                    .Where(ps => ps.IsActive)
                    .OrderBy(ps => ps.Name)
                    .Select(ps => new
                    {
                        Id = ps.Id,
                        Name = ps.Name,
                        Description = ps.Description,
                        DefaultHourlyRate = ps.DefaultHourlyRate,
                        DefaultSetupMinutes = ps.DefaultSetupMinutes,
                        // Note: DefaultTeardownMinutes not available in current schema
                        DefaultTeardownMinutes = 0,
                        IsActive = ps.IsActive
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} available production stages", stages.Count);
                return Ok(stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available production stages");
                return StatusCode(500, new { error = "Failed to retrieve production stages", details = ex.Message });
            }
        }

        /// <summary>
        /// Get stage requirements for a specific part
        /// </summary>
        [HttpGet("{partId}/stage-requirements")]
        public async Task<IActionResult> GetPartStageRequirements(int partId)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    return NotFound(new { error = "Part not found" });
                }

                var stageRequirements = await _context.PartStageRequirements
                    .Include(psr => psr.ProductionStage)
                    .Where(psr => psr.PartId == partId && psr.IsActive)
                    .OrderBy(psr => psr.ExecutionOrder)
                    .Select(psr => new
                    {
                        Id = psr.Id,
                        PartId = psr.PartId,
                        ProductionStageId = psr.ProductionStageId,
                        ProductionStage = new
                        {
                            Id = psr.ProductionStage.Id,
                            Name = psr.ProductionStage.Name,
                            DefaultHourlyRate = psr.ProductionStage.DefaultHourlyRate,
                            DefaultSetupMinutes = psr.ProductionStage.DefaultSetupMinutes,
                            DefaultTeardownMinutes = 0 // Not available in current schema
                        },
                        ExecutionOrder = psr.ExecutionOrder,
                        EstimatedHours = psr.EstimatedHours,
                        SetupTimeMinutes = psr.SetupTimeMinutes,
                        TeardownTimeMinutes = 0, // Not available in current schema
                        HourlyRateOverride = psr.HourlyRateOverride,
                        MaterialCost = psr.MaterialCost,
                        IsRequired = psr.IsRequired,
                        IsActive = psr.IsActive,
                        RequirementNotes = psr.RequirementNotes,
                        SpecialInstructions = psr.SpecialInstructions
                    })
                    .ToListAsync();

                return Ok(stageRequirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stage requirements for part {PartId}", partId);
                return StatusCode(500, new { error = "Failed to retrieve stage requirements", details = ex.Message });
            }
        }

        /// <summary>
        /// Add or update a stage requirement for a part
        /// </summary>
        [HttpPost("{partId}/stage-requirements")]
        public async Task<IActionResult> SavePartStageRequirement(int partId, [FromBody] PartStageRequirementDto stageDto)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    return NotFound(new { error = "Part not found" });
                }

                var productionStage = await _context.ProductionStages.FindAsync(stageDto.ProductionStageId);
                if (productionStage == null)
                {
                    return BadRequest(new { error = "Invalid production stage" });
                }

                PartStageRequirement stageRequirement;

                if (stageDto.Id.HasValue && stageDto.Id > 0)
                {
                    // Update existing stage requirement
                    stageRequirement = await _context.PartStageRequirements
                        .FirstOrDefaultAsync(psr => psr.Id == stageDto.Id && psr.PartId == partId);
                    
                    if (stageRequirement == null)
                    {
                        return NotFound(new { error = "Stage requirement not found" });
                    }
                }
                else
                {
                    // Create new stage requirement
                    stageRequirement = new PartStageRequirement
                    {
                        PartId = partId,
                        ProductionStageId = stageDto.ProductionStageId,
                        CreatedBy = User.Identity?.Name ?? "API",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.PartStageRequirements.Add(stageRequirement);
                }

                // Update properties
                stageRequirement.ExecutionOrder = stageDto.ExecutionOrder;
                stageRequirement.EstimatedHours = stageDto.EstimatedHours;
                stageRequirement.SetupTimeMinutes = stageDto.SetupTimeMinutes;
                // TeardownTimeMinutes not available in current schema - skip for now
                stageRequirement.HourlyRateOverride = stageDto.HourlyRateOverride;
                stageRequirement.MaterialCost = stageDto.MaterialCost;
                stageRequirement.IsRequired = stageDto.IsRequired;
                stageRequirement.IsActive = true;
                stageRequirement.RequirementNotes = stageDto.RequirementNotes ?? string.Empty;
                stageRequirement.SpecialInstructions = stageDto.SpecialInstructions ?? string.Empty;
                stageRequirement.LastModifiedBy = User.Identity?.Name ?? "API";
                stageRequirement.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved stage requirement for part {PartId}, stage {StageId}", 
                    partId, stageDto.ProductionStageId);

                return Ok(new { 
                    id = stageRequirement.Id, 
                    message = "Stage requirement saved successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving stage requirement for part {PartId}", partId);
                return StatusCode(500, new { error = "Failed to save stage requirement", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a stage requirement
        /// </summary>
        [HttpDelete("{partId}/stage-requirements/{stageRequirementId}")]
        public async Task<IActionResult> DeletePartStageRequirement(int partId, int stageRequirementId)
        {
            try
            {
                var stageRequirement = await _context.PartStageRequirements
                    .FirstOrDefaultAsync(psr => psr.Id == stageRequirementId && psr.PartId == partId);

                if (stageRequirement == null)
                {
                    return NotFound(new { error = "Stage requirement not found" });
                }

                // Soft delete
                stageRequirement.IsActive = false;
                stageRequirement.LastModifiedBy = User.Identity?.Name ?? "API";
                stageRequirement.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted stage requirement {StageRequirementId} for part {PartId}", 
                    stageRequirementId, partId);

                return Ok(new { message = "Stage requirement deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stage requirement {StageRequirementId} for part {PartId}", 
                    stageRequirementId, partId);
                return StatusCode(500, new { error = "Failed to delete stage requirement", details = ex.Message });
            }
        }

        /// <summary>
        /// Bulk update stage requirements for a part
        /// Used by the form submission process
        /// </summary>
        [HttpPost("{partId}/stage-requirements/bulk")]
        public async Task<IActionResult> BulkUpdateStageRequirements(int partId, [FromBody] List<PartStageRequirementDto> stageRequirements)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    return NotFound(new { error = "Part not found" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Get existing stage requirements
                    var existingRequirements = await _context.PartStageRequirements
                        .Where(psr => psr.PartId == partId)
                        .ToListAsync();

                    // Deactivate existing requirements
                    foreach (var existing in existingRequirements)
                    {
                        existing.IsActive = false;
                        existing.LastModifiedBy = User.Identity?.Name ?? "API";
                        existing.LastModifiedDate = DateTime.UtcNow;
                    }

                    // Add new requirements
                    foreach (var stageDto in stageRequirements)
                    {
                        var productionStage = await _context.ProductionStages.FindAsync(stageDto.ProductionStageId);
                        if (productionStage == null)
                        {
                            continue; // Skip invalid stages
                        }

                        var stageRequirement = new PartStageRequirement
                        {
                            PartId = partId,
                            ProductionStageId = stageDto.ProductionStageId,
                            ExecutionOrder = stageDto.ExecutionOrder,
                            EstimatedHours = stageDto.EstimatedHours,
                            SetupTimeMinutes = stageDto.SetupTimeMinutes,
                            // TeardownTimeMinutes not available in current schema
                            HourlyRateOverride = stageDto.HourlyRateOverride,
                            MaterialCost = stageDto.MaterialCost,
                            IsRequired = stageDto.IsRequired,
                            IsActive = true,
                            RequirementNotes = stageDto.RequirementNotes ?? string.Empty,
                            SpecialInstructions = stageDto.SpecialInstructions ?? string.Empty,
                            CreatedBy = User.Identity?.Name ?? "API",
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = User.Identity?.Name ?? "API",
                            LastModifiedDate = DateTime.UtcNow
                        };

                        _context.PartStageRequirements.Add(stageRequirement);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Bulk updated {Count} stage requirements for part {PartId}", 
                        stageRequirements.Count, partId);

                    return Ok(new { message = "Stage requirements updated successfully" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating stage requirements for part {PartId}", partId);
                return StatusCode(500, new { error = "Failed to update stage requirements", details = ex.Message });
            }
        }

        /// <summary>
        /// Get stage requirement statistics for a part
        /// </summary>
        [HttpGet("{partId}/stage-requirements/summary")]
        public async Task<IActionResult> GetStageRequirementsSummary(int partId)
        {
            try
            {
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    return NotFound(new { error = "Part not found" });
                }

                var stageRequirements = await _context.PartStageRequirements
                    .Include(psr => psr.ProductionStage)
                    .Where(psr => psr.PartId == partId && psr.IsActive)
                    .ToListAsync();

                var totalStages = stageRequirements.Count;
                var totalDuration = stageRequirements.Sum(psr => psr.EstimatedHours ?? 0.0);
                var totalSetupTime = stageRequirements.Sum(psr => psr.SetupTimeMinutes ?? 0);
                var totalTeardownTime = 0; // Not available in current schema
                
                decimal totalCost = 0;
                foreach (var psr in stageRequirements)
                {
                    var hourlyRate = psr.HourlyRateOverride ?? psr.ProductionStage?.DefaultHourlyRate ?? 85.00m;
                    var laborCost = Convert.ToDecimal(psr.EstimatedHours ?? 0.0) * hourlyRate;
                    var setupCost = Convert.ToDecimal(psr.SetupTimeMinutes ?? 0) / 60m * hourlyRate;
                    var teardownCost = 0m; // Not available in current schema
                    var materialCost = psr.MaterialCost;
                    totalCost += laborCost + setupCost + teardownCost + materialCost;
                }

                var complexity = CalculateComplexity(totalStages, totalDuration);

                var summary = new
                {
                    TotalStages = totalStages,
                    TotalDuration = Math.Round((decimal)totalDuration, 2),
                    TotalSetupTime = totalSetupTime,
                    TotalTeardownTime = totalTeardownTime,
                    TotalCost = Math.Round(totalCost, 2),
                    Complexity = complexity,
                    StageList = stageRequirements
                        .OrderBy(psr => psr.ExecutionOrder)
                        .Select(psr => new
                        {
                            Name = psr.ProductionStage?.Name,
                            Order = psr.ExecutionOrder,
                            Duration = psr.EstimatedHours ?? 0.0
                        })
                        .ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stage requirements summary for part {PartId}", partId);
                return StatusCode(500, new { error = "Failed to get stage requirements summary", details = ex.Message });
            }
        }

        private string CalculateComplexity(int stageCount, double totalHours)
        {
            var score = stageCount + Math.Floor(totalHours / 4);
            
            return score switch
            {
                <= 2 => "Simple",
                <= 4 => "Medium",
                <= 6 => "Complex",
                _ => "Very Complex"
            };
        }
    }

    /// <summary>
    /// DTO for Part Stage Requirement API operations
    /// </summary>
    public class PartStageRequirementDto
    {
        public int? Id { get; set; }
        public int ProductionStageId { get; set; }
        public int ExecutionOrder { get; set; }
        public double EstimatedHours { get; set; }
        public int? SetupTimeMinutes { get; set; }
        public int? TeardownTimeMinutes { get; set; } // Note: Not available in current schema
        public decimal? HourlyRateOverride { get; set; }
        public decimal MaterialCost { get; set; }
        public bool IsRequired { get; set; } = true;
        public string? RequirementNotes { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}