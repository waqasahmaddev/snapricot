# Design

The mockups were agreed in the kickoff session on 2026-10-09. This page describes them in enough detail to build from.

## Visual language

The full palette, logo, and usage rules are in **[brand.md](brand.md)**. In the UI:

| Token (resource key) | Value | Use |
|---|---|---|
| Accent: Apricot deep (`BrandApricotDeepBrush`, Fluent accent) | `#D9622B` | Selection border, primary button (Copy), active tool, tab underline |
| Accent tint (`BrandTintBrush` / `BrandTintTextBrush`) | light `#FFF1E4` / `#9A3F14`, dark `#4A2A1C` / `#FFC98F` | Active tool background, shortcut badges |
| Sidebar: Plum (`BrandPlumBrush`) | `#2E2240` | Main window sidebar (always dark, in both themes) |
| Overlay dim | black at 50% | Area outside the selection while capturing |

> The kickoff mockups used an earlier palette (`#D85A30` accent, gray sidebar). The brand was revised on 2026-10-09
> to look clearly different from Icecream Screen Recorder; the values above replace the mockup colors.

- Follow the system light/dark theme everywhere except the sidebar and the capture overlay.
- Sentence case for every label ("Capture region", not "Capture Region").
- Show shortcuts in monospace badges.
- Only one accent-filled button per view (Copy).

## Screens

### 1. Capture overlay (shown on the region hotkey)
- Full-screen, frameless, topmost window across **all monitors**, dimming everything outside the selection.
- Dashed accent selection rectangle with corner handles; a size label above it (e.g. `600 × 300`, physical pixels).
- A round **magnifier** near the cursor with a crosshair, and a pixel coordinate readout under it.
- A **floating toolbar** under the selection: Region | Window | Full screen, then Edit, Copy text, Pin, Save, **Copy** (accent).
  The toolbar is built from registered `IAfterCaptureAction`s, so modules can add buttons.
- Keys: `Esc` cancels, `Enter` copies, hold `Shift` for a square, arrow keys nudge by 1 px.

### 2. Annotation editor
- Left vertical toolbar: select, arrow, rectangle, text, highlighter, blur/pixelate, numbered step, crop.
- Center canvas on a neutral background; right properties panel (color swatches, stroke width, shadow, background padding).
- Title bar: logo, file name, undo/redo. Bottom bar: zoom %, image size, then Pin, Save, **Copy**.

### 3. Tray menu and toast
- Tray menu: Capture region `Ctrl+Shift+S`, Capture window `Ctrl+Shift+W`, Capture full screen `PrtSc`,
  Copy text from screen `Ctrl+Shift+T`, then History, Settings, Quit. Items come from modules plus the shell.
- After a capture: a small toast with a thumbnail, "Copied to clipboard", size and save location, and an edit shortcut.

### 4. Settings
- Tabs: Hotkeys, After capture, Files, About. Each module can add its own settings page.
- Files: save folder (default `Pictures/Snapricot`) and file name pattern (default `{date}_{time}.png`).

### 5. Main window (style of Icecream Screen Recorder)
- **Dark sidebar**: logo and name; module pages (History, Screenshot, Record screen *Soon*, Copy text, Pinned,
  Color picker); and at the bottom Modules, Settings, Help.
- **History page**: tabs All / Screenshots / Recordings, search, grid/list toggle. List rows: thumbnail, file name,
  resolution, size, relative time, actions (copy, edit, open folder, more). Footer: save folder and capture count.
- Must stay smooth with thousands of items (virtualized list, small cached thumbnails).

## Reference

- Inspiration for the main window: Icecream Screen Recorder (sidebar plus history list). Its responsiveness is the bar we want to meet.
