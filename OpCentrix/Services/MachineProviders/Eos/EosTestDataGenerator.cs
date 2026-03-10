using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders.Eos;

/// <summary>
/// Generates realistic EOS M4 ONIX test data for development and testing.
/// Simulates build phases: Warming ? Printing ? Cooling
/// </summary>
public class EosTestDataGenerator
{
    private readonly Dictionary<int, EosTestMachineState> _machineStates = new();
    private readonly Random _random = new();

    public MachineData GenerateMachineData(int machineId)
    {
        var state = GetOrCreateState(machineId);
        UpdateSimulation(state);

        return new MachineData
        {
            MachineId = machineId,
            Status = new MachineStatus
            {
                MachineId = machineId,
                Status = state.StatusString,
                IsConnected = true,
                BuildProgressPercent = state.Phase == BuildPhase.Printing ? state.BuildProgress : null,
                CurrentLayer = state.Phase == BuildPhase.Printing ? state.CurrentLayer : null,
                TotalLayers = state.Phase == BuildPhase.Printing ? state.TotalLayers : null,
                EstimatedMinutesRemaining = state.Phase == BuildPhase.Printing ? state.EstimatedMinutesRemaining : null,
                CurrentJobReference = state.JobName,
                Alarms = state.Alarms.ToList(),
                Timestamp = DateTime.UtcNow
            },
            Telemetry = GenerateTelemetry(state),
            ProviderType = "Eos",
            Timestamp = DateTime.UtcNow
        };
    }

    private EosTestMachineState GetOrCreateState(int machineId)
    {
        if (!_machineStates.TryGetValue(machineId, out var state))
        {
            state = new EosTestMachineState
            {
                MachineId = machineId,
                Phase = BuildPhase.Idle,
                TotalLayers = 800 + _random.Next(400), // 800-1200 layers
                JobName = $"Job-{machineId}-{DateTime.UtcNow:yyyyMMdd}",
                LastUpdate = DateTime.UtcNow
            };
            _machineStates[machineId] = state;
        }
        return state;
    }

    private void UpdateSimulation(EosTestMachineState state)
    {
        var elapsed = (DateTime.UtcNow - state.LastUpdate).TotalSeconds;
        if (elapsed < 1) return;

        state.LastUpdate = DateTime.UtcNow;

        switch (state.Phase)
        {
            case BuildPhase.Idle:
                // 10% chance to start warming each minute
                if (_random.NextDouble() < 0.002 * elapsed)
                {
                    state.Phase = BuildPhase.Warming;
                    state.WarmingProgress = 0;
                    state.JobName = $"Job-{state.MachineId}-{DateTime.UtcNow:HHmmss}";
                }
                break;

            case BuildPhase.Warming:
                // Warming takes ~30 minutes, progress 0.05% per second
                state.WarmingProgress += 0.05 * elapsed;
                state.BuildPlatformTemp = 25 + (state.WarmingProgress / 100.0) * 155; // 25?180°C

                if (state.WarmingProgress >= 100)
                {
                    state.Phase = BuildPhase.Printing;
                    state.CurrentLayer = 0;
                    state.BuildProgress = 0;
                }
                break;

            case BuildPhase.Printing:
                // Each layer takes ~20 seconds, so 3 layers per minute
                var layersPerSecond = 0.05;
                state.CurrentLayer += (int)(layersPerSecond * elapsed);
                state.BuildProgress = (state.CurrentLayer * 100.0) / state.TotalLayers;

                // Add small random variation to laser power
                state.LaserPower = 280 + _random.NextDouble() * 20;

                if (state.CurrentLayer >= state.TotalLayers)
                {
                    state.Phase = BuildPhase.Cooling;
                    state.CoolingProgress = 0;
                }
                break;

            case BuildPhase.Cooling:
                // Cooling takes ~60 minutes
                state.CoolingProgress += 0.028 * elapsed;
                state.BuildPlatformTemp = 180 - (state.CoolingProgress / 100.0) * 140; // 180?40°C

                if (state.CoolingProgress >= 100)
                {
                    state.Phase = BuildPhase.Idle;
                    state.BuildProgress = 0;
                    state.CurrentLayer = 0;
                    state.TotalLayers = 800 + _random.Next(400); // New job config
                }
                break;
        }

        // Random alarm generation (1% chance per minute)
        if (_random.NextDouble() < 0.0002 * elapsed && state.Alarms.Count < 3)
        {
            state.Alarms.Add(GenerateRandomAlarm());
        }

        // Clear old alarms occasionally
        if (_random.NextDouble() < 0.001 * elapsed && state.Alarms.Count > 0)
        {
            state.Alarms.RemoveAt(0);
        }
    }

