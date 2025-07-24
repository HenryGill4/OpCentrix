using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpCentrix.Models;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a TruPrint 3000 machine configuration and status
    /// </summary>
    public class SlsMachine
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty; // TI1, TI2, INC, etc.
        
        [Required]
        [StringLength(100)]
        public string MachineName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string MachineModel { get; set; } = "TruPrint 3000";
        
        [StringLength(50)]
        public string SerialNumber { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;
        
        #region Machine Capabilities
        
        // Build envelope specifications
        [Range(0, 500)]
        public double BuildLengthMm { get; set; } = 250;
        
        [Range(0, 500)]
        public double BuildWidthMm { get; set; } = 250;
        
        [Range(0, 500)]
        public double BuildHeightMm { get; set; } = 300;
        
        // Supported materials
        [StringLength(500)]
        public string SupportedMaterials { get; set; } = "Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23";
        
        [StringLength(100)]
        public string CurrentMaterial { get; set; } = string.Empty;
        
        // Process capabilities
        [Range(0, 3000)]
        public double MaxLaserPowerWatts { get; set; } = 400;
        
        [Range(0, 10000)]
        public double MaxScanSpeedMmPerSec { get; set; } = 7000;
        
        [Range(5, 100)]
        public double MinLayerThicknessMicrons { get; set; } = 20;
        
        [Range(5, 100)]
        public double MaxLayerThicknessMicrons { get; set; } = 60;
        
        #endregion
        
        #region OPC UA Configuration
        
        [StringLength(200)]
        public string OpcUaEndpointUrl { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OpcUaUsername { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string OpcUaPasswordHash { get; set; } = string.Empty; // Encrypted password
        
        [StringLength(100)]
        public string OpcUaNamespace { get; set; } = string.Empty;
        
        public bool OpcUaEnabled { get; set; } = false;
        
        public DateTime? OpcUaLastConnection { get; set; }
        
        [StringLength(500)]
        public string OpcUaConnectionStatus { get; set; } = "Disconnected";
        
        #endregion
        
        #region Current Status
        
        [StringLength(50)]
        public string Status { get; set; } = "Offline"; // Offline, Idle, Building, Maintenance, Error
        
        [StringLength(500)]
        public string StatusMessage { get; set; } = string.Empty;
        
        public DateTime LastStatusUpdate { get; set; } = DateTime.UtcNow;
        
        // Current job information
        public int? CurrentJobId { get; set; }
        public virtual Job? CurrentJob { get; set; }
        
        [Range(0, 100)]
        public double CurrentBuildProgress { get; set; } = 0;
        
        public DateTime? CurrentJobStartTime { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        
        #endregion
        
        #region Real-time Telemetry
        
        // Temperature data
        [Range(0, 500)]
        public double CurrentBuildTemperature { get; set; } = 0;
        
        [Range(0, 500)]
        public double TargetBuildTemperature { get; set; } = 0;
        
        [Range(0, 100)]
        public double AmbientTemperature { get; set; } = 0;
        
        // Atmosphere data
        [Range(0, 100)]
        public double CurrentOxygenLevel { get; set; } = 0;
        
        [Range(0, 100)]
        public double ArgonFlowRate { get; set; } = 0;
        
        [Range(0, 100)]
        public double ArgonPressure { get; set; } = 0;
        
        // Laser system
        [Range(0, 1000)]
        public double CurrentLaserPower { get; set; } = 0;
        
        [Range(0, 10000)]
        public double LaserOnTime { get; set; } = 0; // Total on-time in seconds
        
        public bool LaserStatus { get; set; } = false;
        
        // Powder system
        [Range(0, 100)]
        public double PowderLevelPercent { get; set; } = 0;
        
        [Range(0, 50)]
        public double PowderRemainingKg { get; set; } = 0;
        
        public DateTime? LastPowderRefill { get; set; }
        
        // Build platform
        [Range(0, 500)]
        public double CurrentBuildHeight { get; set; } = 0;
        
        [Range(0, 10000)]
        public int TotalLayersCompleted { get; set; } = 0;
        
        [Range(0, 10000)]
        public int TotalLayersPlanned { get; set; } = 0;
        
        #endregion
        
        #region Maintenance and Utilization
        
        // Operating hours
        [Range(0, double.MaxValue)]
        public double TotalOperatingHours { get; set; } = 0;
        
        [Range(0, double.MaxValue)]
        public double HoursSinceLastMaintenance { get; set; } = 0;
        
        [Range(0, 1000)]
        public double MaintenanceIntervalHours { get; set; } = 500;
        
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        
        // Build statistics
        public int TotalJobsCompleted { get; set; } = 0;
        public int TotalPartsPrinted { get; set; } = 0;
        
        [Range(0, 100)]
        public double AverageUtilizationPercent { get; set; } = 0;
        
        [Range(0, 100)]
        public double QualityScorePercent { get; set; } = 100;
        
        #endregion
        
        #region Configuration and Settings
        
        public bool IsActive { get; set; } = true;
        public bool IsAvailableForScheduling { get; set; } = true;
        
        [Range(1, 10)]
        public int Priority { get; set; } = 3; // Scheduling priority
        
        [StringLength(1000)]
        public string MaintenanceNotes { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string OperatorNotes { get; set; } = string.Empty;
        
        // Audit trail
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;
        
        #endregion
        
        #region Computed Properties (NotMapped)
        
        [NotMapped]
        public bool RequiresMaintenance => HoursSinceLastMaintenance >= MaintenanceIntervalHours;
        
        [NotMapped]
        public double BuildVolumeM3 => (BuildLengthMm * BuildWidthMm * BuildHeightMm) / 1_000_000_000.0;
        
        [NotMapped]
        public string MaterialCompatibility => string.Join(", ", GetSupportedMaterialsList());
        
        [NotMapped]
        public double CurrentBuildProgressPercent => TotalLayersPlanned > 0 
            ? Math.Round((double)TotalLayersCompleted / TotalLayersPlanned * 100, 1) 
            : 0;
        
        [NotMapped]
        public TimeSpan? EstimatedTimeRemaining => EstimatedCompletionTime.HasValue 
            ? EstimatedCompletionTime.Value - DateTime.UtcNow 
            : null;
        
        [NotMapped]
        public bool IsConnectedToOpcUa => OpcUaEnabled && 
            OpcUaLastConnection.HasValue && 
            OpcUaLastConnection.Value > DateTime.UtcNow.AddMinutes(-5);
        
        #endregion
        
        #region Helper Methods
        
        public string[] GetSupportedMaterialsList()
        {
            return SupportedMaterials?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(m => m.Trim()).ToArray() ?? Array.Empty<string>();
        }
        
        public bool SupportsMaterial(string material)
        {
            return GetSupportedMaterialsList().Contains(material, StringComparer.OrdinalIgnoreCase);
        }
        
        public bool CanAccommodatePart(Part part)
        {
            return part.LengthMm <= BuildLengthMm &&
                   part.WidthMm <= BuildWidthMm &&
                   part.HeightMm <= BuildHeightMm &&
                   SupportsMaterial(part.SlsMaterial);
        }
        
        public string GetStatusColor()
        {
            return Status?.ToLower() switch
            {
                "building" => "#10B981", // green
                "idle" => "#3B82F6", // blue
                "maintenance" => "#F59E0B", // amber
                "error" => "#EF4444", // red
                "offline" => "#6B7280", // gray
                _ => "#9CA3AF" // light gray
            };
        }
        
        public double CalculateUtilizationPercent(DateTime fromDate, DateTime toDate)
        {
            var totalHours = (toDate - fromDate).TotalHours;
            if (totalHours <= 0) return 0;
            
            // This would typically query actual job data
            // For now, return stored average
            return AverageUtilizationPercent;
        }
        
        public bool IsCompatibleMaterial(string newMaterial)
        {
            if (string.IsNullOrEmpty(CurrentMaterial))
                return true;
            
            // Define material compatibility matrix
            var compatibilityMatrix = new Dictionary<string, string[]>
            {
                ["Ti-6Al-4V Grade 5"] = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                ["Ti-6Al-4V ELI Grade 23"] = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                ["Inconel 718"] = new[] { "Inconel 718", "Inconel 625" },
                ["Inconel 625"] = new[] { "Inconel 718", "Inconel 625" }
            };
            
            return compatibilityMatrix.ContainsKey(CurrentMaterial) &&
                   compatibilityMatrix[CurrentMaterial].Contains(newMaterial);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents real-time data from TruPrint 3000 via OPC UA
    /// </summary>
    public class MachineDataSnapshot
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;
        
        public int SlsMachineId { get; set; }
        public virtual SlsMachine? SlsMachine { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Process data snapshot
        [StringLength(2000)]
        public string ProcessDataJson { get; set; } = "{}";
        
        // Quality data snapshot
        [StringLength(2000)]
        public string QualityDataJson { get; set; } = "{}";
        
        // Alarm and event data
        [StringLength(2000)]
        public string AlarmDataJson { get; set; } = "{}";
        
        // Performance metrics
        [Range(0, 100)]
        public double UtilizationPercent { get; set; } = 0;
        
        [Range(0, 1000)]
        public double EnergyConsumptionKwh { get; set; } = 0;
        
        // Material consumption
        [Range(0, 50)]
        public double PowderConsumedKg { get; set; } = 0;
        
        [Range(0, 1000)]
        public double ArgonConsumedM3 { get; set; } = 0;
    }
}

namespace OpCentrix.Services
{
    /// <summary>
    /// Service interface for OPC UA communication with TruPrint 3000 machines
    /// </summary>
    public interface IOpcUaService
    {
        Task<bool> ConnectToMachineAsync(string machineId);
        Task<bool> DisconnectFromMachineAsync(string machineId);
        Task<MachineDataSnapshot?> GetMachineDataAsync(string machineId);
        Task<bool> SendJobToMachineAsync(string machineId, Job job);
        Task<bool> StartJobAsync(string machineId, int jobId);
        Task<bool> StopJobAsync(string machineId, int jobId);
        Task<List<string>> GetMachineAlarmsAsync(string machineId);
        Task<bool> IsConnectedAsync(string machineId);
    }
    
    /// <summary>
    /// OPC UA service implementation for TruPrint 3000 integration
    /// Currently returns mock data - implement OPC UA client when hardware is available
    /// </summary>
    public class OpcUaService : IOpcUaService
    {
        private readonly ILogger<OpcUaService> _logger;
        private readonly Dictionary<string, bool> _connectionStatus = new();
        
        public OpcUaService(ILogger<OpcUaService> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> ConnectToMachineAsync(string machineId)
        {
            _logger.LogInformation("Attempting to connect to machine {MachineId}", machineId);
            
            // TODO: Implement actual OPC UA connection
            // For now, simulate connection
            await Task.Delay(1000); // Simulate connection time
            
            _connectionStatus[machineId] = true;
            _logger.LogInformation("Successfully connected to machine {MachineId}", machineId);
            
            return true;
        }
        
        public async Task<bool> DisconnectFromMachineAsync(string machineId)
        {
            _logger.LogInformation("Disconnecting from machine {MachineId}", machineId);
            
            await Task.Delay(500);
            _connectionStatus.Remove(machineId);
            
            return true;
        }
        
        public async Task<MachineDataSnapshot?> GetMachineDataAsync(string machineId)
        {
            if (!_connectionStatus.GetValueOrDefault(machineId, false))
            {
                _logger.LogWarning("Machine {MachineId} is not connected", machineId);
                return null;
            }
            
            await Task.Delay(100); // Simulate data retrieval
            
            // Return mock data for development
            return new MachineDataSnapshot
            {
                MachineId = machineId,
                Timestamp = DateTime.UtcNow,
                ProcessDataJson = GenerateMockProcessData(machineId),
                QualityDataJson = GenerateMockQualityData(),
                AlarmDataJson = "{}",
                UtilizationPercent = new Random().Next(70, 95),
                EnergyConsumptionKwh = new Random().Next(50, 150),
                PowderConsumedKg = Math.Round(new Random().NextDouble() * 2, 2),
                ArgonConsumedM3 = Math.Round(new Random().NextDouble() * 10, 1)
            };
        }
        
        public async Task<bool> SendJobToMachineAsync(string machineId, Job job)
        {
            if (!_connectionStatus.GetValueOrDefault(machineId, false))
            {
                _logger.LogWarning("Cannot send job to disconnected machine {MachineId}", machineId);
                return false;
            }
            
            _logger.LogInformation("Sending job {JobId} to machine {MachineId}", job.Id, machineId);
            
            // TODO: Implement actual job transfer via OPC UA
            await Task.Delay(2000); // Simulate file transfer
            
            _logger.LogInformation("Job {JobId} successfully sent to machine {MachineId}", job.Id, machineId);
            return true;
        }
        
        public async Task<bool> StartJobAsync(string machineId, int jobId)
        {
            _logger.LogInformation("Starting job {JobId} on machine {MachineId}", jobId, machineId);
            
            await Task.Delay(1000);
            return true;
        }
        
        public async Task<bool> StopJobAsync(string machineId, int jobId)
        {
            _logger.LogInformation("Stopping job {JobId} on machine {MachineId}", jobId, machineId);
            
            await Task.Delay(1000);
            return true;
        }
        
        public async Task<List<string>> GetMachineAlarmsAsync(string machineId)
        {
            await Task.Delay(100);
            
            // Return mock alarms occasionally
            if (new Random().Next(0, 10) > 7)
            {
                return new List<string>
                {
                    "Low powder level detected",
                    "Oxygen level slightly elevated"
                };
            }
            
            return new List<string>();
        }
        
        public async Task<bool> IsConnectedAsync(string machineId)
        {
            await Task.Delay(50);
            return _connectionStatus.GetValueOrDefault(machineId, false);
        }
        
        private string GenerateMockProcessData(string machineId)
        {
            var random = new Random();
            var data = new
            {
                LaserPower = random.Next(180, 220),
                ScanSpeed = random.Next(1100, 1300),
                BuildTemperature = random.Next(175, 185),
                OxygenLevel = random.Next(30, 60),
                ArgonFlowRate = random.Next(15, 25),
                BuildProgress = random.Next(0, 100),
                LayersCompleted = random.Next(0, 1000),
                EstimatedTimeRemaining = random.Next(60, 480) // minutes
            };
            
            return System.Text.Json.JsonSerializer.Serialize(data);
        }
        
        private string GenerateMockQualityData()
        {
            var random = new Random();
            var data = new
            {
                SurfaceQuality = Math.Round(random.NextDouble() * 20 + 80, 1), // 80-100%
                DimensionalAccuracy = Math.Round(random.NextDouble() * 10 + 90, 1), // 90-100%
                Density = Math.Round(random.NextDouble() * 2 + 98, 1), // 98-100%
                DefectRate = Math.Round(random.NextDouble() * 5, 2) // 0-5%
            };
            
            return System.Text.Json.JsonSerializer.Serialize(data);
        }
    }
}