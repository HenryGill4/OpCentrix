// wwwroot/js/utils.js
export function parseAvgToMin(avg) {
    const parts = avg.match(/(\d+)d\s*(\d+)h\s*(\d+)m/) || avg.match(/(\d+)h\s*(\d+)m/) || avg.match(/(\d+)m/);
    let total = 0;
    if (parts) {
        if (parts.length === 4) {
            total += parseInt(parts[1]) * 1440;
            total += parseInt(parts[2]) * 60;
            total += parseInt(parts[3]);
        } else if (parts.length === 3) {
            total += parseInt(parts[1]) * 60;
            total += parseInt(parts[2]);
        } else if (parts.length === 2) {
            total += parseInt(parts[1]);
        }
    }
    return total || 60;
}
