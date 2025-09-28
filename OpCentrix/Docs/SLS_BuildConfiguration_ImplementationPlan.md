# SLS Build Configuration Implementation Plan

Version: 1.0
Owner: Parts / SLS Enhancements
Status: Approved (per user responses)

## 1. Final Decisions (Q1–Q6)
Q1 Keep existing property name `PartsPerBuild` for single? Answer: NO ? Introduce explicit `PartsPerBuildSingle` (do not reuse / overload any legacy field).  
Q2 Separate parts counts for Double/Triple? YES ? Add `PartsPerBuildDouble`, `PartsPerBuildTriple`.  
Q3 Store enable flags? YES ? Add `EnableDoubleStack`, `EnableTripleStack`.  
Q4 Show stage estimate vs observed comparison panel? YES.  
Q5 Keep EDM as batch stage? YES, but note: "a new cut for each stack" (operational note – tracked for future refinement; still treated as batch in current stage estimate logic).  
Q6 Need “Active Mode” selection now? NO.

## 2. Data Model Changes (Entity + Migration)
Modify `Part` entity (add if not existing):
- int `PartsPerBuildSingle` (NOT NULL, default 1)
- int? `PartsPerBuildDouble`
- int? `PartsPerBuildTriple`
- double? `SingleStackDurationHours` (already exists – now observational only)
- double? `DoubleStackDurationHours`
- double? `TripleStackDurationHours`
- bool `EnableDoubleStack` (default false)
- bool `EnableTripleStack` (default false)

Backward compatibility / defaulting:
- Migration script sets `PartsPerBuildSingle = 1` where null/unset.
- If a deprecated `PartsPerBuild` column exists (verify) and `PartsPerBuildSingle` is null: copy value ? `PartsPerBuildSingle`.
- If `SingleStackDurationHours` has value but no legacy parts-per-build, still set `PartsPerBuildSingle = 1`.

Indexes: None needed (all scalar descriptive fields).

## 3. EF Core Migration Steps
1. Add new properties to `Part` model partial.
2. `Add-Migration AddSlsBuildConfiguration` ? generates columns.
3. In `Up()`:
   - Add columns with defaults.
   - Data backfill block (raw SQL):
     - If legacy `PartsPerBuild` exists: UPDATE new single column.
     - Else set `PartsPerBuildSingle = 1` where null.
4. In `Down()` drop added columns only.

## 4. Database (Raw SQL Fallback – if manual)
```
ALTER TABLE Parts ADD PartsPerBuildSingle int NOT NULL DEFAULT(1);
ALTER TABLE Parts ADD PartsPerBuildDouble int NULL;
ALTER TABLE Parts ADD PartsPerBuildTriple int NULL;
ALTER TABLE Parts ADD EnableDoubleStack bit NOT NULL DEFAULT(0);
ALTER TABLE Parts ADD EnableTripleStack bit NOT NULL DEFAULT(0);
ALTER TABLE Parts ADD DoubleStackDurationHours float NULL;
ALTER TABLE Parts ADD TripleStackDurationHours float NULL;
-- Backfill (conditional examples)
UPDATE Parts SET PartsPerBuildSingle = COALESCE(Legacy.PartsPerBuild, 1)
FROM Parts p -- adjust if legacy column exists
```
(Adjust if schema / table naming differs.)

## 5. UI / Razor Changes
Location: Remove separate “Build Config” tab. Add new card inside existing Basic tab for Part create/edit.
Card: "SLS Build Configuration"
Sections:
A. Build Modes Table
| Mode | Enabled (toggle except Single) | Parts / Build (input) | Observed Duration (hrs, step 0.1) | Derived Hours / Part (readonly) |
- Single: always enabled (no toggle).
- Double/Triple: show toggle bound to `EnableDoubleStack`, `EnableTripleStack`.
- Derived = duration / parts (if both >0, else blank).

B. Stage-Based Estimate (comparison panel)
- Show: Stage Estimated Total (Single Stack): computed client-side (see §7 logic) as `stageEstimateSingle`.
- If `SingleStackDurationHours` present: show diff badge (Observed ? Estimate) with color coding (small abs diff = neutral, large positive = warning).

C. (Future) Notes: placeholder `<textarea>` hidden or commented (not persisted now).

Hidden inputs to carry: selected stage IDs, new stacking fields, flags, stage estimate hours.

## 6. Client State & JavaScript
State object:
```
state = {
  parts: { single: number, double: number|null, triple: number|null },
  durations: { single: number|null, double: number|null, triple: number|null },
  enabled: { double: bool, triple: bool },
  stages: Map(stageId => { type: 'per-part'|'batch', cycleMinutes?: number, manualHours?: number }),
  stageEstimateSingle: number // computed
};
```
Functions (NO auto-overwrite of user durations):
- `recalcStageEstimate()` ? updates `state.stageEstimateSingle`.
- `updateDerivedHoursPerPart()` ? recompute derived cells in table.
- `syncFormHiddenFields()` ? push current state into hidden inputs for server model binding.
- `validate()` ? enforce rules (see §8). Blocks submit with inline messages.
- Event bindings: on input change for parts/durations/toggles triggers update + sync.

All JavaScript isolated inside an IIFE or page-scope module (e.g. `window.SlsBuildConfig = {...}`) to avoid pollution.

