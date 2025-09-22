/**
 * Modern Stage Management Component for Part Form
 * Replaces legacy boolean checkboxes with dynamic stage selection
 * Enhanced with robust error handling and fallback mechanisms
 */
class ModernStageManager {
    constructor(partId = null) {
        console.log('?? [STAGE-MANAGER] Initializing ModernStageManager for partId:', partId);
        
        this.partId = partId;
        this.availableStages = [];
        this.selectedStages = [];
        this.stageContainer = null;
        this.isInitialized = false;
        this.debugMode = true; // Enable debug logging
        
        // Immediately try to initialize
        this.initialize().catch(error => {
            console.error('? [STAGE-MANAGER] Failed to initialize:', error);
            this.showError('Failed to initialize stage manager: ' + error.message);
        });
    }

    async initialize() {
        console.log('?? [STAGE-MANAGER] Starting initialization...');
        
        try {
            // Step 1: Find required DOM elements
            if (!this.findDOMElements()) {
                throw new Error('Required DOM elements not found');
            }
            
            // Step 2: Load available stages from API
            await this.loadAvailableStages();
            
            // Step 3: Load existing stages if editing a part
            if (this.partId && this.partId > 0) {
                await this.loadPartStages();
            }
            
            // Step 4: Render the interface
            this.renderStageInterface();
            
            // Step 5: Set up event handlers
            this.setupEventHandlers();
            
            this.isInitialized = true;
            console.log('? [STAGE-MANAGER] Initialization complete');
            
        } catch (error) {
            console.error('? [STAGE-MANAGER] Initialization failed:', error);
            this.showError('Stage manager initialization failed: ' + error.message);
            throw error;
        }
    }

    findDOMElements() {
        console.log('?? [STAGE-MANAGER] Finding DOM elements...');
        
        this.stageContainer = document.getElementById('stage-requirements-container');
        this.summaryContainer = document.getElementById('stage-summary');
        this.availableContainer = document.getElementById('availableStagesContainer');
        
        const found = {
            stageContainer: !!this.stageContainer,
            summaryContainer: !!this.summaryContainer,
            availableContainer: !!this.availableContainer
        };
        
        console.log('?? [STAGE-MANAGER] DOM elements found:', found);
        
        if (!this.stageContainer) {
            console.error('? [STAGE-MANAGER] Critical: stage-requirements-container not found');
            return false;
        }
        
        return true;
    }

