// wwwroot/js/calendar.js
export const printers = ['TI1', 'TI2', 'INC'];
export const today = new Date();
export let dates = [];

export function getLatestJobEndDate(jobs) {
    let latest = new Date(today);
    for (const job of jobs) {
        const start = new Date(job.date);
        const span = Math.ceil(job.duration / 1440);
        const end = new Date(start);
        end.setDate(start.getDate() + span - 1);
        if (end > latest) latest = end;
    }
    return latest;
}

export function generateCalendarDatesUntil(endDate) {
    const totalDays = Math.ceil((endDate - today) / (1000 * 60 * 60 * 24));
    const paddedDays = Math.ceil((totalDays + 1) / 7) * 7;
    return [...Array(paddedDays)].map((_, i) => {
        const d = new Date(today);
        d.setDate(today.getDate() + i);
        return d.toISOString().split('T')[0];
    });
}

export function buildCalendarGrid(dates, addJobCallback) {
    const head = document.getElementById('calendar-head');
    const body = document.getElementById('calendar-body');

    let headerRow = '<tr><th></th>';
    printers.forEach(printer => {
        headerRow += `<th>${printer}</th>`;
    });
    headerRow += '</tr>';
    head.innerHTML = headerRow;

    let bodyHtml = '';
    dates.forEach(date => {
        const d = new Date(date + 'T12:00:00');
        const isWeekend = d.getDay() === 0 || d.getDay() === 6;
        const weekendClass = isWeekend ? 'weekend-column' : '';

        bodyHtml += `<tr><th class="${weekendClass}">${date}</th>`;
        printers.forEach(printer => {
            const cellId = `${date}_${printer}`;
            bodyHtml += `
                <td id="${cellId}" class="${weekendClass}">
                    <div class="job-container">
                        <button class="add-btn" onclick="addJob('${printer}','${date}')">Add</button>
                    </div>
                </td>`;
        });
        bodyHtml += '</tr>';
    });

    body.innerHTML = bodyHtml;
}
