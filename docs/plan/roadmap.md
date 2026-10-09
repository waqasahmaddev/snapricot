# Roadmap

Assumes ~8–10 hours per week. Week numbers are relative to the start of Phase 1.
Tick items here as they're done, and record progress in [STATUS.md](../STATUS.md).

## Phase 0: Foundation (done in the kickoff session)
- [x] Solution, module system, event bus, shell, tray, tests, CI, docs, ADRs
- [ ] First commit and push to GitHub; CI green
- [ ] Verify the Native AOT build locally (needs the C++ workload)

## Phase 1: Capture (weeks 1–2)
- [ ] `Snapricot.Platform.Windows`: `IScreenCapture` via Windows.Graphics.Capture, GDI `BitBlt` fallback (ADR-0008)
- [ ] Display enumeration with per-monitor DPI (`DisplayInfo.Scale`), tested on mixed-DPI setups
- [ ] `IHotkeyService` via `RegisterHotKey` on a hidden message window
- [ ] `IClipboardService` (CF_DIB / PNG on the clipboard)
- [ ] Capture overlay window: pre-created and hidden at startup, dim + selection + handles + size label + magnifier
- [ ] Capture sources in the Screenshot module: region, window, full screen (`ICaptureSource`)
- [ ] After-capture actions: copy, save (`IAfterCaptureAction`), with file name pattern
- [ ] Publish `CaptureCompleted` / `CaptureSaved` events
- [ ] Design note: `docs/architecture/dpi-and-multi-monitor.md`

## Phase 2: Editor (weeks 3–4)
- [ ] New module `Snapricot.Modules.Editor`, opened via a `CaptureCompleted` subscription or the toolbar "Edit"
- [ ] `IAnnotationTool` extension point; tools: arrow, rectangle, text, highlighter, blur/pixelate, numbered step, crop
- [ ] Command-pattern undo/redo (unit-tested)
- [ ] Rendering and export with SkiaSharp (PNG/JPG), optional padded background

## Phase 3: Polish and packaging (weeks 5–6)
- [ ] `Snapricot.Modules.History`: virtualized list/grid, thumbnail cache, search, tabs
- [ ] Settings shell with per-module pages; hotkey rebinding; start with Windows
- [ ] Velopack installer and auto-update (ADR-0009)
- [ ] Code signing: apply to SignPath.io (free for OSS) or use Azure Trusted Signing
- [ ] App icon (.ico), About page, first-run experience
- [ ] Closed beta with 5–10 people

## Phase 4: Launch v1.0 (weeks 7–8)
- [ ] README with a demo GIF, landing page, release notes
- [ ] Launch week, as in [marketing.md](marketing.md)
- [ ] winget, Scoop, and Chocolatey packages

## After v1.0
- **v1.1:** `Modules.Ocr` (Windows.Media.Ocr), `Modules.Pin`, `Modules.ColorPicker`
- **v1.2:** `Modules.Recording` (GIF / short MP4)
- **v2.0:** `Snapricot.Platform.MacOS` (ScreenCaptureKit, Carbon hotkeys, NSPasteboard, Screen Recording permission onboarding), signed and notarized `.app`

## Engineering practices (always on)
- PRs only into a protected `main` branch, with CI green
- Conventional Commits; changelog via release-please
- Coverage target of 70%+ on Core and modules
- An ADR for every significant decision
