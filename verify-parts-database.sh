#!/bin/bash

echo "OpCentrix Parts Database Verification"
echo "======================================="

# Check if OpCentrix directory exists
if [ ! -d "OpCentrix" ]; then
    echo "ERROR: OpCentrix directory not found. Please run from the repository root."
    exit 1
fi

cd OpCentrix

echo ""
echo "Checking Parts Database Status..."

# Check if database file exists
if [ -f "Data/OpCentrix.db" ]; then
    echo "SUCCESS: Database file exists: Data/OpCentrix.db"
    
    # Use sqlite3 to check parts count if available
    if command -v sqlite3 &> /dev/null; then
        echo ""
        echo "Database Query Results:"
        echo "======================="
        
        PART_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM Parts;" 2>/dev/null || echo "0")
        echo "Total Parts in database: $PART_COUNT"
        
        if [ "$PART_COUNT" -gt 0 ]; then
            echo ""
            echo "Recent Parts (Last 10):"
            echo "------------------------"
            sqlite3 -header -column Data/OpCentrix.db "
            SELECT 
                Id,
                PartNumber,
                Description,
                Material,
                EstimatedHours,
                IsActive,
                CreatedDate
            FROM Parts 
            ORDER BY CreatedDate DESC 
            LIMIT 10;" 2>/dev/null || echo "Could not retrieve parts list"
        else
            echo "WARNING: No parts found in database"
        fi
        
        echo ""
        echo "Sample Parts Insert Test:"
        echo "========================"
        
        # Insert a test part
        TEST_RESULT=$(sqlite3 Data/OpCentrix.db "
        INSERT OR IGNORE INTO Parts (
            PartNumber, 
            Description, 
            Material, 
            SlsMaterial,
            Industry,
            Application,
            EstimatedHours, 
            AvgDuration,
            AvgDurationDays,
            IsActive,
            MaterialCostPerKg,
            StandardLaborCostPerHour,
            SetupCost,
            MachineOperatingCostPerHour,
            ArgonCostPerHour,
            RecommendedLaserPower,
            RecommendedScanSpeed,
            RecommendedBuildTemperature,
            RecommendedLayerThickness,
            RecommendedHatchSpacing,
            RequiredArgonPurity,
            MaxOxygenContent,
            PreheatingTimeMinutes,
            CoolingTimeMinutes,
            PostProcessingTimeMinutes,
            WeightGrams,
            LengthMm,
            WidthMm,
            HeightMm,
            VolumeMm3,
            PowderRequirementKg,
            PartCategory,
            PartClass,
            TotalJobsCompleted,
            TotalUnitsProduced,
            CreatedDate,
            CreatedBy,
            LastModifiedDate,
            LastModifiedBy
        ) VALUES (
            'TEST-' || strftime('%s', 'now'),
            'Test Part - Database Verification',
            'Ti-6Al-4V Grade 5',
            'Ti-6Al-4V Grade 5',
            'General',
            'General Component',
            8.0,
            '8.0h',
            1,
            1,
            450.00,
            85.00,
            125.00,
            125.00,
            15.00,
            200,
            1200,
            180,
            30,
            120,
            99.9,
            50,
            60,
            120,
            90,
            0,
            0,
            0,
            0,
            0,
            1.5,
            'Production',
            'B',
            0,
            0,
            datetime('now'),
            'TestScript',
            datetime('now'),
            'TestScript'
        );" 2>&1)
        
        if [ $? -eq 0 ]; then
            echo "SUCCESS: Test part inserted successfully"
            
            # Check if it was actually inserted
            NEW_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM Parts;" 2>/dev/null || echo "0")
            echo "New total parts count: $NEW_COUNT"
            
            echo ""
            echo "Most Recent Part:"
            echo "-----------------"
            sqlite3 -header -column Data/OpCentrix.db "
            SELECT 
                Id,
                PartNumber,
                Description,
                Material,
                EstimatedHours,
                CreatedDate
            FROM Parts 
            ORDER BY CreatedDate DESC 
            LIMIT 1;" 2>/dev/null || echo "Could not retrieve latest part"
        else
            echo "ERROR: Test part insertion failed: $TEST_RESULT"
        fi
        
    else
        echo "WARNING: sqlite3 not available - install with: sudo apt install sqlite3"
        echo "         Or check database manually in the application"
    fi
    
else
    echo "ERROR: Database file not found: Data/OpCentrix.db"
    echo "       The database should be created when you first run the application"
fi

echo ""
echo "Next Steps:"
echo "==========="
echo "1. Run the application: dotnet run"
echo "2. Navigate to: http://localhost:5000/Admin/Parts"
echo "3. Try adding a new part"
echo "4. Check if it appears in the list"
echo "5. If issues persist, check the application logs"

echo ""
echo "Troubleshooting:"
echo "================"
echo "- Ensure the application has write permissions to the Data directory"
echo "- Check application logs for any error messages"
echo "- Try refreshing the browser page after adding parts"
echo "- Verify all required fields are filled in the form"

cd ..