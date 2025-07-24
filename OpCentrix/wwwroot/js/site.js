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
 * Enhanced wrapper function for safe execution with error logging
 */
function safeExecute(category, operation, fn, context = {}) {
    try {
        console.log(`→ [${category}] Starting ${operation}`);
        const result = fn();
        console.log(`✓ [${category}] ${operation} completed successfully`);
        return result;
    } catch (error) {
        const errorId = OpCentrixErrorLogger.logError(category, operation, error, context);
        console.error(`✗ [${category}] ${operation} failed with error ID: ${errorId}`);
        
        // Show user-friendly error notification
        if (typeof showErrorNotification === 'function') {
            showErrorNotification(`Operation failed: ${operation}. Error ID: ${errorId.slice(-8)}`, 8000);
        }
        
        return null;
    }
}

/**
 * Enhanced async wrapper function for safe execution with error logging
 */
async function safeExecuteAsync(category, operation, fn, context = {}) {
    try {
        console.log(`→ [${category}] Starting async ${operation}`);
        const result = await fn();
        console.log(`✓ [${category}] Async ${operation} completed successfully`);
        return result;
    } catch (error) {
        const errorId = OpCentrixErrorLogger.logError(category, operation, error, context);
        console.error(`✗ [${category}] Async ${operation} failed with error ID: ${errorId}`);
        
        // Show user-friendly error notification
        if (typeof showErrorNotification === 'function') {
            showErrorNotification(`Operation failed: ${operation}. Error ID: ${errorId.slice(-8)}`, 8000);
        }
        
        return null;
    }
}

// **ENHANCED GLOBAL FUNCTION DEFINITIONS WITH ERROR LOGGING**

/**
 * Updates the SLS Material field based on the Material dropdown selection
 * Called from: Admin/_PartForm.cshtml materialSelect onchange
 * Enhanced version with comprehensive error logging
 */
window.updateSlsMaterial_Enhanced = function() {
    return safeExecute('SITE', 'updateSlsMaterial', () => {
        const materialSelect = document.getElementById('materialSelect');
        const slsMaterialInput = document.getElementById('slsMaterialInput');
        
        if (!materialSelect) {
            throw new Error('Material select element not found (ID: materialSelect)');
        }
        
        if (!slsMaterialInput) {
            throw new Error('SLS Material input element not found (ID: slsMaterialInput)');
        }
        
        if (materialSelect.value) {
            slsMaterialInput.value = materialSelect.value;
            console.log('✓ [SITE] SLS Material updated to:', materialSelect.value);
        } else {
            slsMaterialInput.value = '';
            console.log('→ [SITE] SLS Material cleared');
        }
        
        return true;
    }, {
        elementIds: ['materialSelect', 'slsMaterialInput'],
        formContext: 'PartForm'
    });
};

// Create base function for backward compatibility
window.updateSlsMaterial = window.updateSlsMaterial_Enhanced;

// Store enhanced version for access by other parts of the system
window.updateSlsMaterial.enhanced = window.updateSlsMaterial_Enhanced;

/**
 * Updates the duration display fields based on EstimatedHours input
 * Called from: Admin/_PartForm.cshtml EstimatedHours onchange
 * Enhanced version with comprehensive error logging
 */
window.updateDurationDisplay_Enhanced = function() {
    return safeExecute('SITE', 'updateDurationDisplay', () => {
        const hoursInput = document.querySelector('input[name="EstimatedHours"]');
        const durationDisplay = document.getElementById('durationDisplay');
        const durationDays = document.getElementById('durationDays');
        
        if (!hoursInput) {
            throw new Error('EstimatedHours input element not found (selector: input[name="EstimatedHours"])');
        }
        
        if (!durationDisplay) {
            throw new Error('Duration display element not found (ID: durationDisplay)');
        }
        
        if (!durationDays) {
            throw new Error('Duration days element not found (ID: durationDays)');
        }
        
        const hours = parseFloat(hoursInput.value) || 0;
        
        if (hours > 0) {
            if (hours > 8760) { // More than a year
                throw new Error(`Invalid hours value: ${hours} (exceeds maximum of 8760 hours)`);
            }
            
            const days = Math.floor(hours / 24);
            const remainingHours = hours % 24;
            
            // Update duration display
            if (days > 0) {
                durationDisplay.value = `${days}d ${remainingHours.toFixed(1)}h`;
            } else {
                durationDisplay.value = `${hours.toFixed(1)}h`;
            }
            
            // Update duration days (work days, assuming 8-hour days)
            durationDays.value = Math.ceil(hours / 8);
            console.log('✓ [SITE] Duration updated:', durationDisplay.value);
        } else {
            durationDisplay.value = '';
            durationDays.value = '';
            console.log('→ [SITE] Duration cleared');
        }
        
        return true;
    }, {
        inputValue: document.querySelector('input[name="EstimatedHours"]')?.value,
        elementIds: ['durationDisplay', 'durationDays'],
        formContext: 'PartForm'
    });
};

