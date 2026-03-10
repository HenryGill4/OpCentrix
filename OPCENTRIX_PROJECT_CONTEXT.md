# OpCentrix Project Context & Implementation Plan

> **Last Updated**: 2026-03-10
> **Branch**: `claude/machine-provider-and-stage-learning-wbQDv` (from `origin/New-Master-Parts`)
> **Status**: Deep Analysis Complete - Ready for Phased Implementation

---

## 1. Project Overview

### What is OpCentrix?
OpCentrix is a **Manufacturing Execution System (MES)** for additive manufacturing operations, primarily focused on SLS metal 3D printing with multi-stage manufacturing workflows.

### Target Hardware
- **Primary**: EOS M4 ONIX SLS metal 3D printers
- **Secondary**: Sieve stations, CNC machines, EDMs, other SLS machines
- **Future**: Universal machine support (any manufacturer)

### Core Goals
1. **Full Process Visibility**: Real-time monitoring of all machines and operations
2. **Intelligent Scheduling**: Flexible, user-configurable scheduling with stacking support
3. **Multi-Stage Workflows**: Track parts through SLS ? CNC ? EDM ? Assembly ? Finishing
4. **Learning System**: Auto-refine duration estimates from actual operation data
5. **Universal Machine Integration**: Provider-based architecture for any machine type

---

## 2. HONEST Current State Analysis

### What ACTUALLY Exists (Verified)

#### Models - ACTUAL STATUS
| Model | Purpose | Real Status | Issues |
|-------|---------|-------------|--------|
| `Machine` | Generic machine | ? Complete | Has `OpcUaEndpointUrl` but no real integration |
| `Job` | Scheduled job | ? Complete | Has `BuildCohortId`, `WorkflowStage`, `StageOrder` |
| `Part` | Part definition | ? Complete | Has stage requirement flags (RequiresSLSPrinting, etc.) |
| `MasterPart` | Stacking configs | ? Complete | 1x/2x/3x durations, parts per build |
| `BuildJob` | SLS print tracking | ? Complete | Links to parts, tracks actual hours |
| `BuildCohort` | Batch tracking | ? Complete | Groups parts from single SLS build |
| `JobStage` | Stage instance | ? Complete | Has ActualStart/ActualEnd, ProgressPercent |
| `ProductionStage` | Stage definition | ? Complete | Custom fields, machine assignment |
| `PartStageRequirement` | Part?Stage mapping | ?? INCOMPLETE | **Missing 5 learning fields** |
| `JobStageHistory` | Audit trail | ? Complete | Has `StageHours`, `ProductionStageId` |
| `JobStageDependency` | Stage dependencies | ? Complete | FinishToStart, lag time |

#### Services - ACTUAL STATUS
| Service | Purpose | Real Status | Issues |
|---------|---------|-------------|--------|
| `IOpcUaService` | Machine comms | ?? **STUB ONLY** | Returns hardcoded mock data |
| `SchedulerService` | Job scheduling | ?? Partial | No stacking intelligence |
| `PrintTrackingService` | Build tracking | ? Complete | Creates cohorts, tracks builds |
| `MultiStageJobService` | Stage workflow | ?? Partial | **No learning hook in CompleteStageAsync** |
| `MachineManagementService` | Machine CRUD | ? Complete | Status updates work |
| `StageProgressionService` | Downstream jobs | ? Complete | Creates EDM/CNC/etc jobs from cohort |
| `CohortManagementService` | Cohort tracking | ? Complete | Full workflow support |

#### Pages - ACTUAL STATUS
| Page | Purpose | Real Status | Issues |
|------|---------|-------------|--------|
| `/Scheduler` | Main grid | ?? Partial | No multi-day stage view, no stacking UI |
| `/PrintTracking` | SLS dashboard | ? Complete | Real-time status, job tracking |
| `/Operations/StageDashboard` | Stage view | ?? Partial | Shows stages but limited interaction |
| `/Admin/Parts` | Part CRUD | ? Complete | No stage performance analytics |

### Critical Gaps Identified

#### GAP 1: Machine Integration (No Real Machine Data)
```
Current: OpcUaService returns hardcoded values like:
  - Status = "Running" (always)
  - BuildProgress = 75.5 (always)
  - LaserPower = 285.0 (always)

Need: Real EOS M4 ONIX integration via REST + OPC UA
```

