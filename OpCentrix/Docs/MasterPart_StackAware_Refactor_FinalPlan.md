# Master Part Stack-Aware Refactor – Final Implementation Plan
Date: 2025-09-28
Status: Approved (Planning Only – No Code Changes Yet)
Owner: Manufacturing Platform / MES Core

## 1. Objectives (Definition of Done)
Provide an end?to?end, stack-aware (1x / 2x / 3x) production planning model that:
- Accurately models SLS build scalability and downstream stage loads.
- Produces deterministic, auditable, and recomputable timing & cost data.
- Enables scheduling by stack level with per-stage machine hour reservations.
- Exposes quoting metrics (cost/part, hrs/part, lead time) to Sales/Engineering.
- Supports change impact analysis (who changed what; effect on throughput & cost).
- Remains backward compatible until rollout flag is enabled.

## 2. In Scope
- Data model extensions (MasterPart, StageDefinition, StageExecution, ProductionBuild).
- Planner & cost estimation services.
- Master Part form UI refactor (stack preview + per-stage per-stack inputs).
- Scheduling preview API & integration.
- JSON matrix persistence for reproducibility.
- Observability (diff logging, events, capacity forecast skeleton).

## 3. Out of Scope (Phase 2+)
- Advanced machine optimization heuristics.
- Automated rescheduling on plan drift.
- Multi-facility capacity simulation.
- BI dashboards beyond raw endpoints.

## 4. Data Model Changes (Additive – Non Breaking First Pass)
| Entity | Field | Type | Notes |
|--------|-------|------|-------|
| MasterPart | StackPlanningJson | nvarchar(8000) | Snapshot of computed matrix. |
| MasterPart | (helper, code only) GetPartsPerBuild(level) | method | Returns parts-per-build for level. |
| StageDefinition | IsBatchStage | bit | True for SLS Printing, EDM Ops, etc. |
| StageDefinition | BatchHoursSingle/Double/Triple | float NULL | Batch cycle hours per stack mode. |
| StageDefinition | PerPartCycleMinutes | float NULL | Raw cycle time per part (before multiplication). |
| StageDefinition | OverridePerPartHours | float NULL | Direct override if needed. |
| StageExecution | PlannedHours | float NULL | Frozen planning assumption. |
| StageExecution | PlannedQuantity | int | Expected qty through stage. |
| ProductionBuild | PlannedTotalHours | float NULL | Sum of stage hours at scheduling. |
| ProductionBuild | PlannedSlsHours | float NULL | SLS portion for analytics. |
| (Optional future) | CostRate table | StageName, LaborRate, MachineRate | Centralized rates. |

Legacy fields retained temporarily (SingleStackDurationHours etc.) for migration fallback.

## 5. Migrations Strategy
1. Migration 1: Add new nullable columns.
2. Data Patch: Set IsBatchStage = 1 where StageName in ('SLS Printing','EDM Operations').
3. Backfill PerPartCycleMinutes = (EstimatedHoursPerPart * 60) for non-batch.
4. Feature flag `StackPlanningV2` remains OFF ? legacy unaffected.
5. After adoption & validation ? cleanup migration dropping deprecated legacy stacking fields (Phase 9b).

## 6. Core Services
### 6.1 IStackCapacityPlanner
Input: MasterPart + stackLevel
Output: BuildPlan DTO:
```
{
  partsPerBuild: int,
  stages: [ { id, name, order, isBatch, hours, machineType } ],
  totals: { totalHours, slsHours, otherHours, hrsPerPart }
}
```
Rules:
- Batch stage hours = specific BatchHours(mode) fallback cascade (Triple?Double?Single) else 0.
- Per-part stage hours = (OverridePerPartHours || PerPartCycleMinutes/60) * partsPerBuild.
- Sum yields build total.

### 6.2 ICostEstimator
Combines BuildPlan + rate cards ? costPerBuild, costPerPart, margin suggestions (optionally markup rules).

### 6.3 IThroughputForecaster
Aggregates scheduled builds ? machine hour demand per day / week.

### 6.4 IChangeImpactAnalyzer
Input: old vs new plan matrix ? returns list of StageDiff { stage, oldHours, newHours, deltaPct, stackModesAffected }.

## 7. JSON Schema (StackPlanningJson)
```
{
  "partsPerBuild": { "1": 12, "2": 24, "3": 36 },
  "stages": [
    { "id": 19, "name": "SLS Printing", "batch": true, "hours": { "1": 8, "2": 12, "3": 16 } },
    { "id": 2,  "name": "Heat Treatment", "batch": false, "perPartCycleMin": 12, "perPartHours": 0.20 },
    { "id": 20, "name": "CNC Machining", "batch": false, "perPartCycleMin": 30, "perPartHours": 0.50 }
  ],
  "derived": {
    "1": { "buildHours": 14.4, "hrsPerPart": 1.20, "costPerPart": 18.55 },
    "2": { ... },
    "3": { ... }
  },
  "meta": { "version": 1, "generatedUtc": "2025-09-28T17:00:00Z", "user": "admin" }
}
```

