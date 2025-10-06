using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using OpCentrix.Data;
using OpCentrix.Models.Maintenance;
using OpCentrix.ViewModels.Maintenance;

namespace OpCentrix.Services.Maintenance
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<MaintenanceService> _logger;
        private readonly MaintenanceCustomService _customService;
        private readonly IMemoryCache _cache;

        private bool _schemaChecked = false;
        private bool _schemaAvailable = true;

        private const string FLEET_STATUS_CACHE_KEY = "maintenance_fleet_status";
        private const string MACHINE_COMPONENTS_CACHE_KEY = "machine_components_{0}";
        private const string MAINTENANCE_RULES_CACHE_KEY = "maintenance_rules_{0}";
        private const int CACHE_DURATION_MINUTES = 5;

        public MaintenanceService(
            SchedulerContext context, 
            ILogger<MaintenanceService> logger, 
            ILoggerFactory loggerFactory,
            IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _customService = new MaintenanceCustomService(context, loggerFactory.CreateLogger<MaintenanceCustomService>());
        }

        #region Schema Validation

        private async Task<bool> EnsureSchemaAsync()
        {
            if (_schemaChecked) return _schemaAvailable;
            
            _schemaChecked = true;
            
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                // Check all maintenance tables exist and are accessible
                var tableChecks = new[]
                {
                    "SELECT 1 FROM MaintenanceRules LIMIT 1",
                    "SELECT 1 FROM MaintenanceStates LIMIT 1", 
                    "SELECT 1 FROM MaintenanceActionLogs LIMIT 1",
                    "SELECT 1 FROM MachineComponents LIMIT 1",
                    "SELECT 1 FROM MaintenanceServices LIMIT 1",
                    "SELECT 1 FROM MaintenanceWorkOrders LIMIT 1",
                    "SELECT 1 FROM MaintenanceSchedules LIMIT 1",
                    "SELECT 1 FROM MaintenanceNotifications LIMIT 1"
                };

                foreach (var sql in tableChecks)
                {
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }

                await transaction.CommitAsync();
                _schemaAvailable = true;
                
                _logger.LogDebug("Maintenance schema validation successful");
            }
            catch (Exception ex)
            {
                _schemaAvailable = false;
                _logger.LogWarning(ex, "Maintenance schema not fully present. Some features may be disabled until migrations are applied");
            }
            
            return _schemaAvailable;
        }

        #endregion

        #region Machine Status and Fleet Management

        public async Task<List<RuleStatusDto>> GetMachineStatusAsync(string machineId, bool includeComponents = false)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                throw new ArgumentException("Machine ID cannot be null or empty", nameof(machineId));

            if (!await EnsureSchemaAsync())
                return new List<RuleStatusDto>();

            try
            {
                var cacheKey = $"machine_status_{machineId}_{includeComponents}";
                if (_cache.TryGetValue(cacheKey, out List<RuleStatusDto> cachedResult))
                {
                    return cachedResult;
                }

                // Single query to get all required data
                var query = from rule in _context.MaintenanceRules
                           join state in _context.MaintenanceStates on new { rule.Id, MachineId = machineId } 
                               equals new { Id = state.RuleId, state.MachineId } into stateGroup
                           from state in stateGroup.DefaultIfEmpty()
                           where rule.IsActive && (rule.MachineId == machineId || rule.MachineId == null)
                           select new { Rule = rule, State = state };

                if (!includeComponents)
                {
                    query = query.Where(x => x.Rule.MachineComponentId == null && x.Rule.ProductionStageId == null);
                }

                var results = await query.ToListAsync();
                var statusList = new List<RuleStatusDto>();

                foreach (var item in results)
                {
                    var status = MapToStatusDto(item.Rule, item.State);
                    statusList.Add(status);
                }

                // Get component names if needed
                if (includeComponents && results.Any(r => r.Rule.MachineComponentId.HasValue))
                {
                    var componentIds = results.Where(r => r.Rule.MachineComponentId.HasValue)
                                             .Select(r => r.Rule.MachineComponentId!.Value)
                                             .Distinct()
                                             .ToList();

                    var components = await _context.MachineComponents
                        .Where(c => componentIds.Contains(c.Id))
                        .ToDictionaryAsync(c => c.Id, c => c.Name);

                    foreach (var status in statusList)
                    {
                        var rule = results.First(r => r.Rule.Id == status.RuleId).Rule;
                        if (rule.MachineComponentId.HasValue && components.TryGetValue(rule.MachineComponentId.Value, out var componentName))
                        {
                            status.ComponentName = componentName;
                        }
                    }
                }

                var orderedResults = statusList
                    .OrderByDescending(r => r.IsOverdue)
                    .ThenByDescending(r => r.IsDue)
                    .ThenByDescending(r => r.Percent)
                    .ToList();

                _cache.Set(cacheKey, orderedResults, TimeSpan.FromMinutes(2));
                return orderedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving machine status for {MachineId}", machineId);
                throw;
            }
        }

        public async Task<Dictionary<string, List<RuleStatusDto>>> GetFleetStatusAsync(bool includeComponents = false)
        {
            if (!await EnsureSchemaAsync()) 
                return new Dictionary<string, List<RuleStatusDto>>();

            try
            {
                var cacheKey = $"{FLEET_STATUS_CACHE_KEY}_{includeComponents}";
                if (_cache.TryGetValue(cacheKey, out Dictionary<string, List<RuleStatusDto>> cachedResult))
                {
                    return cachedResult;
                }

                var activeMachines = await _context.Machines
                    .Where(m => m.IsActive)
                    .Select(m => m.MachineId)
                    .ToListAsync();

                var fleetStatus = new Dictionary<string, List<RuleStatusDto>>();

                // Process machines in parallel for better performance
                var tasks = activeMachines.Select(async machineId =>
                {
                    var status = await GetMachineStatusAsync(machineId, includeComponents);
                    return new { MachineId = machineId, Status = status };
                });

                var results = await Task.WhenAll(tasks);

                foreach (var result in results)
                {
                    fleetStatus[result.MachineId] = result.Status;
                }

                _cache.Set(cacheKey, fleetStatus, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                return fleetStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fleet status");
                throw;
            }
        }

        #endregion

        #region Component Management

        public async Task<int> CreateComponentAsync(MachineComponent component, int userId)
        {
            ValidateComponent(component);
            ValidateUserId(userId);

            if (!await EnsureSchemaAsync())
                throw new InvalidOperationException("Maintenance schema not available");

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                component.CreatedAt = DateTime.UtcNow;
                component.CreatedBy = userId.ToString();

                _context.MachineComponents.Add(component);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear cache
                _cache.Remove(string.Format(MACHINE_COMPONENTS_CACHE_KEY, component.MachineId));

                _logger.LogInformation("Created component {ComponentName} for machine {MachineId} by user {UserId}", 
                    component.Name, component.MachineId, userId);

                return component.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating component {ComponentName} for machine {MachineId}", 
                    component.Name, component.MachineId);
                throw;
            }
        }

        public async Task<bool> UpdateComponentAsync(MachineComponent component, int userId)
        {
            ValidateComponent(component);
            ValidateUserId(userId);

            if (!await EnsureSchemaAsync()) return false;

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var existing = await _context.MachineComponents.FindAsync(component.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Component {ComponentId} not found for update", component.Id);
                    return false;
                }

                // Update properties
                existing.Name = component.Name;
                existing.Description = component.Description;
                existing.IsActive = component.IsActive;
                existing.DisplayOrder = component.DisplayOrder;
                existing.Category = component.Category;
                existing.Icon = component.Icon;
                existing.ColorCode = component.ColorCode;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear cache
                _cache.Remove(string.Format(MACHINE_COMPONENTS_CACHE_KEY, existing.MachineId));

                _logger.LogInformation("Updated component {ComponentId} by user {UserId}", component.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating component {ComponentId}", component.Id);
                throw;
            }
        }

        public async Task<List<MachineComponent>> GetComponentsAsync(string machineId, bool activeOnly = true)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                throw new ArgumentException("Machine ID cannot be null or empty", nameof(machineId));

            if (!await EnsureSchemaAsync()) 
                return new List<MachineComponent>();

            try
            {
                var cacheKey = string.Format(MACHINE_COMPONENTS_CACHE_KEY, $"{machineId}_{activeOnly}");
                if (_cache.TryGetValue(cacheKey, out List<MachineComponent> cachedResult))
                {
                    return cachedResult;
                }

                var query = _context.MachineComponents.Where(c => c.MachineId == machineId);
                if (activeOnly) 
                    query = query.Where(c => c.IsActive);

                var components = await query
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                _cache.Set(cacheKey, components, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                return components;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving components for machine {MachineId}", machineId);
                throw;
            }
        }

        #endregion

        #region Maintenance Rules

        public async Task<int> CreateRuleAsync(MaintenanceRule rule, int userId)
        {
            ValidateMaintenanceRule(rule);
            ValidateUserId(userId);

            if (!await EnsureSchemaAsync())
                throw new InvalidOperationException("Maintenance schema not available. Run migrations.");

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                rule.CreatedAt = DateTime.UtcNow;
                rule.CreatedBy = userId.ToString();
                rule.IsActive = true;

                _context.MaintenanceRules.Add(rule);
                await _context.SaveChangesAsync();

                // Create maintenance states for applicable machines
                await CreateMaintenanceStatesForRuleAsync(rule);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear cache
                ClearRulesCache(rule.MachineId);

                _logger.LogInformation("Created maintenance rule {RuleTitle} for machine {MachineId} by user {UserId}", 
                    rule.Title, rule.MachineId ?? "All", userId);

                return rule.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance rule {RuleTitle}", rule.Title);
                throw;
            }
        }

        public async Task<bool> UpdateRuleAsync(MaintenanceRule rule, int userId)
        {
            ValidateMaintenanceRule(rule);
            ValidateUserId(userId);

            if (!await EnsureSchemaAsync()) return false;

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var existing = await _context.MaintenanceRules.FindAsync(rule.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Maintenance rule {RuleId} not found for update", rule.Id);
                    return false;
                }

                // Update properties
                existing.Title = rule.Title;
                existing.Description = rule.Description;
                existing.TriggerType = rule.TriggerType;
                existing.Severity = rule.Severity;
                existing.ThresholdValue = rule.ThresholdValue;
                existing.IntervalDays = rule.IntervalDays;
                existing.IsActive = rule.IsActive;
                existing.EarlyWarningDays = rule.EarlyWarningDays;
                existing.EarlyWarningPercent = rule.EarlyWarningPercent;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear cache
                ClearRulesCache(existing.MachineId);

                _logger.LogInformation("Updated maintenance rule {RuleId} by user {UserId}", rule.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance rule {RuleId}", rule.Id);
                throw;
            }
        }

        public async Task<bool> DisableRuleAsync(int ruleId, int userId)
        {
            ValidateUserId(userId);

            if (!await EnsureSchemaAsync()) return false;

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var existing = await _context.MaintenanceRules.FindAsync(ruleId);
                if (existing == null)
                {
                    _logger.LogWarning("Maintenance rule {RuleId} not found for disable", ruleId);
                    return false;
                }

                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear cache
                ClearRulesCache(existing.MachineId);

                _logger.LogInformation("Disabled maintenance rule {RuleId} by user {UserId}", ruleId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling maintenance rule {RuleId}", ruleId);
                throw;
            }
        }

        public async Task<bool> LogActionAsync(int ruleId, string machineId, int userId, string notes, bool reset)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                throw new ArgumentException("Machine ID cannot be null or empty", nameof(machineId));
            ValidateUserId(userId);

            if (!await EnsureSchemaAsync()) return false;

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var state = await _context.MaintenanceStates
                    .FirstOrDefaultAsync(s => s.RuleId == ruleId && s.MachineId == machineId);

                if (state == null)
                {
                    await EnsureStateAsync(machineId, ruleId);
                    state = await _context.MaintenanceStates
                        .FirstAsync(s => s.RuleId == ruleId && s.MachineId == machineId);
                }

                _context.MaintenanceActionLogs.Add(new MaintenanceActionLog
                {
                    RuleId = ruleId,
                    MachineId = machineId,
                    Notes = notes,
                    PerformedAt = DateTime.UtcNow,
                    PerformedByUserId = userId,
                    ResetPerformed = reset
                });

                if (reset)
                {
                    state.CurrentValue = 0;
                    state.LastServiceDate = DateTime.UtcNow;
                    state.IsDue = false;
                    state.IsOverdue = false;
                    state.NextDueDate = null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear cache
                _cache.Remove($"machine_status_{machineId}_true");
                _cache.Remove($"machine_status_{machineId}_false");

                _logger.LogInformation("Logged maintenance action for rule {RuleId} on machine {MachineId} by user {UserId}, Reset: {Reset}", 
                    ruleId, machineId, userId, reset);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging maintenance action for rule {RuleId} on machine {MachineId}", ruleId, machineId);
                throw;
            }
        }

        public async Task RecalculateAsync(string? machineId = null, int? ruleId = null)
        {
            if (!await EnsureSchemaAsync()) return;

            try
            {
                var query = _context.MaintenanceRules.AsQueryable();
                if (machineId != null) 
                    query = query.Where(r => r.MachineId == machineId || r.MachineId == null);
                if (ruleId.HasValue) 
                    query = query.Where(r => r.Id == ruleId.Value);

                var rules = await query.Where(r => r.IsActive).ToListAsync();
                if (!rules.Any()) return;

                var machines = machineId != null 
                    ? new List<string> { machineId } 
                    : await _context.Machines.Where(m => m.IsActive).Select(m => m.MachineId).ToListAsync();

                var since = DateTime.UtcNow.AddDays(-90);
                var buildJobData = await _context.BuildJobs
                    .Where(b => b.ActualStartTime >= since)
                    .Select(b => new { 
                        b.PrinterName, 
                        b.ActualStartTime, 
                        b.ActualEndTime, 
                        b.Status, 
                        b.BuildId 
                    })
                    .ToListAsync();

                var buildsByMachine = buildJobData.GroupBy(b => b.PrinterName).ToDictionary(g => g.Key, g => g.ToList());

                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var machine in machines)
                {
                    var machineBuilds = buildsByMachine.TryGetValue(machine, out var buildData) 
                        ? buildData.Cast<object>().ToList() 
                        : new List<object>();

                    foreach (var rule in rules)
                    {
                        await EnsureStateAsync(machine, rule.Id);
                        var state = await _context.MaintenanceStates
                            .FirstAsync(s => s.MachineId == machine && s.RuleId == rule.Id);

                        state.CurrentValue = CalculateCurrentValue(rule, machineBuilds, state);
                        EvaluateFlags(rule, state);
                        state.CalculatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear all relevant caches
                _cache.Remove($"{FLEET_STATUS_CACHE_KEY}_true");
                _cache.Remove($"{FLEET_STATUS_CACHE_KEY}_false");
                
                foreach (var machine in machines)
                {
                    _cache.Remove($"machine_status_{machine}_true");
                    _cache.Remove($"machine_status_{machine}_false");
                }

                _logger.LogInformation("Recalculated maintenance data for {MachineCount} machines and {RuleCount} rules", 
                    machines.Count, rules.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during maintenance recalculation");
                throw;
            }
        }

        public async Task<MaintenanceRule?> GetRuleAsync(int id)
        {
            try
            {
                return await _context.MaintenanceRules
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance rule {RuleId}", id);
                throw;
            }
        }

        public async Task<List<MaintenanceRule>> GetRulesAsync(string? machineId = null, bool activeOnly = false)
        {
            try
            {
                var cacheKey = string.Format(MAINTENANCE_RULES_CACHE_KEY, $"{machineId}_{activeOnly}");
                if (_cache.TryGetValue(cacheKey, out List<MaintenanceRule> cachedResult))
                {
                    return cachedResult;
                }

                var query = _context.MaintenanceRules.AsQueryable();
                if (machineId != null) 
                    query = query.Where(r => r.MachineId == machineId || r.MachineId == null);
                if (activeOnly) 
                    query = query.Where(r => r.IsActive);

                var rules = await query
                    .OrderBy(r => r.MachineId)
                    .ThenBy(r => r.Title)
                    .ToListAsync();

                _cache.Set(cacheKey, rules, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                return rules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance rules for machine {MachineId}", machineId);
                throw;
            }
        }

        #endregion

        #region Enhanced Maintenance Services

        public async Task<int> CreateMaintenanceServiceAsync(OpCentrix.Models.Maintenance.MaintenanceService service, int userId)
        {
            ValidateMaintenanceService(service);
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                service.CreatedAt = DateTime.UtcNow;
                service.CreatedBy = userId.ToString();
                service.LastUpdateTime = DateTime.UtcNow;

                _context.MaintenanceServices.Add(service);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created maintenance service {ServiceName} for machine {MachineId} by user {UserId}", 
                    service.ServiceName, service.MachineId, userId);

                return service.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance service {ServiceName}", service.ServiceName);
                throw;
            }
        }

        public async Task<bool> UpdateMaintenanceServiceAsync(OpCentrix.Models.Maintenance.MaintenanceService service, int userId)
        {
            ValidateMaintenanceService(service);
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var existing = await _context.MaintenanceServices.FindAsync(service.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Maintenance service {ServiceId} not found for update", service.Id);
                    return false;
                }

                // Update properties
                existing.ServiceName = service.ServiceName;
                existing.Description = service.Description;
                existing.ServiceType = service.ServiceType;
                existing.IsEnabled = service.IsEnabled;
                existing.UpdateIntervalMinutes = service.UpdateIntervalMinutes;
                existing.DataSource = service.DataSource;
                existing.Configuration = service.Configuration;
                existing.WarningThreshold = service.WarningThreshold;
                existing.CriticalThreshold = service.CriticalThreshold;
                existing.MaxValue = service.MaxValue;
                existing.Unit = service.Unit;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated maintenance service {ServiceId} by user {UserId}", service.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance service {ServiceId}", service.Id);
                throw;
            }
        }

        public async Task<bool> DeleteMaintenanceServiceAsync(int serviceId, int userId)
        {
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var service = await _context.MaintenanceServices.FindAsync(serviceId);
                if (service == null)
                {
                    _logger.LogWarning("Maintenance service {ServiceId} not found for deletion", serviceId);
                    return false;
                }

                // Soft delete by disabling
                service.IsEnabled = false;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Disabled maintenance service {ServiceId} by user {UserId}", serviceId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance service {ServiceId}", serviceId);
                throw;
            }
        }

        public async Task<List<OpCentrix.Models.Maintenance.MaintenanceService>> GetMaintenanceServicesAsync(string? machineId = null)
        {
            try
            {
                var query = _context.MaintenanceServices.AsQueryable();
                if (machineId != null)
                    query = query.Where(s => s.MachineId == machineId);

                return await query
                    .OrderBy(s => s.MachineId)
                    .ThenBy(s => s.ServiceName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance services for machine {MachineId}", machineId);
                throw;
            }
        }

        public async Task<OpCentrix.Models.Maintenance.MaintenanceService?> GetMaintenanceServiceAsync(int serviceId)
        {
            try
            {
                return await _context.MaintenanceServices
                    .Include(s => s.Machine)
                    .Include(s => s.MachineComponent)
                    .FirstOrDefaultAsync(s => s.Id == serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance service {ServiceId}", serviceId);
                throw;
            }
        }

        public async Task<bool> UpdateServiceValueAsync(int serviceId, double value, string? source = null)
        {
            if (value < 0)
                throw new ArgumentException("Service value cannot be negative", nameof(value));

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var service = await _context.MaintenanceServices.FindAsync(serviceId);
                if (service == null || !service.IsEnabled)
                {
                    _logger.LogWarning("Maintenance service {ServiceId} not found or disabled for value update", serviceId);
                    return false;
                }

                service.CurrentValue = value;
                service.LastUpdateTime = DateTime.UtcNow;

                // Log the data point
                _context.MaintenanceServiceData.Add(new MaintenanceServiceData
                {
                    MaintenanceServiceId = serviceId,
                    Value = value,
                    Source = source ?? "Manual",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Updated service {ServiceId} value to {Value} from source {Source}", 
                    serviceId, value, source ?? "Manual");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service value for service {ServiceId}", serviceId);
                throw;
            }
        }

        public async Task<bool> ResetServiceAsync(int serviceId, int userId, string? notes = null)
        {
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var service = await _context.MaintenanceServices.FindAsync(serviceId);
                if (service == null)
                {
                    _logger.LogWarning("Maintenance service {ServiceId} not found for reset", serviceId);
                    return false;
                }

                service.CurrentValue = 0;
                service.LastResetTime = DateTime.UtcNow;
                service.LastUpdateTime = DateTime.UtcNow;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = userId.ToString();

                // Log the reset
                _context.MaintenanceServiceData.Add(new MaintenanceServiceData
                {
                    MaintenanceServiceId = serviceId,
                    Value = 0,
                    Source = "Reset",
                    Notes = notes ?? "Service reset by user",
                    Timestamp = DateTime.UtcNow,
                    IsReset = true
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Reset maintenance service {ServiceId} by user {UserId}", serviceId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting maintenance service {ServiceId}", serviceId);
                throw;
            }
        }

        public async Task<List<MaintenanceServiceData>> GetServiceHistoryAsync(int serviceId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.MaintenanceServiceData.Where(d => d.MaintenanceServiceId == serviceId);

                if (fromDate.HasValue)
                    query = query.Where(d => d.Timestamp >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(d => d.Timestamp <= toDate.Value);

                return await query
                    .OrderByDescending(d => d.Timestamp)
                    .Take(1000) // Limit to prevent performance issues
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service history for service {ServiceId}", serviceId);
                throw;
            }
        }

        #endregion

        #region Work Orders

        public async Task<int> CreateWorkOrderAsync(MaintenanceWorkOrder workOrder, int userId)
        {
            ValidateWorkOrder(workOrder);
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                workOrder.WorkOrderNumber = await GenerateWorkOrderNumberAsync();
                workOrder.CreatedAt = DateTime.UtcNow;
                workOrder.CreatedBy = userId.ToString();

                _context.MaintenanceWorkOrders.Add(workOrder);
                await _context.SaveChangesAsync();

                // Create notification
                await CreateNotificationAsync(new MaintenanceNotification
                {
                    MachineId = workOrder.MachineId,
                    WorkOrderId = workOrder.Id,
                    NotificationType = MaintenanceNotificationType.WorkOrderCreated,
                    Severity = MapPriorityToSeverity(workOrder.Priority),
                    Title = $"Work Order Created: {workOrder.Title}",
                    Message = $"Work order {workOrder.WorkOrderNumber} has been created for {workOrder.MachineId}"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created work order {WorkOrderNumber} for machine {MachineId} by user {UserId}", 
                    workOrder.WorkOrderNumber, workOrder.MachineId, userId);

                return workOrder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work order for machine {MachineId}", workOrder.MachineId);
                throw;
            }
        }

        public async Task<bool> UpdateWorkOrderAsync(MaintenanceWorkOrder workOrder, int userId)
        {
            ValidateWorkOrder(workOrder);
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var existing = await _context.MaintenanceWorkOrders.FindAsync(workOrder.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Work order {WorkOrderId} not found for update", workOrder.Id);
                    return false;
                }

                // Update properties
                existing.Title = workOrder.Title;
                existing.Description = workOrder.Description;
                existing.WorkOrderType = workOrder.WorkOrderType;
                existing.Priority = workOrder.Priority;
                existing.Status = workOrder.Status;
                existing.ScheduledStartDate = workOrder.ScheduledStartDate;
                existing.ScheduledEndDate = workOrder.ScheduledEndDate;
                existing.AssignedTechnician = workOrder.AssignedTechnician;
                existing.AssignedTechnicianUserId = workOrder.AssignedTechnicianUserId;
                existing.EstimatedHours = workOrder.EstimatedHours;
                existing.EstimatedCost = workOrder.EstimatedCost;
                existing.RequiresShutdown = workOrder.RequiresShutdown;
                existing.ShutdownDurationMinutes = workOrder.ShutdownDurationMinutes;
                existing.Notes = workOrder.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated work order {WorkOrderId} by user {UserId}", workOrder.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work order {WorkOrderId}", workOrder.Id);
                throw;
            }
        }

        public async Task<bool> AssignWorkOrderAsync(int workOrderId, int technicianUserId, int assignedByUserId)
        {
            ValidateUserId(technicianUserId);
            ValidateUserId(assignedByUserId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var workOrder = await _context.MaintenanceWorkOrders.FindAsync(workOrderId);
                if (workOrder == null)
                {
                    _logger.LogWarning("Work order {WorkOrderId} not found for assignment", workOrderId);
                    return false;
                }

                var technician = await _context.Users.FindAsync(technicianUserId);
                if (technician == null)
                {
                    _logger.LogWarning("Technician user {TechnicianUserId} not found", technicianUserId);
                    return false;
                }

                workOrder.AssignedTechnicianUserId = technicianUserId;
                workOrder.AssignedTechnician = technician.FullName;
                workOrder.Status = MaintenanceWorkOrderStatus.Assigned;
                workOrder.UpdatedAt = DateTime.UtcNow;
                workOrder.UpdatedBy = assignedByUserId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Assigned work order {WorkOrderId} to technician {TechnicianUserId} by user {AssignedByUserId}", 
                    workOrderId, technicianUserId, assignedByUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning work order {WorkOrderId} to technician {TechnicianUserId}", 
                    workOrderId, technicianUserId);
                throw;
            }
        }

        public async Task<bool> StartWorkOrderAsync(int workOrderId, int userId)
        {
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var workOrder = await _context.MaintenanceWorkOrders.FindAsync(workOrderId);
                if (workOrder == null)
                {
                    _logger.LogWarning("Work order {WorkOrderId} not found for start", workOrderId);
                    return false;
                }

                workOrder.Status = MaintenanceWorkOrderStatus.InProgress;
                workOrder.ActualStartDate = DateTime.UtcNow;
                workOrder.UpdatedAt = DateTime.UtcNow;
                workOrder.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Started work order {WorkOrderId} by user {UserId}", workOrderId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        public async Task<bool> CompleteWorkOrderAsync(int workOrderId, int userId, string workPerformed, double actualHours, decimal actualCost)
        {
            ValidateUserId(userId);
            if (string.IsNullOrWhiteSpace(workPerformed))
                throw new ArgumentException("Work performed description is required", nameof(workPerformed));
            if (actualHours < 0)
                throw new ArgumentException("Actual hours cannot be negative", nameof(actualHours));
            if (actualCost < 0)
                throw new ArgumentException("Actual cost cannot be negative", nameof(actualCost));

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var workOrder = await _context.MaintenanceWorkOrders.FindAsync(workOrderId);
                if (workOrder == null)
                {
                    _logger.LogWarning("Work order {WorkOrderId} not found for completion", workOrderId);
                    return false;
                }

                workOrder.Status = MaintenanceWorkOrderStatus.Completed;
                workOrder.ActualEndDate = DateTime.UtcNow;
                workOrder.WorkPerformed = workPerformed;
                workOrder.ActualHours = actualHours;
                workOrder.ActualCost = actualCost;
                workOrder.UpdatedAt = DateTime.UtcNow;
                workOrder.UpdatedBy = userId.ToString();

                // Reset related maintenance services/rules if applicable
                if (workOrder.MaintenanceRuleId.HasValue)
                {
                    await LogActionAsync(workOrder.MaintenanceRuleId.Value, workOrder.MachineId, userId, 
                        $"Completed work order {workOrder.WorkOrderNumber}", true);
                }

                await _context.SaveChangesAsync();

                // Create completion notification
                await CreateNotificationAsync(new MaintenanceNotification
                {
                    MachineId = workOrder.MachineId,
                    WorkOrderId = workOrder.Id,
                    NotificationType = MaintenanceNotificationType.WorkOrderCompleted,
                    Severity = MaintenanceNotificationSeverity.Info,
                    Title = $"Work Order Completed: {workOrder.Title}",
                    Message = $"Work order {workOrder.WorkOrderNumber} has been completed. Actual time: {actualHours:F1}h, Cost: ${actualCost:F2}"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Completed work order {WorkOrderId} by user {UserId}, Hours: {ActualHours}, Cost: {ActualCost}", 
                    workOrderId, userId, actualHours, actualCost);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        public async Task<List<MaintenanceWorkOrder>> GetWorkOrdersAsync(string? machineId = null, MaintenanceWorkOrderStatus? status = null)
        {
            try
            {
                var query = _context.MaintenanceWorkOrders.AsQueryable();

                if (machineId != null)
                    query = query.Where(w => w.MachineId == machineId);
                if (status.HasValue)
                    query = query.Where(w => w.Status == status.Value);

                return await query
                    .Include(w => w.Machine)
                    .Include(w => w.MachineComponent)
                    .OrderByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work orders for machine {MachineId}", machineId);
                throw;
            }
        }

        public async Task<MaintenanceWorkOrder?> GetWorkOrderAsync(int workOrderId)
        {
            try
            {
                return await _context.MaintenanceWorkOrders
                    .Include(w => w.Machine)
                    .Include(w => w.MachineComponent)
                    .Include(w => w.MaintenanceRule)
                    .Include(w => w.AssignedTechnicianUser)
                    .FirstOrDefaultAsync(w => w.Id == workOrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        public async Task<List<MaintenanceWorkOrder>> GetTechnicianWorkOrdersAsync(int userId, MaintenanceWorkOrderStatus? status = null)
        {
            ValidateUserId(userId);

            try
            {
                var query = _context.MaintenanceWorkOrders.Where(w => w.AssignedTechnicianUserId == userId);

                if (status.HasValue)
                    query = query.Where(w => w.Status == status.Value);

                return await query
                    .Include(w => w.Machine)
                    .OrderBy(w => w.ScheduledStartDate ?? w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work orders for technician {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Scheduling

        public async Task<int> CreateMaintenanceScheduleAsync(MaintenanceSchedule schedule, int userId)
        {
            ValidateMaintenanceSchedule(schedule);
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                schedule.CreatedAt = DateTime.UtcNow;
                schedule.CreatedBy = userId.ToString();

                _context.MaintenanceSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Created maintenance schedule {ScheduleName} for machine {MachineId} by user {UserId}", 
                    schedule.ScheduleName, schedule.MachineId, userId);

                return schedule.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance schedule {ScheduleName}", schedule.ScheduleName);
                throw;
            }
        }

        public async Task<bool> UpdateMaintenanceScheduleAsync(MaintenanceSchedule schedule, int userId)
        {
            ValidateMaintenanceSchedule(schedule);
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var existing = await _context.MaintenanceSchedules.FindAsync(schedule.Id);
                if (existing == null)
                {
                    _logger.LogWarning("Maintenance schedule {ScheduleId} not found for update", schedule.Id);
                    return false;
                }

                // Update properties
                existing.ScheduleName = schedule.ScheduleName;
                existing.Description = schedule.Description;
                existing.ScheduleType = schedule.ScheduleType;
                existing.IsActive = schedule.IsActive;
                existing.IntervalDays = schedule.IntervalDays;
                existing.IntervalWeeks = schedule.IntervalWeeks;
                existing.IntervalMonths = schedule.IntervalMonths;
                existing.NextMaintenanceDate = schedule.NextMaintenanceDate;
                existing.EstimatedDurationHours = schedule.EstimatedDurationHours;
                existing.EstimatedCost = schedule.EstimatedCost;
                existing.AutoCreateWorkOrders = schedule.AutoCreateWorkOrders;
                existing.LeadTimeDays = schedule.LeadTimeDays;
                existing.Instructions = schedule.Instructions;
                existing.DefaultTechnician = schedule.DefaultTechnician;
                existing.DefaultTechnicianUserId = schedule.DefaultTechnicianUserId;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Updated maintenance schedule {ScheduleId} by user {UserId}", schedule.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance schedule {ScheduleId}", schedule.Id);
                throw;
            }
        }

        public async Task<bool> DeleteMaintenanceScheduleAsync(int scheduleId, int userId)
        {
            ValidateUserId(userId);

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var schedule = await _context.MaintenanceSchedules.FindAsync(scheduleId);
                if (schedule == null)
                {
                    _logger.LogWarning("Maintenance schedule {ScheduleId} not found for deletion", scheduleId);
                    return false;
                }

                schedule.IsActive = false;
                schedule.UpdatedAt = DateTime.UtcNow;
                schedule.UpdatedBy = userId.ToString();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Disabled maintenance schedule {ScheduleId} by user {UserId}", scheduleId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance schedule {ScheduleId}", scheduleId);
                throw;
            }
        }

        public async Task<List<MaintenanceSchedule>> GetMaintenanceSchedulesAsync(string? machineId = null)
        {
            try
            {
                var query = _context.MaintenanceSchedules.Where(s => s.IsActive);
                if (machineId != null)
                    query = query.Where(s => s.MachineId == machineId);

                return await query
                    .Include(s => s.Machine)
                    .Include(s => s.MachineComponent)
                    .OrderBy(s => s.MachineId)
                    .ThenBy(s => s.ScheduleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance schedules for machine {MachineId}", machineId);
                throw;
            }
        }

        public async Task<bool> ProcessScheduledMaintenanceAsync()
        {
            try
            {
                await _customService.AutoCreateWorkOrdersFromSchedulesAsync();
                _logger.LogInformation("Processed scheduled maintenance successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled maintenance");
                throw;
            }
        }

        public async Task<List<MaintenanceSchedule>> GetDueSchedulesAsync(DateTime? asOfDate = null)
        {
            try
            {
                var checkDate = asOfDate ?? DateTime.UtcNow;

                return await _context.MaintenanceSchedules
                    .Where(s => s.IsActive && s.NextMaintenanceDate.HasValue && s.NextMaintenanceDate.Value <= checkDate)
                    .Include(s => s.Machine)
                    .Include(s => s.MachineComponent)
                    .OrderBy(s => s.NextMaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving due maintenance schedules");
                throw;
            }
        }

        #endregion

        #region Notifications

        public async Task<int> CreateNotificationAsync(MaintenanceNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            try
            {
                notification.CreatedAt = DateTime.UtcNow;
                _context.MaintenanceNotifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Created maintenance notification: {Title}", notification.Title);
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance notification");
                throw;
            }
        }

        public async Task<bool> AcknowledgeNotificationAsync(int notificationId, int userId)
        {
            ValidateUserId(userId);

            try
            {
                var notification = await _context.MaintenanceNotifications.FindAsync(notificationId);
                if (notification == null || notification.AcknowledgedAt.HasValue)
                {
                    _logger.LogWarning("Notification {NotificationId} not found or already acknowledged", notificationId);
                    return false;
                }

                notification.AcknowledgedAt = DateTime.UtcNow;
                notification.AcknowledgedBy = userId.ToString();

                await _context.SaveChangesAsync();

                _logger.LogInformation("Acknowledged notification {NotificationId} by user {UserId}", notificationId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging notification {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<bool> DismissNotificationAsync(int notificationId, int userId)
        {
            ValidateUserId(userId);

            try
            {
                var notification = await _context.MaintenanceNotifications.FindAsync(notificationId);
                if (notification == null)
                {
                    _logger.LogWarning("Notification {NotificationId} not found for dismissal", notificationId);
                    return false;
                }

                notification.IsDismissed = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Dismissed notification {NotificationId} by user {UserId}", notificationId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing notification {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<List<MaintenanceNotification>> GetActiveNotificationsAsync(string? machineId = null)
        {
            try
            {
                var query = _context.MaintenanceNotifications.Where(n => !n.IsDismissed);

                if (machineId != null)
                    query = query.Where(n => n.MachineId == machineId);

                return await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(100) // Limit to prevent performance issues
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active notifications for machine {MachineId}", machineId);
                throw;
            }
        }

        public async Task<List<MaintenanceNotification>> GetUserNotificationsAsync(int userId, bool includeAcknowledged = false)
        {
            ValidateUserId(userId);

            try
            {
                var query = _context.MaintenanceNotifications.Where(n => !n.IsDismissed);

                if (!includeAcknowledged)
                    query = query.Where(n => !n.AcknowledgedAt.HasValue);

                return await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50) // Limit to prevent performance issues
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Analytics and Reporting

        public async Task<MaintenanceDashboardViewModel> GetMaintenanceDashboardAsync()
        {
            var dashboard = new MaintenanceDashboardViewModel();

            try
            {
                // Get fleet status with parallel execution
                var fleetStatusTask = GetFleetStatusAsync(true);
                var activeWorkOrdersTask = GetActiveWorkOrdersForDashboardAsync();
                var upcomingWorkOrdersTask = GetUpcomingWorkOrdersForDashboardAsync();
                var activeNotificationsTask = GetActiveNotificationsAsync();
                var criticalServicesTask = GetCriticalServicesAsync();

                await Task.WhenAll(fleetStatusTask, activeWorkOrdersTask, upcomingWorkOrdersTask, 
                    activeNotificationsTask, criticalServicesTask);

                var fleetStatus = await fleetStatusTask;
                dashboard.OverdueItems = fleetStatus.Values.SelectMany(v => v).Where(r => r.IsOverdue).ToList();
                dashboard.DueItems = fleetStatus.Values.SelectMany(v => v).Where(r => r.IsDue && !r.IsOverdue).ToList();

                dashboard.ActiveWorkOrders = await activeWorkOrdersTask;
                dashboard.UpcomingWorkOrders = await upcomingWorkOrdersTask;
                dashboard.ActiveNotifications = await activeNotificationsTask;
                dashboard.CriticalServices = await criticalServicesTask;

                // Calculate statistics
                var statisticsTask = CalculateDashboardStatisticsAsync(dashboard);
                await statisticsTask;

                _logger.LogDebug("Generated maintenance dashboard with {OverdueCount} overdue items and {DueCount} due items", 
                    dashboard.OverdueItems.Count, dashboard.DueItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building maintenance dashboard");
            }

            return dashboard;
        }

        public async Task<List<MaintenanceMetricsDto>> GetMaintenanceMetricsAsync(string? machineId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var metrics = new List<MaintenanceMetricsDto>();
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                var machines = machineId != null 
                    ? await _context.Machines.Where(m => m.MachineId == machineId).ToListAsync()
                    : await _context.Machines.Where(m => m.IsActive).ToListAsync();

                foreach (var machine in machines)
                {
                    var metric = new MaintenanceMetricsDto
                    {
                        MachineId = machine.MachineId,
                        MachineName = machine.MachineName
                    };

                    // Calculate metrics for this machine
                    await PopulateMachineMetricsAsync(metric, from, to);
                    metrics.Add(metric);
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance metrics for machine {MachineId}", machineId);
                throw;
            }
        }

        public async Task<MaintenanceCostAnalysisDto> GetCostAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var analysis = new MaintenanceCostAnalysisDto();

                var workOrders = await _context.MaintenanceWorkOrders
                    .Where(w => w.ActualEndDate >= fromDate && w.ActualEndDate <= toDate && w.ActualCost.HasValue)
                    .ToListAsync();

                if (workOrders.Any())
                {
                    analysis.TotalCost = workOrders.Sum(w => w.ActualCost!.Value);
                    analysis.PreventiveCost = workOrders.Where(w => w.WorkOrderType == MaintenanceWorkOrderType.Preventive)
                        .Sum(w => w.ActualCost!.Value);
                    analysis.CorrectiveCost = workOrders.Where(w => w.WorkOrderType == MaintenanceWorkOrderType.Corrective)
                        .Sum(w => w.ActualCost!.Value);
                    analysis.EmergencyCost = workOrders.Where(w => w.Priority == MaintenanceWorkOrderPriority.Emergency)
                        .Sum(w => w.ActualCost!.Value);

                    analysis.CostByMachine = workOrders.GroupBy(w => w.MachineId)
                        .ToDictionary(g => g.Key, g => g.Sum(w => w.ActualCost!.Value));

                    analysis.AverageCostPerWorkOrder = analysis.TotalCost / workOrders.Count;
                    
                    if (analysis.PreventiveCost + analysis.CorrectiveCost > 0)
                    {
                        analysis.PreventiveVsCorrectiveRatio = (double)(analysis.PreventiveCost / (analysis.PreventiveCost + analysis.CorrectiveCost));
                    }
                }

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cost analysis from {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<List<MaintenanceHistoryDto>> GetMaintenanceHistoryAsync(string? machineId = null, int? ruleId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var history = new List<MaintenanceHistoryDto>();
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                // Get work order history
                var workOrderQuery = _context.MaintenanceWorkOrders.AsQueryable();
                if (machineId != null) workOrderQuery = workOrderQuery.Where(w => w.MachineId == machineId);

                var workOrders = await workOrderQuery
                    .Where(w => w.CreatedAt >= from && w.CreatedAt <= to)
                    .Include(w => w.MachineComponent)
                    .ToListAsync();

                history.AddRange(workOrders.Select(w => new MaintenanceHistoryDto
                {
                    Id = w.Id,
                    Date = w.ActualEndDate ?? w.CreatedAt,
                    MachineId = w.MachineId,
                    ComponentName = w.MachineComponent?.Name,
                    Description = w.Title,
                    ActionType = "Work Order",
                    Status = w.Status.ToString(),
                    TechnicianName = w.AssignedTechnician,
                    Hours = w.ActualHours,
                    Cost = w.ActualCost,
                    Notes = w.WorkPerformed
                }));

                // Get action log history
                var actionQuery = _context.MaintenanceActionLogs.AsQueryable();
                if (machineId != null) actionQuery = actionQuery.Where(a => a.MachineId == machineId);
                if (ruleId.HasValue) actionQuery = actionQuery.Where(a => a.RuleId == ruleId.Value);

                var actions = await actionQuery
                    .Where(a => a.PerformedAt >= from && a.PerformedAt <= to)
                    .ToListAsync();

                history.AddRange(actions.Select(a => new MaintenanceHistoryDto
                {
                    Id = a.Id,
                    Date = a.PerformedAt,
                    MachineId = a.MachineId,
                    Description = "Maintenance Action",
                    ActionType = "Maintenance Action",
                    Status = a.ResetPerformed ? "Reset" : "Logged",
                    Notes = a.Notes
                }));

                return history.OrderByDescending(h => h.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance history");
                throw;
            }
        }

        #endregion

        #region Background Processing

        public async Task ProcessMaintenanceServicesAsync()
        {
            try
            {
                await _customService.UpdateMachineHoursAsync();
                await _customService.UpdateCycleCountersAsync();
                await _customService.UpdatePartCountersAsync();
                await _customService.UpdateTimeElapsedServicesAsync();

                _logger.LogDebug("Maintenance services processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing maintenance services");
                throw;
            }
        }

        public async Task ProcessMaintenanceAlertsAsync()
        {
            try
            {
                await _customService.CheckServiceThresholdsAsync();
                _logger.LogDebug("Maintenance alerts processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing maintenance alerts");
                throw;
            }
        }

        public async Task AutoCreateWorkOrdersAsync()
        {
            try
            {
                await _customService.AutoCreateWorkOrdersFromSchedulesAsync();
                _logger.LogDebug("Auto work order creation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-creating work orders");
                throw;
            }
        }

        #endregion

        #region Additional Private Helper Methods

        private void ValidateMaintenanceSchedule(MaintenanceSchedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));
            if (string.IsNullOrWhiteSpace(schedule.ScheduleName))
                throw new ArgumentException("Schedule name is required", nameof(schedule));
            if (string.IsNullOrWhiteSpace(schedule.MachineId))
                throw new ArgumentException("Machine ID is required", nameof(schedule));
            if (schedule.EstimatedDurationHours < 0)
                throw new ArgumentException("Estimated duration cannot be negative", nameof(schedule));
        }

        private async Task<List<MaintenanceWorkOrder>> GetActiveWorkOrdersForDashboardAsync()
        {
            return await _context.MaintenanceWorkOrders
                .Where(w => w.Status == MaintenanceWorkOrderStatus.InProgress || w.Status == MaintenanceWorkOrderStatus.Assigned)
                .Include(w => w.Machine)
                .OrderBy(w => w.ScheduledStartDate)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<MaintenanceWorkOrder>> GetUpcomingWorkOrdersForDashboardAsync()
        {
            return await _context.MaintenanceWorkOrders
                .Where(w => w.Status == MaintenanceWorkOrderStatus.Open && w.ScheduledStartDate >= DateTime.UtcNow)
                .Include(w => w.Machine)
                .OrderBy(w => w.ScheduledStartDate)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<OpCentrix.Models.Maintenance.MaintenanceService>> GetCriticalServicesAsync()
        {
            return await _context.MaintenanceServices
                .Where(s => s.IsEnabled && s.CriticalThreshold.HasValue && s.CurrentValue >= s.CriticalThreshold.Value)
                .Include(s => s.Machine)
                .ToListAsync();
        }

        private async Task CalculateDashboardStatisticsAsync(MaintenanceDashboardViewModel dashboard)
        {
            dashboard.TotalMachines = await _context.Machines.CountAsync(m => m.IsActive);
            dashboard.MachinesWithIssues = dashboard.OverdueItems.Select(i => i.MachineId).Distinct().Count();
            dashboard.OpenWorkOrders = await _context.MaintenanceWorkOrders
                .CountAsync(w => w.Status != MaintenanceWorkOrderStatus.Completed && w.Status != MaintenanceWorkOrderStatus.Cancelled);
            dashboard.OverdueWorkOrders = await _context.MaintenanceWorkOrders
                .CountAsync(w => w.ScheduledEndDate < DateTime.UtcNow && 
                    w.Status != MaintenanceWorkOrderStatus.Completed && w.Status != MaintenanceWorkOrderStatus.Cancelled);

            // Get cost data (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            dashboard.TotalMaintenanceCost = await _context.MaintenanceWorkOrders
                .Where(w => w.ActualEndDate >= thirtyDaysAgo && w.ActualCost.HasValue)
                .SumAsync(w => w.ActualCost!.Value);

            // Work orders by status
            dashboard.WorkOrdersByStatus = await _context.MaintenanceWorkOrders
                .GroupBy(w => w.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        private async Task PopulateMachineMetricsAsync(MaintenanceMetricsDto metric, DateTime from, DateTime to)
        {
            // Get work orders for this machine in the date range
            var workOrders = await _context.MaintenanceWorkOrders
                .Where(w => w.MachineId == metric.MachineId && w.CreatedAt >= from && w.CreatedAt <= to)
                .ToListAsync();

            metric.TotalWorkOrders = workOrders.Count;
            metric.CompletedWorkOrders = workOrders.Count(w => w.Status == MaintenanceWorkOrderStatus.Completed);
            metric.TotalMaintenanceCost = workOrders.Where(w => w.ActualCost.HasValue).Sum(w => w.ActualCost!.Value);
            metric.MaintenanceHours = workOrders.Where(w => w.ActualHours.HasValue).Sum(w => w.ActualHours!.Value);

            var lastMaintenance = workOrders.Where(w => w.ActualEndDate.HasValue)
                .OrderByDescending(w => w.ActualEndDate).FirstOrDefault();
            if (lastMaintenance != null)
            {
                metric.LastMaintenanceDate = lastMaintenance.ActualEndDate!.Value;
            }

            // Get services for this machine
            metric.Services = await _context.MaintenanceServices
                .Where(s => s.MachineId == metric.MachineId)
                .ToListAsync();

            // Calculate uptime (simplified - could be enhanced with actual machine data)
            var totalPossibleHours = (to - from).TotalHours;
            metric.TotalOperatingHours = totalPossibleHours - metric.MaintenanceHours;
            metric.UptimePercentage = totalPossibleHours > 0 ? (metric.TotalOperatingHours / totalPossibleHours) * 100 : 0;
        }

        private RuleStatusDto MapToStatusDto(MaintenanceRule r, MaintenanceState? s)
        {
            var current = s?.CurrentValue ?? 0;
            double threshold = r.TriggerType == MaintenanceTriggerType.DateInterval && r.IntervalDays.HasValue ? r.IntervalDays.Value : r.ThresholdValue;
            double percent = threshold > 0 ? current / threshold * 100.0 : 0;
            bool overdue = false; 
            bool due = false; 
            string dueText = string.Empty;
            
            if (r.TriggerType == MaintenanceTriggerType.DateInterval)
            {
                if (s?.NextDueDate.HasValue == true)
                {
                    var daysLeft = (s.NextDueDate.Value - DateTime.UtcNow).TotalDays;
                    dueText = daysLeft >= 0 ? $"{Math.Ceiling(daysLeft)}d" : $"overdue {Math.Abs(Math.Floor(daysLeft))}d";
                    overdue = daysLeft < 0;
                    if (!overdue && r.EarlyWarningDays.HasValue && daysLeft <= r.EarlyWarningDays.Value)
                        due = true;
                }
            }
            else
            {
                overdue = percent >= 100;
                if (!overdue && r.EarlyWarningPercent.HasValue && percent >= r.EarlyWarningPercent.Value)
                    due = true;
                var remain = threshold - current;
                dueText = threshold > 0 ? (r.TriggerType == MaintenanceTriggerType.BuildsCompleted ? $"{Math.Max(0, Math.Round(remain))} builds" : $"{remain:F1} left") : string.Empty;
            }
            
            return new RuleStatusDto
            {
                RuleId = r.Id,
                MachineId = r.MachineId,
                Title = r.Title,
                Severity = r.Severity,
                CurrentValue = current,
                Threshold = threshold,
                Percent = percent,
                IsDue = due,
                IsOverdue = overdue,
                NextDueDate = s?.NextDueDate,
                DueInText = dueText,
                ComponentName = null
            };
        }

        private void EvaluateFlags(MaintenanceRule r, MaintenanceState state)
        {
            if (r.TriggerType == MaintenanceTriggerType.DateInterval)
            {
                if (state.NextDueDate.HasValue)
                {
                    var daysLeft = (state.NextDueDate.Value - DateTime.UtcNow).TotalDays;
                    state.IsOverdue = daysLeft < 0;
                    state.IsDue = !state.IsOverdue && r.EarlyWarningDays.HasValue && daysLeft <= r.EarlyWarningDays.Value;
                }
            }
            else
            {
                var threshold = r.ThresholdValue <= 0 ? 0 : r.ThresholdValue;
                var pct = threshold > 0 ? state.CurrentValue / threshold * 100.0 : 0;
                state.IsOverdue = pct >= 100;
                state.IsDue = !state.IsOverdue && r.EarlyWarningPercent.HasValue && pct >= r.EarlyWarningPercent.Value;
            }
        }

        private async Task EnsureStateAsync(string machineId, int ruleId)
        {
            var exists = await _context.MaintenanceStates.AnyAsync(s => s.MachineId == machineId && s.RuleId == ruleId);
            if (!exists)
            {
                _context.MaintenanceStates.Add(new MaintenanceState
                {
                    MachineId = machineId,
                    RuleId = ruleId,
                    CurrentValue = 0,
                    CalculatedAt = DateTime.UtcNow
                });
            }
        }

        private async Task<string> GenerateWorkOrderNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"WO{today:yyyyMMdd}";

            var lastNumber = await _context.MaintenanceWorkOrders
                .Where(w => w.WorkOrderNumber.StartsWith(prefix))
                .Select(w => w.WorkOrderNumber)
                .OrderByDescending(w => w)
                .FirstOrDefaultAsync();

            if (lastNumber != null && lastNumber.Length > prefix.Length)
            {
                var numberPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var num))
                {
                    return $"{prefix}{(num + 1):D3}";
                }
            }

            return $"{prefix}001";
        }

        private void ValidateComponent(MachineComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrWhiteSpace(component.Name))
                throw new ArgumentException("Component name is required", nameof(component));
            if (string.IsNullOrWhiteSpace(component.MachineId))
                throw new ArgumentException("Machine ID is required", nameof(component));
        }

        private void ValidateMaintenanceRule(MaintenanceRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
            if (string.IsNullOrWhiteSpace(rule.Title))
                throw new ArgumentException("Rule title is required", nameof(rule));
            if (rule.ThresholdValue <= 0 && rule.TriggerType != MaintenanceTriggerType.DateInterval)
                throw new ArgumentException("Threshold value must be greater than zero", nameof(rule));
        }

        private void ValidateMaintenanceService(OpCentrix.Models.Maintenance.MaintenanceService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrWhiteSpace(service.ServiceName))
                throw new ArgumentException("Service name is required", nameof(service));
            if (string.IsNullOrWhiteSpace(service.MachineId))
                throw new ArgumentException("Machine ID is required", nameof(service));
            if (service.UpdateIntervalMinutes <= 0)
                throw new ArgumentException("Update interval must be greater than zero", nameof(service));
        }

        private void ValidateWorkOrder(MaintenanceWorkOrder workOrder)
        {
            if (workOrder == null)
                throw new ArgumentNullException(nameof(workOrder));
            if (string.IsNullOrWhiteSpace(workOrder.Title))
                throw new ArgumentException("Work order title is required", nameof(workOrder));
            if (string.IsNullOrWhiteSpace(workOrder.MachineId))
                throw new ArgumentException("Machine ID is required", nameof(workOrder));
            if (workOrder.EstimatedHours < 0)
                throw new ArgumentException("Estimated hours cannot be negative", nameof(workOrder));
        }

        private void ValidateUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero", nameof(userId));
        }

        private double CalculateCurrentValue(MaintenanceRule rule, List<object> machineBuilds, MaintenanceState state)
        {
            return rule.TriggerType switch
            {
                MaintenanceTriggerType.HoursRun => machineBuilds
                    .Cast<dynamic>()
                    .Where(b => (string)b.Status == "Completed" && b.ActualEndTime != null)
                    .Sum(b => ((DateTime)b.ActualEndTime - (DateTime)b.ActualStartTime).TotalHours),
                
                MaintenanceTriggerType.BuildsCompleted => machineBuilds
                    .Cast<dynamic>()
                    .Count(b => (string)b.Status == "Completed"),
                
                MaintenanceTriggerType.DateInterval => 
                    (DateTime.UtcNow - (state.LastServiceDate ?? rule.CreatedAt)).TotalDays,
                
                MaintenanceTriggerType.CustomMeter => state.CurrentValue,
                
                _ => state.CurrentValue
            };
        }

        private MaintenanceNotificationSeverity MapPriorityToSeverity(MaintenanceWorkOrderPriority priority)
        {
            return priority switch
            {
                MaintenanceWorkOrderPriority.Emergency => MaintenanceNotificationSeverity.Emergency,
                MaintenanceWorkOrderPriority.Critical => MaintenanceNotificationSeverity.Critical,
                MaintenanceWorkOrderPriority.High => MaintenanceNotificationSeverity.Warning,
                _ => MaintenanceNotificationSeverity.Info
            };
        }

        private void ClearRulesCache(string? machineId)
        {
            _cache.Remove(string.Format(MAINTENANCE_RULES_CACHE_KEY, $"{machineId}_true"));
            _cache.Remove(string.Format(MAINTENANCE_RULES_CACHE_KEY, $"{machineId}_false"));
            _cache.Remove(string.Format(MAINTENANCE_RULES_CACHE_KEY, "null_true"));
            _cache.Remove(string.Format(MAINTENANCE_RULES_CACHE_KEY, "null_false"));
        }

        private async Task CreateMaintenanceStatesForRuleAsync(MaintenanceRule rule)
        {
            var machines = !string.IsNullOrEmpty(rule.MachineId)
                ? new List<string> { rule.MachineId }
                : await _context.Machines.Where(m => m.IsActive).Select(m => m.MachineId).ToListAsync();

            foreach (var machineId in machines)
            {
                await EnsureStateAsync(machineId, rule.Id);
            }
        }

        #endregion
    }
}