#### GAP 2: Learning Pipeline (Estimates Never Update)
```
Current: PartStageRequirement.EstimatedHours is set once and never updated
         MultiStageJobService.CompleteStageAsync() does NOT call any learning service

Need: 
  1. Add 5 fields to PartStageRequirement for learning
  2. Create IPartStageLearningService
  3. Hook CompleteStageAsync ? RecordCompletionAsync
```

#### GAP 3: Scheduler Stacking (Manual Only)
```
Current: MasterPart has stacking configs but scheduler doesn't use them
         No weekend fill recommendations
         No automatic stack level selection

Need:
  1. ScheduleSuggestionService to recommend stack levels
  2. WeekendFillService to identify gaps
  3. UI to display recommendations
```

#### GAP 4: Real-Time Updates (Polling Only)
```
Current: Dashboard polls via HTMX every 60 seconds
         No push updates when machine state changes

Need:
  1. SignalR MachineStateHub
  2. MachineSyncService as IHostedService
  3. Real-time UI updates
```

#### GAP 5: Multi-Stage Scheduler View
```
Current: Scheduler shows single-machine timeline
         Stages exist in JobStage but not visualized across machines

Need:
  1. Stage-based scheduler view option
  2. Cross-machine workflow visualization
  3. Dependency-aware scheduling
```

---

## 3. Architecture Design

### 3.1 Machine Provider Architecture

```
???????????????????????????????????????????????????????????????
?                    IMachineProvider                          ?
?  - GetStatusAsync(machineId)                                 ?
?  - GetMachineDataAsync(machineId)                           ?
?  - SendCommandAsync(machineId, command)                     ?
?  - GetAlarmsAsync(machineId)                                ?
?  - SubscribeToUpdatesAsync(machineId, callback)             ?
???????????????????????????????????????????????????????????????
              ?                    ?                    ?
              ?                    ?                    ?
    ????????????????????  ??????????????????   ???????????????
    ?MockMachineProvider?  ?EosMachineProvider?   ?GenericOpcUaProvider?
    ?  (dev/test)      ?  ?  (EOS M4 ONIX)  ?   ?  (other machines)?
    ????????????????????  ???????????????????   ????????????????????
                                ?
                    ?????????????????????????
                    ?                       ?
           ??????????????????    ????????????????????
           ? EosRestClient  ?    ? EosOpcUaClient   ?
           ? (OAuth2 + API) ?    ? (OPC Foundation) ?
           ??????????????????    ????????????????????
```

### 3.2 Data Models for Machine Provider

```csharp
// MachineConnectionSettings - FK to Machine.Id (int)
public class MachineConnectionSettings
{
    public int Id { get; set; }
    public int MachineId { get; set; }  // FK to Machine.Id
    public string ProviderType { get; set; } = "Mock";  // "Mock", "Eos", "GenericOpcUa"
    public string ConnectionConfigJson { get; set; } = "{}";  // Provider-specific config
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastConnectedAt { get; set; }
    public string? LastError { get; set; }
    public virtual Machine Machine { get; set; } = null!;
}

// MachineStateRecord - Historical state snapshots
public class MachineStateRecord
{
    public int Id { get; set; }
    public int MachineId { get; set; }  // FK to Machine.Id
    public string Status { get; set; } = "Unknown";
    public double? BuildProgressPercent { get; set; }
    public string? CurrentJobReference { get; set; }
    public string? StateDataJson { get; set; }  // Full telemetry snapshot
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public virtual Machine Machine { get; set; } = null!;
}
```

### 3.3 Learning Pipeline Architecture

