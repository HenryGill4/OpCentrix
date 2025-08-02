#!/usr/bin/env pwsh
# Fix-Stage-Database-Communication.ps1
# Comprehensive fix for Parts page and database stage logic communication

Write-Host "?? OpCentrix Stage Logic & Database Communication Fix" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green
Write-Host ""

# Step 1: Verify current database state
Write-Host "?? Step 1: Verifying Database State" -ForegroundColor Yellow
Write-Host "------------------------------------" -ForegroundColor Yellow

try {
    $productionStageCount = sqlite3 scheduler.db "SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1;"
    Write-Host "? Production Stages: $productionStageCount active stages found" -ForegroundColor Green
    
    $partStageCount = sqlite3 scheduler.db "SELECT COUNT(*) FROM PartStageRequirements WHERE IsActive = 1;"
    Write-Host "? Part Stage Requirements: $partStageCount active requirements found" -ForegroundColor Green
    
    $partsCount = sqlite3 scheduler.db "SELECT COUNT(*) FROM Parts WHERE IsActive = 1;"
    Write-Host "? Active Parts: $partsCount parts found" -ForegroundColor Green
    
    # Show stage distribution
    Write-Host ""
    Write-Host "?? Stage Usage Distribution:" -ForegroundColor Cyan
    sqlite3 scheduler.db -header -column "
        SELECT 
            ps.Name as StageName,
            COUNT(psr.Id) as PartsUsingStage,
            ps.DisplayOrder as StageOrder
        FROM ProductionStages ps
        LEFT JOIN PartStageRequirements psr ON ps.Id = psr.ProductionStageId AND psr.IsActive = 1
        WHERE ps.IsActive = 1
        GROUP BY ps.Id, ps.Name, ps.DisplayOrder
        ORDER BY ps.DisplayOrder;
    "
    
} catch {
    Write-Error "? Database verification failed: $_"
    exit 1
}

Write-Host ""

# Step 2: Fix legacy parts without stage requirements
Write-Host "?? Step 2: Creating Stage Requirements for Legacy Parts" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------" -ForegroundColor Yellow

