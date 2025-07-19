export function renderJobs(jobs, dates, openEditModal) {
    const dateIndex = {};
    dates.forEach((d, i) => dateIndex[d] = i);
    const usageMap = {};

    jobs.forEach(job => {
        const index = dateIndex[job.date || job.startDateTime?.split('T')[0]];
        if (index === undefined) return;

        const printer = job.printer;
        const totalMinutes = job.duration || job.durationMinutes;
        const changeoverMinutes = 180;
        let remaining = totalMinutes;
        let currentDateIndex = index;

        while (remaining > 0 && currentDateIndex < dates.length) {
            const currentDate = dates[currentDateIndex];
            const cellId = `${currentDate}_${printer}`;
            const cell = document.getElementById(cellId);
            if (!cell) break;

            const container = cell.querySelector('.job-container');
            const addBtn = cell.querySelector('.add-btn');

            usageMap[cellId] = usageMap[cellId] || 0;
            const timeLeft = 1440 - usageMap[cellId];
            if (timeLeft <= 0) {
                if (addBtn) addBtn.style.display = 'none';
                currentDateIndex++;
                continue;
            }

            const minutesThisDay = Math.min(remaining, timeLeft);
            const heightPercent = (minutesThisDay / 1440) * 100;

            const jobDiv = document.createElement('div');
            jobDiv.className = 'job';
            jobDiv.style.height = heightPercent + '%';
            jobDiv.textContent = job.partNumber;
            jobDiv.onclick = () => openEditModal(job);

            // Color coding by printer
            if (printer === 'TI1') jobDiv.style.background = '#007bff';
            if (printer === 'TI2') jobDiv.style.background = '#28a745';
            if (printer === 'INC') jobDiv.style.background = '#dc3545';

            // Time tooltip
            let startTime = job.startTime || "08:00";
            const [startHour, startMinute] = startTime.split(':').map(Number);
            const start = new Date(0, 0, 0, startHour, startMinute);
            const end = new Date(start.getTime() + totalMinutes * 60000);

            function formatTime(d) {
                let h = d.getHours();
                let m = d.getMinutes().toString().padStart(2, '0');
                const ampm = h >= 12 ? 'PM' : 'AM';
                h = h % 12 || 12;
                return `${h}:${m} ${ampm}`;
            }

            jobDiv.title = `Part: ${job.partNumber}\n${formatTime(start)}–${formatTime(end)}\n${totalMinutes} min`;
            container.appendChild(jobDiv);

            usageMap[cellId] += minutesThisDay;
            remaining -= minutesThisDay;

            if (remaining <= 0 && usageMap[cellId] + changeoverMinutes <= 1440) {
                const changeDiv = document.createElement('div');
                changeDiv.className = 'changeover';
                changeDiv.style.height = (changeoverMinutes / 1440) * 100 + '%';
                changeDiv.textContent = 'Changeover';
                changeDiv.title = 'Changeover (3 hours)';
                container.appendChild(changeDiv);
                usageMap[cellId] += changeoverMinutes;
            }

            if (usageMap[cellId] >= 1440 && addBtn) {
                addBtn.style.display = 'none';
            }

            currentDateIndex++;
            if (addBtn && container.contains(addBtn)) {
                container.removeChild(addBtn);
                container.appendChild(addBtn);
            }
        }
    });
}
