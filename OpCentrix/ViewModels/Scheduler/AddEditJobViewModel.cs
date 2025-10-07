using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.ViewModels.Scheduler
{
    /// <summary>
    /// Modern view model for job creation/editing with proper machine + part (legacy & master) integration
    /// </summary>
    public class AddEditJobViewModel
    {
        public Job Job { get; set; } = new();
        public List<Part> Parts { get; set; } = new();              // Legacy parts
        public List<MasterPart> MasterParts { get; set; } = new();   // New master parts
        public List<Machine> Machines { get; set; } = new();
        public List<string> Errors { get; set; } = new();

        // Upstream dependency editing (Phase B.1 single predecessor)
        public List<Job>? CandidatePredecessors { get; set; } // populated server-side when opening modal
        public int? SelectedPredecessorId => Job.PredecessorJobId;
        public double? UpstreamGapHours => Job.UpstreamGapHours;

        public bool IsEdit => Job.Id > 0;
        public bool IsEditing => Job.Id > 0;
        public string Title => IsEditing ? "Edit Job" : "Add New Job";

        public int AvailableMachineCount => Machines?.Count ?? 0;
        public int AvailablePartCount => Parts?.Count ?? 0;
        public int AvailableMasterPartCount => MasterParts?.Count ?? 0;
        public bool HasMachines => AvailableMachineCount > 0;
        public bool HasParts => AvailablePartCount > 0;
        public bool HasMasterParts => AvailableMasterPartCount > 0;
    }
}
