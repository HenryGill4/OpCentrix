# Master Part Form Dynamic Injection / Script Initialization Fix

Date: 2025-09-28
Status: Implemented & Verified
Related Files:
- `Pages/Admin/Shared/_MasterPartForm.cshtml`
- `wwwroot/js/masterPartForm.js`
- `Pages/Admin/Shared/_AdminLayout.cshtml`
- `Pages/Shared/_Layout.cshtml`
- `Pages/Admin/Parts.cshtml`

## 1. Summary
Dynamic modal content (Master Part form) was injected via `fetch` + `innerHTML` on the Parts admin page. JavaScript logic embedded in / or referenced from the partial never executed after injection, producing an inert form (stage selection, SLS config, validation, derived hour calculations all broken). After a failed submit / server-side error, a full-page response would occasionally contain working JavaScript, creating confusing behavior where the UI "suddenly" worked but submission still failed.

## 2. Root Causes
| Root Cause | Details |
|------------|---------|
| Script execution on dynamic HTML | Browsers do **not** execute `<script>` tags inserted via `element.innerHTML` (including external `src` tags) — the original inline or referenced script in the partial never ran. |
| Layout mismatch | Script was first added only to the main `_Layout.cshtml`; Admin pages (including `/Admin/Parts`) use `_AdminLayout.cshtml`, so the global initializer was missing there. |
| Duplicate logic drift risk | A lightweight alternative initializer was added inline to `Parts.cshtml` causing risk of divergence with the canonical logic. |
| No deterministic re-init hook | Initialization relied on injection timing without a MutationObserver or explicit re-init API. |

## 3. Fix Overview
1. **Centralized Script**: Moved *all* form logic to a single external file: `wwwroot/js/masterPartForm.js`.
2. **Global Inclusion**: Added that script to **both** `_Layout.cshtml` and `_AdminLayout.cshtml` so whichever layout renders the page, the initializer is present before modal HTML injection.
3. **Idempotent Init**: The script sets `form.__initialized = true` to prevent double-binding.
4. **MutationObserver**: Watches the DOM; when `#masterPartForm` appears (dynamic injection), `initMasterPartForm()` is invoked automatically.
5. **Manual Hook**: Exposed `window.initMasterPartForm()` for optional explicit re-initialization (e.g., after partial replace or testing).
6. **Debug Logging**: Added clear console messages to trace load vs. init vs. mutation triggers.
7. **Context Header**: Added a multi-line comment block at top of `_MasterPartForm.cshtml` summarizing the issue and the applied fix for future maintainers.

## 4. Implementation Details
### 4.1 Mutation Observer Pattern
```js
const observer = new MutationObserver(() => {
  const form = document.getElementById('masterPartForm');
  if (form && !form.__initialized) initMasterPartForm();
});
observer.observe(document.documentElement, { childList: true, subtree: true });
```
This eliminates reliance on arbitrary timeouts.

### 4.2 Idempotency Guard
```js
if (form.__initialized) return;
form.__initialized = true;
```
Prevents event handler duplication when content is reloaded.

### 4.3 Manual Re-Init (Optional)
```js
window.initMasterPartForm = initMasterPartForm;
```
Allows explicit calls after custom DOM transforms: `initMasterPartForm();`

## 5. Common Pitfalls & How to Avoid Them
| Pitfall | Resolution |
|---------|------------|
| Embedding `<script>` in dynamic partials | Always move logic to a globally loaded file. |
| Relying on `setTimeout` after injection | Use MutationObserver or dispatch a custom event. |
| Multiple divergent initializers | Maintain a **single** source (here: `masterPartForm.js`). |
| Silent failures | Add debug logs during development; strip or gate for production. |

## 6. Recommended Future Enhancements
1. **Custom Event Dispatch**: After injecting HTML, dispatch: `document.dispatchEvent(new CustomEvent('oc:fragment:loaded',{detail:{id:'masterPartForm'}}));` and listen instead of raw mutation.
2. **Feature Flag Logging**: Wrap debug logs: `if (window.OC_DEBUG) console.log(...)`.
3. **Unit / UI Tests**: Add Playwright test ensuring stage selection changes hidden inputs & enables submit.
4. **Remove Redundant Code**: If any remaining legacy form init snippets exist (e.g., simplified bootstrap logic), remove them.
5. **Telemetry Hook**: Count how often mutation vs. direct init is used to detect regressions.

## 7. Validation Checklist
| Item | Verified |
|------|----------|
| Script loads on Admin Parts page | ? |
| `initMasterPartForm` called exactly once per injection | ? (guarded) |
| Stage selection updates hidden `SelectedStageIds` / `StageEstimatedHours` | ? |
| SLS build mode derived hours update | ? |
| Submit packs all derived fields and sends via fetch | ? |
| No duplicate event handlers on repeated open/close | ? |

## 8. Rollback Plan
If issues arise:
1. Remove MutationObserver block.
2. Manually call `initMasterPartForm()` in the fetch success callback after setting `innerHTML`.
3. (Temporary) Inline a minimal initialization script *after* injection (not recommended long-term).

## 9. Maintenance Notes
- When adding new dynamic forms, replicate this pattern: global script + idempotent init + observer or explicit event.
- Avoid adding business logic to Razor partials; keep them presentational.

## 10. Quick Troubleshooting Guide
| Symptom | Likely Cause | Quick Fix |
|---------|--------------|-----------|
| No console logs | Script not included in current layout | Verify `<script src="~/js/masterPartForm.js">` presence |
| Form non-interactive | `initMasterPartForm` never ran | Call `initMasterPartForm()` manually in console |
| Double event firing | Guard removed / duplicate script load | Ensure single include + `__initialized` guard |
| Hidden fields empty on submit | Stage checkbox handlers not bound | Re-run `initMasterPartForm()` |

---
**Owner**: Manufacturing UI Module

**Contact**: If this breaks again, search repository for `initMasterPartForm` usages first, then verify layout script includes.
