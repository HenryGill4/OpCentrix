using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpCentrix.Migrations
{
    /// <inheritdoc />
    public partial class AddBTIndustrySpecialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BTQualityStandards",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTRegulatoryNotes",
                table: "Parts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BTTestingRequirements",
                table: "Parts",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComponentType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExportClassification",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirearmType",
                table: "Parts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsControlledItem",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEARControlled",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PartClassificationId",
                table: "Parts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresATFCompliance",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresDimensionalVerification",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresFFLTracking",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresITARCompliance",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresMaterialCertification",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPressureTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresProofTesting",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSerialization",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSurfaceFinishVerification",
                table: "Parts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PartClassifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassificationCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ClassificationName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IndustryType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Firearms"),
                    ComponentCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SuppressorType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BafflePosition = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsEndCap = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsThreadMount = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTubeHousing = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsInternalComponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMountingHardware = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirearmType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsReceiver = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBarrelComponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsOperatingSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSafetyComponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTriggerComponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFurniture = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecommendedMaterial = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "Ti-6Al-4V Grade 5"),
                    AlternativeMaterials = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    MaterialGrade = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Aerospace"),
                    RequiresSpecialHandling = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiredProcess = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "SLS Metal Printing"),
                    PostProcessingRequired = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    ComplexityLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    RequiresPressureTesting = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresProofTesting = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresDimensionalVerification = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresSurfaceFinishVerification = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresMaterialCertification = table.Column<bool>(type: "INTEGER", nullable: false),
                    TestingRequirements = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    QualityStandards = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    RequiresATFCompliance = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresITARCompliance = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresFFLTracking = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresSerialization = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsControlledItem = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEARControlled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExportClassification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    RegulatoryNotes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartClassifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequirementCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequirementName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ComplianceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RegulatoryAuthority = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequirementDetails = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DocumentationRequired = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    FormsRequired = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    RecordKeepingRequirements = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    ApplicableIndustries = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ApplicablePartTypes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    ApplicableProcesses = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    AppliesToManufacturing = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppliesToDistribution = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppliesToExport = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppliesToImport = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnforcementLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Mandatory"),
                    PenaltyType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    PenaltyDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    MaxPenaltyDays = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxPenaltyAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RenewalIntervalMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiresRenewal = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresInspection = table.Column<bool>(type: "INTEGER", nullable: false),
                    RenewalProcess = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    ImplementationSteps = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    RequiredTraining = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    RequiredCertifications = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    SystemRequirements = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    EstimatedImplementationHours = table.Column<double>(type: "REAL", nullable: false),
                    EstimatedImplementationCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ReferenceDocuments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    WebResources = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    ContactInformation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    AdditionalNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValue: ""),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "INTEGER", nullable: false),
                    PartClassificationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceRequirements_PartClassifications_PartClassificationId",
                        column: x => x.PartClassificationId,
                        principalTable: "PartClassifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SerialNumberValue = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SerialNumberFormat = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ManufacturerCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "BT"),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ManufacturedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssignedJobId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ComponentName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ComponentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ManufacturingMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "SLS Metal Printing"),
                    MaterialUsed = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    MaterialLotNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    MachineUsed = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    Operator = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    QualityInspector = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ATFComplianceStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ATFClassification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    FFLDealer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FFLNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ATFFormSubmissionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ATFApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ATFFormNumbers = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    TaxStampNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TransferStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "In Manufacturing"),
                    TransferDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TransferTo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TransferDocument = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TransferNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsDestructionScheduled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScheduledDestructionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualDestructionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DestructionMethod = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsITARControlled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEARControlled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExportClassification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    ExportLicense = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ExportLicenseExpiration = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DestinationCountry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EndUser = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RequiresExportPermit = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExportPermitObtained = table.Column<bool>(type: "INTEGER", nullable: false),
                    QualityStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    QualityInspectionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QualityCertificateNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TestResultsSummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    DimensionalTestPassed = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaterialTestPassed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PressureTestPassed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProofTestPassed = table.Column<bool>(type: "INTEGER", nullable: false),
                    QualityNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValue: ""),
                    ManufacturingHistory = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: "{}"),
                    ComponentGenealogy = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValue: ""),
                    AssemblyComponents = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BuildPlatformId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    PartId = table.Column<int>(type: "INTEGER", nullable: true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: true),
                    ComplianceRequirementId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialNumbers_ComplianceRequirements_ComplianceRequirementId",
                        column: x => x.ComplianceRequirementId,
                        principalTable: "ComplianceRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SerialNumbers_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SerialNumbers_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocumentTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ComplianceCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DocumentClassification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Unclassified"),
                    RegulatoryAuthority = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    FormNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    ApprovalNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FileSizeMB = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DocumentContent = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, defaultValue: ""),
                    AssociatedSerialNumbers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    AssociatedPartNumbers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    AssociatedJobNumbers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    Customer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PreparedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    ReviewedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovalDateInternal = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewComments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    ApprovalComments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false, defaultValue: ""),
                    RetentionPeriod = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Permanent"),
                    RetentionEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArchiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DisposalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArchiveLocation = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DisposalMethod = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDisposed = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresRenewal = table.Column<bool>(type: "INTEGER", nullable: false),
                    RenewalReminderDays = table.Column<int>(type: "INTEGER", nullable: false),
                    NextReminderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EmailNotificationSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastNotificationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NotificationRecipients = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: ""),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    LastAccessedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastAccessedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: ""),
                    AccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuditNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValue: ""),
                    SerialNumberId = table.Column<int>(type: "INTEGER", nullable: true),
                    ComplianceRequirementId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartId = table.Column<int>(type: "INTEGER", nullable: true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceDocuments_ComplianceRequirements_ComplianceRequirementId",
                        column: x => x.ComplianceRequirementId,
                        principalTable: "ComplianceRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceDocuments_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceDocuments_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceDocuments_SerialNumbers_SerialNumberId",
                        column: x => x.SerialNumberId,
                        principalTable: "SerialNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ComponentType",
                table: "Parts",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_FirearmType",
                table: "Parts",
                column: "FirearmType");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartClassificationId",
                table: "Parts",
                column: "PartClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresATFCompliance",
                table: "Parts",
                column: "RequiresATFCompliance");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresITARCompliance",
                table: "Parts",
                column: "RequiresITARCompliance");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_RequiresSerialization",
                table: "Parts",
                column: "RequiresSerialization");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_ComplianceCategory",
                table: "ComplianceDocuments",
                column: "ComplianceCategory");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_ComplianceRequirementId",
                table: "ComplianceDocuments",
                column: "ComplianceRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_DocumentDate",
                table: "ComplianceDocuments",
                column: "DocumentDate");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_DocumentNumber",
                table: "ComplianceDocuments",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_DocumentType",
                table: "ComplianceDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_EffectiveDate",
                table: "ComplianceDocuments",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_ExpirationDate",
                table: "ComplianceDocuments",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_IsActive",
                table: "ComplianceDocuments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_IsArchived",
                table: "ComplianceDocuments",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_JobId",
                table: "ComplianceDocuments",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_PartId",
                table: "ComplianceDocuments",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_SerialNumberId",
                table: "ComplianceDocuments",
                column: "SerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceDocuments_Status",
                table: "ComplianceDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_ComplianceType",
                table: "ComplianceRequirements",
                column: "ComplianceType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_EffectiveDate",
                table: "ComplianceRequirements",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_EnforcementLevel",
                table: "ComplianceRequirements",
                column: "EnforcementLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_ExpirationDate",
                table: "ComplianceRequirements",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_IsActive",
                table: "ComplianceRequirements",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_IsCurrentVersion",
                table: "ComplianceRequirements",
                column: "IsCurrentVersion");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_PartClassificationId",
                table: "ComplianceRequirements",
                column: "PartClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_RegulatoryAuthority",
                table: "ComplianceRequirements",
                column: "RegulatoryAuthority");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRequirements_RequirementCode",
                table: "ComplianceRequirements",
                column: "RequirementCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_ClassificationCode",
                table: "PartClassifications",
                column: "ClassificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_ClassificationName",
                table: "PartClassifications",
                column: "ClassificationName");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_ComponentCategory",
                table: "PartClassifications",
                column: "ComponentCategory");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_FirearmType",
                table: "PartClassifications",
                column: "FirearmType");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_IndustryType",
                table: "PartClassifications",
                column: "IndustryType");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_IsActive",
                table: "PartClassifications",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_RequiresATFCompliance",
                table: "PartClassifications",
                column: "RequiresATFCompliance");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_RequiresITARCompliance",
                table: "PartClassifications",
                column: "RequiresITARCompliance");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_RequiresSerialization",
                table: "PartClassifications",
                column: "RequiresSerialization");

            migrationBuilder.CreateIndex(
                name: "IX_PartClassifications_SuppressorType",
                table: "PartClassifications",
                column: "SuppressorType");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_AssignedDate",
                table: "SerialNumbers",
                column: "AssignedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ATFComplianceStatus",
                table: "SerialNumbers",
                column: "ATFComplianceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ComplianceRequirementId",
                table: "SerialNumbers",
                column: "ComplianceRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ComponentType",
                table: "SerialNumbers",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_IsActive",
                table: "SerialNumbers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_IsLocked",
                table: "SerialNumbers",
                column: "IsLocked");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_JobId",
                table: "SerialNumbers",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ManufacturedDate",
                table: "SerialNumbers",
                column: "ManufacturedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_ManufacturerCode",
                table: "SerialNumbers",
                column: "ManufacturerCode");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_PartId",
                table: "SerialNumbers",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_QualityStatus",
                table: "SerialNumbers",
                column: "QualityStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_SerialNumberValue",
                table: "SerialNumbers",
                column: "SerialNumberValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_TransferStatus",
                table: "SerialNumbers",
                column: "TransferStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_PartClassifications_PartClassificationId",
                table: "Parts",
                column: "PartClassificationId",
                principalTable: "PartClassifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_PartClassifications_PartClassificationId",
                table: "Parts");

            migrationBuilder.DropTable(
                name: "ComplianceDocuments");

            migrationBuilder.DropTable(
                name: "SerialNumbers");

            migrationBuilder.DropTable(
                name: "ComplianceRequirements");

            migrationBuilder.DropTable(
                name: "PartClassifications");

            migrationBuilder.DropIndex(
                name: "IX_Parts_ComponentType",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_FirearmType",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartClassificationId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_RequiresATFCompliance",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_RequiresITARCompliance",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_RequiresSerialization",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTQualityStandards",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTRegulatoryNotes",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "BTTestingRequirements",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ComponentType",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ExportClassification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "FirearmType",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "IsControlledItem",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "IsEARControlled",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartClassificationId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresATFCompliance",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresDimensionalVerification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresFFLTracking",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresITARCompliance",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresMaterialCertification",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresPressureTesting",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresProofTesting",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresSerialization",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RequiresSurfaceFinishVerification",
                table: "Parts");
        }
    }
}
