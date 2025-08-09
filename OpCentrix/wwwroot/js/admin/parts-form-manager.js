/**
 * OpCentrix Part Form Manager - Best Practices Implementation
 * Centralized management for part form interactions with proper error boundaries
 * Follows modern JavaScript patterns and validation best practices
 */

class OpCentrixPartFormManager {
    constructor() {
        this.initialized = false;
        this.validationRules = {};
        this.stageManager = null;
        this.formElement = null;
        this.submitInProgress = false;
        this.validationRequired = true;

        // Bind methods to maintain context
        this.handleFormSubmit = this.handleFormSubmit.bind(this);
        this.validateForm = this.validateForm.bind(this);
        this.populateHiddenFields = this.populateHiddenFields.bind(this);

        this.init();
    }

    async init() {
        try {
            console.log('?? [PART-FORM] Initializing OpCentrix Part Form Manager...');
            
            // Wait for DOM to be ready
            if (document.readyState === 'loading') {
                await new Promise(resolve => {
                    document.addEventListener('DOMContentLoaded', resolve);
                });
            }

            // Initialize components in proper order
            await this.initializeForm();
            await this.initializeValidation();
            await this.initializeStageManager();
            await this.initializeEventHandlers();

            this.initialized = true;
            console.log('? [PART-FORM] Part Form Manager initialized successfully');

        } catch (error) {
            console.error('? [PART-FORM] Failed to initialize Part Form Manager:', error);
            this.showInitializationError(error);
        }
    }

    async initializeForm() {
        this.formElement = document.getElementById('partForm');
        
        if (!this.formElement) {
            throw new Error('Part form element not found');
        }

        console.log('? [PART-FORM] Form element located');
    }

    async initializeValidation() {
        this.validationRules = {
            required: ['Part.PartNumber', 'Part.Name', 'Part.Description'],
            stages: {
                minimum: 0,
                requireAtLeastOne: false // Can be configured based on business rules
            }
        };

        console.log('? [PART-FORM] Validation rules configured');
    }

    async initializeStageManager() {
        try {
            // Wait for ModernStageManager to be available
            if (typeof ModernStageManager === 'undefined') {
                await this.waitForStageManager();
            }

            const partIdInput = document.querySelector('input[name="Part.Id"]');
            const partId = partIdInput ? parseInt(partIdInput.value) || null : null;

            this.stageManager = new ModernStageManager(partId);
            
            // Expose stage manager globally for backward compatibility
            window.stageManager = this.stageManager;

            // Register stage change callback
            this.stageManager.onStageChange = this.onStageSelectionChange.bind(this);

            console.log('? [PART-FORM] Stage manager initialized');

        } catch (error) {
            console.warn('?? [PART-FORM] Stage manager initialization failed, using fallback:', error);
            this.initializeFallbackStageManager();
        }
    }

    async waitForStageManager() {
        return new Promise((resolve, reject) => {
            let attempts = 0;
            const maxAttempts = 50; // 5 seconds max wait
            
            const checkForStageManager = () => {
                attempts++;
                
                if (typeof ModernStageManager !== 'undefined') {
                    resolve();
                } else if (attempts >= maxAttempts) {
                    reject(new Error('ModernStageManager failed to load'));
                } else {
                    setTimeout(checkForStageManager, 100);
                }
            };
            
            checkForStageManager();
        });
    }

    initializeFallbackStageManager() {
        // Create minimal fallback stage manager
        this.stageManager = {
            selectedStages: new Map(),
            initialized: true,
            getStageDataForSubmission: () => ({
                stageIds: [],
                executionOrders: [],
                estimatedHours: [],
                hourlyRates: [],
                materialCosts: []
            })
        };

        window.stageManager = this.stageManager;
        console.log('? [PART-FORM] Fallback stage manager active');
    }

    async initializeEventHandlers() {
        // Form submission handler
        this.formElement.addEventListener('submit', this.handleFormSubmit);

        // Stage function exposure for backward compatibility
        this.exposeGlobalStageFunctions();

        // Form field change handlers
        this.initializeFieldHandlers();

        console.log('? [PART-FORM] Event handlers initialized');
    }

