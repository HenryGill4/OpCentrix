# MasterPart SLS Build Configuration Refactor – Consolidated Context Bundle

This document aggregates the full refactor plan plus key extracted code/context so a follow?up implementation session can proceed without re-loading large files.

---
## 1. Refactor Objectives (Original Plan)

### Remove / Decommission
- Entire "Build Config" (stacking) tab + its tab-pane UI (stacking, timing, capacity calculator).
- Legacy stacking fields / logic: `AllowStacking`, `MaxStackCount`, prior stacking-derived duration auto-sum behavior, double/triple stack duration fields in that tab.
- Any JS that:
  - Auto-populates `SingleStackDurationHours` from stage totals (now observed value only).
  - References legacy `partsPerBuild` placeholder inside old stacking logic.

### Add / Introduce (UI – inside Basic tab)
Insert new card: "SLS Build Configuration" (after existing approach/material row) visible only when `ManufacturingApproach` in { `SLS-Based`, `Hybrid-Approach` }.

Card Sections:
A. Build Modes table
- Rows: Single (always enabled), Double (toggle), Triple (toggle)
- Columns: Enable (switch for Double/Triple), Observed Parts/Build (int), Observed Duration (h, step 0.1), Derived Hours/Part (readonly = duration / parts)

B. Stage-Based Estimate Panel
- Shows: `Stage Estimated Total (Single Stack)` (computed), Observed Single (from Single mode duration), Diff badge (Observed ? Estimate, with ±10% thresholds: within=secondary, over>10%=warning, under>10%=info)

C. Hidden field: `StageEstimateSingle` (persist optional analytics)

### Model Additions (if absent)
```
int PartsPerBuildSingle (default 1)
int? PartsPerBuildDouble
int? PartsPerBuildTriple
bool EnableDoubleStack
bool EnableTripleStack
double? SingleStackDurationHours   (observed – DO NOT overwrite)
double? DoubleStackDurationHours
double? TripleStackDurationHours
double? StageEstimateSingle (optional persistence)
```
Migration rule: If legacy `SingleStackDurationHours` has value and new `PartsPerBuildSingle` null/0 ? set `PartsPerBuildSingle = previous generic parts-per-build or 1`.

### Stage Calculation Rules
- Stage selection logic unchanged.
- Per?part stages: contribution = `(cycleMinutes / 60) * PartsPerBuildSingle`.
- Batch stages (`SLS Printing`, `EDM Operations`): use manually entered hours directly.
- Sum = `stageEstimateSingle`; never overwrites observed `SingleStackDurationHours`.
- (Future extension: EDM extra cut per stack not modeled yet.)

### JavaScript Target State
State object:
```
state = {
  stages: Map(stageId => { name, isBatch, cycleMin?, hours }),
  parts: { single, double?, triple? },
  durations: { single?, double?, triple? }, // observed
  enabled: { double: bool, triple: bool },
  stageEstimateSingle: number
}
```
Functions:
- `recalcStageEstimate()`
- `updateDerivedHoursPerPart()`
- `updateComplexity()` (uses observed single hours else stage estimate)
- `syncFormHiddenFields()` (push new fields before submit)
- `validate()` (conditional rules listed below)
- `onManufacturingApproachChange()` (toggles card visibility; keeps values, relaxes validation when hidden)
- `onStageChange` / cycle edits ? recompute estimate ? diff badge ? complexity
- No automatic mutation of observed duration inputs.

### Validation Rules
Always: required basic fields + ?1 stage.
If SLS card visible:
- Single: `PartsPerBuildSingle ? 1` AND `SingleStackDurationHours > 0`.
- If `EnableDoubleStack`: `PartsPerBuildDouble ? 1` AND `DoubleStackDurationHours > 0`.
- If `EnableTripleStack`: analogous.
Stage estimate is advisory only (no blocking if mismatch).

### Submission Payload (Ensure)
```
SelectedStageIds
StageEstimatedHours (existing)
PartsPerBuildSingle, PartsPerBuildDouble?, PartsPerBuildTriple?
EnableDoubleStack, EnableTripleStack
SingleStackDurationHours, DoubleStackDurationHours?, TripleStackDurationHours?
StageEstimateSingle (optional if persisted)
```

### Diff / Badge Logic
- Diff = `ObservedSingle - StageEstimateSingle` (show sign & absolute). Threshold classification: ±10%.

