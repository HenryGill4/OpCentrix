using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using System.Text.Json;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// B&T Manufacturing Workflow Service
    /// Handles B&T-specific manufacturing stage transitions, workflow management, and process orchestration
    /// </summary>
    public class BTManufacturingWorkflowService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<BTManufacturingWorkflowService> _logger;

        public BTManufacturingWorkflowService(SchedulerContext context, ILogger<BTManufacturingWorkflowService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region B&T Workflow Management

        /// <summary>
        /// Get available B&T manufacturing workflows
        /// </summary>
        public Dictionary<string, BTWorkflowDefinition> GetAvailableBTWorkflows()
        {
            return new Dictionary<string, BTWorkflowDefinition>
            {
                ["BT_Suppressor_Workflow"] = new BTWorkflowDefinition
                {
                    Name = "B&T Suppressor Workflow",
                    Description = "Complete workflow for suppressor components with ATF compliance",
                    Stages = new[]
                    {
                        "Design & Engineering",
                        "ATF Form 1 Submission", 
                        "SLS Metal Printing",
                        "Secondary Operations (CNC/EDM)",
                        "Sound Testing & Validation",
                        "Final Assembly",
                        "Quality Verification",
                        "ATF Compliance Documentation"
                    },
                    RequiredApprovals = new[] { "Engineering", "Compliance", "Quality" },
                    EstimatedDuration = TimeSpan.FromDays(21), // 3 weeks including ATF
                    RequiresATFCompliance = true,
                    RequiresSerialNumber = true
                },

                ["BT_Firearm_Workflow"] = new BTWorkflowDefinition
                {
                    Name = "B&T Firearm Workflow", 
                    Description = "Complete workflow for firearm components with FFL tracking",
                    Stages = new[]
                    {
                        "Design & Engineering",
                        "FFL Manufacturing Authorization",
                        "SLS Metal Printing",
                        "CNC Precision Machining",
                        "Heat Treatment & Finishing",
                        "Proof Testing",
                        "Final Assembly",
                        "FFL Documentation & Serialization"
                    },
                    RequiredApprovals = new[] { "Engineering", "Compliance", "Quality", "FFL" },
                    EstimatedDuration = TimeSpan.FromDays(14), // 2 weeks
                    RequiresATFCompliance = true,
                    RequiresSerialNumber = true
                },

                ["BT_Regulated_Workflow"] = new BTWorkflowDefinition
                {
                    Name = "B&T Regulated Component Workflow",
                    Description = "Workflow for export-controlled and regulated components",
                    Stages = new[]
                    {
                        "Design & Engineering",
                        "Export Control Review",
                        "ITAR Compliance Verification",
                        "SLS Metal Printing",
                        "Secondary Operations",
                        "Compliance Testing",
                        "Documentation & Certification",
                        "Secure Storage/Handling"
                    },
                    RequiredApprovals = new[] { "Engineering", "Compliance", "Export Control" },
                    EstimatedDuration = TimeSpan.FromDays(10),
                    RequiresATFCompliance = false,
                    RequiresSerialNumber = false
                },

                ["BT_Standard_Workflow"] = new BTWorkflowDefinition
                {
                    Name = "B&T Standard Manufacturing Workflow",
                    Description = "Standard workflow for general B&T components",
                    Stages = new[]
                    {
                        "Design & Engineering",
                        "SLS Metal Printing",
                        "Post-Processing",
                        "Quality Inspection",
                        "Final Approval"
                    },
                    RequiredApprovals = new[] { "Engineering", "Quality" },
                    EstimatedDuration = TimeSpan.FromDays(7), // 1 week
                    RequiresATFCompliance = false,
                    RequiresSerialNumber = false
                },

                ["BT_Prototype_Workflow"] = new BTWorkflowDefinition
                {
                    Name = "B&T Prototype Development Workflow",
                    Description = "Rapid prototyping workflow for B&T development",
                    Stages = new[]
                    {
                        "Concept Design",
                        "Rapid Prototyping (SLS)",
                        "Initial Testing",
                        "Design Iteration",
                        "Validation Testing",
                        "Documentation"
                    },
                    RequiredApprovals = new[] { "Engineering" },
                    EstimatedDuration = TimeSpan.FromDays(5),
                    RequiresATFCompliance = false,
                    RequiresSerialNumber = false
                }
            };
        }

        /// <summary>
        /// Initialize B&T manufacturing workflow for a part
        /// </summary>
        public async Task<BTWorkflowInstance> InitializeBTWorkflow(Part part)
        {
            _logger.LogInformation("?? Initializing B&T workflow for part: {PartNumber}", part.PartNumber);

            var workflows = GetAvailableBTWorkflows();
            var workflowKey = part.WorkflowTemplate ?? "BT_Standard_Workflow";
            
            if (!workflows.ContainsKey(workflowKey))
            {
                _logger.LogWarning("?? Unknown workflow template: {WorkflowTemplate}, using standard", workflowKey);
                workflowKey = "BT_Standard_Workflow";
            }

            var workflowDef = workflows[workflowKey];
            var instance = new BTWorkflowInstance
            {
                PartId = part.Id,
                PartNumber = part.PartNumber,
                WorkflowTemplate = workflowKey,
                WorkflowName = workflowDef.Name,
                CurrentStage = workflowDef.Stages.First(),
                CurrentStageIndex = 0,
                TotalStages = workflowDef.Stages.Length,
                Status = "Active",
                StartedDate = DateTime.UtcNow,
                EstimatedCompletionDate = DateTime.UtcNow.Add(workflowDef.EstimatedDuration),
                RequiredApprovals = workflowDef.RequiredApprovals.ToList(),
                CompletedApprovals = new List<string>(),
                WorkflowData = JsonSerializer.Serialize(new
                {
                    Stages = workflowDef.Stages.Select((stage, index) => new
                    {
                        Index = index,
                        Name = stage,
                        Status = index == 0 ? "Active" : "Pending",
                        StartedDate = index == 0 ? DateTime.UtcNow : (DateTime?)null,
                        CompletedDate = (DateTime?)null
                    }).ToArray()
                })
            };

            _logger.LogInformation("? B&T workflow initialized: {WorkflowName} with {StageCount} stages", 
                workflowDef.Name, workflowDef.Stages.Length);

            return instance;
        }

        /// <summary>
        /// Advance B&T workflow to next stage
        /// </summary>
        public async Task<bool> AdvanceBTWorkflowStage(int partId, string approvedBy, string notes = "")
        {
            _logger.LogInformation("?? Advancing B&T workflow stage for part ID: {PartId}", partId);

            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
            {
                _logger.LogError("? Part not found: {PartId}", partId);
                return false;
            }

            var workflows = GetAvailableBTWorkflows();
            var workflowKey = part.WorkflowTemplate ?? "BT_Standard_Workflow";
            
            if (!workflows.ContainsKey(workflowKey))
            {
                _logger.LogError("? Invalid workflow template: {WorkflowTemplate}", workflowKey);
                return false;
            }

            var workflowDef = workflows[workflowKey];
            var currentStageIndex = part.StageOrder - 1; // Convert to 0-based index

            // Validate stage advancement
            if (currentStageIndex >= workflowDef.Stages.Length - 1)
            {
                _logger.LogWarning("?? Workflow already at final stage for part: {PartNumber}", part.PartNumber);
                return false;
            }

            // Check required approvals for current stage
            var requiredApprovals = GetRequiredApprovalsForStage(workflowKey, currentStageIndex);
            var missingApprovals = requiredApprovals.Where(approval => 
                !HasApproval(part, approval)).ToList();

            if (missingApprovals.Any())
            {
                _logger.LogWarning("?? Missing required approvals: {MissingApprovals}", 
                    string.Join(", ", missingApprovals));
                return false;
            }

            // Advance to next stage
            var nextStageIndex = currentStageIndex + 1;
            var nextStage = workflowDef.Stages[nextStageIndex];
            
            part.StageOrder = nextStageIndex + 1; // Convert back to 1-based
            part.ManufacturingStage = GetShortStageName(nextStage);
            part.LastModifiedBy = approvedBy;
            part.LastModifiedDate = DateTime.UtcNow;

            // Update stage details
            var stageDetails = new
            {
                Stage = nextStage,
                AdvancedDate = DateTime.UtcNow,
                AdvancedBy = approvedBy,
                Notes = notes,
                PreviousStage = workflowDef.Stages[currentStageIndex]
            };
            part.StageDetails = JsonSerializer.Serialize(stageDetails);

            await _context.SaveChangesAsync();

            _logger.LogInformation("? Advanced to stage {StageIndex}/{TotalStages}: {StageName}", 
                nextStageIndex + 1, workflowDef.Stages.Length, nextStage);

            return true;
        }

        /// <summary>
        /// Complete B&T manufacturing workflow
        /// </summary>
        public async Task<bool> CompleteBTWorkflow(int partId, string completedBy, string finalNotes = "")
        {
            _logger.LogInformation("?? Completing B&T workflow for part ID: {PartId}", partId);

            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
            {
                _logger.LogError("? Part not found: {PartId}", partId);
                return false;
            }

            // Validate all required approvals are complete
            var workflows = GetAvailableBTWorkflows();
            var workflowKey = part.WorkflowTemplate ?? "BT_Standard_Workflow";
            var workflowDef = workflows[workflowKey];

            var missingApprovals = workflowDef.RequiredApprovals.Where(approval => 
                !HasApproval(part, approval)).ToList();

            if (missingApprovals.Any())
            {
                _logger.LogWarning("?? Cannot complete workflow. Missing approvals: {MissingApprovals}", 
                    string.Join(", ", missingApprovals));
                return false;
            }

            // Complete workflow
            part.ManufacturingStage = "Complete";
            part.StageOrder = workflowDef.Stages.Length;
            part.LastModifiedBy = completedBy;
            part.LastModifiedDate = DateTime.UtcNow;

            // Update completion details
            var completionDetails = new
            {
                CompletedDate = DateTime.UtcNow,
                CompletedBy = completedBy,
                FinalNotes = finalNotes,
                WorkflowTemplate = workflowKey,
                TotalStages = workflowDef.Stages.Length,
                Duration = DateTime.UtcNow - part.CreatedDate
            };
            part.StageDetails = JsonSerializer.Serialize(completionDetails);

            await _context.SaveChangesAsync();

            _logger.LogInformation("? B&T workflow completed for part: {PartNumber}", part.PartNumber);

            return true;
        }

        /// <summary>
        /// Get B&T workflow status and progress
        /// </summary>
        public BTWorkflowStatus GetBTWorkflowStatus(Part part)
        {
            var workflows = GetAvailableBTWorkflows();
            var workflowKey = part.WorkflowTemplate ?? "BT_Standard_Workflow";
            
            if (!workflows.ContainsKey(workflowKey))
                workflowKey = "BT_Standard_Workflow";

            var workflowDef = workflows[workflowKey];
            var currentStageIndex = part.StageOrder - 1;

            var status = new BTWorkflowStatus
            {
                PartNumber = part.PartNumber,
                WorkflowName = workflowDef.Name,
                CurrentStage = part.ManufacturingStage,
                CurrentStageIndex = currentStageIndex,
                TotalStages = workflowDef.Stages.Length,
                ProgressPercentage = (currentStageIndex + 1) * 100 / workflowDef.Stages.Length,
                IsComplete = part.ManufacturingStage == "Complete",
                EstimatedCompletionDate = part.CreatedDate.Add(workflowDef.EstimatedDuration),
                RequiredApprovals = workflowDef.RequiredApprovals.ToList(),
                CompletedApprovals = GetCompletedApprovals(part),
                NextStage = currentStageIndex < workflowDef.Stages.Length - 1 ? 
                    workflowDef.Stages[currentStageIndex + 1] : null,
                AllStages = workflowDef.Stages.ToList()
            };

            return status;
        }

        #endregion

        #region Helper Methods

        private string GetShortStageName(string fullStageName)
        {
            // Convert full stage names to short codes for storage
            var stageMap = new Dictionary<string, string>
            {
                ["Design & Engineering"] = "Design",
                ["ATF Form 1 Submission"] = "ATF-Form1",
                ["SLS Metal Printing"] = "SLS-Primary",
                ["Secondary Operations (CNC/EDM)"] = "Secondary-Ops",
                ["Sound Testing & Validation"] = "Testing",
                ["Final Assembly"] = "Assembly",
                ["Quality Verification"] = "Quality",
                ["ATF Compliance Documentation"] = "ATF-Docs",
                ["FFL Manufacturing Authorization"] = "FFL-Auth",
                ["CNC Precision Machining"] = "CNC",
                ["Heat Treatment & Finishing"] = "Finishing",
                ["Proof Testing"] = "Proof-Test",
                ["FFL Documentation & Serialization"] = "FFL-Serial",
                ["Export Control Review"] = "Export-Review",
                ["ITAR Compliance Verification"] = "ITAR",
                ["Compliance Testing"] = "Compliance-Test",
                ["Documentation & Certification"] = "Documentation",
                ["Secure Storage/Handling"] = "Secure-Storage",
                ["Post-Processing"] = "Post-Process",
                ["Quality Inspection"] = "QC",
                ["Final Approval"] = "Final-Approval",
                ["Concept Design"] = "Concept",
                ["Rapid Prototyping (SLS)"] = "Prototype",
                ["Initial Testing"] = "Initial-Test",
                ["Design Iteration"] = "Iteration",
                ["Validation Testing"] = "Validation"
            };

            return stageMap.ContainsKey(fullStageName) ? stageMap[fullStageName] : fullStageName;
        }

        private List<string> GetRequiredApprovalsForStage(string workflowKey, int stageIndex)
        {
            // Define stage-specific approval requirements
            var approvalRequirements = new Dictionary<string, Dictionary<int, string[]>>
            {
                ["BT_Suppressor_Workflow"] = new Dictionary<int, string[]>
                {
                    [0] = new[] { "Engineering" },
                    [1] = new[] { "Compliance" },
                    [4] = new[] { "Quality" },
                    [7] = new[] { "Compliance", "Quality" }
                },
                ["BT_Firearm_Workflow"] = new Dictionary<int, string[]>
                {
                    [0] = new[] { "Engineering" },
                    [1] = new[] { "FFL", "Compliance" },
                    [5] = new[] { "Quality" },
                    [7] = new[] { "FFL", "Compliance" }
                },
                ["BT_Regulated_Workflow"] = new Dictionary<int, string[]>
                {
                    [0] = new[] { "Engineering" },
                    [1] = new[] { "Export Control" },
                    [2] = new[] { "Compliance" },
                    [6] = new[] { "Compliance", "Export Control" }
                }
            };

            if (approvalRequirements.ContainsKey(workflowKey) && 
                approvalRequirements[workflowKey].ContainsKey(stageIndex))
            {
                return approvalRequirements[workflowKey][stageIndex].ToList();
            }

            return new List<string>();
        }

        private bool HasApproval(Part part, string approvalType)
        {
            // Check if specific approval has been granted
            // This would typically check against an approvals table
            // For now, using part flags as proxy
            return approvalType switch
            {
                "Engineering" => part.RequiresEngineeringApproval && !string.IsNullOrEmpty(part.LastModifiedBy),
                "Quality" => part.RequiresQualityApproval && !string.IsNullOrEmpty(part.LastModifiedBy),
                "Compliance" => part.RequiresComplianceApproval && !string.IsNullOrEmpty(part.LastModifiedBy),
                "FFL" => part.RequiresFFLTracking && !string.IsNullOrEmpty(part.LastModifiedBy),
                "Export Control" => part.RequiresExportLicense && !string.IsNullOrEmpty(part.LastModifiedBy),
                _ => true // Default to approved if not specifically required
            };
        }

        private List<string> GetCompletedApprovals(Part part)
        {
            var completed = new List<string>();

            if (part.RequiresEngineeringApproval && !string.IsNullOrEmpty(part.LastModifiedBy))
                completed.Add("Engineering");
            if (part.RequiresQualityApproval && !string.IsNullOrEmpty(part.LastModifiedBy))
                completed.Add("Quality");
            if (part.RequiresComplianceApproval && !string.IsNullOrEmpty(part.LastModifiedBy))
                completed.Add("Compliance");
            if (part.RequiresFFLTracking && !string.IsNullOrEmpty(part.LastModifiedBy))
                completed.Add("FFL");
            if (part.RequiresExportLicense && !string.IsNullOrEmpty(part.LastModifiedBy))
                completed.Add("Export Control");

            return completed;
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// B&T Workflow Definition
    /// </summary>
    public class BTWorkflowDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Stages { get; set; } = Array.Empty<string>();
        public string[] RequiredApprovals { get; set; } = Array.Empty<string>();
        public TimeSpan EstimatedDuration { get; set; }
        public bool RequiresATFCompliance { get; set; }
        public bool RequiresSerialNumber { get; set; }
    }

    /// <summary>
    /// B&T Workflow Instance
    /// </summary>
    public class BTWorkflowInstance
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string WorkflowTemplate { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public int CurrentStageIndex { get; set; }
        public int TotalStages { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime StartedDate { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<string> RequiredApprovals { get; set; } = new();
        public List<string> CompletedApprovals { get; set; } = new();
        public string WorkflowData { get; set; } = string.Empty;
    }

    /// <summary>
    /// B&T Workflow Status
    /// </summary>
    public class BTWorkflowStatus
    {
        public string PartNumber { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public int CurrentStageIndex { get; set; }
        public int TotalStages { get; set; }
        public int ProgressPercentage { get; set; }
        public bool IsComplete { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public List<string> RequiredApprovals { get; set; } = new();
        public List<string> CompletedApprovals { get; set; } = new();
        public string? NextStage { get; set; }
        public List<string> AllStages { get; set; } = new();
    }

    #endregion
}