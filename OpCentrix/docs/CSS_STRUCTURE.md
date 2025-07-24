# ?? OpCentrix CSS Structure Guide

## ?? Overview

This document provides a comprehensive guide to the OpCentrix CSS architecture, design system, and styling conventions. The CSS structure follows a modern, scalable approach with design tokens, component-based architecture, and responsive design principles.

## ??? CSS Architecture

### **Architecture Philosophy**
- **Design System Approach**: Consistent visual language with reusable components
- **CSS Custom Properties**: Centralized design tokens for maintainability
- **Mobile-First**: Responsive design with progressive enhancement
- **Performance Optimized**: Efficient selectors and minimal CSS bloat
- **Accessibility First**: WCAG AA compliant styling with focus management

### **File Organization**
```
OpCentrix/wwwroot/css/
??? site.css                     # Main stylesheet (complete design system)
??? scheduler-modal.css          # Modal-specific styles
??? [future modules]/            # Department-specific stylesheets
    ??? printing.css
    ??? coating.css
    ??? analytics.css
```

## ?? Design System Foundation

### **1. CSS Custom Properties (Design Tokens)**

#### **Color Palette**
```css
:root {
    /* === PRIMARY COLORS === */
    --opcentrix-primary: #3B82F6;        /* Main brand blue */
    --opcentrix-primary-dark: #1E40AF;   /* Darker blue for hover states */
    --opcentrix-primary-light: #93C5FD;  /* Light blue for backgrounds */
    
    --opcentrix-secondary: #6366F1;      /* Accent indigo */
    --opcentrix-secondary-dark: #4338CA; /* Dark indigo for depth */
    --opcentrix-secondary-light: #A5B4FC; /* Light indigo for highlights */
    
    /* === SEMANTIC COLORS === */
    --opcentrix-success: #10B981;        /* Success green */
    --opcentrix-success-dark: #047857;   /* Dark green for hover */
    --opcentrix-success-light: #A7F3D0;  /* Light green for backgrounds */
    
    --opcentrix-warning: #F59E0B;        /* Warning amber */
    --opcentrix-warning-dark: #D97706;   /* Dark amber for hover */
    --opcentrix-warning-light: #FDE68A;  /* Light amber for backgrounds */
    
    --opcentrix-danger: #EF4444;         /* Error red */
    --opcentrix-danger-dark: #DC2626;    /* Dark red for hover */
    --opcentrix-danger-light: #FCA5A5;   /* Light red for backgrounds */
    
    --opcentrix-info: #06B6D4;           /* Info cyan */
    --opcentrix-info-dark: #0891B2;      /* Dark cyan for hover */
    --opcentrix-info-light: #A5F3FC;     /* Light cyan for backgrounds */
    
    /* === NEUTRAL GRAYS === */
    --opcentrix-gray-50: #F9FAFB;        /* Lightest background */
    --opcentrix-gray-100: #F3F4F6;       /* Light backgrounds */
    --opcentrix-gray-200: #E5E7EB;       /* Borders and dividers */
    --opcentrix-gray-300: #D1D5DB;       /* Disabled states */
    --opcentrix-gray-400: #9CA3AF;       /* Muted text */
    --opcentrix-gray-500: #6B7280;       /* Secondary text */
    --opcentrix-gray-600: #4B5563;       /* Primary text light */
    --opcentrix-gray-700: #374151;       /* Primary text */
    --opcentrix-gray-800: #1F2937;       /* Dark text */
    --opcentrix-gray-900: #111827;       /* Darkest text */
    
    /* === GRADIENTS === */
    --opcentrix-gradient-primary: linear-gradient(135deg, var(--opcentrix-primary) 0%, var(--opcentrix-secondary) 100%);
    --opcentrix-gradient-success: linear-gradient(135deg, var(--opcentrix-success) 0%, var(--opcentrix-info) 100%);
    --opcentrix-gradient-warning: linear-gradient(135deg, var(--opcentrix-warning) 0%, var(--opcentrix-danger) 100%);
}
```

#### **Typography Scale**
```css
:root {
    /* === FONT FAMILIES === */
    --opcentrix-font-primary: 'Inter', system-ui, -apple-system, sans-serif;
    --opcentrix-font-mono: 'JetBrains Mono', 'SF Mono', 'Monaco', 'Inconsolata', monospace;
    
    /* === FONT SIZES === */
    --opcentrix-text-xs: 0.75rem;      /* 12px */
    --opcentrix-text-sm: 0.875rem;     /* 14px */
    --opcentrix-text-base: 1rem;       /* 16px */
    --opcentrix-text-lg: 1.125rem;     /* 18px */
    --opcentrix-text-xl: 1.25rem;      /* 20px */
    --opcentrix-text-2xl: 1.5rem;      /* 24px */
    --opcentrix-text-3xl: 1.875rem;    /* 30px */
    --opcentrix-text-4xl: 2.25rem;     /* 36px */
    
    /* === FONT WEIGHTS === */
    --opcentrix-font-light: 300;
    --opcentrix-font-normal: 400;
    --opcentrix-font-medium: 500;
    --opcentrix-font-semibold: 600;
    --opcentrix-font-bold: 700;
    
    /* === LINE HEIGHTS === */
    --opcentrix-leading-tight: 1.25;
    --opcentrix-leading-normal: 1.5;
    --opcentrix-leading-relaxed: 1.75;
}
```