### Complexity Badge
Use observed single if present else stage estimate.
Thresholds: 0=--, ?4 Simple, ?12 Medium, ?24 Complex, >24 Very Complex.

### Migration / Backfill Defaults
- `PartsPerBuildSingle = 1` if null.
- `EnableDoubleStack / EnableTripleStack = false` unless existing durations present (then set true & infer parts if possible ? fallback 1).
- Derived fields (Hours/Part) computed client-side; not stored historically.

### Edge Cases
- Enabling a mode then blanking parts/duration blocks submission until fixed or disabled.
- Switching away from SLS retains data; just skips SLS-specific validation.

### Server-Side Adjustments
- Extend `MasterPart` (and EF configuration) with new fields.
- Bind new properties in `PartsModel` PageModel.
- Conditional ModelState validation mirrored server-side (only when approach SLS/Hybrid).
- Update Create/Update handlers to persist new fields & not overwrite observed durations from stage estimates.

---
## 2. Current UI Snapshot (Excerpt Observations from _MasterPartForm.cshtml)
File currently contains three tabs:
1. Basic Info (includes a hidden `SingleStackDurationHoursHidden` used for auto-calculated duration) – This will change: observed single duration becomes user-entered in the new SLS card, not replaced by stage sum.
2. Production Stages (per-part & batch logic already separated via `isBatch` detection; computing total hours into `SingleStackDurationHoursHidden`).
3. Build Config (Stacking) – Must be removed entirely.

Other legacy elements to remove/repurpose:
- Hidden field: `SingleStackDurationHoursHidden` (auto-populated) – new design: keep separate hidden `StageEstimateSingle`; observed single remains editable.
- `partsPerBuild` input (`#partsPerBuild`), currently only surfaces when SLS approach or SLS stage selected. Replace with structured rows in new SLS card (Single/Double/Triple).
- Legacy stacking controls: `allowStackingSwitch`, `stackingControls`, `timingControls`, build capacity calculator, duration fields for double/triple.

---
## 3. Current JavaScript (Inside Form)
Key behaviors:
- Derives total hours from stages and sets hidden `MasterPart.SingleStackDurationHours` (overwriting observed concept).
- Maintains `partsPerBuild` that influences per-part stage multiplication.
- Complexity badge tied directly to aggregated total hours.

Changes Needed:
- Separate computed stage estimate from observed single stack hours.
- Introduce new state shape & functions (see target state).
- Remove stacking-specific capacity and timing logic.
- Add diff badge for stage estimate vs observed single.
- Add derived hours/part computation per enabled mode.

---
## 4. PageModel (Parts.cshtml.cs) – Relevant Points
- Currently binds legacy stacking fields on `MasterPart` (`AllowStacking`, `SingleStackDurationHours`, `DoubleStackDurationHours`, `TripleStackDurationHours`, `MaxStackCount`).
- Create/Update sets `SingleStackDurationHours` directly from form (which today is the stage sum). Need to distinguish:
  - Observed durations (user inputs) vs stage estimate (computed, optional persistence).
- Must extend binding for new fields: `PartsPerBuildSingle`, `PartsPerBuildDouble?`, `PartsPerBuildTriple?`, `EnableDoubleStack`, `EnableTripleStack`, `StageEstimateSingle`.
- Validation currently only for uniqueness + standard required; add conditional SLS validation.

---
## 5. Data Model Gaps
The provided repository snapshot included `Part` model (large legacy) but `MasterPart` definition not shown here. Need to:
- Locate `MasterPart` class file (not in extracted context) or create migration if absent.
- Add new properties enumerated above.

EF Configuration: ensure indexes / precision as needed; optional persistence of `StageEstimateSingle` (double, nullable).

---
## 6. Removal / Replacement Checklist
UI:
- Delete Build Config tab markup and related CSS.
- Delete capacity calculator & stacking forms.
- Insert SLS Build Configuration card into Basic tab region (after approach selection row).
- Inputs for: enable toggles, parts/build, observed durations, derived hours/part (readonly), diff panel.

JS:
- Remove stacking & calculator code blocks.
- Stop writing to `SingleStackDurationHoursHidden` as observed; instead populate new hidden `StageEstimateSingle`.
- Provide manual input field for observed Single duration (if design requires direct editing) OR keep old value if editing existing record.
- New validation gating on SLS card presence.

Server:
- Adjust save handlers to trust observed durations, not recompute from stage sum.
- Optionally compute & store `StageEstimateSingle` at save time (server cross-check) for analytics.

