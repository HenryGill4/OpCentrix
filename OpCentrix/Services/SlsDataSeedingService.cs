using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for seeding the database with realistic SLS printing data
    /// COMMENTED OUT: Legacy seeding service - data will be added via admin pages instead
    /// Keeping for reference and potential future testing scenarios
    /// </summary>
    public class SlsDataSeedingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<SlsDataSeedingService> _logger;

        public SlsDataSeedingService(SchedulerContext context, ILogger<SlsDataSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// DEPRECATED: Use admin pages to add data instead
        /// This method is commented out to prevent automatic seeding
        /// </summary>
        public async Task SeedDatabaseAsync()
        {
            _logger.LogInformation("SlsDataSeedingService.SeedDatabaseAsync() called - SKIPPING: Use admin pages to add data instead");
            
            // COMMENTED OUT: Legacy seeding logic - use admin pages instead
            /*
            try
            {
                _logger.LogInformation("Starting comprehensive database seeding...");

                // Seed core data first
                await SeedSlsMachinesAsync();
                await SeedUsersAsync();
                await SeedPartsAsync();

                // Determine environment
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var seedSampleData = Environment.GetEnvironmentVariable("SEED_SAMPLE_DATA");

                // Seed sample data in development or when explicitly requested
                if (environment == "Development" || seedSampleData == "true")
                {
                    await SeedJobsAsync();
                    // await SeedDepartmentOperationsAsync(); // TEMPORARILY COMMENTED OUT
                }
                else
                {
                    _logger.LogInformation("Production environment - skipping sample data seeding");
                }

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
            */
        }

        /// <summary>
        /// DEPRECATED: Use admin pages to add data instead
        /// </summary>
        public async Task SeedDataAsync()
        {
            _logger.LogInformation("SlsDataSeedingService.SeedDataAsync() called - SKIPPING: Use admin pages to add data instead");
            
            // COMMENTED OUT: Alias for SeedDatabaseAsync to maintain compatibility
            // await SeedDatabaseAsync();
        }

        /// <summary>
        /// DEPRECATED: Machines should be added via /Admin/Machines page
        /// Keeping method signature for compatibility but implementation is disabled
        /// </summary>
        public async Task SeedSlsMachinesAsync()
        {
            _logger.LogInformation("SlsDataSeedingService.SeedSlsMachinesAsync() called - SKIPPING: Use /Admin/Machines page to add machines instead");
            
            // COMMENTED OUT: Legacy machine seeding - use Admin/Machines page instead
            /*
            // Only seed if no machines exist
            if (await _context.SlsMachines.AnyAsync())
                return;

            _logger.LogInformation("Seeding initial SLS machines with dynamic configuration...");

            var machines = new List<SlsMachine>
            {
                new SlsMachine
                {
                    MachineId = "TI1",
                    MachineName = "TruPrint 3000 - Titanium Line 1",
                    MachineModel = "TruPrint 3000",
                    SerialNumber = "TP3000-001",
                    Location = "Building A, Bay 1",
                    SupportedMaterials = "Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23",
                    CurrentMaterial = "Ti-6Al-4V Grade 5",
                    Status = "Idle",
                    IsActive = true,
                    IsAvailableForScheduling = true,
                    Priority = 1,
                    OpcUaEndpointUrl = "opc.tcp://192.168.1.101:4840",
                    OpcUaEnabled = false, // Start with manual mode
                    BuildLengthMm = 250,
                    BuildWidthMm = 250,
                    BuildHeightMm = 300,
                    MaxLaserPowerWatts = 400,
                    MaxScanSpeedMmPerSec = 7000,
                    MinLayerThicknessMicrons = 20,
                    MaxLayerThicknessMicrons = 60,
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                new SlsMachine
                {
                    MachineId = "TI2",
                    MachineName = "TruPrint 3000 - Titanium Line 2",
                    MachineModel = "TruPrint 3000",
                    SerialNumber = "TP3000-002",
                    Location = "Building A, Bay 2",
                    SupportedMaterials = "Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23",
                    CurrentMaterial = "Ti-6Al-4V ELI Grade 23",
                    Status = "Idle",
                    IsActive = true,
                    IsAvailableForScheduling = true,
                    Priority = 1,
                    OpcUaEndpointUrl = "opc.tcp://192.168.1.102:4840",
                    OpcUaEnabled = false,
                    BuildLengthMm = 250,
                    BuildWidthMm = 250,
                    BuildHeightMm = 300,
                    MaxLaserPowerWatts = 400,
                    MaxScanSpeedMmPerSec = 7000,
                    MinLayerThicknessMicrons = 20,
                    MaxLayerThicknessMicrons = 60,
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                },
                new SlsMachine
                {
                    MachineId = "INC",
                    MachineName = "TruPrint 3000 - Inconel Line",
                    MachineModel = "TruPrint 3000",
                    SerialNumber = "TP3000-003",
                    Location = "Building A, Bay 3",
                    SupportedMaterials = "Inconel 718,Inconel 625",
                    CurrentMaterial = "Inconel 718",
                    Status = "Idle",
                    IsActive = true,
                    IsAvailableForScheduling = true,
                    Priority = 2,
                    OpcUaEndpointUrl = "opc.tcp://192.168.1.103:4840",
                    OpcUaEnabled = false,
                    BuildLengthMm = 250,
                    BuildWidthMm = 250,
                    BuildHeightMm = 300,
                    MaxLaserPowerWatts = 400,
                    MaxScanSpeedMmPerSec = 7000,
                    MinLayerThicknessMicrons = 20,
                    MaxLayerThicknessMicrons = 60,
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                }
            };

            await _context.SlsMachines.AddRangeAsync(machines);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} SLS machines with basic configuration", machines.Count);

            // Seed basic machine capabilities for each machine
            await SeedMachineCapabilitiesAsync();
            */
        }

        /// <summary>
        /// DEPRECATED: Machine capabilities should be added via /Admin/Machines page
        /// </summary>
        private async Task SeedMachineCapabilitiesAsync()
        {
            _logger.LogInformation("SeedMachineCapabilitiesAsync() called - SKIPPING: Use /Admin/Machines page to add capabilities instead");
            
            // COMMENTED OUT: Legacy capability seeding - use Admin/Machines page instead
            /*
            // Only seed if no capabilities exist
            if (await _context.MachineCapabilities.AnyAsync())
                return;

            _logger.LogInformation("Seeding machine capabilities...");

            var machines = await _context.SlsMachines.ToListAsync();
            var capabilities = new List<MachineCapability>();

            foreach (var machine in machines)
            {
                // Common capabilities for all machines
                capabilities.AddRange(new[]
                {
                    new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Process Parameter",
                        CapabilityName = "Laser Power Control",
                        CapabilityValue = "Laser power control range for printing",
                        IsAvailable = true,
                        Priority = 1,
                        MinValue = 100,
                        MaxValue = machine.MaxLaserPowerWatts,
                        Unit = "W",
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    },
                    new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Process Parameter",
                        CapabilityName = "Scan Speed Control",
                        CapabilityValue = "Scan speed control for laser",
                        IsAvailable = true,
                        Priority = 1,
                        MinValue = 500,
                        MaxValue = machine.MaxScanSpeedMmPerSec,
                        Unit = "mm/s",
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    },
                    new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Process Parameter",
                        CapabilityName = "Layer Thickness Control",
                        CapabilityValue = "Layer thickness control for printing",
                        IsAvailable = true,
                        Priority = 1,
                        MinValue = machine.MinLayerThicknessMicrons,
                        MaxValue = machine.MaxLayerThicknessMicrons,
                        Unit = "µm",
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    },
                    new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Process Parameter",
                        CapabilityName = "Build Temperature Control",
                        CapabilityValue = "Build platform temperature control",
                        IsAvailable = true,
                        Priority = 1,
                        MinValue = 150,
                        MaxValue = 250,
                        Unit = "°C",
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    },
                    new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Quality Metric",
                        CapabilityName = "Oxygen Control",
                        CapabilityValue = "Oxygen level monitoring and control",
                        IsAvailable = true,
                        Priority = 1,
                        MinValue = 0,
                        MaxValue = 100,
                        Unit = "ppm",
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    }
                });

                // Material-specific capabilities
                if (machine.SupportedMaterials.Contains("Ti-6Al-4V"))
                {
                    capabilities.Add(new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Material Property",
                        CapabilityName = "Titanium Processing",
                        CapabilityValue = "Capability to process titanium alloys",
                        IsAvailable = true,
                        Priority = 1,
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    });
                }

                if (machine.SupportedMaterials.Contains("Inconel"))
                {
                    capabilities.Add(new MachineCapability
                    {
                        MachineId = machine.Id, // FIXED: Use machine.Id (int) not machine.MachineId (string)
                        CapabilityType = "Material Property",
                        CapabilityName = "Inconel Processing",
                        CapabilityValue = "Capability to process Inconel super alloys",
                        IsAvailable = true,
                        Priority = 1,
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow
                    });
                }
            }

            await _context.MachineCapabilities.AddRangeAsync(capabilities);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} machine capabilities", capabilities.Count);
            */
        }

        /// <summary>
        /// DEPRECATED: Parts should be added via /Admin/Parts page
        /// </summary>
        public async Task SeedPartsAsync()
        {
            _logger.LogInformation("SlsDataSeedingService.SeedPartsAsync() called - SKIPPING: Use /Admin/Parts page to add parts instead");
            
            // COMMENTED OUT: Legacy parts seeding - use Admin/Parts page instead
            /*
            var parts = new List<Part>
            {
                // Aerospace components
                new Part
                {
                    PartNumber = "14-5396",
                    Description = "Aerospace turbine blade for jet engines",
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    EstimatedHours = 12.5,
                    WeightGrams = 245.8,
                    LengthMm = 85.4,
                    WidthMm = 32.7,
                    HeightMm = 156.3,
                    VolumeMm3 = 87500,
                    PowderRequirementKg = 2.1,
                    RecommendedLaserPower = 210,
                    RecommendedScanSpeed = 1150,
                    RecommendedBuildTemperature = 180,
                    Industry = "Aerospace",
                    Application = "Turbine Blade",
                    PartCategory = "Production",
                    PartClass = "A",
                    RequiresFDA = false,
                    RequiresAS9100 = true,
                    RequiresNADCAP = true,
                    CreatedBy = "Engineer",
                    LastModifiedBy = "Engineer"
                },
                new Part
                {
                    PartNumber = "14-5397",
                    Description = "Lightweight bracket with complex geometry for aerospace applications",
                    Material = "Ti-6Al-4V ELI Grade 23",
                    SlsMaterial = "Ti-6Al-4V ELI Grade 23",
                    EstimatedHours = 8.75,
                    WeightGrams = 156.2,
                    LengthMm = 124.5,
                    WidthMm = 78.3,
                    HeightMm = 42.1,
                    VolumeMm3 = 65400,
                    PowderRequirementKg = 1.5,
                    RecommendedLaserPower = 195,
                    RecommendedScanSpeed = 1280,
                    RecommendedBuildTemperature = 175,
                    Industry = "Aerospace",
                    Application = "Structural Bracket",
                    PartCategory = "Production",
                    PartClass = "A",
                    RequiresSupports = true,
                    SupportRemovalTimeMinutes = 45,
                    RequiresAS9100 = true,
                    CreatedBy = "Engineer",
                    LastModifiedBy = "Engineer"
                },
                new Part
                {
                    PartNumber = "14-5398",
                    Description = "Hip cup prototype for femoral head replacement",
                    Material = "Ti-6Al-4V ELI Grade 23",
                    SlsMaterial = "Ti-6Al-4V ELI Grade 23",
                    EstimatedHours = 6.25,
                    WeightGrams = 89.5,
                    LengthMm = 65.0,
                    WidthMm = 65.0,
                    HeightMm = 35.2,
                    VolumeMm3 = 42800,
                    PowderRequirementKg = 1.0,
                    RecommendedLaserPower = 185,
                    RecommendedScanSpeed = 1350,
                    RecommendedBuildTemperature = 170,
                    MaxSurfaceRoughnessRa = 15,
                    Industry = "Medical",
                    Application = "Hip Implant",
                    PartCategory = "Prototype",
                    PartClass = "A",
                    RequiresFDA = true,
                    RequiresCertification = true,
                    ToleranceRequirements = "±0.05mm on all critical dimensions",
                    QualityStandards = "ISO 13485, ASTM F3001",
                    CreatedBy = "Biomedical Engineer",
                    LastModifiedBy = "Quality Engineer"
                },
                new Part
                {
                    PartNumber = "14-5399",
                    Description = "Heat exchanger component for high-temperature applications",
                    Material = "Inconel 718",
                    SlsMaterial = "Inconel 718",
                    EstimatedHours = 15.5,
                    WeightGrams = 387.2,
                    LengthMm = 156.8,
                    WidthMm = 89.4,
                    HeightMm = 124.7,
                    VolumeMm3 = 145600,
                    PowderRequirementKg = 3.2,
                    RecommendedLaserPower = 285,
                    RecommendedScanSpeed = 960,
                    RecommendedBuildTemperature = 200,
                    RequiredArgonPurity = 99.95,
                    MaxOxygenContent = 25,
                    Industry = "Aerospace",
                    Application = "Heat Exchanger",
                    PartCategory = "Production",
                    PartClass = "A",
                    RequiresSupports = true,
                    SupportRemovalTimeMinutes = 90,
                    PostProcessingTimeMinutes = 120,
                    CreatedBy = "Engineer",
                    LastModifiedBy = "Engineer"
                },
                new Part
                {
                    PartNumber = "14-5400",
                    Description = "Lightweight bracket for automotive prototype",
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    EstimatedHours = 4.75,
                    WeightGrams = 67.3,
                    LengthMm = 98.2,
                    WidthMm = 56.7,
                    HeightMm = 28.9,
                    VolumeMm3 = 28500,
                    PowderRequirementKg = 0.8,
                    RecommendedLaserPower = 200,
                    RecommendedScanSpeed = 1200,
                    RecommendedBuildTemperature = 180,
                    Industry = "Automotive",
                    Application = "Suspension Component",
                    PartCategory = "Prototype",
                    PartClass = "B",
                    CreatedBy = "Design Engineer",
                    LastModifiedBy = "Design Engineer"
                },
                new Part
                {
                    PartNumber = "14-5401",
                    Description = "Valve body for high-performance engines",
                    Material = "Inconel 625",
                    SlsMaterial = "Inconel 625",
                    EstimatedHours = 18.25,
                    WeightGrams = 456.8,
                    LengthMm = 134.6,
                    WidthMm = 98.3,
                    HeightMm = 145.2,
                    VolumeMm3 = 187400,
                    PowderRequirementKg = 4.1,
                    RecommendedLaserPower = 275,
                    RecommendedScanSpeed = 980,
                    RecommendedBuildTemperature = 195,
                    RequiredArgonPurity = 99.9,
                    MaxOxygenContent = 30,
                    Industry = "Oil & Gas",
                    Application = "Valve Body",
                    PartCategory = "Production",
                    PartClass = "A",
                    RequiresSupports = true,
                    SupportRemovalTimeMinutes = 75,
                    PostProcessingTimeMinutes = 150,
                    CreatedBy = "Engineer",
                    LastModifiedBy = "Engineer"
                },
                new Part
                {
                    PartNumber = "14-5402",
                    Description = "Drone frame component for lightweight and strength",
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    EstimatedHours = 3.5,
                    WeightGrams = 34.2,
                    LengthMm = 156.4,
                    WidthMm = 23.8,
                    HeightMm = 12.5,
                    VolumeMm3 = 18600,
                    PowderRequirementKg = 0.4,
                    RecommendedLaserPower = 190,
                    RecommendedScanSpeed = 1400,
                    RecommendedBuildTemperature = 175,
                    Industry = "Aerospace",
                    Application = "UAV Frame",
                    PartCategory = "Prototype",
                    PartClass = "B",
                    CreatedBy = "Aerospace Engineer",
                    LastModifiedBy = "Aerospace Engineer"
                },
                new Part
                {
                    PartNumber = "14-5403",
                    Description = "Custom surgical tool for minimally invasive procedures",
                    Material = "Ti-6Al-4V ELI Grade 23",
                    SlsMaterial = "Ti-6Al-4V ELI Grade 23",
                    EstimatedHours = 2.75,
                    WeightGrams = 28.7,
                    LengthMm = 187.3,
                    WidthMm = 15.2,
                    HeightMm = 8.4,
                    VolumeMm3 = 12400,
                    PowderRequirementKg = 0.3,
                    RecommendedLaserPower = 180,
                    RecommendedScanSpeed = 1450,
                    RecommendedBuildTemperature = 165,
                    MaxSurfaceRoughnessRa = 8,
                    Industry = "Medical",
                    Application = "Surgical Instrument",
                    PartCategory = "Production",
                    PartClass = "A",
                    RequiresFDA = true,
                    RequiresCertification = true,
                    PostProcessingTimeMinutes = 60,
                    CreatedBy = "Medical Device Engineer",
                    LastModifiedBy = "Quality Engineer"
                }
            };

            await _context.Parts.AddRangeAsync(parts);
            _logger.LogInformation("Seeded {Count} SLS parts", parts.Count);
            */
        }

        /// <summary>
        /// DEPRECATED: Users should be added via /Admin/Users page
        /// </summary>
        private async Task SeedUsersAsync()
        {
            _logger.LogInformation("SeedUsersAsync() called - SKIPPING: Use /Admin/Users page to add users instead");
            
            // COMMENTED OUT: Legacy user seeding - use Admin/Users page instead
            /*
            // Only seed if no users exist
            if (await _context.Users.AnyAsync())
                return;

            // Use a factory pattern to create the authentication service with proper dependencies
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var authLogger = loggerFactory.CreateLogger<AuthenticationService>();
            var authService = new AuthenticationService(_context, authLogger);

            var testUsers = new[]
            {
                new { Username = "admin", Password = "admin123", FullName = "System Administrator", Email = "admin@opcentrix.com", Role = "Admin", Department = "IT" },
                new { Username = "manager", Password = "manager123", FullName = "Production Manager", Email = "manager@opcentrix.com", Role = "Manager", Department = "Production" },
                new { Username = "scheduler", Password = "scheduler123", FullName = "Production Scheduler", Email = "scheduler@opcentrix.com", Role = "Scheduler", Department = "Production" },
                new { Username = "operator", Password = "operator123", FullName = "Machine Operator", Email = "operator@opcentrix.com", Role = "Operator", Department = "Production" },
                new { Username = "printer", Password = "printer123", FullName = "3D Printer Operator", Email = "printer@opcentrix.com", Role = "PrintingSpecialist", Department = "3D Printing" },
                new { Username = "coating", Password = "coating123", FullName = "Coating Specialist", Email = "coating@opcentrix.com", Role = "CoatingSpecialist", Department = "Coating" },
                new { Username = "shipping", Password = "shipping123", FullName = "Shipping Specialist", Email = "shipping@opcentrix.com", Role = "ShippingSpecialist", Department = "Shipping" },
                new { Username = "edm", Password = "edm123", FullName = "EDM Specialist", Email = "edm@opcentrix.com", Role = "EDMSpecialist", Department = "EDM" },
                new { Username = "machining", Password = "machining123", FullName = "Machining Specialist", Email = "machining@opcentrix.com", Role = "MachiningSpecialist", Department = "Machining" },
                new { Username = "qc", Password = "qc123", FullName = "Quality Control Specialist", Email = "qc@opcentrix.com", Role = "QCSpecialist", Department = "Quality" },
                new { Username = "media", Password = "media123", FullName = "Media Specialist", Email = "media@opcentrix.com", Role = "MediaSpecialist", Department = "Media" },
                new { Username = "analyst", Password = "analyst123", FullName = "Data Analyst", Email = "analyst@opcentrix.com", Role = "Analyst", Department = "Analytics" }
            };

            var users = new List<User>();

            foreach (var userData in testUsers)
            {
                var user = new User
                {
                    Username = userData.Username,
                    FullName = userData.FullName,
                    Email = userData.Email,
                    PasswordHash = authService.HashPassword(userData.Password), // Use proper password hashing
                    Role = userData.Role,
                    Department = userData.Department,
                    IsActive = true,
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                users.Add(user);
            }

            await _context.Users.AddRangeAsync(users);
            _logger.LogInformation("Seeded {Count} test users with proper password hashing", users.Count);
            */
        }

        /// <summary>
        /// DEPRECATED: Jobs should be created via the Scheduler interface
        /// </summary>
        public async Task SeedJobsAsync()
        {
            _logger.LogInformation("SlsDataSeedingService.SeedJobsAsync() called - SKIPPING: Use Scheduler interface to create jobs instead");
            
            // COMMENTED OUT: Legacy job seeding - use Scheduler interface instead
            /*
            var parts = await _context.Parts.ToListAsync();
            if (!parts.Any())
                return;

            // Get machines dynamically from database instead of hardcoded array
            var machines = await _context.SlsMachines
                .Where(m => m.IsActive && m.IsAvailableForScheduling)
                .Select(m => m.MachineId)
                .ToListAsync();

            if (!machines.Any())
            {
                _logger.LogWarning("No active machines available for job seeding");
                return;
            }

            var jobs = new List<Job>();
            var random = new Random(42); // Fixed seed for reproducible data

            // Create jobs for the next 14 days
            var startDate = DateTime.Today;

            for (int day = 0; day < 14; day++)
            {
                var currentDate = startDate.AddDays(day);

                foreach (var machineId in machines)
                {
                    // Skip weekends occasionally
                    if ((currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                        && random.Next(0, 3) == 0)
                        continue;

                    // Generate 1-3 jobs per machine per day
                    var jobCount = random.Next(1, 4);
                    var currentTime = currentDate.AddHours(7); // Start at 7 AM

                    for (int j = 0; j < jobCount; j++)
                    {
                        // Get machine details for material compatibility
                        var machine = await _context.SlsMachines
                            .FirstOrDefaultAsync(m => m.MachineId == machineId);

                        if (machine == null) continue;

                        // Select appropriate parts for machine based on supported materials
                        var availableParts = parts.Where(p => 
                            machine.SupportedMaterials.Contains(p.SlsMaterial, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (!availableParts.Any())
                            continue;

                        var selectedPart = availableParts[random.Next(availableParts.Count)];
                        var duration = TimeSpan.FromHours(selectedPart.EstimatedHours);
                        var scheduledEnd = currentTime.Add(duration);

                        // Ensure job doesn't extend too late (before 6 PM)
                        if (scheduledEnd.Hour > 18)
                        {
                            scheduledEnd = currentDate.AddHours(18);
                            duration = scheduledEnd - currentTime;
                        }

                        var job = new Job
                        {
                            MachineId = machineId,
                            PartId = selectedPart.Id,
                            PartNumber = selectedPart.PartNumber,
                            ScheduledStart = currentTime,
                            ScheduledEnd = scheduledEnd,
                            EstimatedHours = duration.TotalHours,
                            Quantity = random.Next(1, 6),
                            Status = DetermineJobStatus(currentTime, scheduledEnd),
                            Priority = random.Next(1, 6),
                            SlsMaterial = selectedPart.SlsMaterial, // FIXED: Use SlsMaterial (correct property name)
                            LaserPowerWatts = selectedPart.RecommendedLaserPower + random.Next(-20, 21),
                            ScanSpeedMmPerSec = selectedPart.RecommendedScanSpeed + random.Next(-100, 101),
                            LayerThicknessMicrons = 30 + random.Next(-5, 6),
                            HatchSpacingMicrons = 120 + random.Next(-20, 21),
                            BuildTemperatureCelsius = selectedPart.RecommendedBuildTemperature + random.Next(-10, 11),
                            ArgonPurityPercent = selectedPart.RequiredArgonPurity,
                            OxygenContentPpm = selectedPart.MaxOxygenContent + random.Next(-10, 11),
                            EstimatedPowderUsageKg = selectedPart.PowderRequirementKg,
                            PowderRecyclePercentage = 85 + random.Next(-10, 11),
                            PreheatingTimeMinutes = 60 + random.Next(-15, 16),
                            CoolingTimeMinutes = 240 + random.Next(-60, 61),
                            PostProcessingTimeMinutes = selectedPart.PostProcessingTimeMinutes + random.Next(-15, 16),
                            BuildLayerNumber = random.Next(1, 6),
                            CustomerOrderNumber = $"CO-{random.Next(1000, 9999)}",
                            CustomerDueDate = currentDate.AddDays(random.Next(1, 30)),
                            IsRushJob = random.Next(0, 10) < 2, // 20% chance of rush job
                            Operator = "John Smith",
                            CreatedBy = "Scheduler",
                            LastModifiedBy = "Scheduler",
                            RequiresArgonPurge = true,
                            RequiresPreheating = true,
                            RequiresPowderSieving = true,
                            RequiresPostProcessing = selectedPart.PostProcessingTimeMinutes > 0
                        };

                        // Set actual times for completed jobs
                        if (job.Status == "Completed" && currentTime < DateTime.Now)
                        {
                            job.ActualStart = job.ScheduledStart.AddMinutes(random.Next(-15, 31));
                            job.ActualEnd = job.ActualStart.Value.Add(duration).AddMinutes(random.Next(-30, 61));
                            job.ActualPowderUsageKg = job.EstimatedPowderUsageKg * (0.9 + random.NextDouble() * 0.2);
                            job.ProducedQuantity = job.Quantity;
                            job.DefectQuantity = random.Next(0, 3) == 0 ? 1 : 0; // Occasional defect
                        }

                        jobs.Add(job);

                        // Move to next time slot with some gap
                        currentTime = scheduledEnd.AddMinutes(random.Next(30, 121));
                    }
                }
            }

            await _context.Jobs.AddRangeAsync(jobs);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Seeded {Count} SLS jobs across {MachineCount} machines", 
                jobs.Count, machines.Count);
            */
        }

        /// <summary>
        /// DEPRECATED: Department operations would be managed via their respective admin pages
        /// </summary>
        private async Task SeedDepartmentOperationsAsync()
        {
            _logger.LogInformation("SeedDepartmentOperationsAsync() called - SKIPPING: Use respective admin pages for department operations");
            
            // COMMENTED OUT: Legacy department operations seeding
            /*
            try
            {
                _logger.LogInformation("Seeding department operations data...");

                // TODO: Uncomment when department operation tables are implemented
                // await SeedCoatingOperationsAsync();
                // await SeedEDMOperationsAsync();
                // await SeedMachiningOperationsAsync();
                // await SeedQualityInspectionsAsync();
                // await SeedShippingOperationsAsync();

                _logger.LogInformation("Department operations data seeding skipped (tables not yet implemented)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding department operations");
                throw;
            }
            */
        }

        // All the helper methods below are also commented out for consistency

        private async Task SeedCoatingOperationsAsync()
        {
            _logger.LogInformation("SeedCoatingOperationsAsync() called - SKIPPING");
            // COMMENTED OUT: Use coating admin pages instead
        }

        private async Task SeedEDMOperationsAsync()
        {
            _logger.LogInformation("SeedEDMOperationsAsync() called - SKIPPING");
            // COMMENTED OUT: Use EDM admin pages instead
        }

        private async Task SeedMachiningOperationsAsync()
        {
            _logger.LogInformation("SeedMachiningOperationsAsync() called - SKIPPING");
            // COMMENTED OUT: Use machining admin pages instead
        }

        private async Task SeedQualityInspectionsAsync()
        {
            _logger.LogInformation("SeedQualityInspectionsAsync() called - SKIPPING");
            // COMMENTED OUT: Use quality admin pages instead
        }

        private async Task SeedShippingOperationsAsync()
        {
            _logger.LogInformation("SeedShippingOperationsAsync() called - SKIPPING");
            // COMMENTED OUT: Use shipping admin pages instead
        }

        private string DetermineJobStatus(DateTime scheduledStart, DateTime scheduledEnd)
        {
            // Helper method kept for potential future use
            var now = DateTime.Now;

            if (scheduledEnd < now)
                return "Completed";
            else if (scheduledStart <= now && now < scheduledEnd)
                return new[] { "Building", "Cooling", "Post-Processing" }[new Random().Next(3)];
            else if (scheduledStart > now)
                return "Scheduled";
            else
                return "On-Hold";
        }

        // All the remaining helper methods are preserved but unused
        // They can be used for reference or future testing scenarios
    }
}