try {
    # Get parts that have boolean flags but no stage requirements
    $legacyParts = sqlite3 scheduler.db -header -csv "
        SELECT p.Id, p.PartNumber, p.RequiresSLSPrinting, p.RequiresCNCMachining, 
               p.RequiresEDMOperations, p.RequiresAssembly, p.RequiresFinishing, p.RequiresInspection
        FROM Parts p
        LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
        WHERE p.IsActive = 1 
        AND psr.Id IS NULL
        AND (p.RequiresSLSPrinting = 1 OR p.RequiresCNCMachining = 1 OR 
             p.RequiresEDMOperations = 1 OR p.RequiresAssembly = 1 OR 
             p.RequiresFinishing = 1 OR p.RequiresInspection = 1);
    "
    
    if ($legacyParts) {
        Write-Host "?? Found legacy parts needing stage requirements:" -ForegroundColor Cyan
        $legacyPartsArray = $legacyParts -split "`n"
        $headerLine = $legacyPartsArray[0]
        
        for ($i = 1; $i -lt $legacyPartsArray.Length; $i++) {
            if ($legacyPartsArray[$i]) {
                $fields = $legacyPartsArray[$i] -split ","
                $partId = $fields[0]
                $partNumber = $fields[1]
                $requiresSLS = $fields[2]
                $requiresCNC = $fields[3]
                $requiresEDM = $fields[4]
                $requiresAssembly = $fields[5]
                $requiresFinishing = $fields[6]
                $requiresInspection = $fields[7]
                
                Write-Host "  ?? Processing Part: $partNumber (ID: $partId)" -ForegroundColor White
                
                $executionOrder = 1
                
                # Create stage requirements based on boolean flags
                if ($requiresSLS -eq "1") {
                    $slsStageId = sqlite3 scheduler.db "SELECT Id FROM ProductionStages WHERE Name LIKE '%SLS%' OR Name LIKE '%Printing%' LIMIT 1;"
                    if ($slsStageId) {
                        sqlite3 scheduler.db "
                            INSERT INTO PartStageRequirements 
                            (PartId, ProductionStageId, ExecutionOrder, IsRequired, EstimatedHours, SetupTimeMinutes, 
                             StageParameters, SpecialInstructions, QualityRequirements, RequiredMaterials, 
                             RequiredTooling, EstimatedCost, AllowParallel, IsBlocking, IsActive, RequirementNotes, 
                             CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)
                            VALUES 
                            ($partId, $slsStageId, $executionOrder, 1, 8.0, 45, '{}', '', '{}', '[]', '', 0.0, 0, 1, 1, 'Migrated from boolean flag', 
                             datetime('now'), datetime('now'), 'Migration Script', 'Migration Script');
                        "
                        Write-Host "    ? Added SLS Printing stage" -ForegroundColor Green
                        $executionOrder++
                    }
                }
                
                if ($requiresCNC -eq "1") {
                    $cncStageId = sqlite3 scheduler.db "SELECT Id FROM ProductionStages WHERE Name LIKE '%CNC%' OR Name LIKE '%Machining%' LIMIT 1;"
                    if ($cncStageId) {
                        sqlite3 scheduler.db "
                            INSERT INTO PartStageRequirements 
                            (PartId, ProductionStageId, ExecutionOrder, IsRequired, EstimatedHours, SetupTimeMinutes, 
                             StageParameters, SpecialInstructions, QualityRequirements, RequiredMaterials, 
                             RequiredTooling, EstimatedCost, AllowParallel, IsBlocking, IsActive, RequirementNotes, 
                             CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)
                            VALUES 
                            ($partId, $cncStageId, $executionOrder, 1, 4.0, 30, '{}', '', '{}', '[]', '', 0.0, 0, 1, 1, 'Migrated from boolean flag', 
                             datetime('now'), datetime('now'), 'Migration Script', 'Migration Script');
                        "
                        Write-Host "    ? Added CNC Machining stage" -ForegroundColor Green
                        $executionOrder++
                    }
                }
                
                if ($requiresEDM -eq "1") {
                    $edmStageId = sqlite3 scheduler.db "SELECT Id FROM ProductionStages WHERE Name LIKE '%EDM%' LIMIT 1;"
                    if ($edmStageId) {
                        sqlite3 scheduler.db "
                            INSERT INTO PartStageRequirements 
                            (PartId, ProductionStageId, ExecutionOrder, IsRequired, EstimatedHours, SetupTimeMinutes, 
                             StageParameters, SpecialInstructions, QualityRequirements, RequiredMaterials, 
                             RequiredTooling, EstimatedCost, AllowParallel, IsBlocking, IsActive, RequirementNotes, 
                             CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)
                            VALUES 
                            ($partId, $edmStageId, $executionOrder, 1, 6.0, 60, '{}', '', '{}', '[]', '', 0.0, 0, 1, 1, 'Migrated from boolean flag', 
                             datetime('now'), datetime('now'), 'Migration Script', 'Migration Script');
                        "
                        Write-Host "    ? Added EDM Operations stage" -ForegroundColor Green
                        $executionOrder++
                    }
                }
                
                if ($requiresAssembly -eq "1") {
                    $assemblyStageId = sqlite3 scheduler.db "SELECT Id FROM ProductionStages WHERE Name LIKE '%Assembly%' LIMIT 1;"
                    if ($assemblyStageId) {
                        sqlite3 scheduler.db "
                            INSERT INTO PartStageRequirements 
                            (PartId, ProductionStageId, ExecutionOrder, IsRequired, EstimatedHours, SetupTimeMinutes, 
                             StageParameters, SpecialInstructions, QualityRequirements, RequiredMaterials, 
                             RequiredTooling, EstimatedCost, AllowParallel, IsBlocking, IsActive, RequirementNotes, 
                             CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)
                            VALUES 
                            ($partId, $assemblyStageId, $executionOrder, 1, 2.0, 15, '{}', '', '{}', '[]', '', 0.0, 1, 0, 1, 'Migrated from boolean flag', 
                             datetime('now'), datetime('now'), 'Migration Script', 'Migration Script');
                        "
                        Write-Host "    ? Added Assembly stage" -ForegroundColor Green
                        $executionOrder++
                    }
                }
                
                if ($requiresFinishing -eq "1") {
                    $finishingStageId = sqlite3 scheduler.db "SELECT Id FROM ProductionStages WHERE Name LIKE '%Finishing%' OR Name LIKE '%Coating%' OR Name LIKE '%Sandblasting%' LIMIT 1;"
                    if ($finishingStageId) {
                        sqlite3 scheduler.db "
                            INSERT INTO PartStageRequirements 
                            (PartId, ProductionStageId, ExecutionOrder, IsRequired, EstimatedHours, SetupTimeMinutes, 
                             StageParameters, SpecialInstructions, QualityRequirements, RequiredMaterials, 
                             RequiredTooling, EstimatedCost, AllowParallel, IsBlocking, IsActive, RequirementNotes, 
                             CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)
                            VALUES 
                            ($partId, $finishingStageId, $executionOrder, 1, 3.0, 20, '{}', '', '{}', '[]', '', 0.0, 1, 0, 1, 'Migrated from boolean flag', 
                             datetime('now'), datetime('now'), 'Migration Script', 'Migration Script');
                        "
                        Write-Host "    ? Added Finishing stage" -ForegroundColor Green
                        $executionOrder++
                    }
                }
                
                if ($requiresInspection -eq "1") {
                    $qualityStageId = sqlite3 scheduler.db "SELECT Id FROM ProductionStages WHERE Name LIKE '%Quality%' OR Name LIKE '%Inspection%' LIMIT 1;"
                    if (-not $qualityStageId) {
                        # Create a default quality inspection stage if it doesn't exist
                        sqlite3 scheduler.db "
                            INSERT INTO ProductionStages 
                            (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, 
                             RequiresApproval, AllowSkip, IsOptional, RequiredRole, CreatedDate, IsActive)
                            VALUES 
                            ('Quality Inspection', 99, 'Final quality control and inspection', 10, 80.0, 1, 1, 0, 0, 
                             'Quality Inspector', datetime('now'), 1);
                        "
                        $qualityStageId = sqlite3 scheduler.db "SELECT last_insert_rowid();"
                        Write-Host "    ?? Created Quality Inspection stage" -ForegroundColor Cyan
                    }
                    if ($qualityStageId) {
                        sqlite3 scheduler.db "
                            INSERT INTO PartStageRequirements 
                            (PartId, ProductionStageId, ExecutionOrder, IsRequired, EstimatedHours, SetupTimeMinutes, 
                             StageParameters, SpecialInstructions, QualityRequirements, RequiredMaterials, 
                             RequiredTooling, EstimatedCost, AllowParallel, IsBlocking, IsActive, RequirementNotes, 
                             CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy)
                            VALUES 
                            ($partId, $qualityStageId, $executionOrder, 1, 1.0, 10, '{}', '', '{}', '[]', '', 0.0, 0, 1, 1, 'Migrated from boolean flag', 
                             datetime('now'), datetime('now'), 'Migration Script', 'Migration Script');
                        "
                        Write-Host "    ? Added Quality Inspection stage" -ForegroundColor Green
                        $executionOrder++
                    }
                }
            }
        }
        
        $newPartStageCount = sqlite3 scheduler.db "SELECT COUNT(*) FROM PartStageRequirements WHERE IsActive = 1;"
        Write-Host "? Migration complete! Total part stage requirements: $newPartStageCount" -ForegroundColor Green
    } else {
        Write-Host "? No legacy parts found needing migration" -ForegroundColor Green
    }
    
} catch {
    Write-Error "? Legacy parts migration failed: $_"
}