```
????????????????????????????????????????????????????????????????
?                Stage Completion Event                         ?
?  MultiStageJobService.CompleteStageAsync(stageId, operator)  ?
????????????????????????????????????????????????????????????????
                              ?
                              ?
????????????????????????????????????????????????????????????????
?            IPartStageLearningService                          ?
?  RecordCompletionAsync(jobStageId)                           ?
?    1. Load JobStage with Job navigation                      ?
?    2. Calculate actual duration (ActualEnd - ActualStart)    ?
?    3. Write to JobStageHistory.StageHours                    ?
?    4. Find PartStageRequirement (Job.PartId + StageType)     ?
?    5. Call RefineEstimateAsync()                             ?
????????????????????????????????????????????????????????????????
                              ?
                              ?
????????????????????????????????????????????????????????????????
?  RefineEstimateAsync(partId, stageType)                      ?
?    1. Query JobStageHistory for this Part × StageType        ?
?    2. Compute EMA: newAvg = ? * newActual + (1-?) * oldAvg   ?
?       where ? = 0.3 (30% weight to new observation)          ?
?    3. Update PartStageRequirement:                           ?
?       - EstimatedHours = newAvg (if Auto mode)               ?
?       - ActualAverageDurationHours = newAvg                  ?
?       - ActualSampleCount++                                  ?
?       - LastActualDurationHours = thisRun                    ?
?       - EstimateSource = "Auto"                              ?
?       - EstimateLastUpdated = DateTime.UtcNow                ?
????????????????????????????????????????????????????????????????
```

### 3.4 New Fields for PartStageRequirement (5 fields)

```csharp
// Add to existing PartStageRequirement model
public double? ActualAverageDurationHours { get; set; }   // Rolling weighted average
public int ActualSampleCount { get; set; } = 0;           // Number of completions observed
public double? LastActualDurationHours { get; set; }      // Most recent actual duration
public string EstimateSource { get; set; } = "Manual";    // "Manual" | "Auto" | "Default"
public DateTime? EstimateLastUpdated { get; set; }        // Last auto-refinement timestamp
```

---

## 4. Implementation Phases (Detailed Instructions)

### PHASE 1: Machine Provider Foundation
**Goal**: Create pluggable machine provider architecture
**Duration**: 1 session
**Prerequisites**: None

#### Files to Create
```
OpCentrix/
??? Models/MachineProviders/
?   ??? MachineConnectionSettings.cs    # DB entity for provider config
?   ??? MachineStateRecord.cs           # DB entity for state history
?   ??? MachineProviderModels.cs        # DTOs: MachineStatus, MachineData, MachineCommand
??? Services/MachineProviders/
?   ??? IMachineProvider.cs             # Core interface
?   ??? MockMachineProvider.cs          # Simulated data for dev/test
?   ??? MachineProviderFactory.cs       # Creates provider by type
??? Services/
    ??? MachineSyncService.cs           # IHostedService for polling
```

#### Files to Modify
```
OpCentrix/
??? Data/SchedulerContext.cs            # Add DbSets for new entities
??? Program.cs                          # Register new services
??? Migrations/                         # Add migration
```

#### Implementation Steps (for Copilot)
1. **Create `Models/MachineProviders/MachineProviderModels.cs`**
   - Create `MachineStatus` record (Status, BuildProgress, Alarms, Timestamp)
   - Create `MachineData` record (Telemetry dictionary, raw JSON)
   - Create `MachineCommand` record (CommandType, Parameters)

2. **Create `Models/MachineProviders/MachineConnectionSettings.cs`**
   - Entity with FK to `Machine.Id` (int, not string)
   - ProviderType: "Mock", "Eos", "GenericOpcUa"
   - ConnectionConfigJson for provider-specific settings
   - Add navigation property to Machine

3. **Create `Models/MachineProviders/MachineStateRecord.cs`**
   - Historical state snapshots
   - FK to Machine.Id
   - StateDataJson for full telemetry

4. **Create `Services/MachineProviders/IMachineProvider.cs`**
   ```csharp
   public interface IMachineProvider
   {
       string ProviderType { get; }
       Task<MachineStatus> GetStatusAsync(int machineId, CancellationToken ct = default);
       Task<MachineData> GetMachineDataAsync(int machineId, CancellationToken ct = default);
       Task<bool> SendCommandAsync(int machineId, MachineCommand command, CancellationToken ct = default);
       Task<List<string>> GetAlarmsAsync(int machineId, CancellationToken ct = default);
       Task<bool> TestConnectionAsync(int machineId, CancellationToken ct = default);
   }
   ```

5. **Create `Services/MachineProviders/MockMachineProvider.cs`**
   - Return simulated data that varies over time
   - Simulate build progress incrementing
   - Simulate random status changes
   - Useful for UI development without real machines

