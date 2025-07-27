/**
 * OpCentrix Machine Management Module
 * Modern machine management using the new framework architecture
 */

OpCentrix.module('MachineManagement', async (UI) => {
    'use strict';

    const { Utils, EventManager } = OpCentrix;

    // ==================================================
    // MACHINE MANAGEMENT COMPONENT
    // ==================================================

    class MachineManagementComponent {
        constructor() {
            this.setupEventHandlers();
        }

        /**
         * Setup event handlers for machine management
         */
        setupEventHandlers() {
            // Add new machine button
            EventManager.delegate(document, '[data-machine-add]', 'click', async (e, button) => {
                e.preventDefault();
                await this.showAddModal();
            });

            // Edit machine buttons
            EventManager.delegate(document, '[data-machine-edit]', 'click', async (e, button) => {
                e.preventDefault();
                const machineId = button.dataset.machineEdit;
                await this.showEditModal(machineId);
            });

            // Delete machine buttons
            EventManager.delegate(document, '[data-machine-delete]', 'click', async (e, button) => {
                e.preventDefault();
                const machineId = button.dataset.machineDelete;
                const machineName = button.dataset.machineName;
                await this.confirmDelete(machineId, machineName);
            });

            // Material checkbox updates
            EventManager.delegate(document, '[data-machine-materials] input[type="checkbox"]', 'change', async (e, checkbox) => {
                await this.updateSupportedMaterials();
            });
        }

        /**
         * Show add machine modal
         */
        async showAddModal() {
            const operationId = Utils.generateId();
            console.log(`? [MACHINES] ${operationId} Opening add machine modal`);

            try {
                await UI.loadModal('modal-container', '/Admin/Machines?handler=Add');
                console.log(`? [MACHINES] ${operationId} Add modal loaded`);
            } catch (error) {
                console.error(`? [MACHINES] ${operationId} Failed to load add modal:`, error);
                UI.error('Failed to load add machine form. Please try again.');
            }
        }

        /**
         * Show edit machine modal
         */
        async showEditModal(machineId) {
            const operationId = Utils.generateId();
            console.log(`?? [MACHINES] ${operationId} Opening edit machine modal for ID: ${machineId}`);

            try {
                if (!machineId || machineId <= 0) {
                    throw new Error(`Invalid machine ID: ${machineId}`);
                }

                await UI.loadModal('modal-container', `/Admin/Machines?handler=Edit&id=${machineId}`);
                console.log(`? [MACHINES] ${operationId} Edit modal loaded`);
            } catch (error) {
                console.error(`? [MACHINES] ${operationId} Failed to load edit modal:`, error);
                UI.error('Failed to load edit machine form. Please try again.');
            }
        }

        /**
         * Confirm and delete machine
         */
        async confirmDelete(machineId, machineName) {
            const operationId = Utils.generateId();
            console.log(`??? [MACHINES] ${operationId} Delete request for machine: ${machineName} (ID: ${machineId})`);

            const confirmed = confirm(`Are you sure you want to delete machine "${machineName}"? This action cannot be undone.`);
            
            if (confirmed) {
                try {
                    const formData = new FormData();
                    const token = Utils.getElement('input[name="__RequestVerificationToken"]');
                    if (token) {
                        formData.append('__RequestVerificationToken', token.value);
                    }

                    const response = await fetch(`/Admin/Machines?handler=Delete&id=${machineId}`, {
                        method: 'POST',
                        body: formData
                    });

                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }

                    UI.success(`Machine "${machineName}" deleted successfully`);
                    
                    // Refresh page after delay
                    setTimeout(() => window.location.reload(), 1500);

                    console.log(`? [MACHINES] ${operationId} Machine deleted successfully`);

                } catch (error) {
                    console.error(`? [MACHINES] ${operationId} Failed to delete machine:`, error);
                    UI.error(`Failed to delete machine "${machineName}". Please try again.`);
                }
            } else {
                console.log(`? [MACHINES] ${operationId} User cancelled delete`);
            }
        }

        /**
         * Update supported materials display
         */
        async updateSupportedMaterials() {
            const operationId = Utils.generateId();
            console.log(`?? [MACHINES] ${operationId} Updating supported materials`);

            try {
                const checkboxes = Utils.getElements('[data-machine-materials] input[type="checkbox"]');
                const materialTextarea = Utils.getElement('[data-machine-supported-materials]');

                if (!materialTextarea) {
                    console.warn(`?? [MACHINES] ${operationId} Supported materials textarea not found`);
                    return false;
                }

                const selectedMaterials = [];
                checkboxes.forEach(checkbox => {
                    if (checkbox.checked) {
                        selectedMaterials.push(checkbox.value);
                    }
                });

                materialTextarea.value = selectedMaterials.join(', ');
                console.log(`? [MACHINES] ${operationId} Updated supported materials: ${selectedMaterials.length} selected`);
                return true;

            } catch (error) {
                console.error(`? [MACHINES] ${operationId} Error updating supported materials:`, error);
                return false;
            }
        }
    }

    // ==================================================
    // INITIALIZE COMPONENT
    // ==================================================

    const machineManagement = new MachineManagementComponent();

    // ==================================================
    // RETURN PUBLIC API
    // ==================================================

    return {
        // Convenience methods for external use
        showAddModal: machineManagement.showAddModal.bind(machineManagement),
        showEditModal: machineManagement.showEditModal.bind(machineManagement),
        updateSupportedMaterials: machineManagement.updateSupportedMaterials.bind(machineManagement)
    };

}, ['UI']);

console.log('? [MACHINES] OpCentrix Machine Management module loaded');