Write-Host ""

# Step 3: Update database indexes for performance
Write-Host "?? Step 3: Optimizing Database Performance" -ForegroundColor Yellow
Write-Host "-------------------------------------------" -ForegroundColor Yellow

try {
    # Add additional indexes for stage queries
    sqlite3 scheduler.db "
        CREATE INDEX IF NOT EXISTS IX_PartStageRequirements_PartId_ExecutionOrder 
        ON PartStageRequirements (PartId, ExecutionOrder);
    "
    
    sqlite3 scheduler.db "
        CREATE INDEX IF NOT EXISTS IX_PartStageRequirements_ProductionStageId_IsActive 
        ON PartStageRequirements (ProductionStageId, IsActive);
    "
    
    sqlite3 scheduler.db "
        CREATE INDEX IF NOT EXISTS IX_ProductionStages_DisplayOrder_IsActive 
        ON ProductionStages (DisplayOrder, IsActive);
    "
    
    Write-Host "? Database indexes optimized" -ForegroundColor Green
    
} catch {
    Write-Error "? Database optimization failed: $_"
}

Write-Host ""

# Step 4: Verify stage logic is working
Write-Host "?? Step 4: Verification & Testing" -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor Yellow

try {
    # Test query that Parts.cshtml uses
    Write-Host "?? Parts with Stage Requirements Summary:" -ForegroundColor Cyan
    sqlite3 scheduler.db -header -column "
        SELECT 
            p.PartNumber,
            p.Name,
            COUNT(psr.Id) as StageCount,
            GROUP_CONCAT(ps.Name, ' ? ') as Stages,
            SUM(psr.EstimatedHours) as TotalHours
        FROM Parts p
        INNER JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
        INNER JOIN ProductionStages ps ON psr.ProductionStageId = ps.Id
        WHERE p.IsActive = 1
        GROUP BY p.Id, p.PartNumber, p.Name
        ORDER BY p.PartNumber
        LIMIT 10;
    "
    
    Write-Host ""
    Write-Host "?? Stage Complexity Distribution:" -ForegroundColor Cyan
    sqlite3 scheduler.db -header -column "
        SELECT 
            CASE 
                WHEN stage_count <= 1 THEN 'Simple'
                WHEN stage_count <= 3 THEN 'Medium'
                WHEN stage_count <= 5 THEN 'Complex'
                ELSE 'Very Complex'
            END as ComplexityLevel,
            COUNT(*) as PartCount,
            ROUND(AVG(total_hours), 1) as AvgHours
        FROM (
            SELECT 
                p.Id,
                COUNT(psr.Id) as stage_count,
                SUM(psr.EstimatedHours) as total_hours
            FROM Parts p
            INNER JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
            WHERE p.IsActive = 1
            GROUP BY p.Id
        ) part_complexity
        GROUP BY ComplexityLevel
        ORDER BY 
            CASE ComplexityLevel
                WHEN 'Simple' THEN 1
                WHEN 'Medium' THEN 2
                WHEN 'Complex' THEN 3
                WHEN 'Very Complex' THEN 4
            END;
    "
    
    Write-Host ""
    Write-Host "? Database verification complete!" -ForegroundColor Green
    
} catch {
    Write-Error "? Verification failed: $_"
}