## 8. Master Part Form (UI Enhancements)
Add a Stack Modes Preview panel:
| Mode | Parts | SLS | Other Batch | Per-Part Total | Build Hours | Hrs/Part | Cost/Part |

Stage Editors:
- Batch stage row: inputs for Single/Double/Triple hours.
- Per-part stage row: cycle minutes (auto expand to show derived per-stack hours on hover or inline tooltip).
- Validation on enabling Double/Triple ensures data presence.

Hidden Fields:
- `StackPlanningJson` (string) – sent on submit.

Client Logic:
1. Collect stage definitions into an in-memory model.
2. Recompute matrix on change (debounced 200ms).
3. Display diffs vs previously saved matrix (badge if ±>10%).
4. Complexity badge uses chosen stack-level single baseline (or highest mode if available).

## 9. Scheduler Integration
Workflow:
1. User selects Master Part & desired stack level.
2. AJAX GET `/Scheduler/PlanBuild?masterPartId=&stack=` ? returns BuildPlan JSON.
3. Display condensed timeline (per stage hours bar). Machine lane coloring.
4. On confirmation: POST create ProductionBuild + StageExecutions with PlannedHours.
5. Future drift detection: compare Actual vs Planned; raise PlanDeviation event if |?| > threshold.

## 10. API Endpoints
| Endpoint | Method | Purpose |
|----------|--------|---------|
| /Api/Parts/{id}/stackPlan | GET | Returns latest StackPlanningJson or recomputed. |
| /Api/Parts/{id}/costSummary | GET | Cost & hrs per stack mode. |
| /Api/Scheduler/planBuild | GET | BuildPlan preview (masterPartId, stack). |
| /Api/Capacity/forecast | GET | Machine hours forecast. |
| /Api/Parts/{id}/diff | GET | Last N change diffs. |

## 11. Validation Rules
- Batch hours must be > 0 if mode enabled.
- Per-part cycle minutes (0.2 ? value ? 180).
- Triple requires Double (dependency rule).
- At least one required stage must be SLS if approach is SLS-Based / Hybrid.
- JSON size ? 10 KB (reject oversize to prevent bloat).

## 12. Security & Integrity
- Server recomputes BuildPlan ? disregards tampered client JSON (client JSON used as hint only; authoritative recompute persists).
- Audit entries on every save (user, timestamp, hash(old), hash(new), diff summary).
- Feature flag gating new endpoints.

## 13. Observability & Events
Emit structured events (log + optional queue):
- PartPlanGenerated
- PartPlanUpdated
- BuildScheduled
- PlanDeviationDetected
Include correlationId & partId/buildId.

## 14. Performance & Caching
- In-memory cache: (partId + stageDefs hash) ? BuildPlan (TTL 5 min).
- Hash function over ordered StageDefinition fields + stack parameters.
- Lazy invalidation on part update.

## 15. Rollout Plan
Phase | Action | Flag
------|--------|-----
1 | Deploy migrations + planner (flag OFF) | StackPlanningV2=OFF
2 | Enable form UI incrementally (beta users) | partial
3 | Turn ON scheduling preview for internal ops | ON (internal)
4 | Enable cost endpoints for Sales | ON
5 | Remove legacy stacking fields (Phase 9b) | Cleanup

Rollback: Disable flag, revert to legacy SingleStackDurationHours path; no destructive writes.

## 16. Testing Matrix
Type | Case
-----|-----
Unit | Batch fallback (missing double hours) ? uses single.
Unit | Per-part multiplication across stack modes.
Unit | Diff analyzer detects ± threshold.
Integration | Form submit with double enabled missing hours ? validation fail.
Integration | Scheduler build plan generation stress (parallel 10x).
E2E | Create part ? plan stack=2 ? schedule ? modify part ? re-plan drift detected.
E2E | Cost endpoint consumed by quoting tool (mock).

## 17. Risks & Mitigations
Risk | Mitigation
-----|-----------
Data drift between client JSON & server recompute | Server authoritative recompute on save.
User confusion with many inputs | Progressive disclosure (collapse advanced per-stack fields). Use tooltips.
Performance under high concurrency (scheduler preview) | Cache + lightweight DTO + async queries.
Complex migrations blocking deploy | Keep additive & nullable; cleanup later.

## 18. Glossary
- Stack Level: Multiplicative vertical build configuration (1x/2x/3x).
- Batch Stage: Time not multiplied by part count (e.g., SLS Printing thermal cycle).
- Per-Part Stage: Time scales linearly with number of parts per build.

## 19. Open Decisions (Default Assumptions Used)
Item | Decision
-----|---------
Persist JSON snapshot | YES
PlannedHours column | YES
Cost rate table now | DEFER (hardcode map in service short term)
Triple stack allowed by default | YES (if configured hours)

## 20. Next Immediate Steps (Coding Sequence)
1. Create migration adding new columns.
2. Implement planner & cost services + unit tests.
3. Update form Razor & JS (no legacy removal yet).
4. Add scheduling preview endpoint.
5. Introduce feature flag toggles & diff logging.
6. QA & performance smoke tests.

---
END OF PLAN – Awaiting Implementation Approval.
