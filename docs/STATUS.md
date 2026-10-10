# Project status

> The single source of truth for "where are we?". Update it at the end of every working session.

**Current phase:** Phase 1 (Capture): feature-complete for Windows, being polished from the owner's hands-on testing
**Next phase:** History module (Today/Earlier grid), then Phase 2 (annotation editor)
**Last updated:** 2026-10-10 (repo protection and first release)

## Start here (next session)

1. Read [AGENTS.md](../AGENTS.md) (rules: modules only reference Core; no `init` in settings records; **agents never commit**).
2. Build and test: `dotnet build Snapricot.slnx` and `dotnet test --solution Snapricot.slnx` (**177 tests, all passing**).
3. Ask the owner whether the latest overlay fix (no black screen, opaque overlay) held up in daily use. That was the
   last open bug report.
4. Git: all code is committed by the owner (last commit `01f6a06`, "no black screen on capture"). Agents never commit;
   the owner commits and pushes from Rider using the personal account (`git@github-personal:waqasahmaddev/snapricot.git`).

## Repo protection and releases (set up 2026-10-10)

- **Ruleset "Protect main branch"** (Settings, Rules): active on the default branch. Needs a PR, 1 approval, code-owner
  review (`.github/CODEOWNERS`, owner `@waqasahmaddev`), stale approvals dismissed, status checks `Build and test
  (windows-latest)` and `Build and test (macos-latest)` required, force pushes and deletion blocked.
  **Bypass: Repository admin (always)**, so the owner can still push and merge their own PRs.
- **Tag ruleset** protects `v*` tags (no deletion or force push).
- **Actions settings:** fork PR workflows need approval; workflow token is read-only by default.
- **Security:** secret scanning, push protection, Dependabot alerts and security updates are on; 2FA is on for the account.
- **Releases are automatic** ([ADR-0013](adr/0013-tag-driven-github-releases.md)): `git tag vX.Y.Z` then
  `git push origin vX.Y.Z` runs `.github/workflows/release.yml` (tests, Native AOT publish, zip + `.sha256`, GitHub
  Release; a tag with a hyphen is a pre-release). Downloads: https://github.com/waqasahmaddev/snapricot/releases
- **First release `v0.1.0-beta.1`** (2026-10-10): zip about 16 MB, CI job about 2 min. Unsigned, so Windows shows the
  SmartScreen "Windows protected your PC" screen (More info, then Run anyway). The README Download section explains this.
- Required check names must match the CI job names exactly (`Build and test (<os>)`); renaming a job in `ci.yml`
  would block every PR until the ruleset is updated.

## What works now (Windows)

- **Capture:** custom area (8 handles, move, nudge, exact W×H, aspect presets/lock, light toolbar: Save · Copy · Cancel),
  active window, full screen. Hotkeys `Ctrl/Alt/Shift+PrtSc` plus laptop alternatives `Ctrl+Shift+1/2/3`; tray menu.
- **Start from the app:** the main window is excluded from capture, the screen is frozen, an opaque overlay appears,
  and the window hides behind it; it returns when the capture finishes (`IShellWindow.BeginCaptureAsync`).
- **Overlay:** opaque, pixel-exact frozen image, no dim until an area exists (then 30%), normal arrow cursor, reused
  windows (one per display). The magnifier is optional (off by default).
- **Output:** Copy = clipboard + auto-save to the default folder; Save = dialog (name, folder, "Always save here").
  PNG or JPG, never overwrites, optional "open folder after saving".
- **Feedback:** notification (position setting, bottom right by default), optional shutter sound.
- **Settings:** General / Save to / Hotkeys (record a new combination, conflict warning, reset); start with Windows
  (opens in the tray); persisted in `%APPDATA%\Snapricot\settings.json`.
- **Home screen:** capture tiles with live shortcuts, plus a Recent strip (12 newest, fixed-size wrapping thumbnails).
- **Style:** light theme, square edges, flat full-width apricot sidebar selection, round toggles (see brand.md "Shape").

## Open items

- [ ] Owner to confirm: no black screen / text-cursor flash / Snapricot-in-shot with the final overlay flow.
- [ ] Laptop: `Ctrl+PrtSc` may be swallowed by Windows ("Print screen opens screen capture"); the alternatives work.
- [ ] Owner to delete test captures the agent made on the Desktop (`2026-10-09_16-07-18.png`, `2026-10-09_16-09-56.png`).
- [ ] Install the VS "Desktop development with C++" workload to verify the AOT build locally (CI already builds it).
- [ ] Record the first AOT startup / memory / size numbers from a CI job summary in performance.md (sign-in needed to view).
- [x] Repo protection done 2026-10-10 (see "Repo protection and releases" below). Still open: add topics, enable Discussions, social preview image.
- [ ] **Code signing** to remove the SmartScreen warning: apply to the SignPath Foundation OSS programme (signpath.org, repo `waqasahmaddev/snapricot`); fallback Azure Trusted Signing (~$10/month). When approved, add a signing step to `.github/workflows/release.yml`.
- [ ] Test the `v0.1.0-beta.1` download end to end (it ran after "Run anyway"; confirm capture works from the zip build).
- [ ] Publish to winget / Scoop once releases are signed or stable.
- [ ] Docs: `docs/architecture/dpi-and-multi-monitor.md` design note (roadmap item).
- [ ] Check domain availability (snapricot.app / snapricot.com); buy before the v1.0 launch, not now.

