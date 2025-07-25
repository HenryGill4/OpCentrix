# DETAILED CLAUDE SONNET 4 PROMPT FOR SCHEDULER WEEKEND OPERATIONS FIX

## Background Context

You are working on an OpCentrix SLS (Selective Laser Sintering) manufacturing scheduler system built with ASP.NET Core 8 Razor Pages. The system manages job scheduling for 3D metal printing machines (TI1, TI2, INC) and has a critical issue with weekend operations.

## Problem Statement

**CRITICAL ISSUE**: The scheduler is incorrectly posting jobs scheduled for Friday and Saturday to Monday instead, despite weekend operations being intended to be enabled. Users expect to be able to schedule jobs on weekends when the settings allow it.

## Technical Environment

- **.NET 8** ASP.NET Core Razor Pages application
- **Entity Framework Core** with SQLite database  
- **Production system** serving real manufacturing operations
- **Windows environment** (no Unicode characters allowed)
- **Existing codebase** with partial scheduler settings implementation

## Current Architecture Analysis

Based on code analysis, the system has:

1. **SchedulerSettings table** in database with weekend operation flags:
   - `EnableWeekendOperations` (bool)
   - `SaturdayOperations` (bool) 
   - `SundayOperations` (bool)

2. **SchedulerService** with weekend validation methods:
   - `IsWeekendOperationAllowedAsync(DateTime jobDate)`
   - `ValidateSchedulingConstraintsAsync(Job job, List<Job> existingJobs)`
   - `CalculateNextAvailableStartTimeAsync(...)` with complex logic

3. **Missing SchedulerSettingsService**: The code references `ISchedulerSettingsService` but it appears the implementation is missing or incomplete.

## Root Cause Analysis

The primary issues identified:

1. **Missing/Incomplete SchedulerSettingsService**: The `_settingsService` is injected but may not be properly implemented
2. **Weekend Logic Conflicts**: Multiple weekend validation methods may be contradicting each other
3. **Settings Not Applied**: Weekend settings from database may not be properly loaded/applied
4. **Date Calculation Issues**: UTC/local time handling and weekend boundary calculations

## Specific Code Areas With Issues

### SchedulerService.cs Issues:

```csharp
// This method may always return false for weekends:
public async Task<bool> IsWeekendOperationAllowedAsync(DateTime jobDate)
{
    var settings = await _settingsService.GetSettingsAsync();
    
    if (!settings.EnableWeekendOperations)
        return jobDate.DayOfWeek != DayOfWeek.Saturday && jobDate.DayOfWeek != DayOfWeek.Sunday;
        
    // Logic may be inverted or incomplete
}

// This method skips weekends incorrectly:
private async Task<bool> IsValidOperatingDay(DateTime date, SchedulerSettings settings)
{
    // Weekend validation logic needs fixing
}

// This moves jobs away from weekends when they should stay:
private DateTime GetNextValidOperatingDay(DateTime currentDate, SchedulerSettings settings)
{
    // Logic for finding next valid day needs review
}
```

## Requirements for the Fix

### MUST HAVE:
1. **Complete SchedulerSettingsService implementation** with all required methods
2. **Fixed weekend validation logic** that respects settings correctly
3. **Proper settings integration** throughout the scheduler
4. **Admin interface** for managing weekend operation settings
5. **Comprehensive testing** for all weekend scenarios

### SHOULD HAVE:
1. **Audit logging** for settings changes
2. **Real-time settings application** without restart
3. **Error handling** for settings service failures
4. **Time zone consistency** using UTC throughout

### NICE TO HAVE:
1. **Settings validation** with business rules
2. **Settings export/import** functionality
3. **Advanced weekend scheduling rules** (different hours)

## Expected Behavior After Fix

### When `EnableWeekendOperations = true`:
- Jobs scheduled for Friday ? Stay on Friday
- Jobs scheduled for Saturday ? Stay on Saturday (if `SaturdayOperations = true`)
- Jobs scheduled for Sunday ? Stay on Sunday (if `SundayOperations = true`)

### When `EnableWeekendOperations = false`:
- Jobs scheduled for Friday ? Stay on Friday (if within business hours)
- Jobs scheduled for Saturday ? Move to next Monday 7 AM
- Jobs scheduled for Sunday ? Move to next Monday 7 AM

### Settings Hierarchy:
- `EnableWeekendOperations = false` ? No weekend work regardless of individual day settings
- `EnableWeekendOperations = true` + `SaturdayOperations = true` ? Saturday scheduling allowed
- `EnableWeekendOperations = true` + `SundayOperations = true` ? Sunday scheduling allowed

## Implementation Guidelines

