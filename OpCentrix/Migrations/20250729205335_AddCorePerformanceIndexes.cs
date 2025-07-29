using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddCorePerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use SQL commands to safely create indexes only if they don't exist
            // Conservative approach - only add indexes for core tables we know exist
            
            // Performance indexes for Jobs table - critical for scheduler performance
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Jobs_ScheduledStart_ScheduledEnd 
                ON Jobs (ScheduledStart, ScheduledEnd);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Jobs_Status_Priority 
                ON Jobs (Status, Priority DESC);
            ");

            // Performance indexes for Parts table - critical for parts management
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Parts_Material_Industry 
                ON Parts (Material, Industry);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Parts_IsActive_EstimatedHours 
                ON Parts (IsActive DESC, EstimatedHours);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Parts_LastModifiedDate 
                ON Parts (LastModifiedDate DESC);
            ");

            // Performance indexes for Users table - important for authentication
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Users_Role_IsActive 
                ON Users (Role, IsActive DESC);
            ");

            // Performance indexes for Machines table - critical for machine management
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Machines_Status_IsActive 
                ON Machines (Status, IsActive DESC);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes safely
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Machines_Status_IsActive;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Users_Role_IsActive;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Parts_LastModifiedDate;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Parts_IsActive_EstimatedHours;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Parts_Material_Industry;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Jobs_Status_Priority;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Jobs_ScheduledStart_ScheduledEnd;");
        }
    }
}