    exposeGlobalStageFunctions() {
        // Global functions that work with the form manager
        window.addStageGlobal = (stageId) => {
            return this.executeStageAction('add', stageId);
        };

        window.removeStageGlobal = (stageId) => {
            return this.executeStageAction('remove', stageId);
        };

        window.editStageGlobal = (stageId) => {
            return this.executeStageAction('edit', stageId);
        };

        // Debug function
        window.debugPartForm = () => {
            return this.getDebugInformation();
        };

        console.log('? [PART-FORM] Global stage functions exposed');
    }

    executeStageAction(action, stageId) {
        try {
            if (!this.stageManager) {
                console.warn('?? [PART-FORM] Stage manager not available');
                return this.showStageActionFallback(action, stageId);
            }

            switch (action) {
                case 'add':
                    return this.stageManager.addStage ? this.stageManager.addStage(stageId) : false;
                case 'remove':
                    return this.stageManager.removeStage ? this.stageManager.removeStage(stageId) : false;
                case 'edit':
                    return this.stageManager.editStage ? this.stageManager.editStage(stageId) : false;
                default:
                    console.warn('?? [PART-FORM] Unknown stage action:', action);
                    return false;
            }
        } catch (error) {
            console.error('? [PART-FORM] Error executing stage action:', error);
            return this.showStageActionFallback(action, stageId);
        }
    }

    showStageActionFallback(action, stageId) {
        const stageNames = {
            1: 'SLS Printing',
            2: 'EDM Operations',
            3: 'CNC Machining',
            4: 'Assembly',
            5: 'Finishing'
        };

        const stageName = stageNames[stageId] || `Stage ${stageId}`;
        const message = `${action.charAt(0).toUpperCase() + action.slice(1)} ${stageName}`;

        if (confirm(`${message}? (Fallback mode - stage manager not fully loaded)`)) {
            this.showNotification('info', `${message} - will be processed when form is submitted`);
            return true;
        }

        return false;
    }

    handleFormSubmit(event) {
        console.log('?? [PART-FORM] Form submission initiated');

        // Prevent default submission
        event.preventDefault();

        // Prevent multiple submissions
        if (this.submitInProgress) {
            console.warn('?? [PART-FORM] Form submission already in progress');
            return false;
        }

        this.submitInProgress = true;
        this.showSubmitProgress(true);

        // Perform validation
        this.validateForm()
            .then(isValid => {
                if (isValid) {
                    return this.populateHiddenFields();
                } else {
                    throw new Error('Form validation failed');
                }
            })
            .then(() => {
                return this.submitForm();
            })
            .catch(error => {
                console.error('? [PART-FORM] Form submission failed:', error);
                this.showNotification('error', `Form submission failed: ${error.message}`);
            })
            .finally(() => {
                this.submitInProgress = false;
                this.showSubmitProgress(false);
            });

        return false;
    }

    async validateForm() {
        console.log('?? [PART-FORM] Validating form...');
        
        const errors = [];

        // Validate required fields
        for (const fieldName of this.validationRules.required) {
            const field = document.querySelector(`[name="${fieldName}"]`);
            if (!field || !field.value.trim()) {
                errors.push(`${fieldName.replace('Part.', '')} is required`);
                this.markFieldAsInvalid(field);
            } else {
                this.markFieldAsValid(field);
            }
        }

        // Validate stage requirements
        const stageValidation = this.validateStageRequirements();
        if (!stageValidation.isValid) {
            errors.push(...stageValidation.errors);
        }

        // Display validation errors
        if (errors.length > 0) {
            this.displayValidationErrors(errors);
            return false;
        }

        this.clearValidationErrors();
        console.log('? [PART-FORM] Form validation passed');
        return true;
    }

