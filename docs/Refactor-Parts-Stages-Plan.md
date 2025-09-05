# OpCentrix Parts & Stage Management Refactor Plan
Status: Draft  
Owner: (Assign)  
Last Updated: 2025-09-05  
Target Framework: .NET 8 (Razor Pages)

## 1. Confirmed Product Decisions (From Stakeholder Answers)
1. Deprecate legacy process boolean flags (RequiresSLSPrinting, RequiresAssembly, etc.) – keep for backward compatibility until removed, but no longer user-editable. Derive informational badges from actual stage list.
2. Enforce: A Part MUST have ≥1 stage before it can be created or updated.
3. Introduce per-stage execution logging (planned vs actual). Actual execution aggregates into Part-level historical performance and feeds estimate improvement (adaptive estimation loop).
4. Workflow Template application = MERGE (add only missing stages; do not overwrite existing stage overrides unless explicitly chosen).
5. Follow best practices: transactional integrity, separation of concerns, minimal duplication, auditable changes, idempotent APIs, and explicit versioning of stage definitions.

---

## 2. Target Architecture Overview

Component | Purpose
----------|--------
ProductionStage (library) | Canonical stage definition + defaults + version
PartStageRequirement (PSR) | Per-part override (order, duration, setup, cost, required)
PartStageExecution (NEW) | Runtime log of each execution instance (actual duration, start/end)
WorkflowTemplate + WorkflowTemplateStage | Seed/merge source
Rollup Engine | Recomputes part rollups on PSR change or new execution
Estimation Service | Adjusts future estimates using historical execution deltas
API Layer | REST-style JSON endpoints for stages, PSRs, templates, execution logs
UI Stage Manager | Single authoritative client module (add/remove/reorder/edit + metrics)
Auditing | Structured logs + optional DB audit table

---

## 3. Data Model Changes (EF Core)

### 3.1 ProductionStage (add/confirm)
Field | Type | Notes
------|------|------
Id | int | PK
Name | string | Unique
DefaultDurationHours | double | Planned base
DefaultSetupMinutes | int |
DefaultTeardownMinutes | int? |
DefaultMaterialCost | decimal(10,2) |
DefaultHourlyRate | decimal(10,2) |
Category | string? | (Machining / Additive / Finishing / QA etc.)
IsVariableDuration | bool | If false, overrides restricted
Version | int | Increment on definition change
IsActive | bool |
CreatedDate / CreatedBy | … |
LastModifiedDate / LastModifiedBy | … |

### 3.2 PartStageRequirement (modify)
Add / rename:
Field | Type | Notes
------|------|------
Id | int | PK
PartId | int | FK
ProductionStageId | int | FK
ExecutionOrder | int | Unique per part
PlannedHours | double | (Rename from EstimatedHours)
SetupTimeMinutes | int |
TeardownTimeMinutes | int? |
HourlyRateOverride | decimal(10,2)? |
MaterialCostOverride | decimal(10,2)? |
IsRequired | bool |
IsActive | bool |
StageVersionSnapshot | int | Copied from ProductionStage.Version at link time
ChangeSource | string | ManualEdit | TemplateMerge | Propagation
CreatedDate / CreatedBy | … |
LastModifiedDate / LastModifiedBy | … |

### 3.3 New: PartStageExecution
Field | Type | Notes
------|------|------
Id | long | PK
PartId | int | For rollup performance
PartStageRequirementId | int | FK
ProductionStageId | int | Redundant FK for indexing
ExecutionBatchId | string? | Group multiple parallel ops
StartedAt | DateTimeOffset |
EndedAt | DateTimeOffset |
ActualHours | double | Derived (EndedAt - StartedAt)
ActualSetupMinutes | int? | If tracked separately
ActualTeardownMinutes | int? |
ActualMaterialCost | decimal(10,2)? |
OperatorUserId | string? |
WasInterrupted | bool |
Notes | string? |
CreatedDate | DateTimeOffset | Insert timestamp

### 3.4 Part (add / deprecate)
Add (persisted rollups):
Field | Type | Notes
------|------|------
TotalPlannedStageHours | double |
TotalActualStageHours | double |
TotalPlannedLaborCost | decimal(12,2) |
TotalActualLaborCost | decimal(12,2) |
TotalPlannedMaterialCost | decimal(12,2) |
TotalActualMaterialCost | decimal(12,2) |
StageSetVersionHash | string? | For drift detection
HasStageDrift | bool | Derived flag (if any PSR StageVersionSnapshot != current ProductionStage.Version)

