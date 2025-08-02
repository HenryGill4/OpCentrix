#!/usr/bin/env pwsh
# Analyze-Parts-Stages.ps1
# Quick analysis tool for parts and stages

Write-Host "?? OpCentrix Parts & Stages Analysis" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

# Parts without stages
$partsWithoutStages = sqlite3 scheduler.db "
    SELECT COUNT(*) 
    FROM Parts p 
    LEFT JOIN PartStageRequirements psr ON p.Id = psr.PartId AND psr.IsActive = 1 
    WHERE p.IsActive = 1 AND psr.Id IS NULL;
"
Write-Host "Parts without stages: $partsWithoutStages" -ForegroundColor 

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
