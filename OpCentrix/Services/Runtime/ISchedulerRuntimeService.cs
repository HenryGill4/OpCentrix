using System.Threading.Tasks;
using OpCentrix.Models;

namespace OpCentrix.Services.Runtime
{
    public interface ISchedulerRuntimeService
    {
        Task<(bool Success, string? Error, Job Job)> StartJobAsync(int jobId, StartJobRequest request, string userName, int? userId);
        Task<(bool Success, string? Error, Job Job)> CompleteJobAsync(int jobId, CompleteJobRequest request, string userName, int? userId);
    }

    public class StartJobRequest
    {
        public byte? StackLevel { get; set; }
        public int ActualUnitsPlanned { get; set; }
        public int? PrototypeUnitsPlanned { get; set; }
        public decimal? PowderAddedKg { get; set; }
        public double? OverrideDurationHours { get; set; }
        public string? Notes { get; set; }
    }

    public class CompleteJobRequest
    {
        public int? ProducedQuantity { get; set; }
        public int? DefectQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
