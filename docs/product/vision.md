# Vision

## One line

**Snapricot: snap it, mark it, share it.** A free, open-source screenshot tool that is fast enough to feel instant
and simple enough to need no manual.

## Name

- **Snapricot** = *snap* + *apricot*. Pronounced **SNAP-ri-kot** (/ˈsnæp.rɪ.kɒt/).
- Chosen because it's unusual (no existing app found with this name on 2026-10-09), memorable, and gives an easy logo.
- Logo: an apricot with one slice snapped out inside a dashed selection frame. Accent is Apricot deep `#D9622B` on Plum `#2E2240`.
  Full details in [brand.md](brand.md).

## Business model

Free and open source (MIT) now. If it gains traction, an optional Pro edition with extra modules (open core).
See [business-model.md](../plan/business-model.md).

## Problem

- The built-in tools (Snipping Tool, macOS Screenshot) are basic: weak annotation, no history, no OCR workflow.
- Power tools such as ShareX are Windows-only and overwhelming. Polished tools such as CleanShot X are macOS-only and paid.
- There's room for one **fast, clean, cross-platform, open-source** tool.

## Who it's for

1. **Developers and testers**: bug reports, PR screenshots, docs.
2. **Support and ops people**: step-by-step guides with numbered annotations, blur for private data.
3. **The author**: daily personal use, and a strong portfolio/CV project.

## Principles

1. **Instant.** Hotkey to overlay in under 100 ms. If it feels slow, it's a bug (see [performance](../architecture/performance.md)).
2. **Light.** Small download, low idle memory, nothing running that isn't needed.
3. **Extensible.** Every feature is a module, so new features never require core rewrites ([architecture](../architecture/overview.md)).
4. **Private by default.** No accounts, no telemetry, no uploads unless the user adds an upload target.
5. **Honest UI.** Sentence case, plain words, no nagging.

## Features by version

| Version | Platform | Features |
|---|---|---|
| **v1.0** | Windows | Region / window / full-screen capture, global hotkeys, tray, overlay with magnifier, copy and auto-save, annotation editor (arrow, rectangle, text, highlighter, blur/pixelate, numbered steps, crop), undo/redo, history, settings, auto-update |
| **v1.1** | Windows | OCR (copy text from screen), pin a capture on top of other windows, color picker |
| **v1.2** | Windows | GIF and short screen recording |
| **v2.0** | + macOS | ScreenCaptureKit capture, menu-bar app, permission onboarding, notarized build |
| Later | Both | Scrolling capture, upload targets (Imgur/S3/Drive), image effects (backgrounds, rounded corners), plugin SDK |

## Non-goals (for now)

- Linux (possible later; Avalonia supports it, but Wayland capture is its own project).
- Cloud accounts or a hosted sharing service.
- Full video editing.