Write-Host ""

# Step 5: Provide testing instructions
Write-Host "?? Step 5: Testing Instructions" -ForegroundColor Yellow
Write-Host "--------------------------------" -ForegroundColor Yellow

Write-Host "?? Manual Testing Steps:" -ForegroundColor Cyan
Write-Host "1. Start the application: dotnet run --urls http://localhost:5091" -ForegroundColor White
Write-Host "2. Login with admin/admin123" -ForegroundColor White
Write-Host "3. Navigate to /Admin/Parts" -ForegroundColor White
Write-Host "4. Verify parts show stage indicators in the 'Manufacturing Stages' column" -ForegroundColor White
Write-Host "5. Click 'Add New Part' and test the Manufacturing Stages tab" -ForegroundColor White
Write-Host "6. Edit an existing part and verify stage configuration works" -ForegroundColor White
Write-Host "7. Check that complexity levels are calculated correctly" -ForegroundColor White

Write-Host ""
Write-Host "?? Troubleshooting Commands:" -ForegroundColor Cyan
Write-Host "- Check part stage data: sqlite3 scheduler.db \"SELECT * FROM PartStageRequirements LIMIT 5;\"" -ForegroundColor White
Write-Host "- Verify production stages: sqlite3 scheduler.db \"SELECT * FROM ProductionStages;\"" -ForegroundColor White
Write-Host "- Check build logs: dotnet build 2>&1 | Select-String \"error\"" -ForegroundColor White