#### **Spacing System**
```css
:root {
    /* === SPACING SCALE === */
    --opcentrix-spacing-0: 0;
    --opcentrix-spacing-1: 0.25rem;    /* 4px */
    --opcentrix-spacing-2: 0.5rem;     /* 8px */
    --opcentrix-spacing-3: 0.75rem;    /* 12px */
    --opcentrix-spacing-4: 1rem;       /* 16px */
    --opcentrix-spacing-5: 1.25rem;    /* 20px */
    --opcentrix-spacing-6: 1.5rem;     /* 24px */
    --opcentrix-spacing-8: 2rem;       /* 32px */
    --opcentrix-spacing-10: 2.5rem;    /* 40px */
    --opcentrix-spacing-12: 3rem;      /* 48px */
    --opcentrix-spacing-16: 4rem;      /* 64px */
    --opcentrix-spacing-20: 5rem;      /* 80px */
    
    /* === COMPONENT SPACING === */
    --opcentrix-gap-sm: var(--opcentrix-spacing-2);
    --opcentrix-gap-md: var(--opcentrix-spacing-4);
    --opcentrix-gap-lg: var(--opcentrix-spacing-6);
    --opcentrix-gap-xl: var(--opcentrix-spacing-8);
}
```

#### **Border Radius & Shadows**
```css
:root {
    /* === BORDER RADIUS === */
    --opcentrix-radius-none: 0;
    --opcentrix-radius-sm: 0.125rem;   /* 2px */
    --opcentrix-radius: 0.25rem;       /* 4px */
    --opcentrix-radius-md: 0.375rem;   /* 6px */
    --opcentrix-radius-lg: 0.5rem;     /* 8px */
    --opcentrix-radius-xl: 0.75rem;    /* 12px */
    --opcentrix-radius-2xl: 1rem;      /* 16px */
    --opcentrix-radius-full: 9999px;   /* Fully rounded */
    
    /* === SHADOWS === */
    --opcentrix-shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
    --opcentrix-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    --opcentrix-shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    --opcentrix-shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
    --opcentrix-shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
    --opcentrix-shadow-inner: inset 0 2px 4px 0 rgba(0, 0, 0, 0.06);
    
    /* === COMPONENT SHADOWS === */
    --opcentrix-shadow-card: var(--opcentrix-shadow);
    --opcentrix-shadow-modal: var(--opcentrix-shadow-xl);
    --opcentrix-shadow-dropdown: var(--opcentrix-shadow-lg);
}
```

#### **Transitions & Animation**
```css
:root {
    /* === TRANSITION DURATIONS === */
    --opcentrix-duration-75: 75ms;
    --opcentrix-duration-100: 100ms;
    --opcentrix-duration-150: 150ms;
    --opcentrix-duration-200: 200ms;
    --opcentrix-duration-300: 300ms;
    --opcentrix-duration-500: 500ms;
    --opcentrix-duration-700: 700ms;
    --opcentrix-duration-1000: 1000ms;
    
    /* === TRANSITION TIMING === */
    --opcentrix-ease-linear: linear;
    --opcentrix-ease-in: cubic-bezier(0.4, 0, 1, 1);
    --opcentrix-ease-out: cubic-bezier(0, 0, 0.2, 1);
    --opcentrix-ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
    
    /* === COMMON TRANSITIONS === */
    --opcentrix-transition-all: all var(--opcentrix-duration-200) var(--opcentrix-ease-in-out);
    --opcentrix-transition-colors: color var(--opcentrix-duration-200) var(--opcentrix-ease-in-out), 
                                  background-color var(--opcentrix-duration-200) var(--opcentrix-ease-in-out),
                                  border-color var(--opcentrix-duration-200) var(--opcentrix-ease-in-out);
    --opcentrix-transition-transform: transform var(--opcentrix-duration-200) var(--opcentrix-ease-in-out);
}
```

## ?? Component Architecture

### **1. Base Components**

#### **OpCentrix Cards**
```css
.opcentrix-card {
    background: white;
    border-radius: var(--opcentrix-radius-lg);
    box-shadow: var(--opcentrix-shadow-card);
    border: 1px solid var(--opcentrix-gray-200);
    transition: var(--opcentrix-transition-all);
    overflow: hidden;
}

.opcentrix-card:hover {
    box-shadow: var(--opcentrix-shadow-md);
    transform: translateY(-1px);
}

.opcentrix-card-header {
    padding: var(--opcentrix-spacing-6);
    border-bottom: 1px solid var(--opcentrix-gray-200);
    background: var(--opcentrix-gray-50);
}

.opcentrix-card-body {
    padding: var(--opcentrix-spacing-6);
}

.opcentrix-card-footer {
    padding: var(--opcentrix-spacing-4) var(--opcentrix-spacing-6);
    background: var(--opcentrix-gray-50);
    border-top: 1px solid var(--opcentrix-gray-200);
}
```

