using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{   
    /// <summary>
    /// Enhanced Part model for B&T Manufacturing - Supports firearms, suppressors, and complex manufacturing workflows
    /// Includes manufacturing stages, regulatory compliance, and specialized B&T requirements
    /// </summary>
    public class Part
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Material { get; set; } = "Ti-6Al-4V Grade 5";
        
        #region B&T Manufacturing Stages - NEW SECTION
        
        /// <summary>
        /// Current manufacturing stage: Design, SLS-Primary, Secondary-Ops, Multi-Stage, Assembly, Finishing, Complete
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Manufacturing Stage")]
        public string ManufacturingStage { get; set; } = "Design";
        
        /// <summary>
        /// JSON object containing stage-specific details and parameters
        /// </summary>
        [Required]
        [StringLength(500)]
        [Display(Name = "Stage Details")]
        public string StageDetails { get; set; } = "{}";
        
        /// <summary>
        /// Execution order in multi-stage workflows (1-10)
        /// </summary>
        [Range(1, 10)]
        [Display(Name = "Stage Order")]
        public int StageOrder { get; set; } = 1;
        
        /// <summary>
        /// Requires SLS metal printing (primary manufacturing)
        /// </summary>
        [Display(Name = "Requires SLS Printing")]
        public bool RequiresSLSPrinting { get; set; } = true;
        
        /// <summary>
        /// Requires CNC machining (secondary operations)
        /// </summary>
        [Display(Name = "Requires CNC Machining")]
        public bool RequiresCNCMachining { get; set; } = false;
        
        /// <summary>
        /// Requires EDM operations (complex geometries)
        /// </summary>
        [Display(Name = "Requires EDM Operations")]
        public bool RequiresEDMOperations { get; set; } = false;
        
        /// <summary>
        /// Requires assembly operations (multi-component parts)
        /// </summary>
        [Display(Name = "Requires Assembly")]
        public bool RequiresAssembly { get; set; } = false;
        
        /// <summary>
        /// Requires finishing operations (Cerakote, anodizing, etc.)
        /// </summary>
        [Display(Name = "Requires Finishing")]
        public bool RequiresFinishing { get; set; } = false;
        
        #endregion
        
        #region B&T Firearms Classification - NEW SECTION
        
        /// <summary>
        /// B&T component type: Suppressor, Receiver, Barrel, Trigger, Safety, General
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "B&T Component Type")]
        public string BTComponentType { get; set; } = "General";
        
        /// <summary>
        /// Firearm category: Firearm, NFA_Item, Component, Accessory
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Firearm Category")]
        public string BTFirearmCategory { get; set; } = "Component";
        
        /// <summary>
        /// Suppressor type: Baffle, EndCap, Tube, Mount, Internal
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Suppressor Type")]
        public string BTSuppressorType { get; set; } = "";
        
        /// <summary>
        /// Baffle position: Front, Middle, Rear, End
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Baffle Position")]
        public string BTBafflePosition { get; set; } = "";
        
        /// <summary>
        /// Compatible calibers (comma-separated): .223, .308, 9mm, etc.
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Caliber Compatibility")]
        public string BTCaliberCompatibility { get; set; } = "";
        
        /// <summary>
        /// Thread pitch specification: 1/2-28, 5/8-24, M14x1, etc.
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Thread Pitch")]
        public string BTThreadPitch { get; set; } = "";
        
        /// <summary>
        /// Sound reduction capability in decibels
        /// </summary>
        [Range(0, 50)]
        [Display(Name = "Sound Reduction (dB)")]
        public double? BTSoundReductionDB { get; set; }
        
        /// <summary>
        /// Back pressure measurement in PSI
        /// </summary>
        [Range(0, 10000)]
        [Display(Name = "Back Pressure (PSI)")]
        public double? BTBackPressurePSI { get; set; }
        
        #endregion
        
        #region Regulatory Compliance - ENHANCED SECTION
        
        // ATF/FFL Compliance
        public bool RequiresATFCompliance { get; set; } = false;
        public bool RequiresITARCompliance { get; set; } = false;
        public bool RequiresFFLTracking { get; set; } = false;
        public bool RequiresSerialization { get; set; } = false;
        public bool IsControlledItem { get; set; } = false;
        public bool IsEARControlled { get; set; } = false;
        
        /// <summary>
        /// Requires ATF Form 1 (Make/Manufacture)
        /// </summary>
        [Display(Name = "Requires ATF Form 1")]
        public bool RequiresATFForm1 { get; set; } = false;
        
        /// <summary>
        /// Requires ATF Form 4 (Transfer)
        /// </summary>
        [Display(Name = "Requires ATF Form 4")]
        public bool RequiresATFForm4 { get; set; } = false;
        
        /// <summary>
        /// ATF classification: Firearm, Silencer, SBR, SBS, AOW, etc.
        /// </summary>
        [StringLength(100)]
        [Display(Name = "ATF Classification")]
        public string ATFClassification { get; set; } = "";
        
        /// <summary>
        /// FFL requirements and restrictions
        /// </summary>
        [StringLength(200)]
        [Display(Name = "FFL Requirements")]
        public string FFLRequirements { get; set; } = "";
        
        /// <summary>
        /// Requires federal tax stamp
        /// </summary>
        [Display(Name = "Requires Tax Stamp")]
        public bool RequiresTaxStamp { get; set; } = false;
        
        /// <summary>
        /// Tax stamp amount (typically $200 for NFA items)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Tax Stamp Amount")]
        public decimal? TaxStampAmount { get; set; }
        
        // ITAR/EAR Export Control
        [StringLength(50)]
        [Display(Name = "ITAR Category")]
        public string ITARCategory { get; set; } = "";
        
        [StringLength(50)]
        [Display(Name = "EAR Classification")]
        public string EARClassification { get; set; } = "";
        
        [Display(Name = "Requires Export License")]
        public bool RequiresExportLicense { get; set; } = false;
        
        [StringLength(500)]
        [Display(Name = "Export Control Notes")]
        public string ExportControlNotes { get; set; } = "";
        
        [StringLength(50)]
        [Display(Name = "Export Classification")]
        public string ExportClassification { get; set; } = string.Empty;
        
        [StringLength(50)]
        [Display(Name = "Component Type")]
        public string ComponentType { get; set; } = string.Empty;
        
        [StringLength(50)]
        [Display(Name = "Firearm Type")]
        public string FirearmType { get; set; } = string.Empty;
        
        #endregion
        
        #region B&T Quality and Testing - NEW SECTION
        
        /// <summary>
        /// Requires B&T proof testing protocol
        /// </summary>
        [Display(Name = "Requires B&T Proof Testing")]
        public bool RequiresBTProofTesting { get; set; } = false;
        
        /// <summary>
        /// Proof test pressure in PSI
        /// </summary>
        [Range(0, 100000)]
        [Display(Name = "Proof Test Pressure (PSI)")]
        public double? ProofTestPressure { get; set; }
        
        /// <summary>
        /// Requires sound reduction testing
        /// </summary>
        [Display(Name = "Requires Sound Testing")]
        public bool RequiresSoundTesting { get; set; } = false;
        
        /// <summary>
        /// Requires back pressure testing
        /// </summary>
        [Display(Name = "Requires Back Pressure Testing")]
        public bool RequiresBackPressureTesting { get; set; } = false;
        
        /// <summary>
        /// Requires thread pitch verification
        /// </summary>
        [Display(Name = "Requires Thread Verification")]
        public bool RequiresThreadVerification { get; set; } = false;
        
        /// <summary>
        /// B&T specific testing protocol
        /// </summary>
        [StringLength(500)]
        [Display(Name = "B&T Testing Protocol")]
        public string BTTestingProtocol { get; set; } = "";
        
        /// <summary>
        /// B&T quality specification reference
        /// </summary>
        [StringLength(500)]
        [Display(Name = "B&T Quality Specification")]
        public string BTQualitySpecification { get; set; } = "";
        
        // Existing testing requirements
        public bool RequiresPressureTesting { get; set; } = false;
        public bool RequiresProofTesting { get; set; } = false;
        public bool RequiresDimensionalVerification { get; set; } = true;
        public bool RequiresSurfaceFinishVerification { get; set; } = true;
        public bool RequiresMaterialCertification { get; set; } = true;
        
        [StringLength(500)]
        [Display(Name = "B&T Testing Requirements")]
        public string BTTestingRequirements { get; set; } = string.Empty;
        
        [StringLength(500)]
        [Display(Name = "B&T Quality Standards")]
        public string BTQualityStandards { get; set; } = string.Empty;
        
        [StringLength(200)]
        [Display(Name = "B&T Regulatory Notes")]
        public string BTRegulatoryNotes { get; set; } = string.Empty;
        
        #endregion
        
        #region Manufacturing Tracking - NEW SECTION
        
        /// <summary>
        /// Serial number format template: BT-{YYYY}-{####}
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Serial Number Format")]
        public string SerialNumberFormat { get; set; } = "BT-{YYYY}-{####}";
        
        /// <summary>
        /// Requires unique serial number per component
        /// </summary>
        [Display(Name = "Requires Unique Serial Number")]
        public bool RequiresUniqueSerialNumber { get; set; } = false;
        
        /// <summary>
        /// Batch control method: Standard, Lot, Heat, Individual
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Batch Control Method")]
        public string BatchControlMethod { get; set; } = "Standard";
        
        /// <summary>
        /// Maximum batch size for manufacturing
        /// </summary>
        [Range(1, 1000)]
        [Display(Name = "Max Batch Size")]
        public int MaxBatchSize { get; set; } = 1;
        
        /// <summary>
        /// Requires complete traceability documentation
        /// </summary>
        [Display(Name = "Requires Traceability Documents")]
        public bool RequiresTraceabilityDocuments { get; set; } = false;
        
        /// <summary>
        /// Parent components in assembly (JSON array)
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Parent Components")]
        public string ParentComponents { get; set; } = "[]";
        
        /// <summary>
        /// Child components in assembly (JSON array)
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Child Components")]
        public string ChildComponents { get; set; } = "[]";
        
        /// <summary>
        /// This part is used in assemblies
        /// </summary>
        [Display(Name = "Is Assembly Component")]
        public bool IsAssemblyComponent { get; set; } = false;
        
        /// <summary>
        /// This part is a sub-assembly
        /// </summary>
        [Display(Name = "Is Sub-Assembly")]
        public bool IsSubAssembly { get; set; } = false;
        
        #endregion
        
        #region B&T Costing - NEW SECTION
        
        /// <summary>
        /// B&T licensing and regulatory costs
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "B&T Licensing Cost")]
        public decimal BTLicensingCost { get; set; } = 0.00m;
        
        /// <summary>
        /// Compliance and regulatory costs
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Compliance Cost")]
        public decimal ComplianceCost { get; set; } = 0.00m;
        
        /// <summary>
        /// B&T specific testing costs
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Testing Cost")]
        public decimal TestingCost { get; set; } = 0.00m;
        
        /// <summary>
        /// Documentation and paperwork costs
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Documentation Cost")]
        public decimal DocumentationCost { get; set; } = 0.00m;
        
        #endregion
        
        #region Workflow Integration - NEW SECTION
        
        /// <summary>
        /// Workflow template: BT_Suppressor_Workflow, BT_Firearm_Workflow, BT_Standard_Workflow
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Workflow Template")]
        public string WorkflowTemplate { get; set; } = "BT_Standard_Workflow";
        
        /// <summary>
        /// Approval workflow: Standard, Complex, Regulated
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Approval Workflow")]
        public string ApprovalWorkflow { get; set; } = "Standard";
        
        /// <summary>
        /// Requires engineering approval before manufacturing
        /// </summary>
        [Display(Name = "Requires Engineering Approval")]
        public bool RequiresEngineeringApproval { get; set; } = false;
        
        /// <summary>
        /// Requires quality approval before release
        /// </summary>
        [Display(Name = "Requires Quality Approval")]
        public bool RequiresQualityApproval { get; set; } = false;
        
        /// <summary>
        /// Requires compliance approval for regulated items
        /// </summary>
        [Display(Name = "Requires Compliance Approval")]
        public bool RequiresComplianceApproval { get; set; } = false;
        
        #endregion
        
        #region SLS-Specific Manufacturing Properties - EXISTING
        
        [Required]
        [StringLength(100)]
        public string SlsMaterial { get; set; } = "Ti-6Al-4V Grade 5";
        
        [Required]
        [StringLength(100)]
        public string PowderSpecification { get; set; } = "15-45 micron particle size";
        
        [Range(0, 50)]
        public double PowderRequirementKg { get; set; } = 0.5;
        
        [Range(0, 2000)]
        public double RecommendedLaserPower { get; set; } = 200;
        
        [Range(0, 5000)]
        public double RecommendedScanSpeed { get; set; } = 1200;
        
        [Range(10, 100)]
        public double RecommendedLayerThickness { get; set; } = 30;
        
        [Range(50, 200)]
        public double RecommendedHatchSpacing { get; set; } = 120;
        
        [Range(0, 500)]
        public double RecommendedBuildTemperature { get; set; } = 180;
        
        [Range(99.0, 100.0)]
        public double RequiredArgonPurity { get; set; } = 99.9;
        
        [Range(0, 100)]
        public double MaxOxygenContent { get; set; } = 50;
        
        #endregion
        
        #region Physical Properties - EXISTING
        
        [Range(0, 1000)]
        public double WeightGrams { get; set; } = 0;
        
        [Required]
        [StringLength(100)]
        public string Dimensions { get; set; } = string.Empty;
        
        [Range(0, 1000000)]
        public double VolumeMm3 { get; set; } = 0;
        
        [Range(0, 300)]
        public double HeightMm { get; set; } = 0;
        
        [Range(0, 250)]
        public double LengthMm { get; set; } = 0;
        
        [Range(0, 250)]
        public double WidthMm { get; set; } = 0;
        
        [Required]
        [StringLength(100)]
        public string SurfaceFinishRequirement { get; set; } = "As-built";
        
        [Range(0, 100)]
        public double MaxSurfaceRoughnessRa { get; set; } = 25;
        
        #endregion
        
        #region Cost Data Enhanced for B&T
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal MaterialCostPerKg { get; set; } = 450.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal StandardLaborCostPerHour { get; set; } = 85.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SetupCost { get; set; } = 150.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal PostProcessingCost { get; set; } = 75.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal QualityInspectionCost { get; set; } = 50.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MachineOperatingCostPerHour { get; set; } = 125.00m;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal ArgonCostPerHour { get; set; } = 15.00m;
        
        #endregion
        
        #region Manufacturing Data Enhanced for B&T
        
        [Required]
        [StringLength(50)]
        public string ProcessType { get; set; } = "SLS Metal";
        
        [Required]
        [StringLength(100)]
        public string RequiredMachineType { get; set; } = "TruPrint 3000";
        
        [Required]
        [StringLength(200)]
        public string PreferredMachines { get; set; } = "TI1,TI2";
        
        [Range(0, 300)]
        public double SetupTimeMinutes { get; set; } = 45;
        
        [Range(0, 180)]
        public double PowderChangeoverTimeMinutes { get; set; } = 30;
        
        [Range(0, 300)]
        public double PreheatingTimeMinutes { get; set; } = 60;
        
        [Range(0, 600)]
        public double CoolingTimeMinutes { get; set; } = 240;
        
        [Range(0, 180)]
        public double PostProcessingTimeMinutes { get; set; } = 45;
        
        [Required]
        [StringLength(500)]
        public string QualityStandards { get; set; } = "ASTM F3001, ISO 17296";
        
        [Required]
        [StringLength(500)]
        public string ToleranceRequirements { get; set; } = "±0.1mm typical, ±0.05mm critical dimensions";
        
        public bool RequiresInspection { get; set; } = true;
        public bool RequiresCertification { get; set; } = false;
        
        #endregion
        
        #region SLS Resource Requirements - EXISTING
        
        [Required]
        [StringLength(500)]
        public string RequiredSkills { get; set; } = "SLS Operation,Powder Handling,Inert Gas Safety,Post-Processing";
        
        [Required]
        [StringLength(500)]
        public string RequiredCertifications { get; set; } = "SLS Operation Certification,Powder Safety Training";
        
        [Required]
        [StringLength(500)]
        public string RequiredTooling { get; set; } = "Build Platform,Powder Sieve,Support Removal Tools";
        
        [Required]
        [StringLength(500)]
        public string ConsumableMaterials { get; set; } = "Argon Gas,Build Platform Coating";
        
        public bool RequiresSupports { get; set; } = false;
        
        [Required]
        [StringLength(200)]
        public string SupportStrategy { get; set; } = "Minimal supports on overhangs > 45°";
        
        [Range(0, 60)]
        public double SupportRemovalTimeMinutes { get; set; } = 0;
        
        #endregion
        
        #region Customer and Classification Enhanced for B&T
        
        [Required]
        [StringLength(100)]
        public string CustomerPartNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string PartCategory { get; set; } = "Prototype";
        
        [Required]
        [StringLength(10)]
        public string PartClass { get; set; } = "B";
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        [StringLength(100)]
        public string Industry { get; set; } = "Firearms";
        
        [Required]
        [StringLength(100)]
        public string Application { get; set; } = "B&T Manufacturing";
        
        public bool RequiresFDA { get; set; } = false;
        public bool RequiresAS9100 { get; set; } = false;
        public bool RequiresNADCAP { get; set; } = false;
        
        #endregion
        
        #region Historical Performance Data Enhanced - EXISTING
        
        public double AverageActualHours { get; set; } = 0;
        
        [Range(0, 200)]
        public double AverageEfficiencyPercent { get; set; } = 100;
        
        [Range(0, 100)]
        public double AverageQualityScore { get; set; } = 100;
        
        [Range(0, 50)]
        public double AverageDefectRate { get; set; } = 0;
        
        [Range(0, 100)]
        public double AveragePowderUtilization { get; set; } = 85;
        
        public int TotalJobsCompleted { get; set; } = 0;
        public int TotalUnitsProduced { get; set; } = 0;
        public DateTime? LastProduced { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal AverageCostPerUnit { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal StandardSellingPrice { get; set; } = 0;
        
        #endregion
        
        #region Process Parameters Enhanced for B&T - EXISTING
        
        [Required]
        [StringLength(2000)]
        public string ProcessParameters { get; set; } = "{}";
        
        [Required]
        [StringLength(2000)]
        public string QualityCheckpoints { get; set; } = "{}";
        
        [Required]
        [StringLength(255)]
        public string BuildFileTemplate { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string CadFilePath { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string CadFileVersion { get; set; } = string.Empty;
        
        #endregion
        
        #region Audit Trail - EXISTING
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;
        
        #endregion
        
        #region Duration and Time Management Enhanced for B&T - EXISTING
        
        [Required]
        [StringLength(50)]
        public string AvgDuration { get; set; } = "8h 0m";
        public int AvgDurationDays { get; set; } = 1;
        
        [Range(0.1, 200.0)]
        public double EstimatedHours { get; set; } = 8.0;
        
        [Range(0.1, 200.0)]
        public double? AdminEstimatedHoursOverride { get; set; }
        
        [StringLength(500)]
        public string? AdminOverrideReason { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string AdminOverrideBy { get; set; } = string.Empty;
        
        public DateTime? AdminOverrideDate { get; set; }
        
        public bool HasAdminOverride => AdminEstimatedHoursOverride.HasValue;
        
        [NotMapped]
        public double EffectiveDurationHours => AdminEstimatedHoursOverride ?? EstimatedHours;
        
        [NotMapped]
        public string EffectiveDurationDisplay => HasAdminOverride 
            ? $"{EffectiveDurationHours:F1}h (Override)" 
            : $"{EstimatedHours:F1}h";
        
        /// <summary>
        /// Get the effective estimated hours considering admin override
        /// Used by scheduler for pre-populating job forms
        /// </summary>
        /// <returns>Admin override hours if set, otherwise standard estimated hours</returns>
        public double GetEffectiveEstimatedHours()
        {
            return AdminEstimatedHoursOverride ?? EstimatedHours;
        }
        
        #endregion
        
        #region B&T Industry Specialization - Segment 7 EXISTING
        
        public int? PartClassificationId { get; set; }
        
        #endregion
        
        #region Navigation Properties Enhanced for B&T
        
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        public virtual ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();
        public virtual ICollection<InspectionCheckpoint> InspectionCheckpoints { get; set; } = new List<InspectionCheckpoint>();
        public virtual PartClassification? PartClassification { get; set; }
        public virtual ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
        public virtual ICollection<ComplianceDocument> ComplianceDocuments { get; set; } = new List<ComplianceDocument>();
        
        #endregion
        
        #region B&T Computed Properties - NEW SECTION
        
        /// <summary>
        /// Total B&T compliance cost including all regulatory fees
        /// </summary>
        [NotMapped]
        public decimal TotalBTComplianceCost => 
            BTLicensingCost + ComplianceCost + TestingCost + DocumentationCost + (TaxStampAmount ?? 0);
        
        /// <summary>
        /// Enhanced cost calculation including B&T compliance costs
        /// </summary>
        [NotMapped]
        public decimal EstimatedTotalCostPerUnitBT => 
            ((decimal)PowderRequirementKg * MaterialCostPerKg) + 
            (StandardLaborCostPerHour * (decimal)EstimatedHours) + 
            (MachineOperatingCostPerHour * (decimal)EstimatedHours) +
            (ArgonCostPerHour * (decimal)EstimatedHours) +
            SetupCost + 
            PostProcessingCost + 
            QualityInspectionCost +
            TotalBTComplianceCost;
        
        /// <summary>
        /// Manufacturing complexity score including B&T factors
        /// </summary>
        [NotMapped]
        public string BTComplexityLevel
        {
            get
            {
                var score = 0;
                
                // Time complexity
                if (EstimatedHours > 24) score += 3;
                else if (EstimatedHours > 8) score += 2;
                else if (EstimatedHours > 2) score += 1;
                
                // Manufacturing stage complexity
                if (RequiresSLSPrinting) score += 1;
                if (RequiresCNCMachining) score += 2;
                if (RequiresEDMOperations) score += 3;
                if (RequiresAssembly) score += 2;
                if (RequiresFinishing) score += 1;
                
                // B&T compliance complexity
                if (RequiresATFForm1 || RequiresATFForm4) score += 3;
                if (RequiresTaxStamp) score += 2;
                if (RequiresExportLicense) score += 4;
                if (RequiresUniqueSerialNumber) score += 2;
                
                // Testing complexity
                if (RequiresBTProofTesting) score += 2;
                if (RequiresSoundTesting) score += 2;
                if (RequiresBackPressureTesting) score += 2;
                
                return score switch
                {
                    <= 3 => "Simple",
                    <= 7 => "Medium", 
                    <= 12 => "Complex",
                    <= 18 => "Very Complex",
                    _ => "Highly Complex"
                };
            }
        }
        
        /// <summary>
        /// Get manufacturing stage summary
        /// </summary>
        [NotMapped]
        public string ManufacturingStageSummary
        {
            get
            {
                var stages = new List<string>();
                if (RequiresSLSPrinting) stages.Add("SLS");
                if (RequiresCNCMachining) stages.Add("CNC");
                if (RequiresEDMOperations) stages.Add("EDM");
                if (RequiresAssembly) stages.Add("Assembly");
                if (RequiresFinishing) stages.Add("Finishing");
                
                return stages.Count > 0 ? string.Join(" → ", stages) : "Design Only";
            }
        }
        
        /// <summary>
        /// Get compliance summary for display
        /// </summary>
        [NotMapped]
        public string BTComplianceSummary
        {
            get
            {
                var requirements = new List<string>();
                if (RequiresATFForm1) requirements.Add("ATF Form 1");
                if (RequiresATFForm4) requirements.Add("ATF Form 4");
                if (RequiresTaxStamp) requirements.Add("Tax Stamp");
                if (RequiresExportLicense) requirements.Add("Export License");
                if (RequiresUniqueSerialNumber) requirements.Add("Serial Number");
                
                return requirements.Count > 0 ? string.Join(", ", requirements) : "No Special Requirements";
            }
        }
        
        /// <summary>
        /// Check if this is a B&T regulated component
        /// </summary>
        public bool IsBTRegulatedComponent()
        {
            return RequiresATFForm1 || RequiresATFForm4 || RequiresTaxStamp || 
                   RequiresExportLicense || RequiresATFCompliance || RequiresITARCompliance;
        }
        
        /// <summary>
        /// Check if this is a suppressor component
        /// </summary>
        public bool IsSuppressorComponent()
        {
            return BTComponentType.Equals("Suppressor", StringComparison.OrdinalIgnoreCase) ||
                   !string.IsNullOrEmpty(BTSuppressorType) ||
                   Name.Contains("Suppressor", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("Silencer", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is a firearm component
        /// </summary>
        public bool IsFirearmComponent()
        {
            return BTComponentType.Equals("Receiver", StringComparison.OrdinalIgnoreCase) ||
                   BTFirearmCategory.Equals("Firearm", StringComparison.OrdinalIgnoreCase) ||
                   !string.IsNullOrEmpty(FirearmType);
        }
        
        /// <summary>
        /// Get recommended workflow based on component type and complexity
        /// </summary>
        public string GetRecommendedWorkflow()
        {
            if (IsSuppressorComponent()) return "BT_Suppressor_Workflow";
            if (IsFirearmComponent()) return "BT_Firearm_Workflow";
            if (IsBTRegulatedComponent()) return "BT_Regulated_Workflow";
            return "BT_Standard_Workflow";
        }
        
        #endregion
        
        #region Helper Methods for B&T Operations - ENHANCED
        
        /// <summary>
        /// Get all required B&T manufacturing stages
        /// </summary>
        public List<string> GetRequiredManufacturingStages()
        {
            var stages = new List<string>();
            
            if (RequiresSLSPrinting) stages.Add("SLS Printing");
            if (RequiresCNCMachining) stages.Add("CNC Machining");
            if (RequiresEDMOperations) stages.Add("EDM Operations");
            if (RequiresAssembly) stages.Add("Assembly");
            if (RequiresFinishing) stages.Add("Finishing");
            
            return stages;
        }
        
        /// <summary>
        /// Get all required B&T testing types
        /// </summary>
        public List<string> GetRequiredBTTesting()
        {
            var testing = new List<string>();
            
            if (RequiresBTProofTesting) testing.Add($"Proof Testing ({ProofTestPressure:F0} PSI)");
            if (RequiresSoundTesting) testing.Add("Sound Reduction Testing");
            if (RequiresBackPressureTesting) testing.Add("Back Pressure Testing");
            if (RequiresThreadVerification) testing.Add("Thread Verification");
            if (RequiresPressureTesting) testing.Add("Pressure Testing");
            if (RequiresDimensionalVerification) testing.Add("Dimensional Verification");
            if (RequiresSurfaceFinishVerification) testing.Add("Surface Finish");
            if (RequiresMaterialCertification) testing.Add("Material Certification");
            
            return testing;
        }
        
        /// <summary>
        /// Get all required B&T compliance types
        /// </summary>
        public List<string> GetRequiredBTCompliance()
        {
            var compliance = new List<string>();
            
            if (RequiresATFForm1) compliance.Add("ATF Form 1");
            if (RequiresATFForm4) compliance.Add("ATF Form 4");
            if (RequiresTaxStamp) compliance.Add($"Tax Stamp (${TaxStampAmount:F2})");
            if (RequiresExportLicense) compliance.Add("Export License");
            if (RequiresATFCompliance) compliance.Add("ATF Compliance");
            if (RequiresITARCompliance) compliance.Add("ITAR Compliance");
            if (RequiresFFLTracking) compliance.Add("FFL Tracking");
            if (RequiresSerialization) compliance.Add("Serialization");
            if (IsControlledItem) compliance.Add("Controlled Item");
            if (IsEARControlled) compliance.Add("EAR Controlled");
            
            return compliance;
        }
        
        /// <summary>
        /// Calculate total manufacturing time including all stages
        /// </summary>
        public double CalculateTotalManufacturingTime()
        {
            var totalMinutes = 0.0;
            
            // Setup and prep time
            totalMinutes += SetupTimeMinutes;
            totalMinutes += PowderChangeoverTimeMinutes;
            totalMinutes += PreheatingTimeMinutes;
            
            // Main manufacturing time
            totalMinutes += EstimatedHours * 60;
            
            // Post-processing time
            totalMinutes += CoolingTimeMinutes;
            totalMinutes += PostProcessingTimeMinutes;
            totalMinutes += SupportRemovalTimeMinutes;
            
            // Additional stage times (estimated)
            if (RequiresCNCMachining) totalMinutes += EstimatedHours * 60 * 0.3; // 30% of SLS time
            if (RequiresEDMOperations) totalMinutes += EstimatedHours * 60 * 0.5; // 50% of SLS time
            if (RequiresAssembly) totalMinutes += 120; // 2 hours assembly time
            if (RequiresFinishing) totalMinutes += 180; // 3 hours finishing time
            
            return Math.Round(totalMinutes / 60.0, 2); // Return in hours
        }
        
        /// <summary>
        /// Validate B&T manufacturing requirements
        /// </summary>
        public List<string> ValidateBTManufacturingRequirements()
        {
            var issues = new List<string>();
            
            // Suppressor validation
            if (IsSuppressorComponent())
            {
                if (!RequiresATFForm1 && !RequiresATFForm4)
                    issues.Add("Suppressor components typically require ATF Form 1 or Form 4");
                
                if (!RequiresTaxStamp)
                    issues.Add("Suppressor components typically require tax stamp");
                
                if (!RequiresSoundTesting)
                    issues.Add("Suppressor components should include sound testing");
            }
            
            // Firearm validation
            if (IsFirearmComponent())
            {
                if (!RequiresUniqueSerialNumber)
                    issues.Add("Firearm components require unique serial numbers");
                
                if (!RequiresATFCompliance)
                    issues.Add("Firearm components require ATF compliance");
            }
            
            // Material validation
            if (Material.Contains("Inconel") && !RequiresBTProofTesting)
                issues.Add("Inconel parts should include proof testing");
            
            // Regulatory validation
            if (RequiresTaxStamp && !TaxStampAmount.HasValue)
                issues.Add("Tax stamp amount must be specified");
            
            if (RequiresExportLicense && string.IsNullOrEmpty(ITARCategory))
                issues.Add("ITAR category must be specified for export controlled items");
            
            return issues;
        }
        
        #endregion
        
        #region Stage Management Helper Properties - NEW SECTION FOR PARTS REFACTORING
        
        /// <summary>
        /// Get all required manufacturing stages for this part
        /// </summary>
        [NotMapped]
        public List<string> RequiredStages 
        {
            get
            {
                var stages = new List<string>();
                
                if (RequiresSLSPrinting) stages.Add("SLS Printing");
                if (RequiresEDMOperations) stages.Add("EDM Operations");
                if (RequiresCNCMachining) stages.Add("CNC Machining");
                if (RequiresAssembly) stages.Add("Assembly");
                if (RequiresFinishing) stages.Add("Finishing");
                
                return stages;
            }
        }

        /// <summary>
        /// Get stage indicators for display
        /// </summary>
        [NotMapped]
        public List<StageIndicator> StageIndicators
        {
            get
            {
                var indicators = new List<StageIndicator>();
                
                if (RequiresSLSPrinting) 
                    indicators.Add(new StageIndicator { Name = "SLS", Class = "bg-primary", Icon = "fas fa-print", Title = "SLS Printing" });
                if (RequiresEDMOperations) 
                    indicators.Add(new StageIndicator { Name = "EDM", Class = "bg-warning", Icon = "fas fa-bolt", Title = "EDM Operations" });
                if (RequiresCNCMachining) 
                    indicators.Add(new StageIndicator { Name = "CNC", Class = "bg-success", Icon = "fas fa-cogs", Title = "CNC Machining" });
                if (RequiresAssembly) 
                    indicators.Add(new StageIndicator { Name = "Assembly", Class = "bg-info", Icon = "fas fa-puzzle-piece", Title = "Assembly Operations" });
                if (RequiresFinishing) 
                    indicators.Add(new StageIndicator { Name = "Finishing", Class = "bg-secondary", Icon = "fas fa-brush", Title = "Finishing Operations" });
                
                return indicators;
            }
        }

        /// <summary>
        /// Calculate total estimated process time including all stages
        /// </summary>
        [NotMapped]
        public decimal TotalEstimatedProcessTime
        {
            get
            {
                decimal totalMinutes = 0;
                
                // Convert hours to minutes for main process
                totalMinutes += (decimal)(EstimatedHours * 60);
                
                // Add additional stage times (estimated percentages of main process)
                if (RequiresEDMOperations) totalMinutes += (decimal)(EstimatedHours * 60 * 0.3); // 30% additional
                if (RequiresCNCMachining) totalMinutes += (decimal)(EstimatedHours * 60 * 0.4); // 40% additional
                if (RequiresAssembly) totalMinutes += 120; // 2 hours fixed
                if (RequiresFinishing) totalMinutes += 180; // 3 hours fixed
                
                return Math.Round(totalMinutes / 60, 2); // Return in hours
            }
        }

        /// <summary>
        /// Get count of required stages
        /// </summary>
        [NotMapped]
        public int RequiredStageCount => RequiredStages.Count;

        /// <summary>
        /// Get manufacturing complexity level based on stages and time
        /// </summary>
        [NotMapped]
        public string ComplexityLevel
        {
            get
            {
                var score = 0;
                
                // Time complexity
                if (EstimatedHours > 24) score += 4;
                else if (EstimatedHours > 12) score += 3;
                else if (EstimatedHours > 6) score += 2;
                else if (EstimatedHours > 2) score += 1;
                
                // Stage complexity
                score += RequiredStageCount;
                
                return score switch
                {
                    <= 2 => "Simple",
                    <= 4 => "Medium",
                    <= 6 => "Complex",
                    _ => "Very Complex"
                };
            }
        }

        /// <summary>
        /// Get complexity score for sorting/filtering
        /// </summary>
        [NotMapped]
        public int ComplexityScore
        {
            get
            {
                var score = RequiredStageCount;
                if (EstimatedHours > 24) score += 4;
                else if (EstimatedHours > 12) score += 3;
                else if (EstimatedHours > 6) score += 2;
                else if (EstimatedHours > 2) score += 1;
                
                return score;
            }
        }

        #endregion
    }
}
