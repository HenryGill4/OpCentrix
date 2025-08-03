# ?? OpCentrix Embedded JavaScript & CSS Cleanup TODO

## ?? **OVERVIEW**

This document lists all embedded JavaScript and CSS found throughout the OpCentrix application that should be moved to external, organized files. The cleanup will improve maintainability, performance, and code organization.

**Total Files to Refactor**: 15+ files
**Estimated Cleanup Time**: 2-3 days (doing a few at a time)

---

## ?? **PRIORITY 1: CRITICAL PAGES (Start Here)**

### 1. **Pages/Shared/_Layout.cshtml** ?????
**?? HIGH PRIORITY - Main layout affects entire application**

**Embedded JavaScript Issues:**
- [ ] 120+ lines of session management code in `<script>` tag
- [ ] Mobile menu toggle functionality 
- [ ] Complex timeout warning system with intervals
- [ ] Activity tracking with event listeners
- [ ] Logout confirmation logic

**Move to:**
- `wwwroot/js/layout/session-manager.js` (session timeout logic)
- `wwwroot/js/layout/mobile-menu.js` (mobile navigation)
- `wwwroot/js/layout/activity-tracker.js` (user activity tracking)

**CSS Issues:**
- [ ] Tailwind CSS configuration object embedded in `<script>`
- [ ] Custom CSS for `.opcentrix-gradient` and other utilities

**Move to:**
- `wwwroot/css/layout/tailwind-config.js`
- `wwwroot/css/layout/opcentrix-utilities.css`

### 2. **Pages/Admin/Parts.cshtml** ?????
**?? HIGH PRIORITY - 400+ lines of embedded JavaScript**

**Embedded JavaScript Issues:**
- [ ] 400+ lines of parts management JavaScript in `<script>` tag
- [ ] Global function definitions (`handleAddPartClick`, `handleEditPartClick`, etc.)
- [ ] AJAX form submission logic
- [ ] Modal management functions
- [ ] Material defaults configuration object
- [ ] Form validation and error handling
- [ ] Bootstrap tooltip initialization
- [ ] Toast notification system

**Move to:**
- `wwwroot/js/admin/parts-manager.js` (main functionality)
- `wwwroot/js/admin/parts-modal.js` (modal handling)
- `wwwroot/js/admin/parts-validation.js` (form validation)
- `wwwroot/js/admin/material-defaults.js` (material configuration)
- `wwwroot/js/shared/toast-notifications.js` (reusable toasts)

**CSS Issues:**
- [ ] Parts-specific styling embedded in `<style>` tags
- [ ] Custom grid layouts and responsive design
- [ ] Status badge styling

**Move to:**
- `wwwroot/css/admin/parts.css`

### 3. **Pages/EDM.cshtml** ????
**?? HIGH PRIORITY - 600+ lines of embedded JavaScript**

**Embedded JavaScript Issues:**
- [ ] 600+ lines of EDM operations JavaScript
- [ ] `SafeExecute` global wrapper system
- [ ] Form management and validation
- [ ] Print section population logic
- [ ] Database save/update operations
- [ ] Modal management for stored logs
- [ ] Auto-formatting for input fields
- [ ] Error logging and notification system

**Move to:**
- `wwwroot/js/edm/safe-execute.js` (error handling wrapper)
- `wwwroot/js/edm/form-manager.js` (form operations)
- `wwwroot/js/edm/print-manager.js` (printing functionality)
- `wwwroot/js/edm/log-manager.js` (stored logs CRUD)
- `wwwroot/js/shared/error-handler.js` (reusable error handling)

**CSS Issues:**
- [ ] Print-specific CSS embedded in `<style>` tag
- [ ] Media queries for print layout
- [ ] Form styling and responsive grid

**Move to:**
- `wwwroot/css/edm/print-layout.css`
- `wwwroot/css/edm/form-styling.css`

### 4. **Pages/Admin/BugReports.cshtml** ????
**?? HIGH PRIORITY - Bug reporting system**

**Embedded JavaScript Issues:**
- [ ] Bug details modal functions (`viewBugDetails`, `closeBugDetailsModal`)
- [ ] Dynamic content rendering (`renderBugDetails`)
- [ ] Helper functions for CSS classes and icons
- [ ] ESC key event handling

**Move to:**
- `wwwroot/js/admin/bug-reports.js`
- `wwwroot/js/shared/modal-helpers.js`

**CSS Issues:**
- [ ] Inline Tailwind classes could be organized better
- [ ] Custom badge styling patterns

**Move to:**
- `wwwroot/css/admin/bug-reports.css`

---

## ?? **PRIORITY 2: ADMIN PAGES**

