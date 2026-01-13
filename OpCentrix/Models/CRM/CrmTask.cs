using System.ComponentModel.DataAnnotations;
using OpCentrix.Models;

namespace OpCentrix.Models.CRM;

public class CrmTask
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, InProgress, Completed

    public int Priority { get; set; } = 3; // 1-5

    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int? AssignedToUserId { get; set; }
    public int CreatedByUserId { get; set; }

    public int? AccountId { get; set; }
    public int? ContactId { get; set; }

    public CrmAccount? Account { get; set; }
    public CrmContact? Contact { get; set; }
    public User? AssignedToUser { get; set; }

    // Progress tracking
    public virtual List<CrmTaskProgress> ProgressEntries { get; set; } = new();

    // Reminder and notification settings
    public bool HasReminder { get; set; }
    public DateTime? ReminderDateTime { get; set; }
    public int ReminderMinutesBefore { get; set; } = 15; // Default 15 minutes before due date
    public string ReminderType { get; set; } = "Email"; // Email, Browser, Both
    
    public bool IsReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    
    // Notification preferences
    public bool NotifyOnStatusChange { get; set; } = true;
    public bool NotifyAssignee { get; set; } = true;
    public bool NotifyCreator { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}
