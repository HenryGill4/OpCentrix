# ?? **OpCentrix AI Assistant Prompt Helper**

**Version**: 2.0  
**Date**: January 30, 2025  
**Purpose**: Essential prompt instructions for AI assistants working on OpCentrix MES  
**Context**: Razor Pages .NET 8 project with SQLite database  

---

## ?? **CRITICAL MANDATORY INSTRUCTIONS**

### ?? **ALWAYS START WITH RESEARCH**
```markdown
**REQUIRED BEFORE ANY MODIFICATIONS:**
- Use `text_search` tool before making changes to understand context
- Use `get_file` tool to read existing files before editing them  
- Never assume file contents - always verify current state
- Understand the existing implementation before adding new features
```

### ?? **MANDATORY DATABASE BACKUP PROTOCOL**
```powershell
# ALWAYS backup database before ANY database changes
New-Item -ItemType Directory -Path "../backup/database" -Force
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item "OpCentrix/scheduler.db" "../backup/database/scheduler_backup_$timestamp.db"
Test-Path "../backup/database/scheduler_backup_$timestamp.db"

# Verify backup was created successfully before proceeding
```

### ?? **POWERSHELL-ONLY COMMANDS (CRITICAL)**
```powershell
# ? CORRECT: Use individual PowerShell commands
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj
dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal

# ? NEVER USE: && operators in PowerShell (WILL FAIL)
# dotnet clean && dotnet restore  # This WILL FAIL

# ?? NEVER USE: dotnet run (freezes AI assistant)
```

### ?? **VALIDATION PROTOCOL AFTER CHANGES**
```powershell
# Test after each change/phase completion
dotnet clean
dotnet restore
dotnet build OpCentrix/OpCentrix.csproj

# Check database integrity if database was modified
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# Only run tests if build succeeds
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build successful - ready for tests"
    dotnet test OpCentrix.Tests/OpCentrix.Tests.csproj --verbosity normal
} else {
    Write-Host "? Build failed - fix errors before proceeding"
    exit 1
}
```

---

## ?? **DEVELOPMENT WORKFLOW PROTOCOL**

### ?? **Pre-Implementation Research**
```markdown
For ANY feature implementation:

1. **Search for existing functionality**:
   - Use text_search with relevant terms
   - Check for existing models, services, pages
   - Understand current architecture

2. **Read existing files**:
   - Use get_file to read current implementation
   - Never edit without reading first
   - Understand existing patterns and conventions

3. **Check database schema**:
   - Verify table structure with sqlite3 commands
   - Check existing relationships and constraints
   - Understand current data model
```

### ?? **File Modification Protocol**
```markdown
When editing files:

1. **Read the file first** using get_file tool
2. **Use minimal, targeted edits** with comments for unchanged sections
3. **Group changes by file** - complete all changes to one file at once
4. **Test after each file** with dotnet build
5. **Use get_errors tool** to validate changes
6. **Never show code blocks** - use edit_file tool instead
```

### ?? **Edit File Best Practices**
```csharp
// ? CORRECT: Use comments for existing code sections
public class ExampleService
{
    // ...existing code...
    
    // New method being added
    public async Task<SomeResult> NewMethodAsync()
    {
        // Implementation here
    }
    
    // ...existing code...
    
    // Modified method
    public async Task<ExistingResult> ModifiedMethodAsync()
    {
        // Updated implementation
    }
}
```

---

## ??? **DATABASE MODIFICATION PROTOCOLS**

### ?? **CRITICAL: Database Update Approach**
```markdown
**NEVER use EF Core migrations for existing production databases**

**CORRECT APPROACH for database updates:**
1. Create backup first (mandatory)
2. Use individual SQLite ALTER TABLE commands
3. Provide SQL for manual execution in DB Browser
4. Include verification queries
5. Test database integrity after changes
```

