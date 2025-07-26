/**
 * OpCentrix Admin JavaScript Module
 * Standardized JavaScript functions for admin pages
 * Replaces inline event handlers and mixed jQuery/vanilla JS patterns
 */

// Admin Module - Vanilla JS for better performance
window.OpCentrixAdmin = (function() {
    'use strict';

    // Private variables
    let initialized = false;
    let loadingStates = new Map();
    let modalInstances = new Map();

    // Configuration
    const config = {
        alertDuration: 5000,
        loadingDelay: 200,
        animationDuration: 300,
        debounceDelay: 300
    };

    // **MODAL MANAGEMENT**
    const Modal = {
        /**
         * Show modal with proper state management
         * @param {string} modalId - The modal element ID
         * @param {Object} options - Modal configuration options
         */
        show(modalId, options = {}) {
            const operationId = generateOperationId();
            try {
                console.log(`?? [ADMIN-${operationId}] Opening modal: ${modalId}`);

                const modal = document.getElementById(modalId);
                if (!modal) {
                    throw new Error(`Modal not found: ${modalId}`);
                }

                // Handle Bootstrap modals
                if (modal.classList.contains('modal')) {
                    const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
                    modalInstances.set(modalId, bsModal);
                    bsModal.show();
                    console.log(`? [ADMIN-${operationId}] Bootstrap modal opened: ${modalId}`);
                    return;
                }

                // Handle Tailwind CSS modals
                if (modal.classList.contains('hidden')) {
                    modal.classList.remove('hidden');
                    modal.classList.add('flex');
                    modal.setAttribute('aria-hidden', 'false');
                    
                    // Focus management
                    const firstFocusable = modal.querySelector('input, button, select, textarea, [tabindex]:not([tabindex="-1"])');
                    if (firstFocusable) {
                        setTimeout(() => firstFocusable.focus(), 100);
                    }

                    // Prevent body scroll
                    document.body.style.overflow = 'hidden';
                    console.log(`? [ADMIN-${operationId}] Tailwind modal opened: ${modalId}`);
                }

                // Setup event listeners for this modal
                this.setupModalEvents(modalId);

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `showModal-${modalId}`, error, { operationId, modalId });
                Alert.error('Failed to open modal. Please try again.');
            }
        },

        /**
         * Hide modal with cleanup
         * @param {string} modalId - The modal element ID
         */
        hide(modalId) {
            const operationId = generateOperationId();
            try {
                console.log(`?? [ADMIN-${operationId}] Closing modal: ${modalId}`);

                const modal = document.getElementById(modalId);
                if (!modal) {
                    console.warn(`?? [ADMIN-${operationId}] Modal not found for closing: ${modalId}`);
                    return;
                }

                // Handle Bootstrap modals
                const bsModal = modalInstances.get(modalId);
                if (bsModal) {
                    bsModal.hide();
                    modalInstances.delete(modalId);
                    console.log(`? [ADMIN-${operationId}] Bootstrap modal closed: ${modalId}`);
                    return;
                }

                // Handle Tailwind CSS modals
                if (!modal.classList.contains('hidden')) {
                    modal.classList.add('hidden');
                    modal.classList.remove('flex');
                    modal.setAttribute('aria-hidden', 'true');
                    
                    // Restore body scroll
                    document.body.style.overflow = '';
                    
                    // Clear any form data
                    const form = modal.querySelector('form');
                    if (form) {
                        Form.reset(form);
                    }

                    console.log(`? [ADMIN-${operationId}] Tailwind modal closed: ${modalId}`);
                }

                // Hide any loading states
                Loading.hideAll();

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `hideModal-${modalId}`, error, { operationId, modalId });
            }
        },

        /**
         * Setup modal event listeners
         * @param {string} modalId - The modal element ID
         */
        setupModalEvents(modalId) {
            const modal = document.getElementById(modalId);
            if (!modal) return;

            // Remove existing listeners to prevent duplicates
            modal.removeEventListener('click', this.handleBackdropClick);
            document.removeEventListener('keydown', this.handleEscapeKey);

            // Add event listeners
            modal.addEventListener('click', this.handleBackdropClick.bind(this));
            document.addEventListener('keydown', this.handleEscapeKey.bind(this));

            // Setup form validation if form exists
            const form = modal.querySelector('form');
            if (form) {
                Validation.setup(form);
            }
        },

        /**
         * Handle backdrop clicks
         */
        handleBackdropClick(event) {
            if (event.target === event.currentTarget || 
                event.target.classList.contains('modal-backdrop') ||
                event.target.classList.contains('bg-black')) {
                this.hide(event.currentTarget.id);
            }
        },

        /**
         * Handle escape key
         */
        handleEscapeKey(event) {
            if (event.key === 'Escape') {
                // Find the topmost visible modal
                const visibleModals = document.querySelectorAll('.modal:not(.hidden), .modal.show, [class*="modal"][style*="flex"]');
                if (visibleModals.length > 0) {
                    const topModal = visibleModals[visibleModals.length - 1];
                    this.hide(topModal.id);
                }
            }
        }
    };

    // **ALERT SYSTEM**
    const Alert = {
        /**
         * Show success alert
         * @param {string} message - Success message
         */
        success(message) {
            this.show('success', message);
        },

        /**
         * Show error alert
         * @param {string} message - Error message
         */
        error(message) {
            this.show('error', message);
        },

        /**
         * Show warning alert
         * @param {string} message - Warning message
         */
        warning(message) {
            this.show('warning', message);
        },

        /**
         * Show info alert
         * @param {string} message - Info message
         */
        info(message) {
            this.show('info', message);
        },

        /**
         * Show alert with specified type
         * @param {string} type - Alert type (success, error, warning, info)
         * @param {string} message - Alert message
         */
        show(type, message) {
            const operationId = generateOperationId();
            try {
                console.log(`?? [ADMIN-${operationId}] Showing ${type} alert: ${message}`);

                // Check if _AlertNotifications partial is available
                const alertContainer = document.querySelector('.alert-notifications-container');
                if (alertContainer) {
                    // Use the standardized alert system
                    this.showStandardAlert(type, message);
                } else {
                    // Fallback to creating dynamic alert
                    this.showDynamicAlert(type, message);
                }

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `showAlert-${type}`, error, { operationId, type, message });
                // Fallback to console
                console.error(`Alert failed: ${message}`);
            }
        },

        /**
         * Show standardized alert using _AlertNotifications partial
         */
        showStandardAlert(type, message) {
            // Set TempData equivalent in sessionStorage for alerts
            sessionStorage.setItem(`Alert_${type}`, message);
            
            // Trigger alert display (this would work with the _AlertNotifications partial)
            window.dispatchEvent(new CustomEvent('opcentrix:alert', {
                detail: { type, message }
            }));
        },

        /**
         * Show dynamic alert when standard system isn't available
         */
        showDynamicAlert(type, message) {
            // Create alert element
            const alertId = `alert-${Date.now()}`;
            const alertElement = document.createElement('div');
            alertElement.id = alertId;
            alertElement.className = `alert-notification alert-${type}`;
            
            // Alert content with icons
            const icons = {
                success: '?',
                error: '?',
                warning: '??',
                info: '??'
            };

            alertElement.innerHTML = `
                <div class="flex items-center space-x-2">
                    <span class="alert-icon">${icons[type] || '??'}</span>
                    <span class="alert-message">${escapeHtml(message)}</span>
                    <button type="button" class="alert-close" onclick="OpCentrixAdmin.Alert.dismiss('${alertId}')" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            `;

            // Style the alert
            this.styleAlert(alertElement, type);

            // Add to page
            document.body.appendChild(alertElement);

            // Auto-dismiss
            setTimeout(() => this.dismiss(alertId), config.alertDuration);

            // Animate in
            setTimeout(() => alertElement.classList.add('show'), 10);
        },

        /**
         * Style alert element
         */
        styleAlert(element, type) {
            const baseStyles = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                padding: 12px 16px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                max-width: 400px;
                transform: translateX(100%);
                transition: transform 0.3s ease-in-out;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                font-size: 14px;
                line-height: 1.4;
            `;

            const typeStyles = {
                success: 'background-color: #10b981; color: white;',
                error: 'background-color: #ef4444; color: white;',
                warning: 'background-color: #f59e0b; color: white;',
                info: 'background-color: #3b82f6; color: white;'
            };

            element.style.cssText = baseStyles + (typeStyles[type] || typeStyles.info);

            // Show class for animation
            element.classList.add('alert-notification');
            const showStyles = 'transform: translateX(0);';
            
            // Add CSS for show state
            const style = document.createElement('style');
            style.textContent = `.alert-notification.show { ${showStyles} }`;
            document.head.appendChild(style);
        },

        /**
         * Dismiss alert
         * @param {string} alertId - Alert element ID
         */
        dismiss(alertId) {
            const alert = document.getElementById(alertId);
            if (alert) {
                alert.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    if (alert.parentNode) {
                        alert.parentNode.removeChild(alert);
                    }
                }, 300);
            }
        }
    };

    // **LOADING STATE MANAGEMENT**
    const Loading = {
        /**
         * Show loading state on button
         * @param {HTMLElement|string} button - Button element or ID
         */
        show(button) {
            const operationId = generateOperationId();
            try {
                const btn = typeof button === 'string' ? document.getElementById(button) : button;
                if (!btn) {
                    throw new Error(`Button not found: ${button}`);
                }

                console.log(`? [ADMIN-${operationId}] Showing loading state on button: ${btn.id || btn.textContent}`);

                // Store original state
                if (!loadingStates.has(btn)) {
                    loadingStates.set(btn, {
                        disabled: btn.disabled,
                        innerHTML: btn.innerHTML,
                        className: btn.className
                    });
                }

                // Set loading state
                btn.disabled = true;
                
                // Handle different button structures
                const submitText = btn.querySelector('.submit-text');
                const submitLoading = btn.querySelector('.submit-loading');

                if (submitText && submitLoading) {
                    // Structured button with loading elements
                    submitText.classList.add('hidden', 'd-none');
                    submitLoading.classList.remove('hidden', 'd-none');
                } else {
                    // Simple button - add spinner
                    const originalText = btn.textContent;
                    btn.innerHTML = `
                        <span class="inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" aria-hidden="true"></span>
                        Loading...
                    `;
                }

                // Add loading class
                btn.classList.add('loading');

                console.log(`? [ADMIN-${operationId}] Loading state activated`);

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `showLoading`, error, { operationId, button });
            }
        },

        /**
         * Hide loading state on button
         * @param {HTMLElement|string} button - Button element or ID
         */
        hide(button) {
            const operationId = generateOperationId();
            try {
                const btn = typeof button === 'string' ? document.getElementById(button) : button;
                if (!btn) {
                    console.warn(`?? [ADMIN-${operationId}] Button not found for hiding loading: ${button}`);
                    return;
                }

                console.log(`?? [ADMIN-${operationId}] Hiding loading state on button: ${btn.id || btn.textContent}`);

                // Restore original state
                const originalState = loadingStates.get(btn);
                if (originalState) {
                    btn.disabled = originalState.disabled;
                    btn.innerHTML = originalState.innerHTML;
                    btn.className = originalState.className;
                    loadingStates.delete(btn);
                } else {
                    // Fallback restoration
                    btn.disabled = false;
                    const submitText = btn.querySelector('.submit-text');
                    const submitLoading = btn.querySelector('.submit-loading');

                    if (submitText && submitLoading) {
                        submitText.classList.remove('hidden', 'd-none');
                        submitLoading.classList.add('hidden', 'd-none');
                    }
                }

                // Remove loading class
                btn.classList.remove('loading');

                console.log(`? [ADMIN-${operationId}] Loading state removed`);

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `hideLoading`, error, { operationId, button });
            }
        },

        /**
         * Hide all loading states
         */
        hideAll() {
            console.log(`?? [ADMIN] Hiding all loading states (${loadingStates.size} active)`);
            loadingStates.forEach((originalState, btn) => {
                this.hide(btn);
            });
        }
    };

    // **FORM HANDLING**
    const Form = {
        /**
         * Reset form to initial state
         * @param {HTMLFormElement|string} form - Form element or ID
         */
        reset(form) {
            const operationId = generateOperationId();
            try {
                const formElement = typeof form === 'string' ? document.getElementById(form) : form;
                if (!formElement) {
                    throw new Error(`Form not found: ${form}`);
                }

                console.log(`?? [ADMIN-${operationId}] Resetting form: ${formElement.id || 'unnamed'}`);

                // Reset form fields
                formElement.reset();

                // Clear validation errors
                Validation.clearErrors(formElement);

                // Reset any custom states
                const inputs = formElement.querySelectorAll('input, select, textarea');
                inputs.forEach(input => {
                    input.classList.remove('is-invalid', 'is-valid', 'border-red-500', 'border-green-500');
                });

                console.log(`? [ADMIN-${operationId}] Form reset completed`);

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `resetForm`, error, { operationId, form });
            }
        },

        /**
         * Submit form with loading state and error handling
         * @param {HTMLFormElement|string} form - Form element or ID
         * @param {Object} options - Submit options
         */
        submit(form, options = {}) {
            const operationId = generateOperationId();
            try {
                const formElement = typeof form === 'string' ? document.getElementById(form) : form;
                if (!formElement) {
                    throw new Error(`Form not found: ${form}`);
                }

                console.log(`?? [ADMIN-${operationId}] Submitting form: ${formElement.id || 'unnamed'}`);

                // Show loading on submit button
                const submitBtn = formElement.querySelector('[type="submit"]');
                if (submitBtn) {
                    Loading.show(submitBtn);
                }

                // Validate before submit if required
                if (options.validate !== false) {
                    if (!Validation.validateForm(formElement)) {
                        Loading.hide(submitBtn);
                        console.log(`? [ADMIN-${operationId}] Form validation failed`);
                        return false;
                    }
                }

                // If HTMX is handling the form, let it proceed
                if (formElement.hasAttribute('hx-post') || formElement.hasAttribute('hx-put')) {
                    console.log(`? [ADMIN-${operationId}] HTMX form submission initiated`);
                    return true;
                }

                // Manual form submission
                formElement.submit();
                console.log(`? [ADMIN-${operationId}] Form submitted manually`);
                return true;

            } catch (error) {
                OpCentrixErrorLogger.logError('ADMIN', `submitForm`, error, { operationId, form });
                const submitBtn = form.querySelector('[type="submit"]');
                if (submitBtn) Loading.hide(submitBtn);
                Alert.error('Form submission failed. Please try again.');
                return false;
            }
        }
    };

    // **VALIDATION HANDLING**
    const Validation = {
        /**
         * Setup validation for form
         * @param {HTMLFormElement} form - Form element
         */
        setup(form) {
            if (!form) return;

            // Initialize jQuery validation if available
            if (window.jQuery && window.jQuery.fn.validate) {
                const $form = window.jQuery(form);
                if (!$form.data('validator')) {
                    $form.validate({
                        errorClass: 'is-invalid text-red-500 text-xs',
                        validClass: 'is-valid',
                        errorPlacement: function(error, element) {
                            const container = element.closest('.form-group, .mb-3, .space-y-1');
                            if (container.length) {
                                error.appendTo(container);
                            } else {
                                error.insertAfter(element);
                            }
                        }
                    });
                }
            }

            // Setup custom validation
            this.setupCustomValidation(form);
        },

        /**
         * Setup custom validation rules
         */
        setupCustomValidation(form) {
            const inputs = form.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', debounce(() => this.validateField(input), config.debounceDelay));
            });
        },

        /**
         * Validate individual field
         */
        validateField(field) {
            if (!field) return true;

            let isValid = true;
            const value = field.value.trim();

            // Required validation
            if (field.hasAttribute('required') && !value) {
                this.showFieldError(field, 'This field is required.');
                isValid = false;
            }
            // Email validation
            else if (field.type === 'email' && value && !this.isValidEmail(value)) {
                this.showFieldError(field, 'Please enter a valid email address.');
                isValid = false;
            }
            // URL validation
            else if (field.type === 'url' && value && !this.isValidUrl(value)) {
                this.showFieldError(field, 'Please enter a valid URL.');
                isValid = false;
            }
            // Number validation
            else if (field.type === 'number' && value && isNaN(value)) {
                this.showFieldError(field, 'Please enter a valid number.');
                isValid = false;
            }
            // Min/Max length validation
            else if (field.hasAttribute('minlength') && value.length < parseInt(field.getAttribute('minlength'))) {
                this.showFieldError(field, `Minimum length is ${field.getAttribute('minlength')} characters.`);
                isValid = false;
            }
            else if (field.hasAttribute('maxlength') && value.length > parseInt(field.getAttribute('maxlength'))) {
                this.showFieldError(field, `Maximum length is ${field.getAttribute('maxlength')} characters.`);
                isValid = false;
            }
            else {
                this.hideFieldError(field);
            }

            return isValid;
        },

        /**
         * Validate entire form
         */
        validateForm(form) {
            if (!form) return false;

            console.log(`?? [ADMIN] Validating form: ${form.id || 'unnamed'}`);

            const fields = form.querySelectorAll('input, select, textarea');
            let isValid = true;

            fields.forEach(field => {
                if (!this.validateField(field)) {
                    isValid = false;
                }
            });

            // Check for jQuery validation if available
            if (window.jQuery && window.jQuery.fn.validate) {
                const $form = window.jQuery(form);
                const validator = $form.data('validator');
                if (validator) {
                    isValid = validator.form() && isValid;
                }
            }

            console.log(`${isValid ? '?' : '?'} [ADMIN] Form validation ${isValid ? 'passed' : 'failed'}`);
            return isValid;
        },

        /**
         * Show field error
         */
        showFieldError(field, message) {
            field.classList.add('is-invalid', 'border-red-500');
            field.classList.remove('is-valid', 'border-green-500');

            // Find or create error element
            let errorElement = field.parentNode.querySelector('.invalid-feedback, .text-red-500');
            if (!errorElement) {
                errorElement = document.createElement('div');
                errorElement.className = 'invalid-feedback text-red-500 text-xs mt-1';
                field.parentNode.appendChild(errorElement);
            }
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        },

        /**
         * Hide field error
         */
        hideFieldError(field) {
            field.classList.remove('is-invalid', 'border-red-500');
            field.classList.add('is-valid', 'border-green-500');

            const errorElement = field.parentNode.querySelector('.invalid-feedback, .text-red-500');
            if (errorElement) {
                errorElement.style.display = 'none';
            }
        },

        /**
         * Clear all validation errors
         */
        clearErrors(form) {
            const fields = form.querySelectorAll('input, select, textarea');
            fields.forEach(field => {
                field.classList.remove('is-invalid', 'is-valid', 'border-red-500', 'border-green-500');
            });

            const errorElements = form.querySelectorAll('.invalid-feedback, .text-red-500');
            errorElements.forEach(error => {
                error.style.display = 'none';
            });
        },

        /**
         * Email validation helper
         */
        isValidEmail(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
        },

        /**
         * URL validation helper
         */
        isValidUrl(url) {
            try {
                new URL(url);
                return true;
            } catch {
                return false;
            }
        }
    };

    // **CONFIRMATION DIALOGS**
    const Confirm = {
        /**
         * Show confirmation dialog
         * @param {string} message - Confirmation message
         * @param {Function} onConfirm - Callback for confirmation
         * @param {Function} onCancel - Callback for cancellation
         */
        show(message, onConfirm, onCancel) {
            const operationId = generateOperationId();
            console.log(`? [ADMIN-${operationId}] Showing confirmation: ${message}`);

            // Use native confirm for now - can be enhanced with custom modal
            if (confirm(message)) {
                console.log(`? [ADMIN-${operationId}] User confirmed action`);
                if (onConfirm) onConfirm();
                return true;
            } else {
                console.log(`? [ADMIN-${operationId}] User cancelled action`);
                if (onCancel) onCancel();
                return false;
            }
        },

        /**
         * Confirm delete action
         * @param {string} itemName - Name of item being deleted
         * @param {Function} onConfirm - Callback for confirmation
         */
        delete(itemName, onConfirm) {
            const message = `Are you sure you want to delete "${itemName}"? This action cannot be undone.`;
            return this.show(message, onConfirm);
        }
    };

    // **UTILITY FUNCTIONS**

    /**
     * Generate unique operation ID for tracking
     */
    function generateOperationId() {
        return Math.random().toString(36).substr(2, 8);
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Debounce function calls
     */
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // **HTMX INTEGRATION**
    const HTMX = {
        /**
         * Setup HTMX event handlers
         */
        setup() {
            // Before request - show loading
            document.body.addEventListener('htmx:beforeRequest', (event) => {
                const form = event.target.closest('form');
                if (form) {
                    const submitBtn = form.querySelector('[type="submit"]');
                    if (submitBtn) {
                        Loading.show(submitBtn);
                    }
                }
            });

            // After request - hide loading and handle response
            document.body.addEventListener('htmx:afterRequest', (event) => {
                const form = event.target.closest('form');
                if (form) {
                    const submitBtn = form.querySelector('[type="submit"]');
                    if (submitBtn) {
                        Loading.hide(submitBtn);
                    }
                }

                // Handle successful responses
                if (event.detail.successful) {
                    this.handleSuccess(event);
                } else {
                    this.handleError(event);
                }
            });

            // Response error handling
            document.body.addEventListener('htmx:responseError', (event) => {
                console.error('HTMX Response Error:', event.detail);
                Alert.error('Server error occurred. Please try again.');
                Loading.hideAll();
            });

            // Send error handling
            document.body.addEventListener('htmx:sendError', (event) => {
                console.error('HTMX Send Error:', event.detail);
                Alert.error('Network error. Please check your connection.');
                Loading.hideAll();
            });
        },

        /**
         * Handle successful HTMX responses
         */
        handleSuccess(event) {
            const target = event.target;
            
            // Check if response contains modal - if so, modal should close
            if (target.closest('.modal')) {
                const modal = target.closest('.modal');
                // Small delay to allow content to update before closing
                setTimeout(() => {
                    Modal.hide(modal.id);
                    Alert.success('Operation completed successfully!');
                }, 100);
            }
        },

        /**
         * Handle HTMX errors
         */
        handleError(event) {
            const xhr = event.detail.xhr;
            let errorMessage = 'An error occurred. Please try again.';

            if (xhr.status === 400) {
                errorMessage = 'Invalid request. Please check your input.';
            } else if (xhr.status === 401) {
                errorMessage = 'You are not authorized to perform this action.';
            } else if (xhr.status === 404) {
                errorMessage = 'The requested resource was not found.';
            } else if (xhr.status >= 500) {
                errorMessage = 'Server error. Please try again later.';
            }

            Alert.error(errorMessage);
        }
    };

    // **INITIALIZATION**
    function init() {
        if (initialized) {
            console.log('?? [ADMIN] Already initialized');
            return;
        }

        console.log('?? [ADMIN] Initializing OpCentrix Admin module...');

        try {
            // Setup HTMX integration
            HTMX.setup();

            // Setup global event delegation
            setupEventDelegation();

            // Setup keyboard shortcuts
            setupKeyboardShortcuts();

            // Initialize validation for existing forms
            document.querySelectorAll('form').forEach(form => {
                Validation.setup(form);
            });

            initialized = true;
            console.log('? [ADMIN] OpCentrix Admin module initialized successfully');

        } catch (error) {
            console.error('? [ADMIN] Failed to initialize:', error);
            OpCentrixErrorLogger.logError('ADMIN', 'initialization', error);
        }
    }

    /**
     * Setup global event delegation
     */
    function setupEventDelegation() {
        document.addEventListener('click', (event) => {
            const target = event.target;

            // Handle data attributes for actions
            if (target.hasAttribute('data-action')) {
                event.preventDefault();
                handleDataAction(target);
            }

            // Handle close buttons
            if (target.classList.contains('btn-close') || 
                target.classList.contains('modal-close') ||
                target.getAttribute('aria-label') === 'Close') {
                const modal = target.closest('.modal');
                if (modal) {
                    Modal.hide(modal.id);
                }
            }
        });

        document.addEventListener('submit', (event) => {
            const form = event.target;
            if (form.tagName === 'FORM') {
                // Let HTMX handle if it's configured for it
                if (!form.hasAttribute('hx-post') && !form.hasAttribute('hx-put')) {
                    // Manual form handling
                    const isValid = Validation.validateForm(form);
                    if (!isValid) {
                        event.preventDefault();
                        Alert.error('Please fix the validation errors before submitting.');
                    }
                }
            }
        });
    }

    /**
     * Handle data-action attributes
     */
    function handleDataAction(element) {
        const action = element.getAttribute('data-action');
        const target = element.getAttribute('data-target');

        switch (action) {
            case 'show-modal':
                if (target) Modal.show(target);
                break;
            case 'hide-modal':
                if (target) Modal.hide(target);
                break;
            case 'confirm-delete':
                const itemName = element.getAttribute('data-item-name') || 'this item';
                Confirm.delete(itemName, () => {
                    // Execute the actual delete action
                    const deleteUrl = element.getAttribute('data-delete-url');
                    if (deleteUrl) {
                        window.location.href = deleteUrl;
                    }
                });
                break;
            default:
                console.warn(`Unknown data-action: ${action}`);
        }
    }

    /**
     * Setup keyboard shortcuts
     */
    function setupKeyboardShortcuts() {
        document.addEventListener('keydown', (event) => {
            // Escape key - close modals
            if (event.key === 'Escape') {
                // Already handled in Modal.setupModalEvents
                return;
            }

            // Ctrl+Enter - submit form
            if (event.ctrlKey && event.key === 'Enter') {
                const activeForm = document.activeElement?.closest('form');
                if (activeForm) {
                    event.preventDefault();
                    Form.submit(activeForm);
                }
            }
        });
    }

    // **PUBLIC API**
    return {
        // Core modules
        Modal,
        Alert,
        Loading,
        Form,
        Validation,
        Confirm,
        HTMX,

        // Utility functions
        init,
        escapeHtml,
        debounce,
        generateOperationId,

        // Backward compatibility aliases
        showModal: Modal.show,
        hideModal: Modal.hide,
        showAlert: Alert.show,
        showLoadingState: Loading.show,
        hideLoadingState: Loading.hide,
        confirmDelete: Confirm.delete,
        initializeValidation: Validation.setup
    };
})();

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', OpCentrixAdmin.init);
} else {
    OpCentrixAdmin.init();
}

// Global aliases for backward compatibility
window.showModal = OpCentrixAdmin.showModal;
window.hideModal = OpCentrixAdmin.hideModal;
window.showAlert = OpCentrixAdmin.showAlert;
window.showLoadingState = OpCentrixAdmin.showLoadingState;
window.hideLoadingState = OpCentrixAdmin.hideLoadingState;
window.confirmDelete = OpCentrixAdmin.confirmDelete;

console.log('? [ADMIN] OpCentrix Admin module loaded successfully');