using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeBTPartsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // B&T Performance Optimization Indexes
            
            // B&T Compliance Performance - Optimizes filtering by compliance requirements
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTCompliance_Composite",
                table: "Parts",
                columns: new[] { "RequiresATFCompliance", "RequiresITARCompliance", "RequiresSerialization", "IsActive" });

            // Component and Firearm Type Performance - Optimizes B&T categorization queries
            migrationBuilder.CreateIndex(
                name: "IX_Parts_ComponentFirearm_Composite",
                table: "Parts",
                columns: new[] { "ComponentType", "FirearmType", "IsActive" });

            // Classification and Material Lookup - Optimizes parts classification queries
            migrationBuilder.CreateIndex(
                name: "IX_Parts_Classification_Material",
                table: "Parts",
                columns: new[] { "PartClassificationId", "Material", "IsActive" });

            // B&T Search Optimization - Optimizes B&T-specific search patterns
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTSearch_Composite",
                table: "Parts",
                columns: new[] { "PartNumber", "ComponentType", "FirearmType", "ExportClassification" });

            // Serial Number Performance - Optimizes serial number tracking and status queries
            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_Part_Status",
                table: "SerialNumbers",
                columns: new[] { "PartId", "ATFComplianceStatus", "QualityStatus", "IsActive" });

            // Compliance Document Performance - Optimizes compliance document lookups
            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_Part_Status",
                table: "ComplianceDocuments",
                columns: new[] { "PartId", "Status", "IsActive" });

            // Part Classification Performance - Optimizes classification filtering
            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_Industry_Category",
                table: "PartClassifications",
                columns: new[] { "IndustryType", "ComponentCategory", "IsActive" });

            // Enhanced Part Classification relationships - Optimizes classification joins
            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartClassification_Enhanced",
                table: "Parts",
                columns: new[] { "PartClassificationId", "ComponentType", "IsActive" });

            // Serial Number tracking optimization - Optimizes serial number history queries
            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_Tracking_Enhanced",
                table: "SerialNumbers",
                columns: new[] { "PartId", "AssignedDate", "ATFComplianceStatus", "IsActive" });

            // Compliance Document relationship optimization - Optimizes document relationship queries
            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_Relationships",
                table: "ComplianceDocuments",
                columns: new[] { "PartId", "SerialNumberId", "ComplianceRequirementId", "Status", "IsActive" });

            // Cross-reference optimization - Optimizes part number cross-referencing
            migrationBuilder.CreateIndex(
                name: "IX_Parts_CrossReference",
                table: "Parts",
                columns: new[] { "PartNumber", "CustomerPartNumber", "PartClassificationId", "IsActive" });

            // B&T testing requirements optimization - Optimizes testing requirement queries
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTTesting_Composite",
                table: "Parts",
                columns: new[] { "RequiresPressureTesting", "RequiresProofTesting", "RequiresDimensionalVerification", "IsActive" });

            // Export control optimization - Optimizes export control and ITAR queries
            migrationBuilder.CreateIndex(
                name: "IX_Parts_ExportControl_Composite",
                table: "Parts",
                columns: new[] { "IsEARControlled", "IsControlledItem", "ExportClassification", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove B&T Performance Indexes
            migrationBuilder.DropIndex(name: "IX_Parts_BTCompliance_Composite", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_ComponentFirearm_Composite", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_Classification_Material", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_BTSearch_Composite", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_SerialNumbers_Part_Status", table: "SerialNumbers");
            migrationBuilder.DropIndex(name: "IX_ComplianceDocuments_Part_Status", table: "ComplianceDocuments");
            migrationBuilder.DropIndex(name: "IX_PartClassifications_Industry_Category", table: "PartClassifications");
            migrationBuilder.DropIndex(name: "IX_Parts_PartClassification_Enhanced", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_SerialNumbers_Tracking_Enhanced", table: "SerialNumbers");
            migrationBuilder.DropIndex(name: "IX_ComplianceDocuments_Relationships", table: "ComplianceDocuments");
            migrationBuilder.DropIndex(name: "IX_Parts_CrossReference", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_BTTesting_Composite", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_ExportControl_Composite", table: "Parts");
        }
    }
}