Deprecate (mark [Obsolete], remove from UI):
- RequiresSLSPrinting, RequiresCNCMachining, RequiresEDMOperations, RequiresAssembly, RequiresFinishing, ManufacturingStage, StageDetails, StageOrder, EstimatedHours (convert to derived or keep as legacy reference only).

### 3.5 WorkflowTemplateStage
Field | Notes
------|------
WorkflowTemplateId | FK
ProductionStageId | FK
DefaultExecutionOrder | Used when seeding
DurationOverrideHours | Optional
SetupOverrideMinutes | Optional
TeardownOverrideMinutes | Optional
IsRequired | Bool

---

## 4. Adaptive Estimation Logic

Step | Logic
-----|------
1. On new execution completion -> Insert PartStageExecution.
2. Recalculate rolling averages per (PartId, ProductionStageId):
   - RollingMean = (PrevMean * n + NewActual) / (n + 1)
   - Track variance for anomaly filtering (3σ exclusion).
3. Update PartStageRequirement.PlannedHours IF:
   - IsVariableDuration = true
   - Deviation threshold exceeded (e.g., |Actual - Planned| > 15%) for M consecutive runs (configurable)
   - Log adjustment event (ChangeSource = AutoTune).
4. Recompute part rollups.
5. Emit structured log:
   - EventType: StageExecutionCompleted
   - PartId, StageId, PlannedHours Prev, ActualHours, AdjustmentApplied? bool, NewPlannedHours.

---

## 5. Template Merge Semantics

Rule | Description
-----|------------
Stage present in template but missing in part | Add as new PSR (ChangeSource=TemplateMerge)
Stage exists in both | Keep existing PSR (no overwrite) unless optional “Update From Template” explicitly chosen
ExecutionOrder collisions | Re-normalize: maintain existing order; append new at end; then resequence 1..N
Duration override in template & part has untouched (never manually edited) PSR | Optionally adopt (flag-driven)
Audit | Log each merge decision (Added / Skipped / Updated)

---

## 6. Validation Rules

Rule | Failure Response
-----|-----------------
At least 1 stage per part | Block save (ModelState + 400 for API)
ExecutionOrder unique & contiguous | Auto-normalize; if unsalvageable → 422
PlannedHours > 0 | 422
Setup / Teardown ≥ 0 | 422
HourlyRate (effective) > 0 | 422
No duplicate ProductionStageId in PSRs | 422
Template merge does not reduce stage count below 1 | Guard
IsVariableDuration false and override attempt occurs | 403 (unless admin)

---

## 7. API Endpoints (Versionable)

Endpoint | Verb | Purpose
---------|------|--------
/api/stages | GET | List stages (with version)
/api/stages/{id} | GET | Stage detail
/api/parts/{partId}/stages | GET | List PSRs + computed effective rates
/api/parts/{partId}/stages | POST | Bulk upsert (replace full set)
/api/parts/{partId}/stages/merge-template/{templateId} | POST | Merge template
/api/parts/{partId}/stages/reorder | POST | Body: [stageIds in order]
/api/parts/{partId}/stages/{psrId} | PATCH | Partial update (hours, setup, cost…)
/api/parts/{partId}/stages/{psrId} | DELETE | Soft remove
/api/parts/{partId}/executions | POST | Log runtime execution (StageRequirementId, start/end, metrics)
/api/parts/{partId}/executions/recompute | POST | Rebuild rollups (admin)
/api/stages/propagate-defaults/{stageId} | POST | Controlled propagation (phase 2 or later)

Response Envelope:
{
  success: true|false,
  message: "…",
  data: { … },
  warnings: [ … ],
  correlationId: "…"
}

---

## 8. Logging & Auditing Specification

Event | Fields
------|-------
StageExecutionCompleted | PartId, PartStageRequirementId, ProductionStageId, PlannedHours, ActualHours, AdjustmentApplied, NewPlannedHours, OperatorUserId, DurationDeltaPct
StageEstimateAdjusted | PartId, PSR Id, OldPlannedHours, NewPlannedHours, Trigger=Deviation|Manual|Propagation
TemplateMerged | PartId, TemplateId, AddedCount, SkippedCount, UpdatedCount
StagePropagationRun | StageId, AffectedParts, Mode=Selective|All, FieldsPropagated[], Duration
DriftDetected (batch) | PartId, StageCountDrift, AffectedStages[]
ValidationFailure | Context=PartStageSync, Errors[], PayloadHash

