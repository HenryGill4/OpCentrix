# COMPREHENSIVE SCHEDULER FIX PLAN

## Problem Analysis
The scheduler is posting jobs on Monday instead of Friday/Saturday when weekend operations should be enabled. The issue appears to be in the weekend operation logic and settings integration.

## Root Cause Analysis

### 1. Weekend Operations Logic Issues
- `IsWeekendOperationAllowedAsync` method in SchedulerService may not be correctly evaluating weekend settings
- Weekend validation in `ValidateSchedulingConstraintsAsync` may be blocking legitimate weekend operations
- `IsValidOperatingDay` method may have incorrect logic for determining valid weekend days

### 2. Settings Service Missing
- The code references `ISchedulerSettingsService` but the implementation appears to be missing
- This is causing weekend settings to not be properly loaded or applied
- Default fallback logic may be defaulting to weekdays only

### 3. Date Calculation Issues
- `CalculateNextAvailableStartTimeAsync` may be skipping weekends incorrectly
- `GetNextValidOperatingDay` logic may not account for enabled weekend operations
- UTC/Local time handling may cause date boundary issues

## Fix Implementation Plan

### Phase 1: Implement Missing SchedulerSettingsService
1. Create `ISchedulerSettingsService` interface
2. Implement `SchedulerSettingsService` class with proper weekend operation methods
3. Register service in dependency injection
4. Add database seeding for default scheduler settings

### Phase 2: Fix Weekend Logic in SchedulerService
1. Fix `IsWeekendOperationAllowedAsync` to properly check settings
2. Update `IsValidOperatingDay` to respect weekend operation flags
3. Fix `GetNextValidOperatingDay` to include enabled weekends
4. Update `CalculateNextAvailableStartTimeAsync` weekend handling

### Phase 3: Add Settings Management UI
1. Create admin settings page for scheduler configuration
2. Add weekend operation toggle controls
3. Implement settings validation and persistence
4. Add settings change audit logging

### Phase 4: Testing and Validation
1. Add comprehensive unit tests for weekend scenarios
2. Test Saturday/Sunday job scheduling
3. Validate settings persistence and application
4. Test edge cases around weekend boundaries

## Files to Modify

### New Files to Create:
1. `OpCentrix/Services/ISchedulerSettingsService.cs`
2. `OpCentrix/Services/SchedulerSettingsService.cs`
3. `OpCentrix/Pages/Admin/SchedulerSettings.cshtml`
4. `OpCentrix/Pages/Admin/SchedulerSettings.cshtml.cs`
5. `OpCentrix/Models/SchedulerSettings.cs` (if not exists)

### Existing Files to Modify:
1. `OpCentrix/Services/SchedulerService.cs` - Fix weekend logic
2. `OpCentrix/Program.cs` - Register new service
3. `OpCentrix/Data/SchedulerContext.cs` - Ensure SchedulerSettings table
4. `OpCentrix/Pages/Scheduler/Index.cshtml.cs` - Update job creation logic
5. `OpCentrix/Pages/Shared/_AdminLayout.cshtml` - Add settings navigation

## Database Changes Needed

### SchedulerSettings Table (may already exist):
- Ensure EnableWeekendOperations column exists
- Ensure SaturdayOperations column exists  
- Ensure SundayOperations column exists
- Add default record with proper weekend settings

## Testing Scenarios

### Weekend Job Scheduling Tests:
1. Schedule job for Friday - should work
2. Schedule job for Saturday with weekend operations enabled - should work
3. Schedule job for Saturday with weekend operations disabled - should move to Monday
4. Schedule job for Sunday with weekend operations enabled - should work
5. Schedule job for Sunday with weekend operations disabled - should move to Monday

### Settings Integration Tests:
1. Toggle weekend operations on/off
2. Verify settings persistence
3. Test settings application in real-time
4. Validate settings defaults

## Expected Behavior After Fix

### When Weekend Operations Are ENABLED:
- Friday jobs should schedule for Friday
- Saturday jobs should schedule for Saturday
- Sunday jobs should schedule for Sunday
- Scheduler should respect weekend shift hours

### When Weekend Operations Are DISABLED:
- Friday jobs should schedule for Friday (if within business hours)
- Saturday jobs should move to next Monday
- Sunday jobs should move to next Monday
- No weekend scheduling should be allowed

## Priority Order

1. **HIGH**: Implement SchedulerSettingsService (blocks all other fixes)
2. **HIGH**: Fix weekend validation logic in SchedulerService
3. **MEDIUM**: Add settings management UI
4. **LOW**: Add comprehensive testing

## Success Criteria

1. Jobs can be scheduled on weekends when weekend operations are enabled
2. Jobs are properly moved to Monday when weekend operations are disabled
3. Settings can be managed through admin interface
4. All weekend scenarios work correctly
5. No regression in weekday scheduling

## Next Steps

1. Start with implementing the missing SchedulerSettingsService
2. Update SchedulerService weekend logic
3. Test basic weekend scheduling functionality
4. Add admin settings UI
5. Perform comprehensive testing

## Technical Notes

- All datetime operations should use UTC consistently
- Weekend logic should account for different time zones
- Settings changes should take effect immediately
- Proper error handling for settings service failures
- Audit logging for settings changes