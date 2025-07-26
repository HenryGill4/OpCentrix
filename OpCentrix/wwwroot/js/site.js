// OpCentrix Site-wide JavaScript Functions
// Global functions for form handling, notifications, and UI interactions with Enhanced Error Logging

console.log('✓ [SITE] OpCentrix site.js loading with enhanced error logging...');

// **ENHANCED ERROR LOGGING SYSTEM**
// Centralized error tracking and logging for debugging and issue tracking

/**
 * Enhanced Error Logger - Central error tracking system
 * Provides comprehensive error tracking with context capture and server reporting
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
            category: category, // 'SITE', 'HTMX', 'FORM', 'MODAL', 'API', 'EDM', etc.
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
                online: navigator.onLine,
                cookieEnabled: navigator.cookieEnabled,
                language: navigator.language,
                platform: navigator.platform
            }
        };
        
        // Add to errors array
        this.errors.push(errorEntry);
        
        // Keep only last maxErrors entries
        if (this.errors.length > this.maxErrors) {
            this.errors = this.errors.slice(-this.maxErrors);
        }
        
        // Enhanced console logging
        console.group(`❌ [${category}] Error in ${operation} [${errorEntry.id}]`);
        console.error('Error Details:', errorEntry.error);
        console.warn('Context:', errorEntry.context);
        console.info('Browser Info:', errorEntry.browser);
        console.log('Full Error Entry:', errorEntry);
        console.groupEnd();
        
        // Store in localStorage for persistence
        this.persistErrors();
        
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
            const response = await fetch('/Api/ErrorLog', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(errorEntry)
            });
            
            if (response.ok) {
                const result = await response.json();
                console.log(`📡 [SITE] Error sent to server with operation ID: ${result.operationId}`);
            } else {
                console.warn(`📡 [SITE] Failed to send error to server: ${response.status}`);
            }
        } catch (e) {
            console.warn('📡 [SITE] Failed to send error to server:', e);
        }
    },
    
    /**
     * Persist errors to localStorage
     */
    persistErrors() {
        try {
            localStorage.setItem('opcentrix_errors', JSON.stringify(this.errors.slice(-10)));
        } catch (e) {
            console.warn('💾 [SITE] Failed to store errors in localStorage:', e);
        }
    },
    
    /**
     * Load errors from localStorage
     */
    loadPersistedErrors() {
        try {
            const stored = localStorage.getItem('opcentrix_errors');
            if (stored) {
                const parsed = JSON.parse(stored);
                if (Array.isArray(parsed)) {
                    this.errors = parsed;
                }
            }
        } catch (e) {
            console.warn('💾 [SITE] Failed to load errors from localStorage:', e);
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
            },
            browserInfo: {
                userAgent: navigator.userAgent,
                viewport: `${window.innerWidth}x${window.innerHeight}`,
                online: navigator.onLine,
                language: navigator.language
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
    },
    
    /**
     * Export errors as JSON for debugging
     */
    exportErrors() {
        const data = {
            exported: new Date().toISOString(),
            errors: this.errors,
            report: this.getErrorReport()
        };
        
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `opcentrix-errors-${new Date().toISOString().split('T')[0]}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        console.log('📄 [SITE] Errors exported to file');
    }
};

/**
 * PAGE MONITORING SYSTEM
 * Comprehensive monitoring for production debugging
 */
window.OpCentrixPageMonitor = {
    currentPage: null,
    pageStartTime: null,
    interactions: [],
    performanceMetrics: {},
    
    /**
     * Initialize page monitoring
     */
    init() {
        this.currentPage = this.getPageInfo();
        this.pageStartTime = performance.now();
        this.interactions = [];
        this.performanceMetrics = {};
        
        console.log('🔍 [MONITOR] Page monitoring started:', this.currentPage.path);
        
        // Monitor all clicks
        document.addEventListener('click', (e) => this.logInteraction('click', e), true);
        
        // Monitor all form submissions
        document.addEventListener('submit', (e) => this.logInteraction('submit', e), true);
        
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
        
        // Monitor resource loading errors
        window.addEventListener('error', (e) => {
            if (e.target !== window) {
                this.logResourceError(e);
            }
        }, true);
        
        // Monitor unhandled promise rejections
        window.addEventListener('unhandledrejection', (e) => {
            this.logPromiseRejection(e);
        });
        
        // Monitor performance
        this.monitorPerformance();
        
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
            url: window.location.href,
            referrer: document.referrer
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
            details: this.getEventDetails(event),
            timeFromPageLoad: performance.now() - this.pageStartTime
        };
        
        this.interactions.push(interaction);
        
        // Keep only last 50 interactions
        if (this.interactions.length > 50) {
            this.interactions = this.interactions.slice(-50);
        }
        
        console.log(`👆 [MONITOR] ${type.toUpperCase()}:`, interaction.element.identifier, interaction.details);
        
        // Check for potential issues
        this.checkForIssues(interaction);
        
        // Store interactions for debugging
        this.persistInteractions();
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
            value: element.value || null,
            dataset: element.dataset ? Object.assign({}, element.dataset) : {}
        };
    },
    
    /**
     * Get event details
     */
    getEventDetails(event) {
        const details = {
            type: event.type,
            timestamp: event.timeStamp,
            bubbles: event.bubbles,
            cancelable: event.cancelable
        };
        
        // Add specific details based on event type
        if (event.detail?.xhr) {
            details.xhr = {
                status: event.detail.xhr.status,
                statusText: event.detail.xhr.statusText,
                responseLength: event.detail.xhr.responseText?.length || 0,
                url: event.detail.xhr.responseURL
            };
        }
        
        if (event.detail?.pathInfo) {
            details.pathInfo = event.detail.pathInfo;
        }
        
        // Mouse events
        if (event.clientX !== undefined) {
            details.mouse = {
                x: event.clientX,
                y: event.clientY,
                button: event.button
            };
        }
        
        // Keyboard events
        if (event.key) {
            details.keyboard = {
                key: event.key,
                code: event.code,
                altKey: event.altKey,
                ctrlKey: event.ctrlKey,
                shiftKey: event.shiftKey
            };
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
            timeOnPage: performance.now() - this.pageStartTime,
            interactions: this.interactions.length,
            performance: this.getPerformanceSnapshot()
        };
        
        console.log(`📄 [MONITOR] Page ${activity}:`, activityLog);
        
        // Store page activity
        this.persistPageActivity(activityLog);
    },
    
    /**
     * Log resource loading errors
     */
    logResourceError(event) {
        const resourceError = {
            type: 'resource-error',
            timestamp: new Date().toISOString(),
            resource: {
                src: event.target.src || event.target.href,
                type: event.target.tagName,
                id: event.target.id,
                className: event.target.className
            },
            page: this.currentPage.path
        };
        
        console.error('📁 [MONITOR] Resource loading error:', resourceError);
        
        // Log as error
        OpCentrixErrorLogger.logError('RESOURCE', 'loadResource', {
            message: `Failed to load resource: ${resourceError.resource.src}`,
            stack: null
        }, resourceError);
    },
    
    /**
     * Log unhandled promise rejections
     */
    logPromiseRejection(event) {
        const rejectionError = {
            type: 'promise-rejection',
            timestamp: new Date().toISOString(),
            reason: event.reason,
            page: this.currentPage.path
        };
        
        console.error('🚫 [MONITOR] Unhandled promise rejection:', rejectionError);
        
        // Log as error
        OpCentrixErrorLogger.logError('PROMISE', 'unhandledRejection', {
            message: `Unhandled promise rejection: ${event.reason}`,
            stack: event.reason?.stack || null
        }, rejectionError);
    },
    
    /**
     * Monitor performance metrics
     */
    monitorPerformance() {
        if ('performance' in window) {
            // Page load performance
            window.addEventListener('load', () => {
                setTimeout(() => {
                    const navigation = performance.getEntriesByType('navigation')[0];
                    if (navigation) {
                        this.performanceMetrics.pageLoad = {
                            domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
                            loadComplete: navigation.loadEventEnd - navigation.loadEventStart,
                            totalTime: navigation.loadEventEnd - navigation.navigationStart,
                            dnsLookup: navigation.domainLookupEnd - navigation.domainLookupStart,
                            tcpConnect: navigation.connectEnd - navigation.connectStart,
                            serverResponse: navigation.responseEnd - navigation.requestStart
                        };
                        
                        console.log('⚡ [MONITOR] Page performance:', this.performanceMetrics.pageLoad);
                    }
                }, 100);
            });
            
            // Monitor long tasks
            if ('PerformanceObserver' in window) {
                try {
                    const observer = new PerformanceObserver((list) => {
                        for (const entry of list.getEntries()) {
                            if (entry.duration > 50) { // Tasks longer than 50ms
                                console.warn('🐌 [MONITOR] Long task detected:', {
                                    duration: entry.duration,
                                    startTime: entry.startTime,
                                    name: entry.name
                                });
                            }
                        }
                    });
                    observer.observe({ entryTypes: ['longtask'] });
                } catch (e) {
                    console.warn('Performance observer not supported for longtask');
                }
            }
        }
    },
    
    /**
     * Get performance snapshot
     */
    getPerformanceSnapshot() {
        if (!('performance' in window)) return {};
        
        return {
            now: performance.now(),
            memory: performance.memory ? {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit
            } : null,
            navigation: performance.navigation ? {
                type: performance.navigation.type,
                redirectCount: performance.navigation.redirectCount
            } : null
        };
    },
    
    /**
     * Check for potential issues in interactions
     */
    checkForIssues(interaction) {
        // Check for rapid clicking (potential double-click issues)
        const recentInteractions = this.interactions.slice(-3);
        const rapidClicks = recentInteractions.filter(i => 
            i.type === 'click' && 
            i.timestamp > Date.now() - 1000 && 
            i.element.identifier === interaction.element.identifier
        );
        
        if (rapidClicks.length >= 2) {
            console.warn('⚠️ [MONITOR] Potential rapid clicking detected:', interaction.element.identifier);
        }
        
        // Check for HTMX errors
        if (interaction.type === 'htmx-error') {
            console.error('💥 [MONITOR] HTMX error detected:', interaction.details);
            
            OpCentrixErrorLogger.logError('HTMX', 'requestError', {
                message: `HTMX request failed: ${interaction.details.xhr?.status}`,
                stack: null
            }, interaction);
        }
    },
    
    /**
     * Persist interactions to localStorage
     */
    persistInteractions() {
        try {
            localStorage.setItem('opcentrix_interactions', JSON.stringify(this.interactions.slice(-20)));
        } catch (e) {
            console.warn('💾 [MONITOR] Failed to store interactions:', e);
        }
    },
    
    /**
     * Persist page activity to localStorage
     */
    persistPageActivity(activity) {
        try {
            const activities = JSON.parse(localStorage.getItem('opcentrix_page_activities') || '[]');
            activities.unshift(activity);
            localStorage.setItem('opcentrix_page_activities', JSON.stringify(activities.slice(0, 10)));
        } catch (e) {
            console.warn('💾 [MONITOR] Failed to store page activity:', e);
        }
    },
    
    /**
     * Get monitoring report
     */
    getMonitoringReport() {
        return {
            currentPage: this.currentPage,
            timeOnPage: performance.now() - this.pageStartTime,
            interactionCount: this.interactions.length,
            recentInteractions: this.interactions.slice(-10),
            performance: this.performanceMetrics,
            errors: OpCentrixErrorLogger.errors.length
        };
    }
};

/**
 * SAFE EXECUTION WRAPPER
 * Wraps functions with error handling and logging
 */
window.SafeExecute = {
    call: function(functionName, ...args) {
        const operationId = this.generateOperationId();
        console.log(`🎯 [SAFE-${operationId}] Executing: ${functionName}`);
        
        try {
            let targetFunction = null;
            
            // Try to find the function in various scopes
            if (typeof window[functionName] === 'function') {
                targetFunction = window[functionName];
            } else if (typeof this[functionName] === 'function') {
                targetFunction = this[functionName];
            } else {
                // Look for the function in global scope or specific objects
                const parts = functionName.split('.');
                let obj = window;
                for (const part of parts) {
                    obj = obj[part];
                    if (!obj) break;
                }
                if (typeof obj === 'function') {
                    targetFunction = obj;
                }
            }
            
            if (targetFunction) {
                const result = targetFunction.apply(this, args);
                console.log(`✅ [SAFE-${operationId}] ${functionName} completed successfully`);
                return result;
            } else {
                throw new Error(`Function ${functionName} not found`);
            }
        } catch (error) {
            this.logError(operationId, functionName, error, args);
            this.showUserFriendlyError(functionName, error);
            return false;
        }
    },
    
    generateOperationId: function() {
        return Math.random().toString(36).substr(2, 8);
    },
    
    logError: function(operationId, functionName, error, args) {
        OpCentrixErrorLogger.logError('SAFE', functionName, error, {
            operationId: operationId,
            arguments: args,
            callStack: new Error().stack
        });
    },
    
    showUserFriendlyError: function(functionName, error) {
        const message = `Unable to execute ${functionName}. Please try again or contact support if the problem persists.`;
        this.showNotification('error', message, error.message);
    },
    
    showNotification: function(type, message, detail) {
        const notification = document.createElement('div');
        notification.className = `fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg max-w-md ${
            type === 'error' ? 'bg-red-500 text-white' : 
            type === 'warning' ? 'bg-yellow-500 text-black' : 
            'bg-green-500 text-white'
        }`;
        
        notification.innerHTML = `
            <div class="flex items-start">
                <div class="flex-shrink-0">
                    ${type === 'error' ? '❌' : type === 'warning' ? '⚠️' : '✅'}
                </div>
                <div class="ml-3">
                    <p class="font-semibold">${message}</p>
                    ${detail ? `<p class="text-sm opacity-90 mt-1">${detail}</p>` : ''}
                </div>
                <button onclick="this.parentElement.parentElement.remove()" class="ml-auto pl-3 text-lg opacity-70 hover:opacity-100">×</button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notification.parentElement) {
                notification.remove();
            }
        }, 5000);
    }
};

