/**
 * OpCentrix Form Validation & Error Handling System - PROMPT 2
 * Comprehensive client-side validation with real-time feedback
 * Matches .NET 8 Razor Pages validation patterns
 */

(function() {
    'use strict';
    
    // ========================================================================
    // COMPREHENSIVE FORM VALIDATION SYSTEM
    // ========================================================================
    
    class OpCentrixFormValidator {
        constructor() {
            this.validators = new Map();
            this.fieldStates = new Map();
            this.formStates = new Map();
            this.autoSaveTimers = new Map();
            this.isInitialized = false;
            
            this.init();
            console.log('? [FORM-VALIDATOR] OpCentrix Form Validation System initialized');
        }
        
        init() {
            if (this.isInitialized) return;
            
            // Initialize on DOM ready
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', () => this.setupValidation());
            } else {
                this.setupValidation();
            }
            
            this.isInitialized = true;
        }
        
        setupValidation() {
            // Setup global form validation
            this.setupGlobalValidation();
            
            // Setup parts-specific validation
            this.setupPartsValidation();
            
            // Setup auto-save functionality
            this.setupAutoSave();
            
            // Setup unsaved changes warning
            this.setupUnsavedChangesWarning();
            
            console.log('? [FORM-VALIDATOR] Validation setup complete');
        }
        
        // ====================================================================
        // GLOBAL FORM VALIDATION
        // ====================================================================
        
        setupGlobalValidation() {
            // Real-time validation for all forms
            document.addEventListener('input', (e) => {
                if (e.target.matches('input, textarea, select')) {
                    this.validateField(e.target);
                }
            });
            
            // Blur validation for more thorough checks
            document.addEventListener('blur', (e) => {
                if (e.target.matches('input, textarea, select')) {
                    this.validateField(e.target, true);
                }
            }, true);
            
            // Form submission validation
            document.addEventListener('submit', (e) => {
                if (e.target.matches('form')) {
                    this.validateForm(e.target, e);
                }
            });
            
            // Enhanced Bootstrap validation
            this.enhanceBootstrapValidation();
        }
        
        validateField(field, isBlur = false) {
            const fieldId = field.id || field.name || 'unknown';
            const fieldType = this.getFieldType(field);
            
            // Clear previous states
            this.clearFieldState(field);
            
            // Basic HTML5 validation
            let isValid = field.checkValidity();
            let errors = [];
            let warnings = [];
            
            // Custom validation based on field type
            const customValidation = this.runCustomValidation(field, fieldType, isBlur);
            if (!customValidation.isValid) {
                isValid = false;
                errors = errors.concat(customValidation.errors);
            }
            
            warnings = warnings.concat(customValidation.warnings || []);
            
            // Update field state
            this.updateFieldState(field, {
                isValid,
                errors,
                warnings,
                isDirty: field.value !== (field.defaultValue || ''),
                lastValidated: new Date()
            });
            
            // Update UI
            this.updateFieldUI(field, isValid, errors, warnings);
            
            // Cross-field validation
            if (isBlur) {
                this.runCrossFieldValidation(field);
            }
            
            return { isValid, errors, warnings };
        }
        
        runCustomValidation(field, fieldType, isBlur) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            const value = field.value.trim();
            
            switch (fieldType) {
                case 'part-number':
                    return this.validatePartNumber(field, value, isBlur);
                    
                case 'email':
                    return this.validateEmail(field, value);
                    
                case 'dimensions':
                    return this.validateDimensions(field, value);
                    
                case 'hours':
                    return this.validateHours(field, value);
                    
                case 'cost':
                    return this.validateCost(field, value);
                    
                case 'material':
                    return this.validateMaterial(field, value);
                    
                case 'admin-override':
                    return this.validateAdminOverride(field, value);
                    
                default:
                    return { isValid: true, errors: [], warnings: [] };
            }
        }
        
        // ====================================================================
        // PARTS-SPECIFIC VALIDATION
        // ====================================================================
        
        setupPartsValidation() {
            // Setup part number validation with duplicate checking
            this.setupPartNumberValidation();
            
            // Setup material-specific validation
            this.setupMaterialValidation();
            
            // Setup dimension validation with volume calculation
            this.setupDimensionValidation();
            
            // Setup cost validation
            this.setupCostValidation();
        }
        
        setupPartNumberValidation() {
            const partNumberInputs = document.querySelectorAll('[name="PartNumber"], .part-number-input, #Part\\.PartNumber');
            
            partNumberInputs.forEach(input => {
                input.classList.add('part-number-input');
                
                // Add validation indicator
                const validationDiv = document.createElement('div');
                validationDiv.className = 'part-number-validation';
                validationDiv.innerHTML = `
                    <span class="part-number-status" id="${input.id}-status"></span>
                    <small class="form-text">
                        <i class="fas fa-info-circle"></i>
                        Format: ABC-123 or ABC123 (3-50 characters)
                    </small>
                `;
                
                if (input.parentNode) {
                    input.parentNode.insertBefore(validationDiv, input.nextSibling);
                }
                
                // Real-time validation with debouncing
                let timeout;
                input.addEventListener('input', () => {
                    clearTimeout(timeout);
                    this.updatePartNumberStatus(input, 'checking');
                    
                    timeout = setTimeout(() => {
                        this.validatePartNumber(input, input.value, false);
                    }, 500);
                });
            });
        }
        
        validatePartNumber(field, value, isBlur = false) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            // Pattern validation
            const partNumberPattern = /^[A-Z0-9][A-Z0-9\-_]{2,49}$/i;
            
            if (value && !partNumberPattern.test(value)) {
                isValid = false;
                errors.push('Part number must be 3-50 characters, alphanumeric with hyphens/underscores');
                this.updatePartNumberStatus(field, 'invalid');
            } else if (value) {
                // Check for duplicate (only on blur or when complete)
                if (isBlur && value.length >= 3) {
                    this.checkPartNumberDuplicate(field, value);
                } else {
                    this.updatePartNumberStatus(field, 'valid');
                }
                
                // Format suggestions
                if (value.toLowerCase() === value) {
                    warnings.push('Consider using uppercase for consistency');
                }
            }
            
            return { isValid, errors, warnings };
        }
        
        async checkPartNumberDuplicate(field, partNumber) {
            try {
                this.updatePartNumberStatus(field, 'checking');
                
                const response = await fetch(`/Admin/Parts?handler=CheckDuplicate&partNumber=${encodeURIComponent(partNumber)}`, {
                    method: 'GET',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                
                if (response.ok) {
                    const result = await response.json();
                    
                    if (result.isDuplicate) {
                        this.updatePartNumberStatus(field, 'duplicate');
                        this.addFieldError(field, 'This part number is already in use');
                    } else {
                        this.updatePartNumberStatus(field, 'valid');
                        this.removeFieldError(field);
                    }
                } else {
                    this.updatePartNumberStatus(field, 'valid');
                }
            } catch (error) {
                console.warn('?? [FORM-VALIDATOR] Duplicate check failed:', error);
                this.updatePartNumberStatus(field, 'valid');
            }
        }
        
        updatePartNumberStatus(field, status) {
            const statusElement = document.getElementById(`${field.id}-status`);
            if (!statusElement) return;
            
            statusElement.className = `part-number-status ${status}`;
            
            const statusText = {
                'checking': 'Checking...',
                'valid': 'Available',
                'invalid': 'Invalid Format',
                'duplicate': 'Already Exists'
            };
            
            statusElement.textContent = statusText[status] || '';
        }
        
        // ====================================================================
        // MATERIAL-SPECIFIC VALIDATION
        // ====================================================================
        
        setupMaterialValidation() {
            const materialSelects = document.querySelectorAll('[name*="Material"], #materialSelect');
            
            materialSelects.forEach(select => {
                select.addEventListener('change', () => {
                    this.validateMaterialCompatibility(select);
                });
            });
        }
        
        validateMaterial(field, value) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            if (value) {
                // Check if material parameters are reasonable
                const form = field.closest('form');
                if (form) {
                    const laserPower = form.querySelector('[name*="LaserPower"]')?.value;
                    const scanSpeed = form.querySelector('[name*="ScanSpeed"]')?.value;
                    
                    if (laserPower && scanSpeed) {
                        const materialLimits = this.getMaterialLimits(value);
                        
                        if (laserPower < materialLimits.minLaserPower || laserPower > materialLimits.maxLaserPower) {
                            warnings.push(`Laser power outside recommended range (${materialLimits.minLaserPower}-${materialLimits.maxLaserPower}W)`);
                        }
                    }
                }
            }
            
            return { isValid, errors, warnings };
        }
        
        getMaterialLimits(material) {
            const materialLimits = {
                'Ti-6Al-4V Grade 5': { minLaserPower: 180, maxLaserPower: 220 },
                'Ti-6Al-4V ELI Grade 23': { minLaserPower: 160, maxLaserPower: 200 },
                'Inconel 718': { minLaserPower: 260, maxLaserPower: 310 },
                'Inconel 625': { minLaserPower: 250, maxLaserPower: 300 },
                '316L Stainless Steel': { minLaserPower: 180, maxLaserPower: 220 },
                '17-4 PH Stainless Steel': { minLaserPower: 175, maxLaserPower: 215 },
                'AlSi10Mg': { minLaserPower: 150, maxLaserPower: 200 }
            };
            
            return materialLimits[material] || { minLaserPower: 100, maxLaserPower: 400 };
        }
        
        // ====================================================================
        // DIMENSION VALIDATION WITH CROSS-FIELD CHECKS
        // ====================================================================
        
        setupDimensionValidation() {
            const dimensionInputs = document.querySelectorAll('[name*="Length"], [name*="Width"], [name*="Height"], [name*="Weight"]');
            
            dimensionInputs.forEach(input => {
                input.addEventListener('input', () => {
                    this.validateDimensionsGroup(input);
                });
            });
        }
        
        validateDimensions(field, value) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            const numValue = parseFloat(value);
            
            if (value && isNaN(numValue)) {
                isValid = false;
                errors.push('Must be a valid number');
            } else if (numValue < 0) {
                isValid = false;
                errors.push('Dimensions cannot be negative');
            } else if (numValue > 1000) {
                warnings.push('Unusually large dimension - please verify');
            }
            
            return { isValid, errors, warnings };
        }
        
        validateDimensionsGroup(field) {
            const form = field.closest('form');
            if (!form) return;
            
            const length = parseFloat(form.querySelector('[name*="Length"]')?.value || 0);
            const width = parseFloat(form.querySelector('[name*="Width"]')?.value || 0);
            const height = parseFloat(form.querySelector('[name*="Height"]')?.value || 0);
            const weight = parseFloat(form.querySelector('[name*="Weight"]')?.value || 0);
            
            // Calculate volume and validate against weight
            if (length > 0 && width > 0 && height > 0 && weight > 0) {
                const volume = length * width * height; // mm³
                const volumeInCm3 = volume / 1000; // cm³
                
                // Rough density check (very basic)
                const density = weight / volumeInCm3; // g/cm³
                
                if (density < 0.5 || density > 20) {
                    this.addFieldWarning(field, 'Density seems unusual - please verify dimensions and weight');
                }
            }
            
            // Update calculated fields
            if (window.calculateVolume) {
                window.calculateVolume();
            }
        }
        
        // ====================================================================
        // COST AND HOURS VALIDATION
        // ====================================================================
        
        validateHours(field, value) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            const numValue = parseFloat(value);
            
            if (value && isNaN(numValue)) {
                isValid = false;
                errors.push('Must be a valid number');
            } else if (numValue < 0) {
                isValid = false;
                errors.push('Hours cannot be negative');
            } else if (numValue > 168) {
                warnings.push('More than a week of work - please verify');
            } else if (numValue > 0 && numValue < 0.1) {
                warnings.push('Very short duration - consider minimum job time');
            }
            
            return { isValid, errors, warnings };
        }
        
        validateCost(field, value) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            const numValue = parseFloat(value);
            
            if (value && isNaN(numValue)) {
                isValid = false;
                errors.push('Must be a valid currency amount');
            } else if (numValue < 0) {
                isValid = false;
                errors.push('Cost cannot be negative');
            } else if (numValue > 10000) {
                warnings.push('High cost - please verify');
            }
            
            return { isValid, errors, warnings };
        }
        
        validateAdminOverride(field, value) {
            let isValid = true;
            let errors = [];
            let warnings = [];
            
            const numValue = parseFloat(value);
            
            if (value && numValue > 0) {
                // Admin override is being used
                const form = field.closest('form');
                const reasonField = form?.querySelector('[name*="AdminOverrideReason"]');
                
                if (!reasonField?.value?.trim()) {
                    warnings.push('Admin override reason is recommended');
                }
                
                // Compare with standard hours
                const standardHours = form?.querySelector('[name*="EstimatedHours"]')?.value;
                if (standardHours) {
                    const standardValue = parseFloat(standardHours);
                    const difference = Math.abs(numValue - standardValue);
                    const percentChange = (difference / standardValue) * 100;
                    
                    if (percentChange > 50) {
                        warnings.push(`Override differs significantly from standard (${percentChange.toFixed(0)}% change)`);
                    }
                }
            }
            
            return { isValid, errors, warnings };
        }
        
        // ====================================================================
        // CROSS-FIELD VALIDATION
        // ====================================================================
        
        runCrossFieldValidation(field) {
            const form = field.closest('form');
            if (!form) return;
            
            // Validate material compatibility
            this.validateMaterialCompatibility(form);
            
            // Validate dimension consistency
            this.validateDimensionsGroup(field);
            
            // Validate cost consistency
            this.validateCostConsistency(form);
        }
        
        validateMaterialCompatibility(form) {
            const materialField = form.querySelector('[name*="Material"]:not([name*="Sls"])');
            const slsMaterialField = form.querySelector('[name*="SlsMaterial"]');
            
            if (materialField?.value && slsMaterialField?.value) {
                if (materialField.value !== slsMaterialField.value) {
                    // Different materials - this might be intentional
                    this.addFieldWarning(slsMaterialField, 'SLS material differs from base material');
                }
            }
        }
        
        validateCostConsistency(form) {
            const materialCost = parseFloat(form.querySelector('[name*="MaterialCost"]')?.value || 0);
            const laborCost = parseFloat(form.querySelector('[name*="LaborCost"]')?.value || 0);
            const hours = parseFloat(form.querySelector('[name*="EstimatedHours"]')?.value || 0);
            
            if (materialCost > 0 && laborCost > 0 && hours > 0) {
                const totalLaborCost = laborCost * hours;
                
                if (materialCost > totalLaborCost * 5) {
                    this.addFieldWarning(form.querySelector('[name*="MaterialCost"]'), 'Material cost is very high compared to labor');
                } else if (totalLaborCost > materialCost * 10) {
                    this.addFieldWarning(form.querySelector('[name*="LaborCost"]'), 'Labor cost is very high compared to material');
                }
            }
        }
        
        // ====================================================================
        // UI UPDATE METHODS
        // ====================================================================
        
        updateFieldUI(field, isValid, errors, warnings) {
            // Clear existing states
            field.classList.remove('is-valid', 'is-invalid', 'is-warning');
            
            // Apply new state
            if (errors.length > 0) {
                field.classList.add('is-invalid');
                this.showFieldErrors(field, errors);
            } else if (warnings.length > 0) {
                field.classList.add('is-warning');
                this.showFieldWarnings(field, warnings);
            } else if (field.value.trim() && isValid) {
                field.classList.add('is-valid');
                this.clearFieldMessages(field);
            } else {
                this.clearFieldMessages(field);
            }
        }
        
        showFieldErrors(field, errors) {
            this.clearFieldMessages(field);
            
            errors.forEach(error => {
                const errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback';
                errorDiv.textContent = error;
                
                if (field.parentNode) {
                    field.parentNode.insertBefore(errorDiv, field.nextSibling);
                }
            });
        }
        
        showFieldWarnings(field, warnings) {
            this.clearFieldMessages(field);
            
            warnings.forEach(warning => {
                const warningDiv = document.createElement('div');
                warningDiv.className = 'form-text text-warning';
                warningDiv.innerHTML = `<i class="fas fa-exclamation-triangle"></i> ${warning}`;
                
                if (field.parentNode) {
                    field.parentNode.insertBefore(warningDiv, field.nextSibling);
                }
            });
        }
        
        clearFieldMessages(field) {
            if (!field.parentNode) return;
            
            const messages = field.parentNode.querySelectorAll('.invalid-feedback, .valid-feedback, .form-text.text-warning');
            messages.forEach(msg => {
                if (msg.parentNode === field.parentNode) {
                    msg.remove();
                }
            });
        }
        
        addFieldError(field, message) {
            field.classList.add('is-invalid');
            this.showFieldErrors(field, [message]);
        }
        
        addFieldWarning(field, message) {
            field.classList.add('is-warning');
            this.showFieldWarnings(field, [message]);
        }
        
        removeFieldError(field) {
            field.classList.remove('is-invalid', 'is-warning');
            this.clearFieldMessages(field);
            
            if (field.value.trim()) {
                field.classList.add('is-valid');
            }
        }
        
        clearFieldState(field) {
            field.classList.remove('is-valid', 'is-invalid', 'is-warning', 'field-validating');
        }
        
        // ====================================================================
        // FORM-LEVEL VALIDATION
        // ====================================================================
        
        validateForm(form, event) {
            let isFormValid = true;
            let errorCount = 0;
            const errors = [];
            
            // Validate all fields
            const fields = form.querySelectorAll('input, textarea, select');
            fields.forEach(field => {
                if (!field.disabled && !field.readOnly) {
                    const result = this.validateField(field, true);
                    if (!result.isValid) {
                        isFormValid = false;
                        errorCount += result.errors.length;
                        errors.push(...result.errors);
                    }
                }
            });
            
            // Show form-level validation summary if there are errors
            if (!isFormValid) {
                event.preventDefault();
                this.showValidationSummary(form, errors);
                this.focusFirstInvalidField(form);
            }
            
            return isFormValid;
        }
        
        showValidationSummary(form, errors) {
            // Remove existing summary
            const existingSummary = form.querySelector('.validation-summary');
            if (existingSummary) {
                existingSummary.remove();
            }
            
            // Create new summary
            const summaryDiv = document.createElement('div');
            summaryDiv.className = 'validation-summary alert alert-danger';
            summaryDiv.innerHTML = `
                <div class="alert-heading">
                    <i class="fas fa-exclamation-triangle"></i>
                    Please correct the following issues:
                </div>
                <ul>
                    ${errors.map(error => `<li>${error}</li>`).join('')}
                </ul>
            `;
            
            // Insert at top of form
            form.insertBefore(summaryDiv, form.firstChild);
            
            // Scroll to summary
            summaryDiv.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
        
        focusFirstInvalidField(form) {
            const firstInvalidField = form.querySelector('.is-invalid');
            if (firstInvalidField) {
                firstInvalidField.focus();
            }
        }
        
        // ====================================================================
        // AUTO-SAVE FUNCTIONALITY
        // ====================================================================
        
        setupAutoSave() {
            const forms = document.querySelectorAll('form[data-auto-save], .parts-form');
            
            forms.forEach(form => {
                this.initAutoSaveForForm(form);
            });
        }
        
        initAutoSaveForForm(form) {
            const formId = form.id || 'form-' + Date.now();
            let saveTimer;
            let hasUnsavedChanges = false;
            
            // Create auto-save indicator
            const indicator = this.createAutoSaveIndicator();
            document.body.appendChild(indicator);
            
            // Watch for changes
            form.addEventListener('input', () => {
                hasUnsavedChanges = true;
                this.showAutoSaveIndicator(indicator, 'saving');
                
                clearTimeout(saveTimer);
                saveTimer = setTimeout(() => {
                    this.autoSaveForm(form, indicator);
                }, 2000); // 2 second delay
            });
            
            // Store reference
            this.autoSaveTimers.set(formId, { timer: saveTimer, indicator, hasUnsavedChanges });
        }
        
        createAutoSaveIndicator() {
            const indicator = document.createElement('div');
            indicator.className = 'form-auto-save';
            indicator.innerHTML = `
                <i class="fas fa-save"></i>
                <span class="status-text">Changes saved</span>
            `;
            return indicator;
        }
        
        showAutoSaveIndicator(indicator, status) {
            indicator.classList.add('visible');
            indicator.classList.remove('saving', 'saved', 'error');
            indicator.classList.add(status);
            
            const statusText = {
                'saving': 'Saving...',
                'saved': 'Changes saved',
                'error': 'Save failed'
            };
            
            const statusIcon = {
                'saving': 'fa-spinner fa-spin',
                'saved': 'fa-check',
                'error': 'fa-exclamation-triangle'
            };
            
            indicator.querySelector('.status-text').textContent = statusText[status];
            indicator.querySelector('i').className = `fas ${statusIcon[status]}`;
            
            if (status === 'saved') {
                setTimeout(() => {
                    indicator.classList.remove('visible');
                }, 3000);
            }
        }
        
        async autoSaveForm(form, indicator) {
            try {
                const formData = new FormData(form);
                const data = Object.fromEntries(formData.entries());
                
                // Save to localStorage as backup
                localStorage.setItem(`autosave-${form.id}`, JSON.stringify({
                    data,
                    timestamp: new Date().toISOString()
                }));
                
                this.showAutoSaveIndicator(indicator, 'saved');
                
            } catch (error) {
                console.error('Auto-save failed:', error);
                this.showAutoSaveIndicator(indicator, 'error');
            }
        }
        
        // ====================================================================
        // UNSAVED CHANGES WARNING
        // ====================================================================
        
        setupUnsavedChangesWarning() {
            let hasUnsavedChanges = false;
            
            // Track form changes
            document.addEventListener('input', (e) => {
                if (e.target.matches('form input, form textarea, form select')) {
                    hasUnsavedChanges = true;
                    this.showUnsavedChangesWarning();
                }
            });
            
            // Clear on form submission
            document.addEventListener('submit', () => {
                hasUnsavedChanges = false;
                this.hideUnsavedChangesWarning();
            });
            
            // Warn on page unload
            window.addEventListener('beforeunload', (e) => {
                if (hasUnsavedChanges) {
                    const message = 'You have unsaved changes. Are you sure you want to leave?';
                    e.returnValue = message;
                    return message;
                }
            });
        }
        
        showUnsavedChangesWarning() {
            let warning = document.getElementById('unsaved-changes-warning');
            
            if (!warning) {
                warning = document.createElement('div');
                warning.id = 'unsaved-changes-warning';
                warning.className = 'unsaved-changes-warning';
                warning.innerHTML = `
                    <i class="fas fa-exclamation-triangle"></i>
                    You have unsaved changes
                    <button onclick="this.parentElement.classList.remove('visible')" style="background: none; border: none; color: inherit; margin-left: 10px;">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.body.appendChild(warning);
            }
            
            warning.classList.add('visible');
        }
        
        hideUnsavedChangesWarning() {
            const warning = document.getElementById('unsaved-changes-warning');
            if (warning) {
                warning.classList.remove('visible');
            }
        }
        
        // ====================================================================
        // UTILITY METHODS
        // ====================================================================
        
        getFieldType(field) {
            // Determine field type based on name, class, or data attributes
            const name = field.name?.toLowerCase() || '';
            const className = field.className?.toLowerCase() || '';
            const type = field.type?.toLowerCase() || '';
            
            if (name.includes('partnumber') || className.includes('part-number')) {
                return 'part-number';
            } else if (type === 'email' || name.includes('email')) {
                return 'email';
            } else if (name.includes('length') || name.includes('width') || name.includes('height') || name.includes('weight')) {
                return 'dimensions';
            } else if (name.includes('hours') || name.includes('duration')) {
                return 'hours';
            } else if (name.includes('cost') || name.includes('price') || type === 'number' && name.includes('$')) {
                return 'cost';
            } else if (name.includes('material')) {
                return 'material';
            } else if (name.includes('adminoverride') || name.includes('override')) {
                return 'admin-override';
            }
            
            return 'default';
        }
        
        updateFieldState(field, state) {
            const fieldId = field.id || field.name || 'unknown';
            this.fieldStates.set(fieldId, state);
        }
        
        getFieldState(field) {
            const fieldId = field.id || field.name || 'unknown';
            return this.fieldStates.get(fieldId) || {};
        }
        
        enhanceBootstrapValidation() {
            // Enhance existing Bootstrap validation with our custom styling
            document.addEventListener('invalid', (e) => {
                e.target.classList.add('is-invalid');
            }, true);
            
            document.addEventListener('valid', (e) => {
                if (e.target.value.trim()) {
                    e.target.classList.add('is-valid');
                    e.target.classList.remove('is-invalid');
                }
            }, true);
        }
    }
    
    // ========================================================================
    // CONFIRMATION DIALOG SYSTEM
    // ========================================================================
    
    class OpCentrixConfirmationDialog {
        static show(title, message, options = {}) {
            return new Promise((resolve) => {
                const dialog = document.createElement('div');
                dialog.className = 'confirmation-dialog';
                dialog.innerHTML = `
                    <div class="confirmation-dialog-content">
                        <div class="confirmation-dialog-title">
                            <i class="${options.icon || 'fas fa-question-circle'}"></i>
                            ${title}
                        </div>
                        <div class="confirmation-dialog-message">
                            ${message}
                        </div>
                        <div class="confirmation-dialog-actions">
                            <button class="btn btn-outline-secondary" data-action="cancel">
                                ${options.cancelText || 'Cancel'}
                            </button>
                            <button class="btn ${options.confirmClass || 'btn-primary'}" data-action="confirm">
                                ${options.confirmText || 'Confirm'}
                            </button>
                        </div>
                    </div>
                `;
                
                document.body.appendChild(dialog);
                
                // Show dialog
                setTimeout(() => dialog.classList.add('visible'), 10);
                
                // Handle clicks
                dialog.addEventListener('click', (e) => {
                    const action = e.target.dataset.action;
                    if (action) {
                        dialog.classList.remove('visible');
                        setTimeout(() => document.body.removeChild(dialog), 300);
                        resolve(action === 'confirm');
                    } else if (e.target === dialog) {
                        // Clicked backdrop
                        dialog.classList.remove('visible');
                        setTimeout(() => document.body.removeChild(dialog), 300);
                        resolve(false);
                    }
                });
                
                // Handle escape key
                const escapeHandler = (e) => {
                    if (e.key === 'Escape') {
                        dialog.classList.remove('visible');
                        setTimeout(() => document.body.removeChild(dialog), 300);
                        document.removeEventListener('keydown', escapeHandler);
                        resolve(false);
                    }
                };
                document.addEventListener('keydown', escapeHandler);
            });
        }
    }
    
    // ========================================================================
    // INITIALIZE SYSTEM
    // ========================================================================
    
    // Initialize the form validation system
    const formValidator = new OpCentrixFormValidator();
    
    // Expose global methods
    window.OpCentrixFormValidator = formValidator;
    window.OpCentrixConfirmationDialog = OpCentrixConfirmationDialog;
    
    // Enhanced global functions for parts page
    window.confirmDelete = async function(itemType, itemName) {
        return await OpCentrixConfirmationDialog.show(
            `Delete ${itemType}`,
            `Are you sure you want to delete "${itemName}"? This action cannot be undone.`,
            {
                icon: 'fas fa-trash',
                confirmText: 'Delete',
                confirmClass: 'btn-danger',
                cancelText: 'Cancel'
            }
        );
    };
    
    window.validatePartForm = function(form) {
        return formValidator.validateForm(form, { preventDefault: () => {} });
    };
    
    console.log('? [FORM-VALIDATOR] OpCentrix Form Validation & Error Handling System loaded successfully');
    
})();