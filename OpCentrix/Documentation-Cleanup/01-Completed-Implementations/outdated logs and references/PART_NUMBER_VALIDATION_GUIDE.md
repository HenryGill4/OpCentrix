# Part Number Validation Configuration Guide

## Overview

The OpCentrix Parts system now supports **configurable part number validation** through the Admin Settings interface. Administrators can customize the validation pattern, example format, and error messages without code changes.

---

## ?? **How to Configure Part Number Validation**

### **Step 1: Access Admin Settings**
1. Login as admin
2. Navigate to **Admin** ? **Settings**
3. Look for the **Parts** category

### **Step 2: Configure Validation Settings**
The system provides three configurable settings:

| Setting Key | Description | Default Value |
|-------------|-------------|---------------|
| `parts.validation_pattern` | Regular expression pattern for validation | `^\d{2}-\d{4}$` |
| `parts.format_example` | Example to show users | `14-5396` |
| `parts.validation_message` | Error message when validation fails | `Part number must be in format XX-XXXX` |

### **Step 3: Update Settings**
1. **Validation Pattern**: Enter a regular expression pattern
2. **Format Example**: Provide an example that matches your pattern
3. **Validation Message**: Customize the error message
4. Click **Save All Changes**

---

## ?? **Common Validation Patterns**

### **Current Default (XX-XXXX)**
```regex
^\d{2}-\d{4}$
```
- **Matches**: `14-5396`, `99-1234`
- **Example**: `14-5396`
- **Message**: `Part number must be in format XX-XXXX`

### **Company Prefix + Number (COMP-XXXXX)**
```regex
^COMP-\d{5}$
```
- **Matches**: `COMP-12345`, `COMP-98765`
- **Example**: `COMP-12345`
- **Message**: `Part number must be in format COMP-XXXXX`

### **Flexible Format (Letters + Numbers)**
```regex
^[A-Z]{2,4}-\d{3,6}$
```
- **Matches**: `ABC-123`, `PART-123456`, `XY-999`
- **Example**: `PART-12345`
- **Message**: `Part number must be 2-4 letters, dash, then 3-6 numbers`

### **Department + Sequential (DEP-XXX-YYYY)**
```regex
^[A-Z]{3}-\d{3}-\d{4}$
```
- **Matches**: `ENG-001-2024`, `MFG-999-1234`
- **Example**: `ENG-001-2024`
- **Message**: `Part number must be in format DEP-XXX-YYYY`

### **Year-Based Numbering (YYYY-XXXXX)**
```regex
^20\d{2}-\d{5}$
```
- **Matches**: `2024-12345`, `2025-00001`
- **Example**: `2024-12345`
- **Message**: `Part number must be in format YYYY-XXXXX`

### **Alphanumeric Free-Form**
```regex
^[A-Z0-9]{6,12}$
```
- **Matches**: `ABC123`, `PART001`, `XYZ999ABC`
- **Example**: `PART12345`
- **Message**: `Part number must be 6-12 characters (letters and numbers only)`

---

## ? **Real-Time Implementation**

### **Automatic Updates**
- Changes take effect **immediately** after saving
- No application restart required
- All new part forms use the updated validation
- Existing parts are not affected unless edited

### **Client-Side Validation**
The system automatically:
- ? Downloads new patterns when forms load
- ? Updates placeholder text with examples
- ? Shows real-time validation feedback
- ? Displays custom error messages

### **Server-Side Validation**
- ? Validates against current pattern in database
- ? Provides detailed error messages
- ? Handles invalid regex patterns gracefully
- ? Falls back to defaults if settings are corrupted

---

## ?? **Testing Your Pattern**

### **Method 1: Online Regex Tester**
1. Go to [regex101.com](https://regex101.com)
2. Enter your pattern
3. Test with sample part numbers
4. Verify it matches what you expect

### **Method 2: In OpCentrix**
1. Update the settings
2. Go to **Admin** ? **Parts**
3. Click **Add New Part**
4. Try entering different part numbers
5. Verify validation works as expected

---

## ??? **Advanced Configuration Examples**

### **Multi-Format Support (OR Logic)**
```regex
^(\d{2}-\d{4}|[A-Z]{3}\d{3}|PART-\d{5})$
```
- **Matches**: `14-5396` OR `ABC123` OR `PART-12345`
- **Example**: `14-5396 or ABC123 or PART-12345`
- **Message**: `Use format XX-XXXX, XXXNNN, or PART-NNNNN`

### **Optional Prefixes**
```regex
^(PROJ-)?[A-Z]{2}\d{4}$
```
- **Matches**: `AB1234` or `PROJ-AB1234`
- **Example**: `AB1234 or PROJ-AB1234`
- **Message**: `Format: AB1234 (PROJ- prefix optional)`

### **Case-Insensitive Matching**
```regex
^(?i)[a-z]{2}-\d{4}$
```
- **Matches**: `ab-1234`, `AB-1234`, `Ab-1234`
- **Example**: `AB-1234`
- **Message**: `Two letters, dash, four numbers (case insensitive)`

---

## ?? **Important Notes**

### **Pattern Safety**
- Test patterns thoroughly before deploying
- Invalid regex patterns will cause validation to fail gracefully
- System falls back to default pattern if current pattern is invalid
- Use online regex testers to verify patterns

### **User Experience**
- Provide clear examples that match your pattern
- Write helpful error messages
- Avoid overly complex patterns that confuse users
- Consider backward compatibility with existing part numbers

### **Performance**
- Complex regex patterns may slow down validation
- Keep patterns as simple as possible
- Test with large datasets if you have many parts

---

## ?? **Quick Start: Changing to Your Company Format**

### **Example: Changing to "ACME-XXXXX" Format**

1. **Access Settings**: Admin ? Settings ? Parts category

2. **Update Pattern**:
   ```regex
   ^ACME-\d{5}$
   ```

3. **Update Example**:
   ```
   ACME-12345
   ```

4. **Update Message**:
   ```
   Part number must be in format ACME-XXXXX
   ```

5. **Save Changes** and test immediately

### **Result**
- ? Users see: "Part number must be in format ACME-XXXXX (e.g., ACME-12345)"
- ? Only accepts: `ACME-12345`, `ACME-99999`, etc.
- ? Rejects: `14-5396`, `ACME-123`, `acme-12345`

---

## ?? **Troubleshooting**

### **Pattern Not Working**
- Check regex syntax at [regex101.com](https://regex101.com)
- Ensure pattern starts with `^` and ends with `$`
- Test with your expected part numbers
- Check application logs for regex errors

### **Validation Too Strict**
- Review and simplify your pattern
- Add more flexibility (e.g., make parts optional with `?`)
- Test with existing part numbers

### **Users Confused**
- Improve the example to show correct format clearly
- Update the validation message to be more helpful
- Consider training or documentation for users

---

## ?? **Support**

If you need help configuring validation patterns:

1. **Check Application Logs**: Look for validation errors
2. **Test Online**: Use regex101.com to validate patterns
3. **Start Simple**: Begin with basic patterns and increase complexity
4. **Document Changes**: Keep track of what patterns work for your organization

---

*Last Updated: 2025-01-27*  
*Feature Status: ? Available and Production-Ready*