// Create base function for backward compatibility
window.updateDurationDisplay = window.updateDurationDisplay_Enhanced;

// Store enhanced version for access by form scripts
window.updateDurationDisplay.enhanced = window.updateDurationDisplay_Enhanced;

/**
 * Shows loading state for forms
 * Called from: Various forms via HTMX hx-on::before-request
 */
window.showFormLoading = function() {
    return safeExecute('SITE', 'showFormLoading', () => {
        // Try multiple possible submit button IDs
        const submitBtn = document.getElementById('submitBtn') || 
                         document.getElementById('submit-job-btn') ||
                         document.querySelector('button[type="submit"]');
        
        const submitText = document.getElementById('submitText') || 
                          document.getElementById('submit-text');
        
        const submitSpinner = document.getElementById('submitSpinner') || 
                             document.getElementById('submit-spinner');
        
        const foundElements = {
            submitBtn: !!submitBtn,
            submitText: !!submitText,
            submitSpinner: !!submitSpinner
        };
        
        console.log('→ [SITE] Form loading elements found:', foundElements);
        
        if (submitBtn) {
            submitBtn.disabled = true;
            console.log('→ [SITE] Submit button disabled');
        } else {
            console.warn('⚠ [SITE] No submit button found');
        }
        
        if (submitText) {
            submitText.textContent = 'Saving...';
            console.log('→ [SITE] Submit text updated');
        }
        
        if (submitSpinner) {
            submitSpinner.classList.remove('hidden');
            console.log('→ [SITE] Spinner shown');
        }
        
        return foundElements;
    }, {
        formContext: 'FormLoading',
        elementSearch: ['submitBtn', 'submit-job-btn', 'button[type="submit"]']
    });
};

// Store enhanced version for access by form scripts
window.showFormLoading.enhanced = window.showFormLoading;

/**
 * Handles form responses from HTMX with enhanced error logging
 * Called from: Various forms via HTMX hx-on::after-request
 */
