using System;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        public string PartNumber { get; set; }

        [Required]
        public string Printer { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public int DurationMinutes { get; set; }
    }
}
