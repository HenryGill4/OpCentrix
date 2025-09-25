// OpCentrix Scheduler Job Form JS (externalized)
// Ensures modal functions work when content is injected via HTMX
(function(){
  window.OpCentrixScheduler = window.OpCentrixScheduler || {};

  // Fallback close modal
  if (typeof window.closeJobModal !== 'function') {
    window.closeJobModal = function(){ const c=document.getElementById('modal-container'); if(c){ c.classList.add('hidden'); c.style.display='none'; c.innerHTML=''; } document.body.style.overflow=''; };
  }

  // Delete job (POST with antiforgery)
  OpCentrixScheduler.deleteJobWithToken = function(jobId){
    if(!jobId) return;
    if(!confirm('Are you sure you want to delete this job? This action cannot be undone.')) return;
    const token = (document.querySelector('input[name="__RequestVerificationToken"]')||{}).value;
    if(!token){ alert('Security token not found. Refresh and try again.'); return; }
    if(typeof htmx==='undefined'){ alert('HTMX not loaded'); return; }
    htmx.ajax('POST', `/Scheduler?handler=DeleteJob&id=${jobId}`, {
      headers:{ 'RequestVerificationToken': token, 'X-Requested-With': 'XMLHttpRequest' },
      values:{ '__RequestVerificationToken': token, id: jobId },
      target:'#modal-container', swap:'innerHTML'
    }).then(()=>{
      if(window.closeJobModal) window.closeJobModal();
      if(window.showSuccessNotification) window.showSuccessNotification('Job deleted successfully!');
      setTimeout(()=>window.location.reload(), 500);
    }).catch(()=>{ alert('Error deleting job'); });
  };

  // Core handler
  const Handler = {
    init(){
      const form = document.getElementById('job-form');
      if(!form) return; // nothing to init
      if(form.dataset.bound === 'true') return; // prevent duplicate init
      form.dataset.bound = 'true';
      this.updateDurationDisplay();
      this.setupEventListeners();
      const partSelect = document.getElementById('part-select');
      if (partSelect && partSelect.value) { this.updateJobFromPart(); } else { this.refreshStackingUI(null); }
    },
    setupEventListeners(){
      const partSelect = document.getElementById('part-select');
      const quantityInput = document.getElementById('quantity-input');
      const startInput = document.getElementById('start-input');
      const endInput = document.getElementById('end-input');
      if (partSelect) partSelect.addEventListener('change', ()=>this.updateJobFromPart());
      if (quantityInput) quantityInput.addEventListener('change', ()=>this.updateEndTimeFromQuantity());
      if (startInput) startInput.addEventListener('change', ()=>this.updateEndTimeFromStart());
      if (endInput) endInput.addEventListener('change', ()=>{ this.updateDurationDisplay(); this.checkTimeSlotAvailability(); this.handleDurationChange(); });
      document.querySelectorAll('input[name="stackOption"]').forEach(r=> r.addEventListener('change', ()=>this.onStackChange()));
      this.bindChipClicks();
    },
    bindChipClicks(){
      ['stack-single','stack-double','stack-triple'].forEach(id=>{
        const chip = document.getElementById(id); if(!chip || chip.dataset.bound==='true') return; chip.dataset.bound='true';
        const radio = chip.querySelector('input[type="radio"]');
        const activate = (evt)=>{ if(chip.dataset.disabled==='true'||!radio||radio.disabled) return; if(!radio.checked){ radio.checked=true; radio.dispatchEvent(new Event('change',{bubbles:true})); } else { this.onStackChange(); } };
        chip.addEventListener('pointerdown', activate);
        chip.addEventListener('click', activate);
        chip.addEventListener('keydown', e=>{ if(e.key==='Enter'||e.key===' ') activate(e); });
      });
    },
    onStackChange(){
      const selected = this.getSelectedStack();
      if(selected){ const qty=document.getElementById('quantity-input'); if(qty) qty.value=String(selected); }
      const dur = this.getSelectedStackDuration();
      if(!isNaN(dur)){ const h=document.getElementById('hidden-estimated-hours'); if(h) h.value=dur.toFixed(2); }
      this.updateEndTimeFromStart();
      this.highlightActiveStackChip();
    },
    getSelectedStack(){ const r=document.querySelector('input[name="stackOption"]:checked'); return r?parseInt(r.value):null; },
    getSelectedStackDuration(){
      const partSelect = document.getElementById('part-select');
      const opt = partSelect && partSelect.selectedIndex>=0 ? partSelect.options[partSelect.selectedIndex] : null; if(!opt) return NaN;
      const s=this.getSelectedStack();
      if(s===1) return parseFloat(opt.getAttribute('data-single-hours'));
      if(s===2) return parseFloat(opt.getAttribute('data-double-hours'));
      if(s===3) return parseFloat(opt.getAttribute('data-triple-hours'));
      return NaN;
    },
    refreshStackingUI(selectedOption){
      const section = document.getElementById('stacking-section'); const partSelect=document.getElementById('part-select');
      const opt = selectedOption || (partSelect && partSelect.selectedIndex>=0 ? partSelect.options[partSelect.selectedIndex] : null);
      if(!section || !opt){ if(section) section.style.display='none'; return; }
      const allow = opt.getAttribute('data-allow-stacking')==='true';
      const single=parseFloat(opt.getAttribute('data-single-hours'));
      const dbl=parseFloat(opt.getAttribute('data-double-hours'));
      const tpl=parseFloat(opt.getAttribute('data-triple-hours'));
      const setText=(id,val)=>{ const el=document.getElementById(id); if(el) el.textContent=isNaN(val)?'-- h':`${val.toFixed(2)}h`; };
      const setEnabled=(chipId,radioSel,enabled)=>{ const chip=document.getElementById(chipId); const r=chip?chip.querySelector('input'):null; if(!chip||!r) return; chip.dataset.disabled = enabled?'false':'true'; r.disabled = !enabled; chip.style.opacity = enabled?'1':'0.5'; };
      if(!allow){ section.style.display='none'; const est=parseFloat(opt.getAttribute('data-estimated-hours')); const h=document.getElementById('hidden-estimated-hours'); if(!isNaN(est)&&h) h.value=est.toFixed(2); this.highlightActiveStackChip(); return; }
      section.style.display='';
      setText('badge-single',single); setText('badge-double',dbl); setText('badge-triple',tpl);
      setEnabled('stack-single','input[value="1"]',!isNaN(single));
      setEnabled('stack-double','input[value="2"]',!isNaN(dbl));
      setEnabled('stack-triple','input[value="3"]',!isNaN(tpl));
      const r1=document.querySelector('input[name="stackOption"][value="1"]');
      const r2=document.querySelector('input[name="stackOption"][value="2"]');
      const r3=document.querySelector('input[name="stackOption"][value="3"]');
      if(!isNaN(tpl) && r3){ r3.checked=true; }
      else if(!isNaN(dbl) && r2){ r2.checked=true; }
      else if(!isNaN(single) && r1){ r1.checked=true; }
      else { section.style.display='none'; }
      this.onStackChange();
      this.bindChipClicks();
      this.autoSelectBestStack().catch(()=>{});
    },
    highlightActiveStackChip(){
      ['stack-single','stack-double','stack-triple'].forEach(id=>{ const el=document.getElementById(id); if(!el) return; const r=el.querySelector('input'); if(r&&r.checked){ el.classList.add('chip-active'); el.setAttribute('aria-pressed','true'); } else { el.classList.remove('chip-active'); el.setAttribute('aria-pressed','false'); }})
    },
    updateJobFromPart(){
      const partSelect=document.getElementById('part-select'); const opt=partSelect?.options[partSelect.selectedIndex]; if(!opt){ this.refreshStackingUI(null); return; }
      const pn=document.getElementById('hidden-part-number'); if(pn) pn.value = opt.getAttribute('data-part-number')||'';
      const slsMaterial = opt.getAttribute('data-sls-material'); const materialSelect=document.getElementById('sls-material'); if(materialSelect&&slsMaterial){ materialSelect.value=slsMaterial; }
      this.updateProcessParameters(opt);
      this.refreshStackingUI(opt);
      this.autoSelectBestStack().catch(()=>{});
    },
    updateProcessParameters(opt){
      const setVal=(id,attr,fmt)=>{ const v=opt.getAttribute(attr); const el=document.getElementById(id); if(el&&v){ el.value = fmt? fmt(v): v; }};
      setVal('laser-power','data-laser-power');
      setVal('scan-speed','data-scan-speed');
      setVal('layer-thickness','data-layer-thickness');
      setVal('hatch-spacing','data-hatch-spacing');
      setVal('build-temperature','data-build-temperature');
      setVal('powder-usage','data-powder-usage', (x)=>parseFloat(x).toFixed(2));
    },
    filterPartsByMachine(){ /* currently no filter */ },
    updateEndTimeFromStart(){
      const start=document.getElementById('start-input'); const end=document.getElementById('end-input'); const hidden=document.getElementById('hidden-estimated-hours'); if(!start||!end) return;
      const st=new Date(start.value); if(isNaN(st.getTime())) return; let hours=this.getSelectedStackDuration(); if(isNaN(hours)) hours=parseFloat(hidden?.value); if(isNaN(hours)||hours<=0) hours=8;
      const et=new Date(st.getTime() + hours*3600000); end.value = et.toISOString().slice(0,16); if(hidden) hidden.value=hours.toFixed(2); this.updateDurationDisplay();
    },
    updateEndTimeFromQuantity(){
      const qty=parseInt((document.getElementById('quantity-input')||{}).value||'0'); if(qty>=1&&qty<=3){ const r=document.querySelector(`input[name="stackOption"][value="${qty}"]`); if(r && !r.checked && !r.disabled){ r.checked=true; this.onStackChange(); return; }} this.updateEndTimeFromStart();
    },
    updateDurationDisplay(){
      const s=document.getElementById('start-input'); const e=document.getElementById('end-input'); const d=document.getElementById('duration-display'); if(!s||!e||!d) return;
      const st=new Date(s.value); const et=new Date(e.value); if(isNaN(st.getTime())||isNaN(et.getTime())) { d.textContent='Invalid dates'; return; }
      const hours=(et-st)/3600000; const hidden=document.getElementById('hidden-duration-hours'); if(hidden) hidden.value=hours.toFixed(2);
      d.textContent = hours>=24 ? `${Math.floor(hours/24)}d ${(hours%24).toFixed(1)}h` : `${hours.toFixed(1)} hours`;
      d.style.color='#374151';
    },
    checkTimeSlotAvailability(){ /* server check could be added later */ },
    handleDurationChange(){},
    suggestNextAvailableTime(){
      const machine=document.getElementById('machine-select'); const start=document.getElementById('start-input'); const dur=parseFloat((document.getElementById('hidden-duration-hours')||{}).value)||8.0; if(!machine?.value){ alert('Please select a machine first'); return; }
      fetch(`/Scheduler?handler=SuggestNextTime&machineId=${encodeURIComponent(machine.value)}&durationHours=${dur}`)
        .then(r=>r.json()).then(data=>{ if(data?.success && data.startTime){ start.value=data.startTime; const end=document.getElementById('end-input'); if(end&&data.endTime) end.value=data.endTime; this.updateDurationDisplay(); } })
        .catch(()=>{ const t=new Date(); t.setDate(t.getDate()+1); t.setHours(8,0,0,0); start.value=t.toISOString().slice(0,16); this.updateEndTimeFromStart(); });
    },
    showFormLoading(){ const btn=document.getElementById('submit-job-btn'); const txt=document.getElementById('submit-text'); const sp=document.getElementById('submit-spinner'); if(btn) { btn.disabled=true; btn.style.opacity='0.7'; } if(txt) txt.textContent='Saving...'; if(sp) sp.style.display='inline-block'; },
    hideFormLoading(){ const btn=document.getElementById('submit-job-btn'); const txt=document.getElementById('submit-text'); const sp=document.getElementById('submit-spinner'); if(btn){ btn.disabled=false; btn.style.opacity='1'; } if(txt){ txt.textContent=txt.getAttribute('data-original-text')||txt.textContent; } if(sp) sp.style.display='none'; },
    handleFormResponse(event){ this.hideFormLoading(); if(event?.detail?.xhr?.status===200){ if(window.closeJobModal) window.closeJobModal(); if(window.showSuccessNotification) window.showSuccessNotification('Job saved successfully!'); setTimeout(()=>window.location.reload(), 600);} },
    async autoSelectBestStack(){
      const machine=document.getElementById('machine-select'); const part=document.getElementById('part-select'); const section=document.getElementById('stacking-section'); if(!machine?.value||!part?.value||section?.style.display==='none') return;
      const opt = part.options[part.selectedIndex];
      const arr=[ {stack:1,hours:parseFloat(opt.getAttribute('data-single-hours')), r:document.querySelector('input[name="stackOption"][value="1"]')}, {stack:2,hours:parseFloat(opt.getAttribute('data-double-hours')), r:document.querySelector('input[name="stackOption"][value="2"]')}, {stack:3,hours:parseFloat(opt.getAttribute('data-triple-hours')), r:document.querySelector('input[name="stackOption"][value="3"]')} ].filter(x=>!isNaN(x.hours)&&x.r && !x.r.disabled);
      if(arr.length===0) return;
      const results = await Promise.all(arr.map(async a=>{ try{ const resp=await fetch(`/Scheduler?handler=SuggestNextTime&machineId=${encodeURIComponent(machine.value)}&durationHours=${a.hours}`); const j=await resp.json(); return {ok:!!j?.success, end:j?.endTime, start:j?.startTime, a}; } catch{ return {ok:false, a}; } }));
      let best=null; for(const r of results){ if(r.ok && r.end){ const score=Date.parse(r.end); if(!best||score<best.score) best={r,score}; } }
      if(!best){ arr.sort((x,y)=>x.hours-y.hours); if(!arr[0].r.checked) arr[0].r.checked=true; this.onStackChange(); return; }
      if(!best.r.a.r.checked) best.r.a.r.checked=true; this.onStackChange(); const hint=document.getElementById('stacking-hint'); if(hint && best.r.start && best.r.end){ const label=best.r.a.stack===1?'Single':best.r.a.stack===2?'Double':'Triple'; hint.textContent=`Auto-selected: ${label} (${best.r.a.hours.toFixed(2)}h) to fit shift window: ${best.r.start} ? ${best.r.end}`; }
    }
  };

  // Namespace mappings
  OpCentrixScheduler.bootstrapModal = function(){ try { Handler.init(); } catch(e){ console.warn('bootstrapModal failed', e); } };
  OpCentrixScheduler.updateJobFromPart = ()=>Handler.updateJobFromPart();
  OpCentrixScheduler.filterPartsByMachine = ()=>Handler.filterPartsByMachine();
  OpCentrixScheduler.updateEndTimeFromStart = ()=>Handler.updateEndTimeFromStart();
  OpCentrixScheduler.updateEndTimeFromQuantity = ()=>Handler.updateEndTimeFromQuantity();
  OpCentrixScheduler.updateDurationDisplay = ()=>Handler.updateDurationDisplay();
  OpCentrixScheduler.checkTimeSlotAvailability = ()=>Handler.checkTimeSlotAvailability();
  OpCentrixScheduler.handleDurationChange = ()=>Handler.handleDurationChange();
  OpCentrixScheduler.suggestNextAvailableTime = ()=>Handler.suggestNextAvailableTime();
  OpCentrixScheduler.showFormLoading = ()=>Handler.showFormLoading();
  OpCentrixScheduler.hideFormLoading = ()=>Handler.hideFormLoading();
  OpCentrixScheduler.handleFormResponse = (e)=>Handler.handleFormResponse(e);

  // Legacy globals for old inline calls
  window.deleteJobWithToken = OpCentrixScheduler.deleteJobWithToken;
  window.deleteJob = OpCentrixScheduler.deleteJobWithToken;
  window.updateJobFromPart = OpCentrixScheduler.updateJobFromPart;
  window.filterPartsByMachine = OpCentrixScheduler.filterPartsByMachine;
  window.updateEndTimeFromStart = OpCentrixScheduler.updateEndTimeFromStart;
  window.updateEndTimeFromQuantity = OpCentrixScheduler.updateEndTimeFromQuantity;
  window.updateDurationDisplay = OpCentrixScheduler.updateDurationDisplay;
  window.checkTimeSlotAvailability = OpCentrixScheduler.checkTimeSlotAvailability;
  window.handleDurationChange = OpCentrixScheduler.handleDurationChange;
  window.suggestNextAvailableTime = OpCentrixScheduler.suggestNextAvailableTime;
  window.showFormLoading = OpCentrixScheduler.showFormLoading;
  window.hideFormLoading = OpCentrixScheduler.hideFormLoading;
  window.handleFormResponse = OpCentrixScheduler.handleFormResponse;
  window.handleJobFormResponse = OpCentrixScheduler.handleFormResponse;
  window.handleJobFormError = function(e){ console.error('[JOB-FORM] HTMX error', e); Handler.hideFormLoading(); if(window.showErrorNotification) window.showErrorNotification('Error saving job. Please try again.'); };

  // Init hooks
  function tryBootstrapNow(){ if(document.getElementById('job-form')) OpCentrixScheduler.bootstrapModal(); }
  if(document.readyState==='loading') document.addEventListener('DOMContentLoaded', tryBootstrapNow); else tryBootstrapNow();
  document.addEventListener('htmx:afterSwap', function(e){
    const t=e.detail && e.detail.target; if(!t) return;
    if(t.id==='modal-container' || (t.querySelector && t.querySelector('#job-form'))){ setTimeout(()=>OpCentrixScheduler.bootstrapModal(),0); }
    if(t.id==='modal-container'){
      const mc=document.getElementById('modal-container'); if(mc){ mc.style.display='flex'; mc.classList.remove('hidden'); document.body.style.overflow='hidden'; }
    }
   });

  console.log('[Job Form] module loaded');
})();
