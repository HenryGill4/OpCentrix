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

    addStage(stageId) {
        const stage = this.availableStages.find(s => s.id === stageId);
        if (!stage) {
            console.error('? [STAGE-MANAGER] Stage not found:', stageId);
            return;
        }

        const executionOrder = this.selectedStages.size + 1;
        
        this.selectedStages.set(stageId, {
            stageId: stageId,
            stageName: stage.name,
            executionOrder: executionOrder,
            estimatedHours: 2.0, // Default hours
            setupMinutes: stage.defaultSetupMinutes || 30,
            teardownMinutes: stage.defaultTeardownMinutes || 0,
            hourlyRate: stage.defaultHourlyRate || 85.00,
            materialCost: 0.00,
            isRequired: true,
            existingId: null // New stage
        });

        this.renderStageSelection();
        this.showSuccess(`Added ${stage.name} to manufacturing workflow`);
        
        console.log(`? [STAGE-MANAGER] Added stage: ${stage.name}`);
    }

    removeStage(stageId) {
        const stage = this.selectedStages.get(stageId);
        if (!stage) return;

        if (confirm(`Remove ${stage.stageName} from manufacturing workflow?`)) {
            this.selectedStages.delete(stageId);
            this.renderStageSelection();
            this.showSuccess(`Removed ${stage.stageName} from workflow`);
            
            console.log(`??? [STAGE-MANAGER] Removed stage: ${stage.stageName}`);
        }
    }

    updateStageSummary() {
        // Update stage summary information
        const totalStages = this.selectedStages.size;
        const totalHours = Array.from(this.selectedStages.values())
            .reduce((sum, stage) => sum + stage.estimatedHours, 0);
        
        // Update any summary displays
        const summaryElement = document.getElementById('stage-summary');
        if (summaryElement) {
            summaryElement.innerHTML = `
                <div class="d-flex justify-content-between">
                    <span><strong>${totalStages}</strong> stages</span>
                    <span><strong>${totalHours.toFixed(1)}h</strong> total</span>
                </div>
            `;
        }
    }

    showSuccess(message) {
        // Simple success notification
        if (window.showToast) {
            window.showToast('success', message);
        } else {
            console.log(`? [STAGE-MANAGER] ${message}`);
        }
    }

    showInfo(message) {
        // Simple info notification
        if (window.showToast) {
            window.showToast('info', message);
        } else {
            console.log(`?? [STAGE-MANAGER] ${message}`);
        }
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

console.log('?? [STAGE-MANAGER] Enhanced ModernStageManager loaded and ready');