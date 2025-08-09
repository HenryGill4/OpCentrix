/**
 * Modern Stage Management Component for Part Form
 * Replaces legacy boolean checkboxes with dynamic stage selection
 * Enhanced with robust error handling and fallback mechanisms
 */
class ModernStageManager {
    constructor(partId = null) {
        this.partId = partId;
        this.selectedStages = new Map();
        this.availableStages = [];
        this.initialized = false;
        this.loadingIndicator = null;
        this.errorIndicator = null;
        
        this.init();
    }

    async init() {
        try {
            console.log('?? [STAGE-MANAGER] Initializing ModernStageManager...');
            
            // Show loading state
            this.showLoadingState();
            
            // Load data with enhanced error handling
            await this.loadAvailableStages();
            await this.loadExistingStageRequirements();
            
            // Initialize UI
            this.initializeEventHandlers();
            this.renderStageSelection();
            
            this.initialized = true;
            console.log('? [STAGE-MANAGER] ModernStageManager initialized successfully');
            
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error initializing ModernStageManager:', error);
            this.showErrorState('Failed to initialize stage management');
        }
    }

    async loadAvailableStages() {
        try {
            console.log('?? [STAGE-MANAGER] Loading available stages...');
            
            const response = await fetch('/api/production-stages/available', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin'
            });
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            this.availableStages = await response.json();
            console.log(`? [STAGE-MANAGER] Loaded ${this.availableStages.length} available stages`);
            
        } catch (error) {
            console.warn('?? [STAGE-MANAGER] API failed, using fallback stages:', error);
            
            // Enhanced fallback with more comprehensive data
            this.availableStages = [
                { 
                    id: 1, 
                    name: 'SLS Printing', 
                    description: 'Selective Laser Sintering',
                    defaultHourlyRate: 85.00, 
                    defaultSetupMinutes: 45,
                    defaultTeardownMinutes: 30,
                    isActive: true
                },
                { 
                    id: 2, 
                    name: 'EDM Operations', 
                    description: 'Electrical Discharge Machining',
                    defaultHourlyRate: 95.00, 
                    defaultSetupMinutes: 30,
                    defaultTeardownMinutes: 15,
                    isActive: true
                },
                { 
                    id: 3, 
                    name: 'CNC Machining', 
                    description: 'Computer Numerical Control Machining',
                    defaultHourlyRate: 105.00, 
                    defaultSetupMinutes: 60,
                    defaultTeardownMinutes: 20,
                    isActive: true
                },
                { 
                    id: 4, 
                    name: 'Assembly', 
                    description: 'Component Assembly',
                    defaultHourlyRate: 75.00, 
                    defaultSetupMinutes: 15,
                    defaultTeardownMinutes: 10,
                    isActive: true
                },
                { 
                    id: 5, 
                    name: 'Finishing', 
                    description: 'Surface Finishing and Post-Processing',
                    defaultHourlyRate: 65.00, 
                    defaultSetupMinutes: 30,
                    defaultTeardownMinutes: 15,
                    isActive: true
                }
            ];
            
            console.log('?? [STAGE-MANAGER] Using fallback stage data');
        }
    }

    async loadExistingStageRequirements() {
        if (!this.partId || this.partId === 0) {
            console.log('?? [STAGE-MANAGER] New part - no existing stage requirements to load');
            return;
        }

        try {
            console.log(`?? [STAGE-MANAGER] Loading existing stages for part ${this.partId}...`);
            
            const response = await fetch(`/api/parts/${this.partId}/stage-requirements`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin'
            });
            
            if (!response.ok) {
                if (response.status === 404) {
                    console.log('?? [STAGE-MANAGER] No existing stage requirements found for part');
                    return;
                }
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            const existingStages = await response.json();
            
            existingStages.forEach(stage => {
                this.selectedStages.set(stage.productionStageId, {
                    stageId: stage.productionStageId,
                    stageName: stage.productionStage?.name || 'Unknown Stage',
                    executionOrder: stage.executionOrder || 1,
                    estimatedHours: stage.estimatedHours || 1.0,
                    setupMinutes: stage.setupTimeMinutes || 30,
                    teardownMinutes: stage.teardownTimeMinutes || 0,
                    hourlyRate: stage.hourlyRateOverride || stage.productionStage?.defaultHourlyRate || 85.00,
                    materialCost: stage.materialCost || 0.00,
                    isRequired: stage.isRequired !== false,
                    existingId: stage.id
                });
            });

            console.log(`? [STAGE-MANAGER] Loaded ${existingStages.length} existing stage requirements`);
            
        } catch (error) {
            console.warn('?? [STAGE-MANAGER] Error loading existing stage requirements:', error);
            // Don't throw - just continue with empty requirements
        }
    }

    showLoadingState() {
        const container = document.getElementById('stage-requirements-container');
        if (!container) return;
        
        container.innerHTML = `
            <div class="text-center py-4" id="stageLoadingIndicator">
                <div class="spinner-border text-primary mb-3" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-muted">Loading manufacturing stages...</p>
            </div>
        `;
    }

    showErrorState(message) {
        const container = document.getElementById('stage-requirements-container');
        if (!container) return;
        
        container.innerHTML = `
            <div class="text-center py-4" id="stageErrorIndicator">
                <div class="alert alert-warning d-flex align-items-center" role="alert">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <div>
                        <strong>Unable to Load Stages</strong><br>
                        ${message}
                    </div>
                </div>
                <div class="mt-3">
                    <button type="button" class="btn btn-outline-primary btn-sm me-2" onclick="stageManager?.init()">
                        <i class="fas fa-redo me-1"></i>Retry Loading
                    </button>
                    <button type="button" class="btn btn-outline-secondary btn-sm" onclick="stageManager?.useDefaultStages()">
                        <i class="fas fa-tools me-1"></i>Use Default Stages
                    </button>
                </div>
            </div>
        `;
    }

    useDefaultStages() {
        console.log('?? [STAGE-MANAGER] Using default stages as fallback');
        
        // Clear existing state
        this.selectedStages.clear();
        
        // Use the fallback stages from loadAvailableStages
        this.loadAvailableStages().then(() => {
            this.renderStageSelection();
            this.showInfo('Default manufacturing stages loaded successfully');
        });
    }

    renderStageSelection() {
        const container = document.getElementById('stage-requirements-container');
        if (!container) {
            console.warn('?? [STAGE-MANAGER] Stage requirements container not found');
            return;
        }

        // Hide loading/error indicators
        const loadingIndicator = document.getElementById('stageLoadingIndicator');
        const errorIndicator = document.getElementById('stageErrorIndicator');
        
        if (loadingIndicator) loadingIndicator.style.display = 'none';
        if (errorIndicator) errorIndicator.style.display = 'none';

        if (this.availableStages.length === 0) {
            container.innerHTML = this.renderErrorFallback();
            return;
        }

        if (this.selectedStages.size === 0) {
            container.innerHTML = this.renderEmptyState();
        } else {
            container.innerHTML = this.renderSelectedStages();
        }

        this.updateStageSummary();
    }

    renderEmptyState() {
        return `
            <div class="text-center py-4">
                <i class="fas fa-tasks fa-3x text-muted mb-3"></i>
                <h6 class="text-muted">No Manufacturing Stages Selected</h6>
                <p class="text-muted mb-3">Add manufacturing stages to define the production workflow for this part.</p>
                <button type="button" class="btn btn-primary" onclick="stageManager?.showAddStageModal()">
                    <i class="fas fa-plus me-2"></i>Add First Stage
                </button>
            </div>
        `;
    }

    renderErrorFallback() {
        return `
            <div class="text-center py-4">
                <i class="fas fa-exclamation-triangle fa-3x text-warning mb-3"></i>
                <h6 class="text-warning">Stage Data Unavailable</h6>
                <p class="text-muted mb-3">Unable to load manufacturing stages. Please check your connection or contact support.</p>
                <div class="d-flex justify-content-center gap-2">
                    <button type="button" class="btn btn-outline-primary btn-sm" onclick="stageManager?.init()">
                        <i class="fas fa-redo me-1"></i>Retry
                    </button>
                    <button type="button" class="btn btn-outline-warning btn-sm" onclick="stageManager?.useDefaultStages()">
                        <i class="fas fa-tools me-1"></i>Use Defaults
                    </button>
                </div>
            </div>
        `;
    }

    renderSelectedStages() {
        const sortedStages = Array.from(this.selectedStages.values())
            .sort((a, b) => a.executionOrder - b.executionOrder);

        return `
            <div class="row g-3">
                ${sortedStages.map((stage, index) => this.renderStageCard(stage, index)).join('')}
            </div>
        `;
    }

    renderStageCard(stage, index) {
        const totalCost = (stage.estimatedHours * stage.hourlyRate) + 
                         ((stage.setupMinutes + (stage.teardownMinutes || 0)) / 60 * stage.hourlyRate) + 
                         stage.materialCost;

        return `
            <div class="col-md-6">
                <div class="card stage-requirement-card border-primary" data-stage-id="${stage.stageId}">
                    <div class="card-header bg-light d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <span class="badge bg-primary me-2">${stage.executionOrder}</span>
                            <h6 class="mb-0 fw-bold">${stage.stageName}</h6>
                        </div>
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-secondary btn-sm" 
                                    onclick="stageManager?.editStage(${stage.stageId})" title="Edit Stage">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-sm" 
                                    onclick="stageManager?.removeStage(${stage.stageId})" title="Remove Stage">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="row g-2 small">
                            <div class="col-6">
                                <strong>Duration:</strong> ${stage.estimatedHours}h
                            </div>
                            <div class="col-6">
                                <strong>Setup:</strong> ${stage.setupMinutes}min
                            </div>
                            <div class="col-6">
                                <strong>Rate:</strong> $${stage.hourlyRate}/hr
                            </div>
                            <div class="col-6">
                                <strong>Total Cost:</strong> $${totalCost.toFixed(2)}
                            </div>
                        </div>
                        ${stage.teardownMinutes && stage.teardownMinutes > 0 ? `
                            <div class="mt-2 small text-muted">
                                <i class="fas fa-clock me-1"></i>Teardown: ${stage.teardownMinutes}min
                            </div>
                        ` : ''}
                        ${stage.materialCost > 0 ? `
                            <div class="mt-1 small text-info">
                                <i class="fas fa-dollar-sign me-1"></i>Materials: $${stage.materialCost.toFixed(2)}
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    }

    initializeEventHandlers() {
        // Add stage button
        const addStageBtn = document.getElementById('add-stage-btn');
        if (addStageBtn) {
            addStageBtn.addEventListener('click', () => this.showAddStageModal());
        }

        console.log('? [STAGE-MANAGER] Event handlers initialized');
    }

    showAddStageModal() {
        console.log('?? [STAGE-MANAGER] Opening add stage modal...');
        
        const availableToAdd = this.availableStages.filter(stage => 
            !this.selectedStages.has(stage.id)
        );

        if (availableToAdd.length === 0) {
            this.showInfo('All available stages have been added to this part.');
            return;
        }

        // For now, show a simple selection interface
        this.showStageSelectionInterface(availableToAdd);
    }

    showStageSelectionInterface(availableStages) {
        // Create a simple modal-like interface for stage selection
        const modalHtml = `
            <div class="modal fade" id="stageSelectionModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Add Manufacturing Stage</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="list-group">
                                ${availableStages.map(stage => `
                                    <a href="#" class="list-group-item list-group-item-action" 
                                       onclick="stageManager?.addStage(${stage.id}); bootstrap.Modal.getInstance(document.getElementById('stageSelectionModal')).hide();">
                                        <div class="d-flex w-100 justify-content-between">
                                            <h6 class="mb-1">${stage.name}</h6>
                                            <small>$${stage.defaultHourlyRate}/hr</small>
                                        </div>
                                        <p class="mb-1">${stage.description}</p>
                                        <small>Setup: ${stage.defaultSetupMinutes}min</small>
                                    </a>
                                `).join('')}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal
        const existing = document.getElementById('stageSelectionModal');
        if (existing) existing.remove();

        // Add and show modal
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        const modal = new bootstrap.Modal(document.getElementById('stageSelectionModal'));
        modal.show();
    }

    // Add stage to requirements
    addStage(stageId) {
        try {
            console.log(`?? [STAGE-MANAGER] Adding stage ${stageId}`);
            
            const stage = this.availableStages.find(s => s.id === stageId);
            if (!stage) {
                console.warn(`?? [STAGE-MANAGER] Stage ${stageId} not found in available stages`);
                return false;
            }
            
            if (this.selectedStages.has(stageId)) {
                console.warn(`?? [STAGE-MANAGER] Stage ${stageId} already selected`);
                return false;
            }
            
            // Add stage with default values
            const stageData = {
                stageId: stageId,
                stageName: stage.name,
                executionOrder: this.selectedStages.size + 1,
                estimatedHours: stage.defaultDurationHours || 1.0,
                hourlyRate: stage.defaultHourlyRate || 85.00,
                materialCost: stage.defaultMaterialCost || 0.00,
                setupTimeMinutes: stage.defaultSetupMinutes || 30,
                isRequired: true,
                isBlocking: true
            };
            
            this.selectedStages.set(stageId, stageData);
            this.renderSelectedStages();
            this.updateSummary();
            
            console.log(`? [STAGE-MANAGER] Stage ${stageId} (${stage.name}) added successfully`);
            return true;
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error adding stage:', error);
            return false;
        }
    }

    // Remove stage from requirements
    removeStage(stageId) {
        try {
            console.log(`?? [STAGE-MANAGER] Removing stage ${stageId}`);
            
            if (!this.selectedStages.has(stageId)) {
                console.warn(`?? [STAGE-MANAGER] Stage ${stageId} not in selected stages`);
                return false;
            }
            
            this.selectedStages.delete(stageId);
            this.reorderStages();
            this.renderSelectedStages();
            this.updateSummary();
            
            // Notify form manager of changes
            this.notifyStageChange();
            
            console.log(`? [STAGE-MANAGER] Stage ${stageId} removed successfully`);
            return true;
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error removing stage:', error);
            return false;
        }
    }

    // Edit stage properties
    editStage(stageId) {
        try {
            console.log(`?? [STAGE-MANAGER] Editing stage ${stageId}`);
            
            const stageData = this.selectedStages.get(stageId);
            if (!stageData) {
                console.warn(`?? [STAGE-MANAGER] Stage ${stageId} not found for editing`);
                return false;
            }
            
            this.showEditStageModal(stageData);
            return true;
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error editing stage:', error);
            return false;
        }
    }

    // Show edit stage modal
    showEditStageModal(stageData) {
        const modalHtml = `
            <div class="modal fade" id="editStageModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Edit Stage: ${stageData.stageName}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="editStageForm">
                                <input type="hidden" id="editStageId" value="${stageData.stageId}">
                                
                                <div class="mb-3">
                                    <label for="editExecutionOrder" class="form-label">Execution Order</label>
                                    <input type="number" class="form-control" id="editExecutionOrder" 
                                           value="${stageData.executionOrder}" min="1" max="10" required>
                                </div>
                                
                                <div class="mb-3">
                                    <label for="editEstimatedHours" class="form-label">Estimated Hours</label>
                                    <input type="number" class="form-control" id="editEstimatedHours" 
                                           value="${stageData.estimatedHours}" min="0.1" step="0.1" required>
                                </div>
                                
                                <div class="mb-3">
                                    <label for="editHourlyRate" class="form-label">Hourly Rate ($)</label>
                                    <input type="number" class="form-control" id="editHourlyRate" 
                                           value="${stageData.hourlyRate}" min="0" step="0.01" required>
                                </div>
                                
                                <div class="mb-3">
                                    <label for="editMaterialCost" class="form-label">Material Cost ($)</label>
                                    <input type="number" class="form-control" id="editMaterialCost" 
                                           value="${stageData.materialCost}" min="0" step="0.01">
                                </div>
                                
                                <div class="mb-3">
                                    <label for="editSetupMinutes" class="form-label">Setup Time (minutes)</label>
                                    <input type="number" class="form-control" id="editSetupMinutes" 
                                           value="${stageData.setupTimeMinutes || 30}" min="0" step="1">
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" onclick="stageManager?.saveStageEdit()">
                                Save Changes
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal
        const existing = document.getElementById('editStageModal');
        if (existing) existing.remove();

        // Add and show modal
        document.body.insertAdjacentHTML('beforeend', modalHtml);
        const modal = new bootstrap.Modal(document.getElementById('editStageModal'));
        modal.show();
    }

    // Save stage edit
    saveStageEdit() {
        try {
            const stageId = parseInt(document.getElementById('editStageId').value);
            const executionOrder = parseInt(document.getElementById('editExecutionOrder').value);
            const estimatedHours = parseFloat(document.getElementById('editEstimatedHours').value);
            const hourlyRate = parseFloat(document.getElementById('editHourlyRate').value);
            const materialCost = parseFloat(document.getElementById('editMaterialCost').value) || 0;
            const setupMinutes = parseInt(document.getElementById('editSetupMinutes').value) || 30;

            // Validate inputs
            if (!stageId || !executionOrder || !estimatedHours || !hourlyRate) {
                this.showError('Please fill in all required fields');
                return;
            }

            // Update stage data
            const stageData = this.selectedStages.get(stageId);
            if (stageData) {
                stageData.executionOrder = executionOrder;
                stageData.estimatedHours = estimatedHours;
                stageData.hourlyRate = hourlyRate;
                stageData.materialCost = materialCost;
                stageData.setupTimeMinutes = setupMinutes;

                // Re-render
                this.renderSelectedStages();
                this.updateSummary();
                this.notifyStageChange();

                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('editStageModal'));
                if (modal) modal.hide();

                this.showSuccess('Stage updated successfully');
            }
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error saving stage edit:', error);
            this.showError('Error saving stage changes');
        }
    }

    // Reorder stages to maintain sequential execution orders
    reorderStages() {
        const stages = Array.from(this.selectedStages.values()).sort((a, b) => a.executionOrder - b.executionOrder);
        
        stages.forEach((stage, index) => {
            stage.executionOrder = index + 1;
        });
    }

    // Update stage summary
    updateSummary() {
        this.updateStageSummary();
    }

    updateStageSummary() {
        const summaryContainer = document.getElementById('stage-summary-container');
        if (!summaryContainer) return;

        if (this.selectedStages.size === 0) {
            summaryContainer.innerHTML = `
                <div class="text-muted text-center py-2">
                    <i class="fas fa-info-circle me-1"></i>
                    No manufacturing stages selected
                </div>
            `;
            return;
        }

        const stages = Array.from(this.selectedStages.values()).sort((a, b) => a.executionOrder - b.executionOrder);
        const totalHours = stages.reduce((sum, stage) => sum + stage.estimatedHours, 0);
        const totalCost = stages.reduce((sum, stage) => {
            const laborCost = stage.estimatedHours * stage.hourlyRate;
            const setupCost = (stage.setupTimeMinutes || 0) / 60 * stage.hourlyRate;
            return sum + laborCost + setupCost + stage.materialCost;
        }, 0);

        summaryContainer.innerHTML = `
            <div class="row g-2 text-center">
                <div class="col-md-3">
                    <div class="border rounded p-2">
                        <div class="h6 mb-0 text-primary">${stages.length}</div>
                        <small class="text-muted">Stages</small>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="border rounded p-2">
                        <div class="h6 mb-0 text-info">${totalHours.toFixed(1)}h</div>
                        <small class="text-muted">Total Time</small>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="border rounded p-2">
                        <div class="h6 mb-0 text-success">$${totalCost.toFixed(2)}</div>
                        <small class="text-muted">Est. Cost</small>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="border rounded p-2">
                        <div class="h6 mb-0 text-warning">${stages.map(s => s.executionOrder).join('?')}</div>
                        <small class="text-muted">Workflow</small>
                    </div>
                </div>
            </div>
        `;
    }

    // Get stage data for form submission
    getStageDataForSubmission() {
        try {
            const stageData = Array.from(this.selectedStages.entries());
            
            const result = {
                stageIds: stageData.map(([stageId, data]) => stageId),
                executionOrders: stageData.map(([stageId, data]) => data.executionOrder || 1),
                estimatedHours: stageData.map(([stageId, data]) => data.estimatedHours || 1.0),
                hourlyRates: stageData.map(([stageId, data]) => data.hourlyRate || 85.00),
                materialCosts: stageData.map(([stageId, data]) => data.materialCost || 0.00)
            };

            console.log('?? [STAGE-MANAGER] Stage data for submission:', result);
            return result;
        } catch (error) {
            console.error('? [STAGE-MANAGER] Error getting stage data for submission:', error);
            return {
                stageIds: [],
                executionOrders: [],
                estimatedHours: [],
                hourlyRates: [],
                materialCosts: []
            };
        }
    }

    // Notify external listeners of stage changes
    notifyStageChange() {
        if (this.onStageChange && typeof this.onStageChange === 'function') {
            try {
                this.onStageChange();
            } catch (error) {
                console.error('? [STAGE-MANAGER] Error in stage change callback:', error);
            }
        }

        // Dispatch custom event
        const event = new CustomEvent('stageSelectionChanged', {
            detail: {
                selectedStages: Array.from(this.selectedStages.entries()),
                stageCount: this.selectedStages.size
            }
        });
        document.dispatchEvent(event);
    }

    // Show info message
    showInfo(message) {
        this.showMessage('info', message);
    }

    // Show success message
    showSuccess(message) {
        this.showMessage('success', message);
    }

    // Show error message
    showError(message) {
        this.showMessage('error', message);
    }

    // Show message using available notification system
    showMessage(type, message) {
        if (window.showToast) {
            window.showToast(type, message);
        } else if (window.showToastMessage) {
            window.showToastMessage(type, message);
        } else {
            console.log(`[${type.toUpperCase()}] ${message}`);
            
            // Fallback: show in stage container
            const container = document.getElementById('stage-requirements-container');
            if (container) {
                const alertClass = type === 'error' ? 'alert-danger' : type === 'success' ? 'alert-success' : 'alert-info';
                const alertHtml = `
                    <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                        ${message}
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                `;
                container.insertAdjacentHTML('afterbegin', alertHtml);
                
                // Auto dismiss after 3 seconds
                setTimeout(() => {
                    const alert = container.querySelector('.alert');
                    if (alert) alert.remove();
                }, 3000);
            }
        }
    }

    // Validation method for form integration
    validateStages() {
        const result = { isValid: true, errors: [] };

        try {
            // Check if any stages are selected
            if (this.selectedStages.size === 0) {
                // This is not necessarily an error - business rules determine if stages are required
                console.log('?? [STAGE-MANAGER] No stages selected');
                return result;
            }

            // Validate each selected stage
            for (const [stageId, stageData] of this.selectedStages) {
                if (!stageData.estimatedHours || stageData.estimatedHours <= 0) {
                    result.isValid = false;
                    result.errors.push(`${stageData.stageName}: Estimated hours must be greater than 0`);
                }

                if (!stageData.hourlyRate || stageData.hourlyRate <= 0) {
                    result.isValid = false;
                    result.errors.push(`${stageData.stageName}: Hourly rate must be greater than 0`);
                }

                if (stageData.materialCost < 0) {
                    result.isValid = false;
                    result.errors.push(`${stageData.stageName}: Material cost cannot be negative`);
                }
            }

            // Check for duplicate execution orders
            const executionOrders = Array.from(this.selectedStages.values()).map(s => s.executionOrder);
            const uniqueOrders = new Set(executionOrders);
            if (uniqueOrders.size !== executionOrders.length) {
                result.isValid = false;
                result.errors.push('Stages cannot have duplicate execution orders');
            }

        } catch (error) {
            console.error('? [STAGE-MANAGER] Error validating stages:', error);
            result.isValid = false;
            result.errors.push('Error validating stage data');
        }

        return result;
    }
}