    validateStageRequirements() {
        const result = { isValid: true, errors: [] };

        try {
            if (!this.stageManager) {
                console.warn('?? [PART-FORM] Cannot validate stages - stage manager not available');
                return result; // Allow submission without stage validation
            }

            const stageData = this.stageManager.getStageDataForSubmission ? 
                this.stageManager.getStageDataForSubmission() : 
                { stageIds: [] };

            // Business rule: Check if at least one stage is required
            if (this.validationRules.stages.requireAtLeastOne && stageData.stageIds.length === 0) {
                result.isValid = false;
                result.errors.push('At least one manufacturing stage must be selected');
            }

            // Validate stage data consistency
            if (stageData.stageIds.length > 0) {
                const { stageIds, executionOrders, estimatedHours, hourlyRates, materialCosts } = stageData;
                
                if (stageIds.length !== executionOrders.length ||
                    stageIds.length !== estimatedHours.length ||
                    stageIds.length !== hourlyRates.length ||
                    stageIds.length !== materialCosts.length) {
                    result.isValid = false;
                    result.errors.push('Stage data is inconsistent - please refresh and try again');
                }
            }

        } catch (error) {
            console.error('? [PART-FORM] Error validating stage requirements:', error);
            // Don't fail validation due to stage validation errors
        }

        return result;
    }

    async populateHiddenFields() {
        console.log('?? [PART-FORM] Populating hidden form fields...');

        try {
            // Get stage data
            const stageData = this.stageManager && this.stageManager.getStageDataForSubmission ? 
                this.stageManager.getStageDataForSubmission() : 
                { stageIds: [], executionOrders: [], estimatedHours: [], hourlyRates: [], materialCosts: [] };

            // Populate hidden fields
            this.setHiddenFieldValue('selectedStageIds', stageData.stageIds.join(','));
            this.setHiddenFieldValue('stageExecutionOrders', stageData.executionOrders.join(','));
            this.setHiddenFieldValue('stageEstimatedHours', stageData.estimatedHours.join(','));
            this.setHiddenFieldValue('stageHourlyRates', stageData.hourlyRates.join(','));
            this.setHiddenFieldValue('stageMaterialCosts', stageData.materialCosts.join(','));

            console.log('? [PART-FORM] Hidden fields populated:', {
                stageCount: stageData.stageIds.length,
                stageIds: stageData.stageIds.join(',')
            });

        } catch (error) {
            console.error('? [PART-FORM] Error populating hidden fields:', error);
            throw error;
        }
    }

