using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing custom field templates for production stages
    /// Provides pre-configured custom field templates for common manufacturing processes
    /// </summary>
    public interface IStageTemplateService
    {
        /// <summary>
        /// Get all available stage templates with custom fields
        /// </summary>
        Task<List<ProductionStageTemplate>> GetAllStageTemplatesAsync();
        
        /// <summary>
        /// Get custom field template for a specific stage type
        /// </summary>
        Task<List<CustomFieldDefinition>> GetCustomFieldsForStageAsync(string stageName);
        
        /// <summary>
        /// Create a production stage with template-based custom fields
        /// </summary>
        Task<ProductionStage> CreateStageFromTemplateAsync(string stageName);
        
        /// <summary>
        /// Get all available stage template names
        /// </summary>
        Task<List<string>> GetAvailableStageNamesAsync();
    }

    public class StageTemplateService : IStageTemplateService
    {
        private readonly ILogger<StageTemplateService> _logger;

        public StageTemplateService(ILogger<StageTemplateService> logger)
        {
            _logger = logger;
        }

        public async Task<List<ProductionStageTemplate>> GetAllStageTemplatesAsync()
        {
            return await Task.FromResult(GetStageTemplates());
        }

        public async Task<List<CustomFieldDefinition>> GetCustomFieldsForStageAsync(string stageName)
        {
            var template = GetStageTemplates().FirstOrDefault(t => 
                t.Name.Equals(stageName, StringComparison.OrdinalIgnoreCase));
            
            return await Task.FromResult(template?.CustomFields ?? new List<CustomFieldDefinition>());
        }

        public async Task<ProductionStage> CreateStageFromTemplateAsync(string stageName)
        {
            var template = GetStageTemplates().FirstOrDefault(t => 
                t.Name.Equals(stageName, StringComparison.OrdinalIgnoreCase));
            
            if (template == null)
            {
                throw new ArgumentException($"No template found for stage: {stageName}");
            }

            var stage = new ProductionStage
            {
                Name = template.Name,
                DisplayOrder = template.DisplayOrder,
                Description = template.Description,
                DefaultSetupMinutes = template.DefaultSetupMinutes,
                DefaultHourlyRate = template.DefaultHourlyRate,
                DefaultDurationHours = template.DefaultDurationHours,
                DefaultMaterialCost = template.DefaultMaterialCost,
                RequiresQualityCheck = template.RequiresQualityCheck,
                RequiresApproval = template.RequiresApproval,
                AllowSkip = template.AllowSkip,
                IsOptional = template.IsOptional,
                RequiredRole = template.RequiredRole,
                StageColor = template.StageColor,
                StageIcon = template.StageIcon,
                Department = template.Department,
                AllowParallelExecution = template.AllowParallelExecution,
                RequiresMachineAssignment = template.RequiresMachineAssignment,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = "System",
                LastModifiedBy = "System",
                IsActive = true
            };

            stage.SetCustomFields(template.CustomFields);
            
            return await Task.FromResult(stage);
        }

        public async Task<List<string>> GetAvailableStageNamesAsync()
        {
            var templates = GetStageTemplates();
            return await Task.FromResult(templates.Select(t => t.Name).ToList());
        }

        /// <summary>
        /// Get all predefined stage templates with comprehensive custom fields
        /// </summary>
        private List<ProductionStageTemplate> GetStageTemplates()
        {
            return new List<ProductionStageTemplate>
            {
                // 1. 3D PRINTING (SLS/FDM/DMLS)
                new ProductionStageTemplate
                {
                    Name = "3D Printing",
                    DisplayOrder = 1,
                    Description = "Additive manufacturing using SLS, FDM, or DMLS technologies",
                    DefaultSetupMinutes = 45,
                    DefaultHourlyRate = 85.00m,
                    DefaultDurationHours = 8.0,
                    DefaultMaterialCost = 120.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "3D Print Operator",
                    StageColor = "#007bff",
                    StageIcon = "fas fa-print",
                    Department = "Additive Manufacturing",
                    AllowParallelExecution = false,
                    RequiresMachineAssignment = true,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "printTechnology",
                            Type = "dropdown",
                            Label = "Print Technology",
                            Description = "Additive manufacturing technology to use",
                            Required = true,
                            Options = new List<string> { "SLS Metal", "SLS Polymer", "FDM", "DMLS", "EBM", "SLA", "DLP" },
                            DefaultValue = "SLS Metal",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "material",
                            Type = "dropdown",
                            Label = "Print Material",
                            Description = "Material to use for printing",
                            Required = true,
                            Options = new List<string> { "Ti-6Al-4V Grade 5", "316L Stainless Steel", "Inconel 718", "Aluminum AlSi10Mg", "Maraging Steel", "PLA", "ABS", "PETG", "Nylon PA12" },
                            DefaultValue = "Ti-6Al-4V Grade 5",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "layerHeight",
                            Type = "number",
                            Label = "Layer Height",
                            Description = "Layer thickness in microns",
                            Required = true,
                            DefaultValue = "30",
                            MinValue = 10,
                            MaxValue = 100,
                            Unit = "?m",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "laserPower",
                            Type = "number",
                            Label = "Laser Power",
                            Description = "Laser power percentage",
                            Required = true,
                            DefaultValue = "75",
                            MinValue = 50,
                            MaxValue = 100,
                            Unit = "%",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "scanSpeed",
                            Type = "number",
                            Label = "Scan Speed",
                            Description = "Laser scan speed",
                            Required = true,
                            DefaultValue = "1200",
                            MinValue = 800,
                            MaxValue = 2000,
                            Unit = "mm/s",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "buildTemperature",
                            Type = "number",
                            Label = "Build Temperature",
                            Description = "Build chamber temperature",
                            Required = true,
                            DefaultValue = "180",
                            MinValue = 150,
                            MaxValue = 200,
                            Unit = "°C",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "supportStructures",
                            Type = "checkbox",
                            Label = "Requires Support Structures",
                            Description = "Whether the part requires support structures",
                            Required = false,
                            DefaultValue = "true",
                            DisplayOrder = 7
                        },
                        new CustomFieldDefinition
                        {
                            Name = "infillDensity",
                            Type = "number",
                            Label = "Infill Density",
                            Description = "Internal infill density percentage (FDM only)",
                            Required = false,
                            DefaultValue = "100",
                            MinValue = 10,
                            MaxValue = 100,
                            Unit = "%",
                            DisplayOrder = 8
                        },
                        new CustomFieldDefinition
                        {
                            Name = "postProcessing",
                            Type = "dropdown",
                            Label = "Post-Processing Required",
                            Description = "Required post-processing after printing",
                            Required = false,
                            Options = new List<string> { "None", "Support Removal", "Powder Removal", "Heat Treatment", "Surface Cleaning", "All" },
                            DefaultValue = "Support Removal",
                            DisplayOrder = 9
                        }
                    }
                },

                // 2. CNC MACHINING
                new ProductionStageTemplate
                {
                    Name = "CNC Machining",
                    DisplayOrder = 2,
                    Description = "Computer numerical control machining for precision operations",
                    DefaultSetupMinutes = 30,
                    DefaultHourlyRate = 95.00m,
                    DefaultDurationHours = 4.5,
                    DefaultMaterialCost = 50.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "CNC Machinist",
                    StageColor = "#28a745",
                    StageIcon = "fas fa-cogs",
                    Department = "Precision Machining",
                    AllowParallelExecution = false,
                    RequiresMachineAssignment = true,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "machineType",
                            Type = "dropdown",
                            Label = "CNC Machine Type",
                            Description = "Type of CNC machine required",
                            Required = true,
                            Options = new List<string> { "3-Axis Mill", "4-Axis Mill", "5-Axis Mill", "CNC Lathe", "Swiss Lathe", "Turn-Mill Center" },
                            DefaultValue = "3-Axis Mill",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "operations",
                            Type = "dropdown",
                            Label = "Primary Operations",
                            Description = "Main CNC operations to perform",
                            Required = true,
                            Options = new List<string> { "Facing", "Drilling", "Tapping", "Boring", "Contouring", "Pocketing", "Threading", "Turning", "Grooving" },
                            DefaultValue = "Facing",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "tolerances",
                            Type = "dropdown",
                            Label = "Dimensional Tolerance",
                            Description = "Required dimensional accuracy",
                            Required = true,
                            Options = new List<string> { "±0.005\"", "±0.002\"", "±0.001\"", "±0.0005\"", "±0.0002\"" },
                            DefaultValue = "±0.002\"",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "surfaceFinish",
                            Type = "dropdown",
                            Label = "Surface Finish",
                            Description = "Required surface finish quality",
                            Required = true,
                            Options = new List<string> { "125 RMS", "63 RMS", "32 RMS", "16 RMS", "8 RMS", "4 RMS" },
                            DefaultValue = "63 RMS",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "tooling",
                            Type = "text",
                            Label = "Special Tooling Required",
                            Description = "List any special tooling or fixtures needed",
                            Required = false,
                            PlaceholderText = "e.g., Custom fixture, specialized drill bits...",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "workholding",
                            Type = "dropdown",
                            Label = "Workholding Method",
                            Description = "Method to hold the workpiece during machining",
                            Required = true,
                            Options = new List<string> { "Machine Vise", "Chuck", "Collet", "Custom Fixture", "Magnetic Chuck", "Vacuum Table" },
                            DefaultValue = "Machine Vise",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "coolant",
                            Type = "dropdown",
                            Label = "Coolant Type",
                            Description = "Coolant/lubrication requirements",
                            Required = true,
                            Options = new List<string> { "Flood Coolant", "Mist Coolant", "Air Blast", "Minimum Quantity Lubrication", "Dry Machining" },
                            DefaultValue = "Flood Coolant",
                            DisplayOrder = 7
                        }
                    }
                },

                // 3. EDM (ELECTRICAL DISCHARGE MACHINING)
                new ProductionStageTemplate
                {
                    Name = "EDM Operations",
                    DisplayOrder = 3,
                    Description = "Electrical discharge machining for complex geometries and hard materials",
                    DefaultSetupMinutes = 60,
                    DefaultHourlyRate = 120.00m,
                    DefaultDurationHours = 6.0,
                    DefaultMaterialCost = 75.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "EDM Specialist",
                    StageColor = "#ffc107",
                    StageIcon = "fas fa-bolt",
                    Department = "Special Processes",
                    AllowParallelExecution = false,
                    RequiresMachineAssignment = true,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "edmType",
                            Type = "dropdown",
                            Label = "EDM Process Type",
                            Description = "Type of EDM process to use",
                            Required = true,
                            Options = new List<string> { "Wire EDM", "Sinker EDM", "Hole Drilling EDM", "Fast Hole EDM" },
                            DefaultValue = "Wire EDM",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "electrodeType",
                            Type = "dropdown",
                            Label = "Electrode Type",
                            Description = "Electrode material for sinker EDM",
                            Required = false,
                            Options = new List<string> { "Copper", "Graphite", "Copper Tungsten", "Silver Tungsten", "Brass" },
                            DefaultValue = "Copper",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "wireType",
                            Type = "dropdown",
                            Label = "Wire Type",
                            Description = "Wire electrode type for wire EDM",
                            Required = false,
                            Options = new List<string> { "Brass Wire", "Coated Wire", "Zinc Coated", "Molybdenum", "Tungsten" },
                            DefaultValue = "Brass Wire",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "wireDiameter",
                            Type = "number",
                            Label = "Wire Diameter",
                            Description = "Wire diameter in inches",
                            Required = false,
                            DefaultValue = "0.010",
                            MinValue = 0.004,
                            MaxValue = 0.012,
                            Unit = "in",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "dielectric",
                            Type = "dropdown",
                            Label = "Dielectric Fluid",
                            Description = "Dielectric fluid type",
                            Required = true,
                            Options = new List<string> { "Deionized Water", "Oil-based", "Hydrocarbon" },
                            DefaultValue = "Deionized Water",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "surfaceFinish",
                            Type = "dropdown",
                            Label = "Required Surface Finish",
                            Description = "Target surface finish quality",
                            Required = true,
                            Options = new List<string> { "Rough (125+ RMS)", "Semi-Finish (32-63 RMS)", "Finish (16-32 RMS)", "Super Finish (<16 RMS)" },
                            DefaultValue = "Semi-Finish (32-63 RMS)",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "tolerances",
                            Type = "dropdown",
                            Label = "Dimensional Tolerance",
                            Description = "Required dimensional accuracy",
                            Required = true,
                            Options = new List<string> { "±0.0005\"", "±0.0002\"", "±0.0001\"", "±0.00005\"" },
                            DefaultValue = "±0.0002\"",
                            DisplayOrder = 7
                        }
                    }
                },

                // 4. LASER ENGRAVING
                new ProductionStageTemplate
                {
                    Name = "Laser Engraving",
                    DisplayOrder = 4,
                    Description = "Laser engraving for serial numbers, markings, and regulatory compliance",
                    DefaultSetupMinutes = 15,
                    DefaultHourlyRate = 75.00m,
                    DefaultDurationHours = 1.5,
                    DefaultMaterialCost = 5.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Laser Operator",
                    StageColor = "#dc3545",
                    StageIcon = "fas fa-crosshairs",
                    Department = "Marking & Engraving",
                    AllowParallelExecution = true,
                    RequiresMachineAssignment = true,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "laserType",
                            Type = "dropdown",
                            Label = "Laser Type",
                            Description = "Type of laser engraving system",
                            Required = true,
                            Options = new List<string> { "Fiber Laser", "CO2 Laser", "UV Laser", "Green Laser", "Diode Laser" },
                            DefaultValue = "Fiber Laser",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "markingType",
                            Type = "dropdown",
                            Label = "Marking Type",
                            Description = "Type of marking to apply",
                            Required = true,
                            Options = new List<string> { "Serial Number", "Part Number", "Logo", "Date Code", "QR Code", "Barcode", "Regulatory Marking" },
                            DefaultValue = "Serial Number",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "markingText",
                            Type = "text",
                            Label = "Marking Text/Pattern",
                            Description = "Text or pattern to engrave",
                            Required = true,
                            PlaceholderText = "Enter the text to be engraved",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "markingDepth",
                            Type = "number",
                            Label = "Engraving Depth",
                            Description = "Target engraving depth",
                            Required = true,
                            DefaultValue = "0.002",
                            MinValue = 0.001,
                            MaxValue = 0.010,
                            Unit = "in",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "fontSize",
                            Type = "number",
                            Label = "Font Size",
                            Description = "Font size for text markings",
                            Required = false,
                            DefaultValue = "0.125",
                            MinValue = 0.050,
                            MaxValue = 0.500,
                            Unit = "in",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "markingLocation",
                            Type = "text",
                            Label = "Marking Location",
                            Description = "Where on the part to place the marking",
                            Required = true,
                            PlaceholderText = "e.g., Top surface, bottom left corner",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "regulatoryCompliance",
                            Type = "checkbox",
                            Label = "ATF/ITAR Compliant Marking",
                            Description = "Marking meets ATF/ITAR regulatory requirements",
                            Required = false,
                            DefaultValue = "true",
                            DisplayOrder = 7
                        }
                    }
                },

                // 5. SANDBLASTING
                new ProductionStageTemplate
                {
                    Name = "Sandblasting",
                    DisplayOrder = 5,
                    Description = "Abrasive blasting for surface preparation and finish uniformity",
                    DefaultSetupMinutes = 20,
                    DefaultHourlyRate = 65.00m,
                    DefaultDurationHours = 2.0,
                    DefaultMaterialCost = 15.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Surface Finisher",
                    StageColor = "#6c757d",
                    StageIcon = "fas fa-spray-can",
                    Department = "Surface Treatment",
                    AllowParallelExecution = true,
                    RequiresMachineAssignment = false,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "blastMedia",
                            Type = "dropdown",
                            Label = "Blast Media Type",
                            Description = "Abrasive media for blasting",
                            Required = true,
                            Options = new List<string> { "Aluminum Oxide", "Glass Beads", "Steel Shot", "Ceramic Beads", "Walnut Shells", "Plastic Media", "Garnet" },
                            DefaultValue = "Aluminum Oxide",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "meshSize",
                            Type = "dropdown",
                            Label = "Media Mesh Size",
                            Description = "Abrasive media particle size",
                            Required = true,
                            Options = new List<string> { "16-30 Mesh", "24-46 Mesh", "36-60 Mesh", "46-100 Mesh", "80-120 Mesh", "120-220 Mesh" },
                            DefaultValue = "36-60 Mesh",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "blastPressure",
                            Type = "number",
                            Label = "Blast Pressure",
                            Description = "Air pressure for blasting",
                            Required = true,
                            DefaultValue = "80",
                            MinValue = 40,
                            MaxValue = 120,
                            Unit = "PSI",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "surfaceProfile",
                            Type = "dropdown",
                            Label = "Target Surface Profile",
                            Description = "Desired surface roughness after blasting",
                            Required = true,
                            Options = new List<string> { "Light Texture (1-2 mils)", "Medium Texture (2-4 mils)", "Heavy Texture (4-6 mils)", "Very Heavy (6+ mils)" },
                            DefaultValue = "Medium Texture (2-4 mils)",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "maskingRequired",
                            Type = "checkbox",
                            Label = "Masking Required",
                            Description = "Part requires selective masking before blasting",
                            Required = false,
                            DefaultValue = "false",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "postCleanMethod",
                            Type = "dropdown",
                            Label = "Post-Blast Cleaning",
                            Description = "Cleaning method after blasting",
                            Required = true,
                            Options = new List<string> { "Compressed Air", "Solvent Wipe", "Ultrasonic Cleaning", "Water Rinse", "Vacuum Only" },
                            DefaultValue = "Compressed Air",
                            DisplayOrder = 6
                        }
                    }
                },

                // 6. COATING
                new ProductionStageTemplate
                {
                    Name = "Coating",
                    DisplayOrder = 6,
                    Description = "Surface coating application including Cerakote, anodizing, and protective finishes",
                    DefaultSetupMinutes = 45,
                    DefaultHourlyRate = 70.00m,
                    DefaultDurationHours = 3.0,
                    DefaultMaterialCost = 35.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = false,
                    AllowSkip = true,
                    IsOptional = true,
                    RequiredRole = "Coating Specialist",
                    StageColor = "#17a2b8",
                    StageIcon = "fas fa-paint-brush",
                    Department = "Surface Treatment",
                    AllowParallelExecution = true,
                    RequiresMachineAssignment = false,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "coatingType",
                            Type = "dropdown",
                            Label = "Coating Type",
                            Description = "Type of coating to apply",
                            Required = true,
                            Options = new List<string> { "Cerakote", "Anodizing", "Powder Coating", "Electroplating", "PVD Coating", "DLC Coating", "Parkerizing" },
                            DefaultValue = "Cerakote",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "coatingColor",
                            Type = "dropdown",
                            Label = "Coating Color",
                            Description = "Color or finish specification",
                            Required = true,
                            Options = new List<string> { "Matte Black", "Gloss Black", "FDE (Flat Dark Earth)", "OD Green", "Tungsten", "Burnt Bronze", "Clear Coat", "Custom Color" },
                            DefaultValue = "Matte Black",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "coatingThickness",
                            Type = "number",
                            Label = "Target Coating Thickness",
                            Description = "Desired coating thickness",
                            Required = true,
                            DefaultValue = "0.0005",
                            MinValue = 0.0002,
                            MaxValue = 0.002,
                            Unit = "in",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "surfacePrep",
                            Type = "dropdown",
                            Label = "Surface Preparation",
                            Description = "Required surface preparation before coating",
                            Required = true,
                            Options = new List<string> { "Sandblast", "Chemical Clean", "Solvent Degrease", "Acid Etch", "Passivation", "Combination" },
                            DefaultValue = "Sandblast",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "cureTemperature",
                            Type = "number",
                            Label = "Cure Temperature",
                            Description = "Curing temperature for the coating",
                            Required = true,
                            DefaultValue = "300",
                            MinValue = 200,
                            MaxValue = 400,
                            Unit = "°F",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "cureTime",
                            Type = "number",
                            Label = "Cure Time",
                            Description = "Curing time at temperature",
                            Required = true,
                            DefaultValue = "120",
                            MinValue = 60,
                            MaxValue = 240,
                            Unit = "minutes",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "maskingRequired",
                            Type = "checkbox",
                            Label = "Selective Masking Required",
                            Description = "Part requires masking to protect certain areas",
                            Required = false,
                            DefaultValue = "true",
                            DisplayOrder = 7
                        },
                        new CustomFieldDefinition
                        {
                            Name = "qualityStandard",
                            Type = "dropdown",
                            Label = "Quality Standard",
                            Description = "Quality standard or specification to meet",
                            Required = false,
                            Options = new List<string> { "MIL-DTL-5541", "ASTM B117", "ASTM D3359", "ISO 12944", "Internal Standard" },
                            DefaultValue = "Internal Standard",
                            DisplayOrder = 8
                        }
                    }
                },

                // 7. ASSEMBLY
                new ProductionStageTemplate
                {
                    Name = "Assembly",
                    DisplayOrder = 7,
                    Description = "Final assembly with components, hardware, and sub-assemblies",
                    DefaultSetupMinutes = 30,
                    DefaultHourlyRate = 80.00m,
                    DefaultDurationHours = 4.0,
                    DefaultMaterialCost = 25.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = true,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Assembler",
                    StageColor = "#fd7e14",
                    StageIcon = "fas fa-puzzle-piece",
                    Department = "Final Assembly",
                    AllowParallelExecution = false,
                    RequiresMachineAssignment = false,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "assemblyType",
                            Type = "dropdown",
                            Label = "Assembly Type",
                            Description = "Type of assembly operation",
                            Required = true,
                            Options = new List<string> { "Mechanical Assembly", "Threaded Assembly", "Press Fit", "Welded Assembly", "Bonded Assembly", "Hybrid Assembly" },
                            DefaultValue = "Mechanical Assembly",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "componentCount",
                            Type = "number",
                            Label = "Component Count",
                            Description = "Total number of components in assembly",
                            Required = true,
                            DefaultValue = "5",
                            MinValue = 2,
                            MaxValue = 50,
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "torqueSpecs",
                            Type = "text",
                            Label = "Torque Specifications",
                            Description = "Torque values for threaded fasteners",
                            Required = false,
                            PlaceholderText = "e.g., M5 screws: 8 Nm, M8 bolts: 25 Nm",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "toolingRequired",
                            Type = "text",
                            Label = "Special Tools Required",
                            Description = "List any special tools or fixtures needed",
                            Required = false,
                            PlaceholderText = "e.g., Torque wrench, assembly fixture, alignment tools",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "sealantRequired",
                            Type = "checkbox",
                            Label = "Thread Sealant Required",
                            Description = "Threaded connections require sealant or locking compound",
                            Required = false,
                            DefaultValue = "false",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "functionalTest",
                            Type = "dropdown",
                            Label = "Functional Testing",
                            Description = "Functional testing requirements",
                            Required = true,
                            Options = new List<string> { "None Required", "Basic Function Check", "Pressure Test", "Leak Test", "Performance Test", "Full Validation" },
                            DefaultValue = "Basic Function Check",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "calibration",
                            Type = "checkbox",
                            Label = "Requires Calibration",
                            Description = "Assembly requires calibration after completion",
                            Required = false,
                            DefaultValue = "false",
                            DisplayOrder = 7
                        },
                        new CustomFieldDefinition
                        {
                            Name = "packaging",
                            Type = "dropdown",
                            Label = "Packaging Requirements",
                            Description = "Special packaging or protection needed",
                            Required = false,
                            Options = new List<string> { "Standard Box", "Anti-Static Bag", "Foam Protection", "Custom Case", "Vacuum Sealed", "Moisture Barrier" },
                            DefaultValue = "Standard Box",
                            DisplayOrder = 8
                        }
                    }
                },

                // 8. SHIPPING
                new ProductionStageTemplate
                {
                    Name = "Shipping",
                    DisplayOrder = 8,
                    Description = "Final packaging and shipping preparation with documentation",
                    DefaultSetupMinutes = 15,
                    DefaultHourlyRate = 45.00m,
                    DefaultDurationHours = 1.0,
                    DefaultMaterialCost = 10.00m,
                    RequiresQualityCheck = true,
                    RequiresApproval = true,
                    AllowSkip = false,
                    IsOptional = false,
                    RequiredRole = "Shipping Clerk",
                    StageColor = "#6f42c1",
                    StageIcon = "fas fa-shipping-fast",
                    Department = "Shipping & Receiving",
                    AllowParallelExecution = true,
                    RequiresMachineAssignment = false,
                    CustomFields = new List<CustomFieldDefinition>
                    {
                        new CustomFieldDefinition
                        {
                            Name = "shippingMethod",
                            Type = "dropdown",
                            Label = "Shipping Method",
                            Description = "Method of shipment",
                            Required = true,
                            Options = new List<string> { "Ground", "2-Day Air", "Next Day Air", "International", "Freight", "Customer Pickup", "Hand Delivery" },
                            DefaultValue = "Ground",
                            DisplayOrder = 1
                        },
                        new CustomFieldDefinition
                        {
                            Name = "packagingType",
                            Type = "dropdown",
                            Label = "Packaging Type",
                            Description = "Type of packaging required",
                            Required = true,
                            Options = new List<string> { "Standard Box", "Padded Envelope", "Custom Crate", "Anti-Static Package", "Hazmat Package", "Temperature Controlled" },
                            DefaultValue = "Standard Box",
                            DisplayOrder = 2
                        },
                        new CustomFieldDefinition
                        {
                            Name = "insuranceValue",
                            Type = "number",
                            Label = "Insurance Value",
                            Description = "Declared value for insurance",
                            Required = false,
                            DefaultValue = "500",
                            MinValue = 0,
                            MaxValue = 50000,
                            Unit = "$",
                            DisplayOrder = 3
                        },
                        new CustomFieldDefinition
                        {
                            Name = "specialHandling",
                            Type = "checkbox",
                            Label = "Special Handling Required",
                            Description = "Package requires special handling instructions",
                            Required = false,
                            DefaultValue = "false",
                            DisplayOrder = 4
                        },
                        new CustomFieldDefinition
                        {
                            Name = "regulatoryDocs",
                            Type = "dropdown",
                            Label = "Required Documentation",
                            Description = "Regulatory or compliance documentation needed",
                            Required = false,
                            Options = new List<string> { "None", "Certificate of Compliance", "Material Cert", "Test Report", "ATF Form", "Export License", "All Documents" },
                            DefaultValue = "Certificate of Compliance",
                            DisplayOrder = 5
                        },
                        new CustomFieldDefinition
                        {
                            Name = "trackingRequired",
                            Type = "checkbox",
                            Label = "Tracking Required",
                            Description = "Customer requires tracking information",
                            Required = false,
                            DefaultValue = "true",
                            DisplayOrder = 6
                        },
                        new CustomFieldDefinition
                        {
                            Name = "signatureRequired",
                            Type = "checkbox",
                            Label = "Signature Required",
                            Description = "Delivery requires recipient signature",
                            Required = false,
                            DefaultValue = "false",
                            DisplayOrder = 7
                        },
                        new CustomFieldDefinition
                        {
                            Name = "exportControl",
                            Type = "checkbox",
                            Label = "Export Controlled Item",
                            Description = "Item subject to export control regulations",
                            Required = false,
                            DefaultValue = "false",
                            DisplayOrder = 8
                        }
                    }
                }
            };
        }
    }
}

/// <summary>
/// Represents a template for creating production stages with predefined custom fields
/// </summary>
public class ProductionStageTemplate
{
    public string Name { get; set; } = "";
    public int DisplayOrder { get; set; }
    public string Description { get; set; } = "";
    public int DefaultSetupMinutes { get; set; }
    public decimal DefaultHourlyRate { get; set; }
    public double DefaultDurationHours { get; set; }
    public decimal DefaultMaterialCost { get; set; }
    public bool RequiresQualityCheck { get; set; }
    public bool RequiresApproval { get; set; }
    public bool AllowSkip { get; set; }
    public bool IsOptional { get; set; }
    public string? RequiredRole { get; set; }
    public string StageColor { get; set; } = "#007bff";
    public string StageIcon { get; set; } = "fas fa-cogs";
    public string? Department { get; set; }
    public bool AllowParallelExecution { get; set; }
    public bool RequiresMachineAssignment { get; set; }
    public List<CustomFieldDefinition> CustomFields { get; set; } = new List<CustomFieldDefinition>();
}