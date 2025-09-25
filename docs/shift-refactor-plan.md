# Shift Management Refactor Plan (Razor Pages, .NET 8)

Objectives
- Make the Shifts page fully functional and reliable.
- Auto-seed default shifts and display them clearly.
- Allow assigning employees (users) to machines (with primary/secondary) and warn on double assignment.
- Integrate softly with the Scheduler (enforcement and UX signals), without blocking Admin overrides.

Default operating hours to implement
- Monday–Friday:
  - Day shift: 06:00 – 15:30
  - Night shift: 15:30 – 00:00 (crosses midnight)
- Saturday–Sunday:
  - Single 12h shift: 06:00 – 18:00
- These will be created as Global (no MachineId) OperatingShifts on first load if no shifts exist. Machine-specific shifts can still be added and will override Global for that machine.

Eligible roles for operator assignment
- Operator, PrintingSpecialist, EDMSpecialist, MachiningSpecialist, CoatingSpecialist, QCSpecialist

Double-assignment behavior
- Allowed with a confirmation popup if a user is already a primary operator on another machine during overlapping effective dates.
- Do not block; return a warning payload and allow Admin to confirm.

Phases and tasks

[X] Phase 1: Data model and services
- Entity: MachineOperatorAssignment
  - Id (int)
  - MachineId (string, FK reference to Machine.MachineId)
  - UserId (int, FK to Users.Id)
  - IsPrimary (bool)
  - EffectiveFrom (DateTime?)
  - EffectiveTo (DateTime?)
  - IsActive (bool, default true)
  - CreatedDate (utc), CreatedBy
  - LastModifiedDate (utc), LastModifiedBy
- EF: SchedulerContext
  - Add DbSet<MachineOperatorAssignment>
  - Fluent config:
    - Property lengths (MachineId up to 50)
    - Indexes: MachineId, UserId, IsActive, (MachineId, IsPrimary), (UserId, IsActive)
    - Optional check constraint: EffectiveTo >= EffectiveFrom when both set
- Service: IOperatorAssignmentService
  - Task<List<MachineOperatorAssignment>> GetAssignmentsByMachineAsync(string machineId)
  - Task<List<MachineOperatorAssignment>> GetAssignmentsByUserAsync(int userId)
  - Task<MachineOperatorAssignment?> GetAsync(int id)
  - Task<(bool Success, string? Warning, MachineOperatorAssignment? Assignment)> AssignOperatorAsync(string machineId, int userId, bool isPrimary, DateTime? from, DateTime? to, string actor)
  - Task<bool> UnassignAsync(int assignmentId, string actor)
  - Task<bool> SetPrimaryAsync(int assignmentId, string actor)
  - Task<string?> CheckDoubleAssignmentWarningAsync(int userId, string? excludingMachineId = null)
- Acceptance criteria:
  - CRUD works, double-assignment warnings surface via return tuple, not exceptions.

[X] Phase 2: Admin users (employees)
- Admin/Employees Razor Pages
  - Index: list employees, filter by Role, toggle active (satisfied by existing /Admin/Users page; toggle via edit modal)
  - Create/Edit: Username, FullName, Email, Role, IsActive, Password bootstrap (hash only)
  - Validation: unique Username/Email; Role from UserRoles.AllRoles
- Acceptance criteria:
  - Admin can create/update users and mark Operator-type roles active. (Met)

[ ] Phase 3: Shifts page refactor
- Seeding defaults
  - OnGet: if no OperatingShifts, seed default global shifts (above) and show a temporary banner “Default shifts applied.”
- UI/UX improvements
  - Weekly calendar remains; show Machine scope tag (Global or specific MachineId)
  - Add “Assignments” drawer/modal:
    - Machine selector (dropdown of active machines)
    - List current assignments for selected machine (primary badge)
    - Add operator (dropdown of eligible roles), IsPrimary checkbox, optional effective dates
    - Remove/Unassign buttons
    - On assign: if potential double-assignment for primary overlaps, show confirm; proceed on confirm
- Handlers
  - OnGetAssignments(machineId) -> partial list (HTMX target in drawer)
  - OnPostAssign(machineId, userId, isPrimary, effectiveFrom, effectiveTo) -> JSON { success, warning?, assignment }
  - OnPostUnassign(assignmentId) -> JSON { success }
  - OnGetLoadDefaults() -> internal; called from OnGet if no shifts exist
- Acceptance criteria:
  - Add/edit/delete/toggle shifts fully functional (kept existing flow), assignments panel fully functional.

[ ] Phase 4: Scheduler tie-in (soft enforcement)
- When creating jobs:
  - If no active assignment for selected machine, return a non-blocking warning in modal: “No operator assigned to this machine.” Allow Admin override; for non-Admin/Scheduler, suggest contacting Admin.
- SuggestNextTime response:
  - Include primary operator for machine if available (for UI hints)
- Acceptance criteria:
  - Jobs can still be scheduled by Admin/Scheduler; operators see hints.

[ ] Phase 5: Validation/business rules
- Shifts
  - Continue conflict checks; allow cross-midnight by existing IsTimeWithinShift logic
- Assignments
  - Only one active primary per machine at a time (enforced on SetPrimary/Assign); multiple secondary allowed
  - Double-assignment detection returns warning message with machine/user details and overlapping timeframe

[ ] Phase 6: QA and roll-out
- Smoke tests: add default seed, create/edit shifts (global & machine-specific), assign/unassign operators, double-assignment confirm, Scheduler warning presence
- Performance: ensure indexes are in place and pages remain responsive

Technical details
- Cross-midnight shift handling:
  - Night shift: 15:30–00:00 will be saved with EndTime < StartTime; OperatingShift.IsTimeWithinShift already treats this as crossing midnight
- Default shifts seeding (global):
  - Mon–Fri: (Day) 06:00–15:30; (Night) 15:30–00:00
  - Sat–Sun: (Weekend) 06:00–18:00
- HTMX integration points:
  - Shifts page keeps existing HTMX wiring for forms; add endpoints for Assignments drawer
- Security/Authorization:
  - Shifts page: [Authorize(Policy = "AdminOnly")]
  - Employees page: [Authorize(Policy = "AdminOnly")]
  - Assignment endpoints: Admin only

Deliverables
- Entity + DbSet + EF config + service interface/impl
- Admin/Employees Razor Pages (Index/Create/Edit)
- Shifts page: default seeding + assignments drawer/modal + handlers
- Soft tie-in in Scheduler for warnings & primary operator hints
- Documentation: this plan + brief README section for Admin usage

Notes
- SQLite constraints are limited; we will enforce most rules in service code and via indexes.
- We will keep the current temporary bypass in Scheduler shift checks until the Shifts/Assignments are deployed and verified.
