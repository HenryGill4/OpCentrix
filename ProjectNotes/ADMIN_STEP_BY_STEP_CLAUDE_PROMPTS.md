# Admin Execution Prompts with Testing

This file provides a sequenced set of Claude Sonnet 4 prompts to implement the Admin Control System. Each step concludes with instructions to run the test suite so progress is validated incrementally. Copy each prompt to Claude one at a time and execute in order.

0. **Setup Testing Environment**
   - Prompt: "Install the required .NET SDK and restore packages. Run `dotnet test` to verify the baseline before implementing features."

1. **Finalize Folder Structure**
   - Prompt: "Create `/Pages/Admin`, `/Pages/Admin/Shared`, `/ViewModels/Admin`, and `/Services/Admin` as the home for all admin logic. After creating these folders, run `dotnet test` to ensure no regressions."  

2. **Prepare Database Models and Migrations**
   - Prompt: "Add entities `OperatingShift`, `MachineCapability`, `SystemSetting`, `RolePermission`, `InspectionCheckpoint`, `DefectCategory`, `ArchivedJob`, `AdminAlert`, and `FeatureToggle`. Extend `Machine`, `Part`, and `Role`. Generate initial migrations. When done, execute `dotnet test` to validate."  

3. **System Settings Panel**
   - Prompt: "Build `/Admin/Settings` with a `SystemSettingService` to edit global options like default changeover (3h) and cooldown (1h). Run `dotnet test` afterwards."  

4. **Role-Based Permission Grid**
   - Prompt: "Create `/Admin/Roles` to manage feature access per role using the new `RolePermission` table. Run `dotnet test` when complete."  

5. **User Management Panel**
   - Prompt: "Implement `/Admin/Users` to add, edit, disable, and reset passwords for user accounts while assigning roles. Confirm with `dotnet test`."  

6. **Machine Status and Dynamic Addition**
   - Prompt: "Extend machine data with capabilities and allow new machines beyond TI1/TI2/INC in `/Admin/Machines`. After implementation, run `dotnet test`."  

7. **Part Management Enhancements**
   - Prompt: "Update `/Admin/Parts` to support duration overrides and related validation. Execute `dotnet test` to confirm behavior."  

8. **Operating Shift Editor**
   - Prompt: "Develop `/Admin/Shifts` with a calendar grid and CRUD via `OperatingShiftService`. Validate with `dotnet test`."  

9. **Scheduler UI Improvements**
   - Prompt: "Add zoom levels (12h–1h), extended two‑month view, color‑coded job blocks, and notes per schedule step. Run `dotnet test` to ensure updates integrate."  

10. **Modular Scheduling for Multi‑Stage Jobs**
    - Prompt: "Support Printing, EDM, Cerakoting, and Assembly stages with individual permissions and schedules. Then run `dotnet test`."  

11. **Master Schedule View**
    - Prompt: "Combine all job stages into one workflow view showing progress across departments. After completion, run `dotnet test`."  

12. **Inspection Checkpoints Configuration**
    - Prompt: "Create `/Admin/Checkpoints` to assign checkpoints to parts with ordering and active flags. Confirm with `dotnet test`."  

13. **Defect Category Manager**
    - Prompt: "Provide `/Admin/Defects` for CRUD of defect tags, blocking deletion when referenced. Run `dotnet test` when done."  

14. **Job Archive and Cleanup Tools**
    - Prompt: "Implement `/Admin/Archive` to move old jobs to `ArchivedJob` and optionally delete with auditing. Then run `dotnet test`."  

15. **Database Export / Diagnostics**
    - Prompt: "Add `/Admin/Database` to export/import data and run schema validation with EF Core. Execute `dotnet test` afterwards."  

16. **Admin Alerts Panel**
    - Prompt: "Use `/Admin/Alerts` to define triggers and email recipients with a background notification service. Verify with `dotnet test`."  

17. **Feature Toggles Panel**
    - Prompt: "Create `/Admin/Features` to toggle experimental features at runtime from a database table. Run `dotnet test`."  

18. **Final Integration and Testing**
    - Prompt: "Wire up navigation, apply migrations, run `dotnet test` for the entire solution, and document usage in the README."

Follow these prompts sequentially to implement and test the Admin Control System fully.
