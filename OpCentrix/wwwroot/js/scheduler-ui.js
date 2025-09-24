// OpCentrix Scheduler UI - Modern, Efficient Implementation
// Handles zoom controls, grid interactions, and UI state management
// Optimized for performance and maintainability

// --- EARLY GLOBAL SAFEGUARDS & WRAPPERS (must load before any inline onclick calls) ---
if (!window.safeExecute) {
    window.safeExecute = function(source, action, executor, meta) {
        try {
            return executor();
        } catch (err) {
            console.error(`[${source}] ${action} failed`, { meta, err });
            if (window.SafeExecute && typeof window.SafeExecute.logError === 'function') {
                window.SafeExecute.logError(
                    Math.random().toString(36).slice(2, 8),
                    action,
                    err
                );
            }
            return false;
        }
    };
}

// Unified safe modal opener used by Razor inline onclick attributes
if (!window.openJobModalSafely) {
    window.openJobModalSafely = function(machineId, date, jobId = null) {
        // Prefer primary implementation if already defined
        if (typeof window.openJobModal === 'function') {
            return window.openJobModal(machineId, date, jobId);
        }

        console.log('?? [JOB-MODAL] Fallback openJobModalSafely executing', { machineId, date, jobId });
        if (!machineId || !date) {
            console.error('? [JOB-MODAL] Missing machineId or date');
            return false;
        }

        try {
            const params = new URLSearchParams({ handler: 'ShowAddModal', machineId, date });
            if (jobId) params.append('id', jobId);
            const url = `/Scheduler?${params.toString()}`;

            if (typeof htmx !== 'undefined') {
                htmx.ajax('GET', url, { target: '#modal-container', swap: 'innerHTML' })
                    .then(() => {
                        const modalContainer = document.getElementById('modal-container');
                        if (modalContainer) {
                            modalContainer.style.display = 'flex';
                            modalContainer.classList.remove('hidden');
                            document.body.style.overflow = 'hidden';
                        }
                    })
                    .catch(e => console.error('? [JOB-MODAL] HTMX fallback error', e));
                return true;
            }
            // Hard fallback: navigate
            window.location.href = url;
            return true;
        } catch (e) {
            console.error('? [JOB-MODAL] Critical fallback error', e);
            return false;
        }
    };
}

// **ENHANCED GLOBAL FUNCTION DEFINITIONS**
// These functions need to be available immediately when HTML loads
// They provide fallback functionality when specific forms don't override them

window.updateSlsMaterial = function() {
    console.log('?? [GLOBAL] updateSlsMaterial called (fallback)');
    const materialSelect = document.getElementById('materialSelect');
    const slsMaterialInput = document.getElementById('slsMaterialInput');
    
    if (materialSelect && slsMaterialInput) {
        try {
            if (materialSelect.value && materialSelect.value !== '') {
                slsMaterialInput.value = materialSelect.value;
                console.log('? [GLOBAL] SLS Material updated:', materialSelect.value);
            } else {
                slsMaterialInput.value = '';
                console.log('?? [GLOBAL] SLS Material cleared');
            }
            return true;
        } catch (error) {
            console.error('? [GLOBAL] Error updating SLS material:', error);
            return false;
        }
    } else {
        console.warn('?? [GLOBAL] Material select elements not found');
        return false;
    }
};

