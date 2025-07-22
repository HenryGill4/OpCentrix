// Modern Scheduler UI functionality
// Handles zoom controls, grid interactions, and UI state management

class SchedulerUI {
    constructor() {
        this.zoomLevels = ['day', 'hour', '30min', '15min'];
        this.currentZoom = 0;
        this.dayWidth = 120;
        this.init();
    }

    init() {
        this.setupZoomControls();
        this.setupGridInteractions();
        this.setupModalHandlers();
        this.updateGridWidth();
    }

    setupZoomControls() {
        const zoomInBtn = document.getElementById('zoomIn');
        const zoomOutBtn = document.getElementById('zoomOut');
        
        if (zoomInBtn) {
            zoomInBtn.addEventListener('click', () => this.zoomIn());
        }
        
        if (zoomOutBtn) {
            zoomOutBtn.addEventListener('click', () => this.zoomOut());
        }
    }

    setupGridInteractions() {
        // Add hover effects and click handlers for grid cells
        document.addEventListener('mouseover', (e) => {
            if (e.target.matches('.scheduler-grid-cell')) {
                this.highlightCell(e.target);
            }
        });

        document.addEventListener('mouseout', (e) => {
            if (e.target.matches('.scheduler-grid-cell')) {
                this.unhighlightCell(e.target);
            }
        });
    }

    setupModalHandlers() {
        // Handle modal show/hide with HTMX
        document.body.addEventListener('htmx:afterSwap', (e) => {
            if (e.detail.target.id === 'modal-content') {
                const container = document.getElementById('modal-content');
                if (container.innerHTML.trim() === '') {
                    container.classList.add('hidden');
                } else {
                    container.classList.remove('hidden');
                    this.setupModalValidation();
                }
            }
        });
    }

    setupModalValidation() {
        // Setup form validation and auto-calculations
        const partSelect = document.getElementById('modal-part-select');
        const startInput = document.getElementById('modal-start');
        const endInput = document.getElementById('modal-end');

        if (partSelect && startInput && endInput) {
            const updateEndTime = () => {
                const selectedOption = partSelect.options[partSelect.selectedIndex];
                if (!selectedOption || !selectedOption.value) return;

                const estimatedHours = parseFloat(selectedOption.getAttribute('data-estimated-hours')) || 8;
                
                if (startInput.value) {
                    const start = new Date(startInput.value);
                    if (!isNaN(start.getTime())) {
                        const end = new Date(start.getTime() + estimatedHours * 60 * 60 * 1000);
                        endInput.value = end.toISOString().slice(0, 16);
                    }
                }
            };

            partSelect.addEventListener('change', updateEndTime);
            startInput.addEventListener('change', updateEndTime);
        }
    }

    zoomIn() {
        if (this.currentZoom < this.zoomLevels.length - 1) {
            this.currentZoom++;
            this.updateZoom();
        }
    }

    zoomOut() {
        if (this.currentZoom > 0) {
            this.currentZoom--;
            this.updateZoom();
        }
    }

    updateZoom() {
        const newZoom = this.zoomLevels[this.currentZoom];
        const url = new URL(window.location);
        url.searchParams.set('zoom', newZoom);
        window.location.href = url.toString();
    }

    updateGridWidth() {
        // Update CSS custom property for grid width
        const urlParams = new URLSearchParams(window.location.search);
        const zoom = urlParams.get('zoom') || 'day';
        
        let width;
        switch (zoom) {
            case 'hour': width = 50; break;
            case '30min': width = 25; break;
            case '15min': width = 15; break;
            default: width = 120; break;
        }
        
        document.documentElement.style.setProperty('--day-width', width + 'px');
        this.dayWidth = width;
    }

    highlightCell(cell) {
        if (!cell.classList.contains('today')) {
            cell.style.backgroundColor = '#f0f9ff';
        }
    }

    unhighlightCell(cell) {
        if (!cell.classList.contains('today') && !cell.classList.contains('weekend')) {
            cell.style.backgroundColor = '';
        }
    }

    // Utility method to show notifications
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type} fade-in`;
        notification.textContent = message;
        
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 16px 20px;
            border-radius: 8px;
            color: white;
            font-weight: 600;
            z-index: 1000;
            max-width: 300px;
        `;
        
        switch (type) {
            case 'success':
                notification.style.backgroundColor = '#10b981';
                break;
            case 'error':
                notification.style.backgroundColor = '#ef4444';
                break;
            case 'warning':
                notification.style.backgroundColor = '#f59e0b';
                break;
            default:
                notification.style.backgroundColor = '#3b82f6';
        }
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.remove();
        }, 4000);
    }
}

// Legacy function support for existing code
window.initSchedulerZoom = function(zoomInId, zoomOutId, gridId) {
    // This function exists for backward compatibility
    console.log('Using new SchedulerUI class instead of legacy zoom functions');
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.schedulerUI = new SchedulerUI();
});

// Re-initialize after HTMX updates
document.addEventListener('htmx:afterSwap', () => {
    if (window.schedulerUI) {
        window.schedulerUI.setupGridInteractions();
    }
});