#### **OpCentrix Buttons**
```css
.opcentrix-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: var(--opcentrix-spacing-3) var(--opcentrix-spacing-5);
    border-radius: var(--opcentrix-radius-md);
    font-weight: var(--opcentrix-font-semibold);
    font-size: var(--opcentrix-text-sm);
    line-height: var(--opcentrix-leading-tight);
    text-decoration: none;
    border: 1px solid transparent;
    cursor: pointer;
    transition: var(--opcentrix-transition-all);
    user-select: none;
    position: relative;
    overflow: hidden;
}

.opcentrix-button:focus {
    outline: 2px solid var(--opcentrix-primary);
    outline-offset: 2px;
}

.opcentrix-button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none !important;
}

/* Button Variants */
.opcentrix-button-primary {
    background: var(--opcentrix-gradient-primary);
    color: white;
    box-shadow: var(--opcentrix-shadow-sm);
}

.opcentrix-button-primary:hover:not(:disabled) {
    box-shadow: var(--opcentrix-shadow-md);
    transform: translateY(-1px);
}

.opcentrix-button-secondary {
    background: white;
    color: var(--opcentrix-gray-700);
    border-color: var(--opcentrix-gray-300);
}

.opcentrix-button-secondary:hover:not(:disabled) {
    background: var(--opcentrix-gray-50);
    border-color: var(--opcentrix-gray-400);
}

.opcentrix-button-success {
    background: var(--opcentrix-success);
    color: white;
}

.opcentrix-button-success:hover:not(:disabled) {
    background: var(--opcentrix-success-dark);
    transform: translateY(-1px);
}

.opcentrix-button-danger {
    background: var(--opcentrix-danger);
    color: white;
}

.opcentrix-button-danger:hover:not(:disabled) {
    background: var(--opcentrix-danger-dark);
    transform: translateY(-1px);
}

/* Button Sizes */
.opcentrix-button-sm {
    padding: var(--opcentrix-spacing-2) var(--opcentrix-spacing-4);
    font-size: var(--opcentrix-text-xs);
}

.opcentrix-button-lg {
    padding: var(--opcentrix-spacing-4) var(--opcentrix-spacing-8);
    font-size: var(--opcentrix-text-lg);
}
```

#### **OpCentrix Form Elements**
```css
.opcentrix-input {
    display: block;
    width: 100%;
    padding: var(--opcentrix-spacing-3) var(--opcentrix-spacing-4);
    font-size: var(--opcentrix-text-base);
    line-height: var(--opcentrix-leading-normal);
    color: var(--opcentrix-gray-900);
    background: white;
    background-clip: padding-box;
    border: 1px solid var(--opcentrix-gray-300);
    border-radius: var(--opcentrix-radius-md);
    transition: var(--opcentrix-transition-colors);
}

.opcentrix-input:focus {
    outline: none;
    border-color: var(--opcentrix-primary);
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.opcentrix-input:disabled {
    background: var(--opcentrix-gray-100);
    color: var(--opcentrix-gray-500);
    cursor: not-allowed;
}

.opcentrix-input.error {
    border-color: var(--opcentrix-danger);
}

.opcentrix-input.error:focus {
    border-color: var(--opcentrix-danger);
    box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.opcentrix-label {
    display: block;
    font-weight: var(--opcentrix-font-medium);
    color: var(--opcentrix-gray-700);
    margin-bottom: var(--opcentrix-spacing-2);
}

.opcentrix-select {
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
    background-position: right var(--opcentrix-spacing-3) center;
    background-repeat: no-repeat;
    background-size: 1.5em 1.5em;
    padding-right: var(--opcentrix-spacing-10);
}
```

### **2. Layout Components**

#### **Navigation Components**
```css
.nav-container {
    background: white;
    border-bottom: 1px solid var(--opcentrix-gray-200);
    box-shadow: var(--opcentrix-shadow-sm);
}

.nav-brand {
    display: flex;
    align-items: center;
    font-weight: var(--opcentrix-font-bold);
    font-size: var(--opcentrix-text-xl);
    color: var(--opcentrix-gray-900);
    text-decoration: none;
    transition: var(--opcentrix-transition-colors);
}

.nav-brand-gradient {
    background: var(--opcentrix-gradient-primary);
    background-clip: text;
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
}

.nav-item {
    display: flex;
    align-items: center;
    padding: var(--opcentrix-spacing-3) var(--opcentrix-spacing-4);
    border-radius: var(--opcentrix-radius-md);
    color: var(--opcentrix-gray-600);
    text-decoration: none;
    font-weight: var(--opcentrix-font-medium);
    transition: var(--opcentrix-transition-all);
    position: relative;
}

.nav-item:hover {
    color: var(--opcentrix-primary);
    background: var(--opcentrix-gray-50);
}

.nav-item-active {
    background: var(--opcentrix-gradient-primary);
    color: white;
    box-shadow: var(--opcentrix-shadow-md);
}

.nav-item-active:hover {
    color: white;
    transform: translateY(-1px);
}

/* Mobile Navigation */
.nav-mobile-toggle {
    display: none;
    padding: var(--opcentrix-spacing-2);
    border: none;
    background: none;
    cursor: pointer;
}

@media (max-width: 768px) {
    .nav-mobile-toggle {
        display: block;
    }
    
    .nav-menu {
        display: none;
        position: absolute;
        top: 100%;
        left: 0;
        right: 0;
        background: white;
        border-top: 1px solid var(--opcentrix-gray-200);
        box-shadow: var(--opcentrix-shadow-lg);
        z-index: 50;
    }
    
    .nav-menu.open {
        display: block;
    }
    
    .nav-item {
        display: block;
        padding: var(--opcentrix-spacing-4);
        border-bottom: 1px solid var(--opcentrix-gray-200);
    }
}
```