### ?? **Database Update Template**
```sql
-- Phase X Database Update - Execute in DB Browser
-- Enable foreign key constraints
PRAGMA foreign_keys = ON;

-- Add columns individually (safer than migrations)
ALTER TABLE TableName ADD COLUMN NewColumn TEXT;
ALTER TABLE TableName ADD COLUMN AnotherColumn INTEGER DEFAULT 0;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS "IX_TableName_NewColumn" ON "TableName" ("NewColumn");

-- Verification queries
SELECT name FROM sqlite_master WHERE type='table' AND name='TableName';
PRAGMA table_info(TableName);
PRAGMA foreign_key_check;
PRAGMA integrity_check;
```

### ?? **Individual SQLite Commands Protocol**
```powershell
# Check table structure before changes
sqlite3 scheduler.db "PRAGMA table_info(TableName);"

# Add columns one at a time
sqlite3 scheduler.db "ALTER TABLE TableName ADD COLUMN NewColumn TEXT;"

# Verify changes
sqlite3 scheduler.db "PRAGMA table_info(TableName);"
sqlite3 scheduler.db "PRAGMA integrity_check;"
```

---

## ?? **TESTING & VALIDATION PROTOCOLS**

### ? **Build Validation Sequence**
```powershell
# Required sequence for every change
cd OpCentrix  # Navigate to correct directory first
dotnet clean
dotnet restore
dotnet build OpCentrix.csproj

# Check for compilation errors
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build Success"
} else {
    Write-Host "? Build Failed - Review errors"
}
```

### ?? **Database Integrity Checks**
```powershell
# After any database modification
sqlite3 scheduler.db "PRAGMA integrity_check;"
sqlite3 scheduler.db "PRAGMA foreign_key_check;"

# Verify specific table structure
sqlite3 scheduler.db "PRAGMA table_info(TableName);"
sqlite3 scheduler.db "SELECT COUNT(*) FROM TableName;"
```

### ?? **Service Integration Testing**
```powershell
# Verify service registration
Get-Content "OpCentrix/Program.cs" | Select-String "ServiceName"

# Check service dependencies
Get-Content "OpCentrix/Services/ServiceName.cs" | Select-String "constructor\|DI\|inject" -Context 3
```

---

## ?? **CRITICAL "NEVER DO" LIST**

### ? **PowerShell Don'ts**
- **NEVER use `&&` operators** in PowerShell commands
- **NEVER use `dotnet run`** for testing (freezes AI)
- **NEVER chain commands** with semicolons in single line
- **NEVER assume PowerShell = Bash** syntax

### ? **Database Don'ts**
- **NEVER use EF migrations** on existing production databases
- **NEVER modify database** without backup first
- **NEVER skip integrity checks** after database changes
- **NEVER use multi-line SQL** in PowerShell commands

### ? **File Editing Don'ts**
- **NEVER edit files** without reading them first with get_file
- **NEVER show code blocks** instead of using edit_file tool
- **NEVER make assumptions** about file contents
- **NEVER skip error validation** after file changes

### ? **Implementation Don'ts**
- **NEVER break existing functionality** - extend, don't replace
- **NEVER skip research phase** before implementing
- **NEVER proceed to next phase** without completing current phase
- **NEVER ignore build errors** or warnings

---

## ? **CRITICAL "ALWAYS DO" LIST**

### ? **Research & Planning Always**
- **ALWAYS use text_search** before implementing features
- **ALWAYS read existing files** with get_file before editing
- **ALWAYS understand current architecture** before changes
- **ALWAYS check for existing similar functionality**

### ? **Database Always**
- **ALWAYS backup database** before any schema changes
- **ALWAYS use individual SQLite commands** for modifications
- **ALWAYS verify database integrity** after changes
- **ALWAYS include verification queries** in SQL scripts

### ? **Development Always**
- **ALWAYS validate with dotnet build** after changes
- **ALWAYS use get_errors tool** to check compilation
- **ALWAYS preserve existing functionality** when extending
- **ALWAYS document deviations** from original plan

