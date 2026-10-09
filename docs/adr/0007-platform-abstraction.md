# ADR-0007: OS services behind platform interfaces

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

Screen capture, global hotkeys, clipboard images, autostart, and OCR all use different native APIs on Windows and
macOS. Avalonia doesn't cover these (or covers them only partially).

## Decision

- Core defines the interfaces in `Snapricot.Core.Platform` (`IScreenCapture`, `IHotkeyService`, `IClipboardService`, …).
- `Snapricot.Platform.Windows` (TFM `net10.0-windows10.0.19041.0`) and `Snapricot.Platform.MacOS` (`net10.0`)
  implement them and expose `AddWindowsPlatform()` / `AddMacOSPlatform()`.
- `Snapricot.App` chooses its TFM by build OS and defines `SNAPRICOT_WINDOWS` / `SNAPRICOT_MACOS`; `AppHost` registers
  exactly one platform.
- Platform code uses source-generated interop (`LibraryImport`) and CsWinRT projections, so it stays AOT-safe.
- Coordinates are always **physical pixels**; `DisplayInfo.Scale` carries the DPI factor.
- `Snapricot.Platform.Windows` sets `EnableWindowsTargeting` so the macOS CI job can build the full solution.

## Consequences

- Modules are OS-agnostic and testable with fakes.
- Each OS capability is written twice (once per platform), as expected.
- Platform-specific UX differences (e.g. permission prompts) need an interface that can express them.

## Alternatives considered

- **`#if` in modules:** spreads OS code everywhere and makes modules untestable on the other OS.
- **Runtime OS checks in one project:** forces one TFM that can't reach WinRT APIs cleanly.