6. **Create `Services/MachineProviders/MachineProviderFactory.cs`**
   ```csharp
   public interface IMachineProviderFactory
   {
       IMachineProvider GetProvider(string providerType);
       IMachineProvider GetProviderForMachine(int machineId);
   }
   ```

7. **Create `Services/MachineSyncService.cs`**
   - IHostedService that polls machines periodically
   - Updates Machine.Status and Machine.LastStatusUpdate
   - Creates MachineStateRecord snapshots
   - Configurable poll interval via appsettings.json

8. **Update `SchedulerContext.cs`**
   - Add `DbSet<MachineConnectionSettings>`
   - Add `DbSet<MachineStateRecord>`
   - Configure relationships in OnModelCreating

9. **Update `Program.cs`**
   - Register IMachineProvider implementations
   - Register IMachineProviderFactory
   - Register MachineSyncService as hosted service
   - Add config section for sync interval

10. **Create Migration**
    - `dotnet ef migrations add AddMachineProviderTables`

#### Verification
- Build succeeds
- MockMachineProvider returns varying data
- MachineSyncService logs status updates
- MachineStateRecord table populated

---

### PHASE 2: EOS Client Layer (Shell)
**Goal**: Build EOS-specific clients (shell until credentials available)
**Duration**: 1 session
**Prerequisites**: Phase 1 complete

#### Files to Create
```
OpCentrix/Services/MachineProviders/Eos/
??? EosConnectionConfig.cs      # Config model for EOS connection
??? EosRestClient.cs            # HTTP client with OAuth2 placeholder
??? EosOpcUaClient.cs           # OPC UA client wrapper
??? EosMachineProvider.cs       # Combines REST + OPC UA
??? EosTestDataGenerator.cs     # Generates realistic test data
```

#### Implementation Steps (for Copilot)
1. **Create `EosConnectionConfig.cs`**
   - REST API base URL
   - OPC UA endpoint URL
   - OAuth2 client credentials (placeholders)
   - Certificate paths

2. **Create `EosRestClient.cs`**
   - HttpClient wrapper
   - OAuth2 token acquisition (placeholder)
   - Typed API methods: GetJobStatus, GetMachineInfo, GetAlarms
   - Error handling and retry logic

3. **Create `EosOpcUaClient.cs`**
   - Uses OPCFoundation package (add to csproj)
   - Session management
   - Node ID constants for EOS-specific values
   - Read/Subscribe methods

4. **Create `EosMachineProvider.cs`**
   - Implements IMachineProvider
   - Combines REST (job control) + OPC UA (telemetry)
   - Graceful degradation if one fails

5. **Create `EosTestDataGenerator.cs`**
   - Generates realistic EOS M4 ONIX data
   - Simulates build phases: Warming, Printing, Cooling
   - Layer-by-layer progress
   - Realistic temperature/gas readings

6. **Register in `MachineProviderFactory`**
   - Add "Eos" provider type

#### Verification
- EosTestDataGenerator produces realistic data
- EosMachineProvider falls back to test data
- Provider factory returns correct provider type

---

### PHASE 3: Stage Duration Learning
**Goal**: Auto-refine estimates from actual completions
**Duration**: 1 session
**Prerequisites**: None (can run parallel to Phase 1-2)

#### Files to Create
```
OpCentrix/Services/Learning/
??? IPartStageLearningService.cs
??? PartStageLearningService.cs
??? StageLearningConfiguration.cs   # Config for EMA alpha, thresholds
```

#### Files to Modify
```
OpCentrix/
??? Models/PartStageRequirement.cs  # Add 5 learning fields
??? Services/Admin/MultiStageJobService.cs  # Hook CompleteStageAsync
??? Data/SchedulerContext.cs        # Configure new columns
```

#### Implementation Steps (for Copilot)
1. **Add fields to `PartStageRequirement.cs`**
   ```csharp
   // Learning fields
   public double? ActualAverageDurationHours { get; set; }
   public int ActualSampleCount { get; set; } = 0;
   public double? LastActualDurationHours { get; set; }
   [StringLength(20)]
   public string EstimateSource { get; set; } = "Manual";
   public DateTime? EstimateLastUpdated { get; set; }
   ```