### ? **Documentation Always**
- **ALWAYS update progress** in master plan documents
- **ALWAYS create phase completion documentation**
- **ALWAYS document lessons learned** for future reference
- **ALWAYS mark success criteria** as completed

---

## ?? **PHASE COMPLETION PROTOCOL**

### ?? **After Completing Each Phase**
```markdown
**MANDATORY STEPS:**

1. **Update Master Plan Document**:
   - Mark phase as ? COMPLETED with date
   - Document actual duration vs estimated
   - Note any deviations from original plan
   - Update all success criteria checkboxes

2. **Create Phase-Specific Documentation**:
   - PARTS_STAGE_ASSIGNMENT_FIXED.md (Phase 1)
   - ENHANCED_BUILD_TIME_TRACKING.md (Phase 2)
   - AUTOMATED_STAGE_PROGRESSION_COMPLETE.md (Phase 3)
   - ENHANCED_OPERATOR_INTERFACE_COMPLETE.md (Phase 4)
   - BUILD_TIME_LEARNING_SYSTEM_COMPLETE.md (Phase 5)

3. **Validate Next Phase Prerequisites**:
   - Confirm all dependencies are met
   - Update risk assessments based on experience
   - Provide explicit "READY FOR NEXT PHASE" confirmation

4. **Document Implementation Lessons**:
   - What worked differently than planned
   - Database modification approaches that succeeded/failed
   - PowerShell command limitations discovered
   - Best practices for future implementations
```

### ?? **Never Proceed Without Progress Update**
```markdown
**CRITICAL: NEVER proceed to next phase without:**
- ? Updating master plan with completion status
- ? Creating phase-specific documentation
- ? Marking all success criteria
- ? Providing explicit phase completion confirmation
```

---

## ??? **ARCHITECTURE AWARENESS**

### ?? **Current System Knowledge**
```markdown
**OpCentrix is a Razor Pages .NET 8 MES with:**
- SQLite database with 30+ tables
- Complete authentication system (cookie-based)
- Full CRUD admin interfaces for all entities
- Advanced scheduler with multi-zoom and HTMX
- Print tracking with SLS lifecycle management
- Comprehensive analytics and reporting
- Stage-aware manufacturing workflow system

**Key Services:**
- SchedulerService - Job scheduling and conflict detection
- PrintTrackingService - Print job lifecycle management
- PartStageService - Manufacturing stage requirements
- AdminDashboardService - System statistics and metrics
- BuildTimeAnalyticsService - ML-powered analytics

**Database Context:**
- SchedulerContext - Main EF Core context
- 30+ models with complex relationships
- Optimized with indexes and foreign key constraints
```

### ??? **UI Framework Knowledge**
```markdown
**Frontend Stack:**
- Bootstrap 5 for styling
- HTMX for dynamic interactions
- Chart.js for analytics visualization
- Custom CSS for manufacturing-specific components
- Responsive design for mobile compatibility

**Modal System:**
- Consistent modal patterns across all features
- HTMX-powered dynamic loading
- Form validation with server-side integration
- Loading indicators and error handling
```

---

## ?? **TROUBLESHOOTING QUICK REFERENCE**

### ?? **Common Issues & Solutions**

#### **Build Failures**
```powershell
# If build fails after changes:
dotnet clean
dotnet restore
dotnet build OpCentrix.csproj

# Check specific errors:
dotnet build OpCentrix.csproj --verbosity detailed
```

#### **Database Issues**
```powershell
# If database corruption suspected:
sqlite3 scheduler.db "PRAGMA integrity_check;"

# Restore from backup if needed:
$latestBackup = Get-ChildItem "../backup/database/" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $latestBackup.FullName "scheduler.db" -Force
```

#### **Service Registration Issues**
```powershell
# Check service registration in Program.cs:
Get-Content "Program.cs" | Select-String "AddScoped\|AddSingleton\|AddTransient"

# Verify service dependencies:
Get-Content "Services/ServiceName.cs" | Select-String "constructor"
```

