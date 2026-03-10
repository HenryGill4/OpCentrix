/**
 * Machine State SignalR Client
 * Provides real-time machine state updates for OpCentrix dashboards.
 * 
 * Usage:
 *   const client = new MachineStateClient();
 *   client.onStateChange((update) => { console.log(update); });
 *   client.subscribeToAllMachines();
 */

class MachineStateClient {
    constructor(hubUrl = '/hubs/machinestate') {
        this.hubUrl = hubUrl;
        this.connection = null;
        this.handlers = {
            stateChange: [],
            statesBatch: [],
            alarm: [],
            offline: [],
            online: []
        };
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 10;
    }

    /**
     * Start the SignalR connection
     */
    async start() {
        if (typeof signalR === 'undefined') {
            console.error('[MachineState] SignalR library not loaded');
            return false;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl)
            .withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Wire up event handlers
        this.connection.on('MachineStateChanged', (update) => {
            this._emit('stateChange', update);
        });

        this.connection.on('MachineStatesBatch', (updates) => {
            this._emit('statesBatch', updates);
        });

        this.connection.on('MachineAlarm', (alarm) => {
            this._emit('alarm', alarm);
        });

        this.connection.on('MachineOffline', (data) => {
            this._emit('offline', data);
        });

        this.connection.on('MachineOnline', (data) => {
            this._emit('online', data);
        });

        // Connection lifecycle events
        this.connection.onreconnecting((error) => {
            console.warn('[MachineState] Reconnecting...', error);
            this.isConnected = false;
        });

        this.connection.onreconnected((connectionId) => {
            console.log('[MachineState] Reconnected:', connectionId);
            this.isConnected = true;
            this.reconnectAttempts = 0;
        });

        this.connection.onclose((error) => {
            console.warn('[MachineState] Connection closed', error);
            this.isConnected = false;
        });

        try {
            await this.connection.start();
            console.log('[MachineState] Connected to hub');
            this.isConnected = true;
            return true;
        } catch (err) {
            console.error('[MachineState] Connection failed:', err);
            return false;
        }
    }

    /**
     * Stop the SignalR connection
     */
    async stop() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
        }
    }

    /**
     * Subscribe to updates for a specific machine
     */
    async subscribeToMachine(machineId) {
        if (!this.isConnected) {
            console.warn('[MachineState] Not connected');
            return false;
        }
        try {
            await this.connection.invoke('JoinMachineGroup', machineId);
            console.log('[MachineState] Subscribed to machine:', machineId);
            return true;
        } catch (err) {
            console.error('[MachineState] Subscribe failed:', err);
            return false;
        }
    }

    /**
     * Unsubscribe from a specific machine
     */
    async unsubscribeFromMachine(machineId) {
        if (!this.isConnected) return;
        try {
            await this.connection.invoke('LeaveMachineGroup', machineId);
        } catch (err) {
            console.error('[MachineState] Unsubscribe failed:', err);
        }
    }

    /**
     * Subscribe to all machine updates
     */
    async subscribeToAllMachines() {
        if (!this.isConnected) {
            console.warn('[MachineState] Not connected');
            return false;
        }
        try {
            await this.connection.invoke('JoinAllMachines');
            console.log('[MachineState] Subscribed to all machines');
            return true;
        } catch (err) {
            console.error('[MachineState] Subscribe all failed:', err);
            return false;
        }
    }

    /**
     * Unsubscribe from all machine updates
     */
    async unsubscribeFromAllMachines() {
        if (!this.isConnected) return;
        try {
            await this.connection.invoke('LeaveAllMachines');
        } catch (err) {
            console.error('[MachineState] Unsubscribe all failed:', err);
        }
    }

    // Event handlers
    onStateChange(handler) { this.handlers.stateChange.push(handler); }
    onStatesBatch(handler) { this.handlers.statesBatch.push(handler); }
    onAlarm(handler) { this.handlers.alarm.push(handler); }
    onOffline(handler) { this.handlers.offline.push(handler); }
    onOnline(handler) { this.handlers.online.push(handler); }

    _emit(event, data) {
        (this.handlers[event] || []).forEach(h => {
            try { h(data); } catch (e) { console.error('[MachineState] Handler error:', e); }
        });
    }
}

