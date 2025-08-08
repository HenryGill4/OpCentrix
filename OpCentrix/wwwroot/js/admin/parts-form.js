/**
 * OpCentrix Parts Form Management
 * Enhanced client-side validation and form handling for parts administration
 * @version 2.0
 * @author OpCentrix Development Team
 */

(function() {
    'use strict';
    
    // Wait for DOM and ensure proper initialization
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePartsForm);
    } else {
        initializePartsForm();
    }
    
    function initializePartsForm() {
        try {
            setupTabs();
            setupMaterialAutoFill();
            setupVolumeCalculation();
            setupAdminOverrideValidation();
            setupStageCalculations();
            setupEnhancedFormValidation(); // Enhanced validation
            setupFormSubmission(); // Enhanced submission handling
            
            console.log('[PARTS] Form initialized successfully with enhanced validation');
        } catch (error) {
            console.error('[PARTS] Error initializing form:', error);
        }
    }
    
    function setupTabs() {
        const tabTriggers = document.querySelectorAll('[data-bs-toggle="tab"]');
        
        tabTriggers.forEach(trigger => {
            trigger.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Remove active classes
                document.querySelectorAll('.nav-link').forEach(tab => tab.classList.remove('active'));
                document.querySelectorAll('.tab-pane').forEach(pane => {
                    pane.classList.remove('show', 'active');
                });
                
                // Add active class to clicked tab
                this.classList.add('active');
                
                // Show corresponding tab content
                const targetId = this.getAttribute('data-bs-target');
                const targetPane = document.querySelector(targetId);
                if (targetPane) {
                    targetPane.classList.add('show', 'active');
                }
                
                console.log('[PARTS] Tab switched to:', targetId);
            });
        });
    }
    
    function setupMaterialAutoFill() {
        const materialSelect = document.getElementById('materialSelect');
        if (materialSelect) {
            materialSelect.addEventListener('change', function() {
                if (window.updateSlsMaterial) {
                    window.updateSlsMaterial();
                }
            });
        }
    }
    
    function setupVolumeCalculation() {
        const lengthInput = document.querySelector('input[name="Part.LengthMm"]');
        const widthInput = document.querySelector('input[name="Part.WidthMm"]');
        const heightInput = document.querySelector('input[name="Part.HeightMm"]');
        
        if (lengthInput && widthInput && heightInput) {
            [lengthInput, widthInput, heightInput].forEach(input => {
                input.addEventListener('change', function() {
                    if (window.calculateVolume) {
                        window.calculateVolume();
                    }
                });
            });
        }
    }
    
    function setupAdminOverrideValidation() {
        const overrideInput = document.getElementById('adminOverrideInput');
        const reasonTextarea = document.getElementById('overrideReasonText');
        
        if (overrideInput && reasonTextarea) {
            overrideInput.addEventListener('input', function() {
                const hasOverride = parseFloat(this.value) > 0;
                reasonTextarea.required = hasOverride;
                
                // Show/hide validation message
                const helpText = reasonTextarea.closest('.col-md-8').querySelector('.form-text');
                if (helpText) {
                    helpText.style.display = hasOverride ? 'block' : 'none';
                    helpText.style.color = hasOverride ? '#dc3545' : '#6c757d';
                }
                
                // Add visual indicators
                if (hasOverride) {
                    reasonTextarea.classList.add('border-warning');
                    if (!reasonTextarea.value.trim()) {
                        reasonTextarea.classList.add('is-invalid');
                    }
                } else {
                    reasonTextarea.classList.remove('border-warning', 'is-invalid');
                }
            });
            
            reasonTextarea.addEventListener('input', function() {
                const hasOverride = parseFloat(overrideInput.value) > 0;
                if (hasOverride && this.value.trim()) {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                } else if (hasOverride && !this.value.trim()) {
                    this.classList.add('is-invalid');
                    this.classList.remove('is-valid');
                }
            });
        }
    }

    // ENHANCED: Comprehensive client-side validation
    function setupEnhancedFormValidation() {
        const form = document.getElementById('partForm');
        if (!form) return;

        // Get all form fields that need validation
        const requiredFields = form.querySelectorAll('[required]');
        const numericFields = form.querySelectorAll('input[type="number"]');
        const selectFields = form.querySelectorAll('select[required]');
        const textFields = form.querySelectorAll('input[type="text"][required], textarea[required]');

        // Setup real-time validation for required fields
        requiredFields.forEach(field => {
            // Validate on blur (when user leaves field)
            field.addEventListener('blur', function() {
                validateField(this);
            });

            // Validate on input (for text fields)
            if (field.type === 'text' || field.tagName === 'TEXTAREA') {
                field.addEventListener('input', debounce(function() {
                    validateField(this);
                }, 300));
            }

            // Validate on change (for selects and numbers)
            field.addEventListener('change', function() {
                validateField(this);
            });
        });

        // Setup specific validations
        setupPartNumberValidation();
        setupNumericValidation();
        setupStageValidation();
        setupBusinessLogicValidation();

        console.log('[PARTS] Enhanced validation setup complete');
    }

    function setupPartNumberValidation() {
        const partNumberInput = document.querySelector('input[name="Part.PartNumber"]');
        if (!partNumberInput) return;

        partNumberInput.addEventListener('blur', async function() {
            const partNumber = this.value.trim();
            
            if (!partNumber) {
                showFieldError(this, 'Part number is required');
                return;
            }

            // Format validation
            const partNumberPattern = /^[A-Z0-9][A-Z0-9\-_]{2,49}$/i;
            if (!partNumberPattern.test(partNumber)) {
                showFieldError(this, 'Part number must be 3-50 characters, alphanumeric with hyphens/underscores only');
                return;
            }

            // Check for duplicates (if we have a duplicate check endpoint)
            try {
                const partId = document.querySelector('input[name="Part.Id"]')?.value || 0;
                const response = await fetch(`/Admin/Parts?handler=CheckDuplicate&partNumber=${encodeURIComponent(partNumber)}&excludeId=${partId}`);
                
                if (response.ok) {
                    const result = await response.json();
                    if (result.isDuplicate) {
                        showFieldError(this, `Part number "${partNumber}" already exists`);
                        return;
                    }
                }
            } catch (error) {
                console.warn('[PARTS] Could not check for duplicate part number:', error);
            }

            showFieldSuccess(this);
        });
    }

    function setupNumericValidation() {
        const estimatedHoursInput = document.querySelector('input[name="Part.EstimatedHours"]');
        const materialCostInput = document.querySelector('input[name="Part.MaterialCostPerKg"]');
        const laborCostInput = document.querySelector('input[name="Part.StandardLaborCostPerHour"]');

        // Estimated Hours validation
        if (estimatedHoursInput) {
            estimatedHoursInput.addEventListener('blur', function() {
                const hours = parseFloat(this.value);
                if (isNaN(hours) || hours <= 0) {
                    showFieldError(this, 'Estimated hours must be greater than 0');
                } else if (hours > 200) {
                    showFieldError(this, 'Estimated hours cannot exceed 200 hours');
                } else {
                    showFieldSuccess(this);
                }
            });
        }

        // Material Cost validation
        if (materialCostInput) {
            materialCostInput.addEventListener('blur', function() {
                const cost = parseFloat(this.value);
                if (cost < 0) {
                    showFieldError(this, 'Material cost cannot be negative');
                } else if (cost > 10000) {
                    showFieldError(this, 'Material cost seems unreasonably high (over $10,000/kg)');
                } else {
                    showFieldSuccess(this);
                }
            });
        }

        // Labor Cost validation
        if (laborCostInput) {
            laborCostInput.addEventListener('blur', function() {
                const cost = parseFloat(this.value);
                if (cost < 0) {
                    showFieldError(this, 'Labor cost cannot be negative');
                } else if (cost > 500) {
                    showFieldError(this, 'Labor cost seems unreasonably high (over $500/hour)');
                } else {
                    showFieldSuccess(this);
                }
            });
        }
    }

    function setupStageValidation() {
        const stageCheckboxes = document.querySelectorAll('input[type="checkbox"][name^="Part.Requires"]');
        
        stageCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                validateStageSelection();
            });
        });
    }

    function setupBusinessLogicValidation() {
        const btComponentTypeSelect = document.querySelector('select[name="Part.BTComponentType"]');
        const requiresTaxStampCheckbox = document.querySelector('input[name="Part.RequiresTaxStamp"]');
        const atfClassificationInput = document.querySelector('input[name="Part.ATFClassification"]');
        const requiresATFForm1Checkbox = document.querySelector('input[name="Part.RequiresATFForm1"]');
        const requiresATFForm4Checkbox = document.querySelector('input[name="Part.RequiresATFForm4"]');

        // B&T business logic validation
        if (btComponentTypeSelect && requiresTaxStampCheckbox) {
            btComponentTypeSelect.addEventListener('change', function() {
                if (this.value === 'Suppressor' && !requiresTaxStampCheckbox.checked) {
                    showFieldWarning(requiresTaxStampCheckbox.closest('.form-check'), 'Suppressor components typically require a tax stamp');
                }
            });
        }

        // ATF classification validation
        if (atfClassificationInput && (requiresATFForm1Checkbox || requiresATFForm4Checkbox)) {
            [requiresATFForm1Checkbox, requiresATFForm4Checkbox].forEach(checkbox => {
                if (checkbox) {
                    checkbox.addEventListener('change', function() {
                        if ((requiresATFForm1Checkbox?.checked || requiresATFForm4Checkbox?.checked) && !atfClassificationInput.value.trim()) {
                            showFieldError(atfClassificationInput, 'ATF Classification is required when ATF forms are needed');
                        } else if (atfClassificationInput.value.trim()) {
                            showFieldSuccess(atfClassificationInput);
                        }
                    });
                }
            });
        }
    }

    // ENHANCED: Form submission with comprehensive validation
    function setupFormSubmission() {
        const form = document.getElementById('partForm');
        if (!form) return;

        form.addEventListener('submit', function(e) {
            console.log('[PARTS] Form submission started - performing validation');

            // Prevent default submission initially
            e.preventDefault();

            // Clear any existing validation summary
            clearValidationSummary();

            // Perform comprehensive validation
            const validationResult = performCompleteValidation();
            
            if (!validationResult.isValid) {
                console.log('[PARTS] Validation failed:', validationResult.errors);
                
                // Show validation summary
                showValidationSummary(validationResult.errors);
                
                // Focus on first error field
                if (validationResult.firstErrorField) {
                    focusErrorField(validationResult.firstErrorField);
                }

                // Show user-friendly error message
                if (window.showToast) {
                    window.showToast('error', `Please fix ${validationResult.errors.length} validation error(s) before submitting.`);
                }

                return false;
            }

            console.log('[PARTS] All validation passed - proceeding with submission');

            // Show loading state
            showSubmissionLoading();

            // Allow the form to submit via HTMX
            // Remove the event listener temporarily to allow natural submission
            form.removeEventListener('submit', arguments.callee);
            
            // Trigger the form submission
            if (form.hasAttribute('hx-post')) {
                // HTMX will handle the submission
                htmx.trigger(form, 'submit');
            } else {
                // Fallback to normal form submission
                form.submit();
            }

            // Re-add the event listener for future submissions
            setTimeout(() => {
                form.addEventListener('submit', arguments.callee);
            }, 100);
        });
    }

    // ENHANCED: Complete validation function
    function performCompleteValidation() {
        const errors = [];
        let firstErrorField = null;

        // 1. Required field validation
        const requiredFields = document.querySelectorAll('#partForm [required]');
        requiredFields.forEach(field => {
            if (!validateField(field)) {
                const fieldName = getFieldDisplayName(field);
                errors.push(`${fieldName} is required`);
                if (!firstErrorField) firstErrorField = field;
            }
        });

        // 2. Manufacturing stages validation
        const stageValidation = validateStageSelection();
        if (!stageValidation.isValid) {
            errors.push(stageValidation.error);
            if (!firstErrorField) firstErrorField = document.querySelector('#stages-tab');
        }

        // 3. Business logic validation
        const businessValidation = validateBusinessLogic();
        if (!businessValidation.isValid) {
            errors.push(...businessValidation.errors);
            if (!firstErrorField && businessValidation.firstErrorField) {
                firstErrorField = businessValidation.firstErrorField;
            }
        }

        // 4. Admin override validation
        const overrideValidation = validateAdminOverride();
        if (!overrideValidation.isValid) {
            errors.push(overrideValidation.error);
            if (!firstErrorField) firstErrorField = document.getElementById('overrideReasonText');
        }

        return {
            isValid: errors.length === 0,
            errors: errors,
            firstErrorField: firstErrorField
        };
    }

    function validateField(field) {
        if (!field) return true;

        const value = field.type === 'checkbox' ? field.checked : field.value.trim();

        // Required field validation
        if (field.hasAttribute('required')) {
            if (field.type === 'checkbox' && !field.checked) {
                // For required checkboxes, we typically want them to be checked
                showFieldError(field, `${getFieldDisplayName(field)} is required`);
                return false;
            } else if (field.type !== 'checkbox' && !value) {
                showFieldError(field, `${getFieldDisplayName(field)} is required`);
                return false;
            }
        }

        // Type-specific validation
        if (value) {
            if (field.type === 'number') {
                const numValue = parseFloat(value);
                const min = parseFloat(field.getAttribute('min'));
                const max = parseFloat(field.getAttribute('max'));

                if (isNaN(numValue)) {
                    showFieldError(field, 'Please enter a valid number');
                    return false;
                }

                if (!isNaN(min) && numValue < min) {
                    showFieldError(field, `Value must be at least ${min}`);
                    return false;
                }

                if (!isNaN(max) && numValue > max) {
                    showFieldError(field, `Value cannot exceed ${max}`);
                    return false;
                }
            }

            if (field.tagName === 'SELECT' && field.hasAttribute('required') && !value) {
                showFieldError(field, `Please select a ${getFieldDisplayName(field).toLowerCase()}`);
                return false;
            }
        }

        showFieldSuccess(field);
        return true;
    }

    function validateStageSelection() {
        const stageCheckboxes = document.querySelectorAll('input[type="checkbox"][name^="Part.Requires"]:checked');
        
        if (stageCheckboxes.length === 0) {
            // Highlight the stages tab
            const stagesTab = document.querySelector('#stages-tab');
            if (stagesTab) {
                stagesTab.classList.add('text-danger');
                setTimeout(() => stagesTab.classList.remove('text-danger'), 5000);
            }
            
            return {
                isValid: false,
                error: 'At least one manufacturing stage must be selected'
            };
        }

        return { isValid: true };
    }

    function validateBusinessLogic() {
        const errors = [];
        let firstErrorField = null;

        // B&T specific validations
        const btComponentType = document.querySelector('select[name="Part.BTComponentType"]')?.value;
        const requiresTaxStamp = document.querySelector('input[name="Part.RequiresTaxStamp"]')?.checked;
        const atfClassification = document.querySelector('input[name="Part.ATFClassification"]')?.value?.trim();
        const requiresATFForm1 = document.querySelector('input[name="Part.RequiresATFForm1"]')?.checked;
        const requiresATFForm4 = document.querySelector('input[name="Part.RequiresATFForm4"]')?.checked;

        // Suppressor validation
        if (btComponentType === 'Suppressor' && !requiresTaxStamp) {
            errors.push('Suppressor components typically require a tax stamp');
        }

        // ATF form validation
        if ((requiresATFForm1 || requiresATFForm4) && !atfClassification) {
            errors.push('ATF Classification is required when ATF forms are needed');
            if (!firstErrorField) {
                firstErrorField = document.querySelector('input[name="Part.ATFClassification"]');
            }
        }

        return {
            isValid: errors.length === 0,
            errors: errors,
            firstErrorField: firstErrorField
        };
    }

    function validateAdminOverride() {
        const overrideInput = document.getElementById('adminOverrideInput');
        const reasonTextarea = document.getElementById('overrideReasonText');

        if (overrideInput && reasonTextarea) {
            const hasOverride = parseFloat(overrideInput.value) > 0;
            if (hasOverride && !reasonTextarea.value.trim()) {
                return {
                    isValid: false,
                    error: 'Override justification is required when override hours are specified'
                };
            }
        }

        return { isValid: true };
    }

    // ENHANCED: Visual feedback functions
    function showFieldError(field, message) {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');
        
        showFieldMessage(field, message, 'error');
        return false;
    }

    function showFieldSuccess(field) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
        
        hideFieldMessage(field);
        return true;
    }

    function showFieldWarning(field, message) {
        showFieldMessage(field, message, 'warning');
    }

    function showFieldMessage(field, message, type = 'error') {
        // Remove existing message
        hideFieldMessage(field);

        // Create message element
        const messageElement = document.createElement('div');
        messageElement.className = `field-validation-message alert alert-${type === 'error' ? 'danger' : 'warning'} alert-sm mt-1`;
        messageElement.innerHTML = `<i class="fas fa-${type === 'error' ? 'exclamation-circle' : 'exclamation-triangle'} me-1"></i>${message}`;
        messageElement.setAttribute('data-field-message', 'true');

        // Insert after field or field container
        const container = field.closest('.col-md-12, .col-md-6, .col-md-4, .col-md-3, .col-md-8, .form-group') || field.parentElement;
        container.appendChild(messageElement);
    }

    function hideFieldMessage(field) {
        const container = field.closest('.col-md-12, .col-md-6, .col-md-4, .col-md-3, .col-md-8, .form-group') || field.parentElement;
        const existingMessage = container.querySelector('[data-field-message="true"]');
        if (existingMessage) {
            existingMessage.remove();
        }
    }

    function showValidationSummary(errors) {
        const form = document.getElementById('partForm');
        if (!form || !errors.length) return;

        const summaryHtml = `
            <div class="alert alert-danger validation-summary mb-3" role="alert">
                <div class="d-flex align-items-start">
                    <i class="fas fa-exclamation-triangle me-2 mt-1"></i>
                    <div>
                        <strong>Please fix the following errors before submitting:</strong>
                        <ul class="mb-0 mt-2">
                            ${errors.map(error => `<li>${error}</li>`).join('')}
                        </ul>
                    </div>
                </div>
            </div>
        `;

        // Insert at the top of the form
        form.insertAdjacentHTML('afterbegin', summaryHtml);

        // Scroll to validation summary
        const summary = form.querySelector('.validation-summary');
        if (summary) {
            summary.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }

    function clearValidationSummary() {
        const existingSummary = document.querySelector('.validation-summary');
        if (existingSummary) {
            existingSummary.remove();
        }
    }

    function focusErrorField(field) {
        if (!field) return;

        // If the field is in a tab, switch to that tab first
        const tabPane = field.closest('.tab-pane');
        if (tabPane) {
            const tabId = tabPane.id;
            const tabTrigger = document.querySelector(`[data-bs-target="#${tabId}"]`);
            if (tabTrigger) {
                tabTrigger.click();
            }
        }

        // Focus the field
        setTimeout(() => {
            field.focus();
            field.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }, 300);
    }

    function showSubmissionLoading() {
        const submitBtn = document.getElementById('savePartBtn');
        if (submitBtn) {
            submitBtn.disabled = true;
            const spinner = submitBtn.querySelector('.spinner-border');
            const btnText = submitBtn.querySelector('.btn-text');
            
            if (spinner) spinner.classList.remove('d-none');
            if (btnText) btnText.textContent = 'Saving...';
        }
    }

    // Enhanced global form response handler
    window.handleFormResponse = function(event) {
        console.log('[PARTS] Form response received:', event.detail);
        
        const xhr = event.detail.xhr;
        
        if (xhr.status === 200) {
            // Check if this is a redirect response
            const redirectHeader = xhr.getResponseHeader('HX-Redirect');
            if (redirectHeader) {
                console.log('[PARTS] Form saved successfully, redirecting...');
                
                // Show success message
                if (window.showToast) {
                    window.showToast('success', 'Part saved successfully!');
                }
                
                // Close modal
                const modal = document.getElementById('partModal');
                if (modal && typeof bootstrap !== 'undefined') {
                    const bsModal = bootstrap.Modal.getInstance(modal);
                    if (bsModal) {
                        bsModal.hide();
                    }
                }
                
                // Use href redirect instead of reload to preserve styling
                setTimeout(() => {
                    window.location.href = '/Admin/Parts';
                }, 1000);
                
                return;
            }
        } else {
            // Handle server validation errors
            console.log('[PARTS] Server validation errors detected');
            
            // Check if response contains validation errors
            if (xhr.responseText) {
                const responseText = xhr.responseText;
                if (responseText.includes('validation-summary') || responseText.includes('text-danger')) {
                    // Server returned validation errors - form will update automatically
                    console.log('[PARTS] Server validation errors will be displayed');
                }
            }
        }
        
        // Re-enable submit button
        resetSubmitButton();
    };
    
    function resetSubmitButton() {
        const submitBtn = document.getElementById('savePartBtn');
        if (submitBtn) {
            submitBtn.disabled = false;
            const spinner = submitBtn.querySelector('.spinner-border');
            const btnText = submitBtn.querySelector('.btn-text');
            
            if (spinner) spinner.classList.add('d-none');
            if (btnText) {
                const isEdit = document.querySelector('input[name="Part.Id"]')?.value !== '0';
                btnText.textContent = isEdit ? 'Update Part' : 'Create Part';
            }
        }
    }

    // Utility functions
    function getFieldDisplayName(field) {
        const label = document.querySelector(`label[for="${field.id}"]`) || 
                     field.closest('.col-md-12, .col-md-6, .col-md-4, .col-md-3, .col-md-8')?.querySelector('label');
        
        if (label) {
            return label.textContent.replace('*', '').trim();
        }
        
        return field.name?.replace('Part.', '') || 'Field';
    }

    // Global utility functions that are called from the form
    window.updateSlsMaterial = function() {
        console.log('[PARTS] updateSlsMaterial called');
        try {
            const materialSelect = document.getElementById('materialSelect');
            const slsMaterialInput = document.getElementById('slsMaterialInput');
            
            if (materialSelect && slsMaterialInput) {
                slsMaterialInput.value = materialSelect.value;
                console.log('[PARTS] SLS material updated to:', materialSelect.value);
            }
        } catch (error) {
            console.error('[PARTS] Error updating SLS material:', error);
        }
    };

    window.calculateVolume = function() {
        console.log('[PARTS] calculateVolume called');
        try {
            const lengthInput = document.querySelector('input[name="Part.LengthMm"]');
            const widthInput = document.querySelector('input[name="Part.WidthMm"]');
            const heightInput = document.querySelector('input[name="Part.HeightMm"]');
            const volumeInput = document.getElementById('volumeInput');
            const dimensionsInput = document.getElementById('dimensionsInput');
            
            if (lengthInput && widthInput && heightInput && volumeInput && dimensionsInput) {
                const length = parseFloat(lengthInput.value) || 0;
                const width = parseFloat(widthInput.value) || 0;
                const height = parseFloat(heightInput.value) || 0;
                
                const volume = length * width * height;
                volumeInput.value = volume.toFixed(0);
                dimensionsInput.value = `${length} × ${width} × ${height} mm`;
                
                console.log('[PARTS] Volume calculated:', volume);
            }
        } catch (error) {
            console.error('[PARTS] Error calculating volume:', error);
        }
    };

    function updateStageSummary() {
        try {
            // Count selected stages
            const stageCheckboxes = document.querySelectorAll('input[type="checkbox"][name^="Part.Requires"]:checked');
            const stageCount = stageCheckboxes.length;
            
            // Get duration
            const estimatedHoursInput = document.querySelector('input[name="Part.EstimatedHours"]');
            const estimatedHours = parseFloat(estimatedHoursInput?.value) || 0;
            
            // Get costs
            const materialCostInput = document.querySelector('input[name="Part.MaterialCostPerKg"]');
            const laborCostInput = document.querySelector('input[name="Part.StandardLaborCostPerHour"]');
            
            const materialCost = parseFloat(materialCostInput?.value) || 0;
            const laborCost = parseFloat(laborCostInput?.value) || 0;
            
            const totalCost = (materialCost * 0.5) + (laborCost * estimatedHours); // Rough estimate
            
            // Determine complexity
            let complexity = 'Simple';
            if (estimatedHours > 24) complexity = 'Very Complex';
            else if (estimatedHours > 12) complexity = 'Complex';
            else if (estimatedHours > 4) complexity = 'Medium';
            
            // Update summary cards
            const summaryStages = document.getElementById('summary-total-stages');
            const summaryDuration = document.getElementById('summary-total-duration');
            const summaryCost = document.getElementById('summary-total-cost');
            const summaryComplexity = document.getElementById('summary-complexity');
            
            if (summaryStages) summaryStages.textContent = stageCount;
            if (summaryDuration) summaryDuration.textContent = `${estimatedHours.toFixed(1)}h`;
            if (summaryCost) summaryCost.textContent = `$${totalCost.toFixed(2)}`;
            if (summaryComplexity) summaryComplexity.textContent = complexity;
            
            console.log('[PARTS] Summary updated:', { stageCount, estimatedHours, totalCost, complexity });
        } catch (error) {
            console.error('[PARTS] Error updating stage summary:', error);
        }
    }
    
    function setupStageCalculations() {
        // Listen for stage changes to update summary
        const stageCheckboxes = document.querySelectorAll('input[type="checkbox"][name^="Part.Requires"]');
        const estimatedHoursInput = document.querySelector('input[name="Part.EstimatedHours"]');
        const materialCostInput = document.querySelector('input[name="Part.MaterialCostPerKg"]');
        const laborCostInput = document.querySelector('input[name="Part.StandardLaborCostPerHour"]');
        
        // Add event listeners for all inputs that affect calculations
        [stageCheckboxes, [estimatedHoursInput], [materialCostInput], [laborCostInput]].flat().forEach(element => {
            if (element) {
                element.addEventListener('change', updateStageSummary);
                element.addEventListener('input', debounce(updateStageSummary, 300));
            }
        });
        
        // Initial calculation
        setTimeout(updateStageSummary, 100);
    }

    // Utility function for debouncing
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
})();