#### **Grid Systems**
```css
.opcentrix-grid {
    display: grid;
    gap: var(--opcentrix-gap-md);
}

.opcentrix-grid-1 { grid-template-columns: repeat(1, 1fr); }
.opcentrix-grid-2 { grid-template-columns: repeat(2, 1fr); }
.opcentrix-grid-3 { grid-template-columns: repeat(3, 1fr); }
.opcentrix-grid-4 { grid-template-columns: repeat(4, 1fr); }

/* Responsive Grid */
@media (max-width: 768px) {
    .opcentrix-grid-2,
    .opcentrix-grid-3,
    .opcentrix-grid-4 {
        grid-template-columns: 1fr;
    }
}

@media (min-width: 769px) and (max-width: 1024px) {
    .opcentrix-grid-3,
    .opcentrix-grid-4 {
        grid-template-columns: repeat(2, 1fr);
    }
}

/* Flexbox Utilities */
.opcentrix-flex {
    display: flex;
}

.opcentrix-flex-col {
    flex-direction: column;
}

.opcentrix-items-center {
    align-items: center;
}

.opcentrix-justify-between {
    justify-content: space-between;
}

.opcentrix-flex-1 {
    flex: 1;
}
```

## ?? Scheduler-Specific Styles

### **Scheduler Grid System**
```css
.scheduler-container {
    background: white;
    border-radius: var(--opcentrix-radius-lg);
    box-shadow: var(--opcentrix-shadow-card);
    overflow: hidden;
}

.scheduler-header {
    background: var(--opcentrix-gray-50);
    border-bottom: 1px solid var(--opcentrix-gray-200);
    padding: var(--opcentrix-spacing-4);
}

.scheduler-grid {
    display: grid;
    grid-template-columns: 200px 1fr;
    min-height: 600px;
    position: relative;
}

.machine-labels {
    background: var(--opcentrix-gray-50);
    border-right: 1px solid var(--opcentrix-gray-200);
}

.machine-label {
    display: flex;
    align-items: center;
    padding: var(--opcentrix-spacing-4);
    font-weight: var(--opcentrix-font-semibold);
    border-bottom: 1px solid var(--opcentrix-gray-200);
    height: var(--machine-row-height, 80px);
    transition: var(--opcentrix-transition-colors);
}

/* Machine-Specific Colors */
.machine-label.ti1 {
    background: linear-gradient(135deg, #FEF3C7 0%, #FDE68A 100%);
    color: #92400E;
    border-left: 4px solid #F59E0B;
}

.machine-label.ti2 {
    background: linear-gradient(135deg, #DBEAFE 0%, #93C5FD 100%);
    color: #1E40AF;
    border-left: 4px solid #3B82F6;
}

.machine-label.inc {
    background: linear-gradient(135deg, #D1FAE5 0%, #A7F3D0 100%);
    color: #065F46;
    border-left: 4px solid #10B981;
}

.scheduler-timeline {
    position: relative;
    overflow-x: auto;
    overflow-y: hidden;
}

.time-grid {
    display: grid;
    grid-template-rows: auto 1fr;
    min-width: 100%;
}

.time-header {
    display: grid;
    grid-auto-columns: var(--slot-width, 100px);
    grid-auto-flow: column;
    background: var(--opcentrix-gray-50);
    border-bottom: 1px solid var(--opcentrix-gray-200);
    font-size: var(--opcentrix-text-sm);
    font-weight: var(--opcentrix-font-medium);
}

.time-slot {
    padding: var(--opcentrix-spacing-2);
    text-align: center;
    border-right: 1px solid var(--opcentrix-gray-200);
    color: var(--opcentrix-gray-600);
}

.machine-rows {
    display: grid;
    grid-template-rows: repeat(var(--machine-count, 3), var(--machine-row-height, 80px));
}

.machine-row {
    position: relative;
    border-bottom: 1px solid var(--opcentrix-gray-200);
    display: grid;
    grid-auto-columns: var(--slot-width, 100px);
    grid-auto-flow: column;
    align-items: center;
}

.machine-row:hover {
    background: rgba(59, 130, 246, 0.02);
}
```

