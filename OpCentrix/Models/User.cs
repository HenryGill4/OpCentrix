using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginDate { get; set; }
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string LastModifiedBy { get; set; } = string.Empty;
        
        // Navigation property for user settings
        public virtual UserSettings? Settings { get; set; }
        
        // Role-based permissions
        public bool CanAccessScheduler => Role is "Admin" or "Manager" or "Scheduler" or "Operator";
        public bool CanAccessCoating => Role is "Admin" or "CoatingSpecialist" or "Manager";
        public bool CanAccessShipping => Role is "Admin" or "ShippingSpecialist" or "Manager";
        public bool CanAccessEDM => Role is "Admin" or "EDMSpecialist" or "Manager";
        public bool CanAccessMachining => Role is "Admin" or "MachiningSpecialist" or "Manager";
        public bool CanAccessQC => Role is "Admin" or "QCSpecialist" or "Manager";
        public bool CanAccessMedia => Role is "Admin" or "MediaSpecialist" or "Manager";
        public bool CanAccess3DPrinting => Role is "Admin" or "PrintingSpecialist" or "Manager" or "Operator";
        public bool CanAccessAnalytics => Role is "Admin" or "Manager" or "Analyst";
        public bool CanAccessJobLog => Role is "Admin" or "Manager" or "Scheduler";
        public bool CanAccessAdmin => Role is "Admin";
        
        // Permission levels
        public bool CanEditJobs => Role is "Admin" or "Scheduler" or "Manager";
        public bool CanDeleteJobs => Role is "Admin" or "Manager";
        public bool CanViewAllData => Role is "Admin" or "Manager";
        public bool CanManageUsers => Role is "Admin";
    }
    
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Scheduler = "Scheduler";
        public const string Operator = "Operator";
        public const string CoatingSpecialist = "CoatingSpecialist";
        public const string ShippingSpecialist = "ShippingSpecialist";
        public const string EDMSpecialist = "EDMSpecialist";
        public const string MachiningSpecialist = "MachiningSpecialist";
        public const string QCSpecialist = "QCSpecialist";
        public const string MediaSpecialist = "MediaSpecialist";
        public const string PrintingSpecialist = "PrintingSpecialist";
        public const string Analyst = "Analyst";
        
        public static readonly string[] AllRoles = 
        {
            Admin, Manager, Scheduler, Operator, CoatingSpecialist,
            ShippingSpecialist, EDMSpecialist, MachiningSpecialist,
            QCSpecialist, MediaSpecialist, PrintingSpecialist, Analyst
        };
        
        public static string GetDisplayName(string role) => role switch
        {
            Admin => "System Administrator",
            Manager => "Production Manager",
            Scheduler => "Production Scheduler",
            Operator => "Machine Operator",
            CoatingSpecialist => "Coating Specialist",
            ShippingSpecialist => "Shipping Specialist",
            EDMSpecialist => "EDM Specialist",
            MachiningSpecialist => "Machining Specialist",
            QCSpecialist => "Quality Control Specialist",
            MediaSpecialist => "Media Specialist",
            PrintingSpecialist => "3D Printing Specialist",
            Analyst => "Data Analyst",
            _ => role
        };
    }
}