Implementation:
- Use structured logging (Serilog) with consistent property casing.
- Consider secondary sink (SQLite/Elastic) if analytics needed later.

---

## 9. UI (Razor + JS) Refactor Plan

Area | Action
-----|-------
_Form Modal | Remove legacy checkboxes; add Stage tab with dynamic manager
Stage Manager JS | Single module; no duplicate initialization; uses JSON not CSV
Hidden Fields | Replace multiple CSV inputs with one hidden JSON: StagePayload
Validation Feedback | Inline badge if 0 stages selected
Rollup Panel | Real-time: Total Planned vs (if executions exist) Average Actual
Template Merge UI | Button → modal with preview diff (Added / Unchanged / Potential Update)
Drift Indicator | If HasStageDrift → show icon + tooltip "Stage definitions changed"
Execution Logging (Future) | Separate operational screen (not in create/edit modal) to record actual completions; or integrate into job tracking module

---

## 10. Migration Steps

Step | Description
-----|------------
1 | Add new columns (Part, PartStageRequirement, ProductionStage) via migration
2 | Create PartStageExecution table
3 | Backfill StageVersionSnapshot = ProductionStage.Version (set Version=1)
4 | Copy existing single-stage legacy duration (EstimatedHours) proportionally into a default single PSR if a part has no PSRs
5 | Mark legacy flags as deprecated: update UI to read-only badges or hide
6 | Initialize rollups for all parts (compute & persist)
7 | Deploy feature flag: Feature:AdvancedStageWorkflow=true
8 | Enable API & new Stage Manager UI when flag active
9 | Monitor logs for ValidationFailure & DriftDetected
10 | Remove legacy CSV hidden fields & code paths after stabilization

Rollback Strategy:
- Keep old columns untouched until Phase 9
- Feature toggle to revert UI to legacy form (if critical issue)
- Data migration reversible (store mapping script & backups)

---

## 11. Rollup Computation Algorithm (Deterministic)

For each PSR:
EffectiveHourlyRate = HourlyRateOverride ?? ProductionStage.DefaultHourlyRate  
StageLaborPlanned = PlannedHours * EffectiveHourlyRate  
StageSetupPlanned = (SetupTimeMinutes + (TeardownTimeMinutes ?? 0)) / 60 * EffectiveHourlyRate  
StageMaterialPlanned = MaterialCostOverride ?? ProductionStage.DefaultMaterialCost  
StageTotalPlanned = Sum above  

For executions (latest N or all):
ActualLabor = ActualHours * EffectiveHourlyRate(at time of execution or snapshotted)  
If actual setup captured: use it; else estimate from planned ratio.  

Part Totals = Σ(Planned) / Σ(Actual) separated.

---

## 12. Adaptive Estimation Policy (Initial Heuristic)

Parameter | Default
----------|--------
MinExecutionsBeforeAdjust | 3
DeviationThresholdPercent | 15
ConsistencyWindow | 2 consecutive deviations
MaxIncreasePerAdjustment | 30%
MaxDecreasePerAdjustment | 20%
FloorHours | 0.1

Adjustment Pseudocode:
if executions >= MinExecutionsBeforeAdjust &&
  abs(ActualMean - Planned) / Planned *100 > DeviationThresholdPercent &&
  last ConsistencyWindow executions all deviate same direction:
      NewPlanned = clamp(Planned * (ActualMean / Planned), bounds)
      Log StageEstimateAdjusted

---

## 13. Security & Authorization

Action | Policy
-------|-------
Edit Stages (library) | AdminOnly
Modify PSR | Admin or Manager
Template Merge | Admin or Manager
Auto-Tuning | System identity (claim: SystemAutomation)
Propagation | AdminOnly
Execution Logging | Authenticated Operator role

---

## 14. Phased Delivery Timeline

Phase | Focus | Success Criteria
------|-------|-----------------
1 | Data migrations + models | Build passes, migrations applied
2 | Read-only APIs (stages + PSRs) | GET endpoints functional
3 | New Stage Manager (client) with JSON payload & create/update integration | Part creation with ≥1 stage works end-to-end
4 | Rollup calculations & part list updates | UI reflects planned totals
5 | Template merge feature | Merge preview + audit log
6 | Execution logging + rollup refresh | Actual vs planned visible
7 | Adaptive estimation engine | Adjustments logged & bounded
8 | Drift detection & badge | Changes to stage defaults surfaced
9 | Deprecation cleanup | Legacy flags removed from UI, no CSV remnants
10 | Performance tuning & documentation | Response times acceptable (<200ms typical APIs)

