// Controls zoom for the scheduler grid. The zoom value is the width of
// a single day column. It is saved to localStorage so the user's
// preference is remembered between visits.
export function initSchedulerZoom(zoomInId, zoomOutId, gridId) {
    const MIN = 150;
    const MAX = 400;
    const step = 25;

    const zoomInBtn = document.getElementById(zoomInId);
    const zoomOutBtn = document.getElementById(zoomOutId);
    const grid = document.getElementById(gridId);

    let width = parseInt(localStorage.getItem('schedulerZoom') ?? '200', 10);

    function apply() {
        width = Math.min(MAX, Math.max(MIN, width));
        grid.style.setProperty('--day-width', `${width}px`);
        localStorage.setItem('schedulerZoom', width.toString());
    }

    zoomInBtn?.addEventListener('click', () => {
        width += step;
        apply();
    });
    zoomOutBtn?.addEventListener('click', () => {
        width -= step;
        apply();
    });

    apply();
}
