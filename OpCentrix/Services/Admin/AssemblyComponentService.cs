using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for managing assembly components needed in prototype jobs
    /// Handles end caps, springs, baffles, mounting hardware, etc.
    /// </summary>
    public class AssemblyComponentService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<AssemblyComponentService> _logger;

        public AssemblyComponentService(SchedulerContext context, ILogger<AssemblyComponentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Component Management

        /// <summary>
        /// Gets all assembly components for a prototype job
        /// </summary>
        public async Task<List<AssemblyComponent>> GetComponentsForJobAsync(int prototypeJobId)
        {
            return await _context.AssemblyComponents
                .Where(ac => ac.PrototypeJobId == prototypeJobId && ac.IsActive)
                .OrderBy(ac => ac.ComponentType)
                .ThenBy(ac => ac.ComponentDescription)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a specific assembly component by ID
        /// </summary>
        public async Task<AssemblyComponent?> GetComponentAsync(int componentId)
        {
            return await _context.AssemblyComponents
                .Include(ac => ac.PrototypeJob)
                    .ThenInclude(pj => pj!.Part)
                .FirstOrDefaultAsync(ac => ac.Id == componentId);
        }

        /// <summary>
        /// Adds a new assembly component to a prototype job
        /// </summary>
        public async Task<bool> AddComponentAsync(AssemblyComponent component)
        {
            try
            {
                component.CreatedDate = DateTime.UtcNow;
                component.IsActive = true;

                // Calculate total cost if unit cost and quantity are provided
                if (component.UnitCost.HasValue && component.QuantityRequired > 0)
                {
                    component.TotalCost = component.UnitCost.Value * component.QuantityRequired;
                }

                _context.AssemblyComponents.Add(component);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added assembly component {ComponentType} to prototype job {PrototypeJobId}", 
                    component.ComponentType, component.PrototypeJobId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding assembly component to prototype job {PrototypeJobId}", 
                    component.PrototypeJobId);
                return false;
            }
        }

        /// <summary>
        /// Updates an existing assembly component
        /// </summary>
        public async Task<bool> UpdateComponentAsync(AssemblyComponent component)
        {
            try
            {
                // Recalculate total cost
                if (component.UnitCost.HasValue && component.QuantityRequired > 0)
                {
                    component.TotalCost = component.UnitCost.Value * component.QuantityRequired;
                }

                _context.AssemblyComponents.Update(component);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated assembly component {ComponentId}", component.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assembly component {ComponentId}", component.Id);
                return false;
            }
        }

        /// <summary>
        /// Removes an assembly component (soft delete)
        /// </summary>
        public async Task<bool> RemoveComponentAsync(int componentId)
        {
            try
            {
                var component = await _context.AssemblyComponents.FindAsync(componentId);
                if (component == null)
                {
                    _logger.LogWarning("Assembly component not found: {ComponentId}", componentId);
                    return false;
                }

                component.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed assembly component {ComponentId}", componentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing assembly component {ComponentId}", componentId);
                return false;
            }
        }

        /// <summary>
        /// Updates the status of an assembly component
        /// </summary>
        public async Task<bool> UpdateComponentStatusAsync(int componentId, string status, DateTime? statusDate = null)
        {
            try
            {
                var component = await _context.AssemblyComponents.FindAsync(componentId);
                if (component == null)
                {
                    _logger.LogWarning("Assembly component not found: {ComponentId}", componentId);
                    return false;
                }

                var previousStatus = component.Status;
                component.Status = status;

                var changeDate = statusDate ?? DateTime.UtcNow;

                // Update appropriate date fields based on status
                switch (status.ToLower())
                {
                    case "ordered":
                        component.OrderDate = changeDate;
                        break;
                    case "received":
                        component.ReceivedDate = changeDate;
                        if (!component.OrderDate.HasValue)
                        {
                            component.OrderDate = changeDate; // Assume it was ordered on the same day if not set
                        }
                        break;
                    case "used":
                        component.UsedDate = changeDate;
                        if (!component.ReceivedDate.HasValue)
                        {
                            component.ReceivedDate = changeDate; // Assume it was received on the same day if not set
                        }
                        break;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated component {ComponentId} status from {PreviousStatus} to {NewStatus}", 
                    componentId, previousStatus, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating component status {ComponentId}", componentId);
                return false;
            }
        }

        /// <summary>
        /// Bulk updates component statuses
        /// </summary>
        public async Task<bool> BulkUpdateStatusAsync(List<int> componentIds, string status)
        {
            try
            {
                var components = await _context.AssemblyComponents
                    .Where(ac => componentIds.Contains(ac.Id))
                    .ToListAsync();

                var changeDate = DateTime.UtcNow;

                foreach (var component in components)
                {
                    component.Status = status;

                    switch (status.ToLower())
                    {
                        case "ordered":
                            component.OrderDate = changeDate;
                            break;
                        case "received":
                            component.ReceivedDate = changeDate;
                            if (!component.OrderDate.HasValue)
                            {
                                component.OrderDate = changeDate;
                            }
                            break;
                        case "used":
                            component.UsedDate = changeDate;
                            if (!component.ReceivedDate.HasValue)
                            {
                                component.ReceivedDate = changeDate;
                            }
                            break;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk updated {Count} components to status {Status}", 
                    components.Count, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating component statuses");
                return false;
            }
        }

        #endregion

        #region Component Templates and Defaults

        /// <summary>
        /// Gets standard component templates for different part types
        /// </summary>
        public async Task<List<ComponentTemplate>> GetComponentTemplatesAsync(string componentType = "")
        {
            var templates = GetStandardComponentTemplates();

            if (!string.IsNullOrEmpty(componentType))
            {
                templates = templates.Where(t => t.ComponentType.Equals(componentType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return templates;
        }

        /// <summary>
        /// Creates default components for a prototype job based on part type
        /// </summary>
        public async Task<bool> CreateDefaultComponentsAsync(int prototypeJobId, string partType = "Suppressor")
        {
            try
            {
                var defaultComponents = GetDefaultComponentsForPartType(partType);

                foreach (var template in defaultComponents)
                {
                    var component = new AssemblyComponent
                    {
                        PrototypeJobId = prototypeJobId,
                        ComponentType = template.ComponentType,
                        ComponentDescription = template.Description,
                        QuantityRequired = template.DefaultQuantity,
                        UnitCost = template.EstimatedCost,
                        TotalCost = template.EstimatedCost * template.DefaultQuantity,
                        Supplier = template.RecommendedSupplier,
                        LeadTimeDays = template.EstimatedLeadTimeDays,
                        Status = "Needed",
                        InspectionRequired = template.RequiresInspection,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.AssemblyComponents.Add(component);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} default components for prototype job {PrototypeJobId}", 
                    defaultComponents.Count, prototypeJobId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default components for prototype job {PrototypeJobId}", 
                    prototypeJobId);
                return false;
            }
        }

        #endregion

        #region Component Analytics

        /// <summary>
        /// Gets component readiness summary for a prototype job
        /// </summary>
        public async Task<ComponentReadinessSummary> GetComponentReadinessAsync(int prototypeJobId)
        {
            var components = await GetComponentsForJobAsync(prototypeJobId);

            var summary = new ComponentReadinessSummary
            {
                PrototypeJobId = prototypeJobId,
                TotalComponents = components.Count,
                NeededComponents = components.Count(c => c.Status == "Needed"),
                OrderedComponents = components.Count(c => c.Status == "Ordered"),
                ReceivedComponents = components.Count(c => c.Status == "Received"),
                UsedComponents = components.Count(c => c.Status == "Used"),
                TotalEstimatedCost = components.Sum(c => c.TotalCost ?? 0)
            };

            summary.ReadinessPercentage = summary.TotalComponents > 0 
                ? (decimal)(summary.ReceivedComponents + summary.UsedComponents) / summary.TotalComponents * 100 
                : 100;

            summary.ComponentsAwaitingOrder = components
                .Where(c => c.Status == "Needed")
                .Select(c => c.ComponentDescription)
                .ToList();

            summary.ComponentsAwaitingDelivery = components
                .Where(c => c.Status == "Ordered")
                .Select(c => new ComponentDeliveryInfo
                {
                    Description = c.ComponentDescription,
                    Supplier = c.Supplier ?? "Unknown",
                    OrderDate = c.OrderDate,
                    EstimatedDeliveryDate = c.OrderDate?.AddDays(c.LeadTimeDays ?? 7)
                })
                .ToList();

            return summary;
        }

        /// <summary>
        /// Gets components that are overdue for delivery
        /// </summary>
        public async Task<List<OverdueComponent>> GetOverdueComponentsAsync()
        {
            var orderedComponents = await _context.AssemblyComponents
                .Include(ac => ac.PrototypeJob)
                    .ThenInclude(pj => pj!.Part)
                .Where(ac => ac.IsActive && ac.Status == "Ordered" && ac.OrderDate.HasValue)
                .ToListAsync();

            var overdueComponents = new List<OverdueComponent>();

            foreach (var component in orderedComponents)
            {
                var estimatedDeliveryDate = component.OrderDate!.Value.AddDays(component.LeadTimeDays ?? 7);
                if (estimatedDeliveryDate < DateTime.UtcNow)
                {
                    overdueComponents.Add(new OverdueComponent
                    {
                        ComponentId = component.Id,
                        PrototypeNumber = component.PrototypeJob?.PrototypeNumber ?? "Unknown",
                        PartNumber = component.PrototypeJob?.Part?.PartNumber ?? "Unknown",
                        ComponentDescription = component.ComponentDescription,
                        Supplier = component.Supplier ?? "Unknown",
                        OrderDate = component.OrderDate.Value,
                        EstimatedDeliveryDate = estimatedDeliveryDate,
                        DaysOverdue = (int)(DateTime.UtcNow - estimatedDeliveryDate).TotalDays
                    });
                }
            }

            return overdueComponents.OrderByDescending(oc => oc.DaysOverdue).ToList();
        }

        #endregion

        #region Private Helper Methods

        private List<ComponentTemplate> GetStandardComponentTemplates()
        {
            return new List<ComponentTemplate>
            {
                new ComponentTemplate
                {
                    ComponentType = "EndCap",
                    Description = "Titanium End Cap - Standard",
                    DefaultQuantity = 2,
                    EstimatedCost = 45.00m,
                    RecommendedSupplier = "TiParts Inc",
                    EstimatedLeadTimeDays = 5,
                    RequiresInspection = true
                },
                new ComponentTemplate
                {
                    ComponentType = "Spring",
                    Description = "17-4 PH Stainless Steel Spring",
                    DefaultQuantity = 1,
                    EstimatedCost = 12.50m,
                    RecommendedSupplier = "Springs Plus",
                    EstimatedLeadTimeDays = 3,
                    RequiresInspection = false
                },
                new ComponentTemplate
                {
                    ComponentType = "Hardware",
                    Description = "Mounting Hardware Kit",
                    DefaultQuantity = 1,
                    EstimatedCost = 15.00m,
                    RecommendedSupplier = "FastCorp",
                    EstimatedLeadTimeDays = 2,
                    RequiresInspection = false
                },
                new ComponentTemplate
                {
                    ComponentType = "O-Ring",
                    Description = "High-Temperature O-Ring Kit",
                    DefaultQuantity = 1,
                    EstimatedCost = 8.75m,
                    RecommendedSupplier = "Seals Company",
                    EstimatedLeadTimeDays = 3,
                    RequiresInspection = false
                },
                new ComponentTemplate
                {
                    ComponentType = "ThreadInsert",
                    Description = "Helicoil Thread Insert - 1/2-28",
                    DefaultQuantity = 2,
                    EstimatedCost = 15.00m,
                    RecommendedSupplier = "Helicoil",
                    EstimatedLeadTimeDays = 7,
                    RequiresInspection = true
                }
            };
        }

        private List<ComponentTemplate> GetDefaultComponentsForPartType(string partType)
        {
            var allTemplates = GetStandardComponentTemplates();

            return partType.ToLower() switch
            {
                "suppressor" => allTemplates.Where(t => new[] { "EndCap", "Spring", "O-Ring" }.Contains(t.ComponentType)).ToList(),
                "receiver" => allTemplates.Where(t => new[] { "Hardware", "ThreadInsert" }.Contains(t.ComponentType)).ToList(),
                "muzzlebrake" => allTemplates.Where(t => new[] { "Hardware" }.Contains(t.ComponentType)).ToList(),
                _ => allTemplates.Where(t => new[] { "EndCap", "Hardware" }.Contains(t.ComponentType)).ToList()
            };
        }

        #endregion

        /// <summary>
        /// Get assembly components for a prototype job
        /// </summary>
        public async Task<List<AssemblyComponent>> GetComponentsAsync(int prototypeJobId)
        {
            try
            {
                return await _context.AssemblyComponents
                    .Where(ac => ac.PrototypeJobId == prototypeJobId && ac.IsActive)
                    .OrderBy(ac => ac.ComponentType)
                    .ThenBy(ac => ac.ComponentDescription)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assembly components for prototype job {PrototypeJobId}", prototypeJobId);
                return new List<AssemblyComponent>();
            }
        }
    }

    #region Supporting Classes

    /// <summary>
    /// Template for creating standard assembly components
    /// </summary>
    public class ComponentTemplate
    {
        public string ComponentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DefaultQuantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public string RecommendedSupplier { get; set; } = string.Empty;
        public int EstimatedLeadTimeDays { get; set; }
        public bool RequiresInspection { get; set; }
    }

    /// <summary>
    /// Summary of component readiness for a prototype job
    /// </summary>
    public class ComponentReadinessSummary
    {
        public int PrototypeJobId { get; set; }
        public int TotalComponents { get; set; }
        public int NeededComponents { get; set; }
        public int OrderedComponents { get; set; }
        public int ReceivedComponents { get; set; }
        public int UsedComponents { get; set; }
        public decimal ReadinessPercentage { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public List<string> ComponentsAwaitingOrder { get; set; } = new List<string>();
        public List<ComponentDeliveryInfo> ComponentsAwaitingDelivery { get; set; } = new List<ComponentDeliveryInfo>();
    }

    /// <summary>
    /// Information about a component awaiting delivery
    /// </summary>
    public class ComponentDeliveryInfo
    {
        public string Description { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }

    /// <summary>
    /// Information about an overdue component
    /// </summary>
    public class OverdueComponent
    {
        public int ComponentId { get; set; }
        public string PrototypeNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string ComponentDescription { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    #endregion
}