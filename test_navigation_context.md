# Navigation Context Test Guide

## Testing the Navigation Sidebar Consistency

The navigation context tracking system ensures that the sidebar remains consistent when users navigate between the Scheduler page and production areas.

### Test Scenarios:

1. **Admin/Manager Navigation Flow:**
   - Log in as Admin or Manager
   - Navigate to Scheduler using "Production Scheduler" from main sidebar
   - Verify admin-style sidebar shows (Production Areas collapsed group)
   - Apply machine type filter (e.g., SLS)
   - Sidebar should maintain admin context

2. **Production Area Navigation Flow:**
   - Log in as PrintingSpecialist or similar role
   - Navigate to "SLS Printing" from sidebar
   - Click "SLS Schedule" link on the page
   - Verify URL contains: `/Scheduler?navSource=production&machineType=SLS`
   - Verify sidebar shows individual production area links (not collapsed)
   - Navigate back to "SLS Printing" - sidebar should remain consistent

3. **Machine Type Persistence:**
   - Navigate to Scheduler with specific machine type
   - Change machine type filter
   - Navigate away and back
   - Machine type should be remembered

### Expected Behavior:

- **navSource=admin**: Shows collapsed "Production Areas" group in sidebar
- **navSource=production**: Shows individual production area links in sidebar
- **machineType**: Persists across page navigations
- **Active states**: Correctly highlight the current page/section

### Key URLs to Test:

- `/Scheduler` (default admin view)
- `/Scheduler?navSource=admin` (explicit admin context)
- `/Scheduler?navSource=production&machineType=SLS` (production context)
- `/Scheduler?machineType=TI1&__mtRestore=1` (restored context)

### Files Changed:

1. `Pages/Shared/_Layout.cshtml` - Added navigation context tracking
2. `Pages/Scheduler/Index.cshtml` - Added context preservation in filters
3. `Pages/Scheduler/Index.cshtml.cs` - Added navSource parameter
4. `Pages/Printing/Index.cshtml` - Added SLS Schedule link
5. `Pages/Coating/Index.cshtml` - Added Coating Schedule link

### Technical Implementation:

- Uses `sessionStorage` for navigation source persistence
- Uses `localStorage` for machine type persistence
- Tracks navigation context through URL parameters
- Maintains context across page refreshes and navigation