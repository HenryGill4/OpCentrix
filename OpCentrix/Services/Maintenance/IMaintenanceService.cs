using System.Collections.Generic;
using System.Threading.Tasks;
using OpCentrix.Models.Maintenance;
using OpCentrix.ViewModels.Maintenance;

namespace OpCentrix.Services.Maintenance
{
    public interface IMaintenanceService
    {
        // Existing methods
        Task<List<RuleStatusDto>> GetMachineStatusAsync(string machineId, bool includeComponents = false);
        Task<Dictionary<string, List<RuleStatusDto>>> GetFleetStatusAsync(bool includeComponents = false);
        Task<int> CreateRuleAsync(MaintenanceRule rule, int userId);
        Task<bool> UpdateRuleAsync(MaintenanceRule rule, int userId);
        Task<bool> DisableRuleAsync(int ruleId, int userId);
        Task<bool> LogActionAsync(int ruleId, string machineId, int userId, string notes, bool reset);
        Task RecalculateAsync(string? machineId = null, int? ruleId = null);
        Task<MaintenanceRule?> GetRuleAsync(int id);
        Task<List<MaintenanceRule>> GetRulesAsync(string? machineId = null, bool activeOnly = false);
        Task<int> CreateComponentAsync(MachineComponent component, int userId);
        Task<bool> UpdateComponentAsync(MachineComponent component, int userId);
        Task<List<MachineComponent>> GetComponentsAsync(string machineId, bool activeOnly = true);

        // Enhanced Maintenance Services
        Task<int> CreateMaintenanceServiceAsync(OpCentrix.Models.Maintenance.MaintenanceService service, int userId);
        Task<bool> UpdateMaintenanceServiceAsync(OpCentrix.Models.Maintenance.MaintenanceService service, int userId);
        Task<bool> DeleteMaintenanceServiceAsync(int serviceId, int userId);
        Task<List<OpCentrix.Models.Maintenance.MaintenanceService>> GetMaintenanceServicesAsync(string? machineId = null);
        Task<OpCentrix.Models.Maintenance.MaintenanceService?> GetMaintenanceServiceAsync(int serviceId);
        Task<bool> UpdateServiceValueAsync(int serviceId, double value, string? source = null);
        Task<bool> ResetServiceAsync(int serviceId, int userId, string? notes = null);
        Task<List<MaintenanceServiceData>> GetServiceHistoryAsync(int serviceId, DateTime? fromDate = null, DateTime? toDate = null);

        // Work Orders
        Task<int> CreateWorkOrderAsync(MaintenanceWorkOrder workOrder, int userId);
        Task<bool> UpdateWorkOrderAsync(MaintenanceWorkOrder workOrder, int userId);
        Task<bool> AssignWorkOrderAsync(int workOrderId, int technicianUserId, int assignedByUserId);
        Task<bool> StartWorkOrderAsync(int workOrderId, int userId);
        Task<bool> CompleteWorkOrderAsync(int workOrderId, int userId, string workPerformed, double actualHours, decimal actualCost);
        Task<List<MaintenanceWorkOrder>> GetWorkOrdersAsync(string? machineId = null, MaintenanceWorkOrderStatus? status = null);
        Task<MaintenanceWorkOrder?> GetWorkOrderAsync(int workOrderId);
        Task<List<MaintenanceWorkOrder>> GetTechnicianWorkOrdersAsync(int userId, MaintenanceWorkOrderStatus? status = null);

        // Scheduling
        Task<int> CreateMaintenanceScheduleAsync(MaintenanceSchedule schedule, int userId);
        Task<bool> UpdateMaintenanceScheduleAsync(MaintenanceSchedule schedule, int userId);
        Task<bool> DeleteMaintenanceScheduleAsync(int scheduleId, int userId);
        Task<List<MaintenanceSchedule>> GetMaintenanceSchedulesAsync(string? machineId = null);
        Task<bool> ProcessScheduledMaintenanceAsync(); // Background service method
        Task<List<MaintenanceSchedule>> GetDueSchedulesAsync(DateTime? asOfDate = null);

        // Notifications
        Task<int> CreateNotificationAsync(MaintenanceNotification notification);
        Task<bool> AcknowledgeNotificationAsync(int notificationId, int userId);
        Task<bool> DismissNotificationAsync(int notificationId, int userId);
        Task<List<MaintenanceNotification>> GetActiveNotificationsAsync(string? machineId = null);
        Task<List<MaintenanceNotification>> GetUserNotificationsAsync(int userId, bool includeAcknowledged = false);

        // Analytics and Reporting
        Task<MaintenanceDashboardViewModel> GetMaintenanceDashboardAsync();
        Task<List<MaintenanceMetricsDto>> GetMaintenanceMetricsAsync(string? machineId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<MaintenanceCostAnalysisDto> GetCostAnalysisAsync(DateTime fromDate, DateTime toDate);
        Task<List<MaintenanceHistoryDto>> GetMaintenanceHistoryAsync(string? machineId = null, int? ruleId = null, DateTime? fromDate = null, DateTime? toDate = null);

        // Integration and Background Processing
        Task ProcessMaintenanceServicesAsync(); // Update service values from various sources
        Task ProcessMaintenanceAlertsAsync(); // Check thresholds and create notifications
        Task AutoCreateWorkOrdersAsync(); // Create work orders from schedules and rules
    }
}
