# Roadmap

Assumes ~8–10 hours per week. Week numbers are relative to the start of Phase 1.
Tick items here as they're done, and record progress in [STATUS.md](../STATUS.md).

## Phase 0: Foundation (done in the kickoff session)
- [x] Solution, module system, event bus, shell, tray, tests, CI, docs, ADRs
- [ ] First commit and push to GitHub; CI green
- [ ] Verify the Native AOT build locally (needs the C++ workload)

## Phase 1: Capture (weeks 1–2)
- [x] `Snapricot.Platform.Windows`: `IScreenCapture` via GDI `BitBlt` (ADR-0008, revised: WGC only as a later fallback)
- [x] Display enumeration with per-monitor DPI (`DisplayInfo.Scale`); overlay maps DIPs to pixels from the image itself
- [x] `IHotkeyService` via `RegisterHotKey` on a dedicated Win32 message thread; PrtSc defaults (ADR-0011)
- [x] `IClipboardService` (CF_DIB)
- [x] **Custom area** overlay: freeze, dim, draw, 8 handles, move, nudge, W × H boxes, aspect presets and lock, magnifier, toolbar
- [x] Region, active window, and full screen captures (`CaptureCoordinator`), from hotkeys, the tray, and the Screenshot page
- [x] Copy = clipboard plus auto-save to the default folder; **Save dialog** with name, folder picker, "Always save here"
- [x] Settings persisted in `%APPDATA%\Snapricot\settings.json` (ADR-0012)
- [x] Publish `CaptureCompleted` / `CaptureSaved` events
- [ ] Hands-on test on real hardware: mixed-DPI multi-monitor, all hotkeys, pasting into Paint/Word/Slack/browsers
- [ ] Pre-create the overlay window hidden at startup, and measure hotkey to overlay (< 100 ms budget)
- [x] Notification after capture ("Copied and saved", thumbnail, click shows the file) and optional shutter sound
- [ ] Design note: `docs/architecture/dpi-and-multi-monitor.md`

## Phase 2: Editor (weeks 3–4)
- [ ] New module `Snapricot.Modules.Editor`, opened via a `CaptureCompleted` subscription or the toolbar "Edit"
- [ ] `IAnnotationTool` extension point; tools: arrow, rectangle, text, highlighter, blur/pixelate, numbered step, crop
- [ ] Command-pattern undo/redo (unit-tested)
- [ ] Rendering and export with SkiaSharp (PNG/JPG), optional padded background

## Phase 3: Polish and packaging (weeks 5–6)
- [ ] `Snapricot.Modules.History`: virtualized list/grid, thumbnail cache, search, tabs
- [x] Settings shell with per-module tabs/sections; hotkey rebinding; start with Windows; PNG/JPG (done early, 2026-10-09)
- [ ] Velopack installer and auto-update (ADR-0009)
- [ ] Code signing: apply to SignPath.io (free for OSS) or use Azure Trusted Signing
- [ ] App icon (.ico), About page, first-run experience
- [ ] Closed beta with 5–10 people

## Phase 4: Launch v1.0 (weeks 7–8)
- [ ] README with a demo GIF, landing page, release notes
- [ ] Launch week, as in [marketing.md](marketing.md)
- [ ] winget, Scoop, and Chocolatey packages

## After v1.0
- **v1.1:** `Modules.Ocr` (Windows.Media.Ocr), `Modules.Pin` (color picker dropped on 2026-10-09)
- **v1.2:** `Modules.Recording` (GIF / short MP4)
- **v2.0:** `Snapricot.Platform.MacOS` (ScreenCaptureKit, Carbon hotkeys, NSPasteboard, Screen Recording permission onboarding), signed and notarized `.app`

## Engineering practices (always on)
- PRs only into a protected `main` branch, with CI green
- Conventional Commits; changelog via release-please
- Coverage target of 70%+ on Core and modules
- An ADR for every significant decision
