using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <summary>
    /// Fixes critical database schema issues for Parts page functionality
    /// </summary>
    public partial class FixPartsPageSchemaIssues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix MachineCapabilities foreign key type mismatch
            // Current: INTEGER MachineId -> Machines.Id
            // Required: TEXT MachineId -> Machines.MachineId
            
            // Step 1: Create new MachineCapabilities table with correct schema
            migrationBuilder.Sql(@"
                CREATE TABLE MachineCapabilities_New (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MachineId TEXT NOT NULL,
                    CapabilityType TEXT NOT NULL,
                    CapabilityName TEXT NOT NULL,
                    CapabilityValue TEXT NOT NULL,
                    IsAvailable INTEGER NOT NULL DEFAULT 1,
                    Priority INTEGER NOT NULL DEFAULT 3,
                    MinValue REAL,
                    MaxValue REAL,
                    Unit TEXT NOT NULL DEFAULT '',
                    Notes TEXT NOT NULL DEFAULT '',
                    RequiredCertification TEXT NOT NULL DEFAULT '',
                    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                    LastModifiedDate TEXT NOT NULL DEFAULT (datetime('now')),
                    CreatedBy TEXT NOT NULL DEFAULT 'System',
                    LastModifiedBy TEXT NOT NULL DEFAULT 'System'
                );
            ");

            // Step 2: Migrate data with proper foreign key mapping
            migrationBuilder.Sql(@"
                INSERT INTO MachineCapabilities_New (
                    Id, MachineId, CapabilityType, CapabilityName, CapabilityValue,
                    IsAvailable, Priority, MinValue, MaxValue, Unit, Notes,
                    RequiredCertification, CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
                )
                SELECT 
                    mc.Id,
                    m.MachineId,  -- Convert INTEGER reference to TEXT reference
                    mc.CapabilityType,
                    mc.CapabilityName,
                    mc.CapabilityValue,
                    mc.IsAvailable,
                    mc.Priority,
                    mc.MinValue,
                    mc.MaxValue,
                    mc.Unit,
                    mc.Notes,
                    mc.RequiredCertification,
                    mc.CreatedDate,
                    mc.LastModifiedDate,
                    mc.CreatedBy,
                    mc.LastModifiedBy
                FROM MachineCapabilities mc
                INNER JOIN Machines m ON mc.MachineId = m.Id;
            ");

            // Step 3: Replace old table
            migrationBuilder.Sql("DROP TABLE MachineCapabilities;");
            migrationBuilder.Sql("ALTER TABLE MachineCapabilities_New RENAME TO MachineCapabilities;");

            // Step 4: Recreate indexes
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_MachineId ON MachineCapabilities(MachineId);");
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_CapabilityType ON MachineCapabilities(CapabilityType);");
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_IsAvailable ON MachineCapabilities(IsAvailable);");
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_MachineId_CapabilityType ON MachineCapabilities(MachineId, CapabilityType);");

            // Fix any NULL values in Parts table to prevent data reader errors
            migrationBuilder.Sql(@"
                UPDATE Parts 
                SET 
                    Name = COALESCE(Name, ''),
                    Description = COALESCE(Description, ''),
                    CustomerPartNumber = COALESCE(CustomerPartNumber, ''),
                    Industry = COALESCE(Industry, 'General'),
                    Application = COALESCE(Application, 'General Component'),
                    PartClass = COALESCE(PartClass, 'B'),
                    CreatedBy = COALESCE(CreatedBy, 'System'),
                    LastModifiedBy = COALESCE(LastModifiedBy, 'System'),
                    AdminOverrideReason = COALESCE(AdminOverrideReason, ''),
                    AdminOverrideBy = COALESCE(AdminOverrideBy, ''),
                    AvgDuration = COALESCE(AvgDuration, '8h 0m'),
                    Dimensions = COALESCE(Dimensions, ''),
                    PowderSpecification = COALESCE(PowderSpecification, '15-45 micron particle size'),
                    SurfaceFinishRequirement = COALESCE(SurfaceFinishRequirement, 'As-built'),
                    ProcessType = COALESCE(ProcessType, 'SLS Metal'),
                    RequiredMachineType = COALESCE(RequiredMachineType, 'TruPrint 3000'),
                    PreferredMachines = COALESCE(PreferredMachines, 'TI1,TI2'),
                    QualityStandards = COALESCE(QualityStandards, 'ASTM F3001, ISO 17296'),
                    ToleranceRequirements = COALESCE(ToleranceRequirements, '±0.1mm typical'),
                    RequiredSkills = COALESCE(RequiredSkills, 'SLS Operation,Powder Handling'),
                    RequiredCertifications = COALESCE(RequiredCertifications, 'SLS Operation Certification'),
                    RequiredTooling = COALESCE(RequiredTooling, 'Build Platform,Powder Sieve'),
                    ConsumableMaterials = COALESCE(ConsumableMaterials, 'Argon Gas,Build Platform Coating'),
                    SupportStrategy = COALESCE(SupportStrategy, 'Minimal supports on overhangs > 45°'),
                    ProcessParameters = COALESCE(ProcessParameters, '{}'),
                    QualityCheckpoints = COALESCE(QualityCheckpoints, '{}'),
                    BuildFileTemplate = COALESCE(BuildFileTemplate, ''),
                    CadFilePath = COALESCE(CadFilePath, ''),
                    CadFileVersion = COALESCE(CadFileVersion, '')
                WHERE 
                    Name IS NULL OR Name = ''
                    OR Description IS NULL OR Description = ''
                    OR CustomerPartNumber IS NULL
                    OR Industry IS NULL OR Industry = ''
                    OR Application IS NULL OR Application = ''
                    OR PartClass IS NULL OR PartClass = ''
                    OR CreatedBy IS NULL OR CreatedBy = ''
                    OR LastModifiedBy IS NULL OR LastModifiedBy = ''
                    OR AdminOverrideReason IS NULL
                    OR AdminOverrideBy IS NULL
                    OR AvgDuration IS NULL OR AvgDuration = ''
                    OR Dimensions IS NULL
                    OR PowderSpecification IS NULL OR PowderSpecification = ''
                    OR SurfaceFinishRequirement IS NULL OR SurfaceFinishRequirement = ''
                    OR ProcessType IS NULL OR ProcessType = ''
                    OR RequiredMachineType IS NULL OR RequiredMachineType = ''
                    OR PreferredMachines IS NULL OR PreferredMachines = ''
                    OR QualityStandards IS NULL OR QualityStandards = ''
                    OR ToleranceRequirements IS NULL OR ToleranceRequirements = ''
                    OR RequiredSkills IS NULL OR RequiredSkills = ''
                    OR RequiredCertifications IS NULL OR RequiredCertifications = ''
                    OR RequiredTooling IS NULL OR RequiredTooling = ''
                    OR ConsumableMaterials IS NULL OR ConsumableMaterials = ''
                    OR SupportStrategy IS NULL OR SupportStrategy = ''
                    OR ProcessParameters IS NULL OR ProcessParameters = ''
                    OR QualityCheckpoints IS NULL OR QualityCheckpoints = ''
                    OR BuildFileTemplate IS NULL
                    OR CadFilePath IS NULL
                    OR CadFileVersion IS NULL;
            ");

            // Add performance indexes for Parts queries
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Parts_Name ON Parts(Name);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Parts_CustomerPartNumber ON Parts(CustomerPartNumber);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Parts_SlsMaterial ON Parts(SlsMaterial);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Parts_ProcessType ON Parts(ProcessType);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Parts_RequiredMachineType ON Parts(RequiredMachineType);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert MachineCapabilities back to INTEGER foreign key
            migrationBuilder.Sql(@"
                CREATE TABLE MachineCapabilities_Old (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MachineId INTEGER NOT NULL,
                    CapabilityType TEXT NOT NULL,
                    CapabilityName TEXT NOT NULL,
                    CapabilityValue TEXT NOT NULL,
                    IsAvailable INTEGER NOT NULL DEFAULT 1,
                    Priority INTEGER NOT NULL DEFAULT 3,
                    MinValue REAL,
                    MaxValue REAL,
                    Unit TEXT NOT NULL DEFAULT '',
                    Notes TEXT NOT NULL DEFAULT '',
                    RequiredCertification TEXT NOT NULL DEFAULT '',
                    CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                    LastModifiedDate TEXT NOT NULL DEFAULT (datetime('now')),
                    CreatedBy TEXT NOT NULL DEFAULT 'System',
                    LastModifiedBy TEXT NOT NULL DEFAULT 'System'
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO MachineCapabilities_Old (
                    Id, MachineId, CapabilityType, CapabilityName, CapabilityValue,
                    IsAvailable, Priority, MinValue, MaxValue, Unit, Notes,
                    RequiredCertification, CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
                )
                SELECT 
                    mc.Id,
                    m.Id,  -- Convert back to INTEGER reference
                    mc.CapabilityType,
                    mc.CapabilityName,
                    mc.CapabilityValue,
                    mc.IsAvailable,
                    mc.Priority,
                    mc.MinValue,
                    mc.MaxValue,
                    mc.Unit,
                    mc.Notes,
                    mc.RequiredCertification,
                    mc.CreatedDate,
                    mc.LastModifiedDate,
                    mc.CreatedBy,
                    mc.LastModifiedBy
                FROM MachineCapabilities mc
                INNER JOIN Machines m ON mc.MachineId = m.MachineId;
            ");

            migrationBuilder.Sql("DROP TABLE MachineCapabilities;");
            migrationBuilder.Sql("ALTER TABLE MachineCapabilities_Old RENAME TO MachineCapabilities;");
        }
    }
}