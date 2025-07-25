# AI CODING INSTRUCTIONS - NO UNICODE CHARACTERS

## CRITICAL RULE: WINDOWS COMMAND PROMPT COMPATIBILITY

**NEVER USE UNICODE EMOJI CHARACTERS OR SPECIAL SYMBOLS IN ANY FILES**

This project must be compatible with Windows Command Prompt, which cannot handle Unicode emoji characters properly. This causes encoding errors and prevents files from being saved or executed correctly.

---

## BANNED CHARACTERS

### Absolutely NEVER use these characters:
- ?? (magnifying glass)
- ? (check mark)
- ? (cross mark)  
- ?? (warning sign)
- ?? (clipboard)
- ?? (floppy disk)
- ?? (rocket)
- ?? (wrench)
- ?? (bar chart)
- ? (plus sign)
- ?? (pencil)
- ??? (trash)
- ?? (target)
- ?? (memo)
- ?? (arrows)
- Any other Unicode emoji or special symbols

### ALWAYS use ASCII alternatives instead:
- `[SEARCH]` or `FIND:` instead of ??
- `SUCCESS:` or `PASS:` instead of ?
- `ERROR:` or `FAIL:` instead of ?
- `WARNING:` or `WARN:` instead of ??
- `INFO:` or `NOTE:` instead of ??
- `SAVE:` instead of ??
- `START:` instead of ??
- `FIX:` instead of ??
- `DATA:` instead of ??
- `ADD:` instead of ?
- `EDIT:` instead of ??
- `DELETE:` instead of ???
- `TARGET:` instead of ??
- `LOG:` instead of ??
- `RELOAD:` instead of ??

---

## FILE TYPES THAT MUST BE UNICODE-FREE

### Batch Files (.bat)
- Windows Command Prompt cannot handle Unicode
- Use only ASCII characters (A-Z, a-z, 0-9, basic punctuation)
- Test all batch files in Windows Command Prompt before completion

### Shell Scripts (.sh)
- While Linux/Mac can handle Unicode, keep consistent with Windows
- Use ASCII alternatives for cross-platform compatibility
- Test scripts in basic terminal environments

### C# Code Files (.cs)
- Use only ASCII in log messages and comments
- Unicode in strings causes encoding issues in some environments
- Keep all developer comments in ASCII

### Markdown Files (.md)
- Use ASCII alternatives for status indicators
- GitHub displays Unicode correctly, but local tools may not
- Ensure compatibility with all Markdown viewers

### JSON/Config Files
- Never use Unicode in configuration values
- ASCII-only for all settings and messages

---

## ACCEPTABLE PRACTICES

### For Status Indicators:
```
GOOD:
SUCCESS: Database connection established
ERROR: Failed to connect to database
WARNING: Low disk space detected
INFO: Processing 100 records

BAD:
? Database connection established
? Failed to connect to database  
?? Low disk space detected
?? Processing 100 records
```

### For Progress Indicators:
```
GOOD:
[1/5] Checking prerequisites
[2/5] Restoring packages
[3/5] Building project
[4/5] Running tests
[5/5] Complete

BAD:
?? Checking prerequisites
?? Restoring packages
?? Building project
?? Running tests
? Complete
```

### For Categorization:
```
GOOD:
[DATABASE] Connection established
[NETWORK] API endpoint responding
[SECURITY] Authentication successful
[PERFORMANCE] Query completed in 50ms

BAD:
?? Connection established
?? API endpoint responding
?? Authentication successful
? Query completed in 50ms
```

---

## TESTING REQUIREMENTS

### Before Saving Any File:
1. **Batch Files**: Test in Windows Command Prompt
2. **Shell Scripts**: Test in basic bash terminal
3. **All Files**: Check for any Unicode characters using text editor
4. **Encoding**: Ensure files are saved as UTF-8 or ASCII

### Validation Commands:
```bash
# Check for Unicode characters in files
grep -P "[^\x00-\x7F]" filename.bat
grep -P "[^\x00-\x7F]" filename.sh

# If any output appears, the file contains Unicode characters and must be fixed
```

---

## ERROR PREVENTION

### When Writing Log Messages:
```csharp
GOOD:
_logger.LogInformation("SUCCESS: Part {PartNumber} created", part.PartNumber);
_logger.LogError("ERROR: Database connection failed");
_logger.LogWarning("WARNING: Validation failed for {Field}", fieldName);

BAD:
_logger.LogInformation("? Part {PartNumber} created", part.PartNumber);
_logger.LogError("? Database connection failed");
_logger.LogWarning("?? Validation failed for {Field}", fieldName);
```

### When Writing Comments:
```csharp
GOOD:
// CRITICAL: This method must validate input
// TODO: Add error handling for network timeouts
// NOTE: Performance optimization needed here

BAD:
// ?? CRITICAL: This method must validate input
// ?? TODO: Add error handling for network timeouts
// ?? NOTE: Performance optimization needed here
```

---

## IMMEDIATE ACTION REQUIRED

If any file contains Unicode characters:
1. **Stop immediately**
2. **Replace all Unicode characters with ASCII alternatives**
3. **Test the file in Windows Command Prompt**
4. **Verify the file saves and runs correctly**
5. **Only then proceed with other work**

---

## COMPLIANCE CHECK

Before completing any task, verify:
- [ ] No Unicode emoji characters in any files
- [ ] All batch files tested in Windows Command Prompt
- [ ] All scripts use ASCII-only characters
- [ ] Log messages use text-based status indicators
- [ ] Comments and documentation are ASCII-only
- [ ] Files save and execute without encoding errors

This ensures the project works correctly on any Windows PC with standard Command Prompt.