# Scheduler Module Overview

Concise guide to jump back into development of the modern Scheduler (Razor Pages, .NET 8).

## Core Purpose
Plan, visualize, and manipulate manufacturing/printing jobs across multiple machines with integration to Print Tracking and downstream operations.

## Primary PageModel
File: Pages/Scheduler/Index.cshtml.cs (IndexModel)
Attribute: [SchedulerAccess] (role/permission gate)
Pattern: Single Razor Page hosting multiple HTMX-driven partial interactions.

## Key Domain Entities (simplified)
- Job: Scheduled manufacturing/print activity (core record manipulated here)
- Part: Source of default parameters (cost/duration/material)
- Machine: Capability + availability metadata (IsActive, IsAvailableForScheduling)
- BuildJob: Print tracking entity (created when job starts on SLS/INC machines)

## Injected Services
- SchedulerContext (EF Core DbContext)
- ISchedulerService (date grid, layout logic, future optimization hooks)
- IMachineManagementService (active machines + seeding fallback)
- ITimeSlotService (next available slot computation)
- IPrintTrackingService (bridge when a print starts/completes)
- ILogger<IndexModel>

## High-Level Request Flow (Initial Page Load)
OnGetAsync:
1. LoadAvailableMachinesAsync (with auto-seed fallback)
2. GetSchedulerData (grid dates + base structure)
3. Sync ViewModel.Machines with DB machines
4. LoadAvailablePartsAsync
5. LoadJobsAsync (range = visible window ±1 day) including Part navigation
6. GenerateSummaryAsync (hours + counts per machine)
7. Log diagnostics; handle exceptions with empty fallback

## ViewModel Composition
SchedulerPageViewModel (not shown in file but expected members):
- StartDate, Dates (range of days rendered)
- Machines (list<string> machine IDs)
- Jobs (List<Job>)
- MachineRowHeights (layout hints)
FooterSummaryViewModel:
- MachineHours (machine -> total scheduled hours)
- JobCounts (machine -> job count)
- Aggregated totals (TotalJobs, TotalHours computed properties)

## Modal / HTMX Interaction Cycle
Open Modal: OnGetShowAddModalAsync(machineId, date, id?)
- Parse date
- Load existing job OR create provisional job (CreateNewJobAsync uses ITimeSlotService)
- Ensure machines + parts loaded
- Return partial _AddEditJobModal with AddEditJobViewModel

Submit (Create/Update): OnPostAddOrUpdateJobAsync(CreateJobDto)
1. Reload machines/parts (validation context)
2. Validate (custom JobValidationResult)
3. If invalid: return same partial with errors list
4. If valid & Id==0: CreateJobFromDtoAsync
5. Else: UpdateJobFromDtoAsync
6. HTMX: return JS snippet to close modal + reload page
7. Non-HTMX: redirect with TempData

Delete: OnDeleteJobAsync / OnPostDeleteJobAsync (GET & POST for flexibility)
- DeleteJobInternalAsync returns JS snippet performing UI cleanup + page reload

## Print Tracking Integration Points
Start Print: OnPostStartPrintJobAsync(jobId)
- Transition status -> Building
- Create BuildJob via IPrintTrackingService (CreateBuildJobFromScheduledJobAsync)
- Return JSON (used by client to navigate to print tracking dashboard)

Status Poll: OnGetPrintTrackingStatusAsync(jobId)
- Returns combined Job + BuildJob snapshot JSON

Completion Feedback: OnPostUpdateFromPrintTrackingAsync(jobId,...)
- Update Job status -> Completed, set ActualEnd
- Append notes; return JSON notification payload

## Duration & Slot Intelligence
Suggest Next Slot: OnGetSuggestNextTimeAsync(machineId, duration)
- Delegates to ITimeSlotService.GetNextAvailableTimeAsync
Adjust Duration Inline: OnPostUpdateJobDurationAsync(jobId, newDurationHours)
- Updates ScheduledEnd + EstimatedHours
- Recomputes part EstimatedHours (learned per-part average) = (newDuration / quantity)
- Optionally notifies print tracking service for SLS/INC machines

## DTO & Validation Strategy
CreateJobDto / EditJobDto unify form binding (BindProperty on PageModel for edit; method parameter for create/update post). Validation performed manually (no DataAnnotations) enabling contextual machine availability checks.
Validation Rules:
- MachineId required + existence + IsActive + IsAvailableForScheduling
- PartId > 0
- Start < End
- Duration bounds: 15 min <= duration <= 168 hrs
Errors aggregated into JobValidationResult then pushed into ModelState and error list for modal.