2. **Create `IPartStageLearningService.cs`**
   ```csharp
   public interface IPartStageLearningService
   {
       Task RecordCompletionAsync(int jobStageId, CancellationToken ct = default);
       Task RefineEstimateAsync(int partId, string stageType, CancellationToken ct = default);
       Task<StagePerformanceReport> GetStagePerformanceAsync(int partId, CancellationToken ct = default);
       Task ResetToManualAsync(int partStageRequirementId, double? manualHours, CancellationToken ct = default);
   }
   ```

3. **Create `PartStageLearningService.cs`**
   - RecordCompletionAsync:
     - Load JobStage with Job
     - Calculate ActualDurationHours
     - Write JobStageHistory record
     - Call RefineEstimateAsync
   - RefineEstimateAsync:
     - Find PartStageRequirement by (Job.PartId, JobStage.StageType)
     - Compute EMA with ?=0.3
     - Update all 5 learning fields
   - GetStagePerformanceAsync:
     - Return comparison of estimated vs actual for all stages

4. **Modify `MultiStageJobService.CompleteStageAsync`**
   - After `SaveChangesAsync()`, inject and call `IPartStageLearningService.RecordCompletionAsync(stageId)`

5. **Update `SchedulerContext.cs`**
   - Configure default values for new columns

6. **Create Migration**
   - `dotnet ef migrations add AddStageLearningFields`

7. **Register in `Program.cs`**
   - `builder.Services.AddScoped<IPartStageLearningService, PartStageLearningService>();`

#### Verification
- Complete a stage via StageDashboard
- Verify JobStageHistory.StageHours populated
- Verify PartStageRequirement learning fields updated
- Verify EstimateSource = "Auto"

---

### PHASE 4: Real-Time Updates (SignalR)
**Goal**: Push machine state changes to UI
**Duration**: 1 session
**Prerequisites**: Phase 1 complete

#### Files to Create
```
OpCentrix/
??? Hubs/
?   ??? MachineStateHub.cs
??? Services/
?   ??? IMachineStateNotifier.cs
??? wwwroot/js/
    ??? machine-state-client.js
```

#### Files to Modify
```
OpCentrix/
??? Program.cs                  # Add SignalR
??? Services/MachineSyncService.cs  # Notify hub on state change
??? Pages/PrintTracking/Index.cshtml  # Add SignalR client
```

#### Implementation Steps (for Copilot)
1. **Add SignalR to `Program.cs`**
   - `builder.Services.AddSignalR();`
   - `app.MapHub<MachineStateHub>("/hubs/machinestate");`

2. **Create `MachineStateHub.cs`**
   - JoinMachineGroup(machineId)
   - LeaveMachineGroup(machineId)
   - Methods to broadcast state changes

3. **Create `IMachineStateNotifier.cs`**
   - Interface for notifying connected clients
   - Injected into MachineSyncService

4. **Modify `MachineSyncService.cs`**
   - On state change, call notifier
   - Compare previous state to detect changes

5. **Create `machine-state-client.js`**
   - Connect to SignalR hub
   - Subscribe to machine groups
   - Update UI on state change

6. **Update `PrintTracking/Index.cshtml`**
   - Include SignalR client script
   - Replace polling with SignalR events

#### Verification
- Open PrintTracking in two browsers
- Change machine state
- Both browsers update within 1 second

---

### PHASE 5: Intelligent Scheduling
**Goal**: Stacking recommendations and weekend fill
**Duration**: 1-2 sessions
**Prerequisites**: Phase 3 complete (for duration data)

#### Files to Create
```
OpCentrix/Services/Scheduling/
??? IScheduleSuggestionService.cs
??? ScheduleSuggestionService.cs
??? StackingRecommendation.cs       # DTO for recommendations
??? WeekendFillAnalyzer.cs          # Identifies gaps
```

#### Implementation Steps (for Copilot)
1. **Create `StackingRecommendation.cs`**
   - Recommended stack level (1, 2, 3)
   - Parts per build
   - Estimated duration
   - Efficiency score
   - Reasoning

2. **Create `IScheduleSuggestionService.cs`**
   - GetStackingRecommendationAsync(partId, quantity, startDate)
   - GetWeekendFillSuggestionsAsync(machineId, weekendStart, weekendEnd)
   - GetOptimalScheduleAsync(jobs, machines, constraints)