### Code Quality Requirements:
1. **No Unicode characters** (Windows compatibility)
2. **Comprehensive error handling** with detailed logging
3. **Proper async/await patterns** throughout
4. **Entity Framework best practices** for settings persistence
5. **Consistent UTC datetime handling**

### Testing Requirements:
1. **Unit tests** for all weekend scenarios
2. **Integration tests** for settings persistence
3. **Manual testing scripts** for validation
4. **Edge case testing** (midnight boundaries, time zones)

## Specific Implementation Tasks

### Task 1: Create SchedulerSettingsService
```csharp
public interface ISchedulerSettingsService
{
    Task<SchedulerSettings> GetSettingsAsync();
    Task<bool> UpdateSettingsAsync(SchedulerSettings settings);
    Task<bool> IsWeekendOperationAllowedAsync(DateTime date);
    Task<bool> IsOperatorAvailableAsync(DateTime start, DateTime end);
    Task<double> GetChangeoverTimeAsync(string fromMaterial, string toMaterial);
    // Add other required methods
}
```

### Task 2: Fix SchedulerService Weekend Logic
- Fix `IsWeekendOperationAllowedAsync` implementation
- Update `CalculateNextAvailableStartTimeAsync` weekend handling
- Correct `IsValidOperatingDay` logic
- Fix `GetNextValidOperatingDay` calculations

### Task 3: Create Admin Settings Interface
- Settings management page at `/Admin/SchedulerSettings`
- Weekend operation toggle controls
- Settings validation and persistence
- Real-time preview of changes

### Task 4: Database Integration
- Ensure SchedulerSettings table has proper defaults
- Add migration if needed for missing columns
- Implement settings seeding in startup

## Testing Scenarios to Validate

### Critical Test Cases:
1. **Friday 2 PM ? Should stay Friday 2 PM** (weekday)
2. **Saturday 8 AM ? Should stay Saturday 8 AM** (when weekend enabled)
3. **Saturday 8 AM ? Should move to Monday 7 AM** (when weekend disabled)
4. **Sunday 10 AM ? Should stay Sunday 10 AM** (when weekend enabled)
5. **Sunday 10 AM ? Should move to Monday 7 AM** (when weekend disabled)

### Edge Cases:
1. Jobs scheduled during weekend ? Monday transition
2. Jobs scheduled at midnight boundaries
3. Settings changes while jobs are being scheduled
4. Multiple jobs on same weekend day

## File Structure Needed

```
OpCentrix/
??? Services/
?   ??? ISchedulerSettingsService.cs (NEW)
?   ??? SchedulerSettingsService.cs (NEW)
?   ??? SchedulerService.cs (MODIFY)
??? Pages/
?   ??? Admin/
?   ?   ??? SchedulerSettings.cshtml (NEW)
?   ?   ??? SchedulerSettings.cshtml.cs (NEW)
?   ??? Scheduler/
?       ??? Index.cshtml.cs (MODIFY)
??? Models/
?   ??? SchedulerSettings.cs (VERIFY EXISTS)
??? Program.cs (MODIFY - register service)
```

## Success Criteria

### Functional Success:
- [ ] Weekend jobs schedule correctly when enabled
- [ ] Weekend jobs move to Monday when disabled  
- [ ] Settings persist and apply immediately
- [ ] Admin can manage weekend settings
- [ ] All existing weekday functionality still works

### Technical Success:
- [ ] No breaking changes to existing API
- [ ] Proper error handling throughout
- [ ] Comprehensive logging for debugging
- [ ] Good test coverage for weekend scenarios
- [ ] Performance not degraded

## Urgent Priority

This is a **PRODUCTION CRITICAL** issue affecting real manufacturing operations. The fix should:

1. **Start immediately** with SchedulerSettingsService implementation
2. **Focus on core weekend logic** before UI enhancements
3. **Test thoroughly** before deployment
4. **Maintain backward compatibility** with existing data
5. **Document all changes** for future maintenance

## Request for Claude Sonnet 4

Please provide a complete, production-ready implementation that:

1. **Implements the missing SchedulerSettingsService** with all required methods
2. **Fixes the weekend operation logic** in SchedulerService 
3. **Creates an admin settings interface** for weekend operation management
4. **Includes proper error handling** and logging throughout
5. **Provides testing guidance** and validation scripts
6. **Maintains code quality standards** for a production system

Focus on solving the core issue: **Jobs should schedule on weekends when weekend operations are enabled, and move to Monday when disabled**. The implementation should be robust, well-tested, and ready for immediate production deployment.

Please analyze the existing code patterns and maintain consistency with the established architecture while fixing the weekend operation issues.