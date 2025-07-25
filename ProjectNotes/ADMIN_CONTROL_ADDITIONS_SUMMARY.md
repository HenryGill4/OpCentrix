# Admin Control System – Additions & Spoken Notes Summary

This file captures all additions and spoken feedback from the conversation that were not originally included in the Admin Control System plan. These items are now approved and should be integrated into the main plan.

---

## ✅ 1. User Management Panel
- **Page**: /Admin/Users
- **Purpose**: Add/edit user accounts, reset passwords, assign or revoke roles.
- **Notes**:
  - Must integrate with existing role-based login system.
  - Includes ability to disable users and change roles on the fly.

---

## ✅ 2. Scheduler Zoom Levels
- **Zoom Increments**: 12h, 10h, 8h, 6h, 4h, 2h, 1h
- **Purpose**: Allow granular visual scaling of job blocks.
- **Notes**: Visual width of jobs must adapt smoothly at each zoom level.

---

## ✅ 3. Extended Scheduler View
- **Purpose**: Show up to two months in advance on the daily scheduler.
- **Notes**: Applies to day-based views to allow long-term planning.

---

## ✅ 4. Color-Coded Job Blocks
- **Purpose**: Improve visual clarity of jobs and transitions.
- **Notes**:
  - Use color coding to distinguish job types, changeovers, cooldowns.
  - Ensure transitions are clearly visible at all zoom levels.

---

## ✅ 5. Default Changeover and Cooldown Times
- **Changeover**: 3 hours
- **Cooldown**: 1 hour
- **Notes**:
  - These should be configurable from `/Admin/Settings`.
  - Defaults should apply across the system unless overridden.

---

## ✅ 6. Dynamic Machine Addition
- **Purpose**: Allow administrators to add new machines beyond TI1, TI2, and INC.
- **Notes**: Each machine added must integrate fully with scheduler logic.

---

## ✅ 7. Modular Scheduling for Multi-Stage Jobs
- **Stages Supported**:
  - Printing
  - EDM
  - Cerakoting
  - Assembly
- **Notes**: Each stage should have its own visual schedule and permissions.

---

## ✅ 8. Master Schedule View
- **Purpose**: Combine all job stages into a single visual workflow.
- **Notes**: Should show part progress across departments in correct order.

---

## ✅ 9. Notes Field Per Schedule Step
- **Purpose**: Allow notes per part/job step.
- **Notes**: Used for communication or instructions per job stage.

---

## ✅ 10. Confirmed Folder Structure
```
/Pages/Admin          — Razor Pages for admin views
/Pages/Admin/Shared   — Shared layout/partials
/ViewModels/Admin     — View models for admin logic
/Services/Admin       — Backend logic and services
```

---

Generated on: 2025-07-25 20:09:38
