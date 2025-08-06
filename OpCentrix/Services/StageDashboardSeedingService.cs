using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for creating comprehensive stage-aware sample data for testing our new Stage Dashboards
    /// This follows the master plan requirements and creates realistic manufacturing workflow data
    /// </summary>
    public class StageDashboardSeedingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageDashboardSeedingService> _logger;

        public StageDashboardSeedingService(SchedulerContext context, ILogger<StageDashboardSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Create comprehensive sample data for testing all stage dashboard functionality
        /// Following the master plan requirements from OPCENTRIX_STAGE_DASHBOARD_COMPREHENSIVE_MASTER_PLAN.md
        /// </summary>
        public async Task CreateStageDashboardTestDataAsync()
        {
            try
            {
                _logger.LogInformation("[STAGE-SEED] Creating comprehensive stage dashboard test data...");

                // Step 1: Ensure we have the required production stages
                await EnsureProductionStagesExistAsync();

                // Step 2: Create stage-aware test parts with stage requirements
                await CreateStageAwarePartsAsync();

                // Step 3: Create machines for different stages
                await EnsureStageSpecificMachinesAsync();

                // Step 4: Create jobs in various stages of completion
                await CreateStageProgressionJobsAsync();

                // Step 5: Create stage executions to show progression
                await CreateProductionStageExecutionsAsync();

                // Step 6: Create cohorts to test stage progression
                await CreateBuildCohortsAsync();

                _logger.LogInformation("[STAGE-SEED] Stage dashboard test data creation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[STAGE-SEED] Error creating stage dashboard test data");
                throw;
            }
        }

        /// <summary>
        /// Ensure all 7 production stages exist as per master plan
        /// </summary>
        private async Task EnsureProductionStagesExistAsync()
        {
            _logger.LogInformation("[STAGE-SEED] Ensuring production stages exist...");

            var existingStages = await _context.ProductionStages.Where(ps => ps.IsActive).ToListAsync();

            if (existingStages.Count >= 7)
            {
                _logger.LogInformation("[STAGE-SEED] Production stages already exist ({Count} stages)", existingStages.Count);
                return;
            }

            // Add any missing stages according to the master plan
            var requiredStages = new[]
            {
                new { Name = "3D Printing (SLS)", Order = 1, Color = "#007bff", Dept = "3D Printing", DefaultMinutes = 240, RequiresApproval = false, RequiredRole = "Operator" },
                new { Name = "CNC Machining", Order = 2, Color = "#28a745", Dept = "CNC Machining", DefaultMinutes = 120, RequiresApproval = false, RequiredRole = "Operator" },
                new { Name = "EDM", Order = 3, Color = "#ffc107", Dept = "EDM", DefaultMinutes = 180, RequiresApproval = true, RequiredRole = "Manager" },
                new { Name = "Laser Engraving", Order = 4, Color = "#fd7e14", Dept = "Laser Operations", DefaultMinutes = 60, RequiresApproval = false, RequiredRole = "Operator" },
                new { Name = "Sandblasting", Order = 5, Color = "#6c757d", Dept = "Finishing", DefaultMinutes = 90, RequiresApproval = false, RequiredRole = "Operator" },
                new { Name = "Coating/Cerakote", Order = 6, Color = "#17a2b8", Dept = "Finishing", DefaultMinutes = 180, RequiresApproval = false, RequiredRole = "Operator" },
                new { Name = "Assembly", Order = 7, Color = "#dc3545", Dept = "Assembly", DefaultMinutes = 120, RequiresApproval = false, RequiredRole = "Operator" }
            };

            foreach (var stageData in requiredStages)
            {
                var existingStage = existingStages.FirstOrDefault(s => s.Name == stageData.Name);
                if (existingStage == null)
                {
                    var stage = new ProductionStage
                    {
                        Name = stageData.Name,
                        DisplayOrder = stageData.Order,
                        StageColor = stageData.Color,
                        Department = stageData.Dept,
                        DefaultSetupMinutes = stageData.DefaultMinutes,
                        RequiresApproval = stageData.RequiresApproval,
                        RequiredRole = stageData.RequiredRole,
                        IsActive = true,
                        CreatedBy = "StageDashboardSeeder",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = "StageDashboardSeeder",
                        LastModifiedDate = DateTime.UtcNow
                    };

                    _context.ProductionStages.Add(stage);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[STAGE-SEED] Production stages verified/created");
        }

        /// <summary>
        /// Create test parts with stage requirements to test multi-stage workflows
        /// </summary>
        private async Task CreateStageAwarePartsAsync()
        {
            _logger.LogInformation("[STAGE-SEED] Creating stage-aware test parts...");

            // Get all production stages
            var stages = await _context.ProductionStages.Where(ps => ps.IsActive).OrderBy(ps => ps.DisplayOrder).ToListAsync();

            if (!stages.Any())
            {
                throw new InvalidOperationException("No production stages found. Cannot create stage-aware parts.");
            }

            var testParts = new[]
            {
                new {
                    PartNumber = "STAGE-001",
                    Description = "Simple Bracket - SLS Only",
                    Material = "SS316L",
                    StagePattern = new[] { "3D Printing (SLS)" } // Single stage
                },
                new {
                    PartNumber = "STAGE-002",
                    Description = "Machined Housing - SLS + CNC",
                    Material = "SS316L",
                    StagePattern = new[] { "3D Printing (SLS)", "CNC Machining" } // Two stages
                },
                new {
                    PartNumber = "STAGE-003",
                    Description = "Complex Component - Full Workflow",
                    Material = "Inconel 625",
                    StagePattern = new[] { "3D Printing (SLS)", "CNC Machining", "EDM", "Sandblasting", "Coating/Cerakote" } // Full workflow
                },
                new {
                    PartNumber = "STAGE-004",
                    Description = "Medical Device - High Precision",
                    Material = "Ti-6Al-4V",
                    StagePattern = new[] { "3D Printing (SLS)", "CNC Machining", "Laser Engraving", "Assembly" } // Medical workflow
                },
                new {
                    PartNumber = "STAGE-005",
                    Description = "Aerospace Component - Full Treatment",
                    Material = "Inconel 718",
                    StagePattern = new[] { "3D Printing (SLS)", "CNC Machining", "EDM", "Sandblasting", "Coating/Cerakote", "Assembly" } // Aerospace workflow
                },
                new {
                    PartNumber = "STAGE-006",
                    Description = "Automotive Part - Standard Process",
                    Material = "SS316L",
                    StagePattern = new[] { "3D Printing (SLS)", "CNC Machining", "Sandblasting" } // Automotive workflow
                }
            };

            foreach (var partData in testParts)
            {
                // Check if part already exists
                var existingPart = await _context.Parts.FirstOrDefaultAsync(p => p.PartNumber == partData.PartNumber);
                if (existingPart != null) continue;

                // Create the part
                var part = new Part
                {
                    PartNumber = partData.PartNumber,
                    Description = partData.Description,
                    SlsMaterial = partData.Material,
                    Material = partData.Material,
                    IsActive = true,
                    Industry = "Testing",
                    Application = "Stage Testing",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "StageDashboardSeeder",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "StageDashboardSeeder"
                };

                _context.Parts.Add(part);
                await _context.SaveChangesAsync(); // Save to get the Part ID

                // Create stage requirements for this part
                for (int i = 0; i < partData.StagePattern.Length; i++)
                {
                    var stageName = partData.StagePattern[i];
                    var stage = stages.FirstOrDefault(s => s.Name == stageName);

                    if (stage != null)
                    {
                        var stageReq = new PartStageRequirement
                        {
                            PartId = part.Id,
                            ProductionStageId = stage.Id,
                            IsRequired = true,
                            ExecutionOrder = i + 1,
                            EstimatedHours = stage.DefaultSetupMinutes / 60.0,
                            CreatedBy = "StageDashboardSeeder",
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = "StageDashboardSeeder",
                            LastModifiedDate = DateTime.UtcNow
                        };

                        _context.PartStageRequirements.Add(stageReq);
                    }
                }

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("[STAGE-SEED] Stage-aware test parts created");
        }

        /// <summary>
        /// Ensure machines exist for different stages
        /// </summary>
        private async Task EnsureStageSpecificMachinesAsync()
        {
            _logger.LogInformation("[STAGE-SEED] Ensuring stage-specific machines exist...");

            var stageMachines = new[]
            {
                // SLS Machines
                new { Id = "SLS-01", Name = "TruPrint 3000 #1", Type = "SLS", Dept = "3D Printing" },
                new { Id = "SLS-02", Name = "TruPrint 3000 #2", Type = "SLS", Dept = "3D Printing" },
                
                // CNC Machines
                new { Id = "CNC-01", Name = "Haas VF-3", Type = "CNC", Dept = "CNC Machining" },
                new { Id = "CNC-02", Name = "DMG Mori NHX4000", Type = "CNC", Dept = "CNC Machining" },
                
                // EDM Machines
                new { Id = "EDM-01", Name = "Sodick AQ327L", Type = "EDM", Dept = "EDM" },
                
                // Laser Machines
                new { Id = "LAS-01", Name = "Trumpf TruLaser Station 5004", Type = "Laser", Dept = "Laser Operations" },
                
                // Finishing Equipment
                new { Id = "SB-01", Name = "Sandblasting Booth #1", Type = "Sandblasting", Dept = "Finishing" },
                new { Id = "CT-01", Name = "Cerakote Coating Station", Type = "Coating", Dept = "Finishing" },
                
                // Assembly Stations
                new { Id = "ASM-01", Name = "Assembly Workstation #1", Type = "Assembly", Dept = "Assembly" }
            };

            foreach (var machineData in stageMachines)
            {
                var existingMachine = await _context.Machines.FirstOrDefaultAsync(m => m.MachineId == machineData.Id);
                if (existingMachine == null)
                {
                    var machine = new Machine
                    {
                        MachineId = machineData.Id,
                        MachineName = machineData.Name,
                        MachineType = machineData.Type,
                        Department = machineData.Dept,
                        Status = "Idle",
                        IsActive = true,
                        IsAvailableForScheduling = true,
                        Priority = 1,
                        Location = $"{machineData.Dept} Floor",
                        SupportedMaterials = "SS316L,Inconel 625,Ti-6Al-4V",
                        CurrentMaterial = "SS316L"
                    };

                    _context.Machines.Add(machine);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[STAGE-SEED] Stage-specific machines verified/created");
        }

        /// <summary>
        /// Create jobs in various stages of progression to test stage dashboards
        /// </summary>
        private async Task CreateStageProgressionJobsAsync()
        {
            _logger.LogInformation("[STAGE-SEED] Creating stage progression test jobs...");

            // Get parts and machines
            var parts = await _context.Parts
                .Include(p => p.PartStageRequirements)
                .ThenInclude(psr => psr.ProductionStage)
                .Where(p => p.PartNumber.StartsWith("STAGE-"))
                .ToListAsync();

            var machines = await _context.Machines.Where(m => m.IsActive).ToListAsync();

            if (!parts.Any() || !machines.Any())
            {
                _logger.LogWarning("[STAGE-SEED] No parts or machines found. Skipping job creation.");
                return;
            }

            var now = DateTime.Now;
            var jobScenarios = new[]
            {
                // Jobs at different stages of completion
                new { PartIndex = 0, Status = "Scheduled", HoursOffset = 1.0, Description = "Ready to start SLS" },
                new { PartIndex = 1, Status = "InProgress", HoursOffset = -2.0, Description = "Currently in SLS" },
                new { PartIndex = 2, Status = "InProgress", HoursOffset = -8.0, Description = "In CNC after SLS complete" },
                new { PartIndex = 3, Status = "InProgress", HoursOffset = -16.0, Description = "In EDM stage" },
                new { PartIndex = 4, Status = "Completed", HoursOffset = -24.0, Description = "Fully completed workflow" },
                new { PartIndex = 5, Status = "Scheduled", HoursOffset = 4.0, Description = "Future job" },
                
                // More variety
                new { PartIndex = 0, Status = "Scheduled", HoursOffset = 2.0, Description = "Another SLS job" },
                new { PartIndex = 1, Status = "InProgress", HoursOffset = -1.0, Description = "Just started SLS" },
                new { PartIndex = 2, Status = "InProgress", HoursOffset = -12.0, Description = "In Sandblasting" },
                new { PartIndex = 3, Status = "Completed", HoursOffset = -48.0, Description = "Completed yesterday" }
            };

            var jobId = 1;
            foreach (var scenario in jobScenarios)
            {
                var part = parts[scenario.PartIndex % parts.Count];
                var slsMachine = machines.FirstOrDefault(m => m.MachineType == "SLS") ?? machines.First();

                var scheduledStart = now.AddHours(scenario.HoursOffset);
                var estimatedHours = 4.0 + (scenario.PartIndex * 0.5); // Vary by complexity
                var scheduledEnd = scheduledStart.AddHours(estimatedHours);

                var job = new Job
                {
                    PartId = part.Id,
                    PartNumber = part.PartNumber,
                    MachineId = slsMachine.MachineId,
                    Quantity = new Random().Next(1, 5),
                    Priority = 3,
                    Status = scenario.Status,
                    ScheduledStart = scheduledStart,
                    ScheduledEnd = scheduledEnd,
                    EstimatedHours = estimatedHours,
                    CustomerOrderNumber = $"STAGE-TEST-{jobId:D3}",
                    Notes = $"Stage test job {jobId}: {scenario.Description}",
                    Operator = "Stage Test Operator",
                    CreatedDate = now.AddDays(-1),
                    LastModifiedDate = now,
                    CreatedBy = "StageDashboardSeeder",
                    LastModifiedBy = "StageDashboardSeeder",

                    // Add stage progression fields as per master plan
                    WorkflowStage = DetermineCurrentStage(scenario),
                    TotalStages = part.PartStageRequirements?.Count ?? 1,
                    StageOrder = DetermineStageOrder(scenario)
                };

                // Set actual times for in-progress/completed jobs
                if (scenario.Status == "InProgress")
                {
                    job.ActualStart = scheduledStart;
                }
                else if (scenario.Status == "Completed")
                {
                    job.ActualStart = scheduledStart;
                    job.ActualEnd = scheduledEnd.AddMinutes(new Random().Next(-30, 60));
                }

                _context.Jobs.Add(job);
                jobId++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[STAGE-SEED] Stage progression test jobs created");
        }

        /// <summary>
        /// Create production stage executions to show jobs progressing through stages
        /// </summary>
        private async Task CreateProductionStageExecutionsAsync()
        {
            _logger.LogInformation("[STAGE-SEED] Creating production stage executions...");

            var jobs = await _context.Jobs
                .Include(j => j.Part)
                .ThenInclude(p => p.PartStageRequirements)
                .ThenInclude(psr => psr.ProductionStage)
                .Where(j => j.PartNumber.StartsWith("STAGE-"))
                .ToListAsync();

            var stages = await _context.ProductionStages.Where(ps => ps.IsActive).OrderBy(ps => ps.DisplayOrder).ToListAsync();

            foreach (var job in jobs)
            {
                if (job.Part?.PartStageRequirements == null) continue;

                var requiredStages = job.Part.PartStageRequirements
                    .Where(psr => psr.IsRequired)
                    .OrderBy(psr => psr.ExecutionOrder)
                    .ToList();

                // Create stage executions based on job status
                if (job.Status == "InProgress")
                {
                    // For in-progress jobs, complete some stages and have one active
                    var stagesCompleted = DetermineCompletedStages(job);

                    for (int i = 0; i < requiredStages.Count; i++)
                    {
                        var stageReq = requiredStages[i];

                        if (i < stagesCompleted)
                        {
                            // Completed stages
                            var execution = new ProductionStageExecution
                            {
                                JobId = job.Id,
                                ProductionStageId = stageReq.ProductionStageId,
                                Status = "Completed",
                                StartDate = job.ActualStart?.AddHours(i * 2),
                                ActualStartTime = job.ActualStart?.AddHours(i * 2),
                                CompletionDate = job.ActualStart?.AddHours(i * 2 + 1.5),
                                ActualEndTime = job.ActualStart?.AddHours(i * 2 + 1.5),
                                ActualHours = 1.5m,
                                ExecutedBy = "Stage Test Operator",
                                OperatorName = "Stage Test Operator",
                                CreatedBy = "StageDashboardSeeder",
                                CreatedDate = DateTime.UtcNow,
                                LastModifiedBy = "StageDashboardSeeder",
                                LastModifiedDate = DateTime.UtcNow
                            };
                            _context.ProductionStageExecutions.Add(execution);
                        }
                        else if (i == stagesCompleted)
                        {
                            // Currently active stage
                            var execution = new ProductionStageExecution
                            {
                                JobId = job.Id,
                                ProductionStageId = stageReq.ProductionStageId,
                                Status = "InProgress",
                                StartDate = job.ActualStart?.AddHours(i * 2),
                                ActualStartTime = job.ActualStart?.AddHours(i * 2),
                                EstimatedHours = 2.0m,
                                ExecutedBy = "Stage Test Operator",
                                OperatorName = "Stage Test Operator",
                                CreatedBy = "StageDashboardSeeder",
                                CreatedDate = DateTime.UtcNow,
                                LastModifiedBy = "StageDashboardSeeder",
                                LastModifiedDate = DateTime.UtcNow
                            };
                            _context.ProductionStageExecutions.Add(execution);
                            break; // Only one active stage per job
                        }
                    }
                }
                else if (job.Status == "Completed")
                {
                    // For completed jobs, all stages should be completed
                    for (int i = 0; i < requiredStages.Count; i++)
                    {
                        var stageReq = requiredStages[i];
                        var execution = new ProductionStageExecution
                        {
                            JobId = job.Id,
                            ProductionStageId = stageReq.ProductionStageId,
                            Status = "Completed",
                            StartDate = job.ActualStart?.AddHours(i * 2),
                            ActualStartTime = job.ActualStart?.AddHours(i * 2),
                            CompletionDate = job.ActualStart?.AddHours(i * 2 + 1.5),
                            ActualEndTime = job.ActualStart?.AddHours(i * 2 + 1.5),
                            ActualHours = 1.5m,
                            ExecutedBy = "Stage Test Operator",
                            OperatorName = "Stage Test Operator",
                            CreatedBy = "StageDashboardSeeder",
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = "StageDashboardSeeder",
                            LastModifiedDate = DateTime.UtcNow
                        };
                        _context.ProductionStageExecutions.Add(execution);
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[STAGE-SEED] Production stage executions created");
        }

        /// <summary>
        /// Create build cohorts to test cohort-based stage progression
        /// </summary>
        private async Task CreateBuildCohortsAsync()
        {
            _logger.LogInformation("[STAGE-SEED] Creating build cohorts for stage progression testing...");

            // Create a few cohorts with different progression states
            var cohorts = new[]
            {
                new { Name = "Medical Batch A", Status = "InProgress", Description = "Medical device components - currently in CNC" },
                new { Name = "Aerospace Lot B", Status = "Scheduled", Description = "Aerospace parts ready for SLS" },
                new { Name = "Automotive Set C", Status = "Completed", Description = "Completed automotive components" }
            };

            foreach (var cohortData in cohorts)
            {
                var cohort = new BuildCohort
                {
                    BuildNumber = cohortData.Name,
                    Status = cohortData.Status,
                    Notes = cohortData.Description,
                    PartCount = 10,
                    Material = "SS316L",
                    CompletedDate = cohortData.Status == "Completed" ? DateTime.Now.AddDays(-1) : null,
                    CreatedBy = "StageDashboardSeeder",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                _context.BuildCohorts.Add(cohort);
            }

            await _context.SaveChangesAsync();

            // Link some jobs to cohorts
            var savedCohorts = await _context.BuildCohorts.Where(bc => bc.BuildNumber.Contains("Batch") || bc.BuildNumber.Contains("Lot") || bc.BuildNumber.Contains("Set")).ToListAsync();
            var jobs = await _context.Jobs.Where(j => j.PartNumber.StartsWith("STAGE-")).Take(6).ToListAsync();

            for (int i = 0; i < Math.Min(jobs.Count, savedCohorts.Count * 2); i++)
            {
                jobs[i].BuildCohortId = savedCohorts[i / 2].Id;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[STAGE-SEED] Build cohorts created and linked to jobs");
        }

        #region Helper Methods

        private string DetermineCurrentStage(dynamic scenario)
        {
            switch (scenario.Status)
            {
                case "Scheduled":
                    return "3D Printing (SLS)";
                case "InProgress":
                    // Determine based on how long it's been running
                    if (scenario.HoursOffset > -4) return "3D Printing (SLS)";
                    if (scenario.HoursOffset > -12) return "CNC Machining";
                    if (scenario.HoursOffset > -20) return "EDM";
                    return "Sandblasting";
                case "Completed":
                    return "Assembly";
                default:
                    return "3D Printing (SLS)";
            }
        }

        private int DetermineStageOrder(dynamic scenario)
        {
            switch (DetermineCurrentStage(scenario))
            {
                case "3D Printing (SLS)": return 1;
                case "CNC Machining": return 2;
                case "EDM": return 3;
                case "Laser Engraving": return 4;
                case "Sandblasting": return 5;
                case "Coating/Cerakote": return 6;
                case "Assembly": return 7;
                default: return 1;
            }
        }

        private int DetermineCompletedStages(Job job)
        {
            // Determine how many stages should be completed based on how long the job has been running
            if (job.ActualStart == null) return 0;

            var hoursRunning = (DateTime.Now - job.ActualStart.Value).TotalHours;

            if (hoursRunning < 2) return 0;      // Still in first stage
            if (hoursRunning < 6) return 1;      // Completed SLS
            if (hoursRunning < 10) return 2;     // Completed SLS + CNC
            if (hoursRunning < 14) return 3;     // Completed SLS + CNC + EDM

            return Math.Min(4, (int)(hoursRunning / 4)); // One stage every 4 hours
        }

        #endregion
    }
}