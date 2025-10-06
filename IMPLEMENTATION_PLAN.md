# Maintenance V2 + Operational Tasks Implementation Plan

## Conventions (Global)
- Always use the acronym `SLS` (Selective Laser Sintering). Never use `SSL` in code, comments, UI, logs, or documentation.
- Do not use emojis anywhere (Razor, C#, JS, logs, comments). Use semantic SVG / icon components (Heroicons / FontAwesome) instead.
- Standard log tag replacements (text form only):
  - Success -> [OK]
  - Warning -> [WARN]
  - Error -> [ERR]
  - Initialization / maintenance / setup -> [INIT] / [MAINT]
- Prefer consistent log prefix: `[AREA][OPID] Message` (AREA examples: PT, PARTS, SCHED, SITE).
- UI status indicators: use colored badges + SVG icons (no emoji fallbacks).

## CURRENT DB STATE (from `MaintenanceV2_ManualSchema_Log.md`)
Tables present: MaintenanceAsset, MaintenanceProcedureTemplate, MaintenanceFactorDefinition, MaintenanceProcedureTemplateFactor, MaintenanceScheduleInstance, MaintenanceOccurrence, MaintenanceCounterAggregate, MaintenanceCounterBaseline, OperationalTask.

OperationalTask columns (manual SQL baseline) now extended in code with `ConfigJson` + `OverdueFlag` (additive patch path via service auto-patch logic when `AUTO_PATCH_DB=1`).

### Reconciliation SQL (SQLite Additions)
```sql
-- Add OverdueFlag if missing (manual equivalent of auto-patch)
ALTER TABLE OperationalTask ADD COLUMN OverdueFlag INTEGER NULL;
CREATE INDEX IF NOT EXISTS IX_OperationalTask_OverdueFlag ON OperationalTask(OverdueFlag);
```

## Phase 1 (Delivered + Immediate Hardening Additions)
(Completed previously – summary retained for traceability)
1. DB Column Patch **DONE**
2. Validation Hardening **DONE**
3. Duplicate Suppression **DONE**
4. Interval Summary in List **DONE**
5. Background Safety / Auto Patch **DONE**
6. Logging Standardization (initial pass) **DONE**
7. Unit Tests (create + suppression + persistence) **DONE**

## Phase 1.5 (Interval Engine Bootstrap)
Complete with material-aware intervals + evaluation tests.

## Phase 2 (Extended Roadmap) – OverdueFlag + Assignment groundwork
- OverdueFlag persistence & UI badge integrated.
- Upcoming: Assignment workflow, paging, filters.

## Phase 3 (Advanced Automation)
(unchanged)

## Updated Immediate Action Checklist
1. Column patch / index **DONE**
2. Duplicate suppression **DONE**
3. Column existence guard **DONE**
4. Interval summary in list **DONE**
5. Evaluation service + hosted trigger **DONE**
6. Unit tests (create + suppression) **DONE**
7. Schema log updated **DONE**
8. BuildCount evaluation **DONE**
9. MachineHours evaluation **DONE**
10. Numeric sanitation inside evaluation **DONE**
11. Evaluation decision logging **DONE**
12. Material-specific enhancement **DONE**
13. OverdueFlag persistence change **DONE**
14. Add evaluation-focused unit tests **DONE**
15. OverdueFlag UI badge **DONE**
16. Assignment workflow endpoints **PENDING**
17. Task paging & filters **PENDING**

## Verification (Latest Successful Build)
- Build successful after Razor partial refactor (`_TaskList`) adding Overdue badge and interval summary stabilization.

## Outstanding Risks / Next Steps
- Assignment workflow + server-side paging to prevent large list rendering.
- Consider adding HTMX pagination controls (limit 25 + load more).
- Add filter badges (Machine, Overdue only).

## Manual Test Guidance (Current State)
1. Create overdue task (FixedDate past) -> OVERDUE badge displays in list.
2. Trigger interval due (BuildCount threshold) -> shows due time badge (not overdue) until past.
3. Modify DueAt manually to past and refresh -> Overdue badge appears.

## Session Status
- Overdue UI integrated; ready to implement assignment endpoints and paging next.

## Recent Progress Log
- Added OverdueFlag UI badge to task list. [OK]
- Fixed create task modal antiforgery issue by injecting @Html.AntiForgeryToken() inside form. [OK]

---