### **Job Block Styles**
```css
.job-block {
    position: absolute;
    top: var(--opcentrix-spacing-2);
    bottom: var(--opcentrix-spacing-2);
    border-radius: var(--opcentrix-radius-md);
    padding: var(--opcentrix-spacing-2) var(--opcentrix-spacing-3);
    font-size: var(--opcentrix-text-xs);
    font-weight: var(--opcentrix-font-medium);
    color: white;
    cursor: pointer;
    transition: var(--opcentrix-transition-all);
    z-index: 10;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    justify-content: center;
    box-shadow: var(--opcentrix-shadow-sm);
}

.job-block:hover {
    transform: translateY(-2px);
    box-shadow: var(--opcentrix-shadow-md);
    z-index: 20;
}

/* Job Status Colors */
.job-block.scheduled {
    background: linear-gradient(135deg, var(--opcentrix-primary) 0%, var(--opcentrix-secondary) 100%);
}

.job-block.active {
    background: linear-gradient(135deg, var(--opcentrix-success) 0%, var(--opcentrix-info) 100%);
    animation: pulse 2s infinite;
}

.job-block.completed {
    background: linear-gradient(135deg, var(--opcentrix-gray-400) 0%, var(--opcentrix-gray-500) 100%);
}

.job-block.delayed {
    background: linear-gradient(135deg, var(--opcentrix-warning) 0%, var(--opcentrix-danger) 100%);
}

.job-block.cancelled {
    background: linear-gradient(135deg, var(--opcentrix-danger) 0%, var(--opcentrix-danger-dark) 100%);
    opacity: 0.7;
}

/* Priority Indicators */
.job-block.priority-1::before {
    content: "??";
    position: absolute;
    top: 2px;
    right: 4px;
    font-size: 8px;
}

.job-block.priority-2::before {
    content: "?";
    position: absolute;
    top: 2px;
    right: 4px;
    font-size: 8px;
}

.job-block-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    min-height: 0;
}

.job-part-number {
    font-weight: var(--opcentrix-font-bold);
    text-overflow: ellipsis;
    overflow: hidden;
    white-space: nowrap;
}

.job-operator {
    opacity: 0.8;
    font-size: 0.7rem;
    text-overflow: ellipsis;
    overflow: hidden;
    white-space: nowrap;
}
```

### **Responsive Scheduler**
```css
/* Responsive Variables */
:root {
    --slot-width: 100px;
    --machine-row-height: 80px;
    --machine-count: 3;
}

/* Tablet */
@media (max-width: 1024px) {
    :root {
        --slot-width: 80px;
        --machine-row-height: 70px;
    }
    
    .scheduler-grid {
        grid-template-columns: 150px 1fr;
    }
    
    .machine-label {
        font-size: var(--opcentrix-text-sm);
    }
}

/* Mobile */
@media (max-width: 768px) {
    :root {
        --slot-width: 60px;
        --machine-row-height: 60px;
    }
    
    .scheduler-grid {
        grid-template-columns: 120px 1fr;
    }
    
    .job-block {
        font-size: 0.6rem;
        padding: var(--opcentrix-spacing-1) var(--opcentrix-spacing-2);
    }
    
    .time-slot {
        font-size: 0.65rem;
        padding: var(--opcentrix-spacing-1);
    }
}

/* Extra Small Mobile */
@media (max-width: 480px) {
    :root {
        --slot-width: 50px;
        --machine-row-height: 50px;
    }
    
    .scheduler-grid {
        grid-template-columns: 100px 1fr;
    }
    
    .machine-label {
        font-size: 0.7rem;
        padding: var(--opcentrix-spacing-2);
    }
}
```

## ?? Admin Dashboard Styles

### **Dashboard Layout**
```css
.admin-layout {
    display: grid;
    grid-template-columns: 288px 1fr;
    min-height: 100vh;
    background: var(--opcentrix-gray-50);
}

.admin-sidebar {
    background: white;
    border-right: 1px solid var(--opcentrix-gray-200);
    box-shadow: var(--opcentrix-shadow-sm);
    overflow-y: auto;
    position: fixed;
    height: 100vh;
    width: 288px;
}

.admin-content {
    margin-left: 288px;
    padding: var(--opcentrix-spacing-6);
    min-height: 100vh;
}

.admin-header {
    padding: var(--opcentrix-spacing-6);
    border-bottom: 1px solid var(--opcentrix-gray-200);
}

.admin-nav {
    padding: var(--opcentrix-spacing-4);
}

.admin-nav-section {
    margin-bottom: var(--opcentrix-spacing-6);
}

.admin-nav-title {
    font-size: var(--opcentrix-text-xs);
    font-weight: var(--opcentrix-font-bold);
    color: var(--opcentrix-gray-500);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: var(--opcentrix-spacing-3);
    padding: 0 var(--opcentrix-spacing-3);
}

.admin-nav-item {
    display: flex;
    align-items: center;
    padding: var(--opcentrix-spacing-3);
    margin-bottom: var(--opcentrix-spacing-1);
    border-radius: var(--opcentrix-radius-md);
    color: var(--opcentrix-gray-600);
    text-decoration: none;
    font-weight: var(--opcentrix-font-medium);
    transition: var(--opcentrix-transition-all);
}

.admin-nav-item:hover {
    background: var(--opcentrix-gray-50);
    color: var(--opcentrix-primary);
}

.admin-nav-item.active {
    background: var(--opcentrix-gradient-primary);
    color: white;
    box-shadow: var(--opcentrix-shadow-sm);
}

.admin-nav-icon {
    width: 20px;
    height: 20px;
    margin-right: var(--opcentrix-spacing-3);
    flex-shrink: 0;
}

/* Mobile Admin Layout */
@media (max-width: 768px) {
    .admin-layout {
        grid-template-columns: 1fr;
    }
    
    .admin-sidebar {
        position: fixed;
        top: 0;
        left: -288px;
        z-index: 50;
        transition: left var(--opcentrix-duration-300) ease-in-out;
    }
    
    .admin-sidebar.open {
        left: 0;
    }
    
    .admin-content {
        margin-left: 0;
        padding: var(--opcentrix-spacing-4);
    }
    
    .admin-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
        z-index: 40;
        opacity: 0;
        visibility: hidden;
        transition: all var(--opcentrix-duration-300) ease-in-out;
    }
    
    .admin-overlay.open {
        opacity: 1;
        visibility: visible;
    }
}
```

