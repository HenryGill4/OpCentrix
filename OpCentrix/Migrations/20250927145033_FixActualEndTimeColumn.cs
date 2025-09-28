using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class FixActualEndTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration intentionally left as NO-OP to avoid SQLite table rebuild conflicts with existing views.
            // Original intent (default value tweak + stacking columns) already realized in schema; executing again caused errors.
            // We only ensure auxiliary table exists.
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS MachineOperatorAssignments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MachineId TEXT NOT NULL,
                UserId INTEGER NOT NULL,
                IsPrimary INTEGER NOT NULL,
                EffectiveFrom TEXT NULL,
                EffectiveTo TEXT NULL,
                IsActive INTEGER NOT NULL,
                CreatedDate TEXT NOT NULL DEFAULT (datetime('now')),
                LastModifiedDate TEXT NOT NULL DEFAULT (datetime('now')),
                CreatedBy TEXT NOT NULL DEFAULT 'System',
                LastModifiedBy TEXT NOT NULL DEFAULT 'System',
                CONSTRAINT CK_Assignment_DateRange CHECK ((EffectiveTo IS NULL) OR (EffectiveFrom IS NULL) OR (EffectiveTo >= EffectiveFrom)),
                FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_IsActive ON MachineOperatorAssignments (IsActive);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_MachineId ON MachineOperatorAssignments (MachineId);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_MachineId_IsPrimary ON MachineOperatorAssignments (MachineId, IsPrimary);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_UserId ON MachineOperatorAssignments (UserId);
            CREATE INDEX IF NOT EXISTS IX_MachineOperatorAssignments_UserId_IsActive ON MachineOperatorAssignments (UserId, IsActive);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Safe rollback: drop auxiliary table if created.
            migrationBuilder.Sql("DROP TABLE IF EXISTS MachineOperatorAssignments;");
        }
    }
}
