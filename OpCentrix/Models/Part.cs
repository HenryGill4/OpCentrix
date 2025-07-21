//Part.cs:
namespace OpCentrix.Models
{
    public class Part
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string AvgDuration { get; set; } = "1h 0m";
    }
}
