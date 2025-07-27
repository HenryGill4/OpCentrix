/**
 * OpCentrix UI Components Module
 * Modern component-based UI management with proper lifecycle handling
 */

OpCentrix.module('UI', async () => {
    'use strict';

    const { Utils, EventManager, ErrorReporter } = OpCentrix;

    // ==================================================
    // MODAL COMPONENT
    // ==================================================

    class ModalComponent {
        constructor() {
            this.activeModals = new Map();
            this.setupEventDelegation();
        }

        /**
         * Setup global event delegation for modal controls
         */
        setupEventDelegation() {
            // Close buttons
            EventManager.delegate(document, '[data-modal-close]', 'click', (e, target) => {
                const modalId = target.dataset.modalClose || target.closest('[data-modal]')?.dataset.modal;
                if (modalId) this.hide(modalId);
            });

            // Backdrop clicks
            EventManager.delegate(document, '[data-modal]', 'click', (e, target) => {
                if (e.target === target) {
                    this.hide(target.dataset.modal);
                }
            });

            // Keyboard events
            EventManager.on(document, 'keydown', (e) => {
                if (e.key === 'Escape' && this.activeModals.size > 0) {
                    const topModal = Array.from(this.activeModals.keys()).pop();
                    this.hide(topModal);
                }
            });
        }

        /**
         * Show modal with content
         */
        async show(modalId, content = null) {
            const operationId = Utils.generateId();
            console.log(`?? [MODAL] ${operationId} Showing modal: ${modalId}`);

            try {
                const modal = Utils.getElement(`#${modalId}`) || Utils.getElement(`[data-modal="${modalId}"]`);
                if (!modal) {
                    throw new Error(`Modal not found: ${modalId}`);
                }

                // Set content if provided
                if (content) {
                    const contentContainer = modal.querySelector('.modal-content') || modal;
                    if (typeof content === 'string') {
                        contentContainer.innerHTML = content;
                    } else if (content instanceof HTMLElement) {
                        contentContainer.innerHTML = '';
                        contentContainer.appendChild(content);
                    }
                }

                // Show modal
                modal.classList.remove('hidden');
                modal.classList.add('flex');
                modal.setAttribute('aria-hidden', 'false');

                // Manage focus
                const focusableElement = modal.querySelector('input, button, select, textarea, [tabindex]:not([tabindex="-1"])');
                if (focusableElement) {
                    setTimeout(() => focusableElement.focus(), 100);
                }

                // Track active modal
                this.activeModals.set(modalId, {
                    element: modal,
                    shown: Date.now(),
                    operationId
                });

                // Prevent body scroll
                document.body.style.overflow = 'hidden';

                console.log(`? [MODAL] ${operationId} Modal shown successfully`);
                return true;

            } catch (error) {
                console.error(`? [MODAL] ${operationId} Failed to show modal:`, error);
                await ErrorReporter.report(error, { modalId, operationId });
                return false;
            }
        }

        /**
         * Hide modal
         */
        async hide(modalId) {
            const operationId = Utils.generateId();
            console.log(`?? [MODAL] ${operationId} Hiding modal: ${modalId}`);

            try {
                const modalInfo = this.activeModals.get(modalId);
                if (!modalInfo) {
                    console.warn(`?? [MODAL] Modal not active: ${modalId}`);
                    return false;
                }

                const modal = modalInfo.element;

                // Hide modal
                modal.classList.add('hidden');
                modal.classList.remove('flex');
                modal.setAttribute('aria-hidden', 'true');

                // Remove from tracking
                this.activeModals.delete(modalId);

                // Restore body scroll if no modals active
                if (this.activeModals.size === 0) {
                    document.body.style.overflow = '';
                }

                console.log(`? [MODAL] ${operationId} Modal hidden successfully`);
                return true;

            } catch (error) {
                console.error(`? [MODAL] ${operationId} Failed to hide modal:`, error);
                await ErrorReporter.report(error, { modalId, operationId });
                return false;
            }
        }

        /**
         * Load content via AJAX
         */
        async loadContent(modalId, url, options = {}) {
            const operationId = Utils.generateId();
            console.log(`?? [MODAL] ${operationId} Loading content from: ${url}`);

            try {
                // Show loading state
                await this.show(modalId, `
                    <div class="flex items-center justify-center p-12">
                        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                        <span class="ml-3 text-gray-600">Loading...</span>
                    </div>
                `);

                const response = await fetch(url, {
                    method: options.method || 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...options.headers
                    },
                    body: options.body
                });

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }

                const content = await response.text();
                await this.show(modalId, content);

                console.log(`? [MODAL] ${operationId} Content loaded successfully`);
                return true;

            } catch (error) {
                console.error(`? [MODAL] ${operationId} Failed to load content:`, error);
                await this.show(modalId, `
                    <div class="p-6 text-center">
                        <div class="text-red-600 mb-2">?? Error Loading Content</div>
                        <div class="text-sm text-gray-600">${Utils.escapeHtml(error.message)}</div>
                        <button class="mt-4 px-4 py-2 bg-gray-300 rounded" data-modal-close="${modalId}">Close</button>
                    </div>
                `);
                await ErrorReporter.report(error, { modalId, url, operationId });
                return false;
            }
        }
    }

    // ==================================================
    // NOTIFICATION COMPONENT
    // ==================================================

    class NotificationComponent {
        constructor() {
            this.notifications = new Map();
            this.container = this.createContainer();
        }

        /**
         * Create notification container
         */
        createContainer() {
            let container = Utils.getElement('#notification-container');
            if (!container) {
                container = document.createElement('div');
                container.id = 'notification-container';
                container.className = 'fixed top-4 right-4 z-50 space-y-3';
                document.body.appendChild(container);
            }
            return container;
        }

        /**
         * Show notification
         */
        show(message, type = 'info', duration = 5000) {
            const id = Utils.generateId();
            console.log(`?? [NOTIFICATION] ${id} Showing ${type}: ${message.substring(0, 50)}...`);

            const notification = document.createElement('div');
            notification.id = id;
            notification.className = this.getNotificationClasses(type);
            notification.innerHTML = this.getNotificationHTML(message, type, id);

            // Add to container
            this.container.appendChild(notification);
            this.notifications.set(id, { element: notification, type, shown: Date.now() });

            // Animate in
            requestAnimationFrame(() => {
                notification.classList.add('notification-show');
            });

            // Auto-remove
            if (duration > 0) {
                setTimeout(() => this.hide(id), duration);
            }

            return id;
        }

        /**
         * Hide notification
         */
        hide(id) {
            const notification = this.notifications.get(id);
            if (notification) {
                notification.element.classList.add('notification-hide');
                setTimeout(() => {
                    if (notification.element.parentNode) {
                        notification.element.parentNode.removeChild(notification.element);
                    }
                    this.notifications.delete(id);
                }, 300);
            }
        }

        /**
         * Get notification CSS classes
         */
        getNotificationClasses(type) {
            const baseClasses = 'notification max-w-md p-4 rounded-lg shadow-lg transform transition-all duration-300 translate-x-full opacity-0';
            const typeClasses = {
                success: 'bg-green-600 text-white',
                error: 'bg-red-600 text-white',
                warning: 'bg-yellow-500 text-black',
                info: 'bg-blue-600 text-white'
            };
            return `${baseClasses} ${typeClasses[type] || typeClasses.info}`;
        }

        /**
         * Get notification HTML
         */
        getNotificationHTML(message, type, id) {
            const icons = {
                success: '?',
                error: '?',
                warning: '??',
                info: '??'
            };

            return `
                <div class="flex items-start space-x-3">
                    <div class="flex-shrink-0 text-lg">${icons[type] || icons.info}</div>
                    <div class="flex-1 min-w-0">
                        <p class="font-medium">${Utils.escapeHtml(message)}</p>
                    </div>
                    <button onclick="OpCentrix.UI.notifications.hide('${id}')" class="flex-shrink-0 opacity-70 hover:opacity-100">
                        <span class="sr-only">Close</span>
                        <span class="text-lg">&times;</span>
                    </button>
                </div>
            `;
        }

        // Convenience methods
        success(message, duration) { return this.show(message, 'success', duration); }
        error(message, duration) { return this.show(message, 'error', duration); }
        warning(message, duration) { return this.show(message, 'warning', duration); }
        info(message, duration) { return this.show(message, 'info', duration); }
    }

    // ==================================================
    // LOADING COMPONENT
    // ==================================================

    class LoadingComponent {
        constructor() {
            this.loadingStates = new Map();
        }

        /**
         * Show loading state on element
         */
        show(element, text = 'Loading...') {
            if (typeof element === 'string') {
                element = Utils.getElement(element);
            }

            if (!element) {
                console.warn(`?? [LOADING] Element not found`);
                return false;
            }

            const id = Utils.generateId();
            const originalState = {
                disabled: element.disabled,
                innerHTML: element.innerHTML,
                className: element.className
            };

            this.loadingStates.set(element, { id, originalState });

            // Set loading state
            element.disabled = true;
            element.classList.add('loading');

            if (element.tagName === 'BUTTON') {
                element.innerHTML = `
                    <span class="inline-flex items-center">
                        <svg class="animate-spin -ml-1 mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        ${Utils.escapeHtml(text)}
                    </span>
                `;
            }

            console.log(`? [LOADING] ${id} Loading state activated`);
            return id;
        }

        /**
         * Hide loading state
         */
        hide(element) {
            if (typeof element === 'string') {
                element = Utils.getElement(element);
            }

            if (!element) {
                console.warn(`?? [LOADING] Element not found`);
                return false;
            }

            const state = this.loadingStates.get(element);
            if (state) {
                const { originalState } = state;
                element.disabled = originalState.disabled;
                element.innerHTML = originalState.innerHTML;
                element.className = originalState.className;
                
                this.loadingStates.delete(element);
                console.log(`? [LOADING] ${state.id} Loading state removed`);
                return true;
            }

            return false;
        }

        /**
         * Hide all loading states
         */
        hideAll() {
            this.loadingStates.forEach((state, element) => {
                this.hide(element);
            });
        }
    }

    // ==================================================
    // FORM COMPONENT
    // ==================================================

    class FormComponent {
        constructor() {
            this.setupFormDelegation();
        }

        /**
         * Setup global form event delegation
         */
        setupFormDelegation() {
            // Form validation on submit
            EventManager.delegate(document, 'form[data-validate]', 'submit', async (e, form) => {
                if (!this.validate(form)) {
                    e.preventDefault();
                    return false;
                }
            });

            // Real-time validation
            EventManager.delegate(document, 'form[data-validate] input, form[data-validate] select, form[data-validate] textarea', 'blur', (e, field) => {
                this.validateField(field);
            });
        }

        /**
         * Validate entire form
         */
        validate(form) {
            if (typeof form === 'string') {
                form = Utils.getElement(form);
            }

            if (!form) return false;

            const fields = Utils.getElements('input, select, textarea', form);
            let isValid = true;

            fields.forEach(field => {
                if (!this.validateField(field)) {
                    isValid = false;
                }
            });

            return isValid;
        }

        /**
         * Validate individual field
         */
        validateField(field) {
            if (!field) return true;

            const value = field.value.trim();
            const rules = this.getValidationRules(field);
            let isValid = true;
            let errorMessage = '';

            // Required validation
            if (rules.required && !value) {
                isValid = false;
                errorMessage = 'This field is required';
            }
            // Email validation
            else if (rules.email && value && !this.isValidEmail(value)) {
                isValid = false;
                errorMessage = 'Please enter a valid email address';
            }
            // Pattern validation
            else if (rules.pattern && value && !rules.pattern.test(value)) {
                isValid = false;
                errorMessage = rules.patternMessage || 'Invalid format';
            }
            // Length validation
            else if (rules.minLength && value.length < rules.minLength) {
                isValid = false;
                errorMessage = `Minimum length is ${rules.minLength} characters`;
            }
            else if (rules.maxLength && value.length > rules.maxLength) {
                isValid = false;
                errorMessage = `Maximum length is ${rules.maxLength} characters`;
            }

            this.showFieldValidation(field, isValid, errorMessage);
            return isValid;
        }

        /**
         * Get validation rules from field attributes
         */
        getValidationRules(field) {
            return {
                required: field.hasAttribute('required'),
                email: field.type === 'email',
                pattern: field.pattern ? new RegExp(field.pattern) : null,
                patternMessage: field.dataset.patternMessage,
                minLength: field.minLength || null,
                maxLength: field.maxLength || null
            };
        }

        /**
         * Show field validation state
         */
        showFieldValidation(field, isValid, message) {
            // Remove existing validation classes
            field.classList.remove('border-red-500', 'border-green-500');
            
            // Add appropriate class
            if (isValid) {
                field.classList.add('border-green-500');
            } else {
                field.classList.add('border-red-500');
            }

            // Show/hide error message
            let errorElement = field.parentNode.querySelector('.field-error');
            if (!isValid && message) {
                if (!errorElement) {
                    errorElement = document.createElement('div');
                    errorElement.className = 'field-error text-red-500 text-xs mt-1';
                    field.parentNode.appendChild(errorElement);
                }
                errorElement.textContent = message;
            } else if (errorElement) {
                errorElement.remove();
            }
        }

        /**
         * Email validation helper
         */
        isValidEmail(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
        }

        /**
         * Reset form to initial state
         */
        reset(form) {
            if (typeof form === 'string') {
                form = Utils.getElement(form);
            }

            if (form) {
                form.reset();
                // Clear validation states
                Utils.getElements('input, select, textarea', form).forEach(field => {
                    field.classList.remove('border-red-500', 'border-green-500');
                });
                Utils.getElements('.field-error', form).forEach(error => error.remove());
            }
        }
    }

    // ==================================================
    // CSS STYLES FOR COMPONENTS
    // ==================================================

    const styles = `
        .notification-show {
            transform: translateX(0);
            opacity: 1;
        }
        
        .notification-hide {
            transform: translateX(100%);
            opacity: 0;
        }
        
        .loading {
            cursor: wait !important;
            opacity: 0.7;
        }
        
        .field-error {
            animation: slideDown 0.2s ease-out;
        }
        
        @keyframes slideDown {
            from { opacity: 0; transform: translateY(-5px); }
            to { opacity: 1; transform: translateY(0); }
        }
    `;

    // Inject styles
    const styleSheet = document.createElement('style');
    styleSheet.textContent = styles;
    document.head.appendChild(styleSheet);

    // ==================================================
    // RETURN UI MODULE
    // ==================================================

    const modal = new ModalComponent();
    const notifications = new NotificationComponent();
    const loading = new LoadingComponent();
    const form = new FormComponent();

    return {
        modal,
        notifications,
        loading,
        form,
        
        // Convenience methods
        showModal: modal.show.bind(modal),
        hideModal: modal.hide.bind(modal),
        loadModal: modal.loadContent.bind(modal),
        
        notify: notifications.show.bind(notifications),
        success: notifications.success.bind(notifications),
        error: notifications.error.bind(notifications),
        warning: notifications.warning.bind(notifications),
        info: notifications.info.bind(notifications),
        
        showLoading: loading.show.bind(loading),
        hideLoading: loading.hide.bind(loading),
        hideAllLoading: loading.hideAll.bind(loading),
        
        validateForm: form.validate.bind(form),
        resetForm: form.reset.bind(form)
    };

}, []);

console.log('? [UI] OpCentrix UI components module loaded');