    async loadAvailableStages() {
        console.log('?? [STAGE-MANAGER] Loading available stages from API...');
        
        try {
            // Show loading indicator
            this.showLoading('Loading manufacturing stages...');
            
            const response = await fetch('/api/production-stages/available', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            console.log('?? [STAGE-MANAGER] API Response status:', response.status);
            
            if (!response.ok) {
                const errorText = await response.text();
                console.error('? [STAGE-MANAGER] API Error:', response.status, errorText);
                throw new Error(`API returned ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log('? [STAGE-MANAGER] API returned', data.length, 'stages:', data);
            
            if (!Array.isArray(data)) {
                throw new Error('API returned invalid data format (not an array)');
            }
            
            this.availableStages = data;
            
            if (this.availableStages.length === 0) {
                console.warn('?? [STAGE-MANAGER] No stages returned from API');
                this.showError('No production stages are available. Please configure stages first.');
                return;
            }
            
            console.log('? [STAGE-MANAGER] Successfully loaded', this.availableStages.length, 'stages');
            
        } catch (error) {
            console.error('? [STAGE-MANAGER] Failed to load stages:', error);
            
            // Use fallback stages for development/testing
            console.log('?? [STAGE-MANAGER] Using fallback stages...');
            this.availableStages = this.getFallbackStages();
            
            this.showError('Failed to load stages from server. Using fallback data. Error: ' + error.message);
        }
    }

    getFallbackStages() {
        return [
            {
                id: 1,
                name: "3D Printing (SLS)",
                description: "Selective Laser Sintering",
                defaultHourlyRate: 85.00,
                defaultDurationHours: 8.0,
                defaultSetupMinutes: 30,
                defaultMaterialCost: 0.00,
                displayOrder: 1,
                department: "3D Printing",
                stageColor: "#007bff",
                stageIcon: "fas fa-cube"
            },
            {
                id: 2,
                name: "CNC Machining",
                description: "Computer Numerical Control machining",
                defaultHourlyRate: 85.00,
                defaultDurationHours: 4.0,
                defaultSetupMinutes: 45,
                defaultMaterialCost: 0.00,
                displayOrder: 2,
                department: "CNC Machining",
                stageColor: "#28a745",
                stageIcon: "fas fa-cogs"
            },
            {
                id: 3,
                name: "EDM Operations",
                description: "Electrical Discharge Machining",
                defaultHourlyRate: 95.00,
                defaultDurationHours: 6.0,
                defaultSetupMinutes: 60,
                defaultMaterialCost: 0.00,
                displayOrder: 3,
                department: "EDM",
                stageColor: "#ffc107",
                stageIcon: "fas fa-bolt"
            }
        ];
    }

    async loadPartStages() {
        console.log('?? [STAGE-MANAGER] Loading existing stages for part:', this.partId);
        
        try {
            const response = await fetch(`/Admin/Parts?handler=PartStages&partId=${this.partId}`, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const existingStages = await response.json();
                console.log('? [STAGE-MANAGER] Loaded', existingStages.length, 'existing stages');
                
                // Convert existing stages to selected stages format
                this.selectedStages = existingStages.map(stage => ({
                    stageId: stage.productionStageId,
                    name: stage.productionStage?.name || 'Unknown Stage',
                    executionOrder: stage.executionOrder,
                    estimatedHours: stage.estimatedHours,
                    hourlyRate: stage.hourlyRateOverride || stage.productionStage?.defaultHourlyRate || 85,
                    materialCost: stage.materialCost || 0,
                    isRequired: stage.isRequired
                }));
            } else {
                console.warn('?? [STAGE-MANAGER] Could not load existing stages:', response.status);
            }
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error loading part stages:', error);
        }
    }

    renderStageInterface() {
        console.log('?? [STAGE-MANAGER] Rendering stage interface...');
        
        try {
            // Hide loading indicator
            this.hideLoading();
            
            // Render selected stages
            this.renderSelectedStages();
            
            // Update summary
            this.updateSummary();
            
            // Update available stages buttons
            this.updateAvailableStagesButtons();
            
            console.log('? [STAGE-MANAGER] Interface rendered successfully');
            
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error rendering interface:', error);
            this.showError('Error rendering stage interface: ' + error.message);
        }
    }

    renderSelectedStages() {
        if (!this.stageContainer) return;
        
        if (this.selectedStages.length === 0) {
            this.stageContainer.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="fas fa-plus-circle fa-3x mb-3"></i>
                    <h5>No Manufacturing Stages Selected</h5>
                    <p>Add stages to define the manufacturing workflow for this part.</p>
                    <p class="small">Stages define the sequence of operations needed to manufacture this part.</p>
                </div>
            `;
            return;
        }

        const html = this.selectedStages
            .sort((a, b) => a.executionOrder - b.executionOrder)
            .map(stage => this.renderStageCard(stage))
            .join('');

        this.stageContainer.innerHTML = `
            <div class="selected-stages-list">
                ${html}
            </div>
        `;
    }

    renderStageCard(stage) {
        const stageInfo = this.availableStages.find(s => s.id === stage.stageId);
        const totalCost = (stage.estimatedHours * stage.hourlyRate) + stage.materialCost;
        
        return `
            <div class="card mb-3 stage-card" data-stage-id="${stage.stageId}">
                <div class="card-header d-flex justify-content-between align-items-center" 
                     style="background-color: ${stageInfo?.stageColor || '#007bff'}20; border-left: 4px solid ${stageInfo?.stageColor || '#007bff'};">
                    <div class="d-flex align-items-center">
                        <span class="badge me-2" style="background-color: ${stageInfo?.stageColor || '#007bff'};">${stage.executionOrder}</span>
                        <i class="${stageInfo?.stageIcon || 'fas fa-cog'} me-2" style="color: ${stageInfo?.stageColor || '#007bff'};"></i>
                        <strong>${stage.name}</strong>
                        ${stage.isRequired ? '<span class="badge bg-danger ms-2">Required</span>' : '<span class="badge bg-info ms-2">Optional</span>'}
                    </div>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary" onclick="window.stageManager.editStage(${stage.stageId})" title="Edit Stage">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger" onclick="window.stageManager.removeStage(${stage.stageId})" title="Remove Stage">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-3">
                            <label class="form-label small">Estimated Hours</label>
                            <input type="number" class="form-control form-control-sm" 
                                   value="${stage.estimatedHours}" step="0.1" min="0.1"
                                   onchange="window.stageManager.updateStageHours(${stage.stageId}, this.value)">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Hourly Rate ($)</label>
                            <input type="number" class="form-control form-control-sm" 
                                   value="${stage.hourlyRate}" step="0.01" min="0"
                                   onchange="window.stageManager.updateStageRate(${stage.stageId}, this.value)">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Material Cost ($)</label>
                            <input type="number" class="form-control form-control-sm" 
                                   value="${stage.materialCost}" step="0.01" min="0"
                                   onchange="window.stageManager.updateStageMaterialCost(${stage.stageId}, this.value)">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Total Cost</label>
                            <div class="form-control-plaintext form-control-sm fw-bold text-primary">
                                $${totalCost.toFixed(2)}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    updateAvailableStagesButtons() {
        if (!this.availableContainer) return;
        
        const selectedStageIds = this.selectedStages.map(s => s.stageId);
        
        const html = this.availableStages
            .filter(stage => !selectedStageIds.includes(stage.id))
            .slice(0, 5) // Show first 5 unselected stages
            .map(stage => `
                <button type="button" class="btn btn-outline-primary btn-sm mb-1" 
                        onclick="window.stageManager.addStage(${stage.id})" title="${stage.description}">
                    <i class="fas fa-plus me-1"></i>${stage.name}
                </button>
            `).join('');
        
        if (html) {
            this.availableContainer.innerHTML = html;
        } else {
            this.availableContainer.innerHTML = '<p class="text-muted small">All stages added</p>';
        }
    }

    updateSummary() {
        if (!this.summaryContainer) return;
        
        const totalStages = this.selectedStages.length;
        const totalHours = this.selectedStages.reduce((sum, s) => sum + s.estimatedHours, 0);
        const totalCost = this.selectedStages.reduce((sum, s) => sum + (s.estimatedHours * s.hourlyRate) + s.materialCost, 0);
        
        let complexity = 'Simple';
        if (totalStages > 5) complexity = 'Complex';
        else if (totalStages > 3) complexity = 'Moderate';
        
        document.getElementById('summary-total-stages').textContent = totalStages;
        document.getElementById('summary-total-duration').textContent = totalHours.toFixed(1) + 'h';
        document.getElementById('summary-total-cost').textContent = '$' + totalCost.toFixed(2);
        document.getElementById('summary-complexity').textContent = complexity;
    }

    addStage(stageId) {
        console.log('?? [STAGE-MANAGER] Adding stage:', stageId);
        
        const stageInfo = this.availableStages.find(s => s.id === stageId);
        if (!stageInfo) {
            console.error('? [STAGE-MANAGER] Stage not found:', stageId);
            return false;
        }
        
        const newStage = {
            stageId: stageId,
            name: stageInfo.name,
            executionOrder: this.selectedStages.length + 1,
            estimatedHours: stageInfo.defaultDurationHours || 1.0,
            hourlyRate: stageInfo.defaultHourlyRate || 85.00,
            materialCost: stageInfo.defaultMaterialCost || 0.00,
            isRequired: true
        };
        
        this.selectedStages.push(newStage);
        this.renderStageInterface();
        this.updateHiddenFields();
        
        console.log('? [STAGE-MANAGER] Stage added successfully');
        return true;
    }

    removeStage(stageId) {
        console.log('?? [STAGE-MANAGER] Removing stage:', stageId);
        
        this.selectedStages = this.selectedStages.filter(s => s.stageId !== stageId);
        
        // Reorder remaining stages
        this.selectedStages.forEach((stage, index) => {
            stage.executionOrder = index + 1;
        });
        
        this.renderStageInterface();
        this.updateHiddenFields();
        
        console.log('? [STAGE-MANAGER] Stage removed successfully');
    }

    updateStageHours(stageId, hours) {
        const stage = this.selectedStages.find(s => s.stageId === stageId);
        if (stage) {
            stage.estimatedHours = parseFloat(hours) || 1.0;
            this.updateSummary();
            this.updateHiddenFields();
        }
    }

    updateStageRate(stageId, rate) {
        const stage = this.selectedStages.find(s => s.stageId === stageId);
        if (stage) {
            stage.hourlyRate = parseFloat(rate) || 85.00;
            this.updateSummary();
            this.updateHiddenFields();
        }
    }

    updateStageMaterialCost(stageId, cost) {
        const stage = this.selectedStages.find(s => s.stageId === stageId);
        if (stage) {
            stage.materialCost = parseFloat(cost) || 0.00;
            this.updateSummary();
            this.updateHiddenFields();
        }
    }

    updateHiddenFields() {
        console.log('?? [STAGE-MANAGER] Updating hidden form fields...');
        
        try {
            const stageIds = this.selectedStages.map(s => s.stageId).join(',');
            const orders = this.selectedStages.map(s => s.executionOrder).join(',');
            const hours = this.selectedStages.map(s => s.estimatedHours).join(',');
            const rates = this.selectedStages.map(s => s.hourlyRate).join(',');
            const costs = this.selectedStages.map(s => s.materialCost).join(',');
            
            this.setHiddenField('selectedStageIds', stageIds);
            this.setHiddenField('stageExecutionOrders', orders);
            this.setHiddenField('stageEstimatedHours', hours);
            this.setHiddenField('stageHourlyRates', rates);
            this.setHiddenField('stageMaterialCosts', costs);
            
            console.log('? [STAGE-MANAGER] Hidden fields updated:', {
                stageIds, orders, hours, rates, costs
            });
            
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error updating hidden fields:', error);
        }
    }

    setHiddenField(fieldId, value) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.value = value;
        } else {
            console.warn('?? [STAGE-MANAGER] Hidden field not found:', fieldId);
        }
    }

    setupEventHandlers() {
        console.log('?? [STAGE-MANAGER] Setting up event handlers...');
        
        // Add Stage button in the header
        const addStageBtn = document.getElementById('add-stage-btn');
        if (addStageBtn) {
            addStageBtn.onclick = () => this.showAddStageModal();
        }
    }

    showAddStageModal() {
        console.log('?? [STAGE-MANAGER] Showing add stage modal...');
        // This would show a modal to select from available stages
        // For now, just show an alert with available stages
        const availableNames = this.availableStages
            .filter(stage => !this.selectedStages.some(s => s.stageId === stage.id))
            .map(s => s.name)
            .join(', ');
        
        if (availableNames) {
            alert(`Available stages: ${availableNames}\n\nClick the stage buttons on the right to add them.`);
        } else {
            alert('All available stages have been added to this part.');
        }
    }

    showLoading(message = 'Loading...') {
        if (this.stageContainer) {
            this.stageContainer.innerHTML = `
                <div class="text-center py-4" id="stageLoadingIndicator">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="text-muted">${message}</p>
                </div>
            `;
        }
    }

    hideLoading() {
        const loadingIndicator = document.getElementById('stageLoadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.remove();
        }
    }

    showError(message) {
        console.error('? [STAGE-MANAGER] Error:', message);
        
        if (this.stageContainer) {
            this.stageContainer.innerHTML = `
                <div class="alert alert-danger" role="alert">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Stage Manager Error:</strong> ${message}
                    <hr>
                    <div class="small">
                        <strong>Troubleshooting:</strong>
                        <ul class="mb-0 mt-1">
                            <li>Check browser console for detailed errors</li>
                            <li>Verify production stages are configured in Admin</li>
                            <li>Ensure API endpoints are accessible</li>
                        </ul>
                    </div>
                </div>
            `;
        }
    }
}

// Global initialization function
window.initializeStageManagerForModal = function() {
    console.log('?? [STAGE-MANAGER] Global initialization function called');
    
    try {
        // Get part ID from form
        const partIdInput = document.querySelector('input[name="Part.Id"]');
        const partId = partIdInput ? parseInt(partIdInput.value) || null : null;
        
        console.log('?? [STAGE-MANAGER] Part ID found:', partId);
        
        // Create global stage manager instance
        window.stageManager = new ModernStageManager(partId);
        
        console.log('? [STAGE-MANAGER] Global instance created');
        
    } catch (error) {
        console.error('? [STAGE-MANAGER] Global initialization failed:', error);
    }
};

// Auto-initialize if DOM is ready and modal is already open
document.addEventListener('DOMContentLoaded', function() {
    console.log('?? [STAGE-MANAGER] DOM Content Loaded');
    
    // Check if we're in a parts modal context
    if (document.getElementById('stage-requirements-container')) {
        console.log('?? [STAGE-MANAGER] Stage container found, auto-initializing...');
        setTimeout(() => {
            window.initializeStageManagerForModal();
        }, 100);
    } else {
        console.log('?? [STAGE-MANAGER] Stage container not found, waiting for modal...');
    }
});

console.log('? [STAGE-MANAGER] Modern Stage Manager script loaded successfully');