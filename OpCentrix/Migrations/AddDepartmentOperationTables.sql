-- Migration for adding department-specific operation tables
-- Run with: dotnet ef database update

-- CoatingOperations table
CREATE TABLE "CoatingOperations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CoatingOperations" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "CoatingType" TEXT NOT NULL DEFAULT '',
    "CoatingSpecification" TEXT NOT NULL DEFAULT '',
    "Color" TEXT NOT NULL DEFAULT '',
    "Thickness" TEXT NOT NULL DEFAULT '',
    "Status" TEXT NOT NULL DEFAULT 'Pending',
    "ScheduledDate" TEXT NOT NULL,
    "ActualStartDate" TEXT,
    "ActualCompletionDate" TEXT,
    "Operator" TEXT NOT NULL DEFAULT '',
    "QualityInspector" TEXT NOT NULL DEFAULT '',
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "ProcessedQuantity" INTEGER NOT NULL DEFAULT 0,
    "RejectedQuantity" INTEGER NOT NULL DEFAULT 0,
    "ProcessNotes" TEXT NOT NULL DEFAULT '',
    "QualityNotes" TEXT NOT NULL DEFAULT '',
    "RejectionReason" TEXT NOT NULL DEFAULT '',
    "ProcessTemperature" REAL NOT NULL DEFAULT 20.0,
    "Humidity" REAL NOT NULL DEFAULT 50.0,
    "MaterialCost" decimal(10,2) NOT NULL DEFAULT 0,
    "LaborCost" decimal(10,2) NOT NULL DEFAULT 0,
    "OverheadCost" decimal(10,2) NOT NULL DEFAULT 0,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- EDMOperations table
CREATE TABLE "EDMOperations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_EDMOperations" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "WorkPieceDescription" TEXT NOT NULL DEFAULT '',
    "Material" TEXT NOT NULL DEFAULT '',
    "EDMType" TEXT NOT NULL DEFAULT '',
    "MachineId" TEXT NOT NULL DEFAULT '',
    "Status" TEXT NOT NULL DEFAULT 'Setup',
    "ScheduledDate" TEXT NOT NULL,
    "ActualStartDate" TEXT,
    "ActualCompletionDate" TEXT,
    "Operator" TEXT NOT NULL DEFAULT '',
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "CompletedQuantity" INTEGER NOT NULL DEFAULT 0,
    "WireType" TEXT NOT NULL DEFAULT '',
    "WireDiameter" REAL NOT NULL DEFAULT 0,
    "Current" REAL NOT NULL DEFAULT 0,
    "Voltage" REAL NOT NULL DEFAULT 0,
    "PulseOnTime" REAL NOT NULL DEFAULT 0,
    "PulseOffTime" REAL NOT NULL DEFAULT 0,
    "DielectricFluid" TEXT NOT NULL DEFAULT '',
    "FluidTemperature" REAL NOT NULL DEFAULT 20.0,
    "FlowRate" REAL NOT NULL DEFAULT 0,
    "SurfaceFinishRa" REAL NOT NULL DEFAULT 0,
    "ToleranceAchieved" REAL NOT NULL DEFAULT 0,
    "QualityNotes" TEXT NOT NULL DEFAULT '',
    "EstimatedCuttingTime" REAL NOT NULL DEFAULT 0,
    "ActualCuttingTime" REAL NOT NULL DEFAULT 0,
    "MachineUtilization" REAL NOT NULL DEFAULT 0,
    "ProcessNotes" TEXT NOT NULL DEFAULT '',
    "HoldReason" TEXT NOT NULL DEFAULT '',
    "WireCost" decimal(10,2) NOT NULL DEFAULT 0,
    "ElectricityCost" decimal(10,2) NOT NULL DEFAULT 0,
    "LaborCost" decimal(10,2) NOT NULL DEFAULT 0,
    "MachineCost" decimal(10,2) NOT NULL DEFAULT 0,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- MachiningOperations table
