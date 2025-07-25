# Admin Control System - Comprehensive Claude Sonnet 4 Prompts

This file consolidates all approved plans for the Admin Control System into a single ordered list of prompts. Execute each prompt sequentially with Claude Sonnet 4 and run `dotnet test` after completing each step to ensure the solution remains stable. If files must be replaced or removed, specify the deletions in the prompt.

0. **Baseline Validation**
   - Prompt: "Install the required .NET SDK and restore packages. Run `dotnet test` to verify the baseline before implementing features."

1. **Finalize Folder Structure**
   - Prompt: "Create `/Pages/Admin`, `/Pages/Admin/Shared`, `/ViewModels/Admin`, and `/Services/Admin` for admin logic. Delete any obsolete admin placeholder files. Run `dotnet test`."

2. **Prepare Database Models and Migrations**
   - Prompt: "Add entities `OperatingShift`, `MachineCapability`, `SystemSetting`, `RolePermission`, `InspectionCheckpoint`, `DefectCategory`, `ArchivedJob`, `AdminAlert`, and `FeatureToggle`. Extend existing `Machine`, `Part`, and `Role` models accordingly. Generate initial migrations and update the database schema. Remove outdated migration scripts. Run `dotnet test`."

3. **System Settings Panel**
   - Prompt: "Implement `/Admin/Settings` using a `SystemSettingService` to edit global options such as default changeover (3h) and cooldown (1h). Update configuration loading on startup. Run `dotnet test`."

4. **Role-Based Permission Grid**
   - Prompt: "Create `/Admin/Roles` to manage feature access per role via the new `RolePermission` table. Include UI grid and service layer. Run `dotnet test`."

5. **User Management Panel**
   - Prompt: "Build `/Admin/Users` for adding, editing, disabling, and resetting passwords while assigning roles. Ensure integration with the existing authentication system. Run `dotnet test`."

6. **Machine Status and Dynamic Addition**
   - Prompt: "Extend machine data with capability fields and support adding machines beyond TI1/TI2/INC in `/Admin/Machines`. Remove any hard-coded machine lists. Run `dotnet test`."

7. **Part Management Enhancements**
   - Prompt: "Update `/Admin/Parts` so administrators can override estimated duration and validate inputs. Adjust scheduling calculations to respect overrides. Run `dotnet test`."

8. **Operating Shift Editor**
   - Prompt: "Develop `/Admin/Shifts` with a calendar grid and CRUD operations via `OperatingShiftService`. Ensure scheduled jobs are validated against defined shifts. Run `dotnet test`."

9. **Scheduler UI Improvements**
   - Prompt: "Add zoom levels (12h down to 1h), extend the scheduler view to two months, color-code job blocks, and allow notes per schedule step. Remove outdated view code. Run `dotnet test`."

10. **Scheduler Orientation Toggle**
    - Prompt: "Add an option to view the schedule horizontally with machines listed on the left and vertically with machines across the top. Implement a UI toggle or control that swaps the orientation and updates the layout accordingly. Ensure all scheduler features work in both orientations. Run `dotnet test`."

11. **Modular Multi-Stage Scheduling**
    - Prompt: "Implement multi-stage jobs (Printing, EDM, Cerakoting, Assembly) with individual permissions and schedules. Update data models and UI. Run `dotnet test`."

12. **Master Schedule View**
    - Prompt: "Create a master schedule combining all job stages into a single workflow view. Provide filters and progress indicators. Run `dotnet test`."

13. **Inspection Checkpoints Configuration**
    - Prompt: "Add `/Admin/Checkpoints` to assign checkpoints to parts with ordering and active flags. Include data validation and ability to deactivate checkpoints. Run `dotnet test`."

14. **Defect Category Manager**
    - Prompt: "Implement `/Admin/Defects` for CRUD of defect tags, preventing deletion if tags are referenced. Run `dotnet test`."

15. **Job Archive and Cleanup Tools**
    - Prompt: "Create `/Admin/Archive` to move old jobs to `ArchivedJob` and optionally delete them with auditing. Remove outdated cleanup scripts. Run `dotnet test`."

16. **Database Export / Diagnostics**
    - Prompt: "Add `/Admin/Database` to export/import data and run schema validation checks using EF Core. Run `dotnet test`."

17. **Admin Alerts Panel**
    - Prompt: "Build `/Admin/Alerts` to configure triggers and email recipients, backed by `AdminAlert`. Implement a background notification service. Run `dotnet test`."

18. **Feature Toggles Panel**
    - Prompt: "Create `/Admin/Features` to toggle experimental features at runtime via the `FeatureToggle` table. Ensure components read these toggles. Run `dotnet test`."

19. **Final Integration and Testing**
    - Prompt: "Wire up site navigation for all admin pages, apply pending migrations, and run `dotnet test` across all projects. Update the README with admin usage instructions. Delete any superseded documentation files."

Follow these prompts sequentially to fully implement and verify the Admin Control System.
