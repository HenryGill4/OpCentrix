// OpCentrix Site-wide JavaScript Functions - SAFE VERSION
// CRITICAL FIX: Simplified version to prevent memory issues

console.log('✓ [SITE] OpCentrix site.js loading - SAFE MODE');

// DISABLED: All complex monitoring and logging systems
// These were causing memory leaks and Out of Memory errors

// Simple, safe utility functions only
window.OpCentrix = {
    // Simple notification system
    showNotification: function(type, message) {
        console.log(`[${type.toUpperCase()}] ${message}`);
        
        // Simple alert for now
        if (type === 'error') {
            alert(`Error: ${message}`);
        }
    },
    
    // Safe function execution
    safeCall: function(functionName, ...args) {
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