3. **Create `ScheduleSuggestionService.cs`**
   - Load MasterPart stacking configs
   - Compare efficiency of 1x vs 2x vs 3x
   - Account for weekend capacity
   - Consider part urgency/priority

4. **Create `WeekendFillAnalyzer.cs`**
   - Find gaps in SLS machine schedules
   - Recommend stacked builds for weekends
   - Balance across machines

5. **Add UI for suggestions**
   - Scheduler page shows recommendations
   - One-click accept to create job

#### Verification
- Create part with stacking enabled
- Request schedule suggestion
- Verify correct stack level recommended
- Verify weekend gaps identified

---

## 5. Key Design Decisions

### 5.1 Keep "Stage" Naming (for now)
- Internal models use "Stage" (JobStage, ProductionStage, etc.)
- UI can display as "Operation" if desired
- Avoids confusion for AI assistance and codebase consistency

### 5.2 FK Strategy: Integer PKs
- `MachineConnectionSettings.MachineId` ? FK to `Machine.Id` (int)
- `Machine.MachineId` (string) is business identifier only
- Consistent with existing codebase patterns

### 5.3 Learning Algorithm: Exponential Moving Average
- Formula: `newAvg = ? * newActual + (1-?) * oldAvg`
- ? = 0.3 (30% weight to new observation)
- Minimum sample count = 3 before trusting auto-estimates

### 5.4 Provider Registration Pattern
- Feature flag: `eos_machine_integration` in FeatureToggle table
- Factory pattern for provider selection
- Keep existing `IOpcUaService` for backward compatibility

---

## 6. Database Changes Summary

### New Tables (Phase 1)
```sql
MachineConnectionSettings
??? Id (int, PK)
??? MachineId (int, FK ? Machine.Id)
??? ProviderType (nvarchar(50))
??? ConnectionConfigJson (nvarchar(max))
??? IsEnabled (bit)
??? LastConnectedAt (datetime2, nullable)
??? LastError (nvarchar(500), nullable)
??? INDEX IX_MachineConnectionSettings_MachineId

MachineStateRecords
??? Id (int, PK)
??? MachineId (int, FK ? Machine.Id)
??? Status (nvarchar(50))
??? BuildProgressPercent (float, nullable)
??? CurrentJobReference (nvarchar(100), nullable)
??? StateDataJson (nvarchar(max), nullable)
??? Timestamp (datetime2)
??? INDEX IX_MachineStateRecords_MachineId_Timestamp
```

### Modified Tables (Phase 3)
```sql
PartStageRequirements (add columns)
??? ActualAverageDurationHours (float, nullable)
??? ActualSampleCount (int, default 0)
??? LastActualDurationHours (float, nullable)
??? EstimateSource (nvarchar(20), default 'Manual')
??? EstimateLastUpdated (datetime2, nullable)
```

---

## 7. Testing Strategy

### Unit Tests (OpCentrix.Tests)
```
Tests/MachineProviders/
??? MockMachineProviderTests.cs
?   - WhenGetStatus_ThenReturnsValidStatus
?   - WhenGetMachineData_ThenReturnsTelemetry
?   - WhenTestConnection_ThenReturnsTrue
??? MachineProviderFactoryTests.cs
?   - WhenGetProvider_WithValidType_ThenReturnsCorrectProvider
?   - WhenGetProviderForMachine_ThenLooksUpSettings
??? PartStageLearningServiceTests.cs
    - WhenRecordCompletion_ThenCalculatesActualDuration
    - WhenRefineEstimate_ThenAppliesEMA
    - WhenSampleCountBelowThreshold_ThenKeepsManualEstimate
```

### Integration Tests
```
Tests/Integration/
??? MachineSyncServiceTests.cs
?   - WhenStarted_ThenPollsMachines
?   - WhenStateChanges_ThenCreatesStateRecord
??? StageLearningIntegrationTests.cs
    - WhenStageCompleted_ThenUpdatesPartStageRequirement
```

---

## 8. Continuation Instructions

### If Chat Fails - How to Resume

1. **Read this file first**: `OPCENTRIX_PROJECT_CONTEXT.md`

2. **Check current branch**:
   ```bash
   git branch --show-current
   # Should be: claude/machine-provider-and-stage-learning-wbQDv
   ```