## Next steps (suggested order)

1. **History module** (`Snapricot.Modules.History`): Today/Earlier grid, list/grid toggle, search, thumbnails cache;
   the home screen's Recent strip links to it. Subscribe to `CaptureSaved` (no direct module references).
2. **Editor module** (Phase 2): `IAnnotationTool` extension point, arrow/rectangle/text/highlighter/blur/steps/crop,
   undo/redo; then show **Edit** in the overlay toolbar and Save dialog.
3. Pin to screen (v1.1) and OCR (v1.1); then packaging with Velopack (ADR-0009) and code signing.

## Testing aids (not in the repo)

Scripts used this session live in the agent scratchpad and are disposable; the useful techniques are:
- UI Automation (`SelectionItemPattern` / `InvokePattern` on the parent of a text element) to open pages and click tiles.
- `PrintWindow(hwnd, PW_RENDERFULLCONTENT)` to screenshot only the app window (doesn't work for transparent windows).
- End-to-end capture: click the tile via UIA, drag with `SetCursorPos` + `mouse_event`, press Enter, then check the
  clipboard and the newest file. Overlay windows are now opaque, so `CopyFromScreen` sees them.

## Decisions log (summary)

ADRs 0001–0013 in [docs/adr](adr/README.md). Key product decisions this session: name Snapricot; Windows first;
light theme with square edges; Icecream-style area selection ("Custom area"); Copy auto-saves; color picker dropped;
PrtSc + laptop hotkeys; free/MIT now, open-core Pro later; public repo `waqasahmaddev/snapricot`.

## Known issues and notes

- **Local Native AOT publish fails** with "Platform linker not found" until the C++ workload is installed.
- Debug (JIT) baseline on the dev machine: ~1.3–1.8 s to window open, ~90 MB. Budgets apply to the AOT build.
- Kickoff mockups used an older palette; [brand.md](product/brand.md) is the source of truth.
- `PrintWindow` returns black for transparent windows: never use it to judge overlay rendering.

## Session log

The kickoff session (2026-10-09) entries below run oldest to newest from the "Brand refresh" entry
downward; the bottom entry is the latest. Future sessions: add one entry per session at the bottom.

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

### 2026-10-09 (later): Square style, exact colors, smoother area dragging
- **Style (approved option A):** square edges everywhere (Fluent `ControlCornerRadius` / `OverlayCornerRadius` = 0, all view radii 0);
  sidebar selection flat, full width, solid apricot. Toggle switches stay round. Rules recorded in brand.md ("Shape").
- **Colors:** the overlay no longer dims on open (the screen looks unchanged); only the outside of an area is dimmed (30%,
  was 50%). The frozen image is drawn with nearest-neighbor (pixel-exact). Copied and saved files were always exact.
- **Smoothness:** the frozen screenshot moved to a static immutable `Image` layer (GPU-cached); the selection layer is light
  and repaints alone on pointer move. Thumbnails use high-quality downscaling.
- **To confirm by the owner:** drag smoothness in Debug vs Release (`dotnet run -c Release`).

### 2026-10-09 (later): Home screen, hide-on-capture, laptop hotkeys
- **Home screen (option A):** capture tiles (Custom area primary) with live shortcut badges, plus a Recent strip of the
  8 newest captures with Open folder. `RecentCaptures` (file lookup + captions) is unit-tested.
- **Capturing from the app hides the main window** (`IShellWindow.RunHiddenAsync`, Core contract implemented by the shell)
  and restores it after the capture finishes; hotkeys and the tray don't touch the window.
- **Laptop alternatives** `Ctrl+Shift+1/2/3` registered next to the PrtSc defaults (ADR-0011 updated).
- **Text selection** in text boxes is light apricot (`#FFD9C2`) with plum text.
- Tests: 171 passing. Home and Hotkeys verified via UI Automation screenshots.
- **Owner to check:** on the laptop, whether Windows' "Print screen opens screen capture" setting was swallowing `Ctrl+PrtSc`.

### 2026-10-09 (later): Notification position
- Settings → General → **Notification position** (Bottom right default, Bottom left, Top right, Top left); persisted as
  `NotificationPosition` in the screenshot settings. `ToastPlacement` is pure and unit-tested.
- **Bug fixed:** the notification could appear mid-screen because it was positioned before layout knew its height.
  It now places itself after layout (`DispatcherPriority.Loaded`) and stays invisible until placed.
- Tests: 176 passing.

### 2026-10-09 (later): Overlay opens in one step (cursor blinked twice)
- Cause: the overlay window was shown, then resized/moved to the monitor's exact pixels after opening (its DPI is only
  known then), so the screen and cursor changed twice; a new window was also created for every capture.
