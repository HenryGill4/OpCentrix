# PowerShell script to insert default ProductionStages data
# This script uses SQLite command line to insert default stages

Write-Host "Adding default Production Stages to OpCentrix database..." -ForegroundColor Green

# Define the SQL commands as an array for easier execution
$sqlCommands = @(
    "INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive) VALUES ('SLS Printing', 1, 'Selective Laser Sintering metal printing process', 45, 85.00, 1, 0, 0, 0, datetime('now'), 1);",
    
    "INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive) VALUES ('CNC Machining', 2, 'Computer Numerical Control machining operations', 30, 95.00, 1, 0, 0, 1, datetime('now'), 1);",
    
    "INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive) VALUES ('EDM Operations', 3, 'Electrical Discharge Machining for complex geometries', 60, 110.00, 1, 1, 0, 1, datetime('now'), 1);",
    
    "INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedData, IsActive) VALUES ('Assembly', 4, 'Assembly of multiple components', 15, 75.00, 1, 0, 0, 1, datetime('now'), 1);",
    
    "INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive) VALUES ('Finishing', 5, 'Surface finishing, coating, and final processing', 20, 70.00, 1, 0, 0, 1, datetime('now'), 1);",
    
    "INSERT OR IGNORE INTO ProductionStages (Name, DisplayOrder, Description, DefaultSetupMinutes, DefaultHourlyRate, RequiresQualityCheck, RequiresApproval, AllowSkip, IsOptional, CreatedDate, IsActive) VALUES ('Quality Inspection', 6, 'Final quality control and inspection', 10, 80.00, 1, 1, 0, 0, datetime('now'), 1);"
)

# Execute each SQL command
$successCount = 0
foreach ($sql in $sqlCommands) {
    try {
        # Use .NET SQLite to execute commands
        Add-Type -AssemblyName System.Data.SQLite
        $connectionString = "Data Source=scheduler.db;Version=3;"
        $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $rowsAffected = $command.ExecuteNonQuery()
        
        $connection.Close()
        $successCount++
        Write-Host "  ? Executed SQL command $successCount successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "  ? Error executing SQL command $($successCount + 1): $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Verify the data was inserted
try {
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM ProductionStages WHERE IsActive = 1;"
    $stageCount = $command.ExecuteScalar()
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "SUCCESS: $stageCount Production Stages are now available in the database" -ForegroundColor Green
    Write-Host "Default Production Stages setup completed!" -ForegroundColor Green
}
catch {
    Write-Host "Error verifying stages: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "You can now use the enhanced Parts management system with stage requirements!" -ForegroundColor Cyan