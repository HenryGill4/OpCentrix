using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <summary>
    /// B&T Manufacturing Parts Refactor - Comprehensive database update for firearms and suppressor manufacturing
    /// Adds manufacturing stages, compliance tracking, regulatory fields, and B&T-specific part classifications
    /// </summary>
    public partial class BTManufacturingPartsRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================================
            // PHASE 1: ADD B&T MANUFACTURING STAGES TO PARTS TABLE
            // ============================================================================
            
            // Manufacturing Stage Tracking
            migrationBuilder.AddColumn<string>(
                name: "ManufacturingStage",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Design");
                
            migrationBuilder.AddColumn<string>(
                name: "StageDetails",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "{}");
                
            migrationBuilder.AddColumn<int>(
                name: "StageOrder",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresSLSPrinting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresCNCMachining",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresEDMOperations",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresAssembly",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresFinishing",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            // ============================================================================
            // PHASE 2: ADD B&T FIREARMS CLASSIFICATION FIELDS
            // ============================================================================
            
            // Firearm and Suppressor Classification
            migrationBuilder.AddColumn<string>(
                name: "BTComponentType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "General");
                
            migrationBuilder.AddColumn<string>(
                name: "BTFirearmCategory",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Component");
                
            migrationBuilder.AddColumn<string>(
                name: "BTSuppressorType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<string>(
                name: "BTBafflePosition",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<string>(
                name: "BTCaliberCompatibility",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<string>(
                name: "BTThreadPitch",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<double>(
                name: "BTSoundReductionDB",
                table: "Parts",
                type: "REAL",
                nullable: true);
                
            migrationBuilder.AddColumn<double>(
                name: "BTBackPressurePSI",
                table: "Parts",
                type: "REAL",
                nullable: true);
                
            // ============================================================================
            // PHASE 3: ADD REGULATORY COMPLIANCE FIELDS
            // ============================================================================
            
            // ATF/FFL Compliance
            migrationBuilder.AddColumn<bool>(
                name: "RequiresATFForm1",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresATFForm4",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<string>(
                name: "ATFClassification",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<string>(
                name: "FFLRequirements",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresTaxStamp",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<decimal>(
                name: "TaxStampAmount",
                table: "Parts",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);
                
            // ITAR/EAR Export Control
            migrationBuilder.AddColumn<string>(
                name: "ITARCategory",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<string>(
                name: "EARClassification",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresExportLicense",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<string>(
                name: "ExportControlNotes",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
                
            // ============================================================================
            // PHASE 4: ADD B&T QUALITY AND TESTING REQUIREMENTS
            // ============================================================================
            
            // B&T Specific Testing
            migrationBuilder.AddColumn<bool>(
                name: "RequiresBTProofTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<double>(
                name: "ProofTestPressure",
                table: "Parts",
                type: "REAL",
                nullable: true);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresSoundTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresBackPressureTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresThreadVerification",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<string>(
                name: "BTTestingProtocol",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
                
            migrationBuilder.AddColumn<string>(
                name: "BTQualitySpecification",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
                
            // ============================================================================
            // PHASE 5: ADD MANUFACTURING TRACKING FIELDS
            // ============================================================================
            
            // Serial Number and Batch Tracking
            migrationBuilder.AddColumn<string>(
                name: "SerialNumberFormat",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "BT-{YYYY}-{####}");
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresUniqueSerialNumber",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<string>(
                name: "BatchControlMethod",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Standard");
                
            migrationBuilder.AddColumn<int>(
                name: "MaxBatchSize",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresTraceabilityDocuments",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            // Component Genealogy
            migrationBuilder.AddColumn<string>(
                name: "ParentComponents",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "[]");
                
            migrationBuilder.AddColumn<string>(
                name: "ChildComponents",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "[]");
                
            migrationBuilder.AddColumn<bool>(
                name: "IsAssemblyComponent",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "IsSubAssembly",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            // ============================================================================
            // PHASE 6: ADD B&T COSTING AND WORKFLOW FIELDS
            // ============================================================================
            
            // B&T Specific Costing
            migrationBuilder.AddColumn<decimal>(
                name: "BTLicensingCost",
                table: "Parts",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);
                
            migrationBuilder.AddColumn<decimal>(
                name: "ComplianceCost",
                table: "Parts",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);
                
            migrationBuilder.AddColumn<decimal>(
                name: "TestingCost",
                table: "Parts",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);
                
            migrationBuilder.AddColumn<decimal>(
                name: "DocumentationCost",
                table: "Parts",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);
                
            // Workflow Integration
            migrationBuilder.AddColumn<string>(
                name: "WorkflowTemplate",
                table: "Parts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "Standard_SLS");
                
            migrationBuilder.AddColumn<string>(
                name: "ApprovalWorkflow",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "Standard");
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresEngineeringApproval",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresQualityApproval",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            migrationBuilder.AddColumn<bool>(
                name: "RequiresComplianceApproval",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
                
            // ============================================================================
            // PHASE 7: ADD PERFORMANCE INDEXES FOR B&T OPERATIONS
            // ============================================================================
            
            // Manufacturing Stage Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Parts_ManufacturingStage",
                table: "Parts",
                column: "ManufacturingStage");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_StageOrder",
                table: "Parts",
                column: "StageOrder");
                
            // B&T Classification Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTComponentType",
                table: "Parts",
                column: "BTComponentType");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTFirearmCategory",
                table: "Parts",
                column: "BTFirearmCategory");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTSuppressorType",
                table: "Parts",
                column: "BTSuppressorType");
                
            // Compliance Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresATFForm1",
                table: "Parts",
                column: "RequiresATFForm1");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresATFForm4",
                table: "Parts",
                column: "RequiresATFForm4");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresTaxStamp",
                table: "Parts",
                column: "RequiresTaxStamp");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresExportLicense",
                table: "Parts",
                column: "RequiresExportLicense");
                
            // Manufacturing Process Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresSLSPrinting",
                table: "Parts",
                column: "RequiresSLSPrinting");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresCNCMachining",
                table: "Parts",
                column: "RequiresCNCMachining");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresEDMOperations",
                table: "Parts",
                column: "RequiresEDMOperations");
                
            // Quality and Testing Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresBTProofTesting",
                table: "Parts",
                column: "RequiresBTProofTesting");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresSoundTesting",
                table: "Parts",
                column: "RequiresSoundTesting");
                
            // Workflow Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Parts_WorkflowTemplate",
                table: "Parts",
                column: "WorkflowTemplate");
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_ApprovalWorkflow",
                table: "Parts",
                column: "ApprovalWorkflow");
                
            // Composite Indexes for Performance
            migrationBuilder.CreateIndex(
                name: "IX_Parts_BTComponent_Stage",
                table: "Parts",
                columns: new[] { "BTComponentType", "ManufacturingStage" });
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_Compliance_Required",
                table: "Parts",
                columns: new[] { "RequiresATFCompliance", "RequiresITARCompliance", "RequiresSerialization" });
                
            migrationBuilder.CreateIndex(
                name: "IX_Parts_Manufacturing_Process",
                table: "Parts",
                columns: new[] { "RequiresSLSPrinting", "RequiresCNCMachining", "RequiresEDMOperations" });
                
            // ============================================================================
            // PHASE 8: POPULATE DEFAULT B&T DATA
            // ============================================================================
            
            // Update existing parts with B&T defaults based on current data
            migrationBuilder.Sql(@"
                -- Update parts with suppressor-related names to suppressor component type
                UPDATE Parts 
                SET BTComponentType = 'Suppressor',
                    BTFirearmCategory = 'NFA_Item',
                    RequiresATFForm1 = 1,
                    RequiresTaxStamp = 1,
                    TaxStampAmount = 200.00,
                    ATFClassification = 'Silencer',
                    RequiresUniqueSerialNumber = 1,
                    RequiresTraceabilityDocuments = 1
                WHERE Name LIKE '%suppressor%' 
                   OR Name LIKE '%silencer%' 
                   OR Description LIKE '%suppressor%'
                   OR Description LIKE '%silencer%';
            ");
            
            migrationBuilder.Sql(@"
                -- Update parts with baffle-related names
                UPDATE Parts 
                SET BTComponentType = 'Suppressor',
                    BTSuppressorType = 'Baffle',
                    BTBafflePosition = CASE 
                        WHEN Name LIKE '%front%' OR Name LIKE '%first%' THEN 'Front'
                        WHEN Name LIKE '%rear%' OR Name LIKE '%last%' THEN 'Rear'
                        WHEN Name LIKE '%end%' THEN 'End'
                        ELSE 'Middle'
                    END,
                    RequiresATFForm1 = 1,
                    RequiresSoundTesting = 1,
                    RequiresBackPressureTesting = 1
                WHERE Name LIKE '%baffle%' 
                   OR Description LIKE '%baffle%';
            ");
            
            migrationBuilder.Sql(@"
                -- Update parts with receiver-related names
                UPDATE Parts 
                SET BTComponentType = 'Receiver',
                    BTFirearmCategory = 'Firearm',
                    RequiresATFCompliance = 1,
                    RequiresUniqueSerialNumber = 1,
                    ATFClassification = 'Firearm',
                    RequiresTraceabilityDocuments = 1,
                    RequiresEngineeringApproval = 1,
                    RequiresQualityApproval = 1
                WHERE Name LIKE '%receiver%' 
                   OR Name LIKE '%frame%'
                   OR Description LIKE '%receiver%'
                   OR Description LIKE '%frame%';
            ");
            
            migrationBuilder.Sql(@"
                -- Update Titanium parts with specific settings
                UPDATE Parts 
                SET RequiresBTProofTesting = 1,
                    ProofTestPressure = 45000,
                    RequiresThreadVerification = 1,
                    BTTestingProtocol = 'SAAMI Pressure Testing, Thread Gauge Verification',
                    BTQualitySpecification = 'B&T Internal Standard BT-QS-001'
                WHERE Material LIKE '%Ti-6Al-4V%' 
                   OR SlsMaterial LIKE '%Ti-6Al-4V%';
            ");
            
            migrationBuilder.Sql(@"
                -- Update Inconel parts with high-temperature specifications
                UPDATE Parts 
                SET RequiresBTProofTesting = 1,
                    ProofTestPressure = 65000,
                    RequiresSoundTesting = 1,
                    BTTestingProtocol = 'High Pressure Testing, Sound Reduction Verification',
                    BTQualitySpecification = 'B&T Internal Standard BT-QS-002'
                WHERE Material LIKE '%Inconel%' 
                   OR SlsMaterial LIKE '%Inconel%';
            ");
            
            migrationBuilder.Sql(@"
                -- Set default manufacturing stages based on part complexity
                UPDATE Parts 
                SET ManufacturingStage = CASE 
                    WHEN EstimatedHours > 20 THEN 'Multi-Stage'
                    WHEN RequiresCNCMachining = 1 OR RequiresEDMOperations = 1 THEN 'Secondary-Ops'
                    ELSE 'SLS-Primary'
                END,
                StageOrder = CASE 
                    WHEN EstimatedHours > 20 THEN 3
                    WHEN RequiresCNCMachining = 1 OR RequiresEDMOperations = 1 THEN 2
                    ELSE 1
                END,
                WorkflowTemplate = CASE 
                    WHEN BTComponentType = 'Suppressor' THEN 'BT_Suppressor_Workflow'
                    WHEN BTComponentType = 'Receiver' THEN 'BT_Firearm_Workflow'
                    ELSE 'BT_Standard_Workflow'
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all B&T manufacturing indexes
            migrationBuilder.DropIndex(name: "IX_Parts_ManufacturingStage", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_StageOrder", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_BTComponentType", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_BTFirearmCategory", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_BTSuppressorType", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresATFForm1", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresATFForm4", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresTaxStamp", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresExportLicense", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresSLSPrinting", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresCNCMachining", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresEDMOperations", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresBTProofTesting", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_RequiresSoundTesting", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_WorkflowTemplate", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_ApprovalWorkflow", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_BTComponent_Stage", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_Compliance_Required", table: "Parts");
            migrationBuilder.DropIndex(name: "IX_Parts_Manufacturing_Process", table: "Parts");

            // Remove all B&T manufacturing columns
            migrationBuilder.DropColumn(name: "ManufacturingStage", table: "Parts");
            migrationBuilder.DropColumn(name: "StageDetails", table: "Parts");
            migrationBuilder.DropColumn(name: "StageOrder", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresSLSPrinting", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresCNCMachining", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresEDMOperations", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresAssembly", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresFinishing", table: "Parts");
            migrationBuilder.DropColumn(name: "BTComponentType", table: "Parts");
            migrationBuilder.DropColumn(name: "BTFirearmCategory", table: "Parts");
            migrationBuilder.DropColumn(name: "BTSuppressorType", table: "Parts");
            migrationBuilder.DropColumn(name: "BTBafflePosition", table: "Parts");
            migrationBuilder.DropColumn(name: "BTCaliberCompatibility", table: "Parts");
            migrationBuilder.DropColumn(name: "BTThreadPitch", table: "Parts");
            migrationBuilder.DropColumn(name: "BTSoundReductionDB", table: "Parts");
            migrationBuilder.DropColumn(name: "BTBackPressurePSI", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresATFForm1", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresATFForm4", table: "Parts");
            migrationBuilder.DropColumn(name: "ATFClassification", table: "Parts");
            migrationBuilder.DropColumn(name: "FFLRequirements", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresTaxStamp", table: "Parts");
            migrationBuilder.DropColumn(name: "TaxStampAmount", table: "Parts");
            migrationBuilder.DropColumn(name: "ITARCategory", table: "Parts");
            migrationBuilder.DropColumn(name: "EARClassification", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresExportLicense", table: "Parts");
            migrationBuilder.DropColumn(name: "ExportControlNotes", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresBTProofTesting", table: "Parts");
            migrationBuilder.DropColumn(name: "ProofTestPressure", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresSoundTesting", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresBackPressureTesting", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresThreadVerification", table: "Parts");
            migrationBuilder.DropColumn(name: "BTTestingProtocol", table: "Parts");
            migrationBuilder.DropColumn(name: "BTQualitySpecification", table: "Parts");
            migrationBuilder.DropColumn(name: "SerialNumberFormat", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresUniqueSerialNumber", table: "Parts");
            migrationBuilder.DropColumn(name: "BatchControlMethod", table: "Parts");
            migrationBuilder.DropColumn(name: "MaxBatchSize", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresTraceabilityDocuments", table: "Parts");
            migrationBuilder.DropColumn(name: "ParentComponents", table: "Parts");
            migrationBuilder.DropColumn(name: "ChildComponents", table: "Parts");
            migrationBuilder.DropColumn(name: "IsAssemblyComponent", table: "Parts");
            migrationBuilder.DropColumn(name: "IsSubAssembly", table: "Parts");
            migrationBuilder.DropColumn(name: "BTLicensingCost", table: "Parts");
            migrationBuilder.DropColumn(name: "ComplianceCost", table: "Parts");
            migrationBuilder.DropColumn(name: "TestingCost", table: "Parts");
            migrationBuilder.DropColumn(name: "DocumentationCost", table: "Parts");
            migrationBuilder.DropColumn(name: "WorkflowTemplate", table: "Parts");
            migrationBuilder.DropColumn(name: "ApprovalWorkflow", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresEngineeringApproval", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresQualityApproval", table: "Parts");
            migrationBuilder.DropColumn(name: "RequiresComplianceApproval", table: "Parts");
        }
    }
}