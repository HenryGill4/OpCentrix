using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Assignment linking a user (employee) to a machine by MachineId string.
    /// Supports primary/secondary designation and effective date range.
    /// </summary>
    public class MachineOperatorAssignment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty; // references Machine.MachineId (string id)

        [Required]
        public int UserId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "System";

        // Optional navigation to User for convenience
        public virtual User? User { get; set; }

        [NotMapped]
        public bool IsCurrentlyEffective
        {
            get
            {
                var now = DateTime.UtcNow;
                var startOk = !EffectiveFrom.HasValue || EffectiveFrom.Value <= now;
                var endOk = !EffectiveTo.HasValue || EffectiveTo.Value >= now;
                return startOk && endOk && IsActive;
            }
        }
    }
}
