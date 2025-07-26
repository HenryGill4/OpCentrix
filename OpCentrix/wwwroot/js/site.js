// OpCentrix Site-wide JavaScript Functions
// Global functions for form handling, notifications, and UI interactions with Enhanced Error Logging

console.log('✓ [SITE] OpCentrix site.js loading...');

// **ENHANCED ERROR LOGGING SYSTEM**
// Centralized error tracking and logging for debugging and issue tracking

/**
 * Enhanced Error Logger - Central error tracking system
 */
window.OpCentrixErrorLogger = {
    errors: [],
    maxErrors: 100,
    
    /**
     * Log an error with detailed context
     */
    logError(category, operation, error, context = {}) {
        const errorEntry = {
            id: this.generateErrorId(),
            timestamp: new Date().toISOString(),
            category: category, // 'SITE', 'HTMX', 'FORM', 'MODAL', 'API', etc.
            operation: operation, // 'updateSlsMaterial', 'saveJob', 'loadModal', etc.
            error: {
                message: error?.message || error || 'Unknown error',
                stack: error?.stack || null,
                type: error?.constructor?.name || 'Unknown'
            },
            context: {
                url: window.location.href,
                userAgent: navigator.userAgent,
                timestamp: Date.now(),
                ...context
            },
            browser: {
                viewport: `${window.innerWidth}x${window.innerHeight}`,
                screen: `${screen.width}x${screen.height}`,
                pixelRatio: window.devicePixelRatio,
                online: navigator.onLine
            }
        };
        
        // Add to errors array
        this.errors.push(errorEntry);
        
        // Keep only last maxErrors entries
        if (this.errors.length > this.maxErrors) {
            this.errors = this.errors.slice(-this.maxErrors);
        }
        
        // Console logging with enhanced details
        console.group(`✗ [${category}] Error in ${operation}`);
        console.error('Error Details:', errorEntry.error);
        console.warn('Context:', errorEntry.context);
        console.info('Browser Info:', errorEntry.browser);
        console.log('Error ID:', errorEntry.id);
        console.groupEnd();
        
        // Store in localStorage for persistence
        try {
            localStorage.setItem('opcentrix_errors', JSON.stringify(this.errors.slice(-10)));
        } catch (e) {
            console.warn('Failed to store errors in localStorage:', e);
        }
        
        // Send to server if available
        this.sendErrorToServer(errorEntry);
        
        return errorEntry.id;
    },
    
    /**
     * Generate unique error ID
     */
    generateErrorId() {
        return `ERR_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    },
    
    /**
     * Send error to server for logging
     */
    async sendErrorToServer(errorEntry) {
        try {
            await fetch('/Api/ErrorLog', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(errorEntry)
            });
        } catch (e) {
            console.warn('Failed to send error to server:', e);
        }
    },
    
    /**
     * Get error report for debugging
     */
    getErrorReport() {
        return {
            totalErrors: this.errors.length,
            recentErrors: this.errors.slice(-5),
            errorsByCategory: this.groupBy(this.errors, 'category'),
            errorsByOperation: this.groupBy(this.errors, 'operation'),
            timeRange: {
                first: this.errors[0]?.timestamp,
                last: this.errors[this.errors.length - 1]?.timestamp
            }
        };
    },
    
    /**
     * Helper function to group errors
     */
    groupBy(array, key) {
        return array.reduce((result, item) => {
            const group = item[key] || 'unknown';
            result[group] = (result[group] || 0) + 1;
            return result;
        }, {});
    },
    
    /**
     * Clear error log
     */
    clearErrors() {
        this.errors = [];
        localStorage.removeItem('opcentrix_errors');
        console.log('✓ [SITE] Error log cleared');
    }
};

/**
 * PAGE MONITORING SYSTEM
 * Comprehensive monitoring for click-through testing
 */
window.OpCentrixPageMonitor = {
    currentPage: null,
    pageStartTime: null,
    interactions: [],
    
    /**
     * Initialize page monitoring
     */
    init() {
        this.currentPage = this.getPageInfo();
        this.pageStartTime = Date.now();
        this.interactions = [];
        
        console.log('🔍 [MONITOR] Page monitoring started:', this.currentPage.path);
        
        // Monitor all clicks
        document.addEventListener('click', (e) => this.logInteraction('click', e));
        
        // Monitor all form submissions
        document.addEventListener('submit', (e) => this.logInteraction('submit', e));
        
        // Monitor all HTMX requests
        document.body.addEventListener('htmx:beforeRequest', (e) => this.logInteraction('htmx-request', e));
        document.body.addEventListener('htmx:responseError', (e) => this.logInteraction('htmx-error', e));
        document.body.addEventListener('htmx:afterRequest', (e) => this.logInteraction('htmx-response', e));
        
        // Monitor page visibility changes
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.logPageActivity('hidden');
            } else {
                this.logPageActivity('visible');
            }
        });
        
        // Monitor errors in resource loading
        window.addEventListener('error', (e) => {
            if (e.target !== window) { // Resource loading error
                this.logResourceError(e);
            }
        }, true);
        
        // Log initial page load
        this.logPageActivity('loaded');
    },
    
    /**
     * Get current page information
     */
    getPageInfo() {
        const path = window.location.pathname;
        const search = window.location.search;
        const hash = window.location.hash;
        
        // Determine page category
        let category = 'unknown';
        if (path.includes('/Admin/')) category = 'admin';
        else if (path.includes('/Scheduler/')) category = 'scheduler';
        else if (path.includes('/Account/')) category = 'account';
        else if (path === '/' || path === '/Index') category = 'home';
        else if (path.includes('/EDM')) category = 'edm';
        else if (path.includes('/Coating')) category = 'coating';
        else if (path.includes('/Printing')) category = 'printing';
        else if (path.includes('/Shipping')) category = 'shipping';
        else if (path.includes('/QC')) category = 'qc';
        else if (path.includes('/Analytics')) category = 'analytics';
        
        return {
            path: path,
            search: search,
            hash: hash,
            category: category,
            title: document.title,
            url: window.location.href
        };
    },
    
    /**
     * Log user interaction
     */
    logInteraction(type, event) {
        const interaction = {
            id: Date.now(),
            type: type,
            timestamp: new Date().toISOString(),
            element: this.getElementInfo(event.target),
            page: this.currentPage.path,
            details: this.getEventDetails(event)
        };
        
        this.interactions.push(interaction);
        
        // Keep only last 50 interactions
        if (this.interactions.length > 50) {
            this.interactions = this.interactions.slice(-50);
        }
        
        console.log(`👆 [MONITOR] ${type.toUpperCase()}:`, interaction.element.identifier, interaction.details);
        
        // Check for potential issues
        this.checkForIssues(interaction);
    },
    
    /**
     * Get element information
     */
    getElementInfo(element) {
        if (!element) return { identifier: 'unknown', type: 'unknown' };
        
        return {
            identifier: element.id || element.name || element.className || element.tagName,
            type: element.tagName?.toLowerCase(),
            id: element.id,
            name: element.name,
            className: element.className,
            text: element.textContent?.substring(0, 50) || '',
            href: element.href || null,
            value: element.value || null
        };
    },
    
    /**
     * Get event details
     */
    getEventDetails(event) {
        const details = {
            type: event.type,
            timestamp: event.timeStamp
        };
        
        // Add specific details based on event type
        if (event.detail?.xhr) {
            details.xhr = {
                status: event.detail.xhr.status,
                statusText: event.detail.xhr.statusText,
                responseLength: event.detail.xhr.responseText?.length || 0
            };
        }
        
        if (event.detail?.pathInfo) {
            details.pathInfo = event.detail.pathInfo;
        }
        
        return details;
    },
    
    /**
     * Log page activity
     */
    logPageActivity(activity) {
        const activityLog = {
            activity: activity,
            timestamp: new Date().toISOString(),
            page: this.currentPage.path,
            timeOnPage: Date.now() - this.pageStartTime,
            interactions: this.interactions.length
        };
        
        console.log(`📄 [MONITOR] Page ${activity}:`, activityLog);
        
        // Store page activity in localStorage
        try {
            const activities = JSON.parse(localStorage.getItem('opcentrix_page_activities'