## 7. Stage Estimate Calculation Rules
- Keep existing stage selection UX.
- For per-part stages: `hours = (cycleMinutes / 60) * PartsPerBuildSingle` ONLY (ignore double/triple for estimate – per requirements).
- For batch stages: `hours = manualHours` (existing field).
- Sum = `stageEstimateSingle`.
- Do NOT assign to `SingleStackDurationHours` automatically.
- Display only.
- EDM remains batch: included once in sum (note: operational comment "new cut for each stack" captured separately for potential future per-stack modeling; current scope unaffected).

## 8. Validation Rules (Client & Server)
Trigger only when approach = SLS-Based OR Hybrid-Approach.
Required:
- Single: `PartsPerBuildSingle >= 1` AND `SingleStackDurationHours > 0`.
- If `EnableDoubleStack`: `PartsPerBuildDouble >= 1` AND `DoubleStackDurationHours > 0`.
- If `EnableTripleStack`: `PartsPerBuildTriple >= 1` AND `TripleStackDurationHours > 0`.
- Stage selection: at least one stage (existing rule).
No mutation of durations after user edits.
Server-side: replicate checks in handler (`OnPost...`) to avoid trust on client.

## 9. Submission Payload (Form Post)
Add/ensure binding for:
- `SelectedStageIds` (existing)
- `StageEstimatedHours` (single-stack estimate) – unchanged name if already present; else add hidden input.
- `PartsPerBuildSingle`, `PartsPerBuildDouble?`, `PartsPerBuildTriple?`
- `SingleStackDurationHours`, `DoubleStackDurationHours?`, `TripleStackDurationHours?`
- `EnableDoubleStack`, `EnableTripleStack`.

Server maps to Part entity fields. Only observed durations persist.

## 10. Complexity Badge Logic Update
Source of complexity hours base:
- Use `SingleStackDurationHours` if present > 0.
- Else fallback to `stageEstimateSingle`.
Pass resulting hours into existing complexity tier logic (no change to tiers unless required).

## 11. EDM Operations Note
- Still treated as batch stage (manual hours) in estimate.
- Operational note: each stack will require separate EDM cut; future enhancement may introduce stack-mode-specific downstream durations. Track via backlog item: `EDM_MultiStack_Modeling`.

## 12. Approach Switching Behavior
If user changes approach away from SLS-Based / Hybrid:
- Hide card (CSS/JS).
- Retain form values in memory but skip SLS validation on submit (server also checks approach before enforcing SLS rules).

## 13. Migration / Backfill Logic
Order:
1. Deploy migration adding columns.
2. Backfill `PartsPerBuildSingle` (copy legacy or set to 1).
3. Leave Double/Triple null (interpreted as disabled until toggled).
4. Set flags = false by default.
5. No recalculation of existing durations.
6. QA: pick sample legacy part and verify values unaffected.

## 14. Testing Plan
Unit / Integration:
- Model validation for enabling double/triple invalid inputs.
- Migration test (in-memory or test DB) verifying default = 1.
- Stage estimate function with mixed per-part & batch stages.
- Complexity fallback when observed single missing.
- Approach toggle hides validations.
UI / Manual:
- Enter durations, toggle modes off/on (values persist on re-enable).
- Change PartsPerBuildSingle – estimate updates; durations untouched.
- Comparison diff badge renders correct sign & formatting.
Regression:
- Existing part create/edit without SLS approach unaffected.
- Scheduler consumption of `EstimatedHours` remains uninterfered (ensure we do not replace scheduler logic prematurely).

## 15. Rollout Strategy
- Single migration (backward-compatible – additive columns only).
- Feature behind implicit condition (display only when approach qualifies). No runtime feature flag required.
- Can be deployed before UI if needed (columns unused until UI shipped).

## 16. Accessibility & UX Notes
- Provide aria-labels for derived read-only fields.
- Use input `step="0.1"` and min="0.1" for durations.
- Show inline validation messages near fields; summary at top on failure.

## 17. Performance / Safety
- All calculations client-side trivial O(n) where n = stage count.
- No polling / event spam; debounce derived hours updates if performance jitter observed.

## 18. Task Breakdown (Sequential Execution Checklist)
1. Create migration adding new columns & backfill logic.
2. Update `Part` model with new properties (if not already complete).
3. Regenerate / apply EF migration and update DB.
4. Remove legacy Build Config tab markup.
5. Add SLS Build Configuration card markup & hidden fields to `_MasterPartForm.cshtml` (or appropriate partial).
6. Implement JS module for state & functions (inject after form partial; ensure namespacing).
7. Bind inputs to JS state (parts, durations, toggles).
8. Implement stage estimate computation using existing stage list source.
9. Implement derived hours-per-part display & live update.
10. Implement validation (client) + server enforcement in `OnPost`.
11. Add comparison panel (estimate vs observed) + diff badge styling.
12. Add complexity badge fallback logic update (server or client depending on current architecture).
13. QA manual scenarios (Single only, enabling/disabling Double/Triple, clearing durations).
14. Add / update unit tests (model validation, complexity selection logic).
15. Document operational note for EDM multi-stack future enhancement.
16. Code review & merge.
17. Production deploy.

## 19. Future Backlog (Not in Scope Now)
- Stack-specific downstream (EDM / CNC) modeled durations.
- Historical analytics on observed vs estimated variance per part.
- Auto-suggestion of durations based on historical rolling average (non-mutating, suggestion only).
- Mode-specific scheduling optimization.

## 20. Acceptance Criteria Summary
- New fields persist and appear in DB.
- User can enter independent observed durations for 1x/2x/3x modes without automatic overwrites.
- Stage estimate visible & diff badge shown when both values present.
- Validation blocks incomplete SLS configurations only when SLS/Hybrid approach active.
- Existing non-SLS parts unaffected.

---
End of Plan.