window.handleFormResponse = function(event) {
    return safeExecute('SITE', 'handleFormResponse', () => {
        console.log('→ [SITE] handleFormResponse called');
        
        if (!event) {
            throw new Error('Event parameter is null or undefined');
        }
        
        if (!event.detail) {
            throw new Error('Event detail is null or undefined');
        }
        
        const xhr = event.detail.xhr;
        if (!xhr) {
            throw new Error('XHR object is null or undefined in event detail');
        }
        
        console.log('→ [SITE] Response status:', xhr.status);
        console.log('→ [SITE] Response length:', xhr.responseText?.length || 0);
        
        // Reset loading state
        const submitBtn = document.getElementById('submitBtn') || 
                         document.getElementById('submit-job-btn') ||
                         document.querySelector('button[type="submit"]');
        
        const submitText = document.getElementById('submitText') || 
                          document.getElementById('submit-text');
        
        const submitSpinner = document.getElementById('submitSpinner') || 
                             document.getElementById('submit-spinner');
        
        if (submitBtn) {
            submitBtn.disabled = false;
            console.log('→ [SITE] Submit button re-enabled');
        }
        
        if (submitSpinner) {
            submitSpinner.classList.add('hidden');
            console.log('→ [SITE] Spinner hidden');
        }
        
        // Handle response content with error checking
        const response = xhr.responseText;
        
        if (!response) {
            console.warn('⚠ [SITE] Empty response received');
            return { status: 'empty', action: 'none' };
        }
        
        // Check if response contains validation errors (form reload)
        if (response.includes('ValidationErrors')) {
            console.log('→ [SITE] Validation errors found in response');
            return { status: 'validation_error', action: 'show_errors' };
        }
        
        // Check if response contains script (success/error response)
        if (response.includes('<script>')) {
            console.log('✓ [SITE] Script response received - executing');
            try {
                // Create a temporary div to extract and execute the script
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = response;
                const scripts = tempDiv.querySelectorAll('script');
                
                if (scripts.length === 0) {
                    console.warn('⚠ [SITE] No scripts found in response despite script tags');
                }
                
                scripts.forEach((script, index) => {
                    try {
                        console.log(`→ [SITE] Executing script ${index + 1}/${scripts.length}`);
                        eval(script.textContent);
                        console.log(`✓ [SITE] Script ${index + 1} executed successfully`);
                    } catch (scriptError) {
                        OpCentrixErrorLogger.logError('SITE', `executeResponseScript_${index}`, scriptError, {
                            scriptContent: script.textContent?.substring(0, 200),
                            scriptIndex: index,
                            totalScripts: scripts.length
                        });
                    }
                });
                
                return { status: 'script_executed', action: 'scripts_ran', scriptCount: scripts.length };
            } catch (processingError) {
                OpCentrixErrorLogger.logError('SITE', 'processScriptResponse', processingError, {
                    responseLength: response.length,
                    responsePreview: response.substring(0, 200)
                });
                return { status: 'script_error', action: 'processing_failed' };
            }
        }
        
        console.log('→ [SITE] Standard response handled');
        return { status: 'standard', action: 'processed' };
        
    }, {
        eventType: event?.type,
        responseStatus: event?.detail?.xhr?.status,
        responseLength: event?.detail?.xhr?.responseText?.length,
        formContext: 'FormResponse'
    });
};

// Store enhanced version for access by form scripts
window.handleFormResponse.enhanced = window.handleFormResponse;

/**
 * Enhanced Global notification system with error logging
 */
window.showNotification = function(message, type = 'info', duration = 5000) {
    return safeExecute('SITE', 'showNotification', () => {
        console.log(`→ [SITE] Showing ${type} notification:`, message);
        
        if (!message) {
            throw new Error('Notification message is empty or null');
        }
        
        if (typeof message !== 'string') {
            console.warn('⚠ [SITE] Non-string message converted:', message);
            message = String(message);
        }
        
        // CRITICAL FIX: Use fallback notification system directly to prevent infinite loop
        createFallbackNotification(message, type, duration);
        return { status: 'created', method: 'createFallbackNotification' };
        
    }, {
        messageLength: message?.length,
        notificationType: type,
        duration: duration
    });
};

/**
 * Success notification shortcut
 */
window.showSuccessNotification = function(message, duration = 4000) {
    // CRITICAL FIX: Call createFallbackNotification directly instead of showNotification
    return createFallbackNotification(message, 'success', duration);
};

/**
 * Error notification shortcut
 */
window.showErrorNotification = function(message, duration = 8000) {
    // CRITICAL FIX: Call createFallbackNotification directly instead of showNotification
    return createFallbackNotification(message, 'error', duration);
};

/**
 * Warning notification shortcut
 */
window.showWarningNotification = function(message, duration = 6000) {
    // CRITICAL FIX: Call createFallbackNotification directly instead of showNotification
    return createFallbackNotification(message, 'warning', duration);
};

/**
 * Creates a fallback notification when no other system is available
 */
function createFallbackNotification(message, type = 'info', duration = 5000) {
    return safeExecute('SITE', 'createFallbackNotification', () => {
        // Remove any existing notifications
        const existingNotifications = document.querySelectorAll('.opcentrix-notification');
        existingNotifications.forEach(n => n.remove());
        
        const notification = document.createElement('div');
        notification.className = `opcentrix-notification fixed top-20 right-4 px-6 py-4 rounded-lg shadow-lg z-50 text-white font-medium max-w-md transform transition-all duration-300 translate-x-full`;
        
        const colors = {
            success: '#10b981',
            error: '#ef4444',
            warning: '#f59e0b',
            info: '#3b82f6'
        };
        
        const icons = {
            success: '✓',
            error: '✗',
            warning: '⚠',
            info: 'ℹ'
        };
        
        notification.style.backgroundColor = colors[type] || colors.info;
        notification.innerHTML = `
            <div class="flex items-center">
                <span class="mr-2">${icons[type] || icons.info}</span>
                <span>${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" class="ml-4 text-white hover:text-gray-200">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
        `;
        
        if (!document.body) {
            throw new Error('Document body not available for notification injection');
        }
        
        document.body.appendChild(notification);
        
        // Animate in
        requestAnimationFrame(() => {
            notification.classList.remove('translate-x-full');
        });
        
        // Auto remove
        setTimeout(() => {
            notification.classList.add('translate-x-full', 'opacity-0');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        }, duration);
        
        console.log(`✓ [SITE] Fallback notification created: ${type}`);
        return notification;
        
    }, {
        notificationType: type,
        messageLength: message?.length,
        duration: duration,
        existingNotificationCount: document.querySelectorAll('.opcentrix-notification').length
    });
}