### **KPI Cards**
```css
.kpi-card {
    background: white;
    border-radius: var(--opcentrix-radius-xl);
    box-shadow: var(--opcentrix-shadow-card);
    padding: var(--opcentrix-spacing-6);
    transition: var(--opcentrix-transition-all);
    position: relative;
    overflow: hidden;
}

.kpi-card:hover {
    box-shadow: var(--opcentrix-shadow-lg);
    transform: translateY(-2px);
}

.kpi-card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: var(--opcentrix-gradient-primary);
}

.kpi-header {
    display: flex;
    items: center;
    justify-content: space-between;
    margin-bottom: var(--opcentrix-spacing-4);
}

.kpi-title {
    font-size: var(--opcentrix-text-sm);
    font-weight: var(--opcentrix-font-medium);
    color: var(--opcentrix-gray-600);
    margin: 0;
}

.kpi-icon {
    width: 48px;
    height: 48px;
    background: var(--opcentrix-primary-light);
    border-radius: var(--opcentrix-radius-xl);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--opcentrix-primary);
}

.kpi-value {
    font-size: var(--opcentrix-text-3xl);
    font-weight: var(--opcentrix-font-bold);
    color: var(--opcentrix-gray-900);
    margin: 0;
    line-height: 1;
}

.kpi-change {
    font-size: var(--opcentrix-text-sm);
    font-weight: var(--opcentrix-font-medium);
    display: flex;
    align-items: center;
    margin-top: var(--opcentrix-spacing-2);
}

.kpi-change.positive {
    color: var(--opcentrix-success);
}

.kpi-change.negative {
    color: var(--opcentrix-danger);
}

.kpi-change.neutral {
    color: var(--opcentrix-gray-500);
}

.kpi-progress {
    margin-top: var(--opcentrix-spacing-4);
    background: var(--opcentrix-gray-100);
    border-radius: var(--opcentrix-radius-full);
    height: 8px;
    overflow: hidden;
}

.kpi-progress-bar {
    height: 100%;
    background: var(--opcentrix-gradient-primary);
    border-radius: var(--opcentrix-radius-full);
    transition: width var(--opcentrix-duration-500) ease-out;
}
```

## ?? Modal System

### **Modal Styles**
```css
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(4px);
    z-index: 1000;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--opcentrix-spacing-4);
    opacity: 0;
    visibility: hidden;
    transition: all var(--opcentrix-duration-300) ease-in-out;
}

.modal-overlay.open {
    opacity: 1;
    visibility: visible;
}

.modal-content {
    background: white;
    border-radius: var(--opcentrix-radius-xl);
    box-shadow: var(--opcentrix-shadow-modal);
    max-width: 600px;
    width: 100%;
    max-height: 90vh;
    overflow-y: auto;
    transform: scale(0.95) translateY(20px);
    transition: transform var(--opcentrix-duration-300) ease-out;
}

.modal-overlay.open .modal-content {
    transform: scale(1) translateY(0);
}

.modal-header {
    padding: var(--opcentrix-spacing-6);
    border-bottom: 1px solid var(--opcentrix-gray-200);
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.modal-title {
    font-size: var(--opcentrix-text-xl);
    font-weight: var(--opcentrix-font-semibold);
    color: var(--opcentrix-gray-900);
    margin: 0;
}

.modal-close {
    background: none;
    border: none;
    padding: var(--opcentrix-spacing-2);
    cursor: pointer;
    color: var(--opcentrix-gray-400);
    transition: var(--opcentrix-transition-colors);
}

.modal-close:hover {
    color: var(--opcentrix-gray-600);
}

.modal-body {
    padding: var(--opcentrix-spacing-6);
}

.modal-footer {
    padding: var(--opcentrix-spacing-4) var(--opcentrix-spacing-6);
    border-top: 1px solid var(--opcentrix-gray-200);
    display: flex;
    justify-content: flex-end;
    gap: var(--opcentrix-spacing-3);
}

/* Mobile Modal */
@media (max-width: 768px) {
    .modal-overlay {
        padding: var(--opcentrix-spacing-2);
        align-items: flex-end;
    }
    
    .modal-content {
        max-height: 95vh;
        border-radius: var(--opcentrix-radius-xl) var(--opcentrix-radius-xl) 0 0;
        transform: translateY(100%);
    }
    
    .modal-overlay.open .modal-content {
        transform: translateY(0);
    }
}
```

