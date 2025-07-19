using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    public class Part
    {
        public int Id { get; set; }

        [Required]
        public string PartNumber { get; set; }

        // This can be stored as a string like "2d 3h 15m" or normalized later
        public string AvgDuration { get; set; }
    }
}