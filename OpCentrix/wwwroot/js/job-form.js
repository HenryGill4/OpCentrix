// Unified OpCentrix Scheduler Job Form Logic (enhanced recommendation logic + styled recommendation badge + manual override)
(function(){
  window.OpCentrixScheduler = window.OpCentrixScheduler || {};
  const LOG = '[JobFormUnified]';
  let globalObserver = null;

  if (typeof window.closeJobModal !== 'function') {
    window.closeJobModal = function(){
      const c=document.getElementById('modal-container');
      if(c){ c.classList.add('hidden'); c.style.display='none'; c.innerHTML=''; }
      document.body.style.overflow='';
    };
  }

  // --- Added: Material code helpers (migrated from scheduler-addjob-modal.js) ---
  function normalizeCode(raw){ return (raw||'').toUpperCase().replace(/[^A-Z0-9]/g,''); }
  function deriveCode(friendly){
    const u = (friendly||'').toUpperCase();
    if(u.includes('INCONEL 718')) return 'IN718';
    if(u.includes('INCONEL 625')) return 'IN625';
    if(u.includes('ELI') || u.includes('GRADE 23')) return 'TI64-G23';
    if(u.includes('GRADE 5')) return 'TI64-G5';
    if(u.includes('ALSI10')) return 'ALSI10MG';
    if(u.includes('316L')) return 'SS316L';
    return normalizeCode(u);
  }
  const codeToFriendly = {
    'IN718':'Inconel 718',
    'IN625':'Inconel 625',
    'TI64-G5':'Ti-6Al-4V Grade 5',
    'TI64G5':'Ti-6Al-4V Grade 5',
    'TI64-G23':'Ti-6Al-4V ELI Grade 23',
    'TI64G23':'Ti-6Al-4V ELI Grade 23',
    'ALSI10MG':'AlSi10Mg',
    'SS316L':'316L Stainless Steel'
  };

  // Inject once-off styles for recommendation badge & active chip enhancements
  function ensureStackStyles(){
    if(window.__ocStackStylesInjected) return; window.__ocStackStylesInjected=true;
    const css = `#stacking-section{position:relative;}\n#stack-recommend{display:flex;flex-direction:row;align-items:center;gap:.45rem;font-size:.6rem;font-weight:600;background:linear-gradient(135deg,#f5f7ff,#eef6ff);padding:.45rem .65rem;border-radius:16px;color:#183153;border:1px solid #d8e1ff;box-shadow:0 1px 2px rgba(0,0,0,.05),0 0 0 2px rgba(255,255,255,.8) inset;line-height:1.2;flex-wrap:wrap;position:relative;}\n#stack-recommend.compact{max-width:260px;}\n#stack-recommend .rec-head{font-weight:700;color:#3949ab;display:inline-flex;align-items:center;gap:.25rem;}\n#stack-recommend .rec-head:before{content:'\u2728';font-size:.75rem;color:#6366f1;filter:drop-shadow(0 0 2px #fff);}\n#stack-recommend .dot{width:4px;height:4px;border-radius:50%;background:#6366f1;display:inline-block;opacity:.65;}\n#stack-recommend .metric{white-space:nowrap;display:inline-flex;align-items:center;gap:.25rem;}\n#stack-recommend .delta-bad{color:#b91c1c;}\n#stack-recommend .delta-good{color:#027a48;}\n#stack-recommend .apply-rec{margin-left:.25rem;background:#4338ca;color:#fff;border:none;font-size:.55rem;font-weight:600;padding:.3rem .55rem;border-radius:12px;cursor:pointer;display:inline-flex;align-items:center;gap:.25rem;box-shadow:0 1px 1px rgba(0,0,0,.15);}\n#stack-recommend .apply-rec:hover{background:#3730a3;}\n#stack-recommend.override-active{border-color:#fcd34d;background:linear-gradient(135deg,#fffbea,#fef9c3);}\n#stack-recommend.override-active .rec-head:before{content:'\u26A0';color:#d97706;}\n.stack-chip{position:relative;transition:.15s background,.15s color,.15s border-color;}\n.stack-chip.recommended:not(.active){border-color:#6366f1;background:#eef2ff;color:#3730a3;}\n.stack-chip.recommended:not(.active)::after{content:'?';font-size:.55rem;color:#6366f1;position:absolute;top:-4px;right:-4px;background:#fff;border:1px solid #6366f1;border-radius:50%;padding:2px;}\n.stack-chip.active{background:#4f46e5 !important;color:#fff !important;border-color:#4338ca !important;}\n.stack-chip.active .h{color:#fff;}\n.stack-chip .h{font-weight:500;color:#475569;}\n#stack-override-banner{animation:fadeIn .25s ease;}\n@keyframes fadeIn{from{opacity:0;transform:translateY(-2px);}to{opacity:1;transform:translateY(0);}}\n#stacking-section .stack-summary-pill{display:flex;align-items:center;gap:.75rem;}#stacking-section .stack-summary-pill span{display:inline-flex;align-items:center;gap:.25rem;}#stack-recommend .warn{color:#b45309;}#stack-recommend .ok{color:#059669;}#stack-recommend .idle{color:#0369a1;}`;
    const style=document.createElement('style'); style.textContent=css; document.head.appendChild(style);
  }

  function obtainShiftFeedbackHost(){
    let host = document.getElementById('shift-feedback');
    if(!host){
      const stacking = document.getElementById('stacking-section');
      if(stacking){
        host = document.createElement('div');
        host.id='shift-feedback';
        host.style.margin='.55rem 0 0';
        host.style.fontSize='.55rem';
        host.style.fontWeight='600';
        host.style.borderRadius='6px';
        host.style.padding='.4rem .5rem';
        host.style.lineHeight='1.3';
        stacking.appendChild(host);
      }
    }
    return host;
  }
  function setShiftFeedback(text, severity){
    const host = obtainShiftFeedbackHost();
    if(!host) return; 
    if(!text){ host.innerHTML=''; host.style.display='none'; return; }
    host.style.display='block';
    if(severity==='error'){
      host.style.background='#fef2f2'; host.style.border='1px solid #fecaca'; host.style.color='#991b1b';
    } else if(severity==='warn'){
      host.style.background='#fffbeb'; host.style.border='1px solid #fcd34d'; host.style.color='#92400e';
    } else {
      host.style.background='#f0f9ff'; host.style.border='1px solid #bae6fd'; host.style.color='#075985';
    }
    host.textContent = text;
  }

  async function fetchShiftAnalysis(machineId,startIso,duration){
    try{ const url = `/Scheduler?handler=ShiftAnalysis&machineId=${encodeURIComponent(machineId)}&start=${encodeURIComponent(startIso)}&durationHours=${duration}`; const res = await fetch(url); if(!res.ok) return null; return await res.json(); }catch(e){ console.warn('[JobFormUnified] shift analysis failed', e); return null; }
  }

  OpCentrixScheduler.deleteJobWithToken = function(jobId){
    if(!jobId) return;
    if(!confirm('Are you sure you want to delete this job? This action cannot be undone.')) return;
    const token = (document.querySelector('input[name="__RequestVerificationToken"]')||{}).value;
    if(!token){ alert('Security token not found. Refresh and try again.'); return; }
    if(typeof htmx==='undefined'){ alert('HTMX not loaded'); return; }
    htmx.ajax('POST', `/Scheduler?handler=DeleteJob&id=${jobId}`, { headers:{ 'RequestVerificationToken': token, 'X-Requested-With': 'XMLHttpRequest' }, values:{ '__RequestVerificationToken': token, id: jobId }, target:'#modal-container', swap:'innerHTML' })
      .then(()=>{ if(window.closeJobModal) window.closeJobModal(); if(window.showSuccessNotification) window.showSuccessNotification('Job deleted successfully!'); try{ localStorage.removeItem('schedulerSelectMode'); }catch{} setTimeout(()=>window.location.reload(), 400); })
      .catch(()=>{ alert('Error deleting job'); });
  };

  function ensureHidden(form, id, name){ let el = form.querySelector('#'+id); if(!el){ el=document.createElement('input'); el.type='hidden'; el.id=id; el.name=name||id; form.appendChild(el);} else if(name && el.name!==name){ el.name=name; } return el; }
  function findJobForm(root){ if(!root) root=document; if(root.tagName==='FORM' && root.id==='job-form') return root; if(root.querySelector){ const f=root.querySelector('#job-form'); if(f) return f;} return document.getElementById('job-form'); }
  function installGlobalObserver(){ if(globalObserver) return; const container=document.getElementById('modal-container')||document.body; globalObserver=new MutationObserver(muts=>{ muts.forEach(m=> m.addedNodes.forEach(n=>{ if(n.nodeType===1){ const f=findJobForm(n); if(f && f.dataset.ocInit!=='1'){ console.log(LOG,'Observer detected new form, initializing'); OpCentrixScheduler.initializeAddJobModal(f); }}}));}); globalObserver.observe(container,{childList:true,subtree:true}); console.log(LOG,'Global observer installed'); }

  // Scoring weight constants
  const CHANGEOVER_HOURS = 3.0;
  const W_EFFICIENCY = 1.0;
  const W_IDLE_PENALTY = 0.15;
  const W_SHIFT_UTIL = 0.25;

  class JobFormHandler {
    constructor(form){ this.form=form; this.state={manualQtyChange:false,hiddenStackLevel:null,lastAnalysisKey:null,userOverrode:false}; this.elements={}; form.__handlerInstance=this; }
    initialize(){ ensureStackStyles(); this.cacheElements(); this.wireEvents(); this.applyMachineMode(); this.checkPreSelectedMasterPart(); this.rehydrateFromHiddenFields(); this.recommendStackLevel(true); this.updateStackSummary(); this.materialFilterAndAutoSelect(); this.patchSubmit(); }
    cacheElements(){ const f=this.form; this.elements={ qtyInput:f.querySelector('input[name="Quantity"]'), machineSelect:f.querySelector('#machine-select'), masterPartSelect:f.querySelector('#masterpart-select'), startInput:f.querySelector('#start-input'), endInput:f.querySelector('#end-input'), durationDisplay:f.querySelector('#duration-display'), stackingSection:f.querySelector('#stacking-section'), stackChips:f.querySelectorAll('.stack-chip'), stackRecommend:f.querySelector('#stack-recommend'), stackSummary:f.querySelector('#stack-summary'), stackOverrideBanner:f.querySelector('#stack-override-banner'), slsMaterialSelect:f.querySelector('#sls-material-select'), hiddenFields:{ stackLevel:f.querySelector('#hidden-stack-level'), partsPerBuild:f.querySelector('#hidden-parts-per-build'), stackDuration:f.querySelector('#hidden-stack-duration'), partId:f.querySelector('#hidden-part-id'), partNumber:f.querySelector('#hidden-part-number'), suggestedStackLevel:f.querySelector('#hidden-suggested-stack-level') } }; this.state.hiddenStackLevel=this.elements.hiddenFields.stackLevel?.value||null; }
    wireEvents(){ const {machineSelect,masterPartSelect,startInput,stackChips,qtyInput,stackRecommend}=this.elements; if(machineSelect) machineSelect.addEventListener('change',()=>{ this.applyMachineMode(); this.materialFilterAndAutoSelect(); this.recommendStackLevel(!this.state.userOverrode); this.requestShiftAnalysis(); }); if(masterPartSelect) masterPartSelect.addEventListener('change',()=>{ this.state.userOverrode=false; this.handleMasterPartChange(); }); if(startInput) startInput.addEventListener('change',()=>{ this.recommendStackLevel(!this.state.userOverrode); this.requestShiftAnalysis(); }); stackChips.forEach(ch=> ch.addEventListener('click',()=> { this.selectStack(ch,true); this.recommendStackLevel(false); })); if(qtyInput) qtyInput.addEventListener('input',()=>{ this.state.manualQtyChange=true; this.updateStackSummary(); this.recommendStackLevel(false); }); if(stackRecommend){ stackRecommend.addEventListener('click',(e)=>{ const btn=e.target.closest('.apply-rec'); if(btn){ const lvl=stackRecommend.getAttribute('data-rec-level'); if(lvl){ const chip=this.form.querySelector(`.stack-chip[data-level='${lvl}']`); if(chip){ this.state.userOverrode=false; this.selectStack(chip,true); this.recommendStackLevel(false); } } } }); } }
    patchSubmit(){ this.form.addEventListener('submit',(e)=>{ try{ this.ensurePartBinding(); }catch{} const mpSel=this.elements.masterPartSelect; if(!mpSel || !mpSel.value){ e.preventDefault(); alert('Master Part is required.'); return false; } },{capture:true}); }
    ensurePartBinding(){ const { masterPartSelect, hiddenFields } = this.elements; if(masterPartSelect && masterPartSelect.value){ const opt=masterPartSelect.options[masterPartSelect.selectedIndex]; if(opt){ const pn=opt.getAttribute('data-part-number')||opt.textContent.trim(); if(hiddenFields.partNumber) hiddenFields.partNumber.value=pn||''; const legacyId=opt.getAttribute('data-legacy-id'); if(hiddenFields.partId){ hiddenFields.partId.value = legacyId && legacyId!=='' ? legacyId : (hiddenFields.partId.value||'0'); } } } }
    checkPreSelectedMasterPart(){ const {masterPartSelect}=this.elements; if(masterPartSelect && masterPartSelect.value) this.handleMasterPartChange(); }
    rehydrateFromHiddenFields(){ const {hiddenFields}=this.elements; if(this.state.hiddenStackLevel){ const chip=this.form.querySelector(`.stack-chip[data-level="${this.state.hiddenStackLevel}"]`); if(chip) this.selectStack(chip,false); } }
    applyMachineMode(){ const {machineSelect}=this.elements; const id=(machineSelect?.value||'').trim(); const isSLS=/(TI|INC|SLS|TRU|TRUPRINT|PRINT|ADD)/i.test(id); const slsPanel=this.form.querySelector('#sls-params'); const cncPanel=this.form.querySelector('#cnc-params'); if(slsPanel) slsPanel.style.display=isSLS?'block':'none'; if(cncPanel) cncPanel.style.display=(!isSLS && id)?'block':'none'; if(!isSLS) this.hideStacking(); }
    extractPartData(o){ return { allowStacking:o.getAttribute('data-allow-stacking')==='true', singleHours:parseFloat(o.getAttribute('data-single-hours'))||parseFloat(o.getAttribute('data-stage-single'))||null, doubleHours:parseFloat(o.getAttribute('data-double-hours'))||null, tripleHours:parseFloat(o.getAttribute('data-triple-hours'))||null, enableDouble:o.getAttribute('data-enable-double')==='true', enableTriple:o.getAttribute('data-enable-triple')==='true', partsSingle:parseInt(o.getAttribute('data-parts-single'))||1, partsDouble:parseInt(o.getAttribute('data-parts-double'))||null, partsTriple:parseInt(o.getAttribute('data-parts-triple'))||null, legacyId:o.getAttribute('data-legacy-id'), partNumber:o.getAttribute('data-part-number')||o.textContent.trim() }; }
    shouldShowStacking(pd){ return pd.allowStacking||pd.singleHours||pd.doubleHours||pd.tripleHours; }
    showStacking(pd){ const {stackingSection,stackChips}=this.elements; if(stackingSection) stackingSection.style.display='block'; const map={single:pd.singleHours,double:pd.doubleHours,triple:pd.tripleHours}; this.form.querySelectorAll('#stacking-section .h').forEach(span=>{ const key=span.getAttribute('data-h'); const val=map[key]; span.textContent=val?val.toFixed(2)+'h':'--'; }); stackChips.forEach(ch=>{ const lvl=ch.getAttribute('data-level'); if(lvl==='1') ch.removeAttribute('disabled'); else if(lvl==='2') (pd.enableDouble && pd.doubleHours)? ch.removeAttribute('disabled') : ch.setAttribute('disabled','disabled'); else if(lvl==='3') (pd.enableTriple && pd.tripleHours)? ch.removeAttribute('disabled') : ch.setAttribute('disabled','disabled'); }); }
    hideStacking(){ const {stackingSection}=this.elements; if(stackingSection) stackingSection.style.display='none'; }
    handleMasterPartChange(){ const { masterPartSelect }=this.elements; if(!masterPartSelect || !masterPartSelect.value){ this.hideStacking(); return; } const option=masterPartSelect.options[masterPartSelect.selectedIndex]; if(!option) return; const pd=this.extractPartData(option); if(this.shouldShowStacking(pd)) this.showStacking(pd); else this.hideStacking(); this.recommendStackLevel(true); this.updateStackSummary(); this.ensurePartBinding(); this.applyMachineMode(); this.requestShiftAnalysis(); }
    selectStack(chip,userAction){ if(chip.hasAttribute('disabled')) return; const { stackChips, hiddenFields, startInput, endInput, durationDisplay }=this.elements; stackChips.forEach(c=>c.classList.remove('active')); chip.classList.add('active'); const level=chip.getAttribute('data-level'); if(hiddenFields.stackLevel) hiddenFields.stackLevel.value=level; if(userAction) this.state.userOverrode=true; const hourSpan=chip.querySelector('.h'); if(hourSpan){ const hours=parseFloat(hourSpan.textContent.replace('h','').trim())||null; if(hours && startInput?.value && endInput){ const startDate=new Date(startInput.value); if(!isNaN(startDate.getTime())){ const endDate=new Date(startDate.getTime()+hours*3600*1000); endInput.value=this.toLocalDateTimeValue(endDate); if(durationDisplay) durationDisplay.textContent=hours.toFixed(1)+' h'; if(hiddenFields.stackDuration) hiddenFields.stackDuration.value=hours.toString(); } } } this.updatePartsPerBuild(level); this.syncQuantityIfNeeded(); this.showOverrideBanner(level); this.updateStackSummary(); this.ensurePartBinding(); if(userAction) this.requestShiftAnalysis(); }
    updatePartsPerBuild(level){ const { masterPartSelect, hiddenFields }=this.elements; if(!masterPartSelect) return; const opt=masterPartSelect.options[masterPartSelect.selectedIndex]; if(!opt) return; let ppb=null; if(level==='1') ppb=opt.getAttribute('data-parts-single'); else if(level==='2') ppb=opt.getAttribute('data-parts-double'); else if(level==='3') ppb=opt.getAttribute('data-parts-triple'); if(hiddenFields.partsPerBuild) hiddenFields.partsPerBuild.value=(ppb && ppb!=='0')? ppb:''; }
    syncQuantityIfNeeded(){ if(this.state.manualQtyChange) return; const { qtyInput, masterPartSelect, hiddenFields }=this.elements; if(!qtyInput || !masterPartSelect || !hiddenFields.stackLevel?.value) return; const opt=masterPartSelect.options[masterPartSelect.selectedIndex]; if(!opt) return; const lvl=hiddenFields.stackLevel.value; const attr=lvl==='1'? 'data-parts-single': lvl==='2'? 'data-parts-double':'data-parts-triple'; const partsPerBuild=parseInt(opt.getAttribute(attr))||null; if(partsPerBuild>0) qtyInput.value=partsPerBuild; }
    showOverrideBanner(level){ const { stackOverrideBanner, hiddenFields }=this.elements; if(!stackOverrideBanner) return; const suggested=hiddenFields.suggestedStackLevel?.value; stackOverrideBanner.style.display=(suggested && suggested!==level)?'block':'none'; }
    clearRecommendations(){ const { stackRecommend, hiddenFields, stackChips }=this.elements; if(stackRecommend){ stackRecommend.innerHTML=''; stackRecommend.removeAttribute('data-rec-level'); stackRecommend.classList.remove('override-active'); } if(hiddenFields.suggestedStackLevel) hiddenFields.suggestedStackLevel.value=''; stackChips.forEach(c=>c.classList.remove('recommended')); }

    recommendStackLevel(allowAutoSelect){ this.clearRecommendations(); const { startInput, stackChips, stackRecommend, hiddenFields, masterPartSelect, qtyInput }=this.elements; if(!startInput?.value || !masterPartSelect) return; const startDate=new Date(startInput.value); if(isNaN(startDate.getTime())) return; const hour=startDate.getHours(); let shiftEnd=new Date(startDate); if(hour>=6 && hour<14) shiftEnd.setHours(14,0,0,0); else if(hour>=14 && hour<22) shiftEnd.setHours(22,0,0,0); else { if(hour>=22){ shiftEnd.setDate(shiftEnd.getDate()+1); shiftEnd.setHours(6,0,0,0);} else shiftEnd.setHours(6,0,0,0);} const remainingShiftH=Math.max(0,(shiftEnd-startDate)/3600000);
      const opt = masterPartSelect.options[masterPartSelect.selectedIndex];
      const partsMap={1:parseInt(opt.getAttribute('data-parts-single'))||null,2:parseInt(opt.getAttribute('data-parts-double'))||null,3:parseInt(opt.getAttribute('data-parts-triple'))||null};
      const desiredQty = parseInt(qtyInput?.value)||partsMap[1]||1;
      const variants = Array.from(stackChips).filter(c=>!c.hasAttribute('disabled')).map(ch=>{ const lvl=parseInt(ch.getAttribute('data-level')); const hrs=parseFloat((ch.querySelector('.h')?.textContent||'').replace('h',''))||null; if(!hrs) return null; const partsPerBuild=partsMap[lvl]||desiredQty; const buildsNeeded=Math.ceil(desiredQty/partsPerBuild); const totalRunHours=hrs*buildsNeeded; const totalSetupHours=CHANGEOVER_HOURS*buildsNeeded; const effectiveHours=totalRunHours+totalSetupHours; const efficiency=(partsPerBuild*buildsNeeded)/effectiveHours; const variantEndsWithinShift=hrs<=remainingShiftH; const idleHours=variantEndsWithinShift? Math.max(0,remainingShiftH-hrs):0; const shiftUtilRatio=variantEndsWithinShift? (hrs/remainingShiftH):1; const score=(efficiency*W_EFFICIENCY)+(shiftUtilRatio*W_SHIFT_UTIL)-(idleHours*W_IDLE_PENALTY); return {chip:ch,level:lvl,hrs,partsPerBuild,efficiency,idleHours,shiftUtilRatio,score,buildsNeeded,totalRunHours,totalSetupHours}; }).filter(v=>v);
      if(!variants.length) return; variants.sort((a,b)=> (b.score - a.score) || (b.level - a.level)); const rec=variants[0]; rec.chip.classList.add('recommended'); if(hiddenFields.suggestedStackLevel) hiddenFields.suggestedStackLevel.value=rec.level.toString(); if(stackRecommend) stackRecommend.setAttribute('data-rec-level', rec.level);
      const activeChip=this.form.querySelector('.stack-chip.active'); const activeLevel=activeChip? parseInt(activeChip.getAttribute('data-level')):null; const activeVariant=variants.find(v=>v.level===activeLevel) || null;
      const shouldAuto = allowAutoSelect && !this.state.userOverrode; if(shouldAuto || !activeChip){ this.selectStack(rec.chip,false); }
      if(stackRecommend){ const effStr=rec.efficiency.toFixed(2); const idleStr=rec.idleHours>0.05? `<span class="metric idle">Idle ${rec.idleHours.toFixed(1)}h</span>` : `<span class="metric ok">No idle</span>`; const spanShift = rec.hrs<=remainingShiftH? '<span class="metric ok">In shift</span>' : '<span class="metric warn">Spans shifts</span>'; let deltaHtml=''; if(activeVariant && activeVariant.level!==rec.level){ const effDelta=((rec.efficiency-activeVariant.efficiency)/activeVariant.efficiency)*100; const effDeltaFmt=(effDelta>=0?'+':'')+effDelta.toFixed(1)+'%'; deltaHtml=`<span class='dot'></span><span class='metric ${effDelta>=0?'delta-good':'delta-bad'}'>Eff ? ${effDeltaFmt}</span>`; stackRecommend.classList.add('override-active'); } const applyBtn = (!shouldAuto && this.state.userOverrode && activeLevel!==rec.level)? `<button type='button' class='apply-rec' title='Apply recommended stack'>Use ${rec.level}x</button>` : ''; stackRecommend.innerHTML = `<span class='rec-head'>Recommended</span><span class='dot'></span><span class='metric'>${rec.level}x (${rec.partsPerBuild} parts, ${rec.hrs.toFixed(2)}h)</span><span class='dot'></span><span class='metric'>Eff ${effStr} parts/h incl 3h setup</span><span class='dot'></span>${idleStr}<span class='dot'></span>${spanShift}${deltaHtml}${applyBtn}`; }
      this.showOverrideBanner(this.elements.hiddenFields.stackLevel?.value||'');
    }

    updateStackSummary(){ const { stackSummary, hiddenFields, qtyInput }=this.elements; if(!stackSummary) return; const lvl=hiddenFields.stackLevel?.value; const ppb=hiddenFields.partsPerBuild?.value; const qty=qtyInput? parseInt(qtyInput.value)||0:0; if(lvl && qty>0){ stackSummary.style.display='block'; const lEl=stackSummary.querySelector('#stack-summary-level'); const pEl=stackSummary.querySelector('#stack-summary-ppb'); const tEl=stackSummary.querySelector('#stack-summary-total'); if(lEl) lEl.textContent=lvl+'x'; if(pEl) pEl.textContent=ppb||'-'; if(tEl) tEl.textContent=qty.toString(); } else stackSummary.style.display='none'; }

    async requestShiftAnalysis(){ const { machineSelect, startInput, endInput }=this.elements; if(!machineSelect||!startInput||!endInput) return; if(!machineSelect.value||!startInput.value||!endInput.value) return; const startIso=startInput.value+':00Z'; const startDate=new Date(startInput.value); const endDate=new Date(endInput.value); if(isNaN(startDate.getTime())||isNaN(endDate.getTime())) return; const duration=(endDate-startDate)/3600000; const key=`${machineSelect.value}|${startInput.value}|${duration.toFixed(3)}`; if(this.state.lastAnalysisKey===key) return; this.state.lastAnalysisKey=key; setShiftFeedback('Analyzing end alignment…','info'); const data=await fetchShiftAnalysis(machineSelect.value,startIso,duration); if(!data||!data.success){ setShiftFeedback('Shift alignment unavailable','warn'); return; } let severity='success'; if(!data.setupSatisfied) severity='error'; else if(data.postRunIdleHours>0.05) severity='warn'; setShiftFeedback(data.message, severity); }

    toLocalDateTimeValue(d){ const pad=n=> n<10?'0'+n:n; return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`; }

    // --- Added: material filtering & auto selection integrated ---
    getMachineMaterialCode(){ const {machineSelect} = this.elements; if(!machineSelect) return ''; const opt = machineSelect.options[machineSelect.selectedIndex]; if(!opt) return ''; const cur = (opt.getAttribute('data-current-material')||'').trim(); if(cur && cur.length <= 8) return normalizeCode(cur); return deriveCode(cur); }
    filterMasterParts(){ const { masterPartSelect } = this.elements; if(!masterPartSelect) return; const machineCode = this.getMachineMaterialCode(); const normMachine = normalizeCode(machineCode); const showAll = !normMachine; Array.from(masterPartSelect.options).forEach(o=>{ if(!o.value){ o.hidden=false; o.disabled=false; return; } const partCode = normalizeCode(o.getAttribute('data-material-code')||''); const match = showAll || !partCode || partCode===normMachine; o.hidden = !match; o.disabled = !match; }); if(masterPartSelect.selectedOptions.length && masterPartSelect.selectedOptions[0].hidden){ masterPartSelect.value=''; } }
    autoSelectSlsMaterial(){ const { slsMaterialSelect } = this.elements; if(!slsMaterialSelect) return; const code = this.getMachineMaterialCode(); if(!code) return; const friendly = codeToFriendly[code] || codeToFriendly[normalizeCode(code)] || null; if(!friendly) return; for(const opt of slsMaterialSelect.options){ if(opt.value.toLowerCase() === friendly.toLowerCase()) { opt.selected = true; break; } } }
    materialFilterAndAutoSelect(){ this.filterMasterParts(); this.autoSelectSlsMaterial(); }
  }

  OpCentrixScheduler.initializeAddJobModal = function(root){ try { const form=findJobForm(root); if(!form){ installGlobalObserver(); return; } if(form.dataset.ocInit==='1') return; form.dataset.ocInit='1'; ensureHidden(form,'hidden-part-id','PartId'); ensureHidden(form,'hidden-stack-level','StackLevel'); ensureHidden(form,'hidden-parts-per-build','PartsPerBuild'); ensureHidden(form,'hidden-stack-duration','PlannedStackDurationHours'); ensureHidden(form,'hidden-part-number','PartNumber'); ensureHidden(form,'hidden-suggested-stack-level','SuggestedStackLevel'); const handler=new JobFormHandler(form); handler.initialize(); console.log(LOG,'Initialization complete'); } catch(err){ console.error(LOG,'Initialization error', err); } };

  window.deleteJobWithToken = OpCentrixScheduler.deleteJobWithToken;
  window.deleteJob = OpCentrixScheduler.deleteJobWithToken;

  document.body.addEventListener('htmx:afterSwap', function(e){ if(e.detail && e.detail.target && e.detail.target.id === 'modal-container'){ const form=e.detail.target.querySelector('#job-form'); if(form) OpCentrixScheduler.initializeAddJobModal(form); const mc=document.getElementById('modal-container'); if(mc){ mc.style.display='flex'; mc.classList.remove('hidden'); document.body.style.overflow='hidden'; } } });

  function tryBootstrapNow(){ if(document.getElementById('job-form')) OpCentrixScheduler.initializeAddJobModal(); }
  if(document.readyState==='loading') document.addEventListener('DOMContentLoaded', tryBootstrapNow); else tryBootstrapNow();
  installGlobalObserver();
  console.log(LOG,'Module loaded');
})();
