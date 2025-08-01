/**
 * OpCentrix Global Functions
 * Essential functions that must be available globally for inline event handlers
 * These functions work across all forms and pages
 */

(function() {
    'use strict';
    
    console.log('?? [OPCENTRIX-GLOBAL] Loading global functions...');
    
    // ===================================================================
    // VOLUME CALCULATION - GLOBALLY ACCESSIBLE
    // ===================================================================
    
    window.calculateVolume = function() {
        console.log('?? [GLOBAL] calculateVolume called');
        
        try {
            // Try multiple field name patterns to find the dimension inputs
            const lengthSelectors = [
                '[name="LengthMm"]',
                '[name="Part.LengthMm"]',
                '#LengthMm',
                'input[placeholder*="Length"]'
            ];
            
            const widthSelectors = [
                '[name="WidthMm"]', 
                '[name="Part.WidthMm"]',
                '#WidthMm',
                'input[placeholder*="Width"]'
            ];
            
            const heightSelectors = [
                '[name="HeightMm"]',
                '[name="Part.HeightMm"]', 
                '#HeightMm',
                'input[placeholder*="Height"]'
            ];
            
            let length = 0, width = 0, height = 0;
            let lengthInput = null, widthInput = null, heightInput = null;
            
            // Find length input
            for (const selector of lengthSelectors) {
                lengthInput = document.querySelector(selector);
                if (lengthInput) {
                    length = parseFloat(lengthInput.value || 0);
                    console.log('? [GLOBAL] Length found:', length, 'from', selector);
                    break;
                }
            }
            
            // Find width input
            for (const selector of widthSelectors) {
                widthInput = document.querySelector(selector);
                if (widthInput) {
                    width = parseFloat(widthInput.value || 0);
                    console.log('? [GLOBAL] Width found:', width, 'from', selector);
                    break;
                }
            }
            
            // Find height input
            for (const selector of heightSelectors) {
                heightInput = document.querySelector(selector);
                if (heightInput) {
                    height = parseFloat(heightInput.value || 0);
                    console.log('? [GLOBAL] Height found:', height, 'from', selector);
                    break;
                }
            }
            
            // Calculate volume
            const volume = length * width * height;
            console.log('?? [GLOBAL] Calculated volume:', volume, 'mm³');
            
            // Find and update volume input
            const volumeSelectors = [
                '#volumeInput',
                '[name="VolumeMm3"]',
                '[name="Part.VolumeMm3"]',
                'input[readonly][step="1"]'
            ];
            
            let volumeInput = null;
            for (const selector of volumeSelectors) {
                volumeInput = document.querySelector(selector);
                if (volumeInput) {
                    volumeInput.value = Math.round(volume);
                    console.log('? [GLOBAL] Volume updated:', Math.round(volume), 'in', selector);
                    break;
                }
            }
            
            if (!volumeInput) {
                console.warn('?? [GLOBAL] Volume input not found');
            }
            
            // Find and update dimensions display
            const dimensionsSelectors = [
                '#dimensionsInput',
                '[name="Dimensions"]',
                '[name="Part.Dimensions"]',
                'input[placeholder*="×"]'
            ];
            
            let dimensionsInput = null;
            for (const selector of dimensionsSelectors) {
                dimensionsInput = document.querySelector(selector);
                if (dimensionsInput) {
                    if (length > 0 && width > 0 && height > 0) {
                        dimensionsInput.value = `${length} × ${width} × ${height} mm`;
                        console.log('? [GLOBAL] Dimensions updated:', dimensionsInput.value);
                    }
                    break;
                }
            }
            
            if (!dimensionsInput) {
                console.warn('?? [GLOBAL] Dimensions input not found');
            }
            
            // Trigger any other volume-related calculations
            if (window.calculateDensity) {
                window.calculateDensity();
            }
            
            return volume;
            
        } catch (error) {
            console.error('? [GLOBAL] Error calculating volume:', error);
            return 0;
        }
    };
    
    // ===================================================================
    // DURATION CALCULATIONS - GLOBALLY ACCESSIBLE  
    // ===================================================================
    
    window.updateDurationDisplay = function() {
        console.log('?? [GLOBAL] updateDurationDisplay called');
        
        try {
            const estimatedHoursSelectors = [
                '#estimatedHoursInput',
                '[name="EstimatedHours"]',
                '[name="Part.EstimatedHours"]'
            ];
            
            const adminOverrideSelectors = [
                '#adminOverrideInput',
                '[name="AdminEstimatedHoursOverride"]', 
                '[name="Part.AdminEstimatedHoursOverride"]'
            ];
            
            let estimatedHours = 0;
            let adminOverride = 0;
            
            // Find estimated hours
            for (const selector of estimatedHoursSelectors) {
                const element = document.querySelector(selector);
                if (element) {
                    estimatedHours = parseFloat(element.value || 0);
                    console.log('? [GLOBAL] Estimated hours found:', estimatedHours);
                    break;
                }
            }
            
            // Find admin override
            for (const selector of adminOverrideSelectors) {
                const element = document.querySelector(selector);
                if (element) {
                    adminOverride = parseFloat(element.value || 0);
                    console.log('? [GLOBAL] Admin override found:', adminOverride);
                    break;
                }
            }
            
            // Calculate effective duration
            const effectiveHours = adminOverride > 0 ? adminOverride : estimatedHours;
            const displayText = adminOverride > 0 
                ? `${effectiveHours.toFixed(1)}h (Admin Override)` 
                : `${effectiveHours.toFixed(1)}h (Standard)`;
            
            // Update display
            const displaySelectors = [
                '#effectiveDurationDisplay',
                '.duration-display',
                '[data-duration-display]'
            ];
            
            for (const selector of displaySelectors) {
                const element = document.querySelector(selector);
                if (element) {
                    element.textContent = displayText;
                    element.className = adminOverride > 0 
                        ? 'form-control-plaintext bg-warning bg-opacity-25 border rounded p-2 fw-bold'
                        : 'form-control-plaintext bg-light border rounded p-2';
                    console.log('? [GLOBAL] Duration display updated:', displayText);
                    break;
                }
            }
            
        } catch (error) {
            console.error('? [GLOBAL] Error updating duration display:', error);
        }
    };
    
    // ===================================================================
    // MATERIAL MANAGEMENT - GLOBALLY ACCESSIBLE
    // ===================================================================
    
    window.updateSlsMaterial = function() {
        console.log('?? [GLOBAL] updateSlsMaterial called');
        
        try {
            const materialSelect = document.querySelector('#materialSelect, [name="Material"], [name="Part.Material"]');
            const slsMaterialInput = document.querySelector('#slsMaterialInput, [name="SlsMaterial"], [name="Part.SlsMaterial"]');
            
            if (!materialSelect || !slsMaterialInput) {
                console.warn('?? [GLOBAL] Material elements not found');
                console.log('Material select:', !!materialSelect);
                console.log('SLS material input:', !!slsMaterialInput);
                return false;
            }
            
            const selectedMaterial = materialSelect.value;
            if (!selectedMaterial) {
                console.log('?? [GLOBAL] No material selected');
                return false;
            }
            
            // Update SLS material to match selected material
            slsMaterialInput.value = selectedMaterial;
            console.log('? [GLOBAL] SLS material updated to:', selectedMaterial);
            
            // Trigger comprehensive material defaults update
            if (window.updateMaterialDefaults) {
                window.updateMaterialDefaults(selectedMaterial);
            } else {
                // Fallback: Basic SLS parameter auto-fill using predefined defaults
                updateBasicSlsParameters(selectedMaterial);
            }
            
            return true;
            
        } catch (error) {
            console.error('? [GLOBAL] Error updating SLS material:', error);
            return false;
        }
    };
    
    // Enhanced Material Defaults for SLS Parameters
    const MATERIAL_SLS_DEFAULTS = {
        'Ti-6Al-4V Grade 5': {
            laserPower: 200,
            scanSpeed: 1200,
            layerThickness: 30,
            hatchSpacing: 120,
            buildTemperature: 180,
            argonPurity: 99.9,
            oxygenContent: 50,
            materialCost: 450.00,
            laborCost: 85.00,
            machineOperatingCost: 125.00,
            estimatedHours: 8.0,
            argonCost: 15.00
        },
        'Ti-6Al-4V ELI Grade 23': {
            laserPower: 180,
            scanSpeed: 1450,
            layerThickness: 30,
            hatchSpacing: 120,
            buildTemperature: 165,
            argonPurity: 99.95,
            oxygenContent: 30,
            materialCost: 550.00,
            laborCost: 85.00,
            machineOperatingCost: 125.00,
            estimatedHours: 8.5,
            argonCost: 15.00
        },
        'CP Titanium Grade 2': {
            laserPower: 170,
            scanSpeed: 1300,
            layerThickness: 30,
            hatchSpacing: 125,
            buildTemperature: 175,
            argonPurity: 99.9,
            oxygenContent: 40,
            materialCost: 400.00,
            laborCost: 80.00,
            machineOperatingCost: 120.00,
            estimatedHours: 7.5,
            argonCost: 14.00
        },
        'Inconel 718': {
            laserPower: 285,
            scanSpeed: 960,
            layerThickness: 40,
            hatchSpacing: 110,
            buildTemperature: 200,
            argonPurity: 99.9,
            oxygenContent: 50,
            materialCost: 750.00,
            laborCost: 95.00,
            machineOperatingCost: 135.00,
            estimatedHours: 12.0,
            argonCost: 18.00
        },
        'Inconel 625': {
            laserPower: 275,
            scanSpeed: 980,
            layerThickness: 40,
            hatchSpacing: 110,
            buildTemperature: 195,
            argonPurity: 99.9,
            oxygenContent: 30,
            materialCost: 800.00,
            laborCost: 95.00,
            machineOperatingCost: 135.00,
            estimatedHours: 12.5,
            argonCost: 18.00
        },
        '316L Stainless Steel': {
            laserPower: 200,
            scanSpeed: 1150,
            layerThickness: 30,
            hatchSpacing: 120,
            buildTemperature: 170,
            argonPurity: 99.5,
            oxygenContent: 100,
            materialCost: 150.00,
            laborCost: 75.00,
            machineOperatingCost: 115.00,
            estimatedHours: 6.0,
            argonCost: 12.00
        },
        '17-4 PH Stainless Steel': {
            laserPower: 195,
            scanSpeed: 1250,
            layerThickness: 30,
            hatchSpacing: 120,
            buildTemperature: 165,
            argonPurity: 99.5,
            oxygenContent: 100,
            materialCost: 180.00,
            laborCost: 80.00,
            machineOperatingCost: 120.00,
            estimatedHours: 7.0,
            argonCost: 12.00
        },
        'AlSi10Mg': {
            laserPower: 175,
            scanSpeed: 1350,
            layerThickness: 30,
            hatchSpacing: 130,
            buildTemperature: 160,
            argonPurity: 99.5,
            oxygenContent: 100,
            materialCost: 80.00,
            laborCost: 70.00,
            machineOperatingCost: 100.00,
            estimatedHours: 5.0,
            argonCost: 10.00
        }
    };
    
    // Basic SLS parameters auto-fill function
    function updateBasicSlsParameters(materialName) {
        console.log('?? [GLOBAL] Updating SLS parameters for material:', materialName);
        
        const defaults = MATERIAL_SLS_DEFAULTS[materialName];
        if (!defaults) {
            console.warn('?? [GLOBAL] No SLS defaults found for material:', materialName);
            return;
        }
        
        // Update SLS process parameters
        const fieldMappings = [
            { selectors: ['#laserPowerInput', '[name*="LaserPower"]'], value: defaults.laserPower, name: 'Laser Power' },
            { selectors: ['#scanSpeedInput', '[name*="ScanSpeed"]'], value: defaults.scanSpeed, name: 'Scan Speed' },
            { selectors: ['#layerThicknessInput', '[name*="LayerThickness"]'], value: defaults.layerThickness, name: 'Layer Thickness' },
            { selectors: ['#hatchSpacingInput', '[name*="HatchSpacing"]'], value: defaults.hatchSpacing, name: 'Hatch Spacing' },
            { selectors: ['#buildTempInput', '[name*="BuildTemperature"]'], value: defaults.buildTemperature, name: 'Build Temperature' },
            { selectors: ['#argonPurityInput', '[name*="ArgonPurity"]'], value: defaults.argonPurity, name: 'Argon Purity' },
            { selectors: ['#oxygenContentInput', '[name*="OxygenContent"]'], value: defaults.oxygenContent, name: 'Oxygen Content' },
            { selectors: ['#materialCostInput', '[name*="MaterialCost"]'], value: defaults.materialCost, name: 'Material Cost' },
            { selectors: ['#laborCostInput', '[name*="LaborCost"]'], value: defaults.laborCost, name: 'Labor Cost' },
            { selectors: ['#machineCostInput', '[name*="MachineCost"]'], value: defaults.machineOperatingCost, name: 'Machine Cost' },
            { selectors: ['#estimatedHoursInput', '[name*="EstimatedHours"]'], value: defaults.estimatedHours, name: 'Estimated Hours' }
        ];
        
        let updatedCount = 0;
        
        fieldMappings.forEach(mapping => {
            let element = null;
            
            // Try each selector until we find the element
            for (const selector of mapping.selectors) {
                element = document.querySelector(selector);
                if (element) break;
            }
            
            if (element && (!element.value || element.value == '0' || element.value == '')) {
                element.value = mapping.value;
                updatedCount++;
                console.log(`? [GLOBAL] Updated ${mapping.name} to: ${mapping.value}`);
            }
        });
        
        console.log(`? [GLOBAL] Updated ${updatedCount} SLS parameters for ${materialName}`);
        
        // Trigger dependent calculations
        if (window.updateDurationDisplay) {
            window.updateDurationDisplay();
        }
        
        if (window.calculateTotalCost) {
            window.calculateTotalCost();
        }
    }
    
    // Global function to update material defaults (enhanced version)
    window.updateMaterialDefaults = function(selectedMaterial) {
        console.log('?? [GLOBAL] updateMaterialDefaults called for:', selectedMaterial);
        updateBasicSlsParameters(selectedMaterial);
    };
    
    // ===================================================================
    // PART NUMBER VALIDATION - GLOBALLY ACCESSIBLE
    // ===================================================================
    
    window.validatePartNumber = function(partNumber) {
        console.log('?? [GLOBAL] validatePartNumber called:', partNumber);
        
        try {
            const input = document.querySelector('[name="PartNumber"], [name="Part.PartNumber"]');
            if (!input) {
                console.warn('?? [GLOBAL] Part number input not found');
                return false;
            }
            
            if (!partNumber || partNumber.trim() === '') {
                // Clear validation state for empty input
                input.classList.remove('is-valid', 'is-invalid');
                return false;
            }
            
            // Basic validation pattern (flexible)
            const partNumberPattern = /^[A-Za-z0-9][A-Za-z0-9\-_]{2,49}$/;
            const isValidFormat = partNumberPattern.test(partNumber);
            
            if (isValidFormat) {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
                console.log('? [GLOBAL] Part number format valid:', partNumber);
                return true;
            } else {
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');
                console.log('? [GLOBAL] Invalid part number format:', partNumber);
                return false;
            }
            
        } catch (error) {
            console.error('? [GLOBAL] Error validating part number:', error);
            return false;
        }
    };
    
    // ===================================================================
    // TAB MANAGEMENT - GLOBALLY ACCESSIBLE
    // ===================================================================
    
    window.showTab = function(tabName) {
        console.log('?? [GLOBAL] showTab called:', tabName);
        
        try {
            // Hide all tab content
            const allTabs = document.querySelectorAll('.tab-pane');
            allTabs.forEach(tab => {
                tab.classList.remove('show', 'active');
            });
            
            // Remove active from all tab buttons
            const allButtons = document.querySelectorAll('.nav-link');
            allButtons.forEach(button => {
                button.classList.remove('active');
            });
            
            // Show selected tab
            const targetTab = document.getElementById(tabName);
            const targetButton = document.getElementById(tabName + '-tab');
            
            if (targetTab && targetButton) {
                targetTab.classList.add('show', 'active');
                targetButton.classList.add('active');
                console.log('? [GLOBAL] Tab switched successfully:', tabName);
                return true;
            } else {
                console.error('? [GLOBAL] Tab elements not found:', tabName);
                return false;
            }
            
        } catch (error) {
            console.error('? [GLOBAL] Error switching tabs:', error);
            return false;
        }
    };
    
    // ===================================================================
    // COST CALCULATIONS - GLOBALLY ACCESSIBLE
    // ===================================================================
    
    window.calculateTotalCost = function() {
        console.log('?? [GLOBAL] calculateTotalCost called');
        
        try {
            // Find cost inputs
            const materialCost = parseFloat(document.querySelector('[name*="MaterialCost"]')?.value || 0);
            const laborCost = parseFloat(document.querySelector('[name*="LaborCost"]')?.value || 0);
            const machineCost = parseFloat(document.querySelector('[name*="MachineCost"]')?.value || 0);
            const hours = parseFloat(document.querySelector('[name*="EstimatedHours"]')?.value || 0);
            
            // Calculate total
            const totalCost = materialCost + (laborCost * hours) + (machineCost * hours);
            
            // Update display
            const displayElement = document.querySelector('#estimatedTotalCostDisplay, .total-cost-display');
            if (displayElement) {
                displayElement.textContent = `$${totalCost.toFixed(2)}`;
                console.log('? [GLOBAL] Total cost updated:', totalCost.toFixed(2));
            }
            
            return totalCost;
            
        } catch (error) {
            console.error('? [GLOBAL] Error calculating total cost:', error);
            return 0;
        }
    };
    
    // ===================================================================
    // DEBUGGING HELPER
    // ===================================================================
    
    window.debugFormFunctions = function() {
        console.group('?? [GLOBAL] Form Functions Debug');
        console.log('calculateVolume:', typeof window.calculateVolume);
        console.log('updateDurationDisplay:', typeof window.updateDurationDisplay);
        console.log('updateSlsMaterial:', typeof window.updateSlsMaterial);
        console.log('validatePartNumber:', typeof window.validatePartNumber);
        console.log('showTab:', typeof window.showTab);
        console.log('calculateTotalCost:', typeof window.calculateTotalCost);
        
        // Test volume calculation with current inputs
        const length = parseFloat(document.querySelector('[name*="Length"]')?.value || 0);
        const width = parseFloat(document.querySelector('[name*="Width"]')?.value || 0);
        const height = parseFloat(document.querySelector('[name*="Height"]')?.value || 0);
        console.log('Current dimensions:', { length, width, height });
        
        if (length && width && height) {
            console.log('Testing calculateVolume...');
            const result = window.calculateVolume();
            console.log('Volume calculation result:', result);
        }
        
        console.groupEnd();
    };
    
    // ===================================================================
    // AUTO-INITIALIZATION
    // ===================================================================
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeGlobalFunctions);
    } else {
        initializeGlobalFunctions();
    }
    
    function initializeGlobalFunctions() {
        console.log('? [GLOBAL] Global functions initialized');
        
        // Auto-bind volume calculation to dimension inputs
        setTimeout(() => {
            bindVolumeCalculation();
        }, 500);
    }
    
    function bindVolumeCalculation() {
        const dimensionInputs = document.querySelectorAll(
            '[name*="Length"], [name*="Width"], [name*="Height"], ' +
            '[placeholder*="Length"], [placeholder*="Width"], [placeholder*="Height"]'
        );
        
        dimensionInputs.forEach(input => {
            // Remove existing listeners to prevent duplicates
            const newInput = input.cloneNode(true);
            input.parentNode.replaceChild(newInput, input);
            
            // Add our listener
            newInput.addEventListener('input', () => {
                setTimeout(() => window.calculateVolume(), 100);
            });
            
            newInput.addEventListener('change', () => {
                setTimeout(() => window.calculateVolume(), 100);
            });
        });
        
        if (dimensionInputs.length > 0) {
            console.log('? [GLOBAL] Volume calculation bound to', dimensionInputs.length, 'dimension inputs');
        }
    }
    
    console.log('? [OPCENTRIX-GLOBAL] Global functions loaded successfully');
    
})();