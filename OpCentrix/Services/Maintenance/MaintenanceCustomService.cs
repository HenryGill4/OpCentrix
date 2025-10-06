using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.Services.Maintenance
{
    public class MaintenanceCustomService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<MaintenanceCustomService> _logger;

        public MaintenanceCustomService(SchedulerContext context, ILogger<MaintenanceCustomService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Updates machine operating hours from completed build jobs
        /// </summary>
        public async Task UpdateMachineHoursAsync()
        {
            try
            {
                var hourTrackingServices = await _context.MaintenanceServices
                    .Where(s => s.ServiceType == MaintenanceServiceType.HourTracking && s.IsEnabled)
                    .ToListAsync();

                if (!hourTrackingServices.Any())
                {
                    _logger.LogDebug("No hour tracking services found");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var service in hourTrackingServices)
                {
                    try
                    {
                        var lastUpdate = service.LastUpdateTime;
                        
                        // Get completed builds since last update
                        var completedBuilds = await _context.BuildJobs
                            .Where(b => b.PrinterName == service.MachineId 
                                       && b.Status == "Completed" 
                                       && b.ActualEndTime.HasValue 
                                       && b.ActualStartTime > lastUpdate)
                            .ToListAsync();

                        if (completedBuilds.Any())
                        {
                            var totalHours = completedBuilds.Sum(b => 
                                (b.ActualEndTime!.Value - b.ActualStartTime).TotalHours);

                            // Update service value
                            service.CurrentValue += totalHours;
                            service.LastUpdateTime = DateTime.UtcNow;

                            // Log data point
                            _context.MaintenanceServiceData.Add(new MaintenanceServiceData
                            {
                                MaintenanceServiceId = service.Id,
                                Value = totalHours,
                                Source = "BuildJobCompleted",
                                Notes = $"Added {totalHours:F2} hours from {completedBuilds.Count} completed builds",
                                Timestamp = DateTime.UtcNow
                            });

                            _logger.LogDebug("Updated {ServiceName} for {MachineId}: +{Hours:F2}h (total: {Total:F2}h)", 
                                service.ServiceName, service.MachineId, totalHours, service.CurrentValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating hour tracking for service {ServiceId} on machine {MachineId}", 
                            service.Id, service.MachineId);
                        // Continue with other services
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Successfully updated {Count} hour tracking services", hourTrackingServices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating machine hours");
                throw;
            }
        }

        /// <summary>
        /// Updates build/cycle counters from completed jobs
        /// </summary>
        public async Task UpdateCycleCountersAsync()
        {
            try
            {
                var cycleTrackingServices = await _context.MaintenanceServices
                    .Where(s => s.ServiceType == MaintenanceServiceType.CycleTracking && s.IsEnabled)
                    .ToListAsync();

                if (!cycleTrackingServices.Any())
                {
                    _logger.LogDebug("No cycle tracking services found");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var service in cycleTrackingServices)
                {
                    try
                    {
                        var lastUpdate = service.LastUpdateTime;
                        
                        var newCycles = await _context.BuildJobs
                            .Where(b => b.PrinterName == service.MachineId 
                                       && b.Status == "Completed" 
                                       && b.ActualStartTime > lastUpdate)
                            .CountAsync();

                        if (newCycles > 0)
                        {
                            service.CurrentValue += newCycles;
                            service.LastUpdateTime = DateTime.UtcNow;

                            _context.MaintenanceServiceData.Add(new MaintenanceServiceData
                            {
                                MaintenanceServiceId = service.Id,
                                Value = newCycles,
                                Source = "BuildJobCompleted",
                                Notes = $"Added {newCycles} completed cycles",
                                Timestamp = DateTime.UtcNow
                            });

                            _logger.LogDebug("Updated {ServiceName} for {MachineId}: +{Cycles} cycles (total: {Total})", 
                                service.ServiceName, service.MachineId, newCycles, service.CurrentValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating cycle tracking for service {ServiceId} on machine {MachineId}", 
                            service.Id, service.MachineId);
                        // Continue with other services
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Successfully updated {Count} cycle tracking services", cycleTrackingServices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cycle counters");
                throw;
            }
        }

        /// <summary>
        /// Updates part counters from completed jobs
        /// </summary>
        public async Task UpdatePartCountersAsync()
        {
            try
            {
                var partCountingServices = await _context.MaintenanceServices
                    .Where(s => s.ServiceType == MaintenanceServiceType.PartCounter && s.IsEnabled)
                    .ToListAsync();

                if (!partCountingServices.Any())
                {
                    _logger.LogDebug("No part counting services found");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var service in partCountingServices)
                {
                    try
                    {
                        var lastUpdate = service.LastUpdateTime;
                        
                        var newParts = await _context.BuildJobs
                            .Where(b => b.PrinterName == service.MachineId 
                                       && b.Status == "Completed" 
                                       && b.ActualStartTime > lastUpdate)
                            .SumAsync(b => b.TotalPartsInBuild);

                        if (newParts > 0)
                        {
                            service.CurrentValue += newParts;
                            service.LastUpdateTime = DateTime.UtcNow;

                            _context.MaintenanceServiceData.Add(new MaintenanceServiceData
                            {
                                MaintenanceServiceId = service.Id,
                                Value = newParts,
                                Source = "BuildJobCompleted",
                                Notes = $"Added {newParts} completed parts",
                                Timestamp = DateTime.UtcNow
                            });

                            _logger.LogDebug("Updated {ServiceName} for {MachineId}: +{Parts} parts (total: {Total})", 
                                service.ServiceName, service.MachineId, newParts, service.CurrentValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating part counting for service {ServiceId} on machine {MachineId}", 
                            service.Id, service.MachineId);
                        // Continue with other services
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Successfully updated {Count} part counting services", partCountingServices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part counters");
                throw;
            }
        }

        /// <summary>
        /// Updates time elapsed services
        /// </summary>
        public async Task UpdateTimeElapsedServicesAsync()
        {
            try
            {
                var timeElapsedServices = await _context.MaintenanceServices
                    .Where(s => s.ServiceType == MaintenanceServiceType.TimeElapsed && s.IsEnabled)
                    .ToListAsync();

                if (!timeElapsedServices.Any())
                {
                    _logger.LogDebug("No time elapsed services found");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var service in timeElapsedServices)
                {
                    try
                    {
                        var elapsedSinceReset = service.LastResetTime ?? service.CreatedAt;
                        var totalDays = (DateTime.UtcNow - elapsedSinceReset).TotalDays;

                        // Only update if there's a significant change (more than 1 hour)
                        if (Math.Abs(service.CurrentValue - totalDays) > (1.0 / 24.0))
                        {
                            service.CurrentValue = totalDays;
                            service.LastUpdateTime = DateTime.UtcNow;

                            _logger.LogDebug("Updated {ServiceName} for {MachineId}: {Days:F1} days since reset", 
                                service.ServiceName, service.MachineId, totalDays);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating time elapsed for service {ServiceId} on machine {MachineId}", 
                            service.Id, service.MachineId);
                        // Continue with other services
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogDebug("Successfully updated {Count} time elapsed services", timeElapsedServices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating time elapsed services");
                throw;
            }
        }

        /// <summary>
        /// Checks service thresholds and creates alerts
        /// </summary>
        public async Task CheckServiceThresholdsAsync()
        {
            try
            {
                var services = await _context.MaintenanceServices
                    .Where(s => s.IsEnabled && (s.WarningThreshold.HasValue || s.CriticalThreshold.HasValue))
                    .Include(s => s.Machine)
                    .ToListAsync();

                if (!services.Any())
                {
                    _logger.LogDebug("No services with thresholds found");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                var notificationsCreated = 0;

                foreach (var service in services)
                {
                    try
                    {
                        var notifications = await CreateThresholdNotificationsAsync(service);
                        if (notifications.Any())
                        {
                            _context.MaintenanceNotifications.AddRange(notifications);
                            notificationsCreated += notifications.Count;
                            
                            _logger.LogInformation("Created {Count} threshold notifications for service {ServiceName} on {MachineId}", 
                                notifications.Count, service.ServiceName, service.MachineId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking thresholds for service {ServiceId} on machine {MachineId}", 
                            service.Id, service.MachineId);
                        // Continue with other services
                    }
                }

                if (notificationsCreated > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created {Count} total threshold notifications", notificationsCreated);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service thresholds");
                throw;
            }
        }

        /// <summary>
        /// Auto-creates work orders from maintenance schedules
        /// </summary>
        public async Task AutoCreateWorkOrdersFromSchedulesAsync()
        {
            try
            {
                var dueSchedules = await _context.MaintenanceSchedules
                    .Where(s => s.IsActive && s.AutoCreateWorkOrders 
                               && s.NextMaintenanceDate.HasValue 
                               && s.NextMaintenanceDate.Value <= DateTime.UtcNow.AddDays(s.LeadTimeDays ?? 7))
                    .Include(s => s.Machine)
                    .Include(s => s.MachineComponent)
                    .ToListAsync();

                if (!dueSchedules.Any())
                {
                    _logger.LogDebug("No due schedules found for work order creation");
                    return;
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                var workOrdersCreated = 0;

                foreach (var schedule in dueSchedules)
                {
                    try
                    {
                        // Check if work order already exists for this schedule
                        var existingWorkOrder = await _context.MaintenanceWorkOrders
                            .AnyAsync(w => w.MachineId == schedule.MachineId 
                                          && w.MachineComponentId == schedule.MachineComponentId
                                          && w.Title.Contains(schedule.ScheduleName)
                                          && w.Status != MaintenanceWorkOrderStatus.Completed
                                          && w.Status != MaintenanceWorkOrderStatus.Cancelled);

                        if (!existingWorkOrder)
                        {
                            var workOrder = CreateWorkOrderFromSchedule(schedule);
                            _context.MaintenanceWorkOrders.Add(workOrder);

                            // Update schedule's next maintenance date
                            schedule.LastMaintenanceDate = schedule.NextMaintenanceDate;
                            schedule.NextMaintenanceDate = CalculateNextMaintenanceDate(schedule);

                            workOrdersCreated++;

                            _logger.LogInformation("Auto-created work order {WorkOrderNumber} for schedule {ScheduleName} on {MachineId}", 
                                workOrder.WorkOrderNumber, schedule.ScheduleName, schedule.MachineId);
                        }
                        else
                        {
                            _logger.LogDebug("Work order already exists for schedule {ScheduleName} on {MachineId}", 
                                schedule.ScheduleName, schedule.MachineId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating work order for schedule {ScheduleId} ({ScheduleName})", 
                            schedule.Id, schedule.ScheduleName);
                        // Continue with other schedules
                    }
                }

                if (workOrdersCreated > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Auto-created {Count} work orders from schedules", workOrdersCreated);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-creating work orders from schedules");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<List<MaintenanceNotification>> CreateThresholdNotificationsAsync(OpCentrix.Models.Maintenance.MaintenanceService service)
        {
            var notifications = new List<MaintenanceNotification>();

            // Check critical threshold
            if (service.CriticalThreshold.HasValue && service.CurrentValue >= service.CriticalThreshold.Value)
            {
                // Check if we already have an active critical notification
                var existingCritical = await _context.MaintenanceNotifications
                    .AnyAsync(n => n.MaintenanceServiceId == service.Id 
                                  && n.Severity == MaintenanceNotificationSeverity.Critical 
                                  && !n.IsDismissed
                                  && n.CreatedAt >= DateTime.UtcNow.AddDays(-1)); // Don't spam - only check last 24h

                if (!existingCritical)
                {
                    notifications.Add(new MaintenanceNotification
                    {
                        MachineId = service.MachineId,
                        MaintenanceServiceId = service.Id,
                        NotificationType = MaintenanceNotificationType.ThresholdExceeded,
                        Severity = MaintenanceNotificationSeverity.Critical,
                        Title = $"Critical Threshold Exceeded: {service.ServiceName}",
                        Message = $"Service '{service.ServiceName}' on machine {service.MachineId} has exceeded critical threshold. Current: {service.CurrentValue:F1} {service.Unit}, Threshold: {service.CriticalThreshold:F1} {service.Unit}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            // Check warning threshold (only if not already critical)
            else if (service.WarningThreshold.HasValue && service.CurrentValue >= service.WarningThreshold.Value)
            {
                var existingWarning = await _context.MaintenanceNotifications
                    .AnyAsync(n => n.MaintenanceServiceId == service.Id 
                                  && n.Severity == MaintenanceNotificationSeverity.Warning 
                                  && !n.IsDismissed
                                  && n.CreatedAt >= DateTime.UtcNow.AddDays(-1)); // Don't spam

                if (!existingWarning)
                {
                    notifications.Add(new MaintenanceNotification
                    {
                        MachineId = service.MachineId,
                        MaintenanceServiceId = service.Id,
                        NotificationType = MaintenanceNotificationType.ThresholdExceeded,
                        Severity = MaintenanceNotificationSeverity.Warning,
                        Title = $"Warning Threshold Exceeded: {service.ServiceName}",
                        Message = $"Service '{service.ServiceName}' on machine {service.MachineId} has exceeded warning threshold. Current: {service.CurrentValue:F1} {service.Unit}, Threshold: {service.WarningThreshold:F1} {service.Unit}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            return notifications;
        }

        private MaintenanceWorkOrder CreateWorkOrderFromSchedule(MaintenanceSchedule schedule)
        {
            return new MaintenanceWorkOrder
            {
                WorkOrderNumber = GenerateWorkOrderNumber(),
                MachineId = schedule.MachineId,
                MachineComponentId = schedule.MachineComponentId,
                Title = $"Scheduled: {schedule.ScheduleName}",
                Description = schedule.Description,
                WorkOrderType = MaintenanceWorkOrderType.Preventive,
                Priority = MaintenanceWorkOrderPriority.Normal,
                Status = MaintenanceWorkOrderStatus.Open,
                ScheduledStartDate = schedule.NextMaintenanceDate,
                ScheduledEndDate = schedule.NextMaintenanceDate?.AddHours(schedule.EstimatedDurationHours),
                AssignedTechnician = schedule.DefaultTechnician,
                AssignedTechnicianUserId = schedule.DefaultTechnicianUserId,
                EstimatedHours = schedule.EstimatedDurationHours,
                EstimatedCost = schedule.EstimatedCost,
                Notes = BuildWorkOrderNotes(schedule),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "MaintenanceScheduler"
            };
        }

        private string BuildWorkOrderNotes(MaintenanceSchedule schedule)
        {
            var notes = $"Auto-created from schedule: {schedule.ScheduleName}";
            if (!string.IsNullOrEmpty(schedule.Instructions))
            {
                notes += $"\n\nInstructions: {schedule.Instructions}";
            }
            return notes;
        }

        private string GenerateWorkOrderNumber()
        {
            var today = DateTime.Today;
            var timestamp = DateTime.Now.ToString("HHmmss");
            return $"WO{today:yyyyMMdd}{timestamp}";
        }

        private DateTime? CalculateNextMaintenanceDate(MaintenanceSchedule schedule)
        {
            if (!schedule.LastMaintenanceDate.HasValue) 
            {
                _logger.LogWarning("Cannot calculate next maintenance date for schedule {ScheduleId} - no last maintenance date", schedule.Id);
                return null;
            }

            try
            {
                return schedule.ScheduleType switch
                {
                    MaintenanceScheduleType.TimeInterval => CalculateTimeIntervalDate(schedule),
                    MaintenanceScheduleType.Calendar => CalculateCalendarDate(schedule),
                    _ => null // Usage-based schedules removed since enum doesn't have this value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next maintenance date for schedule {ScheduleId}", schedule.Id);
                return null;
            }
        }

        private DateTime? CalculateTimeIntervalDate(MaintenanceSchedule schedule)
        {
            var baseDate = schedule.LastMaintenanceDate!.Value;

            if (schedule.IntervalDays.HasValue)
                return baseDate.AddDays(schedule.IntervalDays.Value);
            
            if (schedule.IntervalWeeks.HasValue)
                return baseDate.AddDays(schedule.IntervalWeeks.Value * 7);
            
            if (schedule.IntervalMonths.HasValue)
                return baseDate.AddMonths(schedule.IntervalMonths.Value);

            _logger.LogWarning("Time interval schedule {ScheduleId} has no interval defined", schedule.Id);
            return null;
        }

        private DateTime? CalculateCalendarDate(MaintenanceSchedule schedule)
        {
            if (schedule.IntervalMonths.HasValue)
                return schedule.LastMaintenanceDate!.Value.AddMonths(schedule.IntervalMonths.Value);

            _logger.LogWarning("Calendar schedule {ScheduleId} has no monthly interval defined", schedule.Id);
            return null;
        }

        #endregion
    }
}