// Initialize stage manager when DOM is ready
let stageManager = null;

function initializeStageManager() {
    try {
        // Get part ID from form if available
        const partIdInput = document.querySelector('input[name="Part.Id"]');
        const partId = partIdInput ? parseInt(partIdInput.value) || null : null;
        
        console.log('?? [STAGE-MANAGER] Initializing with partId:', partId);
        
        // Create stage manager instance
        stageManager = new ModernStageManager(partId);
        
        // Expose globally for form interaction
        window.stageManager = stageManager;
        
        console.log('? [STAGE-MANAGER] Stage manager initialized and exposed globally');
        
    } catch (error) {
        console.error('? [STAGE-MANAGER] Error initializing stage manager:', error);
        
        // Show fallback message
        const container = document.getElementById('stage-requirements-container');
        if (container) {
            container.innerHTML = `
                <div class="alert alert-danger">
                    <strong>Error:</strong> Failed to initialize stage management. 
                    Please refresh the page and try again.
                </div>
            `;
        }
    }
}

// Auto-initialize when script loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeStageManager);
} else {
    initializeStageManager();
}

// Enhanced global stage manager functions for form integration
window.getStageManager = function() {
    return window.stageManager;
};

window.ensureStageManager = function() {
    if (!window.stageManager) {
        console.log('?? [STAGE-MANAGER] Stage manager not found, attempting to initialize...');
        initializeStageManager();
    }
    return window.stageManager;
};

