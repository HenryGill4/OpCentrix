// Clean, consolidated Scheduler UI script (modal form logic now delegated to scheduler-addjob-modal.js)
(function(){
    const SCHED = {};
    let modalRequestInFlight = false; // guard to prevent double-open race

    if(!window.safeExecute){
        window.safeExecute = function(src, action, fn, meta){
            try { return fn(); } catch(err){ console.error(`[${src}] ${action} failed`, {meta, err}); return false; }
        };
    }

    // Runtime bridge for Print Tracking integration from scheduler
    if(!window.OpCentrixRuntime){ window.OpCentrixRuntime = {}; }
    // Open Start modal using job id
    window.OpCentrixRuntime.openStartJobModal = function(jobId){
        return safeExecute('SCHED','openStartJobModal', ()=>{
            if(!jobId){ console.warn('[SCHED] openStartJobModal requires jobId'); return false; }
            const url = `/PrintTracking?handler=StartPrintModal&jobId=${encodeURIComponent(jobId)}`;
            ensureModalContainer();
            if(typeof htmx !== 'undefined'){
                return htmx.ajax('GET', url, { target:'#modal-container', swap:'innerHTML' });
            }
            window.open(url,'_blank');
            return true;
        }, {jobId});
    };
    // Open Complete modal using job id (preferred) or machine id
    window.OpCentrixRuntime.openCompleteJobModal = function(jobId, machineId){
        return safeExecute('SCHED','openCompleteJobModal', ()=>{
            let url = '/PrintTracking?handler=PostPrintModal';
            const p = new URLSearchParams();
            if(jobId){ p.set('jobId', jobId); }
            if(machineId){ p.set('printerName', machineId); }
            if([...
                p.keys()].length){ url += '&' + p.toString(); }
            ensureModalContainer();
            if(typeof htmx !== 'undefined'){
                return htmx.ajax('GET', url, { target:'#modal-container', swap:'innerHTML' });
            }
            window.open(url,'_blank');
            return true;
        }, {jobId, machineId});
    };
    // Close modal shortcut used by print tracking JS
    window.OpCentrixRuntime.closeModal = function(){ try{ window.closeJobModal && window.closeJobModal(); }catch(e){ console.warn('close modal failed', e);} };
    // Refresh scheduler grid after PT operations
    window.OpCentrixRuntime.refreshScheduler = function(){ try{
        const qs = window.location.search || '';
        if(window.htmx){
            htmx.ajax('GET', '/Scheduler?handler=RefreshGrid'+(qs?qs.replace('?', '&'):''), { target:'#scheduler-main-content', swap:'innerHTML' });
            htmx.ajax('GET', '/Scheduler?handler=RefreshSummary'+(qs?qs.replace('?', '&'):''), { target:'#footer-summary', swap:'innerHTML' });
        } else { window.location.reload(); }
    }catch(e){ console.warn('refresh scheduler failed', e);} };

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
        // Click on job blocks to open appropriate modal based on status
        document.querySelectorAll('.job-core[data-job-status]')?.forEach(j=>{
            if(j.dataset.ptBound==='1') return; j.dataset.ptBound='1';
            j.addEventListener('dblclick', (e)=>{
                const jobId = j.getAttribute('data-job-id');
                const machineId = j.getAttribute('data-machine-id');
                const status = (j.getAttribute('data-job-status')||'').toLowerCase();
                if(status.includes('building') || status.includes('progress')){
                    window.OpCentrixRuntime && window.OpCentrixRuntime.openCompleteJobModal(jobId, machineId);
                } else if(status.includes('scheduled')){
                    window.OpCentrixRuntime && window.OpCentrixRuntime.openStartJobModal(jobId);
                }
                e.stopPropagation();
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
                // Success scenario (create/update) -> force full refresh so sizing logic runs cleanly
                console.log('[SCHED] Modal success detected – reloading page for clean sizing');
                window.location.reload();
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
        try{ if(e.target && e.target.id === 'job-form'){ console.error('[SCHED-Debug] sendError network issue', e.detail); } }catch(ex){ console.warn('[SCHED-Debug] sendError log failed', ex); }
    });

    function enhanceLegacyJobBlocks(){
        const monthMap = {jan:0,feb:1,mar:2,apr:3,may:4,jun:5,jul:6,aug:7,sep:8,oct:9,nov:10,dec:11};
        if(!document.getElementById('legacy-progress-css')){
            const css = document.createElement('style');
            css.id='legacy-progress-css';
            css.textContent=`.job-progress-bar{position:absolute;left:0;bottom:0;height:4px;background:rgba(255,255,255,.25);width:100%;overflow:hidden;border-radius:0 0 4px 4px;}
.job-progress-fill{height:100%;background:linear-gradient(90deg,#22c55e,#16a34a);transition:width .6s ease;}
.run-badge{position:absolute;top:0;right:0;background:#16a34a;color:#fff;font-size:10px;padding:0 4px;border-bottom-left-radius:4px;font-weight:600;display:flex;align-items:center;gap:2px;animation:pulse-green 1.4s ease-in-out infinite;}
@keyframes pulse-green{0%,100%{box-shadow:0 0 0 0 rgba(34,197,94,.8);}50%{box-shadow:0 0 0 6px rgba(34,197,94,0);} }
.building-anim:before{content:"";position:absolute;inset:0;background:repeating-linear-gradient(135deg,rgba(34,197,94,.15) 0 8px,rgba(16,185,129,.15) 8px 16px);animation: moveStripes 6s linear infinite;mix-blend-mode:overlay;pointer-events:none;}
@keyframes moveStripes {0%{background-position:0 0;}100%{background-position=256px 0;}}
/* Machine status indicator (reuse existing .machine-status element) */
.scheduler-machine-label{position:relative;}
.scheduler-machine-label .machine-status{position:absolute;top:8px;right:8px;width:12px;height:12px;border-radius:50%;background:#64748b;border:2px solid #fff;box-shadow:0 0 0 0 rgba(0,0,0,.15);}
.scheduler-machine-label .machine-status.status-active{background:#16a34a;}
.scheduler-machine-label .machine-status.status-scheduled{background:#f59e0b;}
.scheduler-machine-label .machine-status.status-idle{background:#64748b;}
.scheduler-machine-label .machine-status.pulse{animation:machPulse 2s infinite;}
@keyframes machPulse{0%{box-shadow:0 0 0 0 rgba(34,197,94,.6);}70%{box-shadow:0 0 0 10px rgba(34,197,94,0);}100%{box-shadow:0 0 0 0 rgba(34,197,94,0);} }`;
            document.head.appendChild(css);
        }
        const now = new Date();
        document.querySelectorAll('.job-block:not([data-legacy-upgraded])').forEach(el=>{
            el.dataset.legacyUpgraded='1';
            if(el.querySelector('.job-core')) return; // new partial already handles UI
            const statusRaw = (el.getAttribute('data-status')||'').toLowerCase();
            if(!(statusRaw.includes('progress') || statusRaw.includes('building'))) return;
            let startIso = el.getAttribute('data-actual-start') || el.getAttribute('data-start');
            let endIso = el.getAttribute('data-end');
            const title = el.getAttribute('title') || '';
            if((!startIso || !endIso) && title.includes(' to ')){
                const regex = /-\s([A-Za-z]{3}) (\d{2}) (\d{2}:\d{2}) to ([A-Za-z]{3}) (\d{2}) (\d{2}:\d{2})\s-/;
                const m = title.match(regex);
                if(m){
                    const year = now.getFullYear();
                    const sm = monthMap[m[1].toLowerCase()];
                    const em = monthMap[m[4].toLowerCase()];
                    if(sm!=null && em!=null){
                        startIso = new Date(year, sm, parseInt(m[2]), parseInt(m[3].split(':')[0]), parseInt(m[3].split(':')[1]),0).toISOString();
                        endIso = new Date(year, em, parseInt(m[5]), parseInt(m[6].split(':')[0]), parseInt(m[6].split(':')[1]),0).toISOString();
                    }
                }
            }
            if(!startIso){
                const onclick = el.getAttribute('onclick')||'';
                const om = onclick.match(/openJobModalSafely\('[^']*','([^']+)'/);
                if(om){ startIso = new Date(om[1]).toISOString(); }
            }
            if(startIso && !el.getAttribute('data-start')) el.setAttribute('data-start', startIso);
            if(endIso && !el.getAttribute('data-end')) el.setAttribute('data-end', endIso);
            if(!startIso || !endIso) return;
            const start = new Date(startIso); const end = new Date(endIso);
            if(isNaN(start)||isNaN(end)|| end <= start) return;
            let pct = (Date.now() - start.getTime()) / (end.getTime() - start.getTime()) * 100;
            pct = Math.max(0, Math.min(150, pct));
            if(!el.querySelector('.job-progress-bar')){
                const bar=document.createElement('div'); bar.className='job-progress-bar';
                const fill=document.createElement('div'); fill.className='job-progress-fill'; bar.appendChild(fill); el.appendChild(bar);
            }
            const fill = el.querySelector('.job-progress-fill'); if(fill) fill.style.width = Math.min(100, pct) + '%';
            if(!el.querySelector('.run-badge')){ const badge=document.createElement('div'); badge.className='run-badge'; badge.textContent='PRINTING'; el.appendChild(badge);}            
            el.classList.add('building-anim');
        });
    }

    function updateMachineStatusDots(){
        const labels = document.querySelectorAll('.scheduler-machine-label');
        labels.forEach(label=>{
            const machineId = label.getAttribute('data-machine');
            if(!machineId) return;
            const indicator = label.querySelector('.machine-status');
            if(!indicator) return; // respect existing markup only
            const jobs = document.querySelectorAll(`.job-block[data-machine='${machineId}'], .job-block[data-machine-id='${machineId}']`);
            let status='idle';
            jobs.forEach(j=>{
                const s=(j.getAttribute('data-status')||'').toLowerCase();
                if(s.includes('progress') || s.includes('building')) status='active';
                else if(status!=='active' && s.includes('scheduled')) status='scheduled';
            });
            indicator.classList.remove('status-active','status-scheduled','status-idle','pulse');
            indicator.classList.add('status-'+status);
            if(status==='active') indicator.classList.add('pulse');
            indicator.title = status==='active'? 'Machine Active' : status==='scheduled'? 'Jobs Scheduled' : 'Idle';
        });
    }

    setInterval(updateMachineStatusDots, 20000);
    document.addEventListener('scheduler:jobsHydrated', ()=>{enhanceLegacyJobBlocks(); updateMachineStatusDots();});
    enhanceLegacyJobBlocks();
    updateMachineStatusDots();

    console.log('[Scheduler UI] Loaded build (modal form logic delegated)');
})();
