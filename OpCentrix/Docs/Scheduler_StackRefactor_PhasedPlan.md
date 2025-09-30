# Scheduler Stack-Aware Refactor & Operator Start Integration

Status: Draft
Last Updated: UTC {{DATE}}
Owner: Scheduler/Manufacturing Refactor

## Objective
Incrementally evolve the existing SLS-focused scheduler (Job + Part) to a stack-aware, operator-driven workflow that supports: 
- MasterPart & stacking metadata
- Operator start modal (simplified)
- Time-based progress & duration learning (excluding prototype runs)
- Inventory powder deductions (simple)
- Future downstream (EDM/CNC) linkage groundwork

## Guiding Principles
1. Ship in small, reversible slices (each phase builds safely on prior state).
2. Keep legacy behavior working until replacement slice passes smoke tests.
3. Defer real-time (SignalR) until fundamentals (data + transitions + UI) are stable.
4. Exclude prototype-inclusive runs from stack duration learning.
5. Never retro-change historical jobs when MasterPart definitions evolve—snapshot at start.

---
## Phase Overview (High-Level)
| Phase | Scope | Risk | Deployable | Rollback |
|-------|-------|------|------------|----------|
| 1 | Data columns (Job) | Low | Yes | Migration revert |
| 2 | Status & learning tables | Low | Yes | Migration revert |
| 3 | Runtime service (start/complete) | Med | Yes | Disable endpoints |
| 4 | Start modal refactor (UI) | Med | Yes | Feature flag back to old start |
| 5 | Scheduler badges (stack + units) | Low | Yes | Remove UI partial |
| 6 | Progress endpoint + polling | Low | Yes | Disable endpoint |
| 7 | Inventory (powder deduction) | Med | Yes | Skip service invocation |
| 8 | Duration override + optional ripple | Med | Yes | Turn off ripple flag |
| 9 | Completion learning (EMA) | Med | Yes | Toggle learning flag |
| 10 | UI enhancements (progress bars, efficiency) | Low | Yes | Remove markup |
| 11 | Downstream job linking groundwork | Low | Yes | Hide chain icon |
| 12 | Legacy cleanup (deprecated start) | Low | Yes | Restore old handler |
| 13 | Admin learning audit tooling | Low | Yes | Hide menu |
| 14 | Tests & hardening | Low | Yes | N/A |
| 15 | (Deferred) SignalR real-time | Med | Yes | Not started |

---
## Detailed Phases

### Phase 1: Minimal Data Extensions (Migration 1)
Add nullable Job fields:
- MasterPartId (int?)
- StackLevel (byte?)
- PartsPerBuild (int?)
- PlannedStackDurationHours (double?)
- PlannedEndUtc (DateTime?)
- ActualUnitsPlanned (int?)
- PrototypeUnitsPlanned (int?)
- PowderAddedKg (decimal(8,2)?)
- PowderMaterial (string 100)
- PredecessorJobId (int? self-FK)
- OperatorUserId (int?)
- LastStatusChangeUtc (DateTime?)

Backfill script:
- StackLevel = 1 where null
- PlannedStackDurationHours = EstimatedHours for all Scheduled jobs
- PlannedEndUtc = ScheduledEnd for Building jobs

Indexes:
- (MachineId, Status)
- MasterPartId
- PredecessorJobId

Deliverables: Migration + context config + smoke test load scheduler.

### Phase 2: Status & Learning Tables (Migration 2)
New tables:
- JobStatusHistory (Id, JobId, OldStatus, NewStatus, ChangedByUserId, ChangedUtc, Notes)
- StackDurationLearning (Id, JobId, MasterPartId, StackLevel, ObservedHours, PrototypeUnits, IncludedInAverage, Reason, CompletedUtc)

No logic yet. Only schema + minimal indexes (JobId, MasterPartId, StackLevel, IncludedInAverage).

### Phase 3: Runtime Transition Service
Create ISchedulerRuntimeService:
- StartJobAsync(jobId, StartPayload)
- CompleteJobAsync(jobId, CompletePayload)

Start rules:
- Status must be Scheduled
- Set ActualStart, LastStatusChangeUtc
- If override duration provided: PlannedEndUtc = ActualStart + overrideHours else use existing ScheduledEnd
- Adjust ScheduledEnd to PlannedEndUtc
- Snapshot: PlannedStackDurationHours, StackLevel, PartsPerBuild
- Insert JobStatusHistory

Complete rules:
- Status must be Building
- Set ActualEnd + LastStatusChangeUtc
- Insert history

### Phase 4: Start Modal Refactor
Replace legacy print start path.
Modal fields:
- StackLevel (if absent default 1)
- ActualUnitsPlanned
- PrototypeUnitsPlanned
- PowderAddedKg
- OverrideDurationHours (optional)
- Notes

Validation: >0 units, PowderAddedKg >=0, override duration in range (0.25–500h).
Feature flag: Scheduler.StartModalV2 (on ? use new modal).

### Phase 5: Scheduler UI Badges
Add to job block:
- Stack badge (1x/2x/3x)
- Units: N (+P prot)
- If Building: minimal “Started” tag.

### Phase 6: Time-Based Progress Endpoint
GET /api/scheduler/jobs/{id}/progress ? {percent, remainingMinutes}
Formula: (UtcNow - ActualStart)/(PlannedEndUtc - ActualStart)
Client polling every 60s while Building.