## ? Animation & Micro-Interactions

### **Keyframe Animations**
```css
@keyframes slideInRight {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

@keyframes slideInLeft {
    from {
        transform: translateX(-100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

@keyframes slideInUp {
    from {
        transform: translateY(100%);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

@keyframes fadeIn {
    from {
        opacity: 0;
    }
    to {
        opacity: 1;
    }
}

@keyframes pulse {
    0%, 100% {
        transform: scale(1);
    }
    50% {
        transform: scale(1.02);
    }
}

@keyframes spin {
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
}

/* Animation Classes */
.animate-slide-in-right {
    animation: slideInRight var(--opcentrix-duration-300) var(--opcentrix-ease-out);
}

.animate-slide-in-left {
    animation: slideInLeft var(--opcentrix-duration-300) var(--opcentrix-ease-out);
}

.animate-slide-in-up {
    animation: slideInUp var(--opcentrix-duration-300) var(--opcentrix-ease-out);
}

.animate-fade-in {
    animation: fadeIn var(--opcentrix-duration-300) var(--opcentrix-ease-out);
}

.animate-pulse {
    animation: pulse var(--opcentrix-duration-1000) infinite;
}

.animate-spin {
    animation: spin var(--opcentrix-duration-1000) linear infinite;
}
```

### **Loading States**
```css
.loading-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(255, 255, 255, 0.8);
    backdrop-filter: blur(2px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    opacity: 0;
    visibility: hidden;
    transition: all var(--opcentrix-duration-200) ease-in-out;
}

.loading-overlay.active {
    opacity: 1;
    visibility: visible;
}

.loading-spinner {
    width: 40px;
    height: 40px;
    border: 3px solid var(--opcentrix-gray-200);
    border-top: 3px solid var(--opcentrix-primary);
    border-radius: var(--opcentrix-radius-full);
    animation: spin var(--opcentrix-duration-1000) linear infinite;
}

/* Skeleton Loading */
.skeleton {
    background: linear-gradient(90deg, var(--opcentrix-gray-200) 25%, var(--opcentrix-gray-100) 50%, var(--opcentrix-gray-200) 75%);
    background-size: 200% 100%;
    animation: skeleton-pulse 1.5s infinite ease-in-out;
    border-radius: var(--opcentrix-radius);
}

@keyframes skeleton-pulse {
    0% {
        background-position: 200% 0;
    }
    100% {
        background-position: -200% 0;
    }
}

.skeleton-text {
    height: 1em;
    margin-bottom: var(--opcentrix-spacing-2);
}

.skeleton-title {
    height: 1.5em;
    width: 60%;
    margin-bottom: var(--opcentrix-spacing-4);
}

.skeleton-paragraph {
    height: 1em;
    margin-bottom: var(--opcentrix-spacing-2);
}

.skeleton-paragraph:last-child {
    width: 80%;
}
```

## ?? Utility Classes

### **Spacing Utilities**
```css
/* Margin */
.m-0 { margin: 0; }
.m-1 { margin: var(--opcentrix-spacing-1); }
.m-2 { margin: var(--opcentrix-spacing-2); }
.m-3 { margin: var(--opcentrix-spacing-3); }
.m-4 { margin: var(--opcentrix-spacing-4); }
.m-5 { margin: var(--opcentrix-spacing-5); }
.m-6 { margin: var(--opcentrix-spacing-6); }

/* Padding */
.p-0 { padding: 0; }
.p-1 { padding: var(--opcentrix-spacing-1); }
.p-2 { padding: var(--opcentrix-spacing-2); }
.p-3 { padding: var(--opcentrix-spacing-3); }
.p-4 { padding: var(--opcentrix-spacing-4); }
.p-5 { padding: var(--opcentrix-spacing-5); }
.p-6 { padding: var(--opcentrix-spacing-6); }

/* Margin Auto */
.mx-auto { margin-left: auto; margin-right: auto; }
.my-auto { margin-top: auto; margin-bottom: auto; }
```