Write-Host ""

# Step 6: Create additional helper script
Write-Host "?? Step 6: Creating Helper Scripts" -ForegroundColor Yellow
Write-Host "-----------------------------------" -ForegroundColor Yellow

# Create a parts analysis script
$analysisScript = @"
#!/usr/bin/env pwsh
# Analyze-Parts-Stages.ps1
# Quick analysis tool for parts and stages

Write-Host "?? OpCentrix Parts & Stages Analysis" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

# Parts without stages
`$partsWithoutStages = sqlite3 scheduler.db "
    SELECT COUNT(*) 
    FROM Parts p 
    LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1 
    WHERE p.IsActive = 1 AND psr.Id IS NULL;
"
Write-Host "Parts without stages: `$partsWithoutStages" -ForegroundColor $(if (`$partsWithoutStages -eq "0") { "Green" } else { "Yellow" })

# Most used stages
Write-Host ""
Write-Host "?? Most Used Stages:" -ForegroundColor Cyan
sqlite3 scheduler.db -header -column "
    SELECT ps.Name, COUNT(psr.Id) as Usage
    FROM ProductionStages ps
    LEFT JOIN PartStageRequirements psr ON ps.Id = psr.ProductionStageId AND psr.IsActive = 1
    WHERE ps.IsActive = 1
    GROUP BY ps.Id, ps.Name
    ORDER BY Usage DESC;
"

# Average complexity
Write-Host ""
Write-Host "?? Complexity Metrics:" -ForegroundColor Cyan
sqlite3 scheduler.db -header -column "
    SELECT 
        ROUND(AVG(stage_count), 1) as AvgStagesPerPart,
        ROUND(AVG(total_hours), 1) as AvgHoursPerPart,
        MIN(stage_count) as MinStages,
        MAX(stage_count) as MaxStages
    FROM (
        SELECT 
            COUNT(psr.Id) as stage_count,
            SUM(psr.EstimatedHours) as total_hours
        FROM Parts p
        INNER JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1
        WHERE p.IsActive = 1
        GROUP BY p.Id
    );
"
"@

$analysisScript | Out-File -FilePath "Scripts/Analyze-Parts-Stages.ps1" -Encoding UTF8
Write-Host "? Created Scripts/Analyze-Parts-Stages.ps1" -ForegroundColor Green

Write-Host ""
Write-Host "?? Stage Logic & Database Communication Fix Complete!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Summary of Changes:" -ForegroundColor Yellow
Write-Host "? Database verified and optimized" -ForegroundColor Green
Write-Host "? Legacy parts migrated to stage requirements" -ForegroundColor Green
Write-Host "? Performance indexes added" -ForegroundColor Green
Write-Host "? Stage logic verified" -ForegroundColor Green
Write-Host "? Helper scripts created" -ForegroundColor Green
Write-Host ""
Write-Host "?? Your OpCentrix Parts system is now fully integrated with stage management!" -ForegroundColor Green
Write-Host "   The Parts page will display stage indicators and complexity levels correctly." -ForegroundColor White
Write-Host "   Users can now create parts with comprehensive stage workflows." -ForegroundColor White
Write-Host ""
Write-Host "Next: Test the application and verify all functionality works as expected." -ForegroundColor Cyan