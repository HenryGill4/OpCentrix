// scheduler-runtime.js
// Shared runtime logic for Start Job modal & future embedded modals.
// Ensure this file is referenced in _Layout.cshtml AFTER site.js.
// Provides: openStartJobModal(jobId), submitStartJob(), utility progress helpers (future phases).

window.OpCentrixRuntime = (function(){
  const api = {};
  api.openStartJobModal = function(jobId){
     fetch(`/api/scheduler/jobs/${jobId}`)
       .then(r=> r.ok ? r.json() : Promise.reject(r))
       .then(job => renderModal(job))
       .catch(()=> notify('Failed to load job details','error'));
  };

  function renderModal(job){
     const container = ensureContainer();
     container.innerHTML = getModalHtml(job);
     wireModal(job);
  }

  function ensureContainer(){
     let c = document.getElementById('embedded-modal-root');
     if(!c){
       c = document.createElement('div');
       c.id='embedded-modal-root';
       document.body.appendChild(c);
     }
     return c;
  }

  function close(){
     const c = document.getElementById('embedded-modal-root');
     if(c) c.innerHTML='';
  }
  api.close = close;

  function wireModal(job){
     const form = document.getElementById('start-job-form');
     if(form){
       form.addEventListener('submit', e => {
          e.preventDefault();
          submitStart(job.id);
       });
     }
  }

  function submitStart(jobId){
     const data = {
       stackLevel: parseInt(document.getElementById('sj-stack-level').value)||null,
       actualUnitsPlanned: parseInt(document.getElementById('sj-actual-units').value)||0,
       prototypeUnitsPlanned: parseInt(document.getElementById('sj-prototype-units').value)||null,
       powderAddedKg: parseFloat(document.getElementById('sj-powder-added').value)||null,
       overrideDurationHours: parseFloat(document.getElementById('sj-override-hours').value)||null,
       notes: document.getElementById('sj-notes').value||null
     };
     if(data.actualUnitsPlanned <=0){ notify('Units must be > 0','error'); return; }
     fetch(`/api/scheduler/jobs/${jobId}/start`,{
       method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(data)
     }).then(r=> r.json().then(j=>({ok:r.ok, body:j})))
       .then(res=>{
          if(!res.ok){ notify(res.body.error||'Start failed','error'); return; }
          notify('Job started','success');
          close();
          // TODO: trigger grid refresh (HTMX) if available
          if(window.htmx){
             const qs = window.location.search.replace('?','&');
             htmx.ajax('GET', `/Scheduler?handler=RefreshGrid${qs}`, {target:'#scheduler-main-content', swap:'innerHTML'});
             htmx.ajax('GET', `/Scheduler?handler=RefreshSummary${qs}`, {target:'#footer-summary', swap:'innerHTML'});
          }
       })
       .catch(()=> notify('Error starting job','error'));
  }

  function getModalHtml(job){
    return `<!-- EMBEDDED START JOB MODAL (requires scheduler-runtime.js loaded in _Layout) -->
<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm" onclick="OpCentrixRuntime.close()">
  <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg relative" onclick="event.stopPropagation()">
    <div class="px-6 py-4 border-b flex items-center justify-between bg-gradient-to-r from-blue-50 to-indigo-50 rounded-t-xl">
      <h3 class="text-lg font-semibold text-gray-800">Start Job #${job.id} - ${job.partNumber}</h3>
      <button class="text-gray-500 hover:text-gray-700" onclick="OpCentrixRuntime.close()">?</button>
    </div>
    <form id="start-job-form" class="p-6 space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Stack Level</label>
          <select id="sj-stack-level" class="w-full border rounded px-2 py-1 text-sm">
            <option value="1">1x</option>
            <option value="2">2x</option>
            <option value="3">3x</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Override Hours</label>
          <input id="sj-override-hours" type="number" step="0.1" min="0" class="w-full border rounded px-2 py-1 text-sm" placeholder="(optional)" />
        </div>
      </div>
      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Units Loaded *</label>
          <input id="sj-actual-units" type="number" min="1" class="w-full border rounded px-2 py-1 text-sm" required />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Prototype Units</label>
          <input id="sj-prototype-units" type="number" min="0" class="w-full border rounded px-2 py-1 text-sm" />
        </div>
      </div>
      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-medium text-gray-600 mb-1">Powder Added (kg)</label>
          <input id="sj-powder-added" type="number" step="0.01" min="0" class="w-full border rounded px-2 py-1 text-sm" />
        </div>
        <div class="flex flex-col justify-end text-xs text-gray-500">
          <span>Material: ${job.powderMaterial || job.slsMaterial || ''}</span>
          <span>Status: ${job.status}</span>
        </div>
      </div>
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-1">Notes</label>
        <textarea id="sj-notes" rows="3" class="w-full border rounded px-2 py-1 text-sm" placeholder="Setup / additional context..."></textarea>
      </div>
      <div class="flex justify-end space-x-3 pt-2 border-t">
        <button type="button" onclick="OpCentrixRuntime.close()" class="px-4 py-2 text-sm rounded border bg-white hover:bg-gray-50">Cancel</button>
        <button type="submit" class="px-4 py-2 text-sm rounded bg-blue-600 text-white hover:bg-blue-700">Start</button>
      </div>
    </form>
  </div>
</div>`;
  }

  function notify(msg,type){
     if(window.showSuccessNotification && type==='success'){ window.showSuccessNotification(msg); return; }
     if(window.showErrorNotification && type==='error'){ window.showErrorNotification(msg); return; }
     console.log(type?`[${type}]`:'[info]', msg);
  }

  return api;
})();