### **Color Utilities**
```css
/* Text Colors */
.text-primary { color: var(--opcentrix-primary) !important; }
.text-secondary { color: var(--opcentrix-secondary) !important; }
.text-success { color: var(--opcentrix-success) !important; }
.text-warning { color: var(--opcentrix-warning) !important; }
.text-danger { color: var(--opcentrix-danger) !important; }
.text-gray-600 { color: var(--opcentrix-gray-600) !important; }
.text-gray-900 { color: var(--opcentrix-gray-900) !important; }

/* Background Colors */
.bg-primary { background-color: var(--opcentrix-primary) !important; }
.bg-secondary { background-color: var(--opcentrix-secondary) !important; }
.bg-success { background-color: var(--opcentrix-success) !important; }
.bg-warning { background-color: var(--opcentrix-warning) !important; }
.bg-danger { background-color: var(--opcentrix-danger) !important; }
.bg-gray-50 { background-color: var(--opcentrix-gray-50) !important; }
.bg-gray-100 { background-color: var(--opcentrix-gray-100) !important; }

/* Gradient Backgrounds */
.bg-gradient-primary { background: var(--opcentrix-gradient-primary) !important; }
.bg-gradient-success { background: var(--opcentrix-gradient-success) !important; }
.bg-gradient-warning { background: var(--opcentrix-gradient-warning) !important; }
```

### **Typography Utilities**
```css
/* Font Sizes */
.text-xs { font-size: var(--opcentrix-text-xs); }
.text-sm { font-size: var(--opcentrix-text-sm); }
.text-base { font-size: var(--opcentrix-text-base); }
.text-lg { font-size: var(--opcentrix-text-lg); }
.text-xl { font-size: var(--opcentrix-text-xl); }
.text-2xl { font-size: var(--opcentrix-text-2xl); }
.text-3xl { font-size: var(--opcentrix-text-3xl); }

/* Font Weights */
.font-light { font-weight: var(--opcentrix-font-light); }
.font-normal { font-weight: var(--opcentrix-font-normal); }
.font-medium { font-weight: var(--opcentrix-font-medium); }
.font-semibold { font-weight: var(--opcentrix-font-semibold); }
.font-bold { font-weight: var(--opcentrix-font-bold); }

/* Text Alignment */
.text-left { text-align: left; }
.text-center { text-align: center; }
.text-right { text-align: right; }

/* Text Decoration */
.underline { text-decoration: underline; }
.no-underline { text-decoration: none; }
```

## ?? Responsive Design

### **Breakpoint System**
```css
/* Extra Small (Mobile) */
@media (max-width: 479px) {
    .xs\:hidden { display: none; }
    .xs\:block { display: block; }
    .xs\:text-sm { font-size: var(--opcentrix-text-sm); }
}

/* Small (Mobile) */
@media (max-width: 639px) {
    .sm\:hidden { display: none; }
    .sm\:block { display: block; }
    .sm\:grid-cols-1 { grid-template-columns: repeat(1, 1fr); }
}

/* Medium (Tablet) */
@media (max-width: 767px) {
    .md\:hidden { display: none; }
    .md\:block { display: block; }
    .md\:grid-cols-2 { grid-template-columns: repeat(2, 1fr); }
}

/* Large (Desktop) */
@media (min-width: 1024px) {
    .lg\:grid-cols-3 { grid-template-columns: repeat(3, 1fr); }
    .lg\:grid-cols-4 { grid-template-columns: repeat(4, 1fr); }
}

/* Extra Large (Wide Desktop) */
@media (min-width: 1280px) {
    .xl\:grid-cols-5 { grid-template-columns: repeat(5, 1fr); }
}
```

### **Mobile Optimizations**
```css
/* Touch-friendly interactions */
@media (hover: none) and (pointer: coarse) {
    .opcentrix-button {
        min-height: 44px;
        min-width: 44px;
    }
    
    .nav-item {
        min-height: 44px;
    }
    
    .job-block {
        min-height: 40px;
    }
}

/* High DPI displays */
@media (-webkit-min-device-pixel-ratio: 2), (min-resolution: 192dpi) {
    .opcentrix-card {
        border-width: 0.5px;
    }
}

/* Reduced motion preference */
@media (prefers-reduced-motion: reduce) {
    * {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
    }
}
```

## ?? Performance Optimizations

### **CSS Performance**
```css
/* Efficient selectors */
.opcentrix-card { /* Class-based selectors */ }
.job-block { /* Avoid deep nesting */ }

/* Hardware acceleration */
.job-block {
    transform: translateZ(0); /* Force GPU layer */
    will-change: transform; /* Optimize for transforms */
}

/* Critical path optimization */
.above-fold {
    /* Inline critical styles for above-fold content */
}

/* Font loading optimization */
@font-face {
    font-family: 'Inter';
    font-display: swap; /* Improve loading performance */
    src: url('fonts/inter.woff2') format('woff2');
}
```

### **CSS Variables for Performance**
```css
/* Efficient property updates */
:root {
    --dynamic-width: 100px;
    --dynamic-height: 80px;
    --dynamic-color: #3B82F6;
}

.dynamic-element {
    width: var(--dynamic-width);
    height: var(--dynamic-height);
    background: var(--dynamic-color);
    /* Update variables via JavaScript for smooth animations */
}
```

---

This comprehensive CSS structure provides a solid foundation for maintaining and extending the OpCentrix design system while ensuring consistency, performance, and accessibility across all user interfaces.

---

*Last Updated: December 2024*  
*Version: 2.0.0*