/**
 * Enhanced Modal management functions with error logging
 */
window.hideModal = function() {
    return safeExecute('SITE', 'hideModal', () => {
        console.log('✓ [SITE] hideModal called');
        
        // Try multiple modal container selectors
        const modalContainers = [
            document.getElementById('modal-container'),
            document.getElementById('modal-content'),
            document.querySelector('.modal-overlay'),
            document.querySelector('[id*="modal"]')
        ];
        
        let hiddenCount = 0;
        
        for (const modal of modalContainers) {
            if (modal) {
                modal.style.display = 'none';
                modal.classList.add('hidden');
                hiddenCount++;
                console.log('✓ [SITE] Modal hidden:', modal.id || modal.className);
            }
        }
        
        if (hiddenCount === 0) {
            console.warn('⚠ [SITE] No modal found to hide');
            return { status: 'not_found', hiddenCount: 0 };
        }
        
        // Restore body scrolling
        document.body.style.overflow = '';
        
        return { status: 'success', hiddenCount: hiddenCount };
        
    }, {
        modalSelectors: ['modal-container', 'modal-content', '.modal-overlay', '[id*="modal"]']
    });
};

window.showModal = function(modalId) {
    return safeExecute('SITE', 'showModal', () => {
        console.log('✓ [SITE] showModal called for:', modalId);
        
        if (!modalId) {
            throw new Error('Modal ID is required but not provided');
        }
        
        const modal = document.getElementById(modalId);
        if (!modal) {
            throw new Error(`Modal not found with ID: ${modalId}`);
        }
        
        modal.style.display = 'flex';
        modal.classList.remove('hidden');
        
        // Prevent bodyScrolling
        document.body.style.overflow = 'hidden';
        
        console.log('✓ [SITE] Modal shown:', modalId);
        return { status: 'success', modalId: modalId };
        
    }, {
        modalId: modalId,
        modalExists: !!document.getElementById(modalId)
    });
};

/**
 * Enhanced Generic form utilities with error logging
 */
window.resetForm = function(formId) {
    return safeExecute('SITE', 'resetForm', () => {
        if (!formId) {
            throw new Error('Form ID is required but not provided');
        }
        
        const form = document.getElementById(formId);
        if (!form) {
            throw new Error(`Form not found with ID: ${formId}`);
        }
        
        form.reset();
        console.log('→ [SITE] Form reset:', formId);
        return { status: 'success', formId: formId };
        
    }, {
        formId: formId,
        formExists: !!document.getElementById(formId)
    });
};

window.submitForm = function(formId) {
    return safeExecute('SITE', 'submitForm', () => {
        if (!formId) {
            throw new Error('Form ID is required but not provided');
        }
        
        const form = document.getElementById(formId);
        if (!form) {
            throw new Error(`Form not found with ID: ${formId}`);
        }
        
        form.submit();
        console.log('→ [SITE] Form submitted:', formId);
        return { status: 'success', formId: formId };
        
    }, {
        formId: formId,
        formExists: !!document.getElementById(formId)
    });
};

/**
 * Enhanced Loading indicator functions with error logging
 */
