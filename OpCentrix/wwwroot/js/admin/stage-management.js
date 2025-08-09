/**
 * OpCentrix Stage Management JavaScript
 * Enhanced functionality for Advanced Stage Management page
 * Integrates with Parts Form for seamless workflow management
 */

class StageManagement {
    constructor() {
        this.currentDetailsType = null;
        this.currentDetailsId = null;
        this.modal = null;
        this.init();
    }

    init() {
        console.log('?? Initializing Stage Management...');
        this.initializeTooltips();
        this.initializeModals();
        this.setupEventListeners();
        console.log('? Stage Management initialized');
    }

    initializeTooltips() {
        // Initialize Bootstrap tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    initializeModals() {
        // Initialize the details modal
        const detailsModalElement = document.getElementById('detailsModal');
        if (detailsModalElement) {
            this.modal = new bootstrap.Modal(detailsModalElement);
        }
    }

    setupEventListeners() {
        // Add event listeners for enhanced functionality
        this.setupTabSwitching();
        this.setupFormValidation();
    }

    setupTabSwitching() {
        // Enhanced tab switching with URL hash support
        const tabs = document.querySelectorAll('#managementTabs button[data-bs-toggle="tab"]');
        tabs.forEach(tab => {
            tab.addEventListener('shown.bs.tab', (event) => {
                const targetId = event.target.getAttribute('data-bs-target').substring(1);
                window.location.hash = targetId;
            });
        });

        // Load tab from URL hash on page load
        if (window.location.hash) {
            const hashTab = document.querySelector(`#managementTabs button[data-bs-target="${window.location.hash}"]`);
            if (hashTab) {
                bootstrap.Tab.getOrCreateInstance(hashTab).show();
            }
        }
    }

    setupFormValidation() {
        // Add real-time form validation
        const forms = document.querySelectorAll('form[method="post"]');
        forms.forEach(form => {
            form.addEventListener('submit', this.validateForm.bind(this));
        });
    }

