// OpCentrix Site-wide JavaScript Functions - SAFE VERSION
// CRITICAL FIX: Simplified version to prevent memory issues

console.log('✓ [SITE] OpCentrix site.js loading - SAFE MODE');

// DISABLED: All complex monitoring and logging systems
// These were causing memory leaks and Out of Memory errors

// GLOBAL SAFE EXECUTE WRAPPER - Available across all pages
// This prevents ReferenceError: function is not defined issues
window.SafeExecute = window.SafeExecute || {
    call: function(functionName, ...args) {
        const operationId = this.generateOperationId();
        console.log(`🔧 [SAFE-${operationId}] Executing: ${functionName}`);
        
        try {
            // Try window scope first
            if (typeof window[functionName] === 'function') {
                const result = window[functionName].apply(this, args);
                console.log(`✅ [SAFE-${operationId}] ${functionName} completed successfully`);
                return result;
            }
            
            // Try specific page objects (EDMOperations, etc.)
            const pageObjects = ['EDMOperations', 'PrintOperations', 'ShippingOperations', 'CoatingOperations'];
            for (const objName of pageObjects) {
                if (window[objName] && typeof window[objName][functionName] === 'function') {
                    const result = window[objName][functionName].apply(this, args);
                    console.log(`✅ [SAFE-${operationId}] ${functionName} completed successfully`);
                    return result;
                }
            }
            
            throw new Error(`Function ${functionName} not found in global scope or page objects`);
        } catch (error) {
            this.logError(operationId, functionName, error);
            this.showUserFriendlyError(functionName, error);
            return false;
        }
    },
    
    generateOperationId: function() {
        return Math.random().toString(36).substr(2, 6);
    },
    
    logError: function(operationId, functionName, error) {
        console.error(`❌ [SAFE-${operationId}] Error in ${functionName}:`, error);
        
        // Store basic error info in localStorage (limited to prevent memory issues)
        try {
            const errors = JSON.parse(localStorage.getItem('safeexecute_errors') || '[]');
            errors.push({
                id: operationId,
                function: functionName,
                error: error.message,
                timestamp: new Date().toISOString()
            });
            localStorage.setItem('safeexecute_errors', JSON.stringify(errors.slice(-5))); // Keep only last 5
        } catch (e) {
            console.warn('Failed to store error in localStorage:', e);
        }
    },
    
    showUserFriendlyError: function(functionName, error) {
        const friendlyMessages = {
            'generatePrintout': 'Unable to generate the printout. Please check that all required fields are filled out.',
            'printAndStore': 'Unable to print and store. Please try generating the printout again.',
            'clearForm': 'Unable to clear the form. Please refresh the page and try again.',
            'viewStoredLogs': 'Unable to view stored data. Please check your browser settings.',
            'closeModal': 'Unable to close the modal. Please refresh the page.'
        };
        
        const message = friendlyMessages[functionName] || `An error occurred with ${functionName}. Please try again or contact support.`;
        
        // Simple notification system
        this.showNotification('error', message);
    },
    
    showNotification: function(type, message, detail) {
        // Create a simple notification
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

// Simple, safe utility functions only
window.OpCentrix = {
    // Simple notification system
    showNotification: function(type, message) {
        console.log(`[${type.toUpperCase()}] ${message}`);
        
        // Use SafeExecute notification system
        if (window.SafeExecute) {
            window.SafeExecute.showNotification(type, message);
        } else {
            // Fallback
            if (type === 'error') {
                alert(`Error: ${message}`);
            }
        }
    },
    
    // Safe function execution (alias to SafeExecute)
    safeCall: function(functionName, ...args) {
        if (window.SafeExecute) {
            return window.SafeExecute.call(functionName, ...args);
        } else {
            // Fallback
            try {
                if (typeof window[functionName] === 'function') {
                    return window[functionName].apply(this, args);
                } else {
                    console.error(`Function ${functionName} not found`);
                    return false;
                }
            } catch (error) {
                console.error(`Error executing ${functionName}:`, error);
                return false;
            }
        }
    }
};

// DISABLED: Complex error logging system
// DISABLED: Page monitoring system  
// DISABLED: Performance monitoring
// DISABLED: Activity tracking
// DISABLED: Event listeners that could accumulate

// Simple initialization only
document.addEventListener('DOMContentLoaded', function() {
    console.log('✅ [SITE] OpCentrix site.js loaded - SAFE MODE ACTIVE');
    console.log('🛡️ [SITE] Complex monitoring disabled to prevent memory issues');
    console.log('🔧 [SITE] Global SafeExecute system available');
});

// Enhanced navigation dropdown functionality
document.addEventListener('DOMContentLoaded', function() {
    // Add smooth scrolling to dropdown menus
    const dropdownMenus = document.querySelectorAll('.dropdown-menu[style*="overflow-y: auto"]');
    
    dropdownMenus.forEach(menu => {
        // Add scroll indicator shadows
        menu.addEventListener('scroll', function() {
            const scrollTop = this.scrollTop;
            const scrollHeight = this.scrollHeight;
            const clientHeight = this.clientHeight;
            
            // Add top shadow when scrolled down
            if (scrollTop > 10) {
                this.style.boxShadow = '0 10px 25px rgba(0, 0, 0, 0.15), inset 0 10px 10px -10px rgba(0, 0, 0, 0.1)';
            } else {
                this.style.boxShadow = '0 10px 25px rgba(0, 0, 0, 0.15)';
            }
            
            // Add bottom shadow when not at bottom
            if (scrollTop + clientHeight < scrollHeight - 10) {
                this.style.borderBottom = '1px solid rgba(0, 0, 0, 0.1)';
            } else {
                this.style.borderBottom = 'none';
            }
        });
    });
    
    // Enhanced dropdown toggle behavior
    const dropdownToggles = document.querySelectorAll('[data-bs-toggle="dropdown"]');
    
    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', function() {
            // Reset scroll position when opening dropdown
            const dropdownMenu = this.nextElementSibling;
            if (dropdownMenu && dropdownMenu.classList.contains('dropdown-menu')) {
                setTimeout(() => {
                    dropdownMenu.scrollTop = 0;
                }, 50);
            }
        });
    });
    
    // Add hover effects for better UX
    const dropdownItems = document.querySelectorAll('.dropdown-item');
    
    dropdownItems.forEach(item => {
        item.addEventListener('mouseenter', function() {
            this.style.transform = 'translateX(3px)';
        });
        
        item.addEventListener('mouseleave', function() {
            this.style.transform = 'translateX(0)';
        });
    });
});

console.log('✅ [SITE] OpCentrix site.js loaded successfully - SAFE MODE');