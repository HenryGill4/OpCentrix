# Admin Control System Implementation Plan

## Overview
This document outlines the steps and components required to add a comprehensive Admin Control System to OpCentrix. Each section describes a specific admin feature with a focus on page layout, required database changes, and core logic.

---

## Operating Shift Editor
- **Page Name**: `/Admin/Shifts`
- **Purpose**: Define production hours and manage daily operating schedules.
- **UI Description**: Calendar-style grid allowing administrators to set start and end times for each day, plus add exceptions for holidays or maintenance.
- **Database Tables Needed**:
  - `OperatingShift` with `Id`, `DayOfWeek`, `StartTime`, `EndTime`, `IsHoliday`.
  - Adjust existing scheduling logic to reference this table.
- **Logic Overview**: Load and edit operating hours via forms; enforce constraints so scheduled jobs fall inside active shifts.

## Machine Status & Capabilities Editor
- **Page Name**: `/Admin/Machines`
- **Purpose**: Configure machine details, status, and supported materials.
- **UI Description**: Table of machines with inline edit for name, model, supported materials, and availability toggles (e.g., TI1, TI2, INC). Status indicators show active or offline.
- **Database Tables Needed**:
  - Existing `Machine` table extended with `Capabilities` and `Status` fields.
  - Optionally a `MachineCapability` join table if multi-select is required.
- **Logic Overview**: CRUD operations for machines, validation for unique machine names, and logic to prevent scheduling on inactive machines.

## System Settings Panel
- **Page Name**: `/Admin/Settings`
- **Purpose**: Manage global settings such as changeover time and default job parameters.
- **UI Description**: Form-based layout with grouped setting categories and save/apply buttons. Include reset-to-default actions.
- **Database Tables Needed**:
  - `SystemSetting` table with key/value pairs (`SettingKey`, `SettingValue`).
- **Logic Overview**: Load settings on application startup and provide caching. Updating a setting should refresh in-memory values for all running instances.

## Role-Based Permission Grid
- **Page Name**: `/Admin/Roles`
- **Purpose**: Enable or disable features by user role.
- **UI Description**: Grid listing roles vertically and features horizontally with checkboxes. Administrators can toggle permissions for each feature.
- **Database Tables Needed**:
  - `Role` table (existing) extended if necessary.
  - `RolePermission` table mapping roles to feature flags.
- **Logic Overview**: Authorization middleware consults the `RolePermission` table to determine feature access.

## Part Management Panel
- **Page Name**: `/Admin/Parts`
- **Purpose**: Perform full CRUD on parts with optional duration overrides.
- **UI Description**: List of parts with add/edit/delete actions and search filters. Duration override field appears in the edit form.
- **Database Tables Needed**:
  - Existing `Part` table gains optional `DefaultDurationMinutes` and override indicators.
- **Logic Overview**: Validate unique part numbers and ensure duration overrides propagate to job scheduling calculations.

## Inspection Checkpoints Configuration
- **Page Name**: `/Admin/Checkpoints`
- **Purpose**: Manage inspection checkpoints for each part.
- **UI Description**: Master-detail screen listing parts on the left and checkpoints on the right. Allows adding, reordering, and disabling checkpoints.
- **Database Tables Needed**:
  - `InspectionCheckpoint` table linked to `Part` via `PartId`.
- **Logic Overview**: When creating jobs, automatically associate relevant checkpoints based on part. Enforce ordering and active/inactive flags.

## Defect Category Manager
- **Page Name**: `/Admin/Defects`
- **Purpose**: Configure defect tags used during quality checks.
- **UI Description**: Simple list with add/edit/delete for defect categories. Each row shows category name and description.
- **Database Tables Needed**:
  - `DefectCategory` table with `Id`, `Name`, `Description`.
- **Logic Overview**: Provide lookup data for inspection modules and ensure that removing categories is restricted if referenced in past records.

## Job Archive & Cleanup Tools
- **Page Name**: `/Admin/Archive`
- **Purpose**: Move or remove old jobs from the active schedule and clean up obsolete data.
- **UI Description**: Filter jobs by date range with archive/delete buttons. Summary panel shows database space saved.
- **Database Tables Needed**:
  - `ArchivedJob` table mirroring key fields from `Job`.
- **Logic Overview**: Selected jobs are copied to `ArchivedJob` and deleted from `Job`. Deletion requires confirmation and audit logging.

## Database Export / Diagnostics
- **Page Name**: `/Admin/Database`
- **Purpose**: Export and import data plus run schema validation.
- **UI Description**: Buttons to export tables as JSON/CSV, import from file, and run diagnostics with status output.
- **Database Tables Needed**: No new tables; uses existing schema metadata.
- **Logic Overview**: Use EF Core to serialize records. Diagnostics verify table counts and required columns, logging issues for review.

## Admin Alerts Panel
- **Page Name**: `/Admin/Alerts`
- **Purpose**: Configure automated email alerts for important events.
- **UI Description**: Form for defining alert triggers (e.g., job completion, machine error) and recipient lists.
- **Database Tables Needed**:
  - `AdminAlert` table with trigger type, email recipients, and active status.
- **Logic Overview**: Background service monitors events and sends emails when trigger conditions are met.

## Feature Toggles Panel
- **Page Name**: `/Admin/Features`
- **Purpose**: Enable or disable experimental features at runtime.
- **UI Description**: Toggle list showing feature name, description, and on/off switch. Includes a note that changes may require reload.
- **Database Tables Needed**:
  - `FeatureToggle` table with `Name`, `IsEnabled`.
- **Logic Overview**: Application components read toggle values and adjust behavior accordingly.

---

## Proposed Folder Structure
```
/Pages/Admin          - Razor Pages for all admin screens
/Pages/Admin/Shared   - Shared layouts and components
/ViewModels/Admin     - View models specific to admin pages
/Services/Admin       - Backend services and business logic
```

---

**Important:** This plan should be reviewed and approved before beginning implementation. Do not start building these features until approval is received.
