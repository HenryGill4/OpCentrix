using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix
{
    /// <summary>
    /// Comprehensive database seeding for OpCentrix Parts system
    /// Creates sample data that demonstrates all features including B&T manufacturing compliance
    /// </summary>
    public static class SeedDatabase
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var partStageService = scope.ServiceProvider.GetService<IPartStageService>();
            
            try
            {
                await context.Database.EnsureCreatedAsync();
                
                // Check if data already exists
                if (await context.Parts.AnyAsync())
                {
                    Console.WriteLine("?? [SEED] Database already contains data, skipping seed");
                    return;
                }
                
                Console.WriteLine("?? [SEED] Starting comprehensive database seeding...");
                
                // Create production stages first
                await SeedProductionStagesAsync(context);
                
                // Create sample parts with all field types
                await SeedPartsAsync(context);
                
                // Create machines and capabilities
                await SeedMachinesAsync(context);
                
                // Create users for testing
                await SeedUsersAsync(context);
                
                // Create part-stage relationships
                if (partStageService != null)
                {
                    await SeedPartStageRequirementsAsync(context, partStageService);
                }
                
                // Create some sample jobs
                await SeedJobsAsync(context);
                
                await context.SaveChangesAsync();
                
                Console.WriteLine("? [SEED] Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? [SEED] Error seeding database: {ex.Message}");
                throw;
            }
        }
        
        private static async Task SeedProductionStagesAsync(SchedulerContext context)
        {
            Console.WriteLine("?? [SEED] Creating production stages...");
            
            var stages = new List<ProductionStage>
            {
                new ProductionStage
                {
                    Name = "SLS Printing",
                    Description = "Selective Laser Sintering metal printing process",
                    DefaultDurationHours = 8.0,
                    DefaultHourlyRate = 125.00m,
                    StageColor = "#007bff",
                    StageIcon = "fas fa-print",
                    DisplayOrder = 1,
                    Department = "3D Printing",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new ProductionStage
                {
                    Name = "Heat Treatment",
                    Description = "Post-print stress relief and material conditioning",
                    DefaultDurationHours = 4.0,
                    DefaultHourlyRate = 85.00m,
                    StageColor = "#dc3545",
                    StageIcon = "fas fa-fire",
                    DisplayOrder = 2,
                    Department = "Heat Treatment",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new ProductionStage
                {
                    Name = "CNC Machining",
                    Description = "Precision machining for critical dimensions and features",
                    DefaultDurationHours = 6.0,
                    DefaultHourlyRate = 95.00m,
                    StageColor = "#28a745",
                    StageIcon = "fas fa-cogs",
                    DisplayOrder = 3,
                    Department = "Machining",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new ProductionStage
                {
                    Name = "EDM Operations",
                    Description = "Electrical Discharge Machining for complex geometries",
                    DefaultDurationHours = 12.0,
                    DefaultHourlyRate = 110.00m,
                    StageColor = "#ffc107",
                    StageIcon = "fas fa-bolt",
                    DisplayOrder = 4,
                    Department = "EDM",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new ProductionStage
                {
                    Name = "Assembly",
                    Description = "Component assembly and integration",
                    DefaultDurationHours = 3.0,
                    DefaultHourlyRate = 75.00m,
                    StageColor = "#17a2b8",
                    StageIcon = "fas fa-puzzle-piece",
                    DisplayOrder = 5,
                    Department = "Assembly",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new ProductionStage
                {
                    Name = "Finishing",
                    Description = "Surface treatment, coating, and final finishing",
                    DefaultDurationHours = 4.0,
                    DefaultHourlyRate = 65.00m,
                    StageColor = "#6c757d",
                    StageIcon = "fas fa-brush",
                    DisplayOrder = 6,
                    Department = "Finishing",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new ProductionStage
                {
                    Name = "Quality Inspection",
                    Description = "Dimensional verification and quality testing",
                    DefaultDurationHours = 2.0,
                    DefaultHourlyRate = 85.00m,
                    StageColor = "#6f42c1",
                    StageIcon = "fas fa-search",
                    DisplayOrder = 7,
                    Department = "Quality",
                    IsActive = true,
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                }
            };
            
            context.ProductionStages.AddRange(stages);
            await context.SaveChangesAsync();
            Console.WriteLine($"? [SEED] Created {stages.Count} production stages");
        }
        
        private static async Task SeedPartsAsync(SchedulerContext context)
        {
            Console.WriteLine("?? [SEED] Creating sample parts...");
            
            var parts = new List<Part>
            {
                // B&T Suppressor Components
                new Part
                {
                    PartNumber = "BT-SUP-001",
                    Name = "B&T Suppressor Baffle Stack",
                    Description = "High-efficiency titanium baffle stack for B&T suppressors, optimized for sound reduction and back pressure management. Features advanced internal geometry.",
                    Industry = "Firearms",
                    Application = "B&T Manufacturing",
                    Material = "Ti-6Al-4V Grade 5",
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    EstimatedHours = 12.0,
                    MaterialCostPerKg = 450.00m,
                    StandardLaborCostPerHour = 85.00m,
                    PartCategory = "Production",
                    PartClass = "A",
                    ProcessType = "SLS Metal",
                    RequiredMachineType = "TruPrint 3000",
                    IsActive = true,
                    RequiresSLSPrinting = true,
                    RequiresInspection = true,
                    
                    // B&T Manufacturing stages
                    ManufacturingStage = "SLS-Primary",
                    StageDetails = "{\"baffle_count\": 8, \"chamber_volume\": \"optimized\"}",
                    StageOrder = 1,
                    RequiresCNCMachining = true,
                    RequiresEDMOperations = true,
                    RequiresFinishing = true,
                    
                    // B&T Classification
                    BTComponentType = "Suppressor",
                    BTFirearmCategory = "NFA_Item",
                    BTSuppressorType = "Baffle",
                    BTCaliberCompatibility = ".223, .308, 5.56",
                    BTThreadPitch = "5/8-24",
                    
                    // Regulatory Compliance
                    RequiresATFForm1 = true,
                    RequiresTaxStamp = true,
                    TaxStampAmount = 200.00m,
                    ATFClassification = "Silencer",
                    RequiresExportLicense = true,
                    ITARCategory = "Category I",
                    
                    // B&T Testing
                    RequiresBTProofTesting = true,
                    RequiresSoundTesting = true,
                    RequiresBackPressureTesting = true,
                    ProofTestPressure = 75000,
                    
                    // Physical Properties
                    LengthMm = 180.0,
                    WidthMm = 35.0,
                    HeightMm = 35.0,
                    WeightGrams = 285.0,
                    VolumeMm3 = 220500,
                    Dimensions = "180 × 35 × 35 mm",
                    MaxSurfaceRoughnessRa = 12.5,
                    
                    // Cost and Compliance
                    BTLicensingCost = 150.00m,
                    ComplianceCost = 75.00m,
                    TestingCost = 200.00m,
                    DocumentationCost = 50.00m,
                    
                    CustomerPartNumber = "BT-INT-SUP-001",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = "Seeder",
                    LastModifiedDate = DateTime.UtcNow
                },
                
                // Aerospace Component
                new Part
                {
                    PartNumber = "AERO-001",
                    Name = "Aerospace Turbine Blade",
                    Description = "High-temperature turbine blade component for aerospace applications. Requires extreme precision and Inconel 718 material for temperature resistance.",
                    Industry = "Aerospace",
                    Application = "Structural Component",
                    Material = "Inconel 718",
                    SlsMaterial = "Inconel 718",
                    EstimatedHours = 16.0,
                    MaterialCostPerKg = 750.00m,
                    StandardLaborCostPerHour = 95.00m,
                    PartCategory = "Production",
                    PartClass = "A",
                    ProcessType = "SLS Metal",
                    RequiredMachineType = "TruPrint 3000",
                    IsActive = true,
                    RequiresSLSPrinting = true,
                    RequiresInspection = true,
                    
                    ManufacturingStage = "Design",
                    StageDetails = "{\"temperature_rating\": \"1200C\", \"stress_analysis\": \"completed\"}",
                    StageOrder = 1,
                    RequiresCNCMachining = true,
                    RequiresFinishing = true,
                    
                    BTComponentType = "General",
                    BTFirearmCategory = "Component",
                    
                    RequiresAS9100 = true,
                    RequiresNADCAP = true,
                    
                    LengthMm = 85.0,
                    WidthMm = 25.0,
                    HeightMm = 125.0,
                    WeightGrams = 165.0,
                    VolumeMm3 = 265625,
                    Dimensions = "85 × 25 × 125 mm",
                    MaxSurfaceRoughnessRa = 6.3,
                    
                    AdminEstimatedHoursOverride = 18.0,
                    AdminOverrideReason = "Additional finishing required for aerospace standards",
                    AdminOverrideBy = "QualityManager",
                    AdminOverrideDate = DateTime.UtcNow,
                    
                    CustomerPartNumber = "AERO-TRB-001",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = "Seeder",
                    LastModifiedDate = DateTime.UtcNow
                },
                
                // Medical Device Component
                new Part
                {
                    PartNumber = "MED-001",
                    Name = "Medical Implant Hip Joint",
                    Description = "Biocompatible titanium hip joint implant component. Requires FDA compliance and medical-grade surface finish. ELI grade titanium for biocompatibility.",
                    Industry = "Medical",
                    Application = "General Component",
                    Material = "Ti-6Al-4V ELI Grade 23",
                    SlsMaterial = "Ti-6Al-4V ELI Grade 23",
                    EstimatedHours = 10.0,
                    MaterialCostPerKg = 520.00m,
                    StandardLaborCostPerHour = 95.00m,
                    PartCategory = "Production",
                    PartClass = "A",
                    ProcessType = "SLS Metal",
                    RequiredMachineType = "TruPrint 3000",
                    IsActive = true,
                    RequiresSLSPrinting = true,
                    RequiresInspection = true,
                    
                    ManufacturingStage = "SLS-Primary",
                    StageDetails = "{\"biocompatibility\": \"ISO 10993\", \"surface_requirements\": \"medical_grade\"}",
                    StageOrder = 1,
                    RequiresCNCMachining = true,
                    RequiresFinishing = true,
                    
                    BTComponentType = "General",
                    BTFirearmCategory = "Component",
                    
                    RequiresFDA = true,
                    RequiresCertification = true,
                    
                    LengthMm = 45.0,
                    WidthMm = 45.0,
                    HeightMm = 65.0,
                    WeightGrams = 125.0,
                    VolumeMm3 = 131625,
                    Dimensions = "45 × 45 × 65 mm",
                    MaxSurfaceRoughnessRa = 3.2,
                    
                    CustomerPartNumber = "MED-HIP-001",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = "Seeder",
                    LastModifiedDate = DateTime.UtcNow
                },
                
                // Simple Prototype Part
                new Part
                {
                    PartNumber = "PROTO-001",
                    Name = "Prototype Bracket",
                    Description = "Simple prototype bracket for concept validation. Aluminum material for lightweight testing.",
                    Industry = "General Manufacturing",
                    Application = "Prototype Development",
                    Material = "AlSi10Mg",
                    SlsMaterial = "AlSi10Mg",
                    EstimatedHours = 4.0,
                    MaterialCostPerKg = 180.00m,
                    StandardLaborCostPerHour = 70.00m,
                    PartCategory = "Prototype",
                    PartClass = "C",
                    ProcessType = "SLS Metal",
                    RequiredMachineType = "TruPrint 3000",
                    IsActive = true,
                    RequiresSLSPrinting = true,
                    RequiresInspection = false,
                    
                    ManufacturingStage = "Design",
                    StageDetails = "{\"prototype_iteration\": 1, \"test_objectives\": \"fit_and_function\"}",
                    StageOrder = 1,
                    
                    BTComponentType = "General",
                    BTFirearmCategory = "Component",
                    
                    LengthMm = 50.0,
                    WidthMm = 25.0,
                    HeightMm = 15.0,
                    WeightGrams = 35.0,
                    VolumeMm3 = 18750,
                    Dimensions = "50 × 25 × 15 mm",
                    MaxSurfaceRoughnessRa = 25.0,
                    
                    CustomerPartNumber = "PROTO-BRK-001",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = "Seeder",
                    LastModifiedDate = DateTime.UtcNow
                },
                
                // Complex Assembly Component
                new Part
                {
                    PartNumber = "ASSY-001",
                    Name = "Complex Assembly Housing",
                    Description = "Multi-component assembly housing requiring multiple manufacturing stages including EDM for internal channels.",
                    Industry = "Defense",
                    Application = "Custom Component",
                    Material = "17-4 PH Stainless Steel",
                    SlsMaterial = "17-4 PH Stainless Steel",
                    EstimatedHours = 20.0,
                    MaterialCostPerKg = 320.00m,
                    StandardLaborCostPerHour = 85.00m,
                    PartCategory = "Production",
                    PartClass = "A",
                    ProcessType = "SLS Metal",
                    RequiredMachineType = "TruPrint 3000",
                    IsActive = true,
                    RequiresSLSPrinting = true,
                    RequiresInspection = true,
                    
                    ManufacturingStage = "Multi-Stage",
                    StageDetails = "{\"assembly_components\": 5, \"internal_channels\": \"complex\"}",
                    StageOrder = 1,
                    RequiresCNCMachining = true,
                    RequiresEDMOperations = true,
                    RequiresAssembly = true,
                    RequiresFinishing = true,
                    
                    BTComponentType = "General",
                    BTFirearmCategory = "Component",
                    
                    IsAssemblyComponent = true,
                    IsSubAssembly = true,
                    
                    LengthMm = 120.0,
                    WidthMm = 80.0,
                    HeightMm = 45.0,
                    WeightGrams = 425.0,
                    VolumeMm3 = 432000,
                    Dimensions = "120 × 80 × 45 mm",
                    MaxSurfaceRoughnessRa = 6.3,
                    
                    CustomerPartNumber = "DEF-ASSY-001",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = "Seeder",
                    LastModifiedDate = DateTime.UtcNow
                }
            };
            
            context.Parts.AddRange(parts);
            await context.SaveChangesAsync();
            Console.WriteLine($"? [SEED] Created {parts.Count} sample parts");
        }
        
        private static async Task SeedMachinesAsync(SchedulerContext context)
        {
            Console.WriteLine("?? [SEED] Creating sample machines...");
            
            var machines = new List<Machine>
            {
                new Machine
                {
                    MachineId = "TI1",
                    Name = "TruPrint 3000 - TI1",
                    MachineName = "TruPrint 3000 - TI1",
                    MachineType = "TruPrint 3000",
                    MachineModel = "TruPrint 3000",
                    SerialNumber = "TP3000-001",
                    Status = "Available",
                    IsActive = true,
                    BuildLengthMm = 300,
                    BuildWidthMm = 300,
                    BuildHeightMm = 400,
                    SupportedMaterials = "Ti-6Al-4V,Inconel 718,316L SS",
                    CurrentMaterial = "Ti-6Al-4V Grade 5",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                },
                new Machine
                {
                    MachineId = "TI2",
                    Name = "TruPrint 3000 - TI2",
                    MachineName = "TruPrint 3000 - TI2",
                    MachineType = "TruPrint 3000",
                    MachineModel = "TruPrint 3000",
                    SerialNumber = "TP3000-002",
                    Status = "Available",
                    IsActive = true,
                    BuildLengthMm = 300,
                    BuildWidthMm = 300,
                    BuildHeightMm = 400,
                    SupportedMaterials = "Ti-6Al-4V,Inconel 718,316L SS",
                    CurrentMaterial = "Inconel 718",
                    CreatedBy = "Seeder",
                    CreatedDate = DateTime.UtcNow
                }
            };
            
            context.Machines.AddRange(machines);
            await context.SaveChangesAsync();
            Console.WriteLine($"? [SEED] Created {machines.Count} machines");
        }
        
        private static async Task SeedUsersAsync(SchedulerContext context)
        {
            Console.WriteLine("?? [SEED] Creating sample users...");
            
            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@opcentrix.com",
                    FullName = "System Administrator",
                    Role = "Administrator",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Seeder"
                },
                new User
                {
                    Username = "operator1",
                    Email = "operator1@opcentrix.com",
                    FullName = "John Smith",
                    Role = "Operator",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Seeder"
                },
                new User
                {
                    Username = "quality1",
                    Email = "quality1@opcentrix.com",
                    FullName = "Sarah Johnson",
                    Role = "QualityInspector",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Seeder"
                }
            };
            
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            Console.WriteLine($"? [SEED] Created {users.Count} users");
        }
        
        private static async Task SeedPartStageRequirementsAsync(SchedulerContext context, IPartStageService partStageService)
        {
            Console.WriteLine("?? [SEED] Creating part-stage relationships...");
            
            var parts = await context.Parts.ToListAsync();
            var stages = await context.ProductionStages.ToListAsync();
            
            // B&T Suppressor - Complex workflow
            var btPart = parts.FirstOrDefault(p => p.PartNumber == "BT-SUP-001");
            if (btPart != null)
            {
                var btStages = new[]
                {
                    (stages.First(s => s.Name == "SLS Printing"), 1, 12.0, 200.00m),
                    (stages.First(s => s.Name == "Heat Treatment"), 2, 4.0, 150.00m),
                    (stages.First(s => s.Name == "CNC Machining"), 3, 6.0, 100.00m),
                    (stages.First(s => s.Name == "EDM Operations"), 4, 8.0, 75.00m),
                    (stages.First(s => s.Name == "Finishing"), 5, 4.0, 50.00m),
                    (stages.First(s => s.Name == "Quality Inspection"), 6, 3.0, 25.00m)
                };
                
                foreach (var (stage, order, hours, materialCost) in btStages)
                {
                    await partStageService.AddPartStageAsync(new PartStageRequirement
                    {
                        PartId = btPart.Id,
                        ProductionStageId = stage.Id,
                        ExecutionOrder = order,
                        EstimatedHours = hours,
                        MaterialCost = materialCost,
                        IsRequired = true,
                        IsActive = true,
                        CreatedBy = "Seeder"
                    });
                }
            }
            
            // Aerospace part - High precision workflow
            var aeroPart = parts.FirstOrDefault(p => p.PartNumber == "AERO-001");
            if (aeroPart != null)
            {
                var aeroStages = new[]
                {
                    (stages.First(s => s.Name == "SLS Printing"), 1, 16.0, 300.00m),
                    (stages.First(s => s.Name == "Heat Treatment"), 2, 6.0, 100.00m),
                    (stages.First(s => s.Name == "CNC Machining"), 3, 8.0, 75.00m),
                    (stages.First(s => s.Name == "Finishing"), 4, 3.0, 25.00m),
                    (stages.First(s => s.Name == "Quality Inspection"), 5, 4.0, 15.00m)
                };
                
                foreach (var (stage, order, hours, materialCost) in aeroStages)
                {
                    await partStageService.AddPartStageAsync(new PartStageRequirement
                    {
                        PartId = aeroPart.Id,
                        ProductionStageId = stage.Id,
                        ExecutionOrder = order,
                        EstimatedHours = hours,
                        MaterialCost = materialCost,
                        IsRequired = true,
                        IsActive = true,
                        CreatedBy = "Seeder"
                    });
                }
            }
            
            // Simple prototype - Basic workflow
            var protoPart = parts.FirstOrDefault(p => p.PartNumber == "PROTO-001");
            if (protoPart != null)
            {
                await partStageService.AddPartStageAsync(new PartStageRequirement
                {
                    PartId = protoPart.Id,
                    ProductionStageId = stages.First(s => s.Name == "SLS Printing").Id,
                    ExecutionOrder = 1,
                    EstimatedHours = 4.0,
                    MaterialCost = 25.00m,
                    IsRequired = true,
                    IsActive = true,
                    CreatedBy = "Seeder"
                });
            }
            
            Console.WriteLine("? [SEED] Created part-stage relationships");
        }
        
        private static async Task SeedJobsAsync(SchedulerContext context)
        {
            Console.WriteLine("?? [SEED] Creating sample jobs...");
            
            var parts = await context.Parts.ToListAsync();
            var machines = await context.Machines.ToListAsync();
            
            if (parts.Any() && machines.Any())
            {
                var jobs = new List<Job>
                {
                    new Job
                    {
                        PartNumber = parts.First().PartNumber,
                        Quantity = 1,
                        Priority = 5, // High priority
                        Status = "InProgress",
                        EstimatedDuration = TimeSpan.FromHours(12),
                        ActualStart = DateTime.Now.AddHours(-2),
                        MachineId = machines.First().MachineId,
                        CreatedBy = "admin",
                        CreatedDate = DateTime.UtcNow,
                        Notes = "Priority job for B&T suppressor component"
                    },
                    new Job
                    {
                        PartNumber = parts.Skip(1).First().PartNumber,
                        Quantity = 2,
                        Priority = 3, // Medium priority
                        Status = "Scheduled",
                        EstimatedDuration = TimeSpan.FromHours(32),
                        ScheduledStart = DateTime.Now.AddDays(1),
                        MachineId = machines.Last().MachineId,
                        CreatedBy = "admin",
                        CreatedDate = DateTime.UtcNow,
                        Notes = "Aerospace turbine blade production run"
                    }
                };
                
                context.Jobs.AddRange(jobs);
                await context.SaveChangesAsync();
                Console.WriteLine($"? [SEED] Created {jobs.Count} sample jobs");
            }
        }
    }
}