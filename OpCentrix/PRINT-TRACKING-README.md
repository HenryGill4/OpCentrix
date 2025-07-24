# ??? OpCentrix Print Tracking System

## Overview

The OpCentrix Print Tracking System has been refactored to provide a clean, operator-focused workflow for tracking actual 3D printer operations. This system replaces complex scheduling with simple, required forms that capture the reality of production.

## ?? Key Features

### ? **Print Start Form** (Required when print begins)
- **Printer Selection**: TI1, TI2, INC
- **Actual Start Time**: Auto-filled to current time, editable
- **Associated Job**: Optional link to scheduled job
- **Setup Notes**: Optional operator notes
- **Auto-fills operator from session**

### ? **Post-Print Log Form** (Required when print finishes)
- **Basic Info**: Printer, start/end times from printer display
- **Part Numbers**: Multi-part support with primary part designation
- **Delay Tracking**: Automatic delay detection and logging
- **Optional Details**: Laser run time, gas usage, powder usage
- **Reason for End**: Completed, Aborted, Error, etc.

### ? **Automatic Features**
- **Delay Detection**: Compares actual vs scheduled start times
- **Cooldown Blocks**: 1-hour cooldown after each job
- **Changeover Blocks**: 3-hour changeover blocks
- **Operator Accountability**: All actions tied to logged-in user

## ??? Database Schema

### BuildJobs Table
- One record per actual print operation
- Tracks actual start/end times vs scheduled times
- Links to operator who performed the work
- Stores printer summary data (gas, powder, laser time)

### BuildJobParts Table
- Multiple parts per build job (multi-part plates)
- Primary part designation for schedule display
- Quantity tracking per part

### DelayLogs Table
- Automatic delay tracking when jobs start late
- Categorized delay reasons (Argon Refill, Plate Not Ready, etc.)
- Duration calculated automatically

## ?? Getting Started

### 1. Setup (First Time)
```bash
# Windows
setup-print-tracking.bat

# Linux/Mac
chmod +x setup-print-tracking.sh
./setup-print-tracking.sh
```

### 2. Run Application
```bash
dotnet run
```

### 3. Access Print Tracking
1. Open: http://localhost:5000
2. Login with your credentials
3. Navigate to "Print Tracking" in the main menu

## ?? User Roles

- **Operator**: Can start/complete prints, view dashboard
- **Scheduler**: Operator access + scheduling functions
- **Manager**: All access + analytics
- **Admin**: Full system access

## ?? Dashboard Features

- **Active Builds**: Live view of running prints
- **Quick Stats**: Active builds, completed today, total hours, delays
- **Recent Activity**: Completed builds and delay logs
- **Quick Actions**: Start/complete prints by printer

## ?? Workflow

### Starting a Print
1. Click "Start Print" button or printer-specific action
2. Select printer (TI1, TI2, INC)
3. Set actual start time (defaults to now)
4. Optionally link to scheduled job
5. Add setup notes if needed
6. Submit form

### Completing a Print
1. Click "Complete" on active build or use Complete button
2. Enter actual start/end times from printer display
3. Select reason for end (Completed, Aborted, etc.)
4. Add part numbers and quantities
5. Fill optional details (gas, powder, laser time)
6. Handle delay information if detected
7. Submit form

### Automatic Actions
- **Cooldown Block**: 1-hour block created after job
- **Changeover Block**: 3-hour block for material/setup changes
- **Delay Logging**: Automatic if actual start > scheduled start
- **Audit Trail**: All actions logged with timestamps

## ?? Integration with Existing System

The print tracking system works alongside the existing OpCentrix scheduler:

- **Existing Jobs**: Can be linked to actual print jobs
- **Schedule Display**: Shows actual vs planned times
- **Part Library**: Integrates with existing part database
- **User System**: Uses existing authentication and roles

## ?? Mobile Support

The interface is fully responsive and works on tablets and phones for shop floor use.

## ?? Analytics

The system provides:
- Actual vs planned time tracking
- Delay analysis by reason and frequency
- Printer utilization metrics
- Operator productivity tracking

## ??? Technical Details

- **Framework**: .NET 8 Razor Pages
- **Database**: SQLite with Entity Framework Core
- **Frontend**: HTMX + Tailwind CSS
- **Authentication**: Cookie-based with role management

## ?? Next Steps

1. **Hardware Integration**: Connect to TruPrint 3000 OPC UA for automatic data
2. **Mobile App**: Native iOS/Android for operators
3. **Advanced Analytics**: ML-powered time prediction
4. **ERP Integration**: Connect to existing business systems

---

*For technical support or questions, refer to the comprehensive documentation in the `ProjectNotes/` directory.*