// **GLOBAL ERROR HANDLERS**
window.addEventListener('error', function(event) {
    OpCentrixErrorLogger.logError('GLOBAL', 'window.error', {
        message: event.message,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
        stack: event.error?.stack
    });
});

window.addEventListener('unhandledrejection', function(event) {
    OpCentrixErrorLogger.logError('PROMISE', 'unhandledRejection', {
        message: event.reason?.message || event.reason,
        stack: event.reason?.stack
    });
});

// **DEBUGGING HELPERS**
window.debugErrors = function() {
    console.group('🔍 [DEBUG] OpCentrix Error Report');
    console.log(OpCentrixErrorLogger.getErrorReport());
    console.groupEnd();
};

window.debugMonitoring = function() {
    console.group('🔍 [DEBUG] OpCentrix Monitoring Report');
    console.log(OpCentrixPageMonitor.getMonitoringReport());
    console.groupEnd();
};

window.clearErrors = function() {
    OpCentrixErrorLogger.clearErrors();
    console.log('✅ [DEBUG] Error log cleared');
};

window.exportErrors = function() {
    OpCentrixErrorLogger.exportErrors();
};

// **INITIALIZATION**
document.addEventListener('DOMContentLoaded', function() {
    try {
        // Load persisted errors
        OpCentrixErrorLogger.loadPersistedErrors();
        
        // Initialize page monitoring
        OpCentrixPageMonitor.init();
        
        console.log('✅ [SITE] Enhanced error logging and monitoring system initialized');
        console.log('🔧 [DEBUG] Available debug commands: debugErrors(), debugMonitoring(), clearErrors(), exportErrors()');
        
    } catch (error) {
        console.error('💥 [SITE] Failed to initialize error logging system:', error);
    }
});

console.log('✅ [SITE] OpCentrix site.js loaded successfully with comprehensive error logging');