## Resilience Patterns
- Extensive try/catch per interaction to prevent page crash
- Logging includes random operationId for correlation
- Machine seeding fallback if none exist
- Modal error fallback returns partial with inline error list
- Final fallback: inline <script>alert(...); reload

## HTMX Response Pattern
Instead of raw JSON, returns executable <script> that:
- Closes modal (DOM cleanup + backdrop removal)
- Triggers UI notifications (showSuccessNotification/showToast fallback to alert)
- Schedules full page reload for fresh grid state

## Extension Points / TODO Opportunities
1. Conflict Detection: Before saving job check overlapping jobs on same machine.
2. Capacity View: Pre-compute daily utilization percentages.
3. Partial Refresh: Instead of full page reload, return updated row or day column (optimize UX).
4. Validation Refactor: Introduce FluentValidation for cleaner rules & client hints.
5. Concurrency: Add optimistic concurrency token on Job (rowversion) to prevent overwrites.
6. Soft Delete: Track deletions for audit; currently permanent remove.
7. Timeline Scaling: Support zoom granularities (hourly blocks) through ISchedulerService.
8. WebSocket / SignalR: Replace reload script with live incremental updates across sessions.
9. Access Control Refinement: Per-machine or per-department scheduling rights.
10. Cost Projection: Integrate cost fields already present into footer summary.

## Error Hotspots to Watch
- Null Part on update (validate Part exists before editing)
- Time zone differences (Utc usage consistent? Ensure UI converts correctly)
- Large date ranges (optimize queries; current load filters by window ±1 day)
- Race conditions when two users create overlapping jobs simultaneously

## Quick Debug Checklist
If jobs not appearing:
- Confirm LoadJobsAsync filter window (StartDate alignment)
- Check machine IDs in ViewModel.Machines match Job.MachineId
- Inspect logs for machine seeding fallback warnings
If modal fails to close:
- Confirm HX-Request header present (HTMX request) on form submit
- Ensure script not blocked (Content-Type = text/html)
If duration suggestions off:
- Verify ITimeSlotService implementation returns future slot respecting existing Jobs

## Adding a New Feature Example (Conflict Detection)
1. Before create/update save: query overlapping jobs
2. If conflict found: add validation error and return modal partial
3. Optionally supply suggestion via ITimeSlotService
4. Re-run summary after successful persistence (currently full reload handles this)

## Data Access Performance Notes
- All queries using AsNoTracking for read operations (good for grid)
- Creation/update reads single Part with FindAsync (EF cache advantage)
- Consider batching machine/part loads with caching if scale grows

## Logging Conventions
- Emoji prefixes categorize action (?? load, ?? process, ? success, ?? warn, ? error, ?? time, ?? status, ??? delete)
- operationId scoped per handler for log correlation (8-char GUID substring)

## Security
- Access gated by [SchedulerAccess] (policy not shown)
- User identity stamped on CreatedBy / LastModifiedBy where applicable
- Consider auditing deletions & status transitions

## Fast Start: Where to Edit What
- Grid layout / date math: ISchedulerService implementation
- Slot logic: ITimeSlotService
- Machine availability: IMachineManagementService (enqueue maintenance, disable)
- Modal fields / form: _AddEditJobModal.cshtml + CreateJobDto
- Validation rules: ValidateJobRequestAsync
- UX after save/delete: HandleSchedulerSuccess / DeleteJobInternalAsync scripts

## Minimal Mental Model
Machines (active) -> Date Grid (ISchedulerService) -> Jobs filtered by window -> Summary aggregates.
Modal orchestrates single job lifecycle with DTO + manual validation; responses use HTMX script pattern to refresh whole view.
Print tracking augments lifecycle (Scheduled -> Building -> Completed) with cross-service creation of BuildJob.

## Immediate Hardening Suggestions (Safe Quick Wins)
- Add conflict overlap check
- Add try/catch around part load in UpdateJobFromDtoAsync
- Normalize DateTime to UTC consistently (ensure UI form posts in UTC or convert)
- Introduce cancellation tokens on async methods

## Future Refactor Targets
- Extract validation to separate service (IJobValidationService)
- Introduce mediator pattern for commands (CreateJob, UpdateJob, DeleteJob)
- Replace script string returns with a unified PartialResult + client-side event bus

End of document.
