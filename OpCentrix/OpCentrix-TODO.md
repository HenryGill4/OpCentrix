OpCentrix Logic Issues - TODO List
==================================
Generated: 2024-12-20
Status: Complete Analysis with Implementation Order
C:\Users\Henry\source\repos\OpCentrix\OpCentrix-Implementation-Status-and-TODO.md

CRITICAL PRIORITY (Week 1)
=========================

1. Parts Page Auto-Fill Logic
-----------------------------
File: OpCentrix/Pages/Admin/Shared/_PartForm.cshtml
Issue: Material selection doesn't update form fields
Missing:
- [ ] Material defaults not populating laser power field
- [ ] Scan speed not auto-filling based on material
- [ ] Temperature fields not updating
- [ ] Material cost calculation not working
- [ ] Build volume shrinkage rates not applied
Implementation:
- [ ] Fix updateSlsMaterial() function in _PartForm.cshtml
- [ ] Add material defaults object with all parameters
- [ ] Update form fields when material changes
- [ ] Recalculate estimated hours based on material complexity

2. Job End Time Calculation (Scheduler)
---------------------------------------
File: OpCentrix/wwwroot/js/scheduler-ui.js
Issue: Using AvgDurationDays instead of EstimatedHours
Current: endTime = startTime + AvgDurationDays (wrong)
Should be: endTime = startTime + (EstimatedHours / 24)
Implementation:
- [ ] Add data-estimated-hours to part options in _AddEditJobModal.cshtml
- [ ] Fix updateEndTime() to use hours calculation
- [ ] Handle jobs spanning multiple days
- [ ] Account for admin override durations

3. UTC Time Standardization
---------------------------
Files: Multiple (SchedulerService.cs, Index.cshtml.cs)
Issue: Mix of DateTime.Today and DateTime.UtcNow
Impact: Jobs appear on wrong days in different timezones
Implementation:
- [ ] Replace all DateTime.Today with DateTime.UtcNow.Date
- [ ] Add timezone conversion for display
- [ ] Fix date range queries to use UTC
- [ ] Update all time comparisons to UTC

4. Concurrent Edit Protection
-----------------------------
Issue: No optimistic concurrency control
Risk: Data loss with simultaneous edits
Implementation:
- [ ] Add RowVersion byte[] to all entities
- [ ] Implement DbUpdateConcurrencyException handling
- [ ] Add retry logic with user notification
- [ ] Create conflict resolution UI

HIGH PRIORITY (Week 1-2)
========================

5. Job Validation Performance
-----------------------------
File: OpCentrix/Pages/Scheduler/Index.cshtml.cs
Issue: Loading ALL jobs for overlap validation
Current: O(n) query for every validation
Implementation:
- [ ] Filter jobs by machine and time window
- [ ] Add date range to validation query
- [ ] Index StartTime and EndTime columns
- [ ] Cache validation results per session

6. Duration Display Logic
-------------------------
File: OpCentrix/Pages/Admin/Shared/_PartForm.cshtml
Issue: Inconsistent duration calculations
Missing:
- [ ] Business days calculation (excluding weekends)
- [ ] Holiday calendar integration
- [ ] Min/max duration validation
- [ ] Consistent format (hours vs days)
Implementation:
- [ ] Create BusinessDaysCalculator service
- [ ] Add holiday configuration
- [ ] Standardize duration display format

7. Delete Cascade Protection
----------------------------
Issue: Deleting parts doesn't check active jobs
Risk: Orphaned jobs in database
Implementation:
- [ ] Add referential integrity checks before delete
- [ ] Implement soft delete for parts with IsActive flag
- [ ] Create "in use" validation method
- [ ] Add cascade rules to DbContext

8. Job Status State Machine
---------------------------
File: OpCentrix/Models/Job.cs
Issue: No validation for status transitions
Missing: Invalid status changes allowed
Implementation:
- [ ] Create JobStatusStateMachine class
- [ ] Define valid status transitions
- [ ] Add CanTransitionTo(status) method
- [ ] Log all status changes with timestamp

MEDIUM PRIORITY (Week 2-3)
==========================

9. N+1 Query Problems
---------------------
Files: SchedulerService.cs, various pages
Issue: Multiple queries for related data
Implementation:
- [ ] Add .Include(j => j.Part).Include(j => j.Machine)
- [ ] Create query specifications pattern
- [ ] Implement projection for read operations
- [ ] Add query result caching

10. Material Changeover Logic
-----------------------------
File: OpCentrix/Services/SchedulerService.cs
Issue: Changeover time not considering material compatibility
Implementation:
- [ ] Check previous job material
- [ ] Apply changeover time only for material changes
- [ ] Consider material family compatibility
- [ ] Update cost calculations

11. Modal State Management
--------------------------
File: OpCentrix/wwwroot/js/scheduler-ui.js
Issue: Form data persists between operations
Implementation:
- [ ] Reset form on modal open for new job
- [ ] Clear validation messages
- [ ] Preserve data during validation errors
- [ ] Add unsaved changes warning

