//Scheduler-UI.js:
let zoomLevels = [
    { label: '30 min', columnsPerDay: 48 },
    { label: '1 hr', columnsPerDay: 24 },
    { label: '2 hr', columnsPerDay: 12 },
    { label: '6 hr', columnsPerDay: 4 },
    { label: '1 day', columnsPerDay: 1 },
    { label: '1 week', columnsPerDay: 1 / 7 },
    { label: '1 month', columnsPerDay: 1 / 30 }
];

let currentZoomIndex = 1; // Start at '1 hr'

export function initSchedulerZoom(zoomInBtnId, zoomOutBtnId, containerId) {
    const zoomIn = document.getElementById(zoomInBtnId);
    const zoomOut = document.getElementById(zoomOutBtnId);
    const container = document.getElementById(containerId);

    function applyZoom() {
        const scale = zoomLevels[currentZoomIndex];
        const gridCols = Math.max(1, Math.round(scale.columnsPerDay * 1)); // 1 day default
        container.style.gridTemplateColumns = `repeat(${gridCols}, minmax(40px, 1fr))`;
        container.setAttribute('data-zoom-label', scale.label);
    }

    zoomIn?.addEventListener('click', () => {
        if (currentZoomIndex > 0) {
            currentZoomIndex--;
            applyZoom();
        }
    });

    zoomOut?.addEventListener('click', () => {
        if (currentZoomIndex < zoomLevels.length - 1) {
            currentZoomIndex++;
            applyZoom();
        }
    });

    applyZoom();
}
