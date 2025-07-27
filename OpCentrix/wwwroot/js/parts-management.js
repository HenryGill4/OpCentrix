/**
 * OpCentrix Parts Management Module
 * Modern part form handling with proper validation and state management
 */

OpCentrix.module('PartManagement', async (UI) => {
    'use strict';

    const { Utils, EventManager } = OpCentrix;

    // ==================================================
    // PART FORM CONFIGURATION
    // ==================================================

    const MATERIAL_DEFAULTS = {
        'Inconel 718': {
            laserPower: 285,
            scanSpeed: 960,
            buildTemperature: 200,
            materialCost: 750.00,
            estimatedHours: 12.0,
            layerThickness: 30,
            hatchSpacing: 120,
            argonPurity: 99.95,
            maxOxygen: 25
        },
        'Inconel 625': {
            laserPower: 275,
            scanSpeed: 980,
            buildTemperature: 195,
            materialCost: 850.00,
            estimatedHours: 14.0,
            layerThickness: 30,
            hatchSpacing: 120,
            argonPurity: 99.95,
            maxOxygen: 25
        },
        'Ti-6Al-4V Grade 5': {
            laserPower: 200,
            scanSpeed: 1200,
            buildTemperature: 180,
            materialCost: 450.00,
            estimatedHours: 8.0,
            layerThickness: 30,
            hatchSpacing: 120,
            argonPurity: 99.9,
            maxOxygen: 50
        },
        'Ti-6Al-4V ELI Grade 23': {
            laserPower: 195,
            scanSpeed: 1280,
            buildTemperature: 175,
            materialCost: 480.00,
            estimatedHours: 8.5,
            layerThickness: 30,
            hatchSpacing: 120,
            argonPurity: 99.9,
            maxOxygen: 50
        },
        '316L Stainless Steel': {
            laserPower: 240,
            scanSpeed: 1100,
            buildTemperature: 170,
            materialCost: 280.00,
            estimatedHours: 6.0,
            layerThickness: 30,
            hatchSpacing: 120,
            argonPurity: 99.5,
            maxOxygen: 100
        }
    };

    let validationSettings = {
        pattern: /^\d{2}-\d{4}$/,
        patternString: '^\\d{2}-\\d{4}$',
        example: '14-5396',
        message: 'Part number must be in format XX-XXXX'
    };

    // ==================================================
    // PART FORM COMPONENT
    // ==================================================

    class PartFormComponent {
        constructor() {
            this.activeForm = null;
            this.setupEventHandlers();
            this.loadValidationSettings();
        }

        /**
         * Setup event handlers for part forms
         */
        setupEventHandlers() {
            // Tab navigation
            EventManager.delegate(document, '[data-part-tab]', 'click', async (e, button) => {
                e.preventDefault();
                const tabName = button.dataset.partTab;
                await this.showTab(tabName);
            });

            // Material selection
            EventManager.delegate(document, '[data-part-material-select]', 'change', async (e, select) => {
                await this.updateMaterialSettings(select.value);
            });

            // Duration calculation
            EventManager.delegate(document, '[data-part-hours]', 'input', async (e, input) => {
                await this.updateDurationDisplay(parseFloat(input.value) || 0);
            });

            // Override display
            EventManager.delegate(document, '[data-part-override]', 'input', async (e, input) => {
                await this.updateOverrideDisplay(parseFloat(input.value) || 0);
            });

            // Part number validation
            EventManager.delegate(document, '[data-part-number]', 'blur', async (e, input) => {
                await this.validatePartNumber(input.value);
            });

            // Form submission
            EventManager.delegate(document, '[data-part-form]', 'submit', async (e, form) => {
                const isValid = await this.validateForm(form);
                if (!isValid) {
                    e.preventDefault();
                    UI.error('Please fix validation errors before submitting');
                    return false;
                }
                
                UI.showLoading(form.querySelector('[type="submit"]'), 'Saving...');
            });
        }

        /**
         * Load validation settings from server
         */
        async loadValidationSettings() {
            try {
                const response = await fetch('/Admin/Parts?handler=ValidationSettings');
                if (response.ok) {
                    const settings = await response.json();
                    validationSettings = {
                        pattern: new RegExp(settings.pattern),
                        patternString: settings.pattern,
                        example: settings.example,
                        message: settings.message
                    };
                    console.log('? [PARTS] Validation settings loaded:', validationSettings);
                }
            } catch (error) {
                console.warn('?? [PARTS] Failed to load validation settings, using defaults:', error);
            }
        }

        /**
         * Show specific tab in part form
         */
        async showTab(tabName) {
            const operationId = Utils.generateId();
            console.log(`??? [PARTS] ${operationId} Showing tab: ${tabName}`);

            try {
                // Hide all tab contents
                Utils.getElements('[data-part-tab-content]').forEach(content => {
                    content.classList.add('hidden');
                });

                // Remove active state from all tabs
                Utils.getElements('[data-part-tab]').forEach(tab => {
                    tab.classList.remove('active', 'border-blue-500', 'text-blue-600');
                    tab.classList.add('border-transparent', 'text-gray-500');
                });

                // Show selected tab content
                const selectedContent = Utils.getElement(`[data-part-tab-content="${tabName}"]`);
                if (selectedContent) {
                    selectedContent.classList.remove('hidden');
                }

                // Make selected tab active
                const selectedTab = Utils.getElement(`[data-part-tab="${tabName}"]`);
                if (selectedTab) {
                    selectedTab.classList.add('active', 'border-blue-500', 'text-blue-600');
                    selectedTab.classList.remove('border-transparent', 'text-gray-500');
                }

                console.log(`? [PARTS] ${operationId} Tab shown successfully`);
                return true;

            } catch (error) {
                console.error(`? [PARTS] ${operationId} Error showing tab:`, error);
                return false;
            }
        }

        /**
         * Update material settings and apply defaults
         */
        async updateMaterialSettings(materialType) {
            const operationId = Utils.generateId();
            console.log(`?? [PARTS] ${operationId} Updating material settings: ${materialType}`);

            try {
                // Update SLS material field
                const slsMaterialInput = Utils.getElement('[data-part-sls-material]');
                if (slsMaterialInput) {
                    slsMaterialInput.value = materialType;
                }

                // Apply material defaults
                const defaults = MATERIAL_DEFAULTS[materialType];
                if (defaults) {
                    const fieldMappings = [
                        { selector: '[data-part-hours]', value: defaults.estimatedHours },
                        { selector: '[data-part-material-cost]', value: defaults.materialCost },
                        { selector: '[data-part-laser-power]', value: defaults.laserPower },
                        { selector: '[data-part-scan-speed]', value: defaults.scanSpeed },
                        { selector: '[data-part-build-temp]', value: defaults.buildTemperature },
                        { selector: '[data-part-layer-thickness]', value: defaults.layerThickness },
                        { selector: '[data-part-hatch-spacing]', value: defaults.hatchSpacing },
                        { selector: '[data-part-argon-purity]', value: defaults.argonPurity },
                        { selector: '[data-part-max-oxygen]', value: defaults.maxOxygen }
                    ];

                    let updateCount = 0;
                    fieldMappings.forEach(mapping => {
                        const element = Utils.getElement(mapping.selector);
                        if (element) {
                            element.value = mapping.value;
                            updateCount++;
                            
                            // Trigger change event for other handlers
                            element.dispatchEvent(new Event('input', { bubbles: true }));
                        }
                    });

                    console.log(`? [PARTS] ${operationId} Applied ${updateCount} material defaults`);
                }

                return true;

            } catch (error) {
                console.error(`? [PARTS] ${operationId} Error updating material settings:`, error);
                return false;
            }
        }

        /**
         * Update duration display based on hours
         */
        async updateDurationDisplay(hours) {
            const operationId = Utils.generateId();
            console.log(`?? [PARTS] ${operationId} Updating duration display: ${hours} hours`);

            try {
                const durationDisplay = Utils.getElement('[data-part-duration-display]');
                const durationDays = Utils.getElement('[data-part-duration-days]');

                if (durationDisplay && hours > 0) {
                    let displayText;
                    if (hours >= 24) {
                        const days = Math.floor(hours / 24);
                        const remainingHours = hours % 24;
                        displayText = `${days}d ${remainingHours.toFixed(1)}h`;
                    } else {
                        displayText = `${hours.toFixed(1)}h`;
                    }
                    durationDisplay.value = displayText;
                } else if (durationDisplay) {
                    durationDisplay.value = '';
                }

                if (durationDays) {
                    durationDays.value = hours > 0 ? Math.ceil(hours / 8) : '';
                }

                console.log(`? [PARTS] ${operationId} Duration display updated`);
                return true;

            } catch (error) {
                console.error(`? [PARTS] ${operationId} Error updating duration display:`, error);
                return false;
            }
        }

        /**
         * Update override display and requirements
         */
        async updateOverrideDisplay(overrideHours) {
            const operationId = Utils.generateId();
            console.log(`????? [PARTS] ${operationId} Updating override display: ${overrideHours} hours`);

            try {
                const overrideReasonInput = Utils.getElement('[data-part-override-reason]');
                
                if (overrideReasonInput) {
                    const hasOverride = overrideHours > 0;
                    
                    if (hasOverride) {
                        overrideReasonInput.setAttribute('required', 'required');
                        overrideReasonInput.placeholder = 'Reason for override is required';
                    } else {
                        overrideReasonInput.removeAttribute('required');
                        overrideReasonInput.placeholder = 'Reason for duration override...';
                    }
                }

                console.log(`? [PARTS] ${operationId} Override display updated`);
                return true;

            } catch (error) {
                console.error(`? [PARTS] ${operationId} Error updating override display:`, error);
                return false;
            }
        }

        /**
         * Validate part number with real-time feedback
         */
        async validatePartNumber(partNumber) {
            const operationId = Utils.generateId();
            console.log(`?? [PARTS] ${operationId} Validating part number: ${partNumber}`);

            const validationDisplay = Utils.getElement('[data-part-number-validation]');
            if (!validationDisplay || !partNumber) {
                return false;
            }

            try {
                // Format validation
                if (!validationSettings.pattern.test(partNumber)) {
                    const message = `${validationSettings.message} (e.g., ${validationSettings.example})`;
                    validationDisplay.innerHTML = `<span class="text-red-500">Invalid format. ${Utils.escapeHtml(message)}</span>`;
                    validationDisplay.classList.remove('hidden');
                    return false;
                }

                // Duplicate check
                const currentId = this.getCurrentPartId();
                const response = await fetch(`/Admin/Parts?handler=CheckDuplicate&partNumber=${encodeURIComponent(partNumber)}&currentId=${currentId}`);
                
                if (response.ok) {
                    const isDuplicate = await response.text();
                    if (isDuplicate === 'true') {
                        validationDisplay.innerHTML = `<span class="text-red-500">Part number "${Utils.escapeHtml(partNumber)}" already exists</span>`;
                        validationDisplay.classList.remove('hidden');
                        return false;
                    } else {
                        validationDisplay.innerHTML = `<span class="text-green-500">? Part number "${Utils.escapeHtml(partNumber)}" is available</span>`;
                        validationDisplay.classList.remove('hidden');
                        return true;
                    }
                }

                return false;

            } catch (error) {
                console.error(`? [PARTS] ${operationId} Error validating part number:`, error);
                validationDisplay.innerHTML = `<span class="text-yellow-500">?? Could not validate part number</span>`;
                validationDisplay.classList.remove('hidden');
                return false;
            }
        }

        /**
         * Get current part ID for duplicate checking
         */
        getCurrentPartId() {
            const idInput = Utils.getElement('[name="Id"]');
            return idInput ? parseInt(idInput.value) || 0 : 0;
        }

        /**
         * Validate entire part form
         */
        async validateForm(form) {
            console.log('?? [PARTS] Validating part form');

            // Standard form validation first
            const isFormValid = UI.validateForm(form);
            
            // Additional part-specific validation
            const partNumberInput = Utils.getElement('[data-part-number]', form);
            let isPartNumberValid = true;
            
            if (partNumberInput) {
                isPartNumberValid = await this.validatePartNumber(partNumberInput.value);
            }

            return isFormValid && isPartNumberValid;
        }
    }

    // ==================================================
    // PARTS PAGE COMPONENT
    // ==================================================

    class PartsPageComponent {
        constructor() {
            this.setupEventHandlers();
        }

        /**
         * Setup event handlers for parts page
         */
        setupEventHandlers() {
            // Add new part button
            EventManager.delegate(document, '[data-parts-add]', 'click', async (e, button) => {
                e.preventDefault();
                await this.showAddModal();
            });

            // Edit part buttons
            EventManager.delegate(document, '[data-parts-edit]', 'click', async (e, button) => {
                e.preventDefault();
                const partId = button.dataset.partsEdit;
                await this.showEditModal(partId);
            });

            // Delete part buttons
            EventManager.delegate(document, '[data-parts-delete]', 'click', async (e, button) => {
                e.preventDefault();
                const partId = button.dataset.partsDelete;
                const partNumber = button.dataset.partNumber;
                await this.confirmDelete(partId, partNumber);
            });
        }

        /**
         * Show add part modal
         */
        async showAddModal() {
            const operationId = Utils.generateId();
            console.log(`? [PARTS] ${operationId} Opening add part modal`);

            try {
                await UI.loadModal('modal-container', '/Admin/Parts?handler=Add');
                console.log(`? [PARTS] ${operationId} Add modal loaded`);
            } catch (error) {
                console.error(`? [PARTS] ${operationId} Failed to load add modal:`, error);
                UI.error('Failed to load add part form. Please try again.');
            }
        }

        /**
         * Show edit part modal
         */
        async showEditModal(partId) {
            const operationId = Utils.generateId();
            console.log(`?? [PARTS] ${operationId} Opening edit part modal for ID: ${partId}`);

            try {
                if (!partId || partId <= 0) {
                    throw new Error(`Invalid part ID: ${partId}`);
                }

                await UI.loadModal('modal-container', `/Admin/Parts?handler=Edit&id=${partId}`);
                console.log(`? [PARTS] ${operationId} Edit modal loaded`);
            } catch (error) {
                console.error(`? [PARTS] ${operationId} Failed to load edit modal:`, error);
                UI.error('Failed to load edit part form. Please try again.');
            }
        }

        /**
         * Confirm and delete part
         */
        async confirmDelete(partId, partNumber) {
            const operationId = Utils.generateId();
            console.log(`??? [PARTS] ${operationId} Delete request for part: ${partNumber} (ID: ${partId})`);

            const confirmed = confirm(`Are you sure you want to delete part "${partNumber}"? This action cannot be undone.`);
            
            if (confirmed) {
                try {
                    const formData = new FormData();
                    const token = Utils.getElement('input[name="__RequestVerificationToken"]');
                    if (token) {
                        formData.append('__RequestVerificationToken', token.value);
                    }

                    const response = await fetch(`/Admin/Parts?handler=Delete&id=${partId}`, {
                        method: 'POST',
                        body: formData
                    });

                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }

                    UI.success(`Part "${partNumber}" deleted successfully`);
                    
                    // Refresh page after delay
                    setTimeout(() => window.location.reload(), 1500);

                    console.log(`? [PARTS] ${operationId} Part deleted successfully`);

                } catch (error) {
                    console.error(`? [PARTS] ${operationId} Failed to delete part:`, error);
                    UI.error(`Failed to delete part "${partNumber}". Please try again.`);
                }
            } else {
                console.log(`? [PARTS] ${operationId} User cancelled delete`);
            }
        }
    }

    // ==================================================
    // HTMX INTEGRATION
    // ==================================================

    const setupHTMXIntegration = () => {
        // Handle successful form submissions
        EventManager.on(document, 'htmx:afterRequest', (e) => {
            if (e.target.closest('[data-part-form]') && e.detail.successful) {
                const response = e.detail.xhr.responseText;
                
                if (response.includes('Part saved successfully')) {
                    // Extract part number if possible
                    const partNumberMatch = response.match(/Part saved successfully: ([^']+)/);
                    const partNumber = partNumberMatch ? partNumberMatch[1] : 'Part';
                    
                    UI.hideModal('modal-container');
                    UI.success(`Part "${partNumber}" saved successfully!`);
                    
                    setTimeout(() => window.location.reload(), 1500);
                }
            }
        });

        // Handle form errors
        EventManager.on(document, 'htmx:responseError', (e) => {
            if (e.target.closest('[data-part-form]')) {
                console.error('HTMX Form Error:', e.detail);
                UI.error('Form submission failed. Please try again.');
                UI.hideAllLoading();
            }
        });
    };

    // ==================================================
    // INITIALIZE COMPONENTS
    // ==================================================

    const partForm = new PartFormComponent();
    const partsPage = new PartsPageComponent();
    
    setupHTMXIntegration();

    // ==================================================
    // RETURN PUBLIC API
    // ==================================================

    return {
        form: partForm,
        page: partsPage,
        
        // Convenience methods for external use
        showAddModal: partsPage.showAddModal.bind(partsPage),
        showEditModal: partsPage.showEditModal.bind(partsPage),
        validatePartNumber: partForm.validatePartNumber.bind(partForm),
        showTab: partForm.showTab.bind(partForm)
    };

}, ['UI']);

console.log('? [PARTS] OpCentrix Part Management module loaded');