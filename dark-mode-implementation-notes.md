# OpCentrix Dark Mode Implementation Notes

## Overview
The OpCentrix application implements a comprehensive dark mode system that responds to both user preferences and system settings. This document explains how the dark mode functionality works when pages use the _Layout.cshtml navigation.

## Core Dark Mode System

### 1. Theme Detection and Initialization
**Location:** `<script>` block in `<head>` section of _Layout.cshtml

The dark mode system initializes before page render with these key functions:

```javascript
const getTheme = () => {
    const stored = localStorage.getItem('theme');
    if (stored && (stored === 'dark' || stored === 'light')) {
        return stored;
    }
    // Only fall back to system preference if no user preference is stored
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};
```

**Key Features:**
- **User Preference Priority**: Stored user choice takes precedence over system settings
- **System Preference Fallback**: Uses `prefers-color-scheme` media query when no user preference exists
- **Immediate Application**: Theme is applied before DOM renders to prevent flashing

### 2. Theme Application Method
**Class-Based Approach** (NOT media queries):

```javascript
const setTheme = (theme) => {
    localStorage.setItem('theme', theme);
    
    if (theme === 'dark') {
        document.documentElement.classList.add('dark');
        document.documentElement.setAttribute('data-theme', 'dark');
    } else {
        document.documentElement.classList.remove('dark');
        document.documentElement.setAttribute('data-theme', 'light');
    }
    
    // Dispatch custom event for components that need to react to theme changes
    window.dispatchEvent(new CustomEvent('themeChanged', { 
        detail: { theme } 
    }));
};
```

**Why Class-Based:**
- Provides granular control over dark mode styling
- Allows CSS to target `.dark` class specifically
- More reliable than media queries for user-controlled themes
- Enables component-specific dark mode reactions

### 3. Theme Toggle Functionality
**Toggle Button Location:** User info card in sidebar

```javascript
window.toggleTheme = () => {
    const current = getTheme();
    const newTheme = current === 'dark' ? 'light' : 'dark';
    setTheme(newTheme);
    
    // Provide user feedback
    console.log(`Theme switched to ${newTheme} mode`);
};
```

**Additional Theme Functions:**
- `getCurrentTheme()`: Returns current theme state
- `resetToSystemTheme()`: Removes user preference, defaults to system
- `forceTheme(theme)`: Admin/testing function to force specific theme

### 4. System Preference Monitoring
**Enhanced Listener** that respects user choice:

```javascript
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    const userPreference = localStorage.getItem('theme');
    // Only auto-switch if user hasn't explicitly set a preference
    if (!userPreference) {
        setTheme(e.matches ? 'dark' : 'light');
    }
});
```

**Smart Behavior:**
- Auto-follows system changes ONLY when user hasn't manually set a preference
- Preserves user choice when system preferences change
- Prevents unwanted theme switching

## CSS Dark Mode Implementation

### 1. Class-Based Targeting
All dark mode styles use **class-based selectors** (not media queries):

```css
/* Class-based dark mode - NOT media query */
:root.dark .sidebar::-webkit-scrollbar-track,
html.dark .sidebar::-webkit-scrollbar-track {
    background: #1e293b;
}

.dark .nav-item {
    color: #d1d5db;
}
```

### 2. Comprehensive Coverage Strategy
**Multiple Selector Patterns** for maximum compatibility:

```css
:root.dark .bg-white,
html.dark .bg-white,
:root.dark [class*="bg-white"],
html.dark [class*="bg-white"] {
    background-color: #1f2937 !important;
    background: #1f2937 !important;
}
```

**Coverage Types:**
- Direct class selectors (`.bg-white`)
- Attribute selectors (`[class*="bg-white"]`)
- Root element targeting (`:root.dark`, `html.dark`)
- Inline style overrides for dynamic content

### 3. Priority and Override System
**High Specificity** with `!important` for reliable overrides:

```css
:root.dark body {
    background-color: #111827 !important;
}
```

**Why !important is Used:**
- Overcomes Bootstrap and other framework defaults
- Ensures dark mode applies consistently across all components
- Prevents external CSS from breaking dark mode

### 4. Component-Specific Dark Mode

#### Sidebar Navigation
```css
.sidebar {
    transition: transform 0.3s ease-in-out;
}

.dark .nav-item {
    color: #d1d5db;
}

.dark .nav-item:hover {
    background: linear-gradient(135deg, #374151 0%, #4b5563 100%);
    color: #e5e7eb;
}
```

#### User Interface Elements
```css
.dark .user-info-card {
    background: linear-gradient(135deg, #374151 0%, #4b5563 100%);
    border-bottom: 1px solid #4b5563;
}
```

