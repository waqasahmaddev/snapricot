# ADR-0008: GDI BitBlt first, Windows.Graphics.Capture as fallback

- **Status:** Accepted (revised at the start of Phase 1; the earlier proposal had WGC first)
- **Date:** 2026-10-09

## Context

Windows offers several capture APIs: GDI `BitBlt` (simple, from the desktop DC), DXGI Desktop Duplication (fast,
per-output, complex), and Windows.Graphics.Capture (WGC; modern, needs CsWinRT and D3D11 interop, shows a capture border
on some builds). Snapricot v1 needs **still** captures of regions, monitors, and the active window, with Native AOT.

## Decision

- Capture with **GDI `BitBlt` from the desktop DC** (`SRCCOPY | CAPTUREBLT`) into a top-down 32-bit DIB section.
  Under DWM composition (Windows 8+) this includes GPU-rendered content. It's a few milliseconds at 4K, and AOT-safe with
  `LibraryImport`. The alpha channel is forced to opaque.
- **Freeze-frame overlay:** each display is captured *before* the overlay opens, and the selection is cropped from that
  still image, so what you select is exactly what you get, instantly.
- **Active window** = the DWM extended frame bounds of the foreground window, captured from the screen (what's visible).
- Keep **WGC as the fallback**, to add only if a real case appears that GDI gets wrong (e.g. DRM-protected or
  hardware-overlay content), or for capturing occluded windows.

## Consequences

- No WinRT/D3D dependency in v1; small, AOT-friendly interop surface (`Interop/NativeMethods.cs`).
- Window capture shows overlapping windows on top of the target (acceptable for "active window"; occluded-window capture
  would need WGC).
- The per-monitor DPI awareness in the app manifest is required, so every coordinate is in physical pixels.

## Alternatives considered

- **WGC first:** heavier interop and a possible yellow border, with no benefit for stills in v1.
- **DXGI Desktop Duplication:** fast, but per-output, no single-window capture, more edge cases (rotation, protected content).