CREATE TABLE "MachiningOperations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MachiningOperations" PRIMARY KEY AUTOINCREMENT,
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "WorkOrderNumber" TEXT NOT NULL DEFAULT '',
    "MachineId" TEXT NOT NULL DEFAULT '',
    "MachineType" TEXT NOT NULL DEFAULT '',
    "Operation" TEXT NOT NULL DEFAULT '',
    "Status" TEXT NOT NULL DEFAULT 'Setup',
    "ScheduledDate" TEXT NOT NULL,
    "ActualStartDate" TEXT,
    "ActualCompletionDate" TEXT,
    "Operator" TEXT NOT NULL DEFAULT '',
    "Programmer" TEXT NOT NULL DEFAULT '',
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "CompletedQuantity" INTEGER NOT NULL DEFAULT 0,
    "RejectedQuantity" INTEGER NOT NULL DEFAULT 0,
    "Material" TEXT NOT NULL DEFAULT '',
    "CuttingTool" TEXT NOT NULL DEFAULT '',
    "SpindleSpeed" REAL NOT NULL DEFAULT 0,
    "FeedRate" REAL NOT NULL DEFAULT 0,
    "DepthOfCut" REAL NOT NULL DEFAULT 0,
    "CuttingSpeed" REAL NOT NULL DEFAULT 0,
    "CoolantType" TEXT NOT NULL DEFAULT '',
    "CoolantFlowRate" REAL NOT NULL DEFAULT 0,
    "ProgramNumber" TEXT NOT NULL DEFAULT '',
    "ToolList" TEXT NOT NULL DEFAULT '',
    "SetupNotes" TEXT NOT NULL DEFAULT '',
    "ProcessNotes" TEXT NOT NULL DEFAULT '',
    "ToleranceRequired" REAL NOT NULL DEFAULT 0,
    "ToleranceAchieved" REAL NOT NULL DEFAULT 0,
    "SurfaceFinishRequired" REAL NOT NULL DEFAULT 0,
    "SurfaceFinishAchieved" REAL NOT NULL DEFAULT 0,
    "QualityNotes" TEXT NOT NULL DEFAULT '',
    "InspectedBy" TEXT NOT NULL DEFAULT '',
    "InspectionDate" TEXT,
    "EstimatedSetupTime" REAL NOT NULL DEFAULT 0,
    "ActualSetupTime" REAL NOT NULL DEFAULT 0,
    "EstimatedCycleTime" REAL NOT NULL DEFAULT 0,
    "ActualCycleTime" REAL NOT NULL DEFAULT 0,
    "MachineUtilization" REAL NOT NULL DEFAULT 0,
    "HoldReason" TEXT NOT NULL DEFAULT '',
    "ToolingCost" decimal(10,2) NOT NULL DEFAULT 0,
    "LaborCost" decimal(10,2) NOT NULL DEFAULT 0,
    "MachineCost" decimal(10,2) NOT NULL DEFAULT 0,
    "MaterialCost" decimal(10,2) NOT NULL DEFAULT 0,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- QualityInspections table
