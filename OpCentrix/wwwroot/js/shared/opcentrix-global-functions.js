/**
 * OpCentrix Global Functions Library
 * Provides global utility functions used across the application
 * Enhanced with stage management functions and error handling
 */

(function() {
    'use strict';
    
    console.log('?? [GLOBAL] Loading OpCentrix global functions...');

    // Volume calculation function
    window.calculateVolume = function() {
        console.log('?? [GLOBAL] calculateVolume called');
        
        try {
            const lengthInput = document.querySelector('[name="Part.LengthMm"]');
            const widthInput = document.querySelector('[name="Part.WidthMm"]');
            const heightInput = document.querySelector('[name="Part.HeightMm"]');
            const volumeInput = document.getElementById('volumeInput');
            const dimensionsInput = document.getElementById('dimensionsInput');

            if (!lengthInput || !widthInput || !heightInput) {
                console.warn('?? [GLOBAL] Dimension inputs not found');
                return;
            }

            const length = parseFloat(lengthInput.value) || 0;
            const width = parseFloat(widthInput.value) || 0;
            const height = parseFloat(heightInput.value) || 0;

            console.log(`? [GLOBAL] Length found: ${length} from [name="Part.LengthMm"]`);
            console.log(`? [GLOBAL] Width found: ${width} from [name="Part.WidthMm"]`);
            console.log(`? [GLOBAL] Height found: ${height} from [name="Part.HeightMm"]`);

            const volume = length * width * height;
            console.log(`?? [GLOBAL] Calculated volume: ${volume} mm³`);

            if (volumeInput) {
                volumeInput.value = Math.round(volume);
                console.log(`? [GLOBAL] Volume updated: ${Math.round(volume)} in #volumeInput`);
            }

            if (dimensionsInput && (length > 0 || width > 0 || height > 0)) {
                const dimensionsText = `${length} × ${width} × ${height} mm`;
                dimensionsInput.value = dimensionsText;
                console.log(`? [GLOBAL] Dimensions updated: ${dimensionsText}`);
            }

        } catch (error) {
            console.error('? [GLOBAL] Error calculating volume:', error);
        }
    };

    // Material update function
    window.updateSlsMaterial = function() {
        console.log('?? [GLOBAL] updateSlsMaterial called');
        
        try {
            const materialSelect = document.getElementById('materialSelect');
            const slsMaterialInput = document.getElementById('slsMaterialInput');

            if (!materialSelect || !slsMaterialInput) {
                console.warn('?? [GLOBAL] Material inputs not found');
                return;
            }

            const selectedMaterial = materialSelect.value;
            if (selectedMaterial) {
                slsMaterialInput.value = selectedMaterial;
                console.log(`? [GLOBAL] SLS material updated to: ${selectedMaterial}`);
            }

        } catch (error) {
            console.error('? [GLOBAL] Error updating SLS material:', error);
        }
    };

    // Stage management functions - Global wrappers
    window.addStageGlobal = function(stageId) {
        console.log(`?? [GLOBAL] addStageGlobal called with stageId: ${stageId}`);
        
        try {
            if (window.stageManager && typeof window.stageManager.addStage === 'function') {
                return window.stageManager.addStage(stageId);
            } else if (window.ModernStageManager) {
                // Initialize stage manager if not already done
                if (!window.stageManager) {
                    console.log('?? [GLOBAL] Initializing stage manager...');
                    const partIdInput = document.querySelector('input[name="Part.Id"]');
                    const partId = partIdInput ? parseInt(partIdInput.value) || null : null;
                    window.stageManager = new window.ModernStageManager(partId);
                }
                return window.stageManager.addStage(stageId);
            } else {
                console.warn('?? [GLOBAL] Stage manager not available, using fallback');
                return showStageManagerFallback('add', stageId);
            }
        } catch (error) {
            console.error('? [GLOBAL] Error in addStageGlobal:', error);
            return showStageManagerFallback('add', stageId);
        }
    };

    window.removeStageGlobal = function(stageId) {
        console.log(`?? [GLOBAL] removeStageGlobal called with stageId: ${stageId}`);
        
        try {
            if (window.stageManager && typeof window.stageManager.removeStage === 'function') {
                return window.stageManager.removeStage(stageId);
            } else {
                console.warn('?? [GLOBAL] Stage manager not available for remove operation');
                return showStageManagerFallback('remove', stageId);
            }
        } catch (error) {
            console.error('? [GLOBAL] Error in removeStageGlobal:', error);
            return showStageManagerFallback('remove', stageId);
        }
    };

    window.editStageGlobal = function(stageId) {
        console.log(`?? [GLOBAL] editStageGlobal called with stageId: ${stageId}`);
        
        try {
            if (window.stageManager && typeof window.stageManager.editStage === 'function') {
                return window.stageManager.editStage(stageId);
            } else {
                console.warn('?? [GLOBAL] Stage manager not available for edit operation');
                return showStageManagerFallback('edit', stageId);
            }
        } catch (error) {
            console.error('? [GLOBAL] Error in editStageGlobal:', error);
            return showStageManagerFallback('edit', stageId);
        }
    };

    // Fallback function for when stage manager is not available
    function showStageManagerFallback(action, stageId) {
        const stageNames = {
            1: 'SLS Printing',
            2: 'EDM Operations', 
            3: 'CNC Machining',
            4: 'Assembly',
            5: 'Finishing'
        };
        
        const stageName = stageNames[stageId] || `Stage ${stageId}`;
        
        if (action === 'add') {
            if (confirm(`Add ${stageName} to this part?`)) {
                // Add to a simple fallback storage
                if (!window.fallbackSelectedStages) {
                    window.fallbackSelectedStages = new Set();
                }
                window.fallbackSelectedStages.add(stageId);
                showToastMessage('success', `${stageName} added (fallback mode)`);
                updateFallbackStageDisplay();
                return true;
            }
        } else if (action === 'remove') {
            if (confirm(`Remove ${stageName} from this part?`)) {
                if (window.fallbackSelectedStages) {
                    window.fallbackSelectedStages.delete(stageId);
                }
                showToastMessage('success', `${stageName} removed (fallback mode)`);
                updateFallbackStageDisplay();
                return true;
            }
        } else if (action === 'edit') {
            showToastMessage('info', `Edit ${stageName} - stage manager loading...`);
        }
        return false;
    }

    // Update fallback stage display
    function updateFallbackStageDisplay() {
        try {
            const container = document.getElementById('stage-requirements-container');
            if (container && window.fallbackSelectedStages && window.fallbackSelectedStages.size > 0) {
                const stageNames = {
                    1: 'SLS Printing',
                    2: 'EDM Operations', 
                    3: 'CNC Machining',
                    4: 'Assembly',
                    5: 'Finishing'
                };
                
                const stageList = Array.from(window.fallbackSelectedStages).map(stageId => {
                    const stageName = stageNames[stageId] || `Stage ${stageId}`;
                    return `
                        <div class="alert alert-info d-flex justify-content-between align-items-center" role="alert">
                            <span><i class="fas fa-cog me-2"></i>${stageName}</span>
                            <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeStageGlobal(${stageId})">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    `;
                }).join('');
                
                container.innerHTML = `
                    <div class="fallback-stage-display">
                        <div class="alert alert-warning" role="alert">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            <strong>Fallback Mode</strong> - Stage manager is loading. Selected stages:
                        </div>
                        ${stageList}
                        <div class="text-muted small mt-2">
                            <i class="fas fa-info-circle me-1"></i>
                            Stages will be saved when you submit the form.
                        </div>
                    </div>
                `;
            }
        } catch (error) {
            console.error('? [GLOBAL] Error updating fallback stage display:', error);
        }
    }

    // Toast message function
    window.showToastMessage = function(type, message) {
        console.log(`?? [GLOBAL] Toast: ${type} - ${message}`);
        
        try {
            // Try to use the existing toast system
            if (window.showToast && typeof window.showToast === 'function') {
                window.showToast(type, message);
                return;
            }
            
            // Fallback to simple alert
            alert(`${type.toUpperCase()}: ${message}`);
            
        } catch (error) {
            console.error('? [GLOBAL] Error showing toast:', error);
            alert(message);
        }
    };

    // Debug function
    window.debugStageManager = function() {
        console.log('?? [GLOBAL] Stage Manager Debug Information:');
        console.log('- stageManager available:', !!window.stageManager);
        console.log('- ModernStageManager available:', !!window.ModernStageManager);
        console.log('- addStageGlobal function:', typeof window.addStageGlobal);
        console.log('- Stage manager instance:', window.stageManager);
        
        if (window.stageManager) {
            console.log('- Stage manager initialized:', !!window.stageManager.initialized);
            console.log('- Available stages:', window.stageManager.availableStages?.length || 0);
            console.log('- Selected stages:', window.stageManager.selectedStages?.size || 0);
        }
        
        return {
            stageManager: !!window.stageManager,
            ModernStageManager: !!window.ModernStageManager,
            initialized: window.stageManager?.initialized || false
        };
    };

    // Initialize function to be called when DOM is ready
    window.initializeGlobalFunctions = function() {
        console.log('?? [GLOBAL] Initializing global functions');
        
        try {
            // Bind volume calculation to dimension inputs
            const dimensionInputs = document.querySelectorAll('[name="Part.LengthMm"], [name="Part.WidthMm"], [name="Part.HeightMm"]');
            dimensionInputs.forEach(input => {
                input.addEventListener('input', window.calculateVolume);
                input.addEventListener('change', window.calculateVolume);
            });
            
            // Bind material update to material select
            const materialSelect = document.getElementById('materialSelect');
            if (materialSelect) {
                materialSelect.addEventListener('change', window.updateSlsMaterial);
            }
            
            console.log('? [GLOBAL] Global functions initialized');
            
        } catch (error) {
            console.error('? [GLOBAL] Error initializing global functions:', error);
        }
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', window.initializeGlobalFunctions);
    } else {
        window.initializeGlobalFunctions();
    }

    console.log('? [GLOBAL] OpCentrix global functions loaded');

})();