// Clean, consolidated Scheduler UI script
(function(){
    const SCHED = {};

    // -------- Utilities --------
    if(!window.safeExecute){
        window.safeExecute = function(src, action, fn, meta){
            try { return fn(); } catch(err){ console.error(`[${src}] ${action} failed`, {meta, err}); return false; }
        };
    }

    // -------- Color Hydration (single source) --------
    function hydrateSchedulerColors(){
        try {
            // Inject one override stylesheet if not present
            if(!document.getElementById('scheduler-color-override')){
                const style = document.createElement('style');
                style.id = 'scheduler-color-override';
                style.textContent = `
.scheduler-horizontal-container .job-block, 
.scheduler-vertical .job-block, 
.scheduler-embedded .job-block {background:var(--machine-color,#6366F1)!important;color:#fff!important;border-radius:6px;}
.job-block .job-block-content{background:transparent!important;color:inherit!important;}
/* Remove legacy left borders from machine label variants */
.scheduler-machine-label[class*='machine-']{border-left:0!important;}

/* Remove legacy per-machine job block border-left + forced legacy color */
.job-block[class*='machine-']{border-left:0!important;}
.job-block.machine-cnc1{background:var(--machine-color,#6366F1)!important;}
`;
                document.head.appendChild(style);
            }
            const machineColors = {};
            document.querySelectorAll('.scheduler-machine-label, .machine-column-header').forEach(el=>{
                const id = el.getAttribute('data-machine');
                const c = (el.getAttribute('data-color')||'').trim();
                if(id && c){ machineColors[id] = c; el.style.setProperty('--machine-color', c); }
            });
            document.querySelectorAll('.job-block').forEach(j=>{
                const mid = j.getAttribute('data-machine') || j.getAttribute('data-machine-id');
                let c = j.getAttribute('data-color');
                if(!c && mid && machineColors[mid]) c = machineColors[mid];
                if(c){
                    j.dataset.color = c;
                    j.style.setProperty('--machine-color', c);
                    j.style.setProperty('background', c, 'important');
                    j.style.setProperty('color', '#fff', 'important');
                    j.style.removeProperty('border-left');
                }
            });
        } catch(e){ console.warn('hydrateSchedulerColors error', e); }
    }
    window.hydrateSchedulerColors = hydrateSchedulerColors;

    // -------- Modal Functions --------
    window.openJobModal = function(machineId, date, jobId){
        return safeExecute('SCHED','openJobModal', ()=>{
            if(!machineId || !date) throw new Error('machineId/date required');
            const params = new URLSearchParams({ handler:'ShowAddModal', machineId, date });
            if(jobId) params.append('id', jobId);
            const url = `/Scheduler?${params.toString()}`;
            if(typeof htmx === 'undefined'){ window.location.href = url; return true; }
            return htmx.ajax('GET', url, { target:'#modal-container', swap:'innerHTML' }).then(()=>{
                const mc = document.getElementById('modal-container');
                if(mc){ mc.style.display='flex'; mc.classList.remove('hidden'); document.body.style.overflow='hidden'; }
                hydrateSchedulerColors();
            });
        }, {machineId,date,jobId});
    };
    if(!window.openJobModalSafely){ window.openJobModalSafely = window.openJobModal; }

    window.closeJobModal = function(){
        return safeExecute('SCHED','closeJobModal', ()=>{
            const mc = document.getElementById('modal-container') || document.querySelector('.modal-backdrop');
            if(mc){ mc.style.display='none'; mc.classList.add('hidden'); mc.innerHTML=''; }
            document.body.style.overflow='';
            return true;
        });
    };

    // Dismiss on backdrop click / ESC
    document.addEventListener('click', e=>{
        if(e.target && (e.target.id==='modal-container' || e.target.classList.contains('modal-backdrop'))){ window.closeJobModal(); }
    });
    document.addEventListener('keydown', e=>{ if(e.key==='Escape') window.closeJobModal(); });

    // -------- Navigation / Zoom Wrappers --------
    const zoomLevels = ['2month','month','week','12h','6h','4h','2h','1h','30min','15min'];

    window.changeZoom = function(action){
        try {
            const url = new URL(window.location.href);
            const current = url.searchParams.get('zoom') || 'week';
            let idx = zoomLevels.indexOf(current);
            if(action === 'in') idx = Math.min(zoomLevels.length-1, idx+1);
            else if(action === 'out') idx = Math.max(0, idx-1);
            else if(typeof action === 'number') idx = Math.max(0, Math.min(zoomLevels.length-1, idx + (action>0?1:-1)));
            else if(typeof action === 'string'){ const explicit = zoomLevels.indexOf(action); if(explicit!==-1) idx = explicit; }
            url.searchParams.set('zoom', zoomLevels[idx]);
            window.location.href = url.toString();
        } catch(e){ console.error('changeZoom error', e); }
        return false;
    };

    window.navigatePeriod = function(direction){
        try {
            const dir = typeof direction==='number'? direction : ((''+direction).toLowerCase().startsWith('p')?-1:1);
            const url = new URL(window.location.href);
            const startStr = url.searchParams.get('startDate');
            let d = startStr? new Date(startStr) : new Date();
            if(isNaN(d.getTime())) d = new Date();
            d.setDate(d.getDate() + (dir<0?-1:1)); // one column at any zoom
            url.searchParams.set('startDate', d.toISOString().split('T')[0]);
            window.location.href = url.toString();
        } catch(e){ console.error('navigatePeriod error', e); }
        return false;
    };

    window.navigateToToday = function(){
        try { const url = new URL(window.location.href); url.searchParams.delete('startDate'); window.location.href = url.toString(); } catch(e){ console.error('navigateToToday error', e); }
        return false;
    };

    window.toggleOrientation = function(target){
        try { const url = new URL(window.location.href); url.searchParams.set('orientation', target==='vertical'?'vertical':'horizontal'); window.location.href=url.toString(); } catch(e){ console.error('toggleOrientation failed', e);} return false;
    };

    // -------- Simple Grid Highlight (optional) --------
    function attachGridHandlers(){
        document.querySelectorAll('.scheduler-grid-cell').forEach(c=>{
            c.addEventListener('click', e=>{
                const machineId = c.getAttribute('data-machine');
                const slotTime = c.getAttribute('data-slot-time');
                if(machineId && slotTime) window.openJobModalSafely(machineId, slotTime);
            });
        });
    }

    // -------- Init --------
    function init(){ hydrateSchedulerColors(); attachGridHandlers(); }
    if(document.readyState==='loading') document.addEventListener('DOMContentLoaded', init); else init();

    // Re-hydrate after HTMX swaps
    document.addEventListener('htmx:afterSwap', e=>{
        if(!e.detail || !e.detail.target) return;
        const t = e.detail.target;
        if(t.id==='scheduler-main-content' || (t.closest && t.closest('#scheduler-main-content'))){
            setTimeout(()=>{ hydrateSchedulerColors(); attachGridHandlers(); }, 30);
        }
        if(t.id==='modal-container' || (t.closest && t.closest('#modal-container'))){
            const mc = document.getElementById('modal-container');
            if(mc){ mc.style.display='flex'; mc.classList.remove('hidden'); document.body.style.overflow='hidden'; }
        }
    });

    console.log('[Scheduler UI] Loaded clean build');
})();
