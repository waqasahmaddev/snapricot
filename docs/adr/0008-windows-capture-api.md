# ADR-0008: Windows.Graphics.Capture with GDI fallback

- **Status:** Proposed (to be confirmed by a spike at the start of Phase 1)
- **Date:** 2026-10-09

## Context

Windows offers several capture APIs: GDI `BitBlt` (old, simple, misses some hardware-accelerated content), DXGI
Desktop Duplication (fast, per-output, more complex), and Windows.Graphics.Capture (WGC; modern, handles windows and
monitors, shows a yellow capture border on older builds).

## Decision (proposed)

- For a **still screenshot** of a region or monitor: capture each affected monitor with **WGC** and compose them into
  physical-pixel space. Hide the capture border with `IsBorderRequired = false` where supported (Windows 11 / Windows 10 20348+).
- **Fallback to GDI `BitBlt`** when WGC is unavailable or fails.
- **Freeze-frame overlay:** capture the screen *before* showing the overlay, then draw the overlay over that still
  image. This is instant, and what you select is exactly what you get.
- Window capture: WGC `GraphicsCaptureItem` for an `HWND`.

## Consequences

- Correct capture of DirectX/GPU content, including in mixed-DPI setups.
- Needs the 10.0.19041 TFM and CsWinRT interop (AOT-compatible).
- The spike must measure hotkey-to-overlay time against the 100 ms budget.

## Alternatives considered

- **GDI only:** simplest, but misses some GPU content and is slower at 4K.
- **DXGI Desktop Duplication only:** fast, but doesn't capture a single window, and has more edge cases (rotation, protected content).
