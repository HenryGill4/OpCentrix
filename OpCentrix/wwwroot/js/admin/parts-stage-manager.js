/**
 * Modern Stage Management Component for Part Form
 * Replaces legacy boolean checkboxes with dynamic stage selection
 */
class ModernStageManager {
    constructor(partId = null) {
        this.partId = partId;
        this.selectedStages = new Map();
        this.availableStages = [];
        this.initialized = false;
        
        this.init();
    }

    async init() {
        try {
            await this.loadAvailableStages();
            await this.loadExistingStageRequirements();
            this.initializeEventHandlers();
            this.renderStageSelection();
            this.initialized = true;
            
            console.log('? ModernStageManager initialized successfully');
        } catch (error) {
            console.error('? Error initializing ModernStageManager:', error);
            this.showError('Failed to initialize stage management');
        }
    }

    async loadAvailableStages() {
        try {
            const response = await fetch('/api/production-stages/available');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            this.availableStages = await response.json();
            console.log(`?? Loaded ${this.availableStages.length} available stages`);
        } catch (error) {
            console.error('Error loading available stages:', error);
            // Fallback to default stages
            this.availableStages = [
                { id: 1, name: 'SLS Printing', defaultHourlyRate: 85.00, defaultSetupMinutes: 45 },
                { id: 2, name: 'EDM Operations', defaultHourlyRate: 95.00, defaultSetupMinutes: 30 },
                { id: 3, name: 'CNC Machining', defaultHourlyRate: 105.00, defaultSetupMinutes: 60 },
                { id: 4, name: 'Assembly', defaultHourlyRate: 75.00, defaultSetupMinutes: 15 },
                { id: 5, name: 'Finishing', defaultHourlyRate: 65.00, defaultSetupMinutes: 30 }
            ];
        }
    }