CREATE TABLE "QualityInspections" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_QualityInspections" PRIMARY KEY AUTOINCREMENT,
    "InspectionNumber" TEXT NOT NULL DEFAULT '',
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "InspectionType" TEXT NOT NULL DEFAULT '',
    "Status" TEXT NOT NULL DEFAULT 'Pending',
    "ScheduledDate" TEXT NOT NULL,
    "ActualInspectionDate" TEXT,
    "Inspector" TEXT NOT NULL DEFAULT '',
    "CustomerOrderNumber" TEXT NOT NULL DEFAULT '',
    "QuantityToInspect" INTEGER NOT NULL DEFAULT 1,
    "QuantityInspected" INTEGER NOT NULL DEFAULT 0,
    "QuantityPassed" INTEGER NOT NULL DEFAULT 0,
    "QuantityFailed" INTEGER NOT NULL DEFAULT 0,
    "QuantityOnHold" INTEGER NOT NULL DEFAULT 0,
    "DimensionalRequirements" TEXT NOT NULL DEFAULT '',
    "DimensionalResults" TEXT NOT NULL DEFAULT '',
    "VisualRequirements" TEXT NOT NULL DEFAULT '',
    "VisualResults" TEXT NOT NULL DEFAULT '',
    "FunctionalRequirements" TEXT NOT NULL DEFAULT '',
    "FunctionalResults" TEXT NOT NULL DEFAULT '',
    "SurfaceFinishRequired" REAL NOT NULL DEFAULT 0,
    "SurfaceFinishMeasured" REAL NOT NULL DEFAULT 0,
    "HardnessRequired" REAL NOT NULL DEFAULT 0,
    "HardnessMeasured" REAL NOT NULL DEFAULT 0,
    "HardnessScale" TEXT NOT NULL DEFAULT '',
    "CriticalDimensions" TEXT NOT NULL DEFAULT '{}',
    "MeasuredDimensions" TEXT NOT NULL DEFAULT '{}',
    "ToleranceAnalysis" TEXT NOT NULL DEFAULT '',
    "TestProcedure" TEXT NOT NULL DEFAULT '',
    "TestEquipment" TEXT NOT NULL DEFAULT '',
    "CalibrationDate" TEXT NOT NULL DEFAULT '',
    "TestResults" TEXT NOT NULL DEFAULT '',
    "CertificationRequirements" TEXT NOT NULL DEFAULT '',
    "RequiresCertificate" INTEGER NOT NULL DEFAULT 0,
    "CertificateGenerated" INTEGER NOT NULL DEFAULT 0,
    "CertificateNumber" TEXT NOT NULL DEFAULT '',
    "NonConformanceDescription" TEXT NOT NULL DEFAULT '',
    "CorrectiveAction" TEXT NOT NULL DEFAULT '',
    "DispositionCode" TEXT NOT NULL DEFAULT '',
    "DispositionNotes" TEXT NOT NULL DEFAULT '',
    "InspectionNotes" TEXT NOT NULL DEFAULT '',
    "CustomerRequirements" TEXT NOT NULL DEFAULT '',
    "DrawingRevision" TEXT NOT NULL DEFAULT '',
    "SpecificationRevision" TEXT NOT NULL DEFAULT '',
    "TraceabilityInfo" TEXT NOT NULL DEFAULT '',
    "Temperature" REAL NOT NULL DEFAULT 20.0,
    "Humidity" REAL NOT NULL DEFAULT 50.0,
    "Pressure" REAL NOT NULL DEFAULT 1013.25,
    "EstimatedInspectionTime" REAL NOT NULL DEFAULT 0,
    "ActualInspectionTime" REAL NOT NULL DEFAULT 0,
    "InspectionCost" decimal(10,2) NOT NULL DEFAULT 0,
    "TestingCost" decimal(10,2) NOT NULL DEFAULT 0,
    "CertificationCost" decimal(10,2) NOT NULL DEFAULT 0,
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- ShippingOperations table
CREATE TABLE "ShippingOperations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ShippingOperations" PRIMARY KEY AUTOINCREMENT,
    "ShipmentNumber" TEXT NOT NULL DEFAULT '',
    "CustomerName" TEXT NOT NULL DEFAULT '',
    "CustomerOrderNumber" TEXT NOT NULL DEFAULT '',
    "Status" TEXT NOT NULL DEFAULT 'Pending',
    "ScheduledShipDate" TEXT NOT NULL,
    "ActualShipDate" TEXT,
    "DeliveryDate" TEXT,
    "ShippingMethod" TEXT NOT NULL DEFAULT '',
    "TrackingNumber" TEXT NOT NULL DEFAULT '',
    "Priority" TEXT NOT NULL DEFAULT 'Standard',
    "ShippingAddress" TEXT NOT NULL DEFAULT '',
    "City" TEXT NOT NULL DEFAULT '',
    "State" TEXT NOT NULL DEFAULT '',
    "ZipCode" TEXT NOT NULL DEFAULT '',
    "Country" TEXT NOT NULL DEFAULT 'USA',
    "ContactPerson" TEXT NOT NULL DEFAULT '',
    "ContactPhone" TEXT NOT NULL DEFAULT '',
    "ContactEmail" TEXT NOT NULL DEFAULT '',
    "WeightKg" REAL NOT NULL DEFAULT 0,
    "LengthCm" REAL NOT NULL DEFAULT 0,
    "WidthCm" REAL NOT NULL DEFAULT 0,
    "HeightCm" REAL NOT NULL DEFAULT 0,
    "PackagingType" TEXT NOT NULL DEFAULT '',
    "PackagingNotes" TEXT NOT NULL DEFAULT '',
    "ShippingCost" decimal(10,2) NOT NULL DEFAULT 0,
    "InsuranceValue" decimal(10,2) NOT NULL DEFAULT 0,
    "CustomsDeclaredValue" decimal(10,2) NOT NULL DEFAULT 0,
    "RequiresSignature" INTEGER NOT NULL DEFAULT 0,
    "RequiresInsurance" INTEGER NOT NULL DEFAULT 0,
    "IsInternational" INTEGER NOT NULL DEFAULT 0,
    "SpecialInstructions" TEXT NOT NULL DEFAULT '',
    "PackingSlipNotes" TEXT NOT NULL DEFAULT '',
    "CustomsDocumentation" TEXT NOT NULL DEFAULT '',
    "PackedBy" TEXT NOT NULL DEFAULT '',
    "InspectedBy" TEXT NOT NULL DEFAULT '',
    "PackingDate" TEXT,
    "InspectionDate" TEXT,
    "QualityCheckNotes" TEXT NOT NULL DEFAULT '',
    "CreatedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastModifiedDate" TEXT NOT NULL DEFAULT (datetime('now')),
    "CreatedBy" TEXT NOT NULL DEFAULT 'System',
    "LastModifiedBy" TEXT NOT NULL DEFAULT 'System'
);

