# Implementation Plans - MasterPart System Refactoring

This folder contains sequential implementation plans based on the **MasterPart-System-Refactoring-Plan.md**. Follow these plans in order to complete the system refactoring.

## Plan Execution Order

### ?? PHASE 1: FIX THE BROKEN FOUNDATION (Week 1-2)
**Priority: CRITICAL - Nothing works without this**

1. **[01-Emergency-Database-Fixes.md](01-Emergency-Database-Fixes.md)** ?? **START HERE**
   - Add missing PowderStock and PowderConsumption tables
   - Add PrinterEstimatedEndTime field to ProductionBuilds
   - Fix foreign key constraints
   - Status: **NOT STARTED**

2. **[02-Delete-Duplicate-ProductionBuild-Pages.md](02-Delete-Duplicate-ProductionBuild-Pages.md)**
   - Remove wrong approach dashboard pages
   - Clean up duplicate functionality
   - Status: **NOT STARTED**

3. **[03-PrintTracking-ProductionBuild-Integration.md](03-PrintTracking-ProductionBuild-Integration.md)**
   - Integrate ProductionBuild system with existing PrintTracking
   - Add printer estimated end time field
   - Add powder tracking to existing modals
   - Status: **NOT STARTED**

### ? PHASE 2: CORE FEATURES + STAGE-SPECIFIC FORMS (Week 3-4)
**Priority: HIGH - Basic production tracking + Better UX**

4. **[04-Stage-Specific-Job-Forms.md](04-Stage-Specific-Job-Forms.md)**
   - Create CNC, EDM, Generic job forms
   - Implement machine type routing
   - Create stage-specific DTOs
   - Status: **NOT STARTED**

5. **[05-Service-Method-Implementation.md](05-Service-Method-Implementation.md)**
   - Implement missing ProductionBuildService methods
   - Create basic PowderInventoryService
   - Fix dashboard data loading
   - Status: **NOT STARTED**

6. **[06-Remove-Calculated-Time-Logic.md](06-Remove-Calculated-Time-Logic.md)**
   - Remove all calculated duration fields from MasterPart forms
   - Remove stack time multipliers (1.3x, 1.6x)
   - Replace with machine estimate tracking
   - Status: **NOT STARTED**

### ?? PHASE 3: ENHANCEMENT FEATURES (Week 5-8)
**Priority: MEDIUM - Nice to have**

7. **[07-Machine-Accuracy-Tracking.md](07-Machine-Accuracy-Tracking.md)**
   - Add BuildTimeHistory table
   - Track printer estimate vs actual times
   - Simple accuracy reporting
   - Status: **NOT STARTED**

8. **[08-Powder-Management-UI.md](08-Powder-Management-UI.md)**
   - Enhance dashboard with powder stock indicators
   - Add powder inventory management
   - Stock alerts integration
   - Status: **NOT STARTED**

9. **[09-Enhanced-Stage-Forms-Validation.md](09-Enhanced-Stage-Forms-Validation.md)**
   - Machine-specific validation
   - Part compatibility checks
   - Smart defaults and cost estimation
   - Status: **NOT STARTED**

### ?? PHASE 4: ADVANCED FEATURES (Month 2)
**Priority: LOW - Future enhancements**

10. **[10-Version-Control-System.md](10-Version-Control-System.md)**
    - Full version control implementation
    - Change approval workflows
    - Version comparison tools
    - Status: **NOT STARTED**

11. **[11-Enhanced-MasterPart-Form.md](11-Enhanced-MasterPart-Form.md)**
    - Calculation-free MasterPart form
    - Historical machine accuracy display
    - Intelligent stacking recommendations
    - Status: **NOT STARTED**

12. **[12-Advanced-Stage-Intelligence.md](12-Advanced-Stage-Intelligence.md)**
    - CNC tool life tracking
    - EDM wire consumption optimization
    - SLS powder efficiency analytics
    - Status: **NOT STARTED**

---

## How to Use These Plans

1. **Start with Plan 1** and complete it fully before moving to the next
2. **Test after each plan** - run the application and verify functionality
3. **Update status** in this README as you complete each plan
4. **Don't skip ahead** - later plans depend on earlier foundations
5. **Use run_build** after each plan to verify no compilation errors

## Key Architectural Principles

- ? **Enhance existing PrintTracking system** (don't create duplicates)
- ? **Machine-driven approach** (no calculations, trust printer estimates)
- ? **Stage-specific forms** (CNC, EDM, SLS have different parameters)
- ? **Single unified workflow** for operators
- ? **No separate ProductionBuild dashboard** (enhance PrintTracking instead)
- ? **No calculated build times** (operators enter printer estimates)

---

*Last Updated: January 2025*
*Status: Plans created, implementation not started*