using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using System.Security.Claims;

namespace OpCentrix.Controllers.Api;

[Route("api/tasks")]
[ApiController]
[Authorize]
public class TaskNotificationController : ControllerBase
{
    private readonly SchedulerContext _context;
    private readonly ILogger<TaskNotificationController> _logger;

    public TaskNotificationController(SchedulerContext context, ILogger<TaskNotificationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("user-notifications")]
    public async Task<IActionResult> GetUserNotifications([FromQuery] int userId = 0)
    {
        try
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                _logger.LogWarning("Unable to determine current user ID from claims");
                return BadRequest("Unable to identify current user");
            }

            // If no userId specified, use current user's ID
            if (userId <= 0)
            {
                userId = currentUserId;
            }
            
            // Users can only access their own notifications unless they're admin/manager
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (userId != currentUserId && userRole != "Admin" && userRole != "Manager")
            {
                return Forbid();
            }

            var tasks = await _context.CrmTasks
                .Include(t => t.ProgressEntries)
                .Where(t => t.AssignedToUserId == userId && t.Status != "Completed")
                .ToListAsync();

            var now = DateTime.UtcNow;
            
            var recentTasks = tasks
                .Where(t => t.DueAt.HasValue && t.DueAt >= now.AddDays(-7))
                .OrderBy(t => t.DueAt)
                .Take(5)
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    dueAt = t.DueAt,
                    priority = t.Priority,
                    status = t.Status,
                    progress = CalculateCompletionPercentage(t)
                })
                .ToList();

            var urgentTasks = tasks.Count(t => 
                (t.Priority >= 4) || 
                (t.DueAt.HasValue && t.DueAt <= now.AddDays(1)));

            // Calculate priority breakdown
            var priorityBreakdown = new
            {
                critical = tasks.Count(t => t.Priority == 5),
                urgent = tasks.Count(t => t.Priority == 4),
                high = tasks.Count(t => t.Priority == 3),
                normal = tasks.Count(t => t.Priority == 2),
                low = tasks.Count(t => t.Priority == 1)
            };

            var result = new
            {
                myTasksCount = tasks.Count,
                urgentTasksCount = urgentTasks,
                recentTasks = recentTasks,
                taskPriorities = priorityBreakdown
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user notifications for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("quick-dropdown")]
    public async Task<IActionResult> GetQuickDropdown([FromQuery] int userId = 0)
    {
        try
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                _logger.LogWarning("Unable to determine current user ID from claims");
                return Content("<div class=\"p-4 text-red-600 text-sm\">Unable to identify current user</div>", "text/html");
            }

            // If no userId specified, use current user's ID
            if (userId <= 0)
            {
                userId = currentUserId;
            }
            
            // Users can only access their own data unless they're admin/manager
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (userId != currentUserId && userRole != "Admin" && userRole != "Manager")
            {
                return Forbid();
            }

            var now = DateTime.UtcNow;
            var tasks = await _context.CrmTasks
                .Include(t => t.Account)
                .Include(t => t.ProgressEntries)
                .Where(t => t.AssignedToUserId == userId && t.Status != "Completed")
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
                .Take(10)
                .ToListAsync();

            var html = "<div class=\"p-4\">";
            
            if (tasks.Any())
            {
                html += "<div class=\"text-sm font-medium text-gray-700 mb-3\">Your Active Tasks</div>";
                
                foreach (var task in tasks.Take(5))
                {
                    var isOverdue = task.DueAt.HasValue && task.DueAt < now;
                    var progressPercentage = CalculateCompletionPercentage(task);
                    
                    html += "<div class=\"mb-3 p-3 bg-gray-50 rounded-lg border\">";
                    html += $"<div class=\"flex items-center justify-between mb-1\">";
                    html += $"<span class=\"font-medium text-sm text-gray-900 truncate\" title=\"{task.Title}\">";
                    html += task.Title.Length > 25 ? task.Title.Substring(0, 25) + "..." : task.Title;
                    html += "</span>";
                    
                    if (task.Priority >= 4)
                    {
                        html += "<span class=\"px-2 py-1 bg-red-100 text-red-800 text-xs rounded-full font-medium\">High</span>";
                    }
                    
                    html += "</div>";
                    
                    if (progressPercentage.HasValue)
                    {
                        html += "<div class=\"w-full bg-gray-200 rounded-full h-2 mb-2\">";
                        html += $"<div class=\"bg-blue-600 h-2 rounded-full\" style=\"width: {progressPercentage}%\"></div>";
                        html += "</div>";
                    }
                    
                    html += "<div class=\"flex items-center justify-between text-xs text-gray-600\">";
                    
                    if (task.DueAt.HasValue)
                    {
                        var dueClass = isOverdue ? "text-red-600 font-semibold" : "text-gray-600";
                        html += $"<span class=\"{dueClass}\">Due {task.DueAt:MMM dd}</span>";
                    }
                    else
                    {
                        html += "<span>No due date</span>";
                    }
                    
                    html += $"<a href=\"/Tasks/{task.Id}/AddProgress\" class=\"text-blue-600 hover:text-blue-800\">Update</a>";
                    html += "</div>";
                    html += "</div>";
                }
                
                if (tasks.Count > 5)
                {
                    html += $"<div class=\"text-center mt-3\">";
                    html += $"<a href=\"/Tasks/MyTasks\" class=\"text-blue-600 hover:text-blue-800 text-sm\">View all {tasks.Count} tasks</a>";
                    html += "</div>";
                }
            }
            else
            {
                html += "<div class=\"text-center text-gray-500 py-6\">";
                html += "<i class=\"fas fa-check-circle text-2xl mb-2\"></i>";
                html += "<div class=\"text-sm\">No active tasks</div>";
                html += "</div>";
            }
            
            html += "</div>";
            
            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick dropdown for user {UserId}", userId);
            return Content("<div class=\"p-4 text-red-600 text-sm\">Error loading tasks</div>", "text/html");
        }
    }

    private int? CalculateCompletionPercentage(OpCentrix.Models.CRM.CrmTask task)
    {
        if (task.Status == "Completed")
            return 100;

        if (task.ProgressEntries?.Any() != true)
            return task.Status == "InProgress" ? 10 : 0;

        var latestProgress = task.ProgressEntries
            .OrderByDescending(p => p.CreatedDate)
            .FirstOrDefault();

        return latestProgress?.PercentComplete ?? (task.Status == "InProgress" ? 25 : 0);
    }
}