-- ShippingItems table
CREATE TABLE "ShippingItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ShippingItems" PRIMARY KEY AUTOINCREMENT,
    "ShippingOperationId" INTEGER NOT NULL,
    "PartNumber" TEXT NOT NULL DEFAULT '',
    "Description" TEXT NOT NULL DEFAULT '',
    "Quantity" INTEGER NOT NULL DEFAULT 1,
    "UnitValue" decimal(10,2) NOT NULL DEFAULT 0,
    "SerialNumbers" TEXT NOT NULL DEFAULT '',
    "Notes" TEXT NOT NULL DEFAULT '',
    CONSTRAINT "FK_ShippingItems_ShippingOperations_ShippingOperationId" FOREIGN KEY ("ShippingOperationId") REFERENCES "ShippingOperations" ("Id") ON DELETE CASCADE
);

-- Create indexes for performance
CREATE INDEX "IX_CoatingOperations_PartNumber" ON "CoatingOperations" ("PartNumber");
CREATE INDEX "IX_CoatingOperations_Status" ON "CoatingOperations" ("Status");
CREATE INDEX "IX_CoatingOperations_ScheduledDate" ON "CoatingOperations" ("ScheduledDate");
CREATE INDEX "IX_CoatingOperations_Operator" ON "CoatingOperations" ("Operator");
CREATE INDEX "IX_CoatingOperations_CoatingType" ON "CoatingOperations" ("CoatingType");

CREATE INDEX "IX_EDMOperations_PartNumber" ON "EDMOperations" ("PartNumber");
CREATE INDEX "IX_EDMOperations_Status" ON "EDMOperations" ("Status");
CREATE INDEX "IX_EDMOperations_ScheduledDate" ON "EDMOperations" ("ScheduledDate");
CREATE INDEX "IX_EDMOperations_Operator" ON "EDMOperations" ("Operator");
CREATE INDEX "IX_EDMOperations_MachineId" ON "EDMOperations" ("MachineId");
CREATE INDEX "IX_EDMOperations_EDMType" ON "EDMOperations" ("EDMType");

CREATE INDEX "IX_MachiningOperations_PartNumber" ON "MachiningOperations" ("PartNumber");
CREATE INDEX "IX_MachiningOperations_Status" ON "MachiningOperations" ("Status");
CREATE INDEX "IX_MachiningOperations_ScheduledDate" ON "MachiningOperations" ("ScheduledDate");
CREATE INDEX "IX_MachiningOperations_Operator" ON "MachiningOperations" ("Operator");
CREATE INDEX "IX_MachiningOperations_MachineId" ON "MachiningOperations" ("MachineId");
CREATE INDEX "IX_MachiningOperations_WorkOrderNumber" ON "MachiningOperations" ("WorkOrderNumber");

CREATE UNIQUE INDEX "IX_QualityInspections_InspectionNumber" ON "QualityInspections" ("InspectionNumber");
CREATE INDEX "IX_QualityInspections_PartNumber" ON "QualityInspections" ("PartNumber");
CREATE INDEX "IX_QualityInspections_Status" ON "QualityInspections" ("Status");
CREATE INDEX "IX_QualityInspections_ScheduledDate" ON "QualityInspections" ("ScheduledDate");
CREATE INDEX "IX_QualityInspections_Inspector" ON "QualityInspections" ("Inspector");
CREATE INDEX "IX_QualityInspections_InspectionType" ON "QualityInspections" ("InspectionType");

CREATE UNIQUE INDEX "IX_ShippingOperations_ShipmentNumber" ON "ShippingOperations" ("ShipmentNumber");
CREATE INDEX "IX_ShippingOperations_Status" ON "ShippingOperations" ("Status");
CREATE INDEX "IX_ShippingOperations_ScheduledShipDate" ON "ShippingOperations" ("ScheduledShipDate");
CREATE INDEX "IX_ShippingOperations_CustomerName" ON "ShippingOperations" ("CustomerName");
CREATE INDEX "IX_ShippingOperations_CustomerOrderNumber" ON "ShippingOperations" ("CustomerOrderNumber");
CREATE INDEX "IX_ShippingOperations_TrackingNumber" ON "ShippingOperations" ("TrackingNumber");

CREATE INDEX "IX_ShippingItems_ShippingOperationId" ON "ShippingItems" ("ShippingOperationId");
CREATE INDEX "IX_ShippingItems_PartNumber" ON "ShippingItems" ("PartNumber");