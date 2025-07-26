using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    public class UserSettings
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        // Session timeout in minutes (30 min to 3 hours)
        [Range(30, 180, ErrorMessage = "Session timeout must be between 30 minutes and 3 hours")]
        public int SessionTimeoutMinutes { get; set; } = 120;
        
        // Theme preferences
        public string Theme { get; set; } = "Light";
        
        // Task 10: Scheduler orientation preference
        [StringLength(20)]
        public string SchedulerOrientation { get; set; } = "horizontal";
        
        // Notification preferences
        public bool EmailNotifications { get; set; } = true;
        public bool BrowserNotifications { get; set; } = true;
        
        // Dashboard preferences
        public string DefaultPage { get; set; } = "/Scheduler";
        public int ItemsPerPage { get; set; } = 20;
        
        // Timezone
        public string TimeZone { get; set; } = "UTC";
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}