Migration:
- Script/backfill defaults (PartsPerBuildSingle=1; set enables if legacy double/triple durations exist; copy existing SingleStackDurationHours to observed field unchanged; compute StageEstimateSingle retrospectively if stage definitions present).

---
## 7. Validation Logic Matrix (Condensed)
| Condition | Visible? | Rule |
|-----------|----------|------|
| Basic fields | Always | Non-empty PartNumber, Name, Material, Approach |
| ?1 Stage | Always | At least one selected |
| Single Mode | SLS / Hybrid | PartsPerBuildSingle ?1 & SingleStackDurationHours >0 |
| Double Mode | If enabled | PartsPerBuildDouble ?1 & DoubleStackDurationHours >0 |
| Triple Mode | If enabled | PartsPerBuildTriple ?1 & TripleStackDurationHours >0 |
| Advisory | N/A | Stage estimate mismatch does not block |

---
## 8. Derived Calculations
- Stage Estimate: sum(batch hours + per-part hours) where per-part hours = `cycleMin/60 * PartsPerBuildSingle`.
- Hours/Part (observed modes): `observedDuration / observedParts` (only if both >0 else blank).
- Diff % = `(ObservedSingle - StageEstimateSingle) / StageEstimateSingle * 100` (guard zero).

---
## 9. Complexity Badge Source
`complexityHours = (ObservedSingle ?? StageEstimateSingle)` then thresholds.

---
## 10. Implementation Phasing Suggestion
1. Add model fields + migration (keep legacy fields; do not drop yet).
2. Introduce server binding & conditional validation.
3. Replace UI: remove Build Config tab; add new SLS card (feature flag optional for safer rollout).
4. Refactor JS state & functions (stage code first, then SLS card logic, then validation integration).
5. Data migration/backfill script.
6. QA against test checklist (provided below).

---
## 11. Test / Acceptance Checklist
1. Create SLS-Based part with Single only – confirm StageEstimate panel shows diff & submission payload correct.
2. Enable Double: supply parts & hours ? derived hours/part populates; submit ? fields persist.
3. Enable Triple: ensure diff unaffected by changes to non-single modes.
4. Switch approach to CNC-Based ? SLS card hidden; SLS validation relaxed; existing SLS values retained in DB.
5. Edit legacy record lacking new parts counts ? Single defaults to 1.
6. Modify stage cycles ? StageEstimateSingle updates; observed durations unchanged.
7. Diff badge color changes around ±10% thresholds.
8. Complexity badge uses observed single if present else estimate.

---
## 12. Risks / Notes
- Must retain legacy fields until all consumers updated (e.g., any reporting expecting `SingleStackDurationHours`).
- Provide backward-compatible null-handling for new nullable columns.
- Keep server-side recomputation ability (optional) to validate client-submitted estimate integrity.

---
## 13. Payload Field Mapping (Final)
| Form / Hidden | Server Property | Notes |
|---------------|-----------------|-------|
| SelectedStageIds | (parsed for definitions) | existing |
| StageEstimatedHours | (parallel arrays) | existing |
| PartsPerBuildSingle | MasterPart.PartsPerBuildSingle | new |
| PartsPerBuildDouble | MasterPart.PartsPerBuildDouble | new nullable |
| PartsPerBuildTriple | MasterPart.PartsPerBuildTriple | new nullable |
| EnableDoubleStack | MasterPart.EnableDoubleStack | new |
| EnableTripleStack | MasterPart.EnableTripleStack | new |
| SingleStackDurationHours | MasterPart.SingleStackDurationHours | observed |
| DoubleStackDurationHours | MasterPart.DoubleStackDurationHours | observed |
| TripleStackDurationHours | MasterPart.TripleStackDurationHours | observed |
| StageEstimateSingle | MasterPart.StageEstimateSingle | optional |

---
## 14. Actionable Next Session Inputs
- Locate / open `MasterPart` model file; add new properties.
- Create EF migration (naming: `Add_SLSBuildModeFields_To_MasterPart`).
- Implement server validation helper for conditional SLS rules.
- Refactor `_MasterPartForm.cshtml` per Section 6.
- Add JS module (or inline script block) implementing new state architecture; remove legacy stacking script.
- Add QA debug logging (optional) for diff and estimate.

---
Prepared as context bundle to avoid re-streaming large source files. Ready for implementation phase.