    setHiddenFieldValue(fieldId, value) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.value = value || '';
        } else {
            console.warn(`?? [PART-FORM] Hidden field not found: ${fieldId}`);
        }
    }

    async submitForm() {
        console.log('?? [PART-FORM] Submitting form to server...');

        // Create form data
        const formData = new FormData(this.formElement);
        
        // Determine handler
        const partId = formData.get('Part.Id');
        const handler = (!partId || partId === '0') ? 'Create' : 'Update';
        const url = `/Admin/Parts?handler=${handler}`;

        try {
            const response = await fetch(url, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'HX-Request': 'true'
                }
            });

            await this.handleSubmissionResponse(response);

        } catch (error) {
            console.error('? [PART-FORM] Network error during submission:', error);
            throw new Error('Network error - please check your connection and try again');
        }
    }

    async handleSubmissionResponse(response) {
        const responseText = await response.text();
        
        if (response.ok) {
            // Check for validation errors in response
            if (responseText.includes('validation-summary') || responseText.includes('field-validation-error')) {
                console.warn('?? [PART-FORM] Server returned validation errors');
                
                // Update form with server validation errors
                const parser = new DOMParser();
                const responseDoc = parser.parseFromString(responseText, 'text/html');
                this.updateFormWithServerErrors(responseDoc);
                
                throw new Error('Please fix the validation errors and try again');
            } else {
                // Success - redirect or close modal
                this.handleSuccessfulSubmission(response);
            }
        } else {
            throw new Error(`Server error (${response.status}): ${response.statusText}`);
        }
    }

    handleSuccessfulSubmission(response) {
        console.log('? [PART-FORM] Form submitted successfully');
        
        const redirectUrl = response.headers.get('HX-Redirect') || '/Admin/Parts';
        
        this.showNotification('success', 'Part saved successfully!');
        
        // Close modal and redirect
        setTimeout(() => {
            this.closeModal();
            window.location.href = redirectUrl;
        }, 1500);
    }

    // Utility methods
    markFieldAsInvalid(field) {
        if (field) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
        }
    }

    markFieldAsValid(field) {
        if (field) {
            field.classList.add('is-valid');
            field.classList.remove('is-invalid');
        }
    }

    displayValidationErrors(errors) {
        const errorContainer = document.getElementById('validation-summary') || this.createValidationSummary();
        
        errorContainer.innerHTML = `
            <div class="alert alert-danger">
                <h6><i class="fas fa-exclamation-triangle me-2"></i>Please fix the following errors:</h6>
                <ul class="mb-0">
                    ${errors.map(error => `<li>${error}</li>`).join('')}
                </ul>
            </div>
        `;
        
        errorContainer.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    clearValidationErrors() {
        const errorContainer = document.getElementById('validation-summary');
        if (errorContainer) {
            errorContainer.innerHTML = '';
        }
        
        // Clear field-level errors
        document.querySelectorAll('.is-invalid').forEach(field => {
            field.classList.remove('is-invalid');
        });
    }

    createValidationSummary() {
        const container = document.createElement('div');
        container.id = 'validation-summary';
        container.className = 'mb-3';
        
        const formBody = this.formElement.querySelector('.modal-body') || this.formElement;
        formBody.insertBefore(container, formBody.firstChild);
        
        return container;
    }

    showSubmitProgress(show) {
        const submitBtn = document.getElementById('savePartBtn');
        if (!submitBtn) return;

        if (show) {
            submitBtn.disabled = true;
            const spinner = submitBtn.querySelector('.spinner-border');
            const btnText = submitBtn.querySelector('.btn-text');
            
            if (spinner) spinner.classList.remove('d-none');
            if (btnText) btnText.textContent = 'Saving...';
        } else {
            submitBtn.disabled = false;
            const spinner = submitBtn.querySelector('.spinner-border');
            const btnText = submitBtn.querySelector('.btn-text');
            
            if (spinner) spinner.classList.add('d-none');
            if (btnText) {
                const partId = document.querySelector('input[name="Part.Id"]')?.value;
                btnText.textContent = (!partId || partId === '0') ? 'Create Part' : 'Update Part';
            }
        }
    }

    showNotification(type, message) {
        if (window.showToast) {
            window.showToast(type, message);
        } else if (window.showToastMessage) {
            window.showToastMessage(type, message);
        } else {
            alert(`${type.toUpperCase()}: ${message}`);
        }
    }

    closeModal() {
        const modal = document.getElementById('partModal');
        if (modal && typeof bootstrap !== 'undefined') {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        }
    }

    showInitializationError(error) {
        const container = document.getElementById('partModalContent') || document.body;
        
        const errorHtml = `
            <div class="alert alert-danger m-3">
                <h6><i class="fas fa-exclamation-triangle me-2"></i>Form Initialization Error</h6>
                <p>The part form failed to initialize properly. Please refresh the page and try again.</p>
                <details class="mt-2">
                    <summary>Technical Details</summary>
                    <pre class="mt-2 small">${error.message}\n${error.stack}</pre>
                </details>
                <div class="mt-3">
                    <button type="button" class="btn btn-primary" onclick="window.location.reload()">
                        <i class="fas fa-redo me-2"></i>Refresh Page
                    </button>
                </div>
            </div>
        `;
        
        container.innerHTML = errorHtml;
    }

    onStageSelectionChange() {
        // Callback when stage selection changes
        console.log('?? [PART-FORM] Stage selection changed');
        
        // Clear any existing stage validation errors
        this.clearStageValidationErrors();
    }

    clearStageValidationErrors() {
        const stageContainer = document.getElementById('stage-requirements-container');
        if (stageContainer) {
            const errorElements = stageContainer.querySelectorAll('.alert-danger');
            errorElements.forEach(el => el.remove());
        }
    }

    initializeFieldHandlers() {
        // Material sync handler
        const materialField = document.getElementById('materialSelect');
        const slsMaterialField = document.getElementById('slsMaterialInput');
        
        if (materialField && slsMaterialField) {
            materialField.addEventListener('change', () => {
                slsMaterialField.value = materialField.value;
            });
        }

        // Volume calculation
        const dimensionFields = document.querySelectorAll('[name="Part.LengthMm"], [name="Part.WidthMm"], [name="Part.HeightMm"]');
        dimensionFields.forEach(field => {
            field.addEventListener('input', this.calculateVolume.bind(this));
        });
    }

    calculateVolume() {
        try {
            const lengthField = document.querySelector('[name="Part.LengthMm"]');
            const widthField = document.querySelector('[name="Part.WidthMm"]');
            const heightField = document.querySelector('[name="Part.HeightMm"]');
            const volumeField = document.getElementById('volumeInput');

            if (!lengthField || !widthField || !heightField || !volumeField) return;

            const length = parseFloat(lengthField.value) || 0;
            const width = parseFloat(widthField.value) || 0;
            const height = parseFloat(heightField.value) || 0;

            const volume = length * width * height;
            volumeField.value = Math.round(volume);

        } catch (error) {
            console.error('? [PART-FORM] Error calculating volume:', error);
        }
    }

    updateFormWithServerErrors(responseDoc) {
        // Update validation summary
        const serverSummary = responseDoc.querySelector('.validation-summary');
        const clientSummary = document.getElementById('validation-summary');
        
        if (serverSummary && clientSummary) {
            clientSummary.innerHTML = serverSummary.innerHTML;
        }

        // Update field-level errors
        const serverErrors = responseDoc.querySelectorAll('.field-validation-error');
        serverErrors.forEach(serverError => {
            const fieldName = serverError.getAttribute('data-valmsg-for');
            if (fieldName) {
                const clientError = document.querySelector(`[data-valmsg-for="${fieldName}"]`);
                const field = document.querySelector(`[name="${fieldName}"]`);
                
                if (clientError) {
                    clientError.textContent = serverError.textContent;
                }
                if (field) {
                    this.markFieldAsInvalid(field);
                }
            }
        });
    }

    getDebugInformation() {
        return {
            initialized: this.initialized,
            formElement: !!this.formElement,
            stageManager: !!this.stageManager,
            stageManagerType: this.stageManager?.constructor?.name || 'Unknown',
            stageManagerInitialized: this.stageManager?.initialized || false,
            selectedStagesCount: this.stageManager?.selectedStages?.size || 0,
            validationRules: this.validationRules,
            submitInProgress: this.submitInProgress
        };
    }
}

// Initialize the form manager
let partFormManager;

document.addEventListener('DOMContentLoaded', () => {
    try {
        partFormManager = new OpCentrixPartFormManager();
        window.partFormManager = partFormManager; // Expose for debugging
        
        console.log('? [PART-FORM] OpCentrix Part Form Manager ready');
    } catch (error) {
        console.error('? [PART-FORM] Failed to initialize Part Form Manager:', error);
    }
});

// Legacy compatibility - expose the old global functions
window.calculateVolume = function() {
    if (partFormManager) {
        partFormManager.calculateVolume();
    }
};

window.updateSlsMaterial = function() {
    const materialField = document.getElementById('materialSelect');
    const slsMaterialField = document.getElementById('slsMaterialInput');
    
    if (materialField && slsMaterialField) {
        slsMaterialField.value = materialField.value;
    }
};

// Expose debugging function
window.debugPartForm = function() {
    return partFormManager ? partFormManager.getDebugInformation() : { error: 'Part Form Manager not initialized' };
};