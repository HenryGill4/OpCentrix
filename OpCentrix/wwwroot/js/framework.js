/**
 * OpCentrix Modern JavaScript Framework
 * A comprehensive, production-ready JavaScript architecture for Razor Pages
 * Following modern best practices for maintainability, performance, and reliability
 */

// ==================================================
// CORE FRAMEWORK - DEPENDENCY INJECTION PATTERN
// ==================================================

window.OpCentrix = (function() {
    'use strict';

    // Framework configuration
    const CONFIG = {
        debug: true, // Set to false in production
        retryAttempts: 3,
        retryDelay: 100,
        defaultTimeout: 5000,
        errorReportingUrl: '/Api/ErrorLog'
    };

    // Dependency registry
    const dependencies = new Map();
    const modules = new Map();
    const eventListeners = new Map();

    // ==================================================
    // CORE UTILITIES
    // ==================================================

    const Utils = {
        /**
         * Generate unique ID for tracking operations
         */
        generateId: () => `op_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,

        /**
         * Safe element selector with logging
         */
        getElement: (selector, context = document) => {
            try {
                const element = context.querySelector(selector);
                if (!element && CONFIG.debug) {
                    console.warn(`?? [UTILS] Element not found: ${selector}`);
                }
                return element;
            } catch (error) {
                console.error(`? [UTILS] Invalid selector: ${selector}`, error);
                return null;
            }
        },

        /**
         * Safe element selector for multiple elements
         */
        getElements: (selector, context = document) => {
            try {
                return Array.from(context.querySelectorAll(selector));
            } catch (error) {
                console.error(`? [UTILS] Invalid selector: ${selector}`, error);
                return [];
            }
        },

        /**
         * Debounce function execution
         */
        debounce: (func, wait) => {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },

        /**
         * Throttle function execution
         */
        throttle: (func, limit) => {
            let inThrottle;
            return function() {
                const args = arguments;
                const context = this;
                if (!inThrottle) {
                    func.apply(context, args);
                    inThrottle = true;
                    setTimeout(() => inThrottle = false, limit);
                }
            };
        },

        /**
         * Safe JSON parse with error handling
         */
        parseJSON: (jsonString, defaultValue = null) => {
            try {
                return JSON.parse(jsonString);
            } catch (error) {
                console.warn(`?? [UTILS] JSON parse failed:`, error);
                return defaultValue;
            }
        },

        /**
         * Escape HTML to prevent XSS
         */
        escapeHtml: (text) => {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    };

    // ==================================================
    // DEPENDENCY MANAGEMENT SYSTEM
    // ==================================================

    const DependencyManager = {
        /**
         * Register a dependency
         */
        register: (name, factory, dependencies = []) => {
            if (modules.has(name)) {
                console.warn(`?? [DEP] Module ${name} already registered`);
                return;
            }

            modules.set(name, {
                factory,
                dependencies,
                instance: null,
                loading: false,
                loaded: false
            });

            console.log(`?? [DEP] Registered module: ${name}`);
        },

        /**
         * Resolve a dependency with async loading
         */
        resolve: async (name) => {
            const moduleInfo = modules.get(name);
            if (!moduleInfo) {
                throw new Error(`Module ${name} not found`);
            }

            if (moduleInfo.loaded) {
                return moduleInfo.instance;
            }

            if (moduleInfo.loading) {
                // Wait for loading to complete
                return new Promise((resolve, reject) => {
                    const checkLoaded = () => {
                        if (moduleInfo.loaded) {
                            resolve(moduleInfo.instance);
                        } else if (!moduleInfo.loading) {
                            reject(new Error(`Module ${name} failed to load`));
                        } else {
                            setTimeout(checkLoaded, 50);
                        }
                    };
                    checkLoaded();
                });
            }

            moduleInfo.loading = true;

            try {
                // Resolve dependencies first
                const resolvedDeps = await Promise.all(
                    moduleInfo.dependencies.map(dep => this.resolve(dep))
                );

                // Create instance
                moduleInfo.instance = await moduleInfo.factory(...resolvedDeps);
                moduleInfo.loaded = true;
                moduleInfo.loading = false;

                console.log(`? [DEP] Loaded module: ${name}`);
                return moduleInfo.instance;
            } catch (error) {
                moduleInfo.loading = false;
                console.error(`? [DEP] Failed to load module ${name}:`, error);
                throw error;
            }
        },

        /**
         * Check if dependency is available
         */
        isAvailable: (name) => {
            const moduleInfo = modules.get(name);
            return moduleInfo && moduleInfo.loaded;
        },

        /**
         * Get all registered modules
         */
        getRegistered: () => Array.from(modules.keys())
    };

    // ==================================================
    // EVENT MANAGEMENT SYSTEM
    // ==================================================

    const EventManager = {
        /**
         * Add event listener with automatic cleanup
         */
        on: (element, event, handler, options = {}) => {
            if (typeof element === 'string') {
                element = Utils.getElement(element);
            }

            if (!element) {
                console.warn(`?? [EVENTS] Cannot add listener: element not found`);
                return null;
            }

            const wrappedHandler = async (e) => {
                try {
                    await handler(e);
                } catch (error) {
                    console.error(`? [EVENTS] Handler error for ${event}:`, error);
                    ErrorReporter.report(error, { event, element: element.tagName });
                }
            };

            element.addEventListener(event, wrappedHandler, options);

            const listenerId = Utils.generateId();
            eventListeners.set(listenerId, {
                element,
                event,
                handler: wrappedHandler,
                options
            });

            return listenerId;
        },

        /**
         * Remove specific event listener
         */
        off: (listenerId) => {
            const listener = eventListeners.get(listenerId);
            if (listener) {
                listener.element.removeEventListener(
                    listener.event,
                    listener.handler,
                    listener.options
                );
                eventListeners.delete(listenerId);
                return true;
            }
            return false;
        },

        /**
         * Delegate events for dynamic content
         */
        delegate: (container, selector, event, handler) => {
            const containerElement = typeof container === 'string' 
                ? Utils.getElement(container) 
                : container;

            if (!containerElement) {
                console.warn(`?? [EVENTS] Delegate container not found`);
                return null;
            }

            return this.on(containerElement, event, async (e) => {
                const target = e.target.closest(selector);
                if (target) {
                    await handler(e, target);
                }
            });
        },

        /**
         * Clear all event listeners for cleanup
         */
        clearAll: () => {
            eventListeners.forEach((listener, id) => {
                this.off(id);
            });
            console.log(`?? [EVENTS] Cleared ${eventListeners.size} listeners`);
        }
    };

    // ==================================================
    // ERROR REPORTING SYSTEM
    // ==================================================

    const ErrorReporter = {
        /**
         * Report error with context
         */
        report: async (error, context = {}) => {
            const errorEntry = {
                id: Utils.generateId(),
                timestamp: new Date().toISOString(),
                message: error.message || String(error),
                stack: error.stack,
                url: window.location.href,
                userAgent: navigator.userAgent,
                context
            };

            console.error(`?? [ERROR] ${errorEntry.id}:`, errorEntry);

            // Send to server if configured
            if (CONFIG.errorReportingUrl) {
                try {
                    await fetch(CONFIG.errorReportingUrl, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(errorEntry)
                    });
                } catch (fetchError) {
                    console.warn(`?? [ERROR] Failed to report error:`, fetchError);
                }
            }

            return errorEntry.id;
        }
    };

    // ==================================================
    // PUBLIC API
    // ==================================================

    return {
        // Core utilities
        Utils,
        DependencyManager,
        EventManager,
        ErrorReporter,
        CONFIG,

        // Framework methods
        module: DependencyManager.register,
        use: DependencyManager.resolve,
        on: EventManager.on,
        off: EventManager.off,
        delegate: EventManager.delegate,

        // Lifecycle methods
        ready: (callback) => {
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', callback);
            } else {
                callback();
            }
        },

        // Debug utilities
        debug: {
            getModules: DependencyManager.getRegistered,
            getListeners: () => Array.from(eventListeners.keys()),
            clearListeners: EventManager.clearAll
        }
    };

})();

// ==================================================
// AUTO-INITIALIZATION
// ==================================================

OpCentrix.ready(() => {
    console.log('?? [FRAMEWORK] OpCentrix JavaScript Framework initialized');
    
    // Global error handler
    window.addEventListener('error', (e) => {
        OpCentrix.ErrorReporter.report(e.error || new Error(e.message), {
            filename: e.filename,
            lineno: e.lineno,
            colno: e.colno
        });
    });

    // Unhandled promise rejection handler
    window.addEventListener('unhandledrejection', (e) => {
        OpCentrix.ErrorReporter.report(
            new Error(`Unhandled Promise Rejection: ${e.reason}`),
            { reason: e.reason }
        );
    });
});

console.log('? [FRAMEWORK] OpCentrix core framework loaded');