using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <summary>
    /// Fixes critical machine capability foreign key issues
    /// Ensures MachineCapabilities properly references Machine.Id (integer primary key)
    /// </summary>
    public partial class FixMachineCapabilitiesForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Backup existing data and recreate MachineCapabilities table with correct schema
            migrationBuilder.Sql(@"
                -- Create backup of existing data
                CREATE TABLE MachineCapabilities_Backup AS 
                SELECT * FROM MachineCapabilities;
            ");

            // Step 2: Drop existing table and recreate with correct foreign key
            migrationBuilder.Sql("DROP TABLE IF EXISTS MachineCapabilities;");

            migrationBuilder.Sql(@"
                CREATE TABLE MachineCapabilities (
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
                    LastModifiedBy TEXT NOT NULL DEFAULT 'System',
                    FOREIGN KEY (MachineId) REFERENCES Machines(Id) ON DELETE CASCADE
                );
            ");

            // Step 3: Migrate data back using proper foreign key relationship
            migrationBuilder.Sql(@"
                INSERT INTO MachineCapabilities (
                    Id, MachineId, CapabilityType, CapabilityName, CapabilityValue,
                    IsAvailable, Priority, MinValue, MaxValue, Unit, Notes,
                    RequiredCertification, CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy
                )
                SELECT 
                    bc.Id,
                    m.Id as MachineId,  -- Use Machine.Id (integer primary key) as foreign key
                    bc.CapabilityType,
                    bc.CapabilityName,
                    bc.CapabilityValue,
                    bc.IsAvailable,
                    COALESCE(bc.Priority, 3) as Priority,
                    bc.MinValue,
                    bc.MaxValue,
                    COALESCE(bc.Unit, '') as Unit,
                    COALESCE(bc.Notes, '') as Notes,
                    COALESCE(bc.RequiredCertification, '') as RequiredCertification,
                    bc.CreatedDate,
                    bc.LastModifiedDate,
                    COALESCE(bc.CreatedBy, 'System') as CreatedBy,
                    COALESCE(bc.LastModifiedBy, 'System') as LastModifiedBy
                FROM MachineCapabilities_Backup bc
                INNER JOIN Machines m ON (
                    -- Handle both scenarios: if backup has string or integer MachineId
                    CASE 
                        WHEN bc.MachineId GLOB '[0-9]*' THEN CAST(bc.MachineId AS INTEGER) = m.Id
                        ELSE bc.MachineId = m.MachineId
                    END
                );
            ");

            // Step 4: Create proper indexes
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_MachineId ON MachineCapabilities(MachineId);");
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_CapabilityType ON MachineCapabilities(CapabilityType);");
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_IsAvailable ON MachineCapabilities(IsAvailable);");
            migrationBuilder.Sql("CREATE INDEX IX_MachineCapabilities_MachineId_CapabilityType ON MachineCapabilities(MachineId, CapabilityType);");

            // Step 5: Clean up backup table
            migrationBuilder.Sql("DROP TABLE MachineCapabilities_Backup;");

            // Step 6: Verify foreign key constraints work
            migrationBuilder.Sql("PRAGMA foreign_key_check(MachineCapabilities);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to previous schema (for rollback if needed)
            migrationBuilder.Sql(@"
                CREATE TABLE MachineCapabilities_Rollback AS 
                SELECT 
                    Id,
                    m.MachineId as MachineId,  -- Convert back to string reference
                    CapabilityType,
                    CapabilityName,
                    CapabilityValue,
                    IsAvailable,
                    Priority,
                    MinValue,
                    MaxValue,
                    Unit,
                    Notes,
                    RequiredCertification,
                    CreatedDate,
                    LastModifiedDate,
                    CreatedBy,
                    LastModifiedBy
                FROM MachineCapabilities mc
                INNER JOIN Machines m ON mc.MachineId = m.Id;
            ");

            migrationBuilder.Sql("DROP TABLE MachineCapabilities;");
            migrationBuilder.Sql("ALTER TABLE MachineCapabilities_Rollback RENAME TO MachineCapabilities;");
        }
    }
}