### 5. **Pages/Admin/Shared/_AdminLayout.cshtml** ???
**?? MEDIUM PRIORITY - Admin navigation**

**Embedded JavaScript Issues:**
- [ ] Sidebar toggle functionality in `<script>` tag
- [ ] Mobile menu handling

**Move to:**
- `wwwroot/js/admin/admin-layout.js`

**CSS Issues:**
- [ ] Navigation active state styling
- [ ] Responsive sidebar behavior

**Move to:**
- `wwwroot/css/admin/admin-layout.css`

### 6. **Pages/Shared/_BugReportTool.cshtml** ????
**?? MEDIUM PRIORITY - 800+ lines of JavaScript**

**Embedded JavaScript Issues:**
- [ ] 800+ lines of bug reporting JavaScript
- [ ] Global `BugReportTool` object with multiple functions
- [ ] Form submission and validation
- [ ] Modal management (add/edit/delete)
- [ ] Statistics rendering
- [ ] Browser detection utilities
- [ ] Error capture and logging

**Move to:**
- `wwwroot/js/shared/bug-report/bug-report-tool.js`
- `wwwroot/js/shared/bug-report/bug-modal-manager.js`
- `wwwroot/js/shared/bug-report/bug-statistics.js`
- `wwwroot/js/shared/utilities/browser-detection.js`

**CSS Issues:**
- [ ] Extensive styling embedded in `<style>` tag
- [ ] Custom modal styling
- [ ] Responsive design utilities
- [ ] Animation and transition classes

**Move to:**
- `wwwroot/css/shared/bug-report-tool.css`
- `wwwroot/css/shared/modal-base.css`

---

## ?? **PRIORITY 3: OPERATIONAL PAGES**

### 7. **Pages/Coating.cshtml** ???
**?? MEDIUM PRIORITY - Similar pattern to EDM**

**Expected JavaScript Issues:**
- [ ] Likely similar to EDM.cshtml with coating-specific operations
- [ ] Form management and validation
- [ ] Print functionality

**Investigation needed** - Check file for embedded JavaScript patterns

**Move to:**
- `wwwroot/js/coating/` (similar structure to EDM)

### 8. **Pages/Scheduler/Index.cshtml** ???
**?? MEDIUM PRIORITY - Complex scheduling interface**

**Embedded CSS Issues:**
- [ ] Extensive scheduler styling in `<style>` tag
- [ ] CSS custom properties for slot widths
- [ ] Orientation-specific styles (vertical/horizontal)
- [ ] Job block styling with material-based coloring
- [ ] Grid container responsive behavior

**Move to:**
- `wwwroot/css/scheduler/scheduler-grid.css`
- `wwwroot/css/scheduler/job-blocks.css`
- `wwwroot/css/scheduler/orientation-styles.css`

### 9. **Pages/Admin/Machines.cshtml** ??
**?? MEDIUM PRIORITY**

**Investigation needed** - Likely contains:
- [ ] Machine management JavaScript
- [ ] Modal handling for add/edit operations
- [ ] Form validation

### 10. **Pages/Scheduler/JobLog.cshtml** ??
**?? MEDIUM PRIORITY**

**Investigation needed** - Check for:
- [ ] Job log filtering and display JavaScript
- [ ] Export functionality

---

## ?? **PRIORITY 4: SUPPORTING PAGES**

### 11. **Pages/Index.cshtml** (Dashboard) ??
**?? LOW PRIORITY**

**Investigation needed** - Check for:
- [ ] Dashboard widget JavaScript
- [ ] Chart or graph initialization

### 12. **Pages/PrintTracking/Index.cshtml** ??
**?? LOW PRIORITY**

**Investigation needed** - Check for:
- [ ] Print tracking functionality
- [ ] Status update operations

### 13. **Pages/Shipping/Index.cshtml** ?
**?? LOW PRIORITY**

**Investigation needed** - Check for embedded scripts

### 14. **Pages/Analytics.cshtml** ?
**?? LOW PRIORITY**

**Investigation needed** - Likely contains:
- [ ] Chart.js or similar charting library initialization
- [ ] Data visualization JavaScript

### 15. **Pages/QC/Index.cshtml** ?
**?? LOW PRIORITY**

**Investigation needed** - Check for QC-specific functionality

---

## ??? **PROPOSED FILE ORGANIZATION STRUCTURE**