/**
 * UI Helper for updating machine cards/elements
 */
class MachineStateUI {
    constructor(client) {
        this.client = client;
        this._setupHandlers();
    }

    _setupHandlers() {
        this.client.onStateChange((update) => this._updateMachineElement(update));
        this.client.onAlarm((alarm) => this._showAlarm(alarm));
        this.client.onOffline((data) => this._markOffline(data));
        this.client.onOnline((data) => this._markOnline(data));
    }

    _updateMachineElement(update) {
        // Find machine card by data attribute
        const card = document.querySelector(`[data-machine-id="${update.machineId}"]`);
        if (!card) return;

        // Update status badge
        const statusBadge = card.querySelector('.machine-status');
        if (statusBadge) {
            statusBadge.textContent = update.status;
            statusBadge.className = 'machine-status badge ' + this._getStatusClass(update.status);
        }

        // Update progress bar
        const progressBar = card.querySelector('.build-progress');
        if (progressBar && update.buildProgressPercent != null) {
            progressBar.style.width = `${update.buildProgressPercent}%`;
            progressBar.setAttribute('aria-valuenow', update.buildProgressPercent);
        }

        // Update layer info
        const layerInfo = card.querySelector('.layer-info');
        if (layerInfo && update.currentLayer != null) {
            layerInfo.textContent = `Layer ${update.currentLayer} / ${update.totalLayers}`;
        }

        // Flash on state change
        if (update.isStateChange) {
            card.classList.add('state-changed');
            setTimeout(() => card.classList.remove('state-changed'), 2000);
        }
    }

    _showAlarm(alarm) {
        // Show toast notification for alarms
        if (typeof showToast === 'function') {
            showToast(`?? ${alarm.machineCode}: ${alarm.alarm}`, 'warning');
        } else {
            console.warn(`[ALARM] ${alarm.machineCode}: ${alarm.alarm}`);
        }
    }

    _markOffline(data) {
        const card = document.querySelector(`[data-machine-id="${data.machineId}"]`);
        if (card) {
            card.classList.add('machine-offline');
            const statusBadge = card.querySelector('.machine-status');
            if (statusBadge) {
                statusBadge.textContent = 'Offline';
                statusBadge.className = 'machine-status badge bg-secondary';
            }
        }
    }

    _markOnline(data) {
        const card = document.querySelector(`[data-machine-id="${data.machineId}"]`);
        if (card) {
            card.classList.remove('machine-offline');
        }
    }

    _getStatusClass(status) {
        const statusMap = {
            'Printing': 'bg-success',
            'Idle': 'bg-info',
            'Ready': 'bg-primary',
            'Preheating': 'bg-warning',
            'Cooling': 'bg-info',
            'Paused': 'bg-warning',
            'Error': 'bg-danger',
            'Offline': 'bg-secondary',
            'Maintenance': 'bg-warning'
        };
        return statusMap[status] || 'bg-secondary';
    }
}

// Auto-initialize if on a dashboard page
document.addEventListener('DOMContentLoaded', async () => {
    // Check if we're on a page that needs machine state updates
    const dashboard = document.querySelector('[data-machine-state-enabled]');
    if (!dashboard) return;

    // Load SignalR library if not already loaded
    if (typeof signalR === 'undefined') {
        const script = document.createElement('script');
        script.src = '/_content/Microsoft.AspNetCore.SignalR.Client/signalr.min.js';
        script.onload = initMachineState;
        document.head.appendChild(script);
    } else {
        initMachineState();
    }
});

async function initMachineState() {
    window.machineStateClient = new MachineStateClient();
    const connected = await window.machineStateClient.start();
    
    if (connected) {
        window.machineStateUI = new MachineStateUI(window.machineStateClient);
        await window.machineStateClient.subscribeToAllMachines();
        console.log('[MachineState] Real-time updates enabled');
    }
}
