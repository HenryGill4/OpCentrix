// Clean, consolidated Scheduler UI script (modal form logic now delegated to scheduler-addjob-modal.js)
(function(){
    const SCHED = {};
    let modalRequestInFlight = false; // guard to prevent double-open race

    if(!window.safeExecute){
        window.safeExecute = function(src, action, fn, meta){
            try { return fn(); } catch(err){ console.error(`[${src}] ${action} failed`, {meta, err}); return false; }
        };
    }

    function hydrateSchedulerColors(){
        try {
            if(!document.getElementById('scheduler-color-override')){
                const style = document.createElement('style');
                style.id = 'scheduler-color-override';
                style.textContent = `
.scheduler-horizontal-container .job-block, 
.scheduler-vertical .job-block, 
.scheduler-embedded .job-block {background:var(--machine-color,#6366F1)!important;color:#fff!important;border-radius:6px;}
.job-block .job-block-content{background:transparent!important;color:inherit!important;}
.scheduler-machine-label[class*='machine-']{border-left:0!important;}
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
            // Hook: allow external batch selection script to re-bind
            document.dispatchEvent(new CustomEvent('scheduler:jobsHydrated'));
        } catch(e){ console.warn('hydrateSchedulerColors error', e); }
    }
    window.hydrateSchedulerColors = hydrateSchedulerColors;

    function ensureModalContainer(){
        let mc = document.getElementById('modal-container');
        if(!mc){
            mc = document.createElement('div');
            mc.id = 'modal-container';
            mc.style.display = 'none';
            mc.classList.add('hidden');
            document.body.appendChild(mc);
        }
        return mc;
    }

    // Guard to prevent a full document swap into a partial target
    document.body.addEventListener('htmx:beforeSwap', function(e){
        try {
            if(!e.detail) return;
            const respRaw = e.detail.serverResponse || '';
            const resp = respRaw.toLowerCase();
            if((resp.includes('<html') && resp.includes('<head') && resp.includes('<body')) || resp.includes('<!doctype html')){
                console.error('[SCHED] Preventing full-document swap');
                e.preventDefault();
            }
        } catch(ex){ console.warn('[SCHED] beforeSwap guard error', ex); }
    });

    window.openJobModal = function(machineId, date, jobId){
        return safeExecute('SCHED','openJobModal', ()=>{
            if(modalRequestInFlight){
                console.log('[SCHED] Modal request suppressed (in flight)');
                return false;
            }
            // If selection mode active (flag added by batch selection script), ignore open
            if(window.__schedulerSelectMode){ return false; }
            console.log('[SCHED] openJobModal called:', {machineId, date, jobId});
            if(!machineId || !date) throw new Error('machineId/date required');
            ensureModalContainer();
            const params = new URLSearchParams({ handler:'ShowAddModal', machineId, date });
            if(jobId) params.append('id', jobId);
            const url = `/Scheduler?${params.toString()}`;
            console.log('[SCHED] Making request to:', url);
            if(typeof htmx === 'undefined'){
                console.warn('[SCHED] HTMX missing – redirecting');
                window.location.href = url;
                return true;
            }
            modalRequestInFlight = true;
            try { htmx.ajax('GET', url, { target:'#modal-container', swap:'innerHTML' }); }
            catch(err){ modalRequestInFlight = false; throw err; }
            return true;
        }, {machineId,date,jobId});
    };

    if(!window.openJobModalSafely){ window.openJobModalSafely = window.openJobModal; }

    window.closeJobModal = function(){
        return safeExecute('SCHED','closeJobModal', ()=>{
            const mc = document.getElementById('modal-container') || document.querySelector('.modal-backdrop');
            if(mc){
                mc.style.display='none';
                mc.classList.add('hidden');
                mc.innerHTML='';
            }
            document.body.style.overflow='';
            modalRequestInFlight = false;
            console.log('[SCHED] Modal closed');
            return true;
        });
    };

    document.addEventListener('click', e=>{
        if(e.target && (e.target.id==='modal-container' || e.target.classList.contains('modal-backdrop'))){
            console.log('[SCHED] Backdrop click');
            window.closeJobModal();
        }
    });
    document.addEventListener('keydown', e=>{ if(e.key==='Escape'){ window.closeJobModal(); } });

    const zoomLevels = ['2month','month','week','12h','6h','4h','2h','1h','30min','15min'];

    window.changeZoom = function(action){
        try {
            const url = new URL(window.location.href);
            const current = url.searchParams.get('zoom') || 'week';
            let idx = zoomLevels.indexOf(current);
            if(action==='in') idx=Math.min(zoomLevels.length-1,idx+1);
            else if(action==='out') idx=Math.max(0,idx-1);
            else if(typeof action==='number') idx=Math.max(0,Math.min(zoomLevels.length-1,idx+(action>0?1:-1)));
            else if(typeof action==='string'){ const explicit = zoomLevels.indexOf(action); if(explicit!==-1) idx=explicit; }
            url.searchParams.set('zoom', zoomLevels[idx]);
            window.location.href = url.toString();
        } catch(e){ console.error('changeZoom error', e); }
        return false;
    };

    window.navigatePeriod = function(direction){
        try {
            const dir = typeof direction==='number'? direction : (((''+direction).toLowerCase().startsWith('p'))?-1:1);
            const url = new URL(window.location.href);
            const startStr = url.searchParams.get('startDate');
            let d = startStr? new Date(startStr) : new Date();
            if(isNaN(d.getTime())) d = new Date();
            d.setDate(d.getDate() + (dir<0?-1:1));
            url.searchParams.set('startDate', d.toISOString().split('T')[0]);
            window.location.href = url.toString();
        } catch(e){ console.error('navigatePeriod error', e); }
        return false;
    };

    window.navigateToToday = function(){
        try { const url=new URL(window.location.href); url.searchParams.delete('startDate'); window.location.href=url.toString(); } catch(e){ console.error('navigateToToday error', e);} return false;
    };

    window.toggleOrientation = function(target){
        try { const url=new URL(window.location.href); url.searchParams.set('orientation', target==='vertical'?'vertical':'horizontal'); window.location.href=url.toString(); } catch(e){ console.error('toggleOrientation failed', e);} return false;
    };

    function attachGridHandlers(){
        document.querySelectorAll('.scheduler-grid-cell').forEach(c=>{
            if(c.dataset.ocClickBound==='1') return;
            c.dataset.ocClickBound='1';
            c.addEventListener('click', ()=>{
                if(window.__schedulerSelectMode) return; // suppress open in selection mode
                const machineId = c.getAttribute('data-machine');
                const slotTime = c.getAttribute('data-slot-time');
                if(machineId && slotTime) window.openJobModalSafely(machineId, slotTime);
            });
        });
    }

    // Modal form logic has been externalized to scheduler-addjob-modal.js (JobFormHandler) to avoid duplication.

    function init(){
        console.log('[SCHED] Initializing scheduler UI');
        ensureModalContainer();
        hydrateSchedulerColors();
        attachGridHandlers();
    }
    if(document.readyState==='loading') document.addEventListener('DOMContentLoaded', init); else init();

    document.addEventListener('htmx:afterSwap', e=>{
        if(!e.detail || !e.detail.target) return;
        const t = e.detail.target;
        console.log('[SCHED] HTMX afterSwap:', t.id);
        if(t.id==='scheduler-main-content' || (t.closest && t.closest('#scheduler-main-content'))){
            setTimeout(()=>{ hydrateSchedulerColors(); attachGridHandlers(); }, 30);
        }
        if(t.id==='modal-container' || (t.closest && t.closest('#modal-container'))){
            modalRequestInFlight = false; // allow further opens
            const mc = document.getElementById('modal-container');
            if(!mc) return;
            const jobForm = mc.querySelector('#job-form');
            if(jobForm){
                mc.style.display='flex';
                mc.classList.remove('hidden');
                document.body.style.overflow='hidden';
                console.log('[SCHED] Modal shown after HTMX swap');
                // Delegate initialization to external handler if available
                if(window.OpCentrixScheduler && typeof window.OpCentrixScheduler.initializeAddJobModal === 'function'){
                    try { window.OpCentrixScheduler.initializeAddJobModal(jobForm); } catch(err){ console.error('[SCHED] External modal init failed', err); }
                } else {
                    console.warn('[SCHED] OpCentrixScheduler.initializeAddJobModal not available yet');
                }
            } else {
                console.log('[SCHED] Swap contained no job form (likely success script), skipping re-init');
            }
        }
    });

    document.addEventListener('htmx:responseError', e=>{
        modalRequestInFlight = false;
        console.error('[SCHED] HTMX responseError', e.detail);
    });

    // HTMX lifecycle instrumentation
    document.body.addEventListener('htmx:beforeRequest', function(e){
        try{ if(e.target && e.target.id === 'job-form'){ console.debug('[SCHED-Debug] htmx:beforeRequest -> scheduling form POST', {url: e.detail.pathInfo?.path}); } }catch(ex){ console.warn('[SCHED-Debug] beforeRequest log failed', ex); }
    });
    document.body.addEventListener('htmx:afterRequest', function(e){
        try{ if(e.target && e.target.id === 'job-form'){ console.debug('[SCHED-Debug] htmx:afterRequest status', e.detail.xhr?.status); } }catch(ex){ console.warn('[SCHED-Debug] afterRequest log failed', ex); }
    });
    document.body.addEventListener('htmx:responseError', function(e){
        try{ if(e.target && e.target.id === 'job-form'){ console.error('[SCHED-Debug] htmx:responseError', {status: e.detail.xhr?.status, response: e.detail.xhr?.responseText?.substring(0,500)}); } }catch(ex){ console.warn('[SCHED-Debug] responseError log failed', ex); }
    });
    document.body.addEventListener('htmx:sendError', function(e){
        try{ if(e.target && e.target.id === 'job-form'){ console.error('[SCHED-Debug] htmx:sendError network issue', e.detail); } }catch(ex){ console.warn('[SCHED-Debug] sendError log failed', ex); }
    });

    console.log('[Scheduler UI] Loaded build (modal form logic delegated)');
})();
