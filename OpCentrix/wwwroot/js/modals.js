//Modals.js:
import { parseAvgToMin } from './utils.js';

let partList = [];

export async function fetchParts() {
    const res = await fetch('/api/parts');
    partList = await res.json();

    const printer = document.getElementById('modalPrinter').value;
    const date = document.getElementById('modalDate').value;
    const materialType = ['TI1', 'TI2'].includes(printer) ? 'titanium' : 'inconel';

    const select = document.getElementById('modalPart');
    const suggestionBox = document.getElementById('suggestionBox');
    select.innerHTML = '';
    suggestionBox.textContent = '';

    const options = [];

    for (const part of partList) {
        const partMaterial = ['TI1', 'TI2'].includes(part.printer) ? 'titanium' : 'inconel';
        if (partMaterial !== materialType) continue;

        const duration = parseAvgToMin(part.avgDuration);
        const jobStart = new Date(date + 'T08:00:00');
        const jobEnd = new Date(jobStart.getTime() + duration * 60000);

        const endHour = jobEnd.getHours();
        const endMin = jobEnd.getMinutes();
        const endDay = jobEnd.getDay();

        let tier = 3;

        if (endDay !== 0 && endDay !== 6) {
            if (endHour < 15 || (endHour === 15 && endMin <= 30)) {
                tier = 1;
            } else {
                tier = 2;
            }
        }

        options.push({
            partNumber: part.partNumber,
            avg: part.avgDuration,
            duration,
            tier,
            label: `${part.partNumber} — ${part.avgDuration}`
        });
    }

    options.sort((a, b) => a.tier - b.tier);

    for (const part of options) {
        const opt = document.createElement('option');
        opt.value = part.partNumber;
        opt.textContent = part.label;
        opt.dataset.duration = part.duration;
        opt.dataset.tier = part.tier;
        select.appendChild(opt);
    }

    select.addEventListener('change', () => {
        const selected = select.options[select.selectedIndex];
        document.getElementById('modalDuration').value = selected.dataset.duration;

        const tier = parseInt(selected.dataset.tier);
        let msg = '';
        if (tier === 1) {
            msg = `🟢 ✔️ Recommended: ${selected.textContent} (Day shift, ends before 3:30 PM)`;
        } else if (tier === 2) {
            msg = `🟡 🌙 Warning: ${selected.textContent} (Night shift, ends after 3:30 PM)`;
        } else {
            msg = `🔴 ⚠️ Last Resort: ${selected.textContent} (Ends on weekend)`;
        }
        suggestionBox.textContent = msg;
    });

    if (options.length > 0) {
        const best = options[0];
        const bestOption = [...select.options].find(o => o.value === best.partNumber);
        if (bestOption) {
            bestOption.selected = true;
            document.getElementById('modalDuration').value = bestOption.dataset.duration;
            select.dispatchEvent(new Event('change'));
        }
    } else {
        suggestionBox.textContent = '⚠️ No matching parts available for this printer.';
        document.getElementById('modalDuration').value = '';
    }
}


export function addJob(printer, date) {
    document.getElementById('modalPrinter').value = printer;
    document.getElementById('modalDate').value = date;

    const cellId = `${date}_${printer}`;
    const cell = document.getElementById(cellId);
    const container = cell.querySelector('.job-container');
    const existingJobs = container.querySelectorAll('.job, .changeover');

    let totalUsed = 0;
    existingJobs.forEach(jobEl => {
        const height = parseFloat(jobEl.style.height || "0");
        const minutes = Math.round((height / 100) * 1440);
        totalUsed += minutes;
    });

    const startMinutes = Math.ceil(totalUsed / 15) * 15;
    const hours = Math.floor(startMinutes / 60);
    const minutes = startMinutes % 60;

    const defaultStartTime = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    document.getElementById('modalStartTime').value = defaultStartTime;

    document.getElementById('modal').style.display = 'flex';

    fetchParts();
}

export function closeModal() {
    document.getElementById('modal').style.display = 'none';
}

export async function confirmAdd() {
    const part = document.getElementById('modalPart').value;
    const printer = document.getElementById('modalPrinter').value;
    const date = document.getElementById('modalDate').value;
    const duration = parseInt(document.getElementById('modalDuration').value);
    const startTime = document.getElementById('modalStartTime').value || "08:00";

    await fetch('/api/jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ partNumber: part, printer, date, startTime, duration })
    });

    closeModal();
    window.location.reload();
}

export function openEditModal(job) {
    document.getElementById('editId').value = job.id;
    document.getElementById('editPart').value = job.partNumber;
    document.getElementById('editDuration').value = job.duration;
    document.getElementById('editModal').style.display = 'flex';
}

export function closeEditModal() {
    document.getElementById('editModal').style.display = 'none';
}

export async function saveEdit() {
    const id = document.getElementById('editId').value;
    const duration = document.getElementById('editDuration').value;
    await fetch(`/api/jobs/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, durationMinutes: parseInt(duration) })
    });
    closeEditModal();
    window.location.reload();
}

export async function deleteJob() {
    const id = document.getElementById('editId').value;
    await fetch(`/api/jobs/${id}`, {
        method: 'DELETE'
    });
    closeEditModal();
    window.location.reload();
}

document.body.addEventListener('htmx:afterSwap', function (e) {
    if (e.detail.target.id === "modal-content") {
        const isClosing = e.detail.xhr.responseText.includes("class=\"bg-white");
        if (!isClosing) {
            document.getElementById("modal-content").classList.remove("hidden");
        }
    }
});
