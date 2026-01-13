using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.MaintenanceV2;
using OpCentrix.Services;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly SlsDataSeedingService _seedingService;
        private readonly DatabaseValidationService _databaseValidationService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            SchedulerContext context,
            SlsDataSeedingService seedingService,
            DatabaseValidationService databaseValidationService,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _seedingService = seedingService;
            _databaseValidationService = databaseValidationService;
            _logger = logger;
        }

        // Core dashboard properties
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalParts { get; set; }
        public int ActiveParts { get; set; }
        public int TotalLogEntries { get; set; }
        public DateTime? LastJobUpdate { get; set; }
        public DateTime? LastPartUpdate { get; set; }
        public List<Job> RecentJobs { get; set; } = new();
        public List<Part> RecentParts { get; set; } = new();
        public List<JobLogEntry> RecentLogEntries { get; set; } = new();

        // Task system properties
        public int OpenTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CompletedTodayTasks { get; set; }
        public List<OperationalTask> RecentTasks { get; set; } = new();

        // Database management properties (kept for backup functionality)
        public bool HasSampleData { get; set; }
        public int SamplePartsCount { get; set; }
        public int SampleJobsCount { get; set; }
        public int RealDataCount { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load core statistics
                TotalJobs = await _context.Jobs.CountAsync();
                ActiveJobs = await _context.Jobs.CountAsync(j => j.Status == "Active" || j.Status == "Scheduled");
                TotalParts = await _context.Parts.CountAsync();
                ActiveParts = await _context.Parts.CountAsync(p => p.IsActive);
                TotalLogEntries = await _context.JobLogEntries.CountAsync();

                // Load recent data
                RecentJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .OrderByDescending(j => j.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                RecentParts = await _context.Parts
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                RecentLogEntries = await _context.JobLogEntries
                    .OrderByDescending(l => l.Timestamp)
                    .Take(8)
                    .ToListAsync();

                // Load task statistics
                await LoadTaskStatisticsAsync();

                // Load database status for backup functionality
                SamplePartsCount = await CountSamplePartsAsync();
                SampleJobsCount = await CountSampleJobsAsync();
                HasSampleData = SamplePartsCount > 0 || SampleJobsCount > 0;
                RealDataCount = TotalParts + TotalJobs - SamplePartsCount - SampleJobsCount;

                // Get last update times
                LastJobUpdate = await _context.Jobs.MaxAsync(j => (DateTime?)j.LastModifiedDate);
                LastPartUpdate = await _context.Parts.MaxAsync(p => (DateTime?)p.CreatedDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard data");
                // Initialize with defaults on error
                RecentJobs = new List<Job>();
                RecentParts = new List<Part>();
                RecentLogEntries = new List<JobLogEntry>();
                RecentTasks = new List<OperationalTask>();
            }
        }

        private async Task LoadTaskStatisticsAsync()
        {
            try
            {
                OpenTasks = await _context.Set<OperationalTask>().CountAsync(t => t.Status == "Open");
                InProgressTasks = await _context.Set<OperationalTask>().CountAsync(t => t.Status == "InProgress");
                OverdueTasks = await _context.Set<OperationalTask>().CountAsync(t => t.OverdueFlag == 1);
                
                var today = DateTime.Today;
                CompletedTodayTasks = await _context.Set<OperationalTask>()
                    .CountAsync(t => t.Status == "Completed" && t.CompletedAt != null && t.CompletedAt >= today);

                RecentTasks = await _context.Set<OperationalTask>()
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load task statistics - table may not exist yet");
                // Initialize with zeros if task system not available
                OpenTasks = 0;
                InProgressTasks = 0;
                OverdueTasks = 0;
                CompletedTodayTasks = 0;
                RecentTasks = new List<OperationalTask>();
            }
        }

        private async Task<int> CountSamplePartsAsync()
        {
            return await _context.Parts.CountAsync(p => 
                p.PartNumber.StartsWith("EX-") || 
                p.PartNumber.StartsWith("SAMPLE-") ||
                p.Description.Contains("Example") ||
                p.Description.Contains("Sample"));
        }

        private async Task<int> CountSampleJobsAsync()
        {
            return await _context.Jobs.CountAsync(j => 
                j.PartNumber.StartsWith("EX-") || 
                j.PartNumber.StartsWith("SAMPLE-") ||
                j.CreatedBy == "SampleDataSeeder");
        }

        // Task system handlers
        public async Task<IActionResult> OnGetCreateTaskModalAsync()
        {
            try
            {
                var taskForm = $@"
                    <div class='bg-white rounded-lg p-6 max-w-2xl w-full mx-4'>
                        <h3 class='text-lg font-semibold mb-4'>Create New Operational Task</h3>
                        <form hx-post='/Admin?handler=CreateTask' hx-include='this' class='space-y-4'>
                            
                            <div class='grid grid-cols-2 gap-4'>
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Task Type</label>
                                    <select name='TaskType' class='w-full border border-gray-300 rounded-lg p-2' required>
                                        <option value='Maintenance'>Maintenance</option>
                                        <option value='Quality'>Quality Check</option>
                                        <option value='Cleaning'>Cleaning</option>
                                        <option value='Inspection'>Inspection</option>
                                        <option value='Calibration'>Calibration</option>
                                        <option value='General' selected>General Task</option>
                                    </select>
                                </div>
                                
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Priority</label>
                                    <select name='Priority' class='w-full border border-gray-300 rounded-lg p-2' required>
                                        <option value='1'>Critical (1)</option>
                                        <option value='2'>High (2)</option>
                                        <option value='3' selected>Normal (3)</option>
                                        <option value='4'>Low (4)</option>
                                        <option value='5'>Lowest (5)</option>
                                    </select>
                                </div>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Title</label>
                                <input type='text' name='Title' class='w-full border border-gray-300 rounded-lg p-2' required maxlength='160' placeholder='Brief description of the task' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Description</label>
                                <textarea name='Description' class='w-full border border-gray-300 rounded-lg p-2' rows='3' placeholder='Detailed task description and instructions'></textarea>
                            </div>
                            
                            <div class='grid grid-cols-2 gap-4'>
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Machine (Optional)</label>
                                    <select name='MachineId' class='w-full border border-gray-300 rounded-lg p-2'>
                                        <option value=''>Select Machine</option>
                                        <option value='TI1'>TI1 - Titanium Line 1</option>
                                        <option value='TI2'>TI2 - Titanium Line 2</option>
                                        <option value='INC'>INC - Inconel Line</option>
                                    </select>
                                </div>
                                
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Due Date (Optional)</label>
                                    <input type='datetime-local' name='DueAt' class='w-full border border-gray-300 rounded-lg p-2' />
                                </div>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Category/Tags (Optional)</label>
                                <input type='text' name='Tags' class='w-full border border-gray-300 rounded-lg p-2' placeholder='maintenance, quality, urgent' />
                            </div>
                            
                            <div class='flex justify-end space-x-3 pt-4'>
                                <button type='button' onclick='hideModal()' class='px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50'>
                                    Cancel
                                </button>
                                <button type='submit' class='px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700'>
                                    Create Task
                                </button>
                            </div>
                        </form>
                    </div>";

                return Content(taskForm, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task modal");
                return Content("<script>showNotification('Error loading task form', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostCreateTaskAsync(string title, string? description, 
            string taskType, int priority, string? machineId, DateTime? dueAt, string? tags)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
                
                var task = new OperationalTask
                {
                    Title = title,
                    Description = description,
                    TaskType = taskType,
                    Priority = priority,
                    MachineId = machineId,
                    DueAt = dueAt,
                    Tags = tags,
                    CreatedByUserId = userId,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<OperationalTask>().Add(task);
                await _context.SaveChangesAsync();

                return Content("<script>hideModal(); showNotification('Task created successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating operational task");
                return Content($"<script>showNotification('Error creating task: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnGetMaintenanceTaskModalAsync()
        {
            var maintenanceForm = $@"
                <div class='bg-white rounded-lg p-6 max-w-2xl w-full mx-4'>
                    <h3 class='text-lg font-semibold mb-4'>Schedule Maintenance Task</h3>
                    <form hx-post='/Admin?handler=CreateMaintenanceTask' hx-include='this' class='space-y-4'>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Maintenance Type</label>
                                <select name='MaintenanceType' class='w-full border border-gray-300 rounded-lg p-2' required>
                                    <option value='Preventive'>Preventive Maintenance</option>
                                    <option value='Corrective'>Corrective Maintenance</option>
                                    <option value='Predictive'>Predictive Maintenance</option>
                                    <option value='Calibration'>Equipment Calibration</option>
                                    <option value='Cleaning'>Deep Cleaning</option>
                                </select>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Machine</label>
                                <select name='MachineId' class='w-full border border-gray-300 rounded-lg p-2' required>
                                    <option value='TI1'>TI1 - Titanium Line 1</option>
                                    <option value='TI2'>TI2 - Titanium Line 2</option>
                                    <option value='INC'>INC - Inconel Line</option>
                                </select>
                            </div>
                        </div>
                        
                        <div>
                            <label class='block text-sm font-medium text-gray-700 mb-1'>Maintenance Description</label>
                            <textarea name='Description' class='w-full border border-gray-300 rounded-lg p-2' rows='3' required placeholder='Describe the maintenance work to be performed...'></textarea>
                        </div>
                        
                        <div class='grid grid-cols-2 gap-4'>
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Scheduled Date</label>
                                <input type='datetime-local' name='ScheduledDate' class='w-full border border-gray-300 rounded-lg p-2' required />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Estimated Duration (hours)</label>
                                <input type='number' name='EstimatedHours' class='w-full border border-gray-300 rounded-lg p-2' min='0.5' max='24' step='0.5' value='2' />
                            </div>
                        </div>
                        
                        <div>
                            <label class='block text-sm font-medium text-gray-700 mb-1'>Special Instructions</label>
                            <textarea name='Instructions' class='w-full border border-gray-300 rounded-lg p-2' rows='2' placeholder='Any special tools, safety requirements, or procedures...'></textarea>
                        </div>
                        
                        <div class='flex justify-end space-x-3 pt-4'>
                            <button type='button' onclick='hideModal()' class='px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50'>
                                Cancel
                            </button>
                            <button type='submit' class='px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700'>
                                Schedule Maintenance
                            </button>
                        </div>
                    </form>
                </div>";

            return Content(maintenanceForm, "text/html");
        }

        public async Task<IActionResult> OnPostCreateMaintenanceTaskAsync(string maintenanceType, 
            string machineId, string description, DateTime scheduledDate, double estimatedHours, string? instructions)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
                
                var task = new OperationalTask
                {
                    Title = $"{maintenanceType} - {machineId}",
                    Description = $"{description}\n\nEstimated Duration: {estimatedHours} hours\n\nInstructions:\n{instructions}",
                    TaskType = "Maintenance",
                    Priority = maintenanceType == "Corrective" ? 1 : 2,
                    MachineId = machineId,
                    DueAt = scheduledDate,
                    Category = maintenanceType,
                    Tags = $"maintenance,{maintenanceType.ToLower()},{machineId.ToLower()}",
                    CreatedByUserId = userId,
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<OperationalTask>().Add(task);
                await _context.SaveChangesAsync();

                return Content("<script>hideModal(); showNotification('Maintenance task scheduled successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance task");
                return Content($"<script>showNotification('Error scheduling maintenance: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostGenerateTaskReportAsync()
        {
            try
            {
                var report = await GenerateTaskSummaryAsync();
                var response = $@"
                    <div class='mt-2 p-3 bg-green-100 rounded border border-green-200'>
                        <div class='text-green-700'>
                            <p class='font-medium'><i class='fas fa-file-alt'></i> Task Report Generated</p>
                            <p class='text-sm mt-1'>{report}</p>
                        </div>
                    </div>";

                return Content(response, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating task report");
                return Content($@"
                    <div class='mt-2 p-3 bg-red-100 rounded border border-red-200'>
                        <p class='text-red-700 text-sm'>Error generating report: {ex.Message}</p>
                    </div>", "text/html");
            }
        }

        private async Task<string> GenerateTaskSummaryAsync()
        {
            var total = await _context.Set<OperationalTask>().CountAsync();
            var open = await _context.Set<OperationalTask>().CountAsync(t => t.Status == "Open");
            var completed = await _context.Set<OperationalTask>().CountAsync(t => t.Status == "Completed");
            var overdue = await _context.Set<OperationalTask>().CountAsync(t => t.OverdueFlag == 1);

            return $"Total: {total}, Open: {open}, Completed: {completed}, Overdue: {overdue}";
        }

        // Task Management Actions
        public async Task<IActionResult> OnPostStartTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    return Content("<script>showNotification('Task not found', 'error');</script>", "text/html");
                }

                if (task.Status != "Open")
                {
                    return Content("<script>showNotification('Task is not in Open status', 'warning');</script>", "text/html");
                }

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
                
                task.Status = "InProgress";
                task.AssignedUserId = userId;
                task.LastResetAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return Content("<script>showNotification('Task started successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting task {TaskId}", taskId);
                return Content($"<script>showNotification('Error starting task: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int taskId, string? completionNotes = null)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    return Content("<script>showNotification('Task not found', 'error');</script>", "text/html");
                }

                if (task.Status != "InProgress")
                {
                    return Content("<script>showNotification('Task must be in progress to complete', 'warning');</script>", "text/html");
                }

                task.Status = "Completed";
                task.CompletedAt = DateTime.UtcNow;
                task.OverdueFlag = 0; // Clear overdue flag
                
                if (!string.IsNullOrEmpty(completionNotes))
                {
                    task.Description += $"\n\nCompletion Notes: {completionNotes}";
                }
                
                await _context.SaveChangesAsync();

                return Content("<script>showNotification('Task completed successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId}", taskId);
                return Content($"<script>showNotification('Error completing task: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnGetEditTaskModalAsync(int taskId)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    return Content("<script>showNotification('Task not found', 'error');</script>", "text/html");
                }

                var editForm = $@"
                    <div class='bg-white rounded-lg p-6 max-w-2xl w-full mx-4'>
                        <h3 class='text-lg font-semibold mb-4'>Edit Task</h3>
                        <form hx-post='/Admin?handler=UpdateTask' hx-include='this' class='space-y-4'>
                            <input type='hidden' name='TaskId' value='{task.Id}' />
                            
                            <div class='grid grid-cols-2 gap-4'>
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Task Type</label>
                                    <select name='TaskType' class='w-full border border-gray-300 rounded-lg p-2' required>
                                        <option value='Maintenance' {(task.TaskType == "Maintenance" ? "selected" : "")}>Maintenance</option>
                                        <option value='Quality' {(task.TaskType == "Quality" ? "selected" : "")}>Quality Check</option>
                                        <option value='Cleaning' {(task.TaskType == "Cleaning" ? "selected" : "")}>Cleaning</option>
                                        <option value='Inspection' {(task.TaskType == "Inspection" ? "selected" : "")}>Inspection</option>
                                        <option value='Calibration' {(task.TaskType == "Calibration" ? "selected" : "")}>Calibration</option>
                                        <option value='General' {(task.TaskType == "General" ? "selected" : "")}>General Task</option>
                                    </select>
                                </div>
                                
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Priority</label>
                                    <select name='Priority' class='w-full border border-gray-300 rounded-lg p-2' required>
                                        <option value='1' {(task.Priority == 1 ? "selected" : "")}>Critical (1)</option>
                                        <option value='2' {(task.Priority == 2 ? "selected" : "")}>High (2)</option>
                                        <option value='3' {(task.Priority == 3 ? "selected" : "")}>Normal (3)</option>
                                        <option value='4' {(task.Priority == 4 ? "selected" : "")}>Low (4)</option>
                                        <option value='5' {(task.Priority == 5 ? "selected" : "")}>Lowest (5)</option>
                                    </select>
                                </div>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Title</label>
                                <input type='text' name='Title' value='{task.Title}' class='w-full border border-gray-300 rounded-lg p-2' required maxlength='160' />
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Description</label>
                                <textarea name='Description' class='w-full border border-gray-300 rounded-lg p-2' rows='3'>{task.Description}</textarea>
                            </div>
                            
                            <div class='grid grid-cols-2 gap-4'>
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Machine (Optional)</label>
                                    <select name='MachineId' class='w-full border border-gray-300 rounded-lg p-2'>
                                        <option value=''>Select Machine</option>
                                        <option value='TI1' {(task.MachineId == "TI1" ? "selected" : "")}>TI1 - Titanium Line 1</option>
                                        <option value='TI2' {(task.MachineId == "TI2" ? "selected" : "")}>TI2 - Titanium Line 2</option>
                                        <option value='INC' {(task.MachineId == "INC" ? "selected" : "")}>INC - Inconel Line</option>
                                    </select>
                                </div>
                                
                                <div>
                                    <label class='block text-sm font-medium text-gray-700 mb-1'>Due Date (Optional)</label>
                                    <input type='datetime-local' name='DueAt' value='{task.DueAt?.ToString("yyyy-MM-ddTHH:mm")}' class='w-full border border-gray-300 rounded-lg p-2' />
                                </div>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Status</label>
                                <select name='Status' class='w-full border border-gray-300 rounded-lg p-2' required>
                                    <option value='Open' {(task.Status == "Open" ? "selected" : "")}>Open</option>
                                    <option value='InProgress' {(task.Status == "InProgress" ? "selected" : "")}>In Progress</option>
                                    <option value='Completed' {(task.Status == "Completed" ? "selected" : "")}>Completed</option>
                                    <option value='Cancelled' {(task.Status == "Cancelled" ? "selected" : "")}>Cancelled</option>
                                </select>
                            </div>
                            
                            <div>
                                <label class='block text-sm font-medium text-gray-700 mb-1'>Category/Tags (Optional)</label>
                                <input type='text' name='Tags' value='{task.Tags}' class='w-full border border-gray-300 rounded-lg p-2' placeholder='maintenance, quality, urgent' />
                            </div>
                            
                            <div class='flex justify-end space-x-3 pt-4'>
                                <button type='button' onclick='hideModal()' class='px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50'>
                                    Cancel
                                </button>
                                <button type='submit' class='px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700'>
                                    Update Task
                                </button>
                            </div>
                        </form>
                    </div>";

                return Content(editForm, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit task modal for {TaskId}", taskId);
                return Content("<script>showNotification('Error loading task edit form', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostUpdateTaskAsync(int taskId, string title, string? description,
            string taskType, int priority, string? machineId, DateTime? dueAt, string status, string? tags)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    return Content("<script>showNotification('Task not found', 'error');</script>", "text/html");
                }

                task.Title = title;
                task.Description = description;
                task.TaskType = taskType;
                task.Priority = priority;
                task.MachineId = machineId;
                task.DueAt = dueAt;
                task.Status = status;
                task.Tags = tags;
                
                // If task is being marked as completed, set completion timestamp
                if (status == "Completed" && task.CompletedAt == null)
                {
                    task.CompletedAt = DateTime.UtcNow;
                    task.OverdueFlag = 0;
                }
                
                await _context.SaveChangesAsync();

                return Content("<script>hideModal(); showNotification('Task updated successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", taskId);
                return Content($"<script>showNotification('Error updating task: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        public async Task<IActionResult> OnPostDeleteTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    return Content("<script>showNotification('Task not found', 'error');</script>", "text/html");
                }

                _context.Set<OperationalTask>().Remove(task);
                await _context.SaveChangesAsync();

                return Content("<script>showNotification('Task deleted successfully!', 'success'); setTimeout(() => window.location.reload(), 1000);</script>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
                return Content($"<script>showNotification('Error deleting task: {ex.Message}', 'error');</script>", "text/html");
            }
        }

        // Backup functionality (kept for essential admin operations)
        public async Task<IActionResult> OnPostBackupDatabaseAsync()
        {
            try
            {
                var backupFileName = $"opcentrix_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                var backupPath = Path.Combine("Backups", backupFileName);
                
                Directory.CreateDirectory("Backups");
                
                var sourcePath = _context.Database.GetConnectionString();
                if (sourcePath?.Contains("Data Source=") == true)
                {
                    var sourceFile = sourcePath.Split('=')[1].Split(';')[0];
                    System.IO.File.Copy(sourceFile, backupPath);
                }

                return Content($@"
                    <div class='mt-2 p-3 bg-green-100 rounded border border-green-200'>
                        <div class='text-green-700'>
                            <p class='font-medium'><i class='fas fa-check-circle'></i> Backup Created</p>
                            <p class='text-sm mt-1'>Database backed up to: {backupFileName}</p>
                        </div>
                    </div>", "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database backup");
                return Content($@"
                    <div class='mt-2 p-3 bg-red-100 rounded border border-red-200'>
                        <p class='text-red-700 text-sm'>Backup failed: {ex.Message}</p>
                    </div>", "text/html");
            }
        }
    }
}