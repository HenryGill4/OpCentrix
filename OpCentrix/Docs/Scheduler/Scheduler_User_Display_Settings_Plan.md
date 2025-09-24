Scheduler User Display Settings – Feasibility Note
Date: 2025-09-22

Goal
- Allow each user to control how the scheduler renders (layout, zoom, time format, UX toggles) without breaking linkable URLs.

Candidate settings
- Default layout: horizontal or vertical
- Default zoom level
- Time format: 12h or 24h
- Row height defaults (horizontal and vertical)
- Add-job button behavior: hidden, instant, delayed
- Visual toggles: show cohort bar, weekend/off-hours shading, animations on/off, compact text density

Implementation options
A) Client-only (localStorage)
- Effort: 0.5–1 day
- Persist and read in wwwroot/js/scheduler-ui.js
- Apply via CSS variables and small conditionals already present
- URL query params continue to override
- Per-device only

B) Cookies (server-aware defaults)
- Effort: 1–2 days
- Read cookies in Pages/Scheduler/Index.cshtml.cs to seed the SchedulerPageViewModel (zoom, orientation, row height, time format)
- Keep client write path in JS; cross-device if cookies sync (usually not)

C) Server-side user profile (DB)
- Effort: 3–5 days
- Add UserPreferences table (or extend Identity user) and a small service
- Load in IndexModel; expose simple UI in /Account/Settings
- Optional lightweight API endpoint for async saves
- Cross-device and durable

Why this is feasible
- Current code already accepts zoom and orientation via query string and uses CSS variables; applying defaults is straightforward
- Time labels are rendered in Razor; adding a 12/24h flag is a small conditional
- Row height defaults exist per machine row; add a per-user baseline and keep per-machine overrides
- Add-job button behavior is a CSS/flag toggle

Integration points
- Pages/Scheduler/Index.cshtml: already sets CSS vars per zoom and picks orientation
- Pages/Scheduler/Index.cshtml.cs: good place to read cookies or DB prefs and inject into the ViewModel
- wwwroot/js/scheduler-ui.js: read/write localStorage or call an API; apply toggles at init
- CSS (wwwroot/css/scheduler.css): flags map to CSS variables or conditional classes

Data model sketch (Option C)
- UserPreferences: UserId (PK/FK), DefaultZoom, DefaultOrientation, TimeFormat, RowHeightH, RowHeightV, AddButtonMode, CompactMode, ShowWeekend, ShowAnimations, UpdatedUtc
- Store as discrete columns or a JSON blob; either is fine

Risks and QA
- More UI states to test (especially vertical view)
- Must ensure URL params override preferences to keep link sharing predictable
- Minimal performance impact

Recommendation
- Do Option A now (localStorage) for zoom, orientation, add-button mode, time format. If adopted by users, graduate to Option C later.

Suggested next steps (for A)
- JS: add a small Preferences module in scheduler-ui.js (get, set, apply)
- On init: apply saved prefs unless a query param is present
- Add simple settings drawer (optional) or reuse /Account/Settings later
- Track a feature flag to move to Option B/C without breaking stored keys