- Fix: `RegionOverlayWindow` stays at `Opacity = 0` until it has its final size and position, then reveals in one step
  (`DispatcherPriority.Loaded`) and refreshes the cursor. `OverlayRegionSelector` keeps one window per display and reuses
  it (`Begin` / `End`); windows for removed or changed displays are closed. Surface and toolbar reset between captures.
- Also: `IDesktopService.RefreshCursor()` (Windows: `SetCursorPos` to the current position), settle delay 120 ms.
- Tests: 176 passing. Needs the owner's hands-on check (the agent can't trigger captures on the owner's screen).

### 2026-10-09 (later): Cursor left alone; Recent thumbnails wrap
- The overlay no longer changes the cursor at all (owner's call after the blink persisted); `RefreshCursor` was removed.
  Handles still show resize/move cursors on hover.
- Recent strip: fixed 200 px thumbnails in a wrapping panel (more per row when maximized, never stretched); shows 12.

### 2026-10-09 (later): No text-cursor flash when capture starts
- Cause: hiding the main window left the mouse over the app behind (e.g. Rider's editor) for ~0.1 s, showing its I-beam;
  Windows then kept that cursor over the overlay until the mouse moved.
- Fix: Custom area / Full screen from the app now keep the window visible but excluded from capture
  (`IShellWindow.RunExcludedFromCaptureAsync` → `SetWindowDisplayAffinity(WDA_EXCLUDEFROMCAPTURE)`; falls back to hiding).
  Active window still hides (the previous app must be foreground). The overlay sets the arrow once on reveal (`RefreshCursor`).

### 2026-10-09 (later): Overlay first (replaces the "exclude main window" attempt)
- Owner feedback: excluding the main window from capture was worse (the tile greyed out, then the window vanished).
- Now: Custom area opens its overlay **first**, see-through (alpha 1/255, so it stays hit-testable) and excluded from capture;
  then the main window hides (`IShellWindow.HideAsync`); then the screen is frozen and the image fills the overlay.
  The cursor is over Snapricot throughout, so no other app's text cursor. Window/Full screen just hide first.
- Commands use `AllowConcurrentExecutions` so tiles never grey out. `IRegionSelector` is now `OpenAsync` → `IRegionSession`.

### 2026-10-09 (later): Snapricot appeared behind the frozen screen
- Cause: Windows 11 fades a hidden window out; the screen was sometimes frozen mid-fade, so Snapricot showed in the image.
- Fix: `ShellWindow.HideAsync` excludes the main window from capture (`SetWindowDisplayAffinity`) before hiding, so it can
  never be in the frozen image; restored afterwards. Falls back to a 250 ms wait where exclusion isn't supported.
- Verified end to end (UI Automation + simulated drag over the main window): saved image shows only what was behind
  Snapricot; clipboard and file correct; main window returns. Test script: scratchpad `e2e-region.ps1` (not in the repo).

### 2026-10-09 (later): Black screen fix — no more see-through overlay
- Owner saw a black screen sometimes. The see-through (transparent) overlay was the risk: when transparency doesn't take
  effect for a frame, a full-screen transparent window shows as solid black.
- Now: overlays are always opaque and only appear with the frozen image. From the app: `IShellWindow.BeginCaptureAsync`
  excludes the main window from capture (it stays on screen, mouse stays over Snapricot), the screen is frozen, the overlay
  appears, and the main window hides behind it (`IShellCaptureScope.HideWindow`, via the overlay's `onShown`).
- Note: PrintWindow can't read transparent windows (always black), so it is not a valid way to inspect overlays.

### 2026-10-10: Repo protection, CODEOWNERS and the first automatic release
- Added `.github/CODEOWNERS`, a branch ruleset for `main`, a tag ruleset for `v*`, and Actions/security settings
  (details in "Repo protection and releases" above). Owner did all GitHub settings by hand, guided step by step.
- Wrote `.github/workflows/release.yml` (ADR-0013). Pushed tag `v0.1.0-beta.1`; the run succeeded first time and the
  pre-release has the zip (about 16 MB) and `.sha256`. Downloaded zip runs after SmartScreen "Run anyway".
- Docs updated: README (Download section with the SmartScreen note), CHANGELOG, hosting-and-distribution.md, ADR-0013 and index.
- **Next:** apply to SignPath Foundation for code signing; then continue with the History module.
- The agent changed files only; the owner commits and pushes (rule 10).
