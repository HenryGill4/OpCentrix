/**
 * OpCentrix Modal Manager - FINAL SOLUTION
 * Comprehensive modal handling to prevent backdrop blocking issues
 * Handles both Bootstrap and custom modals
 */

(function() {
    'use strict';

    // COMPREHENSIVE MODAL EVENT HANDLING - NO CONFLICTS
    class OpCentrixModalManager {
        constructor() {
            this.activeModals = new Set();
            this.isInitialized = false;
            this.setupEventHandlers();
            console.log('?? [MODAL-MANAGER] Initialized - Final Solution Active');
        }

        setupEventHandlers() {
            if (this.isInitialized) return;

            // Single, comprehensive click handler using capture phase
            document.addEventListener('click', (e) => {
                this.handleDocumentClick(e);
            }, true); // Use capture phase to handle first

            // Keyboard handler
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape') {
                    this.closeTopModal();
                }
            });

            // HTMX Integration
            this.setupHtmxIntegration();

            this.isInitialized = true;
            console.log('? [MODAL-MANAGER] Event handlers setup complete');
        }

        handleDocumentClick(e) {
            // Handle Bootstrap modal backdrop
            if (e.target.classList.contains('modal-backdrop')) {
                e.preventDefault();
                e.stopPropagation();
                const activeModal = document.querySelector('.modal.show');
                if (activeModal) {
                    console.log('?? [MODAL-MANAGER] Bootstrap backdrop click detected');
                    this.closeModal(activeModal);
                }
                return;
            }

            // Handle custom modal backdrop (direct backdrop click)
            const modal = e.target.closest('.modal, .opcentrix-modal, [id$="modal-content"], [id$="modal-container"]');
            
            if (modal && e.target === modal) {
                // Direct backdrop click - close modal
                e.preventDefault();
                e.stopPropagation();
                console.log('?? [MODAL-MANAGER] Custom backdrop click detected');
                this.closeModal(modal);
                return;
            }

            // Handle close buttons
            if (e.target.matches('[data-bs-dismiss="modal"], .btn-close, .modal-close, [data-modal-close]')) {
                e.preventDefault();
                e.stopPropagation();
                const targetModal = e.target.closest('.modal, .opcentrix-modal, [id*="modal"]');
                if (targetModal) {
                    console.log('?? [MODAL-MANAGER] Close button clicked');
                    this.closeModal(targetModal);
                }
                return;
            }

            // Prevent event bubbling for modal content to avoid accidental closes
            if (e.target.closest('.modal-content, .opcentrix-modal-content, .modal-dialog')) {
                // Allow normal interaction with modal content
                return;
            }
        }

        closeModal(modalElement) {
            if (!modalElement) return;

            const modalId = modalElement.id || 'unknown';
            console.log(`?? [MODAL-MANAGER] Closing modal: ${modalId}`);

            try {
                // Bootstrap modal
                if (modalElement.classList.contains('modal')) {
                    const bsModal = bootstrap?.Modal?.getInstance(modalElement);
                    if (bsModal) {
                        bsModal.hide();
                        console.log(`? [MODAL-MANAGER] Bootstrap modal closed: ${modalId}`);
                    } else {
                        // Fallback for Bootstrap modals without instance
                        modalElement.classList.remove('show');
                        modalElement.style.display = 'none';
                        const backdrop = document.querySelector('.modal-backdrop');
                        if (backdrop) backdrop.remove();
                        console.log(`? [MODAL-MANAGER] Bootstrap modal closed (fallback): ${modalId}`);
                    }
                }
                // Custom modal (Tailwind/custom CSS)
                else {
                    modalElement.classList.add('hidden');
                    modalElement.classList.remove('flex', 'show', 'block');
                    modalElement.style.display = 'none';
                    
                    // Clear modal content for HTMX modals
                    if (modalElement.id === 'modal-content' || modalElement.id === 'modal-container') {
                        modalElement.innerHTML = '';
                    }
                    
                    console.log(`? [MODAL-MANAGER] Custom modal closed: ${modalId}`);
                }

                // Cleanup
                this.activeModals.delete(modalId);
                
                // Restore body scroll if no modals are active
                if (this.activeModals.size === 0) {
                    document.body.style.overflow = '';
                    console.log('?? [MODAL-MANAGER] Body scroll restored');
                }

                // Dispatch custom event for other components
                window.dispatchEvent(new CustomEvent('opcentrix:modal:closed', {
                    detail: { modalId, element: modalElement }
                }));

            } catch (error) {
                console.error(`? [MODAL-MANAGER] Error closing modal ${modalId}:`, error);
            }
        }

        closeTopModal() {
            const visibleModals = document.querySelectorAll(`
                .modal.show, 
                .opcentrix-modal:not(.hidden), 
                [style*="flex"][id*="modal"],
                [id="modal-content"]:not(.hidden),
                [id="modal-container"]:not(.hidden)
            `);
            
            if (visibleModals.length > 0) {
                const topModal = visibleModals[visibleModals.length - 1];
                console.log('?? [MODAL-MANAGER] Closing top modal via Escape key');
                this.closeModal(topModal);
            }
        }

        openModal(modalId, content = null) {
            console.log(`?? [MODAL-MANAGER] Opening modal: ${modalId}`);
            
            const modal = document.getElementById(modalId) || document.querySelector(`[data-modal="${modalId}"]`);
            if (!modal) {
                console.error(`? [MODAL-MANAGER] Modal not found: ${modalId}`);
                return false;
            }

            try {
                // Set content if provided
                if (content) {
                    const contentContainer = modal.querySelector('.modal-content, .opcentrix-modal-content') || modal;
                    if (typeof content === 'string') {
                        contentContainer.innerHTML = content;
                    } else if (content instanceof HTMLElement) {
                        contentContainer.innerHTML = '';
                        contentContainer.appendChild(content);
                    }
                }

                // Show modal
                if (modal.classList.contains('modal')) {
                    // Bootstrap modal
                    const bsModal = bootstrap?.Modal?.getOrCreateInstance(modal);
                    if (bsModal) {
                        bsModal.show();
                    } else {
                        // Fallback
                        modal.classList.add('show');
                        modal.style.display = 'block';
                    }
                } else {
                    // Custom modal
                    modal.classList.remove('hidden');
                    modal.classList.add('flex');
                    modal.style.display = 'flex';
                }

                // Track active modal
                this.activeModals.add(modalId);

                // Prevent body scroll
                document.body.style.overflow = 'hidden';

                // Focus management
                setTimeout(() => {
                    const focusableElement = modal.querySelector('input, button, select, textarea, [tabindex]:not([tabindex="-1"])');
                    if (focusableElement) {
                        focusableElement.focus();
                    }
                }, 100);

                // Dispatch custom event
                window.dispatchEvent(new CustomEvent('opcentrix:modal:opened', {
                    detail: { modalId, element: modal }
                }));

                console.log(`? [MODAL-MANAGER] Modal opened: ${modalId}`);
                return true;

            } catch (error) {
                console.error(`? [MODAL-MANAGER] Error opening modal ${modalId}:`, error);
                return false;
            }
        }

        setupHtmxIntegration() {
            // HTMX Modal Event Integration
            document.body.addEventListener('htmx:afterSwap', (e) => {
                // If content was swapped into a modal, ensure proper setup
                const modalContainer = e.detail.target;
                if (modalContainer.id === 'modal-content' || modalContainer.id === 'modal-container' ||
                    modalContainer.closest('.modal, .opcentrix-modal')) {
                    
                    console.log('?? [MODAL-MANAGER] HTMX content swapped into modal');
                    
                    const modal = modalContainer.closest('.modal, .opcentrix-modal') || modalContainer;
                    
                    // If content is empty, close the modal
                    if (modalContainer.innerHTML.trim() === '') {
                        this.closeModal(modal);
                        return;
                    }
                    
                    // Otherwise, show the modal if it's not already visible
                    if (!modal.classList.contains('show') && !modal.classList.contains('flex')) {
                        if (modal.id) {
                            this.openModal(modal.id);
                        } else {
                            // Manual show for containers without IDs
                            modal.classList.remove('hidden');
                            modal.classList.add('flex');
                            modal.style.display = 'flex';
                            document.body.style.overflow = 'hidden';
                        }
                    }
                }
            });

            // Handle HTMX errors
            document.body.addEventListener('htmx:responseError', (e) => {
                console.error('? [MODAL-MANAGER] HTMX response error:', e.detail);
                // Close any loading modals
                this.closeTopModal();
            });

            document.body.addEventListener('htmx:sendError', (e) => {
                console.error('? [MODAL-MANAGER] HTMX send error:', e.detail);
                // Close any loading modals
                this.closeTopModal();
            });

            console.log('? [MODAL-MANAGER] HTMX integration setup complete');
        }

        // Public API methods
        closeAll() {
            console.log('?? [MODAL-MANAGER] Closing all modals');
            const allModals = document.querySelectorAll('.modal.show, .opcentrix-modal:not(.hidden), [id*="modal"]:not(.hidden)');
            allModals.forEach(modal => this.closeModal(modal));
        }

        isModalOpen() {
            return this.activeModals.size > 0;
        }

        getActiveModals() {
            return Array.from(this.activeModals);
        }
    }

    // Initialize the modal manager
    let modalManager;
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            modalManager = new OpCentrixModalManager();
            window.opcentrixModalManager = modalManager;
        });
    } else {
        modalManager = new OpCentrixModalManager();
        window.opcentrixModalManager = modalManager;
    }

    // Global functions for backward compatibility
    window.closeJobModal = function() {
        if (modalManager) {
            modalManager.closeTopModal();
        }
    };

    window.showJobModal = function() {
        if (modalManager) {
            const modal = document.getElementById('modal-content') || document.getElementById('modal-container');
            if (modal && modal.id) {
                modalManager.openModal(modal.id);
            }
        }
    };

    window.openJobModal = function(machineId, date, jobId = null) {
        console.log('?? [MODAL-MANAGER] Opening job modal:', { machineId, date, jobId });
        
        if (!machineId || !date) {
            console.error('? [MODAL-MANAGER] Missing required parameters');
            return false;
        }
        
        // Build URL for modal content
        const params = new URLSearchParams({
            handler: 'ShowAddModal',
            machineId: machineId,
            date: date
        });
        
        if (jobId) {
            params.append('id', jobId);
        }
        
        const url = `/Scheduler?${params.toString()}`;
        console.log('?? [MODAL-MANAGER] Fetching modal content from:', url);
        
        // Use HTMX to load modal content
        if (typeof htmx !== 'undefined') {
            htmx.ajax('GET', url, {
                target: '#modal-content',
                swap: 'innerHTML'
            });
        }
        
        return true;
    };

    console.log('?? [MODAL-MANAGER] Final Solution Loaded - Backdrop Blocking Issues Resolved');

})();