#### **HTMX Integration Issues**
```markdown
**Common HTMX problems:**
- Missing hx-target attributes
- Incorrect partial view returns
- JavaScript conflicts with dynamic content
- Modal state management issues

**Solutions:**
- Verify HTMX attributes in HTML
- Ensure controllers return appropriate partial views
- Use proper hx-swap strategies
- Implement modal reset functionality
```

---

## ?? **PROMPT TEMPLATES FOR COMMON TASKS**

### ?? **Research Phase Prompt**
```markdown
"I need to understand the current implementation of [FEATURE] in the OpCentrix MES system. Please:
1. Search for existing [FEATURE] functionality using text_search
2. Read relevant files with get_file to understand current state
3. Identify any existing patterns or conventions
4. Provide a summary of what's already implemented
5. Identify gaps or issues that need to be addressed"
```

### ??? **Database Modification Prompt**
```markdown
"I need to modify the database schema for OpCentrix. Please:
1. First backup the database using the mandatory backup protocol
2. Research existing table structure with sqlite3 commands
3. Provide individual ALTER TABLE statements (NOT EF migrations)
4. Include verification queries to confirm changes
5. Test database integrity after modifications
6. Document the changes in the appropriate reference files"
```

### ??? **Service Implementation Prompt**
```markdown
"I need to implement/modify a service in OpCentrix. Please:
1. Research existing service patterns and architecture
2. Read current implementation with get_file if modifying existing
3. Follow established patterns for dependency injection
4. Implement with proper error handling and logging
5. Register service in Program.cs if new
6. Test integration with dotnet build validation"
```

### ?? **UI Enhancement Prompt**
```markdown
"I need to enhance the UI for [FEATURE] in OpCentrix. Please:
1. Research current UI patterns and modal implementations
2. Read existing page and partial view files
3. Follow Bootstrap 5 and HTMX patterns consistently
4. Ensure responsive design for mobile compatibility
5. Implement proper loading indicators and error handling
6. Test with build validation and manual verification"
```

---

## ?? **REFERENCE QUICK LINKS**

### ?? **Key File Locations**
- **Models**: `OpCentrix/Models/`
- **Services**: `OpCentrix/Services/`
- **Pages**: `OpCentrix/Pages/`
- **Database**: `OpCentrix/scheduler.db`
- **Context**: `OpCentrix/Data/SchedulerContext.cs`
- **Startup**: `OpCentrix/Program.cs`

### ?? **Essential Commands**
```powershell
# Project navigation
cd OpCentrix

# Build validation
dotnet clean && dotnet restore && dotnet build

# Database integrity
sqlite3 scheduler.db "PRAGMA integrity_check;"

# Service verification
Get-Content "Program.cs" | Select-String "ServiceName"
```

### ?? **Documentation Structure**
- **Implementation Plans**: Root level markdown files
- **Feature Docs**: `OpCentrix/Documentation/03-Feature-Implementation/`
- **Architecture**: `OpCentrix/Documentation/02-System-Architecture/`
- **Testing**: `OpCentrix/Documentation/05-Testing-Quality/`

---

## ??? **SUCCESS INDICATORS**

### ? **Phase Completion Criteria**
- All success criteria checkboxes marked complete
- dotnet build succeeds with no errors
- Database integrity checks pass
- All existing functionality preserved
- Phase-specific documentation created
- Master plan document updated

### ?? **Quality Benchmarks**
- Zero breaking changes to existing functionality
- Consistent coding patterns followed
- Proper error handling and logging implemented
- Responsive UI with proper loading indicators
- Database relationships and constraints maintained

---

**?? Remember: This is a production system with users. Always preserve existing functionality while extending capabilities. When in doubt, research first, implement carefully, and validate thoroughly.**

---

*Last Updated: January 30, 2025*  
*Version: 2.0 - Comprehensive AI Assistant Instructions*  
*Status: ? Ready for Implementation Use*