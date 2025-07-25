# Admin Logic Ordered Execution Plan

This plan outlines the recommended sequence for implementing the Admin Control System features described in `ADMIN_CONTROL_IMPLEMENTATION_PLAN.md` and the approved additions. Each step is phrased as a Claude Sonnet 4 prompt to keep the tasks concise and actionable.

1. **Finalize Folder Structure**
   - Prompt: "Create `/Pages/Admin`, `/Pages/Admin/Shared`, `/ViewModels/Admin`, and `/Services/Admin` as the home for all admin logic."

2. **Prepare Database Models and Migrations**
   - Prompt: "Add entities `OperatingShift`, `MachineCapability`, `SystemSetting`, `RolePermission`, `InspectionCheckpoint`, `DefectCategory`, `ArchivedJob`, `AdminAlert`, and `FeatureToggle`. Extend `Machine`, `Part`, and `Role`. Generate initial migrations."

3. **System Settings Panel**
   - Prompt: "Build `/Admin/Settings` with a `SystemSettingService` to edit global options like default changeover (3h) and cooldown (1h)."

4. **Role-Based Permission Grid**
   - Prompt: "Create `/Admin/Roles` to manage feature access per role using the new `RolePermission` table."

5. **User Management Panel**
   - Prompt: "Implement `/Admin/Users` to add, edit, disable, and reset passwords for user accounts while assigning roles."

6. **Machine Status and Dynamic Addition**
   - Prompt: "Extend machine data with capabilities and allow new machines beyond TI1/TI2/INC in `/Admin/Machines`."

7. **Part Management Enhancements**
   - Prompt: "Update `/Admin/Parts` to support duration overrides and related validation."

8. **Operating Shift Editor**
   - Prompt: "Develop `/Admin/Shifts` with a calendar grid and CRUD via `OperatingShiftService`."

9. **Scheduler UI Improvements**
   - Prompt: "Add zoom levels (12h–1h), extended two‑month view, color‑coded job blocks, and notes per schedule step."

10. **Modular Scheduling for Multi‑Stage Jobs**
    - Prompt: "Support Printing, EDM, Cerakoting, and Assembly stages with individual permissions and schedules."

11. **Master Schedule View**
    - Prompt: "Combine all job stages into one workflow view showing progress across departments."

12. **Inspection Checkpoints Configuration**
    - Prompt: "Create `/Admin/Checkpoints` to assign checkpoints to parts with ordering and active flags."

13. **Defect Category Manager**
    - Prompt: "Provide `/Admin/Defects` for CRUD of defect tags, blocking deletion when referenced."

14. **Job Archive and Cleanup Tools**
    - Prompt: "Implement `/Admin/Archive` to move old jobs to `ArchivedJob` and optionally delete with auditing."

15. **Database Export / Diagnostics**
    - Prompt: "Add `/Admin/Database` to export/import data and run schema validation with EF Core."

16. **Admin Alerts Panel**
    - Prompt: "Use `/Admin/Alerts` to define triggers and email recipients with a background notification service."

17. **Feature Toggles Panel**
    - Prompt: "Create `/Admin/Features` to toggle experimental features at runtime from a database table."

18. **Final Integration and Testing**
    - Prompt: "Wire up navigation, run migrations, test each module end-to-end, and document usage in the README."

Follow this chronological order to avoid rework and deliver the admin capabilities smoothly.
