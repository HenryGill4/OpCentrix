// masterPartForm.js
// Extracted logic from _MasterPartForm.cshtml
// Provides initialization for the Master Part form modal.

(function(){
    if(window.__MasterPartFormScriptLoaded){
        console.log('[MASTER-PART-FORM] Script already loaded (guard)');
    } else {
        console.log('[MASTER-PART-FORM] Loading script and installing observers');
    }
    if(window.__MasterPartFormScriptLoaded) return;
    window.__MasterPartFormScriptLoaded = true;

    function initMasterPartForm(){
        const form = document.getElementById('masterPartForm');
        if(!form){
            console.log('[MASTER-PART-FORM] initMasterPartForm: form not found');
            return;
        }
        if(form.__initialized){
            console.log('[MASTER-PART-FORM] initMasterPartForm: already initialized');
            return; // guard
        }
        form.__initialized = true;

        'use strict';
        console.log('?? [MASTER-PART-FORM] Init start');

        const formState = { selectedStages:new Map(), totalHours:0, totalCost:0, isValid:false };
        const batchStages = ['SLS Printing','EDM Operations'];

        function applyStageState(card){
            const checkbox = card.querySelector('.stage-checkbox');
            const isSelected = !!checkbox?.checked;
            const isBatch = card.dataset.stageBatch === 'true';
            const hoursInput = card.querySelector('.stage-hours');
            const cycleInput = card.querySelector('.stage-cycle');
            const config = card.querySelector('.stage-config');
            const orderWrap = card.querySelector('.execution-order');
            card.classList.toggle('border-primary', isSelected);
            card.classList.toggle('shadow-sm', isSelected);
            card.classList.toggle('border-light', !isSelected);

            if(isBatch){
                if(hoursInput) hoursInput.disabled = !isSelected;
            } else if(cycleInput){
                cycleInput.readOnly = !isSelected;
                cycleInput.classList.toggle('cycle-readonly', !isSelected);
            }
            if(config) config.style.display = isSelected ? 'block':'none';
            if(orderWrap) orderWrap.style.display = isSelected ? 'block':'none';
        }

        function refreshAllStageStates(){
            document.querySelectorAll('.stage-card').forEach(applyStageState);
        }

        function getHourlyRateForStage(name){
            const r={ 'SLS Printing':125,'Heat Treatment':85,'CNC Machining':95,'EDM Operations':110,'Assembly':75,'Finishing':65,'Quality Inspection':85 };return r[name]||85;
        }

        function updateStageSelection(){
            const selectedIds=[]; const estimatedHours=[];
            formState.selectedStages.clear(); formState.totalHours=0; formState.totalCost=0;
            document.querySelectorAll('.stage-checkbox:checked').forEach(cb=>{
                const card=cb.closest('.stage-card');
                const stageId=parseInt(cb.value); const stageName=cb.dataset.stageName; const isBatch=batchStages.includes(stageName);
                let hours=0; if(isBatch){ const h=card.querySelector('.stage-hours'); hours=parseFloat(h?.value)||0; } else { const hidden=card.querySelector('.stage-hours'); hours=parseFloat(hidden?.value)||0; }
                const order=parseInt(card.querySelector('.execution-order-input')?.value)||1;
                const rate=getHourlyRateForStage(stageName); const cost=hours*rate;
                selectedIds.push(stageId); estimatedHours.push(hours); formState.selectedStages.set(stageId,{name:stageName,hours,order,cost,isBatch});
                formState.totalHours+=hours; formState.totalCost+=cost;
            });
            const selIdsEl=document.getElementById('selectedStageIds'); if(selIdsEl) selIdsEl.value=selectedIds.join(',');
            const estHoursEl=document.getElementById('stageEstimatedHours'); if(estHoursEl) estHoursEl.value=estimatedHours.join(',');
            updateStageSummary(); updateTimeEstimates(); toggleSlsPartsPerBuild(); validateForm();
        }

        function recalcPerPartStages(){
            const partsPerBuild = parseInt(document.getElementById('partsPerBuild')?.value)||1;
            document.querySelectorAll('.stage-checkbox:checked').forEach(cb=>{
                const card=cb.closest('.stage-card');
                if(card.dataset.stageBatch==='true') return;
                const cycle=card.querySelector('.stage-cycle'); if(!cycle) return;
                const minutes=parseFloat(cycle.value)||0; const totalHours=(minutes*partsPerBuild)/60.0;
                const hoursHidden=card.querySelector('.stage-hours'); if(hoursHidden) hoursHidden.value=totalHours.toFixed(2);
                const display=card.querySelector('.total-hours-display'); if(display) display.textContent=`Total: ${totalHours.toFixed(2)}h`;
            });
            updateStageSelection();
        }

        function updateStageSummary(){
            const summary=document.getElementById('stageSummary'); const content=document.getElementById('stageSummaryContent');
            if(!summary||!content) return;
            if(formState.selectedStages.size===0){ summary.style.display='none'; return; }
            summary.style.display='block';
            const ordered=[...formState.selectedStages.entries()].sort((a,b)=>a[1].order-b[1].order);
            let html='<div class="d-flex flex-wrap gap-2">';
            ordered.forEach(([id,s])=>{ const badge=getBadgeClassForStage(s.name); html+=`<span class="badge ${badge} d-flex align-items-center"><span class=me-1>${s.order}.</span><strong>${s.name}</strong><span class=ms-1>(${s.hours.toFixed(2)}h)</span><span class=ms-1 badge bg-light text-dark>${s.isBatch?'B':'P'}</span></span>`; });
            html+='</div>'; content.innerHTML=html;
            const totalStages=document.getElementById('totalStages'); if(totalStages) totalStages.textContent=formState.selectedStages.size;
            const totalTime=document.getElementById('totalTime'); if(totalTime) totalTime.textContent=`${formState.totalHours.toFixed(2)}h`;
            const totalCost=document.getElementById('totalCost'); if(totalCost) totalCost.textContent=`$${formState.totalCost.toFixed(0)}`;
        }

        function getBadgeClassForStage(name){ const c={ 'SLS Printing':'bg-primary','Heat Treatment':'bg-danger','CNC Machining':'bg-success','EDM Operations':'bg-warning text-dark','Assembly':'bg-info','Finishing':'bg-secondary','Quality Inspection':'bg-dark'}; return c[name]||'bg-secondary'; }
        function updateStageCount(){ const c=document.querySelectorAll('.stage-checkbox:checked').length; const b=document.getElementById('selectedStageCount'); if(b){ b.textContent=c; b.className=c?"badge bg-primary ms-1":"badge bg-secondary ms-1"; } }

        function updateTimeEstimates(){
            const hidden=document.getElementById('SingleStackDurationHoursHidden');
            if(hidden){ hidden.value=formState.totalHours.toFixed(2); }
            const calcSpan=document.getElementById('calculatedBuildDuration');
            if(calcSpan) calcSpan.textContent = formState.totalHours.toFixed(1)+ 'h';
            const h=formState.totalHours; let lbl,cls; if(h===0){lbl='--';cls='bg-secondary';} else if(h<=4){lbl='Simple';cls='bg-success';} else if(h<=12){lbl='Medium';cls='bg-warning text-dark';} else if(h<=24){lbl='Complex';cls='bg-danger';} else {lbl='Very Complex';cls='bg-dark';}
            const complexity=document.getElementById('complexityDisplay'); if(complexity){ complexity.className='badge '+cls; complexity.textContent=lbl+(h?` (${h.toFixed(1)}h)`:''); }
        }

        function validateForm(){
            const pn=document.querySelector('input[name="MasterPart.PartNumber"]')?.value.trim();
            const nm=document.querySelector('input[name="MasterPart.Name"]')?.value.trim();
            const mat=document.querySelector('select[name="MasterPart.Material"]')?.value;
            const app=document.querySelector('select[name="MasterPart.ManufacturingApproach"]')?.value;
            const ok=pn&&nm&&mat&&app&&formState.selectedStages.size>0;
            formState.isValid=!!ok; const btn=document.getElementById('submitBtn'); if(btn){ btn.disabled=!ok; btn.className= ok? 'btn btn-primary':'btn btn-secondary'; }
            updateValidationFeedback(); return ok;
        }

        function updateValidationFeedback(){
            const issues=[]; if(!document.querySelector('input[name="MasterPart.PartNumber"]')?.value.trim()) issues.push('Part Number is required'); if(!document.querySelector('input[name="MasterPart.Name"]')?.value.trim()) issues.push('Part Name is required'); if(!document.querySelector('select[name="MasterPart.Material"]')?.value) issues.push('Material selection is required'); if(!document.querySelector('select[name="MasterPart.ManufacturingApproach"]')?.value) issues.push('Manufacturing Approach is required'); if(formState.selectedStages.size===0) issues.push('At least one production stage must be selected'); const div=document.querySelector('[asp-validation-summary="All"]'); if(div){ if(issues.length){ div.innerHTML='<ul>'+issues.map(i=>`<li>${i}</li>`).join('')+'</ul>'; div.classList.remove('d-none'); } else div.classList.add('d-none'); }
        }

        const manufacturingSelect=document.getElementById('manufacturingApproachSelect');
        function updateManufacturingDisplay(){ if(!manufacturingSelect) return; const infoMap={ 'SLS-Based':'Primary: SLS 3D Printing ? Post-Processing','CNC-Based':'Primary: CNC Machining from Stock Material','Hybrid-Approach':'Hybrid: SLS + CNC / EDM mix','Traditional-Machining':'Conventional Manufacturing'}; const info=document.getElementById('manufacturingApproachInfo'); if(info) info.textContent=infoMap[manufacturingSelect.value]||''; toggleSlsPartsPerBuild(); }
        function toggleSlsPartsPerBuild(){
            const wrap=document.getElementById('slsPartsPerBuildWrapper'); if(!wrap) return;
            const approachVal=manufacturingSelect?.value||'';
            const hasSlsStage=[...formState.selectedStages.values()].some(s=>s.name==='SLS Printing');
            const show = approachVal==='SLS-Based' || approachVal==='Hybrid-Approach' || hasSlsStage;
            wrap.style.display = show? 'block':'none';
            if(show){ recalcPerPartStages(); }
        }
        manufacturingSelect?.addEventListener('change', updateManufacturingDisplay);

        document.querySelectorAll('.stage-checkbox').forEach(cb=> cb.addEventListener('change', ()=>{ applyStageState(cb.closest('.stage-card')); recalcPerPartStages(); updateStageCount(); toggleSlsPartsPerBuild(); }) );
        document.querySelectorAll('.stage-cycle').forEach(inp=> inp.addEventListener('input', recalcPerPartStages));
        document.querySelectorAll('.stage-hours').forEach(inp=> inp.addEventListener('input', ()=>{ updateStageSelection(); }));
        document.querySelectorAll('.execution-order-input').forEach(inp=> inp.addEventListener('input', updateStageSelection));
        document.getElementById('partsPerBuild')?.addEventListener('input', recalcPerPartStages);
        ['MasterPart.PartNumber','MasterPart.Name','MasterPart.Material','MasterPart.ManufacturingApproach'].forEach(n=>{ const f=document.querySelector(`[name="${n}"]`); f?.addEventListener('input', validateForm); f?.addEventListener('change', validateForm); });

        form.addEventListener('submit', e=>{ e.preventDefault(); if(!validateForm()) return; const btn=document.getElementById('submitBtn'); if(!btn) return; const orig=btn.innerHTML; btn.innerHTML='<i class="fas fa-spinner fa-spin me-1"></i>Saving...'; btn.disabled=true; const fd=new FormData(form); const selectedIds=[...formState.selectedStages.keys()]; const est=[...formState.selectedStages.values()].map(s=>s.hours); fd.set('SelectedStageIds',selectedIds.join(',')); fd.set('StageEstimatedHours',est.join(',')); fd.set('MasterPart.SingleStackDurationHours', formState.totalHours.toFixed(2)); fetch(form.action,{method:'POST',body:fd,headers:{'X-Requested-With':'XMLHttpRequest','Accept':'application/json,text/html'}}).then(r=>{ const ct=r.headers.get('content-type')||''; return ct.includes('application/json')?r.json():r.text(); }).then(data=>{ if(typeof data==='object'&&data.success){ const modal=document.getElementById('partModal'); if(modal&&window.bootstrap){ const m=bootstrap.Modal.getInstance(modal); m?.hide(); } window.showToast?window.showToast('success',data.message):alert(data.message); setTimeout(()=>window.location.reload(),800); } else if(typeof data==='string'){ const modalContent=document.querySelector('#partModal .modal-content'); if(modalContent) modalContent.innerHTML=data; } else throw new Error('Unexpected response'); }).catch(err=>{ console.error(err); window.showToast?window.showToast('error','Error saving master part'):alert(err.message); }).finally(()=>{ btn.innerHTML=orig; btn.disabled=false; }); });

        refreshAllStageStates();
        recalcPerPartStages();
        updateStageSelection();
        updateStageCount();
        validateForm();
        if(manufacturingSelect?.value) updateManufacturingDisplay(); else toggleSlsPartsPerBuild();
        setTimeout(()=>{ const first=document.querySelector('input[name="MasterPart.PartNumber"]'); if(first && !first.value) first.focus(); },300);

        // ==================== NEW SLS BUILD CONFIG LOGIC ====================
        (function(){
            const state = {
                enabled: { double:false, triple:false },
                parts: { single:1, double:null, triple:null },
                durations: { single:null, double:null, triple:null },
                stageEstimateSingle: 0,
                approach: document.getElementById('manufacturingApproachSelect')?.value || ''
            };

            const card = document.getElementById('slsBuildConfigCard');
            if(!card) { console.log('[MASTER-PART-FORM] SLS build config card not found'); return; }
            const ppbSingle = document.getElementById('ppbSingle');
            const ppbDouble = document.getElementById('ppbDouble');
            const ppbTriple = document.getElementById('ppbTriple');
            const durSingle = document.getElementById('durSingle');
            const durDouble = document.getElementById('durDouble');
            const durTriple = document.getElementById('durTriple');
            const enableDouble = document.getElementById('enableDouble');
            const enableTriple = document.getElementById('enableTriple');
            const hpSingle = document.getElementById('hpSingle');
            const hpDouble = document.getElementById('hpDouble');
            const hpTriple = document.getElementById('hpTriple');
            const diffValue = document.getElementById('diffValue');
            const diffBadge = document.getElementById('diffBadge');
            const stageEstimateDisplay = document.getElementById('stageEstimateDisplay');
            const observedSingleDisplay = document.getElementById('observedSingleDisplay');
            const stageEstimateHidden = document.getElementById('StageEstimateSingleHidden');

            function showCardIfNeeded(){
                const vis = (state.approach === 'SLS-Based' || state.approach === 'Hybrid-Approach');
                card.style.display = vis ? 'block':'none';
            }

            function recalcStageEstimate(){
                let total = 0;
                document.querySelectorAll('.stage-card').forEach(card=>{
                    const selected = card.querySelector('.stage-checkbox')?.checked;
                    if(!selected) return;
                    const hours = parseFloat(card.querySelector('.stage-hours')?.value)||0; total += hours;
                });
                state.stageEstimateSingle = total;
                if(stageEstimateDisplay) stageEstimateDisplay.textContent = total ? total.toFixed(2)+'h':'--';
                if(stageEstimateHidden) stageEstimateHidden.value = total ? total.toFixed(2) : '';
                updateDiff();
                updateComplexity();
            }

            function updateDerivedHoursPerPart(){
                function calc(dur, parts){ return (dur && parts) ? (dur/parts).toFixed(2) : ''; }
                if(hpSingle) hpSingle.value = calc(state.durations.single, state.parts.single);
                if(hpDouble) hpDouble.value = state.enabled.double ? calc(state.durations.double, state.parts.double):'';
                if(hpTriple) hpTriple.value = state.enabled.triple ? calc(state.durations.triple, state.parts.triple):'';
            }

            function updateDiff(){
                const est = state.stageEstimateSingle;
                const obs = state.durations.single;
                if(observedSingleDisplay) observedSingleDisplay.textContent = obs? obs.toFixed(2)+'h':'--';
                if(!diffValue || !diffBadge){ return; }
                if(!est || !obs){ diffValue.textContent='--'; diffBadge.textContent='N/A'; diffBadge.className='badge bg-secondary'; return; }
                const diff = obs - est; const pct = est>0 ? (diff/est)*100 : 0;
                diffValue.textContent = `${diff>=0?'+':''}${diff.toFixed(2)}h (${pct.toFixed(1)}%)`;
                let cls='bg-secondary';
                if(Math.abs(pct) <=10) cls='bg-secondary'; else if(pct>10) cls='bg-warning text-dark'; else if(pct<-10) cls='bg-info text-dark';
                diffBadge.className = 'badge '+cls; diffBadge.textContent = pct.toFixed(1)+'%';
            }

            function updateComplexity(){
                const hours = state.durations.single || state.stageEstimateSingle || 0;
                const badge = document.getElementById('complexityDisplay');
                if(!badge) return;
                let label='--', cls='bg-secondary';
                if(hours>0){ if(hours<=4){label='Simple';cls='bg-success';} else if(hours<=12){label='Medium';cls='bg-warning text-dark';} else if(hours<=24){label='Complex';cls='bg-danger';} else {label='Very Complex';cls='bg-dark';} }
                badge.className='badge '+cls; badge.textContent=label + (hours?` (${hours.toFixed(1)}h)`:'');
            }

            function validate(){
                const issues=[]; const visible = card.style.display !== 'none';
                if(visible){
                    if(!state.parts.single || state.parts.single<1) issues.push('Single parts/build required');
                    if(!state.durations.single || state.durations.single<=0) issues.push('Observed single duration required');
                    if(state.enabled.double){ if(!state.parts.double || state.parts.double<1) issues.push('Double parts/build required'); if(!state.durations.double || state.durations.double<=0) issues.push('Double duration required'); }
                    if(state.enabled.triple){ if(!state.parts.triple || state.parts.triple<1) issues.push('Triple parts/build required'); if(!state.durations.triple || state.durations.triple<=0) issues.push('Triple duration required'); }
                }
                const summary = document.querySelector('[asp-validation-summary="All"]');
                if(summary){ if(issues.length){ summary.innerHTML='<ul>'+issues.map(i=>`<li>${i}</li>`).join('')+'</ul>'; summary.classList.remove('d-none'); } else if(!summary.querySelector('li')) { summary.classList.add('d-none'); } }
                return issues.length===0;
            }

            function syncFormHiddenFields(formData){
                formData.set('MasterPart.PartsPerBuildSingle', state.parts.single||1);
                formData.set('MasterPart.EnableDoubleStack', state.enabled.double);
                formData.set('MasterPart.EnableTripleStack', state.enabled.triple);
                formData.set('MasterPart.PartsPerBuildDouble', state.enabled.double? (state.parts.double||'') : '');
                formData.set('MasterPart.PartsPerBuildTriple', state.enabled.triple? (state.parts.triple||'') : '');
                formData.set('MasterPart.SingleStackDurationHours', state.durations.single||'');
                formData.set('MasterPart.DoubleStackDurationHours', state.enabled.double? (state.durations.double||'') : '');
                formData.set('MasterPart.TripleStackDurationHours', state.enabled.triple? (state.durations.triple||'') : '');
                formData.set('MasterPart.StageEstimateSingle', state.stageEstimateSingle||'');
            }

            function wire(){
                if(enableDouble){ enableDouble.addEventListener('change',()=>{ state.enabled.double=enableDouble.checked; if(ppbDouble) ppbDouble.disabled=!state.enabled.double; if(durDouble) durDouble.disabled=!state.enabled.double; updateDerivedHoursPerPart(); validate(); }); state.enabled.double=enableDouble.checked; }
                if(enableTriple){ enableTriple.addEventListener('change',()=>{ state.enabled.triple=enableTriple.checked; if(ppbTriple) ppbTriple.disabled=!state.enabled.triple; if(durTriple) durTriple.disabled=!state.enabled.triple; updateDerivedHoursPerPart(); validate(); }); state.enabled.triple=enableTriple.checked; }
                [ppbSingle,ppbDouble,ppbTriple].forEach(el=> el && el.addEventListener('input',()=>{ const id=el.id; const v=parseInt(el.value)||null; if(id==='ppbSingle'){state.parts.single=v||1;} if(id==='ppbDouble'){state.parts.double=v;} if(id==='ppbTriple'){state.parts.triple=v;} updateDerivedHoursPerPart(); recalcStageEstimate(); validate(); }));
                [durSingle,durDouble,durTriple].forEach(el=> el && el.addEventListener('input',()=>{ const v=parseFloat(el.value)||null; if(el.id==='durSingle'){state.durations.single=v;} if(el.id==='durDouble'){state.durations.double=v;} if(el.id==='durTriple'){state.durations.triple=v;} updateDerivedHoursPerPart(); updateDiff(); updateComplexity(); validate(); }));
                document.getElementById('manufacturingApproachSelect')?.addEventListener('change', e=>{ state.approach=e.target.value; showCardIfNeeded(); validate(); });
                document.addEventListener('input', e=>{ if(e.target.classList && (e.target.classList.contains('stage-hours')|| e.target.classList.contains('stage-cycle')|| e.target.classList.contains('stage-checkbox'))){ setTimeout(()=>{ recalcStageEstimate(); },150); } });
                recalcStageEstimate(); updateDiff(); updateComplexity(); updateDerivedHoursPerPart(); showCardIfNeeded();
            }

            state.parts.single = parseInt(ppbSingle?.value)||1;
            state.durations.single = parseFloat(durSingle?.value)||null;
            if(state.enabled.double){ state.parts.double = parseInt(ppbDouble?.value)||null; state.durations.double = parseFloat(durDouble?.value)||null; }
            if(state.enabled.triple){ state.parts.triple = parseInt(ppbTriple?.value)||null; state.durations.triple = parseFloat(durTriple?.value)||null; }
            wire();

            form.addEventListener('submit', function(ev){
                const fd = new FormData(form);
                syncFormHiddenFields(fd);
                if(!validate()){ ev.preventDefault(); return false; }
            }, { capture:true });
        })();

        console.log('? [MASTER-PART-FORM] Init complete');
    }

    // Expose for manual re-init after dynamic loads
    window.initMasterPartForm = initMasterPartForm;

    function attemptInit(){
        console.log('[MASTER-PART-FORM] attemptInit called');
        initMasterPartForm();
    }

    if(document.readyState === 'loading'){
        document.addEventListener('DOMContentLoaded', attemptInit);
    } else {
        attemptInit();
    }

    // Observe DOM for dynamic injection
    const observer = new MutationObserver(()=>{
        const form = document.getElementById('masterPartForm');
        if(form && !form.__initialized){
            console.log('[MASTER-PART-FORM] MutationObserver: form detected, initializing');
            initMasterPartForm();
        }
    });
    observer.observe(document.documentElement, { childList:true, subtree:true });
})();