12. Grid Positioning at Zoom Levels
------------------------------------
File: OpCentrix/Pages/Scheduler/_MachineRow.cshtml
Issue: Jobs overlap visually at different zooms
Implementation:
- [ ] Recalculate positions based on zoom
- [ ] Add minimum job width constraint
- [ ] Implement collision detection
- [ ] Add job stacking for same day

13. Cost Calculation Completeness
---------------------------------
File: OpCentrix/Services/CostCalculationService.cs (missing)
Issue: Only material costs calculated
Missing:
- [ ] Machine hourly rate
- [ ] Setup/teardown time costs
- [ ] Labor costs
- [ ] Overhead allocation
- [ ] Material waste percentage
Implementation:
- [ ] Create comprehensive cost model
- [ ] Add cost configuration to settings
- [ ] Implement cost approval workflow

LOW PRIORITY (Week 3-4)
=======================

14. Client-Side Validation Enhancement
--------------------------------------
Files: Various modal forms
Issue: Using basic alert() for errors
Implementation:
- [ ] Add inline validation messages
- [ ] Real-time field validation
- [ ] Better error styling
- [ ] Progressive enhancement

15. Machine Utilization Calculation
-----------------------------------
File: OpCentrix/Services/SchedulerService.cs
Issue: GetMachineUtilizationAsync returns dummy data
Implementation:
- [ ] Calculate actual utilization
- [ ] Add efficiency metrics
- [ ] Create utilization reports
- [ ] Predictive scheduling

16. Client-Side Caching
-----------------------
Issue: No caching of static data
Implementation:
- [ ] Cache parts list in localStorage
- [ ] Cache machine capabilities
- [ ] Implement cache versioning
- [ ] Add offline mode support

17. Multi-Stage Job Integration
-------------------------------
Issue: Service exists but not integrated
Implementation:
- [ ] Add UI for multi-stage definition
- [ ] Implement stage dependencies
- [ ] Add progress tracking
- [ ] Update scheduler logic

18. Audit Trail Enhancement
---------------------------
Issue: Limited audit logging
Implementation:
- [ ] Add comprehensive change tracking
- [ ] User action logging
- [ ] Before/after value storage
- [ ] Audit report generation

ADDITIONAL ISSUES FOUND
=======================

19. HTMX Response Handling
--------------------------
File: OpCentrix/Pages/Scheduler/Index.cshtml.cs
Issue: Full page refresh after CRUD operations
Implementation:
- [ ] Return partial views for HTMX requests
- [ ] Update hx-target attributes
- [ ] Remove page reload JavaScript
- [ ] Add proper swap strategies

20. Session Timeout Handling
----------------------------
Issue: No graceful session timeout handling
Implementation:
- [ ] Add session timeout warning
- [ ] Auto-save draft functionality
- [ ] Session refresh mechanism
- [ ] Redirect to login on timeout

21. Error Page Duplication Fix
-------------------------------
Issue: Full error pages inserted by HTMX
Implementation:
- [ ] Return empty content for 404/500 in HTMX
- [ ] Add htmx:beforeSwap validation
- [ ] Implement proper error boundaries
- [ ] Clean error response handling

22. Database Connection Pooling
-------------------------------
Issue: No connection pooling configuration
Implementation:
- [ ] Configure connection pool size
- [ ] Add connection lifetime settings
- [ ] Monitor connection usage
- [ ] Implement retry policies

23. Background Job Processing
-----------------------------
Issue: No background job infrastructure
Implementation:
- [ ] Add Hangfire or similar
- [ ] Move long operations to background
- [ ] Implement job queuing
- [ ] Add progress notifications

24. Real-time Updates
---------------------
Issue: No real-time job updates
Implementation:
- [ ] Add SignalR integration
- [ ] Push job status updates
- [ ] Real-time conflict detection
- [ ] Collaborative editing support

25. Mobile Responsiveness
-------------------------
Issue: Scheduler not mobile-friendly
Implementation:
- [ ] Add responsive grid layout
- [ ] Touch-friendly job editing
- [ ] Mobile-specific views
- [ ] Gesture support

TESTING & VALIDATION REQUIREMENTS
=================================

For Each Fix Above:
- [ ] Unit tests for business logic
- [ ] Integration tests for data access
- [ ] UI tests for user interactions
- [ ] Performance tests for optimizations
- [ ] Security tests for access control

IMPLEMENTATION NOTES
====================

Priority Guidelines:
- CRITICAL: Data integrity and core functionality
- HIGH: Performance and major UX issues  
- MEDIUM: Feature completeness and optimization
- LOW: Nice-to-have enhancements

Development Approach:
1. Fix one category at a time
2. Write tests before fixing
3. Document changes in code
4. Update user documentation
5. Performance test after each fix

Estimated Timeline:
- Week 1: Critical fixes (1-4)
- Week 2: High priority (5-8) 
- Week 3: Medium priority (9-13)
- Week 4: Low priority and testing

Risk Mitigation:
- Create backups before major changes
- Test in staging environment first
- Gradual rollout of fixes
- Monitor error logs closely
- Have rollback plan ready