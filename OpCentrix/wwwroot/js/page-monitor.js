// Enhanced Page Load Error Monitoring
// Automatically monitors all pages for load errors and runtime issues

console.log('?? [PAGE-MONITOR] Enhanced page monitoring script loading...');

// Enhanced page load monitoring
(function() {
    'use strict';
    
    const PAGE_MONITOR_VERSION = '1.0.0';
    console.log(`?? [PAGE-MONITOR] Version ${PAGE_MONITOR_VERSION} initializing...`);
    
    // Page load error monitoring
    window.addEventListener('load', function() {
        const operationId = Date.now().toString(36);
        console.log(`?? [PAGE-MONITOR-${operationId}] Page fully loaded: ${window.location.pathname}`);
        
        // Check for missing resources
        checkMissingResources(operationId);
        
        // Check for JavaScript errors in console
        monitorConsoleErrors(operationId);
        
        // Check for broken forms
        validateForms(operationId);
        
        // Check for broken links
        validateLinks(operationId);
        
        // Monitor HTMX if available
        monitorHTMX(operationId);
        
        // Monitor for authentication issues
        checkAuthenticationStatus(operationId);
    });
    
    function checkMissingResources(operationId) {
        try {
            // Check for missing CSS files
            const stylesheets = document.querySelectorAll('link[rel="stylesheet"]');
            stylesheets.forEach((link, index) => {
                if (!link.sheet) {
                    console.error(`?? [PAGE-MONITOR-${operationId}] Missing CSS: ${link.href}`);
                    if (window.OpCentrixErrorLogger) {
                        window.OpCentrixErrorLogger.logError('RESOURCE', 'missingCSS', 
                            new Error(`CSS file failed to load: ${link.href}`), {
                                resourceUrl: link.href,
                                resourceIndex: index
                            });
                    }
                }
            });
            
            // Check for missing images
            const images = document.querySelectorAll('img');
            images.forEach((img, index) => {
                if (img.naturalWidth === 0 && img.naturalHeight === 0 && img.src) {
                    console.error(`?? [PAGE-MONITOR-${operationId}] Missing Image: ${img.src}`);
                    if (window.OpCentrixErrorLogger) {
                        window.OpCentrixErrorLogger.logError('RESOURCE', 'missingImage', 
                            new Error(`Image failed to load: ${img.src}`), {
                                resourceUrl: img.src,
                                resourceIndex: index,
                                altText: img.alt
                            });
                    }
                }
            });
            
            console.log(`? [PAGE-MONITOR-${operationId}] Resource check completed`);
        } catch (error) {
            console.error(`?? [PAGE-MONITOR-${operationId}] Error checking resources:`, error);
        }
    }
    
    function monitorConsoleErrors(operationId) {
        // Store original console.error
        const originalError = console.error;
        
        console.error = function(...args) {
            // Call original console.error
            originalError.apply(console, args);
            
            // Log to our error system
            if (window.OpCentrixErrorLogger) {
                const errorMessage = args.map(arg => 
                    typeof arg === 'object' ? JSON.stringify(arg) : String(arg)
                ).join(' ');
                
                window.OpCentrixErrorLogger.logError('CONSOLE', 'consoleError', 
                    new Error(errorMessage), {
                        originalArgs: args,
                        source: 'console.error'
                    });
            }
        };
        
        console.log(`?? [PAGE-MONITOR-${operationId}] Console error monitoring enabled`);
    }
    
    function validateForms(operationId) {
        try {
            const forms = document.querySelectorAll('form');
            console.log(`?? [PAGE-MONITOR-${operationId}] Checking ${forms.length} forms...`);
            
            forms.forEach((form, index) => {
                // Check for forms without action
                if (!form.action && !form.getAttribute('hx-post') && !form.getAttribute('hx-get')) {
                    console.warn(`?? [PAGE-MONITOR-${operationId}] Form ${index} has no action or HTMX attributes`);
                    if (window.OpCentrixErrorLogger) {
                        window.OpCentrixErrorLogger.logError('FORM', 'noAction', 
                            new Error(`Form has no action attribute: form ${index}`), {
                                formIndex: index,
                                formId: form.id,
                                formClass: form.className
                            });
                    }
                }
                
                // Check for submit buttons
                const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
                if (submitButtons.length === 0) {
                    console.warn(`?? [PAGE-MONITOR-${operationId}] Form ${index} has no submit button`);
                }
                
                // Check for required fields without labels
                const requiredFields = form.querySelectorAll('input[required], select[required], textarea[required]');
                requiredFields.forEach((field, fieldIndex) => {
                    const label = form.querySelector(`label[for="${field.id}"]`);
                    if (!label && !field.getAttribute('aria-label')) {
                        console.warn(`?? [PAGE-MONITOR-${operationId}] Required field ${field.name || fieldIndex} has no label`);
                    }
                });
            });
            
            console.log(`? [PAGE-MONITOR-${operationId}] Form validation completed`);
        } catch (error) {
            console.error(`?? [PAGE-MONITOR-${operationId}] Error validating forms:`, error);
        }
    }
    
    function validateLinks(operationId) {
        try {
            const links = document.querySelectorAll('a[href]');
            console.log(`?? [PAGE-MONITOR-${operationId}] Checking ${links.length} links...`);
            
            let brokenLinks = 0;
            
            links.forEach((link, index) => {
                const href = link.href;
                
                // Check for obviously broken links
                if (href === '#' || href === 'javascript:void(0)' || href === '') {
                    // These might be intentional, just log as info
                    console.info(`?? [PAGE-MONITOR-${operationId}] Placeholder link found: ${link.textContent.trim()}`);
                    return;
                }
                
                // Check for malformed URLs
                try {
                    new URL(href);
                } catch (urlError) {
                    console.error(`?? [PAGE-MONITOR-${operationId}] Malformed URL: ${href}`);
                    brokenLinks++;
                    
                    if (window.OpCentrixErrorLogger) {
                        window.OpCentrixErrorLogger.logError('LINK', 'malformedURL', 
                            new Error(`Malformed URL: ${href}`), {
                                linkText: link.textContent.trim(),
                                linkIndex: index,
                                href: href
                            });
                    }
                }
            });
            
            if (brokenLinks > 0) {
                console.warn(`?? [PAGE-MONITOR-${operationId}] Found ${brokenLinks} potentially broken links`);
            } else {
                console.log(`? [PAGE-MONITOR-${operationId}] All links appear valid`);
            }
        } catch (error) {
            console.error(`?? [PAGE-MONITOR-${operationId}] Error validating links:`, error);
        }
    }
    
    function monitorHTMX(operationId) {
        if (typeof htmx === 'undefined') {
            console.log(`?? [PAGE-MONITOR-${operationId}] HTMX not detected on this page`);
            return;
        }
        
        console.log(`?? [PAGE-MONITOR-${operationId}] HTMX detected, setting up monitoring...`);
        
        // Monitor HTMX errors
        document.body.addEventListener('htmx:responseError', function(e) {
            console.error(`?? [PAGE-MONITOR-${operationId}] HTMX Response Error:`, e.detail);
            
            if (window.OpCentrixErrorLogger) {
                window.OpCentrixErrorLogger.logError('HTMX', 'responseError', 
                    new Error(`HTMX response error: ${e.detail.xhr.status} ${e.detail.xhr.statusText}`), {
                        status: e.detail.xhr.status,
                        statusText: e.detail.xhr.statusText,
                        responseText: e.detail.xhr.responseText?.substring(0, 500),
                        target: e.target?.tagName
                    });
            }
        });
        
        // Monitor HTMX network errors
        document.body.addEventListener('htmx:sendError', function(e) {
            console.error(`?? [PAGE-MONITOR-${operationId}] HTMX Send Error:`, e.detail);
            
            if (window.OpCentrixErrorLogger) {
                window.OpCentrixErrorLogger.logError('HTMX', 'sendError', 
                    new Error(`HTMX send error: ${e.detail.error}`), {
                        error: e.detail.error,
                        target: e.target?.tagName
                    });
            }
        });
        
        console.log(`? [PAGE-MONITOR-${operationId}] HTMX monitoring enabled`);
    }
    
    function checkAuthenticationStatus(operationId) {
        try {
            // Check for authentication-related elements
            const loginLinks = document.querySelectorAll('a[href*="/Account/Login"], a[href*="/login"]');
            const logoutLinks = document.querySelectorAll('a[href*="/Account/Logout"], a[href*="/logout"]');
            
            if (loginLinks.length > 0 && logoutLinks.length > 0) {
                console.warn(`?? [PAGE-MONITOR-${operationId}] Both login and logout links found - possible authentication state issue`);
            }
            
            // Check for access denied indicators
            const accessDenied = document.body.textContent.toLowerCase().includes('access denied') ||
                               document.body.textContent.toLowerCase().includes('unauthorized');
            
            if (accessDenied) {
                console.error(`?? [PAGE-MONITOR-${operationId}] Access denied content detected`);
                
                if (window.OpCentrixErrorLogger) {
                    window.OpCentrixErrorLogger.logError('AUTH', 'accessDenied', 
                        new Error('Access denied content detected on page'), {
                            pageUrl: window.location.href,
                            userAgent: navigator.userAgent
                        });
                }
            }
            
            console.log(`? [PAGE-MONITOR-${operationId}] Authentication status check completed`);
        } catch (error) {
            console.error(`?? [PAGE-MONITOR-${operationId}] Error checking authentication:`, error);
        }
    }
    
    // Performance monitoring
    window.addEventListener('load', function() {
        setTimeout(() => {
            const operationId = Date.now().toString(36);
            const perfData = performance.getEntriesByType('navigation')[0];
            
            if (perfData) {
                const loadTime = perfData.loadEventEnd - perfData.loadEventStart;
                const domContentLoaded = perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart;
                
                console.log(`?? [PAGE-MONITOR-${operationId}] Performance - Load: ${loadTime}ms, DOM: ${domContentLoaded}ms`);
                
                // Log slow page loads
                if (loadTime > 3000) {
                    console.warn(`?? [PAGE-MONITOR-${operationId}] Slow page load detected: ${loadTime}ms`);
                    
                    if (window.OpCentrixErrorLogger) {
                        window.OpCentrixErrorLogger.logError('PERFORMANCE', 'slowPageLoad', 
                            new Error(`Page load time exceeded 3 seconds: ${loadTime}ms`), {
                                loadTime: loadTime,
                                domContentLoaded: domContentLoaded,
                                pageUrl: window.location.href
                            });
                    }
                }
            }
        }, 100);
    });
    
    console.log('? [PAGE-MONITOR] Enhanced page monitoring script loaded successfully');
})();