#### Task Badges and Notifications
```css
.task-badge-critical {
    background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%) !important;
    color: white !important;
    animation: pulse-critical 1.5s ease-in-out infinite;
}
```

### 5. Scrollbar Customization
**Dark-aware scrollbars** throughout the application:

```css
:root.dark ::-webkit-scrollbar-track {
    background: #374151 !important;
}

:root.dark ::-webkit-scrollbar-thumb {
    background: #6b7280 !important;
}
```

## Theme Integration Points

### 1. Layout Body Classes
**Responsive theme classes** applied to body:

```html
<body class="bg-gray-50 dark:bg-gray-900 transition-colors duration-300" 
      data-theme-responsive="true">
```

### 2. Conditional CSS Loading
**Environment-based styling**:

```razor
@if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    <script src="https://cdn.tailwindcss.com"></script>
    <!-- Tailwind config with dark mode support -->
}
else
{
    <link rel="stylesheet" href="~/css/tailwind-output.css" asp-append-version="true" />
}
```

### 3. Theme Toggle Button
**Visual feedback** in user interface:

```html
<button onclick="toggleTheme()" class="theme-toggle-btn">
    <svg class="w-4 h-4 text-gray-600 dark:text-gray-300 dark:hidden">
        <!-- Sun icon for light mode -->
    </svg>
    <svg class="w-4 h-4 text-gray-300 hidden dark:block">
        <!-- Moon icon for dark mode -->
    </svg>
</button>
```

## Advanced Features

### 1. Theme Change Events
**Custom events** for component reactivity:

```javascript
window.dispatchEvent(new CustomEvent('themeChanged', { 
    detail: { theme } 
}));
```

Components can listen for theme changes:
```javascript
window.addEventListener('themeChanged', (e) => {
    console.log(`Theme changed to: ${e.detail.theme}`);
    // Update component-specific styling
});
```

### 2. Debug Mode
**Development debugging** with visual indicators:

```javascript
if (window.location.search.includes('debug=theme')) {
    // Add debug indicator
    const debugIndicator = document.createElement('div');
    debugIndicator.className = 'theme-indicator';
    debugIndicator.textContent = `Theme: ${getCurrentTheme()}`;
    document.body.appendChild(debugIndicator);
    document.body.classList.add('debug-mode');
}
```

### 3. Consistency Monitoring
**Auto-correction** for theme inconsistencies:

```javascript
window.addEventListener('themeChanged', (e) => {
    const htmlHasDark = document.documentElement.classList.contains('dark');
    const expectedDark = e.detail.theme === 'dark';
    
    if (htmlHasDark !== expectedDark) {
        console.warn('?? Theme inconsistency detected! Fixing...');
        // Force correction
        if (expectedDark) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    }
});
```

## How Pages Inherit Dark Mode

### 1. Automatic Inheritance
When pages use the _Layout.cshtml:
- **HTML class inheritance**: `dark` class on `<html>` element affects all child elements
- **CSS cascade**: All dark mode styles automatically apply to page content
- **No page-specific code needed**: Pages inherit dark mode behavior automatically

### 2. Page-Specific Customization
Pages can add custom dark mode styles:

```css
.dark .custom-page-element {
    background-color: #374151;
    color: #e5e7eb;
}
```

### 3. Component Integration
Page components can react to theme changes:

```javascript
// In page-specific JavaScript
window.addEventListener('themeChanged', (e) => {
    updatePageSpecificElements(e.detail.theme);
});
```

## Benefits of This Implementation

### 1. **User Experience**
- Instant theme switching without page reload
- Respects user preference over system default
- Smooth transitions and animations
- Consistent theming across all pages

### 2. **Developer Experience**
- Simple class-based targeting (`css:.dark`)
- Automatic inheritance for new pages
- Debug tools for development
- Event system for custom components

### 3. **Performance**
- No flash of incorrect theme
- Minimal JavaScript overhead
- CSS-only styling transitions
- Efficient localStorage caching

### 4. **Reliability**
- Fallbacks for all scenarios
- Auto-correction for inconsistencies
- High-specificity CSS overrides
- Cross-browser compatibility

## Usage Guidelines

### For New Pages:
1. Use _Layout.cshtml - dark mode works automatically
2. Add page-specific dark styles with `.dark` prefix
3. Test both light and dark modes during development

### For Components:
1. Use `.dark` class selectors for dark mode styles
2. Listen for `themeChanged` events if needed
3. Follow the existing color palette and patterns

### For Debugging:
1. Add `?debug=theme` to URL for debug mode
2. Check console for theme state information
3. Use browser dev tools to inspect theme classes

This implementation provides a robust, user-friendly dark mode system that automatically works for any page using the layout, while providing flexibility for custom styling and component-specific behavior.