    async loadExistingStageRequirements() {
        if (!this.partId || this.partId === 0) return;

        try {
            const response = await fetch(`/api/parts/${this.partId}/stage-requirements`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
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

            console.log(`?? Loaded ${existingStages.length} existing stage requirements`);
        } catch (error) {
            console.error('Error loading existing stage requirements:', error);
        }
    }

    initializeEventHandlers() {
        // Add stage button
        const addStageBtn = document.getElementById('add-stage-btn');
        if (addStageBtn) {
            addStageBtn.addEventListener('click', () => this.showAddStageModal());
        }

        // Stage form submission
        const stageForm = document.getElementById('stage-requirement-form');
        if (stageForm) {
            stageForm.addEventListener('submit', (e) => this.handleStageFormSubmit(e));
        }

        // Listen for form updates to sync with hidden form fields
        document.addEventListener('form-data-sync', () => this.syncWithFormData());
    }

    renderStageSelection() {
        const container = document.getElementById('stage-requirements-container');
        if (!container) {
            console.warn('Stage requirements container not found');
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
                <button type="button" class="btn btn-primary" onclick="stageManager.showAddStageModal()">
                    <i class="fas fa-plus me-2"></i>Add First Stage
                </button>
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
                         ((stage.setupMinutes + stage.teardownMinutes) / 60 * stage.hourlyRate) + 
                         stage.materialCost;

        return `
            <div class="col-md-6">
                <div class="card stage-requirement-card" data-stage-id="${stage.stageId}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <span class="badge bg-primary me-2">${stage.executionOrder}</span>
                            <h6 class="mb-0">${stage.stageName}</h6>
                        </div>
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-secondary" 
                                    onclick="stageManager.editStage(${stage.stageId})" title="Edit Stage">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger" 
                                    onclick="stageManager.removeStage(${stage.stageId})" title="Remove Stage">
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
                                <strong>Cost:</strong> $${totalCost.toFixed(2)}
                            </div>
                        </div>
                        ${stage.teardownMinutes > 0 ? `
                            <div class="mt-2 small text-muted">
                                <i class="fas fa-clock me-1"></i>Teardown: ${stage.teardownMinutes}min
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    }

    showAddStageModal() {
        const availableToAdd = this.availableStages.filter(stage => 
            !this.selectedStages.has(stage.id)
        );

        if (availableToAdd.length === 0) {
            this.showInfo('All available stages have been added to this part.');
            return;
        }

        this.showStageModal({
            title: 'Add Manufacturing Stage',
            stage: null,
            availableStages: availableToAdd,
            isEdit: false
        });
    }

    editStage(stageId) {
        const stage = this.selectedStages.get(stageId);
        if (!stage) return;

        this.showStageModal({
            title: 'Edit Manufacturing Stage',
            stage: stage,
            availableStages: [this.availableStages.find(s => s.id === stageId)],
            isEdit: true
        });
    }

    showStageModal(options) {
        const modalHtml = this.createStageModalHtml(options);
        
        // Remove existing modal
        const existingModal = document.getElementById('stageModal');
        if (existingModal) {
            existingModal.remove();
        }

        // Add modal to document
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('stageModal'));
        modal.show();

        // Initialize form
        this.initializeStageForm(options);
    }

    createStageModalHtml(options) {
        return `
            <div class="modal fade" id="stageModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${options.title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="stage-requirement-form">
                                <div class="row g-3">
                                    <div class="col-12">
                                        <label class="form-label">Production Stage</label>
                                        <select class="form-select" id="stage-select" ${options.isEdit ? 'disabled' : ''}>
                                            <option value="">Select a stage...</option>
                                            ${options.availableStages.map(stage => `
                                                <option value="${stage.id}" ${options.stage && options.stage.stageId === stage.id ? 'selected' : ''}>
                                                    ${stage.name}
                                                </option>
                                            `).join('')}
                                        </select>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Execution Order</label>
                                        <input type="number" class="form-control" id="execution-order" 
                                               value="${options.stage?.executionOrder || (this.selectedStages.size + 1)}" 
                                               min="1" max="20" required>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Estimated Hours</label>
                                        <input type="number" class="form-control" id="estimated-hours" 
                                               value="${options.stage?.estimatedHours || 1.0}" 
                                               step="0.1" min="0.1" max="200" required>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Setup Time (minutes)</label>
                                        <input type="number" class="form-control" id="setup-minutes" 
                                               value="${options.stage?.setupMinutes || 30}" 
                                               min="0" max="600">
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Teardown Time (minutes)</label>
                                        <input type="number" class="form-control" id="teardown-minutes" 
                                               value="${options.stage?.teardownMinutes || 0}" 
                                               min="0" max="300">
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Hourly Rate ($)</label>
                                        <input type="number" class="form-control" id="hourly-rate" 
                                               value="${options.stage?.hourlyRate || 85.00}" 
                                               step="0.01" min="0" max="500">
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label">Material Cost ($)</label>
                                        <input type="number" class="form-control" id="material-cost" 
                                               value="${options.stage?.materialCost || 0.00}" 
                                               step="0.01" min="0">
                                    </div>
                                    <div class="col-12">
                                        <div class="form-check">
                                            <input type="checkbox" class="form-check-input" id="is-required" 
                                                   ${options.stage?.isRequired !== false ? 'checked' : ''}>
                                            <label class="form-check-label" for="is-required">
                                                This stage is required for manufacturing
                                            </label>
                                        </div>
                                    </div>
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" onclick="stageManager.saveStage(${options.isEdit ? options.stage?.stageId : 'null'})">
                                ${options.isEdit ? 'Update Stage' : 'Add Stage'}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    initializeStageForm(options) {
        // Auto-fill hourly rate when stage is selected
        const stageSelect = document.getElementById('stage-select');
        const hourlyRateInput = document.getElementById('hourly-rate');
        const setupMinutesInput = document.getElementById('setup-minutes');

        if (stageSelect && hourlyRateInput) {
            stageSelect.addEventListener('change', () => {
                const selectedStageId = parseInt(stageSelect.value);
                const selectedStage = this.availableStages.find(s => s.id === selectedStageId);
                
                if (selectedStage) {
                    hourlyRateInput.value = selectedStage.defaultHourlyRate || 85.00;
                    setupMinutesInput.value = selectedStage.defaultSetupMinutes || 30;
                }
            });
        }
    }

    saveStage(existingStageId = null) {
        const form = document.getElementById('stage-requirement-form');
        if (!form) return;

        const formData = new FormData(form);
        const stageId = parseInt(document.getElementById('stage-select').value);
        const stageName = document.getElementById('stage-select').selectedOptions[0]?.text;

        const stageData = {
            stageId: stageId,
            stageName: stageName,
            executionOrder: parseInt(document.getElementById('execution-order').value),
            estimatedHours: parseFloat(document.getElementById('estimated-hours').value),
            setupMinutes: parseInt(document.getElementById('setup-minutes').value),
            teardownMinutes: parseInt(document.getElementById('teardown-minutes').value),
            hourlyRate: parseFloat(document.getElementById('hourly-rate').value),
            materialCost: parseFloat(document.getElementById('material-cost').value),
            isRequired: document.getElementById('is-required').checked,
            existingId: existingStageId
        };

        // Validate
        if (!stageId) {
            this.showError('Please select a production stage');
            return;
        }

        // Add/update in memory
        this.selectedStages.set(stageId, stageData);

        // Re-render
        this.renderStageSelection();
        this.syncWithFormData();

        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('stageModal'));
        modal.hide();

        this.showSuccess(existingStageId ? 'Stage updated successfully' : 'Stage added successfully');
    }

    removeStage(stageId) {
        if (confirm('Are you sure you want to remove this manufacturing stage?')) {
            this.selectedStages.delete(stageId);
            this.renderStageSelection();
            this.syncWithFormData();
            this.showSuccess('Stage removed successfully');
        }
    }

    syncWithFormData() {
        // Update hidden form fields for form submission
        const selectedStageIds = Array.from(this.selectedStages.keys());
        const executionOrders = Array.from(this.selectedStages.values()).map(s => s.executionOrder);
        const estimatedHours = Array.from(this.selectedStages.values()).map(s => s.estimatedHours);
        const hourlyRates = Array.from(this.selectedStages.values()).map(s => s.hourlyRate);
        const materialCosts = Array.from(this.selectedStages.values()).map(s => s.materialCost);

        // Update form arrays (these should be bound to the page model)
        this.updateHiddenFormArray('SelectedStageIds', selectedStageIds);
        this.updateHiddenFormArray('StageExecutionOrders', executionOrders);
        this.updateHiddenFormArray('StageEstimatedHours', estimatedHours);
        this.updateHiddenFormArray('StageHourlyRates', hourlyRates);
        this.updateHiddenFormArray('StageMaterialCosts', materialCosts);
    }

    updateHiddenFormArray(fieldName, values) {
        // Remove existing hidden fields
        const existingFields = document.querySelectorAll(`input[name^="${fieldName}"]`);
        existingFields.forEach(field => field.remove());

        // Add new hidden fields
        const form = document.getElementById('partForm');
        if (form) {
            values.forEach((value, index) => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = `${fieldName}[${index}]`;
                input.value = value;
                form.appendChild(input);
            });
        }
    }

    updateStageSummary() {
        const totalStages = this.selectedStages.size;
        const totalDuration = Array.from(this.selectedStages.values())
            .reduce((sum, stage) => sum + stage.estimatedHours, 0);
        const totalCost = Array.from(this.selectedStages.values())
            .reduce((sum, stage) => {
                const stageCost = (stage.estimatedHours * stage.hourlyRate) + 
                                 ((stage.setupMinutes + stage.teardownMinutes) / 60 * stage.hourlyRate) + 
                                 stage.materialCost;
                return sum + stageCost;
            }, 0);

        // Update summary cards if they exist
        this.updateSummaryCard('summary-total-stages', totalStages);
        this.updateSummaryCard('summary-total-duration', `${totalDuration.toFixed(1)}h`);
        this.updateSummaryCard('summary-total-cost', `$${totalCost.toFixed(2)}`);
        
        // Update complexity
        const complexity = this.calculateComplexity(totalStages, totalDuration);
        this.updateSummaryCard('summary-complexity', complexity);
    }

    updateSummaryCard(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    }

    calculateComplexity(stageCount, totalHours) {
        const score = stageCount + Math.floor(totalHours / 4);
        
        if (score <= 2) return 'Simple';
        if (score <= 4) return 'Medium';
        if (score <= 6) return 'Complex';
        return 'Very Complex';
    }

    // Utility methods
    showSuccess(message) {
        this.showToast(message, 'success');
    }

    showError(message) {
        this.showToast(message, 'danger');
    }

    showInfo(message) {
        this.showToast(message, 'info');
    }

    showToast(message, type = 'info') {
        // Simple toast implementation - could be enhanced with a proper toast library
        const toastContainer = this.getOrCreateToastContainer();
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show`;
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        toastContainer.appendChild(toast);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 5000);
    }

    getOrCreateToastContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'position-fixed top-0 end-0 p-3';
            container.style.zIndex = '1055';
            document.body.appendChild(container);
        }
        return container;
    }
}

// Global instance
let stageManager;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Get part ID from form if editing
    const partIdInput = document.getElementById('Part_Id') || document.querySelector('input[name="Part.Id"]');
    const partId = partIdInput ? parseInt(partIdInput.value) : null;
    
    stageManager = new ModernStageManager(partId);
});

// Export for use in other scripts
window.ModernStageManager = ModernStageManager;