    private Dictionary<string, object> GenerateTelemetry(EosTestMachineState state)
    {
        var telemetry = new Dictionary<string, object>
        {
            ["machineState"] = (int)state.Phase,
            ["isRunning"] = state.Phase == BuildPhase.Printing,
            ["buildPlatformTemp"] = Math.Round(state.BuildPlatformTemp, 1),
            ["oxygenContent"] = Math.Round(45 + _random.NextDouble() * 10, 1), // 45-55 ppm
            ["argonFlowRate"] = Math.Round(11 + _random.NextDouble() * 3, 1), // 11-14 L/min
            ["argonPressure"] = Math.Round(1.0 + _random.NextDouble() * 0.2, 2), // 1.0-1.2 bar
            ["chamberPressure"] = Math.Round(0.98 + _random.NextDouble() * 0.04, 3), // ~1 bar
            ["filterLifePercent"] = Math.Max(0, 100 - _random.Next(60)) // 40-100%
        };

        if (state.Phase == BuildPhase.Printing)
        {
            telemetry["laserPower"] = Math.Round(state.LaserPower, 1);
            telemetry["scanSpeed"] = 1200 + _random.Next(100);
            telemetry["currentLayer"] = state.CurrentLayer;
            telemetry["totalLayers"] = state.TotalLayers;
            telemetry["buildProgress"] = Math.Round(state.BuildProgress, 2);
            telemetry["layerThickness"] = 30; // 30 microns
        }

        if (state.Phase == BuildPhase.Warming)
        {
            telemetry["warmingProgress"] = Math.Round(state.WarmingProgress, 1);
            telemetry["targetPlatformTemp"] = 180;
        }

        if (state.Phase == BuildPhase.Cooling)
        {
            telemetry["coolingProgress"] = Math.Round(state.CoolingProgress, 1);
            telemetry["targetPlatformTemp"] = 40;
        }

        return telemetry;
    }

    private string GenerateRandomAlarm()
    {
        var alarms = new[]
        {
            "Powder level low - refill recommended",
            "Filter replacement due in 100 hours",
            "Minor gas flow fluctuation detected",
            "Recoater blade wear warning",
            "Chamber humidity elevated",
            "Laser window cleaning recommended"
        };
        return alarms[_random.Next(alarms.Length)];
    }

    private class EosTestMachineState
    {
        public int MachineId { get; init; }
        public BuildPhase Phase { get; set; } = BuildPhase.Idle;
        public double BuildProgress { get; set; }
        public int CurrentLayer { get; set; }
        public int TotalLayers { get; set; } = 1000;
        public double WarmingProgress { get; set; }
        public double CoolingProgress { get; set; }
        public double BuildPlatformTemp { get; set; } = 25;
        public double LaserPower { get; set; } = 285;
        public string JobName { get; set; } = "";
        public List<string> Alarms { get; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        public string StatusString => Phase switch
        {
            BuildPhase.Idle => "Idle",
            BuildPhase.Warming => "Preheating",
            BuildPhase.Printing => "Printing",
            BuildPhase.Cooling => "Cooling",
            _ => "Unknown"
        };

        public double? EstimatedMinutesRemaining => Phase == BuildPhase.Printing
            ? (TotalLayers - CurrentLayer) * 0.33 // ~20 sec per layer
            : null;
    }

    private enum BuildPhase { Idle, Warming, Printing, Cooling }
}
