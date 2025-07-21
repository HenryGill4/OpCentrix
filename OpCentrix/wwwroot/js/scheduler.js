const zoomLevels = [60, 40, 20];
let zoomIndex = 0;

function applyZoom() {
  const width = zoomLevels[zoomIndex];
  document.documentElement.style.setProperty('--cell-width', width + 'px');
}

window.adjustZoom = function(delta) {
  zoomIndex = Math.min(Math.max(zoomIndex + delta, 0), zoomLevels.length - 1);
  applyZoom();
}

document.addEventListener('DOMContentLoaded', applyZoom);