window.updateDurationDisplay = function() {
    console.log('?? [GLOBAL] updateDurationDisplay called (fallback)');
    const hoursInput = document.querySelector('input[name="EstimatedHours"]');
    const durationDisplay = document.getElementById('durationDisplay');
    const durationDays = document.getElementById('durationDays');
    
    if (!hoursInput || !durationDisplay || !durationDays) {
        console.warn('?? [GLOBAL] Duration display elements not found');
        return false;
    }
    
    try {
        const hours = parseFloat(hoursInput.value) || 0;
        
        if (hours > 0) {
            const days = Math.floor(hours / 24);
            const remainingHours = hours % 24;
            
            // Update duration display
            if (days > 0) {
                durationDisplay.value = `${days}d ${remainingHours.toFixed(1)}h`;
            } else {
                durationDisplay.value = `${hours.toFixed(1)}h`;
            }
            
            // Update duration days (work days, assuming 8-hour days)
            durationDays.value = Math.ceil(hours / 8);
            console.log('? [GLOBAL] Duration updated:', durationDisplay.value);
        } else {
            durationDisplay.value = '';
            durationDays.value = '';
            console.log('?? [GLOBAL] Duration cleared');
        }
        return true;
    } catch (error) {
        console.error('? [GLOBAL] Error updating duration:', error);
        return false;
    }
};

// Additional global functions for form handling
window.showFormLoading = function() {
    console.log('?? [GLOBAL] showFormLoading called (fallback)');
    const submitBtn = document.getElementById('submitBtn') || 
                     document.getElementById('submit-job-btn') ||
                     document.querySelector('button[type="submit"]');
    const submitText = document.getElementById('submitText') || 
                      document.getElementById('submit-text');
    const submitSpinner = document.getElementById('submitSpinner') || 
                         document.getElementById('submit-spinner');
    
    if (submitBtn) {
        submitBtn.disabled = true;
        console.log('?? [GLOBAL] Submit button disabled');
    }
    if (submitText) {
        submitText.textContent = 'Saving...';
        console.log('?? [GLOBAL] Submit text updated');
    }
    if (submitSpinner) {
        submitSpinner.classList.remove('hidden');
        console.log('?? [GLOBAL] Spinner shown');
    }
};

window.handleFormResponse = function(event) {
    console.log('?? [GLOBAL] handleFormResponse called');
    if (event && event.detail) {
        console.log('?? [GLOBAL] Response status:', event.detail.xhr?.status);
    }
    
    // Reset loading state
    const submitBtn = document.getElementById('submitBtn') || document.getElementById('submit-job-btn');
    const submitText = document.getElementById('submitText') || document.getElementById('submit-text');
    const submitSpinner = document.getElementById('submitSpinner') || document.getElementById('submit-spinner');
    
    if (submitBtn) {
        submitBtn.disabled = false;
        console.log('?? [GLOBAL] Submit button re-enabled');
    }
    if (submitSpinner) {
        submitSpinner.classList.add('hidden');
        console.log('?? [GLOBAL] Spinner hidden');
    }
};

class OpCentrixSchedulerUI {
    constructor() {
        // FIXED: Updated zoom levels to match main Index page
        this.zoomLevels = ['2month', 'month', 'week', '12h', '6h', '4h', '2h', '1h', '30min', '15min'];
        this.currentZoomIndex = 2; // Default to 'week'
        this.isInitialized = false;
        this.gridCellCache = new Map();
        this.resizeObserver = null;
        
        this.init();
    }

    init() {
        if (this.isInitialized) return;
        
        try {
            this.detectCurrentZoom();
            this.setupEventListeners();
            this.setupZoomControls();
            this.setupGridInteractions();
            this.updateGridStyles();
            this.setupPerformanceOptimizations();
            
            this.isInitialized = true;
            this.logInfo('OpCentrix Scheduler UI initialized successfully');
        } catch (error) {
            console.error('Failed to initialize Scheduler UI:', error);
        }
    }

    detectCurrentZoom() {
        const urlParams = new URLSearchParams(window.location.search);
        const currentZoom = urlParams.get('zoom') || 'week';
        this.currentZoomIndex = this.zoomLevels.indexOf(currentZoom);
        if (this.currentZoomIndex === -1) {
            this.currentZoomIndex = 2; // Default to 'week'
        }
        console.log(`?? [ZOOM] Current zoom detected: ${currentZoom} (index: ${this.currentZoomIndex})`);
    }