window.showLoadingIndicator = function(message = 'Loading...') {
    return safeExecute('SITE', 'showLoadingIndicator', () => {
        console.log('→ [SITE] Showing loading indicator:', message);
        
        // Remove existing indicator
        const existing = document.getElementById('global-loading-indicator');
        if (existing) {
            existing.remove();
            console.log('→ [SITE] Removed existing loading indicator');
        }
        
        const indicator = document.createElement('div');
        indicator.id = 'global-loading-indicator';
        indicator.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
        indicator.innerHTML = `
            <div class="bg-white rounded-lg p-6 flex items-center space-x-4 shadow-xl">
                <svg class="animate-spin h-8 w-8 text-blue-600" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <span class="text-gray-700 font-medium">${message}</span>
            </div>
        `;
        
        if (!document.body) {
            throw new Error('Document body not available for loading indicator injection');
        }
        
        document.body.appendChild(indicator);
        console.log('✓ [SITE] Loading indicator created');
        return { status: 'success', message: message };
        
    }, {
        message: message,
        hasExisting: !!document.getElementById('global-loading-indicator')
    });
};

window.hideLoadingIndicator = function() {
    return safeExecute('SITE', 'hideLoadingIndicator', () => {
        const indicator = document.getElementById('global-loading-indicator');
        if (indicator) {
            indicator.remove();
            console.log('✓ [SITE] Loading indicator hidden');
            return { status: 'success', removed: true };
        } else {
            console.log('✓ [SITE] No loading indicator to hide');
            return { status: 'not_found', removed: false };
        }
    });
};

/**
 * Enhanced initialization with comprehensive error logging
 */
function initializeSiteJS() {
    return safeExecute('SITE', 'initializeSiteJS', () => {
        console.log('🔧 [SITE] Initializing site-wide JavaScript...');
        
        // Ensure all global functions are properly bound (skip form-specific functions)
        const functions = ['showFormLoading', 'handleFormResponse', 'updateSlsMaterial', 'updateDurationDisplay'];
        const missingFunctions = [];
        
        functions.forEach(funcName => {
            if (typeof window[funcName] !== 'function') {
                missingFunctions.push(funcName);
                console.error(`❌ [SITE] ${funcName} function not properly defined!`);
            }
        });
        
        if (missingFunctions.length > 0) {
            console.warn(`⚠️ [SITE] Missing functions: ${missingFunctions.join(', ')} - will be defined by individual forms`);
        }
        
        // Set up enhanced global error handler
        window.addEventListener('error', function(event) {
            OpCentrixErrorLogger.logError('GLOBAL', 'javascriptError', event.error, {
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                element: event.target?.tagName || 'unknown'
            });
            
            // Show user-friendly error for critical failures
            if (event.message.includes('ReferenceError') || event.message.includes('updateSlsMaterial')) {
                if (typeof showErrorNotification === 'function') {
                    showErrorNotification('A JavaScript error occurred. Please refresh the page and try again.');
                }
            }
        });
        
        // Set up enhanced unhandled promise rejection handler
        window.addEventListener('unhandledrejection', function(event) {
            OpCentrixErrorLogger.logError('GLOBAL', 'unhandledPromiseRejection', event.reason, {
                promise: event.promise?.toString() || 'unknown'
            });
        });
        
        // Load previous errors from localStorage
        try {
            const storedErrors = localStorage.getItem('opcentrix_errors');
            if (storedErrors) {
                const errors = JSON.parse(storedErrors);
                OpCentrixErrorLogger.errors = [...OpCentrixErrorLogger.errors, ...errors];
                console.log(`🔧 [SITE] Loaded ${errors.length} previous errors from localStorage`);
            }
        } catch (e) {
            console.warn('Failed to load previous errors from localStorage:', e);
        }
        
        // Expose error debugging functions globally
        window.debugErrors = () => {
            console.group('🔍 OpCentrix Error Report');
            console.log(OpCentrixErrorLogger.getErrorReport());
            console.groupEnd();
        };
        
        window.clearErrors = () => OpCentrixErrorLogger.clearErrors();
        
        console.log('✅ [SITE] Site-wide JavaScript initialized successfully');
        return { status: 'success', functionsChecked: functions.length, errorsLoaded: OpCentrixErrorLogger.errors.length };
        
    }, {
        readyState: document.readyState,
        userAgent: navigator.userAgent,
        timestamp: new Date().toISOString()
    });
}

// Initialize when DOM is ready with error handling
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSiteJS);
} else {
    initializeSiteJS();
}

console.log('✓ [SITE] OpCentrix site.js loaded successfully');

// Export error logger for debugging
window.ErrorLogger = OpCentrixErrorLogger;