    validateForm(event) {
        const form = event.target;
        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                this.showFieldError(field, 'This field is required');
                isValid = false;
            } else {
                this.clearFieldError(field);
            }
        });

        if (!isValid) {
            event.preventDefault();
            this.showAlert('Please fill in all required fields', 'danger');
        }
    }

    showFieldError(field, message) {
        field.classList.add('is-invalid');
        
        let errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            field.parentNode.appendChild(errorDiv);
        }
        errorDiv.textContent = message;
    }

    clearFieldError(field) {
        field.classList.remove('is-invalid');
        const errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }

    showAlert(message, type = 'info') {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fas fa-${type === 'danger' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        const container = document.querySelector('.container-fluid');
        const firstChild = container.firstElementChild;
        const alertDiv = document.createElement('div');
        alertDiv.innerHTML = alertHtml;
        container.insertBefore(alertDiv.firstElementChild, firstChild);
    }

    // Template Management Methods
    viewTemplate(templateId) {
        this.currentDetailsType = 'template';
        this.currentDetailsId = templateId;
        this.showDetailsModal('Workflow Template Details', templateId, 'WorkflowTemplateDetails');
    }

    editTemplate(templateId) {
        this.currentDetailsType = 'template';
        this.currentDetailsId = templateId;
        this.showDetailsModal('Edit Workflow Template', templateId, 'WorkflowTemplateDetails', true);
    }

    deleteTemplate(templateId, templateName) {
        this.showConfirmDialog(
            'Delete Workflow Template',
            `Are you sure you want to delete the workflow template "${templateName}"?`,
            () => this.submitDeleteForm('DeleteWorkflowTemplate', 'templateId', templateId)
        );
    }

    // Dependency Management Methods
    editDependency(dependencyId) {
        this.currentDetailsType = 'dependency';
        this.currentDetailsId = dependencyId;
        this.showDetailsModal('Edit Stage Dependency', dependencyId, 'StageDependencyDetails', true);
    }

    deleteDependency(dependencyId) {
        this.showConfirmDialog(
            'Delete Stage Dependency',
            'Are you sure you want to delete this stage dependency?',
            () => this.submitDeleteForm('DeleteStageDependency', 'dependencyId', dependencyId)
        );
    }

    // Resource Pool Management Methods
    viewPool(poolId) {
        this.currentDetailsType = 'pool';
        this.currentDetailsId = poolId;
        this.showDetailsModal('Resource Pool Details', poolId, 'ResourcePoolDetails');
    }

    editPool(poolId) {
        this.currentDetailsType = 'pool';
        this.currentDetailsId = poolId;
        this.showDetailsModal('Edit Resource Pool', poolId, 'ResourcePoolDetails', true);
    }

    deletePool(poolId, poolName) {
        this.showConfirmDialog(
            'Delete Resource Pool',
            `Are you sure you want to delete the resource pool "${poolName}"?`,
            () => this.submitDeleteForm('DeleteResourcePool', 'poolId', poolId)
        );
    }

    // Utility Methods
    showDetailsModal(title, id, handlerName, isEdit = false) {
        if (!this.modal) {
            console.error('Details modal not initialized');
            return;
        }

        const titleElement = document.getElementById('detailsModalTitle');
        const bodyElement = document.getElementById('detailsModalBody');
        
        titleElement.textContent = title;
        bodyElement.innerHTML = '<div class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>';
        
        this.modal.show();
        
        // Load details via AJAX
        const url = `${window.location.pathname}?handler=${handlerName}&${this.currentDetailsType}Id=${id}`;
        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                this.displayDetails(data, isEdit);
            })
            .catch(error => {
                console.error('Error loading details:', error);
                bodyElement.innerHTML = '<div class="alert alert-danger">Error loading details. Please try again.</div>';
            });
    }

    displayDetails(data, isEdit) {
        const bodyElement = document.getElementById('detailsModalBody');
        
        if (this.currentDetailsType === 'template') {
            bodyElement.innerHTML = this.createTemplateDetailsHtml(data, isEdit);
        } else if (this.currentDetailsType === 'dependency') {
            bodyElement.innerHTML = this.createDependencyDetailsHtml(data, isEdit);
        } else if (this.currentDetailsType === 'pool') {
            bodyElement.innerHTML = this.createPoolDetailsHtml(data, isEdit);
        }
    }

    createTemplateDetailsHtml(data, isEdit) {
        const editAlert = isEdit ? 
            '<div class="alert alert-info"><i class="fas fa-info-circle me-2"></i>Edit functionality will be implemented in a future update.</div>' :
            '';

        return `
            ${editAlert}
            <div class="row mb-3">
                <div class="col-md-6">
                    <strong>Template Name:</strong>
                    <p class="mb-0">${data.name}</p>
                </div>
                <div class="col-md-6">
                    <strong>Complexity:</strong>
                    <p class="mb-0"><span class="badge bg-secondary">${data.complexity}</span></p>
                </div>
            </div>
            <div class="mb-3">
                <strong>Description:</strong>
                <p class="mb-0">${data.description || 'No description provided'}</p>
            </div>
            <div class="row mb-3">
                <div class="col-md-4">
                    <strong>Category:</strong>
                    <p class="mb-0">${data.category || 'None'}</p>
                </div>
                <div class="col-md-4">
                    <strong>Duration:</strong>
                    <p class="mb-0">${data.estimatedDuration} hours</p>
                </div>
                <div class="col-md-4">
                    <strong>Cost:</strong>
                    <p class="mb-0">$${data.estimatedCost.toFixed(2)}</p>
                </div>
            </div>
            <hr>
            <h6>Usage Statistics</h6>
            <div class="row">
                <div class="col-md-3">
                    <div class="text-center">
                        <div class="h4 text-primary">${data.stats.usageCount}</div>
                        <small class="text-muted">Usage Count</small>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="text-center">
                        <div class="h4 text-success">${data.stats.completedJobs}</div>
                        <small class="text-muted">Completed Jobs</small>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="text-center">
                        <div class="h4 text-info">${data.stats.successRate.toFixed(1)}%</div>
                        <small class="text-muted">Success Rate</small>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="text-center">
                        <div class="h4 text-warning">${data.stats.averageDuration.toFixed(1)}h</div>
                        <small class="text-muted">Avg Duration</small>
                    </div>
                </div>
            </div>
        `;
    }

    createDependencyDetailsHtml(data, isEdit) {
        const editAlert = isEdit ? 
            '<div class="alert alert-info"><i class="fas fa-info-circle me-2"></i>Edit functionality will be implemented in a future update.</div>' :
            '';

        return `
            ${editAlert}
            <div class="row mb-3">
                <div class="col-md-6">
                    <strong>Dependent Stage:</strong>
                    <p class="mb-0"><i class="fas fa-arrow-right me-2 text-primary"></i>${data.dependentStageName}</p>
                </div>
                <div class="col-md-6">
                    <strong>Prerequisite Stage:</strong>
                    <p class="mb-0"><i class="fas fa-flag-checkered me-2 text-success"></i>${data.prerequisiteStageName}</p>
                </div>
            </div>
            <div class="row mb-3">
                <div class="col-md-6">
                    <strong>Dependency Type:</strong>
                    <p class="mb-0"><span class="badge bg-primary">${data.dependencyType}</span></p>
                </div>
                <div class="col-md-6">
                    <strong>Delay:</strong>
                    <p class="mb-0">${data.delayHours > 0 ? data.delayHours + ' hours' : '<span class="text-muted">None</span>'}</p>
                </div>
            </div>
            <div class="mb-3">
                <strong>Dependency Status:</strong>
                <p class="mb-0"><span class="badge bg-${data.isOptional ? 'info' : 'warning'}">${data.isOptional ? 'Optional' : 'Required'}</span></p>
            </div>
            <div class="mb-3">
                <strong>Notes:</strong>
                <p class="mb-0">${data.notes || '<span class="text-muted">No notes provided</span>'}</p>
            </div>
        `;
    }

    createPoolDetailsHtml(data, isEdit) {
        const editAlert = isEdit ? 
            '<div class="alert alert-info"><i class="fas fa-info-circle me-2"></i>Edit functionality will be implemented in a future update.</div>' :
            '';

        return `
            ${editAlert}
            <div class="row mb-3">
                <div class="col-md-6">
                    <strong>Pool Name:</strong>
                    <p class="mb-0">${data.name}</p>
                </div>
                <div class="col-md-6">
                    <strong>Resource Type:</strong>
                    <p class="mb-0"><span class="badge bg-primary">${data.resourceType}</span></p>
                </div>
            </div>
            <div class="mb-3">
                <strong>Description:</strong>
                <p class="mb-0">${data.description || '<span class="text-muted">No description provided</span>'}</p>
            </div>
            <div class="row mb-3">
                <div class="col-md-4">
                    <div class="text-center">
                        <div class="h4 text-primary">${data.maxConcurrentAllocations}</div>
                        <small class="text-muted">Max Concurrent</small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        <div class="h4 text-${data.autoAssign ? 'success' : 'secondary'}">
                            <i class="fas fa-${data.autoAssign ? 'check-circle' : 'hand-paper'}"></i>
                        </div>
                        <small class="text-muted">${data.autoAssign ? 'Auto Assign' : 'Manual Assign'}</small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        <div class="h4 text-info">${data.utilizationPercentage.toFixed(1)}%</div>
                        <small class="text-muted">Utilization</small>
                    </div>
                </div>
            </div>
            <div class="mb-3">
                <strong>Utilization Progress:</strong>
                <div class="progress mt-2" style="height: 8px;">
                    <div class="progress-bar bg-info" style="width: ${data.utilizationPercentage}%"></div>
                </div>
            </div>
        `;
    }

    showConfirmDialog(title, message, confirmCallback) {
        const confirmed = confirm(`${title}\n\n${message}`);
        if (confirmed) {
            confirmCallback();
        }
    }

    submitDeleteForm(handler, paramName, paramValue) {
        // Create and submit form for delete operation
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `${window.location.pathname}?handler=${handler}`;
        
        // Add antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token;
            form.appendChild(tokenInput);
        }
        
        // Add parameter
        const paramInput = document.createElement('input');
        paramInput.type = 'hidden';
        paramInput.name = paramName;
        paramInput.value = paramValue;
        form.appendChild(paramInput);
        
        document.body.appendChild(form);
        form.submit();
    }

    // Integration with Parts Form
    integrateWithPartsForm() {
        // Check if we're on the parts form page
        const partForm = document.querySelector('#partForm');
        if (partForm) {
            console.log('?? Integrating with Parts Form...');
            this.enhancePartsFormStageManagement();
        }
    }

    enhancePartsFormStageManagement() {
        // Add template selection to parts form
        const stageContainer = document.querySelector('#stage-requirements-container');
        if (stageContainer) {
            this.addTemplateSelector(stageContainer);
        }
    }

    addTemplateSelector(container) {
        const templateSelector = document.createElement('div');
        templateSelector.className = 'template-selector mb-3';
        templateSelector.innerHTML = `
            <div class="d-flex justify-content-between align-items-center">
                <label class="form-label">Quick Start with Template:</label>
                <select class="form-select w-auto" id="templateQuickSelect">
                    <option value="">Select a template...</option>
                </select>
            </div>
        `;
        
        container.insertBefore(templateSelector, container.firstChild);
        
        // Load available templates
        this.loadTemplatesForQuickSelect();
    }

    loadTemplatesForQuickSelect() {
        // This would load templates from the server
        // Implementation depends on your API structure
        console.log('Loading templates for quick select...');
    }
}

// Global functions for backward compatibility
let stageManagement;

function viewTemplate(templateId) {
    stageManagement?.viewTemplate(templateId);
}

function editTemplate(templateId) {
    stageManagement?.editTemplate(templateId);
}

function deleteTemplate(templateId, templateName) {
    stageManagement?.deleteTemplate(templateId, templateName);
}

function editDependency(dependencyId) {
    stageManagement?.editDependency(dependencyId);
}

function deleteDependency(dependencyId) {
    stageManagement?.deleteDependency(dependencyId);
}

function viewPool(poolId) {
    stageManagement?.viewPool(poolId);
}

function editPool(poolId) {
    stageManagement?.editPool(poolId);
}

function deletePool(poolId, poolName) {
    stageManagement?.deletePool(poolId, poolName);
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    stageManagement = new StageManagement();
    console.log('? Global Stage Management functions initialized');
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = StageManagement;
}