---

## 15. Acceptance Criteria (Key)

ID | Criterion
---|----------
AC1 | Cannot save a part without at least one stage (UI + API).
AC2 | Creating a part with 3 stages persists all PSRs & rollups visible on refresh.
AC3 | Merging a template adds only missing stages; existing overrides preserved.
AC4 | Updating a stage’s planned hours triggers recalculated part totals.
AC5 | Logging an execution updates TotalActualStageHours within one request.
AC6 | Adaptive estimation adjusts planned hours only after configured deviation conditions.
AC7 | Drift icon appears when a stage definition version changes vs snapshot.
AC8 | Legacy process flags no longer editable; badges reflect derived presence of categories.
AC9 | Structured logs contain correlationId and StageExecutionCompleted events with required fields.
AC10 | API bulk upsert rejects duplicate ProductionStageIds gracefully (422).

---

## 16. Risk & Mitigation

Risk | Mitigation
-----|-----------
Silent data corruption in CSV → JSON switch | Early feature flag; extensive validation
Over-adjusting estimates | Max delta caps + min executions threshold
Template merge confusion | Diff preview + explicit summary
Performance regressions on large parts | Precomputed rollups + batched queries
Rollback difficulty | Feature flag + untouched legacy columns until Phase 9

---

## 17. Implementation Order (Granular Task List)

Order | Task
------|-----
1 | Create EF migration for new columns & PartStageExecution table
2 | Add domain services: StageCatalogService, PartStageService, RollupService
3 | Implement GET APIs (/api/stages, /api/parts/{id}/stages)
4 | Replace form hidden CSV with one JSON hidden field StagePayload
5 | Rebuild Stage Manager JS: load, add, edit, reorder, serialize
6 | Server: OnPostCreate/Update parse JSON StagePayload → service call
7 | RollupService integrated in transaction (create/update)
8 | Template merge endpoint + UI modal
9 | Execution logging endpoint + simple logging UI (or placeholder)
10 | AdaptiveEstimationService (background or inline on execution log)
11 | Drift detection job (compare snapshots nightly or on stage edit)
12 | Deprecate legacy fields in UI; add derived badges
13 | Hardening: add validation + structured log events
14 | Performance pass: indexing (PartId, ProductionStageId) on PSR & execution tables
15 | Documentation & developer handoff

---

## 18. Logging Field Dictionary (Consistency Reference)

Field | Type | Example
------|------|--------
event | string | StageExecutionCompleted
partId | int | 42
productionStageId | int | 7
partStageRequirementId | int | 135
plannedHours | double | 6
actualHours | double | 7.4
deviationPercent | double | 23.3
adjustmentApplied | bool | true
newPlannedHours | double | 6.9
changeSource | string | AutoTune
correlationId | string | 3f29c6d4d1
operatorUserId | string | jsmith
templateId | int | 5
addedStages | int | 2
skippedStages | int | 4

---

## 19. Open Questions (To Resolve Before Phase 4)
Question | Decision Needed
---------|-----------------
Execution logging UI scope now or attach to job scheduling later? | ?
Should ActualMaterialCost be captured or derived from inventory module? | ?
Propagation tool priority (Phase 7 vs later)? | ?
Background job framework (HostedService vs Cron) for nightly drift scan? | ?

(Track in backlog.)

---

## 20. Definition of Done (Refactor Epic)
- All phases 1–8 complete.
- Legacy flags removed from UI interaction.
- Performance: Listing 100 parts with rollups ≤ 250ms server time.
- Automated tests: Services (rollup, merge, estimation) ≥ 80% coverage.
- Manual test scripts executed & signed off.
- Documentation: This plan + API spec + migration notes committed.

---

## 21. Quick Win Sequence (If Time-Constrained)
1. Migrate & create APIs (read-only).
2. New Stage Manager with JSON payload → enforce ≥1 stage.
3. Remove legacy checkbox logic.
4. Rollups + part list updates.
5. Execution logging stub (records only hours).
6. Later: Adaptive estimation + templates + drift.

---

## 22. Notes
- Keep feature flag until after Phase 5 stabilization.
- Avoid premature complexity: propagate defaults later.
- Estimation engine should be stateless (pure functions) for testability.

---

End of Plan