# Project status

> The single source of truth for "where are we?". Update it at the end of every working session.

**Current phase:** Phase 1 (Capture): in progress
**Next phase:** History module, then Phase 2 (Editor)
**Target:** v1.0 (Windows) about 8 weeks after Phase 1 starts, at ~8–10 hours per week

![Phase 0 shell](images/phase0-shell.png)

## Phase 0 checklist

- [x] Name chosen: Snapricot ([vision](product/vision.md))
- [x] UI mockups agreed ([design](product/design.md))
- [x] Brand: logo (apricot with a snapped slice) and palette ([brand](product/brand.md))
- [x] Build and marketing plan written ([roadmap](plan/roadmap.md), [marketing](plan/marketing.md))
- [x] Hosting and business decisions ([hosting](plan/hosting-and-distribution.md), [business model](plan/business-model.md))
- [x] Architecture decided and recorded (ADR-0001 to ADR-0010)
- [x] Solution scaffold: Core, Platform.Windows, Platform.MacOS, App, Screenshot module
- [x] Module system: `ISnapricotModule`, `IModuleBuilder`, `ModuleLoader`, `ModuleCatalog`
- [x] In-process event bus with tests
- [x] Shell: light theme, sidebar built from modules (apricot selection), lazy pages, tray icon (closing hides to tray)
- [x] Brand colors as app resources (`BrandApricotDeepBrush`, `BrandPlumBrush`, `BrandTintBrush`, ...)
- [x] Unit tests (13) and architecture tests (6) passing
- [x] `--measure-startup` diagnostics switch
- [x] CI workflow: build + test on Windows and macOS, Native AOT publish and startup measurement on Windows
- [ ] Install the VS "Desktop development with C++" workload locally to verify the AOT build and its startup time
- [x] Local git identity and `origin` set to the personal account (`waqasahmaddev`), the same as `flow`
- [x] Public repo `waqasahmaddev/snapricot` created; first commit pushed by the owner (**the owner commits; agents don't**)
- [x] CI green on the first push (Windows build+test, macOS build+test, Native AOT publish)
- [ ] Record the first AOT startup, memory, and size numbers from the CI job summary in performance.md
- [ ] Repo settings: protect `main`, add topics, enable Discussions, social preview image
- [ ] Check domain availability (snapricot.app / snapricot.com); buy before the v1.0 launch, not now

## Known issues and notes

- **Local Native AOT publish fails** with "Platform linker not found" until the C++ workload is installed.
  The IL compilation step produced no AOT/trim warnings. CI runners already have the linker.
- Debug (JIT) baseline on the dev machine: **~1.28 s to window open, ~90 MB working set**. This is not the release
  number; budgets apply to the AOT build (see [performance](architecture/performance.md)).
- The kickoff mockups use the old palette; [brand.md](product/brand.md) overrides them.
- **Light theme decided:** white app, warm off-white sidebar, solid apricot selection, apricot tint on hover (see brand.md). The owner can still change these.
- An **About** module was added (`Snapricot.Modules.About`). The 4 extra tests come from it being registered in the architecture tests (23 tests total).

## Session log

Newest first. One entry per session: what changed and what's next.

### 2026-10-09 (later): Brand refresh, hosting, and business plan
- The first logo (orange square with corner brackets) looked too much like Icecream Screen Recorder. Replaced it with an
  **apricot with one slice snapped out inside a dashed selection frame**: no tile, no brackets. SVG source plus a small mark
  for 32 px and below; PNGs come from `tools/brand/Render-Logo.ps1`.
- New palette: Apricot `#F4943C`, Apricot deep `#D9622B` (UI accent), Flesh `#FFC98F`, Leaf `#3E9B6A`, Plum `#2E2240` (sidebar).
  Applied as resource keys in `App.axaml`; the Screenshot module now uses the keys instead of hex values.
- Decided: **public** GitHub repo under a `snapricot` organization; free hosting (GitHub Pages, Releases, Actions); buy a domain
  just before v1.0. Free/MIT now, open-core Pro later (ADR-0010, Proposed).
- **Next:** create the GitHub organization, then the first commit and push, then Phase 1.

### 2026-10-09: Project kickoff
- Picked the name Snapricot. Designed the mockups (capture overlay, editor, tray, settings, main window in the style of Icecream Screen Recorder).
- Decided: Windows first, macOS-ready; modular "everything is a module" architecture; Native AOT and performance budgets.
- Scaffolded the solution, module system, event bus, shell with tray, Screenshot module placeholder, tests, CI, and all docs/ADRs.

### 2026-10-09 (Phase 1 start): Windows capture implemented, not yet tried by hand
- Core: `PixelPoint`, `PixelBounds` helpers, `ImageOperations.Crop`, `FileNamePattern`, `HotkeyGesture` parser, `IDesktopService`; `IHotkeyService.RegisterAsync` is now async.
- Platform.Windows: GDI `BitBlt` capture with per-monitor DPI, display enumeration, foreground window bounds, `RegisterHotKey` and CF_DIB clipboard on a dedicated Win32 message thread. macOS: placeholders that throw `PlatformNotSupportedException`.
- Screenshot module: `CaptureCoordinator` (clipboard first, then save PNG to `Pictures/Snapricot`), overlay window per display (dim, dashed selection, size label, magnifier; Esc cancels, Enter captures the whole display), tray items, hotkeys, Capture buttons on the page.
- Default hotkeys changed to `Ctrl+PrtSc` (region), `Alt+PrtSc` (window), `Shift+PrtSc` (full screen), so they don't override app shortcuts.
- CI: the startup step now fails loudly and prints its numbers to the log; the artifact excludes `.pdb` files.
- Tests: 88 passing (new Screenshot.Tests project).
- **Next:** try it by hand (all hotkeys, multi-monitor, mixed DPI, pasting into Paint/Slack); update ADR-0008 to Accepted (GDI first); write ADR-0011 (PrtSc defaults); update design.md shortcuts and the roadmap checkboxes; write dpi-and-multi-monitor.md.

### 2026-10-09 (later): Custom area and Save dialog (approved mockups)
- **Custom area:** after drawing, the selection stays editable: 8 handles, drag inside to move, arrows/Shift+arrows nudge,
  W × H boxes, aspect Free/16:9/4:3/1:1 and a lock, and a light floating toolbar (Save · Copy · Cancel; Edit and Pin hidden until they exist).
  Enter or double-click = Copy, Esc = cancel, right-click clears the area.
- **Saving:** Copy = clipboard plus auto-save to the default folder. Save = dialog with file name, folder picker, and "Always save here".
  Default = `Pictures\Snapricot`; an unavailable custom default falls back to it. Never overwrites (`name (2).png`).
- **Settings store:** `ISettingsStore` / `JsonFileSettingsStore` in Core (`%APPDATA%\Snapricot\settings.json`, one section per module, AOT-safe).
- **Dropped:** the color picker (removed from vision, roadmap, design, README, business model).
- Docs: design.md (Custom area, Save dialog, saving rules), ADR-0008 accepted (GDI first), ADR-0011 (PrtSc hotkeys), ADR-0012 (settings).
- Tests: 141 passing (geometry, coordinator flows, save locations, file name rules, settings store). The app starts and exits cleanly.
- **Not yet done:** hands-on testing on real hardware (the agent can't press hotkeys or drag on the owner's screen), the toast, the pre-created overlay.
- **Next:** owner tests Custom area and the Save dialog; then the History module (Today/Earlier grid) or Phase 2 (editor).

### 2026-10-09 (later): Settings panel (approved mockup) and capture polish
- **Fixes from the owner's test:** magnifier off by default (now a setting), no crosshair lines, app icon on every window
  (style in `App.axaml`), centered button text in the Save dialog.
- **Settings page:** bottom sidebar group (Settings, About). Tabs come from modules (`AddSettingsTab` / `AddSettingsSection`):
  General (shell: start with Windows, close-to-tray; Screenshot: magnifier, notification, sound), Save to (folder,
  pattern + example, PNG/JPG, auto-save on Copy, open folder after saving), Hotkeys (record a new combination, in-use warning, reset).
- **New platform services:** `IAutostartService` (HKCU Run key, starts with `--tray`), `IShellService` (Explorer select),
  `ISoundService` (synthesized shutter WAV via `PlaySound`).
- **HotkeyManager** in Core replaces the shell's registrar: overrides persisted, status per hotkey, rollback on conflict.
- **Bug found and fixed:** settings records used `init`; source-generated JSON reset missing properties to `false`, so
  "notification" and "auto-save" read as off for existing users. Switched to `set` and added a regression test (ADR-0012 updated).
- **Styling:** light outlined secondary buttons and softer off-toggles (Fluent theme resources overridden in `App.axaml`).
- Tests: 165 passing. Settings tabs verified visually via UI Automation screenshots.
- On the dev machine, `Shift+PrtSc` is already owned by another app; Settings → Hotkeys shows the warning as designed.
- **Next:** owner tests capture, notification, sound, hotkey rebinding, start with Windows; then the History module.