```
wwwroot/
??? js/
?   ??? shared/
?   ?   ??? error-handler.js
?   ?   ??? toast-notifications.js
?   ?   ??? modal-helpers.js
?   ?   ??? form-validation.js
?   ?   ??? utilities/
?   ?   ?   ??? browser-detection.js
?   ?   ?   ??? date-helpers.js
?   ?   ??? bug-report/
?   ?       ??? bug-report-tool.js
?   ?       ??? bug-modal-manager.js
?   ?       ??? bug-statistics.js
?   ??? layout/
?   ?   ??? session-manager.js
?   ?   ??? mobile-menu.js
?   ?   ??? activity-tracker.js
?   ??? admin/
?   ?   ??? admin-layout.js
?   ?   ??? parts-manager.js
?   ?   ??? parts-modal.js
?   ?   ??? parts-validation.js
?   ?   ??? material-defaults.js
?   ?   ??? bug-reports.js
?   ?   ??? machines-manager.js
?   ??? edm/
?   ?   ??? safe-execute.js
?   ?   ??? form-manager.js
?   ?   ??? print-manager.js
?   ?   ??? log-manager.js
?   ??? coating/
?   ?   ??? (similar to EDM structure)
?   ??? scheduler/
?   ?   ??? scheduler-grid.js
?   ?   ??? job-manager.js
?   ?   ??? orientation-toggle.js
?   ??? modules/
?       ??? print-tracking.js
?       ??? analytics.js
?       ??? quality-control.js
??? css/
?   ??? shared/
?   ?   ??? modal-base.css
?   ?   ??? form-base.css
?   ?   ??? toast-notifications.css
?   ?   ??? bug-report-tool.css
?   ??? layout/
?   ?   ??? opcentrix-utilities.css
?   ?   ??? responsive-helpers.css
?   ??? admin/
?   ?   ??? admin-layout.css
?   ?   ??? parts.css
?   ?   ??? bug-reports.css
?   ??? edm/
?   ?   ??? print-layout.css
?   ?   ??? form-styling.css
?   ??? coating/
?   ?   ??? (similar to EDM)
?   ??? scheduler/
?       ??? scheduler-grid.css
?       ??? job-blocks.css
?       ??? orientation-styles.css
```

---

## ? **REFACTORING STRATEGY**

### **Phase 1: Critical Infrastructure (Week 1)**
1. Start with `_Layout.cshtml` - session management is used everywhere
2. Extract shared utilities (`error-handler.js`, `toast-notifications.js`)
3. Create base modal and form helper classes

### **Phase 2: Admin Pages (Week 2)**
1. Refactor `Parts.cshtml` - most complex admin page
2. Clean up `BugReports.cshtml` and `_BugReportTool.cshtml`
3. Standardize admin layout JavaScript

### **Phase 3: Operational Pages (Week 3)**
1. Refactor `EDM.cshtml` - largest JavaScript file
2. Apply same patterns to `Coating.cshtml`
3. Clean up scheduler CSS organization

### **Phase 4: Polish and Testing (Week 4)**
1. Test all functionality after refactoring
2. Optimize bundling and minification
3. Add proper TypeScript definitions if desired

---

## ?? **IMPLEMENTATION GUIDELINES**

### **JavaScript Organization**
- Use ES6 modules where possible
- Create reusable base classes
- Implement proper error handling
- Add JSDoc comments for documentation
- Use consistent naming conventions

### **CSS Organization**
- Follow BEM methodology for class names
- Use CSS custom properties for theming
- Implement mobile-first responsive design
- Create utility classes for common patterns
- Minimize specificity conflicts

### **Testing Checklist**
For each refactored page:
- [ ] All buttons and links work
- [ ] Forms submit correctly
- [ ] Modals open and close properly
- [ ] Error messages display correctly
- [ ] Mobile responsiveness maintained
- [ ] Print functionality works (if applicable)
- [ ] Browser compatibility maintained

---

## ?? **PROGRESS TRACKING**

### **Completed Files**
- [ ] None yet - ready to start!

### **In Progress**
- [ ] None currently

### **Testing Queue**
- [ ] None yet

### **Completed and Verified**
- [ ] None yet

---

## ?? **BENEFITS AFTER COMPLETION**

1. **Maintainability**: Code organized by feature and responsibility
2. **Performance**: Proper caching and minification of external files
3. **Debugging**: Easier to debug with separate, organized files
4. **Reusability**: Shared components can be reused across pages
5. **Collaboration**: Multiple developers can work on different modules
6. **Testing**: Individual modules can be unit tested
7. **CDN Ready**: External files can be served via CDN for better performance

---

## ?? **READY TO START!**

**Recommended Daily Goals:**
- **Day 1-2**: Layout and shared utilities (Priority 1 items 1-2)
- **Day 3-4**: Parts management page (Priority 1 item 2)
- **Day 5-6**: EDM operations page (Priority 1 item 3)
- **Day 7-8**: Bug reporting system (Priority 1 item 4)
- **Continue**: Admin pages and operational pages as time allows

This cleanup will significantly improve the codebase organization and maintainability! ??