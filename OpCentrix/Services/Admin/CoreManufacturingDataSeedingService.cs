using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Data;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Lightweight, idempotent manufacturing data seeder.
/// Restores core SLS production entities (materials, machines, one demo part)
/// ONLY when they are missing. Safe to run every startup.
/// </summary>
public class CoreManufacturingDataSeedingService
{
    private readonly SchedulerContext _context;
    private readonly IMaterialService _materialService;
    private readonly IMachineManagementService _machineService;
    private readonly ILogger<CoreManufacturingDataSeedingService> _logger;

    public CoreManufacturingDataSeedingService(
        SchedulerContext context,
        IMaterialService materialService,
        IMachineManagementService machineService,
        ILogger<CoreManufacturingDataSeedingService> logger)
    {
        _context = context;
        _materialService = materialService;
        _machineService = machineService;
        _logger = logger;
    }

    /// <summary>
    /// Ensures core data exists. Adds ONLY what is missing.
    /// </summary>
    public async Task EnsureCoreManufacturingDataAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            // BEFORE anything else, repair schema inconsistencies
            await EnsureSchemaRepairsAsync();

            await EnsureMaterialsAsync();
            await EnsureMachinesAsync();
            await EnsureSamplePartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [CORE-SEED] Unexpected error ensuring core manufacturing data");
        }
    }

    /// <summary>
    /// Runtime schema repair: adds missing Machine.ColorHex column if migrations failed (SQLite only).
    /// EF believes column exists (model snapshot) so migrations won't recreate it; use PRAGMA check.
    /// </summary>
    private async Task EnsureSchemaRepairsAsync()
    {
        try
        {
            var dbConnection = _context.Database.GetDbConnection();
            if (dbConnection.State != ConnectionState.Open)
                await dbConnection.OpenAsync();

            bool hasColorHex = false;
            using (var cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info('Machines');";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var name = reader.GetString(1); // 0=id,1=name
                    if (string.Equals(name, "ColorHex", StringComparison.OrdinalIgnoreCase))
                    {
                        hasColorHex = true;
                        break;
                    }
                }
            }

            if (!hasColorHex)
            {
                _logger.LogWarning("?? [SCHEMA-REPAIR] Machines.ColorHex column missing. Applying runtime patch...");
                using var alter = dbConnection.CreateCommand();
                alter.CommandText = "ALTER TABLE Machines ADD COLUMN ColorHex TEXT NULL;";
                await alter.ExecuteNonQueryAsync();
                _logger.LogInformation("? [SCHEMA-REPAIR] Added Machines.ColorHex column successfully.");
            }
            else
            {
                _logger.LogDebug("[SCHEMA-REPAIR] Machines.ColorHex column present - no action.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SCHEMA-REPAIR] Failed to verify/repair Machines.ColorHex column");
        }
    }

    private async Task EnsureMaterialsAsync()
    {
        if (await _context.Materials.AnyAsync())
        {
            _logger.LogInformation("?? [CORE-SEED] Materials already present ({Count})", await _context.Materials.CountAsync());
            return;
        }

        _logger.LogWarning("?? [CORE-SEED] No materials detected. Seeding default SLS materials...");
        await _materialService.SeedDefaultMaterialsAsync();
    }

    private async Task EnsureMachinesAsync()
    {
        if (await _context.Machines.AnyAsync())
        {
            _logger.LogInformation("?? [CORE-SEED] Machines already present ({Count})", await _context.Machines.CountAsync());
            return;
        }

        _logger.LogWarning("?? [CORE-SEED] No machines detected. Seeding default demo machines...");
        await _machineService.SeedDefaultMachinesAsync();
    }

    private async Task EnsureSamplePartAsync()
    {
        if (await _context.Parts.AnyAsync())
        {
            _logger.LogInformation("?? [CORE-SEED] Parts already present ({Count}) - skipping demo part", await _context.Parts.CountAsync());
            return;
        }

        _logger.LogWarning("?? [CORE-SEED] No parts detected. Creating SLS demo part (modifiable via Admin > Parts)...");

        // Try to map material code to display name for SlsMaterial field
        var tiMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialCode == "TI64-G5")
                         ?? await _context.Materials.FirstOrDefaultAsync();

        var demoPart = new Part
        {
            PartNumber = "SLS-DEMO-001",
            Name = "Demo SLS Titanium Component",
            Description = "Seeded demo part for SLS scheduling. Modify or delete via Admin > Parts.",
            Material = tiMaterial?.MaterialName ?? "Ti-6Al-4V Grade 5",
            SlsMaterial = tiMaterial?.MaterialName ?? "Ti-6Al-4V Grade 5",
            PowderSpecification = "15-45 micron particle size",
            PowderRequirementKg = 0.6,
            RecommendedLaserPower = 200,
            RecommendedScanSpeed = 1200,
            RecommendedLayerThickness = 30,
            RecommendedHatchSpacing = 120,
            RecommendedBuildTemperature = 180,
            RequiredArgonPurity = 99.9,
            MaxOxygenContent = 50,
            Dimensions = "50x40x35 mm",
            VolumeMm3 = 50000,
            HeightMm = 35,
            LengthMm = 50,
            WidthMm = 40,
            SurfaceFinishRequirement = "As-built",
            MaxSurfaceRoughnessRa = 25,
            MaterialCostPerKg = 450m,
            StandardLaborCostPerHour = 85m,
            SetupCost = 150m,
            PostProcessingCost = 75m,
            QualityInspectionCost = 50m,
            MachineOperatingCostPerHour = 125m,
            ArgonCostPerHour = 15m,
            ProcessType = "SLS Metal",
            RequiredMachineType = "TruPrint 3000",
            PreferredMachines = "TI1,TI2",
            SetupTimeMinutes = 45,
            PowderChangeoverTimeMinutes = 30,
            PreheatingTimeMinutes = 60,
            CoolingTimeMinutes = 240,
            PostProcessingTimeMinutes = 45,
            QualityStandards = "ASTM F3001, ISO 17296",
            ToleranceRequirements = "±0.1mm typical, ±0.05mm critical dimensions",
            RequiredSkills = "SLS Operation,Powder Handling,Inert Gas Safety,Post-Processing",
            RequiredCertifications = "SLS Operation Certification,Powder Safety Training",
            RequiredTooling = "Build Platform,Powder Sieve,Support Removal Tools",
            ConsumableMaterials = "Argon Gas,Build Platform Coating",
            SupportStrategy = "Minimal supports on overhangs > 45°",
            CustomerPartNumber = "",
            PartCategory = "Prototype",
            PartClass = "B",
            Industry = "Additive Manufacturing",
            Application = "Demo",
            EstimatedHours = 8.0,
            AvgDuration = "8h 0m",
            AvgDurationDays = 1,
            ManufacturingStage = "Design",
            StageDetails = "{}",
            StageOrder = 1,
            RequiresSLSPrinting = true,
            WorkflowTemplate = "BT_Standard_Workflow",
            ApprovalWorkflow = "Standard",
            ProcessParameters = "{}",
            QualityCheckpoints = "{}",
            BuildFileTemplate = "",
            CadFilePath = "",
            CadFileVersion = "",
            CreatedBy = "System Seed",
            LastModifiedBy = "System Seed",
            AdminOverrideBy = string.Empty
        };

        _context.Parts.Add(demoPart);
        await _context.SaveChangesAsync();

        _logger.LogInformation("? [CORE-SEED] Demo part created (PartNumber={PartNumber})", demoPart.PartNumber);
    }
}
