# ?? OpCentrix Parts Page Setup Guide

## Overview
Your OpCentrix Parts page is now fully functional and properly connected to your SQLite database. This guide will walk you through testing and using the Parts management system.

## ??? Database Schema Analysis
Your database table has **ALL** the required fields and is fully compatible with your Part model:

### Key Features Supported:
- ? **Complete Part Management**: Create, edit, delete, and view parts
- ? **Admin Duration Overrides**: Override standard durations with tracking
- ? **Material-Specific Defaults**: Auto-populate parameters based on material
- ? **Comprehensive SLS Parameters**: All TruPrint 3000 parameters supported
- ? **Cost Tracking**: Material, labor, setup, and processing costs
- ? **Quality Requirements**: Surface finish, tolerances, certifications
- ? **Audit Trail**: Full tracking of who created/modified what and when

## ?? Quick Start Guide

### Step 1: Add Example Parts for Testing
```powershell
# Run the PowerShell script to add test data
.\Add-ExampleParts.ps1

# Choose option 1 to add example parts
# This will add 9 example parts with varied durations and materials
```

### Step 2: Start the Application
```powershell
# Build and run the application
dotnet build
dotnet run

# Access the application at: http://localhost:5090
# Login with: admin / admin123
```

### Step 3: Navigate to Parts Page
1. Open: http://localhost:5090/Admin/Parts
2. You should see your parts table with all the example parts

## ?? What You'll See

### Parts Table Columns:
1. **Part Number**: Unique identifier (format: XX-XXXX)
2. **Name**: Descriptive part name
3. **Description**: Detailed part description
4. **Material**: Material type with color-coded badge
5. **Duration**: Shows effective duration (with override indicator if applicable)
6. **Costs**: Material, labor, and setup costs
7. **Status**: Active/Inactive badge
8. **Actions**: Edit and Delete buttons

### Example Parts Included:
- **EX-1001**: Small Bracket Component (2.5h) - Quick turnaround
- **EX-1002**: Mini Housing (3.75h) - Short build
- **EX-2001**: Aerospace Test Fitting (8.25h) - Standard job
- **EX-2002**: Medical Test Device (10.5h) - Medium complexity
- **EX-3001**: Complex Inconel Manifold (18.75h) - Long build
- **EX-3002**: Large Titanium Assembly (22.25h) - Very long build
- **EX-4001**: Multi-Day Test Build (32.5h) - Multi-day job
- **EX-5001**: Override Test Part (12h ? 8.5h override) - Tests admin override
- **EX-6001**: Rush Job Test Part (6.75h) - Perfect for rush orders

## ?? Testing the Parts Page

### Test 1: Add a New Part
1. Click "Add New Part" button
2. Fill in required fields:
   - Part Number (e.g., "MY-0001")
   - Name (e.g., "Test Bracket")
   - Description (e.g., "My test part for learning")
   - Industry (select from dropdown)
   - Application (select from dropdown)
3. Select a material to see auto-populated defaults
4. Adjust estimated hours and see duration display update
5. Click "Create Part"

### Test 2: Edit an Existing Part
1. Click "Edit" on any part (e.g., EX-1001)
2. Modify the estimated hours
3. Add an admin override duration
4. Fill in the override reason
5. Click "Update Part"
6. Notice the orange override indicator

### Test 3: Use Material Defaults
1. Add a new part
2. Select "Inconel 718" material
3. Watch as laser power, material cost, and estimated hours auto-populate
4. Try different materials to see different defaults

### Test 4: Admin Duration Override
1. Edit any part
2. Set "Override Duration" to a different value than "Standard Duration"
3. Enter a reason (required when override is set)
4. Save and see the override indicator in the parts list

### Test 5: Search and Filter
1. Use the search box to find parts by number or description
2. Filter by material type
3. Filter by active/inactive status
4. Test pagination if you have many parts

## ??? Database Operations

### Using DB Browser for SQLite:
1. Download from: https://sqlitebrowser.org/
2. Open your `scheduler.db` file
3. Browse the "Parts" table to see your data
4. Run queries to explore your data:

```sql
-- View all your parts
SELECT PartNumber, Name, EstimatedHours, Material, IsActive FROM Parts;

-- See parts with admin overrides
SELECT PartNumber, Name, EstimatedHours, AdminEstimatedHoursOverride, AdminOverrideReason 
FROM Parts 
WHERE AdminEstimatedHoursOverride IS NOT NULL;

-- Count parts by material
SELECT Material, COUNT(*) as Count 
FROM Parts 
GROUP BY Material 
ORDER BY Count DESC;

-- View example parts only
SELECT PartNumber, Name, EstimatedHours, Material 
FROM Parts 
WHERE PartNumber LIKE 'EX-%' 
ORDER BY EstimatedHours;
```

## ?? Troubleshooting

### If the modal doesn't open:
1. Check browser console for JavaScript errors
2. Ensure you're logged in as admin
3. Try hard refresh: Ctrl+F5 (Windows) or Cmd+Shift+R (Mac)

### If parts don't save:
1. Check validation errors in the modal
2. Ensure required fields are filled
3. Check part number format (XX-XXXX)
4. Verify part number isn't duplicated

### If example parts don't load:
1. Run the PowerShell script again
2. Check if parts already exist (script skips if found)
3. Use option 3 in the script to check count
4. Manually remove example parts with option 2 and re-add

### Database locked errors:
1. Stop the application (Ctrl+C)
2. Close DB Browser if open
3. Restart the application

## ?? Key Features Demonstrated

### 1. Material-Specific Defaults:
- Select "Ti-6Al-4V Grade 5" ? Auto-fills 200W laser power, $450/kg cost
- Select "Inconel 718" ? Auto-fills 285W laser power, $750/kg cost

### 2. Admin Override System:
- Set override duration different from standard
- Reason is required and tracked
- Override shows in parts list with indicator
- Audit trail tracks who/when/why

### 3. Comprehensive Validation:
- Part number format validation
- Duplicate part number detection
- Required field validation
- Numeric range validation

### 4. Real-time Updates:
- Duration display updates as you type hours
- Material selection triggers default updates
- Override reason required when override set

## ?? Next Steps

1. **Learn the Database**: Use DB Browser to explore your data
2. **Add Real Parts**: Start adding your actual manufacturing parts
3. **Test Integration**: Use parts in the scheduler to create jobs
4. **Customize**: Modify materials and defaults in the code as needed
5. **Backup**: Regularly backup your `scheduler.db` file

## ?? Pro Tips

1. **Part Numbering**: Use consistent format like "14-5396" for easy sorting
2. **Materials**: Set up your actual materials in the dropdown
3. **Cost Tracking**: Keep material costs current for accurate estimates
4. **Admin Overrides**: Use sparingly and document reasons well
5. **Backup Strategy**: Copy scheduler.db before major changes

Your Parts page is now fully functional and ready for production use! ??