3. **Check what's been created**:
   ```bash
   git status
   # Look for new files in Services/MachineProviders/
   # Look for modified PartStageRequirement.cs
   ```

4. **Review current progress in Section 9 below**

5. **Continue from the next incomplete phase**

### Key Files to Review When Resuming
| File | Why |
|------|-----|
| `Services/MachineProviders/IMachineProvider.cs` | Core provider interface |
| `Services/MachineProviders/MockMachineProvider.cs` | Reference implementation |
| `Services/Learning/IPartStageLearningService.cs` | Learning interface |
| `Models/PartStageRequirement.cs` | Check if learning fields added |
| `Services/Admin/MultiStageJobService.cs` | Check if learning hook added |
| `Program.cs` | Check service registrations (at bottom) |

---

## 9. Current Progress

### Completed
- [x] Deep codebase analysis
- [x] Identified actual gaps vs. perceived completeness
- [x] Created detailed implementation plan with step-by-step instructions
- [x] This context document (v2 - honest assessment)

### Phase 1: Machine Provider Foundation
- [x] Create MachineProviderModels.cs
- [x] Create MachineConnectionSettings.cs
- [x] Create MachineStateRecord.cs
- [x] Create IMachineProvider.cs
- [x] Create MockMachineProvider.cs
- [x] Create MachineProviderFactory.cs
- [x] Create MachineSyncService.cs
- [x] Update SchedulerContext.cs
- [x] Update Program.cs
- [x] Create migration ?
- [x] Verify build succeeds ?
- [x] Database tables created ?

### Phase 2: EOS Client Layer
- [ ] Create EosConnectionConfig.cs
- [ ] Create EosRestClient.cs
- [ ] Create EosOpcUaClient.cs
- [ ] Create EosMachineProvider.cs
- [ ] Create EosTestDataGenerator.cs
- [ ] Register in factory
- [ ] Verify build succeeds

### Phase 3: Stage Duration Learning
- [x] Add 5 fields to PartStageRequirement.cs
- [x] Create IPartStageLearningService.cs
- [x] Create PartStageLearningService.cs
- [x] Hook into MultiStageJobService.CompleteStageAsync
- [x] Create migration ?
- [x] Register in Program.cs
- [x] Verify build succeeds ?
- [x] Database columns created ?
- [ ] Test stage completion updates estimates

### Phase 4: Real-Time Updates
- [ ] Add SignalR to Program.cs
- [ ] Create MachineStateHub.cs
- [ ] Create IMachineStateNotifier.cs
- [ ] Modify MachineSyncService.cs
- [ ] Create machine-state-client.js
- [ ] Update PrintTracking page
- [ ] Test real-time updates

### Phase 5: Intelligent Scheduling
- [ ] Create StackingRecommendation.cs
- [ ] Create IScheduleSuggestionService.cs
- [ ] Create ScheduleSuggestionService.cs
- [ ] Create WeekendFillAnalyzer.cs
- [ ] Add scheduler UI suggestions
- [ ] Test recommendations

---

## 10. Quick Reference

### Commands
```bash
# Build
dotnet build

# Run migrations
dotnet ef migrations add <MigrationName> -p OpCentrix -s OpCentrix
dotnet ef database update -p OpCentrix -s OpCentrix

# Run tests
dotnet test

# Run application
dotnet run --project OpCentrix
```

### Important Namespaces
```csharp
using OpCentrix.Models;
using OpCentrix.Models.MachineProviders;
using OpCentrix.Services;
using OpCentrix.Services.MachineProviders;
using OpCentrix.Services.MachineProviders.Eos;
using OpCentrix.Services.Learning;
using OpCentrix.Data;
```

### Configuration Keys (appsettings.json)
```json
{
  "MachineSync": {
    "Enabled": true,
    "PollIntervalSeconds": 30,
    "DefaultProviderType": "Mock"
  },
  "StageLearning": {
    "EmaAlpha": 0.3,
    "MinSampleCount": 3,
    "AutoUpdateEnabled": true
  }
}
```

---

*Last verified: Phase 1 & Phase 3 COMPLETE with database migrations - 2026-03-10*
*Next action: Begin Phase 2 (EOS Client Layer) or test application*