    setupEventListeners() {
        // Debounced resize handler for performance
        this.setupResizeHandler();
        
        // HTMX integration
        this.setupHtmxListeners();
        
        // Keyboard shortcuts
        this.setupKeyboardShortcuts();
        
        // Visibility change handler
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden) {
                this.refreshGridInteractions();
            }
        });
    }

    setupResizeHandler() {
        let resizeTimeout;
        const debouncedResize = () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                this.handleResize();
            }, 250);
        };

        window.addEventListener('resize', debouncedResize);
        
        // Modern ResizeObserver for better performance
        if (window.ResizeObserver) {
            this.resizeObserver = new ResizeObserver(entries => {
                for (let entry of entries) {
                    if (entry.target.classList.contains('scheduler-main-grid')) {
                        this.handleGridResize(entry);
                    }
                }
            });
            
            const grid = document.querySelector('.scheduler-main-grid');
            if (grid) {
                this.resizeObserver.observe(grid);
            }
        }
    }

    setupHtmxListeners() {
        document.body.addEventListener('htmx:afterSwap', (event) => {
            // Re-initialize interactions after HTMX updates
            if (event.detail.target.id === 'scheduler-main-content' || 
                event.detail.target.closest('#scheduler-main-content')) {
                
                setTimeout(() => {
                    this.refreshGridInteractions();
                    this.updateGridStyles();
                }, 50);
            }
        });

        document.body.addEventListener('htmx:beforeRequest', () => {
            this.clearGridCache();
        });
    }

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (event) => {
            // Only handle shortcuts when not in input fields
            if (event.target.tagName === 'INPUT' || 
                event.target.tagName === 'TEXTAREA' || 
                event.target.tagName === 'SELECT') {
                return;
            }

            switch (event.key) {
                case '=':
                case '+':
                    if (event.ctrlKey || event.metaKey) {
                        event.preventDefault();
                        this.zoomIn();
                    }
                    break;
                case '-':
                    if (event.ctrlKey || event.metaKey) {
                        event.preventDefault();
                        this.zoomOut();
                    }
                    break;
                case 'r':
                    if (event.ctrlKey || event.metaKey) {
                        event.preventDefault();
                        this.refreshScheduler();
                    }
                    break;
            }
        });
    }

    setupZoomControls() {
        const zoomInBtn = document.getElementById('zoomIn');
        const zoomOutBtn = document.getElementById('zoomOut');
        
        if (zoomInBtn && !zoomInBtn.disabled) {
            // Remove existing listeners to prevent duplicates
            zoomInBtn.removeEventListener('click', this.zoomInHandler);
            this.zoomInHandler = () => this.zoomIn();
            zoomInBtn.addEventListener('click', this.zoomInHandler);
        }
        
        if (zoomOutBtn && !zoomOutBtn.disabled) {
            zoomOutBtn.removeEventListener('click', this.zoomOutHandler);
            this.zoomOutHandler = () => this.zoomOut();
            zoomOutBtn.addEventListener('click', this.zoomOutHandler);
        }
    }

    setupGridInteractions() {
        this.clearGridCache();
        
        // Use event delegation for better performance
        const schedulerGrid = document.querySelector('.scheduler-main-grid');
        if (!schedulerGrid) return;

        // Remove existing listeners
        schedulerGrid.removeEventListener('mouseover', this.gridMouseOverHandler);
        schedulerGrid.removeEventListener('mouseout', this.gridMouseOutHandler);
        schedulerGrid.removeEventListener('click', this.gridClickHandler);

        // Setup new listeners with proper binding
        this.gridMouseOverHandler = (e) => this.handleGridMouseOver(e);
        this.gridMouseOutHandler = (e) => this.handleGridMouseOut(e);
        this.gridClickHandler = (e) => this.handleGridClick(e);

        schedulerGrid.addEventListener('mouseover', this.gridMouseOverHandler);
        schedulerGrid.addEventListener('mouseout', this.gridMouseOutHandler);
        schedulerGrid.addEventListener('click', this.gridClickHandler);
    }

    setupPerformanceOptimizations() {
        // Intersection Observer for visibility-based optimizations
        if (window.IntersectionObserver) {
            this.setupIntersectionObserver();
        }
        
        // Preload critical resources
        this.preloadCriticalResources();
    }

    setupIntersectionObserver() {
        const observerOptions = {
            root: null,
            rootMargin: '50px',
            threshold: 0.1
        };

        this.intersectionObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.target.classList.contains('machine-row')) {
                    this.handleMachineRowVisibility(entry);
                }
            });
        }, observerOptions);

        // Observe all machine rows
        document.querySelectorAll('.machine-row').forEach(row => {
            this.intersectionObserver.observe(row);
        });
    }

    handleMachineRowVisibility(entry) {
        const row = entry.target;
        if (entry.isIntersecting) {
            // Row is visible - enable interactions
            row.classList.add('visible');
        } else {
            // Row is not visible - disable heavy interactions
            row.classList.remove('visible');
        }
    }

    preloadCriticalResources() {
        // Preload modal CSS if not already loaded
        const modalCSS = document.querySelector('link[href*="scheduler-modal.css"]');
        if (modalCSS) {
            modalCSS.rel = 'preload';
            modalCSS.as = 'style';
        }
    }

    handleGridMouseOver(event) {
        const cell = event.target.closest('.scheduler-grid-cell');
        if (!cell || cell.classList.contains('highlighted')) return;

        // Cache check for performance
        const cellId = this.getCellId(cell);
        if (this.gridCellCache.has(cellId)) return;

        this.highlightCell(cell);
        this.gridCellCache.set(cellId, true);
    }

    handleGridMouseOut(event) {
        const cell = event.target.closest('.scheduler-grid-cell');
        if (!cell) return;

        this.unhighlightCell(cell);
        
        const cellId = this.getCellId(cell);
        this.gridCellCache.delete(cellId);
    }

    handleGridClick(event) {
        const cell = event.target.closest('.scheduler-grid-cell');
        if (!cell) return;

        // Get machine and time information
        const machineId = cell.getAttribute('data-machine') || 
                         cell.closest('.machine-row')?.getAttribute('data-machine');
        const slotTime = cell.getAttribute('data-slot-time');
        
        if (machineId && slotTime && (window.openJobModal || window.openJobModalSafely)) {
            (window.openJobModal || window.openJobModalSafely)(machineId, slotTime);
        }
    }

    getCellId(cell) {
        const machine = cell.getAttribute('data-machine') || 
                       cell.closest('.machine-row')?.getAttribute('data-machine');
        const slot = cell.getAttribute('data-slot-time');
        return `${machine}-${slot}`;
    }

    highlightCell(cell) {
        if (!cell.classList.contains('current-time') && 
            !cell.classList.contains('weekend')) {
            cell.classList.add('highlighted');
            cell.style.backgroundColor = '#f0f9ff';
        }
    }

    unhighlightCell(cell) {
        cell.classList.remove('highlighted');
        if (!cell.classList.contains('current-time') && 
            !cell.classList.contains('weekend') &&
            !cell.classList.contains('off-hours')) {
            cell.style.backgroundColor = '';
        }
    }

    zoomIn() {
        if (this.currentZoomIndex < this.zoomLevels.length - 1) {
            this.changeZoom(this.currentZoomIndex + 1);
        }
    }

    zoomOut() {
        if (this.currentZoomIndex > 0) {
            this.changeZoom(this.currentZoomIndex - 1);
        }
    }

    changeZoom(newZoomIndex) {
        if (newZoomIndex < 0 || newZoomIndex >= this.zoomLevels.length) {
            return;
        }

        const newZoom = this.zoomLevels[newZoomIndex];
        const url = new URL(window.location);
        url.searchParams.set('zoom', newZoom);
        
        // Show loading indicator if available
        if (window.showLoadingIndicator) {
            window.showLoadingIndicator(`Switching to ${newZoom} view...`);
        }
        
        window.location.href = url.toString();
    }

    updateGridStyles() {
        const currentZoom = this.zoomLevels[this.currentZoomIndex];
        const root = document.documentElement;
        
        // PROFESSIONAL: Enhanced slot widths for better readability
        // ENHANCED: Professional slot widths optimized for extended time spans
        const slotWidths = {
            'day': '120px',       // INCREASED: Maximum readability for 30-day span
            'hour': '70px',       // INCREASED: Comfortable for 336 slots
            '30min': '60px',      // INCREASED: Better for high-density view
            '15min': '50px',      // INCREASED: Still very usable for maximum detail
            '12h': '150px',       // INCREASED: Very wide for excellent time labels
            '6h': '120px',        // INCREASED: Great balance for 120 slots
            '4h': '100px',        // INCREASED: Comfortable for 180 slots
            '2h': '80px',         // INCREASED: Good for 252 slots
            '1h': '70px',         // INCREASED: Comfortable for 336 slots
            'week': '100px',      // INCREASED: More generous size for 42-day span
            'month': '80px',      // INCREASED: Enhanced readability for 30-day span
            '2month': '60px'      // INCREASED: Better horizontal scrolling for 60-day span
        };
        
        const machineWidths = {
            'day': '200px',
            'hour': '200px',
            '30min': '200px',
            '15min': '200px',
            '12h': '200px',
            '6h': '200px',
            '4h': '200px',
            '2h': '200px',
            '1h': '200px',
            'week': '200px',
            'month': '200px',
            '2month': '200px'
        };

        root.style.setProperty('--slot-width', slotWidths[currentZoom] || '120px');
        root.style.setProperty('--machine-label-width', machineWidths[currentZoom] || '200px');
        
        // Update grid container classes for responsive behavior
        const gridContainer = document.querySelector('.scheduler-main-grid');
        if (gridContainer) {
            gridContainer.setAttribute('data-zoom', currentZoom);
            
            // Trigger reflow for better positioning
            requestAnimationFrame(() => {
                gridContainer.style.minWidth = 'fit-content';
            });
        }
        
        // ENHANCED: Update zoom-specific CSS variables
        switch(currentZoom) {
            case '12h':
                root.style.setProperty('--slot-width', 'var(--slot-width-12h)');
                break;
            case '6h':
                root.style.setProperty('--slot-width', 'var(--slot-width-6h)');
                break;
            case '4h':
                root.style.setProperty('--slot-width', 'var(--slot-width-4h)');
                break;
            case '2h':
                root.style.setProperty('--slot-width', 'var(--slot-width-2h)');
                break;
            case '1h':
                root.style.setProperty('--slot-width', 'var(--slot-width-1h)');
                break;
            case '30min':
                root.style.setProperty('--slot-width', 'var(--slot-width-30min)');
                break;
            case '15min':
                root.style.setProperty('--slot-width', 'var(--slot-width-15min)');
                break;
            case 'week':
                root.style.setProperty('--slot-width', 'var(--slot-width-week)');
                break;
            case 'month':
                root.style.setProperty('--slot-width', 'var(--slot-width-month)');
                break;
            case '2month':
                root.style.setProperty('--slot-width', 'var(--slot-width-2month)');
                break;
            default:
                root.style.setProperty('--slot-width', slotWidths[currentZoom] || '120px');
                break;
        }
        
        // Update grid styles based on zoom level
        console.log(`?? [ZOOM] Grid styles updated for ${currentZoom} zoom`);
    }

    handleResize() {
        // Recalculate grid dimensions
        this.updateGridStyles();
        
        // Clear cache to force re-evaluation
        this.clearGridCache();
        
        // Update intersection observer if needed
        if (this.intersectionObserver) {
            this.intersectionObserver.disconnect();
            this.setupIntersectionObserver();
        }
    }

    handleGridResize(entry) {
        // Handle specific grid resize logic
        const grid = entry.target;
        const newWidth = entry.contentRect.width;
        
        // Update grid layout if needed
        if (newWidth < 1200) {
            grid.classList.add('compact-mode');
        } else {
            grid.classList.remove('compact-mode');
        }
    }

    refreshGridInteractions() {
        // Re-setup grid interactions after DOM changes
        this.setupGridInteractions();
        
        // Re-setup zoom controls in case they were replaced
        this.setupZoomControls();
        
        // Update styles
        this.updateGridStyles();
    }

    refreshScheduler() {
        if (window.showLoadingIndicator) {
            window.showLoadingIndicator('Refreshing scheduler...');
        }
        
        // Delay to show loading indicator
        setTimeout(() => {
            window.location.reload();
        }, 100);
    }

    clearGridCache() {
        this.gridCellCache.clear();
    }

    showNotification(message, type = 'info') {
        // Use global notification system if available
        if (window.showSuccessNotification && type === 'success') {
            window.showSuccessNotification(message);
            return;
        }
        
        if (window.showErrorNotification && type === 'error') {
            window.showErrorNotification(message);
            return;
        }
        
        // Fallback notification system
        this.createFallbackNotification(message, type);
    }

    createFallbackNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type} fixed top-20 right-4 px-6 py-4 rounded-lg shadow-lg z-50 text-white font-medium max-w-md transform transition-all duration-300 translate-x-full`;
        
        const colors = {
            success: '#10b981',
            error: '#ef4444',
            warning: '#f59e0b',
            info: '#3b82f6'
        };
        
        notification.style.backgroundColor = colors[type] || colors.info;
        notification.textContent = message;
        
        document.body.appendChild(notification);
        
        // Animate in
        requestAnimationFrame(() => {
            notification.classList.remove('translate-x-full');
        });
        
        // Auto remove
        setTimeout(() => {
            notification.classList.add('translate-x-full', 'opacity-0');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        }, type === 'error' ? 8000 : 5000);
    }

    logInfo(message) {
        if (typeof console !== 'undefined' && console.log) {
            console.log(`[OpCentrix Scheduler] ${message}`);
        }
    }

    // Cleanup method for proper disposal
    destroy() {
        // Remove event listeners
        if (this.zoomInHandler) {
            const zoomInBtn = document.getElementById('zoomIn');
            if (zoomInBtn) zoomInBtn.removeEventListener('click', this.zoomInHandler);
        }
        
        if (this.zoomOutHandler) {
            const zoomOutBtn = document.getElementById('zoomOut');
            if (zoomOutBtn) zoomOutBtn.removeEventListener('click', this.zoomOutHandler);
        }
        
        // Disconnect observers
        if (this.resizeObserver) {
            this.resizeObserver.disconnect();
        }
        
        if (this.intersectionObserver) {
            this.intersectionObserver.disconnect();
        }
        
        // Clear cache
        this.clearGridCache();
        
        this.isInitialized = false;
    }
}

// Legacy function support for backward compatibility
window.initSchedulerZoom = function(zoomInId, zoomOutId, gridId) {
    console.warn('[OpCentrix Scheduler] Using legacy zoom initialization. Consider updating to new OpCentrixSchedulerUI class.');
    
    // Fallback to ensure basic functionality
    if (window.opcentrixSchedulerUI) {
        window.opcentrixSchedulerUI.setupZoomControls();
    }
};

// Initialize when DOM is ready
function initializeOpCentrixScheduler() {
    if (window.opcentrixSchedulerUI) {
        window.opcentrixSchedulerUI.destroy();
    }
    
    window.opcentrixSchedulerUI = new OpCentrixSchedulerUI();
}

// Multiple initialization strategies for reliability
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeOpCentrixScheduler);
} else {
    initializeOpCentrixScheduler();
}

// Re-initialize after HTMX updates
document.addEventListener('htmx:afterSwap', (event) => {
    if (event.detail.target.id === 'scheduler-main-content' || 
        event.detail.target.closest('#scheduler-main-content')) {
        
        setTimeout(() => {
            if (window.opcentrixSchedulerUI) {
                window.opcentrixSchedulerUI.refreshGridInteractions();
            }
        }, 100);
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OpCentrixSchedulerUI;
}

// **MODAL MANAGEMENT FUNCTIONS (REFRESHED - no safeExecute dependency)**
window.openJobModal = function(machineId, date, jobId = null) {
    try {
        console.log('?? [MODAL] Opening job modal for:', { machineId, date, jobId });
        if (!machineId || !date) throw new Error('Machine ID and date are required for opening job modal');

        const params = new URLSearchParams({ machineId, date });
        if (jobId) params.append('id', jobId);
        const url = `/Scheduler?handler=ShowAddModal&${params.toString()}`;
        console.log('?? [MODAL] Fetching modal content from:', url);

        if (typeof htmx === 'undefined') {
            console.warn('? [MODAL] HTMX not found, navigating instead');
            window.location.href = url;
            return true;
        }

        htmx.ajax('GET', url, { target: '#modal-container', swap: 'innerHTML' })
            .then(() => {
                const modalContainer = document.getElementById('modal-container');
                if (modalContainer) {
                    modalContainer.style.display = 'flex';
                    modalContainer.classList.remove('hidden');
                    document.body.style.overflow = 'hidden';
                    console.log('? [MODAL] Modal opened successfully');
                } else {
                    throw new Error('Modal container not found after loading content');
                }
            })
            .catch(error => {
                console.error('? [MODAL] Error loading modal:', error);
                if (window.showErrorNotification) {
                    window.showErrorNotification('Error opening job form. Please try again.');
                } else {
                    alert('Error opening job form.');
                }
            });
        return true;
    } catch (e) {
        console.error('? [MODAL] openJobModal critical failure', e);
        return false;
    }
};

window.closeJobModal = function() {
    try {
        console.log('?? [MODAL] Closing job modal');
        const modalContainer = document.getElementById('modal-container') || 
                              document.getElementById('job-modal-container') ||
                              document.querySelector('.modal-backdrop');
        if (modalContainer) {
            modalContainer.style.display = 'none';
            modalContainer.classList.add('hidden');
            modalContainer.innerHTML = '';
        }
        document.body.style.overflow = '';
        return true;
    } catch (e) {
        console.error('? [MODAL] closeJobModal error', e);
        return false;
    }
};

// Enhanced grid interaction for opening modals
window.handleGridCellClick = function(event) {
    return safeExecute('SITE', 'handleGridCellClick', () => {
        const cell = event.target.closest('.scheduler-grid-cell');
        if (!cell) {
            console.log('?? [GRID] Click not on grid cell');
            return false;
        }
        const machineId = cell.getAttribute('data-machine') || 
                         cell.closest('.machine-row')?.getAttribute('data-machine');
        const slotTime = cell.getAttribute('data-slot-time');
        if (!machineId || !slotTime) {
            console.warn('?? [GRID] Missing machine ID or slot time data');
            return false;
        }
        console.log('?? [GRID] Grid cell clicked:', { machineId, slotTime });
        (window.openJobModal || window.openJobModalSafely)(machineId, slotTime);
        return true;
    }, {
        machineId: event.target.closest('.scheduler-grid-cell')?.getAttribute('data-machine'),
        slotTime: event.target.closest('.scheduler-grid-cell')?.getAttribute('data-slot-time'),
        hasValidData: !!(event.target.closest('.scheduler-grid-cell')?.getAttribute('data-machine') && 
                        event.target.closest('.scheduler-grid-cell')?.getAttribute('data-slot-time'))
    });
};

// Enhanced job block interaction for editing
window.handleJobBlockClick = function(event) {
    return safeExecute('SITE', 'handleJobBlockClick', () => {
        const jobBlock = event.target.closest('.job-block');
        if (!jobBlock) {
            console.log('?? [JOB] Click not on job block');
            return false;
        }
        const jobId = jobBlock.getAttribute('data-job-id');
        const machineId = jobBlock.getAttribute('data-machine-id');
        const jobDate = jobBlock.getAttribute('data-job-date');
        if (!jobId || !machineId || !jobDate) {
            console.warn('?? [JOB] Missing job block data attributes');
            return false;
        }
        console.log('?? [JOB] Job block clicked for editing:', { jobId, machineId, jobDate });
        (window.openJobModal || window.openJobModalSafely)(machineId, jobDate, jobId);
        return true;
    }, {
        jobId: event.target.closest('.job-block')?.getAttribute('data-job-id'),
        machineId: event.target.closest('.job-block')?.getAttribute('data-machine-id'),
        jobDate: event.target.closest('.job-block')?.getAttribute('data-job-date'),
        hasValidData: !!(event.target.closest('.job-block')?.getAttribute('data-job-id') && 
                        event.target.closest('.job-block')?.getAttribute('data-machine-id') && 
                        event.target.closest('.job-block')?.getAttribute('data-job-date'))
    });
};

// Ensure modal backdrop clicking closes modal
document.addEventListener('click', function(event) {
    if (event.target.classList.contains('modal-backdrop') || 
        event.target.classList.contains('job-modal-container')) {
        window.closeJobModal();
    }
});

// Escape key handler for modal
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        const modalContainer = document.getElementById('modal-container') || 
                              document.getElementById('job-modal-container') ||
                              document.querySelector('.modal-backdrop');
        if (modalContainer && modalContainer.style.display !== 'none' && 
            !modalContainer.classList.contains('hidden')) {
            window.closeJobModal();
        }
    }
});

console.log('? [SCHEDULER-UI] Global functions loaded and ready');

// Apply machine colors to job blocks immediately (DB-driven ColorHex)
window.applyMachineColors = function(){
    try {
        const colorMap = {};
        document.querySelectorAll('.scheduler-machine-label').forEach(lbl=>{
            const m = lbl.getAttribute('data-machine');
            const style = getComputedStyle(lbl);
            const c = style.getPropertyValue('--machine-color').trim();
            if(m && c) colorMap[m] = c;
        });
        document.querySelectorAll('.scheduler-vertical .machine-column-header').forEach(h=>{
            const m = h.getAttribute('data-machine');
            const style = getComputedStyle(h);
            const c = style.getPropertyValue('--machine-color').trim();
            if(m && c) colorMap[m] = c;
        });
        document.querySelectorAll('.job-block').forEach(j=>{
            const m = j.getAttribute('data-machine') || j.getAttribute('data-machine-id');
            if(m && colorMap[m]){
                j.style.background = colorMap[m];
                j.style.color = '#fff';
            }
        });
    } catch(e){ console.warn('applyMachineColors failed', e);} 
};

// Observe DOM for new job blocks (before HTMX full refresh script fires)
(function(){
    const target = document.getElementById('scheduler-main-content');
    if(!target) return;
    const mo = new MutationObserver(muts=>{
        let added = false;
        for(const m of muts){
            if([...m.addedNodes].some(n=> n.nodeType===1 && n.classList && n.classList.contains('job-block'))){ added=true; break; }
        }
        if(added) window.applyMachineColors();
    });
    mo.observe(target,{childList:true,subtree:true});
})();

// Initial run after load
if(document.readyState==='loading') document.addEventListener('DOMContentLoaded', ()=> setTimeout(window.applyMachineColors,60));
else setTimeout(window.applyMachineColors,60);

// Extend existing HTMX afterSwap hook to re-color
document.addEventListener('htmx:afterSwap', (ev)=>{
    if(ev.detail.target.id==='scheduler-main-content' || ev.detail.target.closest('#scheduler-main-content')){
        setTimeout(window.applyMachineColors,50);
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OpCentrixSchedulerUI;
}