window.isStageManagerReady = function() {
    return !!(window.stageManager && window.stageManager.initialized);
};

// Expose debugging function
window.debugStageManagerDetailed = function() {
    console.log('?? [STAGE-MANAGER] Detailed Debug Information:');
    console.log('- ModernStageManager class available:', typeof ModernStageManager);
    console.log('- window.stageManager:', window.stageManager);
    console.log('- stageManager.initialized:', window.stageManager?.initialized);
    console.log('- availableStages count:', window.stageManager?.availableStages?.length);
    console.log('- selectedStages count:', window.stageManager?.selectedStages?.size);
    console.log('- stage container exists:', !!document.getElementById('stage-requirements-container'));
    
    if (window.stageManager && window.stageManager.availableStages) {
        console.log('- Available stages:', window.stageManager.availableStages.map(s => ({ id: s.id, name: s.name })));
    }
    
    if (window.stageManager && window.stageManager.selectedStages) {
        console.log('- Selected stages:', Array.from(window.stageManager.selectedStages.entries()));
    }
    
    return {
        classAvailable: typeof ModernStageManager !== 'undefined',
        instanceAvailable: !!window.stageManager,
        initialized: window.stageManager?.initialized || false,
        availableStagesCount: window.stageManager?.availableStages?.length || 0,
        selectedStagesCount: window.stageManager?.selectedStages?.size || 0
    };
};

console.log('?? [STAGE-MANAGER] Enhanced ModernStageManager loaded and ready');