### Phase 7: Inventory (Powder Deduction)
Precondition: Confirm Material table has QuantityKg (add column if absent).
On StartJobAsync:
- If PowderAddedKg > 0: decrement material.QuantityKg (by SlsMaterial match) inside transaction.
- If insufficient ? fail start.
- Log JobLogEntry.

### Phase 8: Duration Override & Optional Ripple
If override entered > existing, update ScheduledEnd & PlannedEndUtc.
Optional ripple (feature flag Scheduler.AutoRipple): shift future jobs on same machine forward preserving durations; record each shift in JobLogEntry.
Default: disabled.

### Phase 9: Completion Learning (EMA)
On CompleteJobAsync:
Eligibility: MasterPartId != null, (PrototypeUnitsPlanned ?? 0) == 0, StackLevel present.
- ObservedHours = (ActualEnd - ActualStart)
- Outlier if Observed/CurrentAvg outside [0.1, 3.0] (config via SystemSettings: StackLearning.MinRatio, MaxRatio)
- EMA: New = Alpha * Observed + (1-Alpha) * Old (Alpha default 0.25, SystemSettings key StackLearning.Alpha)
- Update MasterPart.Single/Double/TripleStackDurationHours.
- Insert StackDurationLearning row (reason if excluded).

### Phase 10: UI Enhancement Round
- Progress bar (color transitions 0–100%)
- ETA (PlannedEndUtc local)
- Efficiency badge after completion (Estimated vs Actual)
- Prototype badge (excluded from learning tooltip)

### Phase 11: Downstream Linking Groundwork
- Use PredecessorJobId when manually creating EDM/CNC jobs.
- Chain icon on scheduler card.
- No auto-spawn yet.

### Phase 12: Legacy Cleanup
- Mark old start-print endpoints deprecated (log warning).
- Remove old buttons if feature flag enabled.

### Phase 13: Admin Learning Audit
Admin page section:
- List StackDurationLearning rows with filters.
- Action: Re-include excluded row ? recompute by replay (simple recompute mean fallback or re-run EMA chronologically).

### Phase 14: Tests & Hardening
Unit Tests:
- Start -> Building transition guard
- Duplicate start rejection
- Progress clamping
- EMA calculation (seed null & subsequent)
- Inventory insufficient scenario
- Prototype exclusion
Integration Tests:
- Start & complete reflect in scheduler JSON feed.
- Migration smoke tests.

### Phase 15 (Deferred): SignalR Real-Time
After stability: broadcast jobStarted, jobProgress (optional later), jobCompleted.

---
## DTOs (Draft Shapes)
StartJobRequest:
- StackLevel? byte
- ActualUnitsPlanned int
- PrototypeUnitsPlanned int?
- PowderAddedKg decimal?
- OverrideDurationHours double?
- Notes string?

CompleteJobRequest:
- ProducedQuantity int?
- DefectQuantity int?
- Notes string?

ProgressResponse:
- JobId
- Percent
- RemainingMinutes
- PlannedEndUtc

---
## Feature Flags / Settings
| Key | Type | Default | Purpose |
|-----|------|---------|---------|
| Scheduler.StartModalV2 | bool | false | Gate new start modal |
| Scheduler.AutoRipple | bool | false | Enable downstream shift |
| StackLearning.Alpha | double | 0.25 | EMA weight |
| StackLearning.MinRatio | double | 0.1 | Lower outlier bound |
| StackLearning.MaxRatio | double | 3.0 | Upper outlier bound |

---
## Rollback Notes
- Each migration is isolated; revert last migration if failure introduced there.
- Service layer can be disabled by removing DI registration of ISchedulerRuntimeService.
- UI feature flags allow quick hide of new elements.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Inventory mismatch | Transaction & validation before commit |
| EMA drift due to anomalies | Outlier bounds + manual admin re-include tool |
| Operator confusion on override | Clear tooltip + persist snapshot fields |
| Time zone confusion | Store UTC, render local, suffix UI labels |

## Acceptance Criteria (Per Phase)
- Phase 1: Scheduler loads unchanged; new columns present.
- Phase 3: API start & complete transitions update status & history.
- Phase 6: Progress endpoint returns correct % at 0%, mid, >100% clamp.
- Phase 9: Completing eligible job updates MasterPart duration field.
- Phase 10: Visual progress + efficiency visible with no JS errors.

## Deferred / Not In Scope Now
- Auto downstream job creation
- SignalR live progress
- Machine OPC UA telemetry
- Advanced multi-stage orchestration

---
## Execution Order Checklist
[ ] Phase 1 migration created & applied
[ ] Phase 2 migration created & applied
[ ] Runtime service + DI registration
[ ] Start modal UI behind flag
[ ] Progress endpoint + polling
[ ] Inventory deduction & tests
[ ] EMA learning logic + tests
[ ] UI badges & progress bar
[ ] Admin audit view
[ ] Documentation update & handoff

---
## Notes
- Keep migrations small to simplify code review.
- Prefer additive schema vs destructive changes.
- Snapshot durations to avoid retro changes when MasterPart values shift.
- Prototypes excluded entirely (no proportional adjustment attempt).

---
End of Plan
