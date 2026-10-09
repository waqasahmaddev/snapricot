# Design

The mockups were agreed in the kickoff session on 2026-10-09. This page describes them in enough detail to build from.

## Visual language

The full palette, logo, and usage rules are in **[brand.md](brand.md)**. In the UI:

| Token (resource key) | Value | Use |
|---|---|---|
| Accent: Apricot deep (`BrandApricotDeepBrush`, Fluent accent) | `#D9622B` | Selection border, primary button (Copy), active tool, tab underline |
| Accent tint (`BrandTintBrush` / `BrandTintTextBrush`) | light `#FFF1E4` / `#9A3F14`, dark `#4A2A1C` / `#FFC98F` | Active tool background, shortcut badges |
| Theme | Light | White app; apricot only for active, selected, and primary elements |
| Sidebar (`ShellSidebarBrush`) | `#FCF8F4` | Warm off-white, with a `#F0E6DC` right edge |
| Selected nav item | `#D9622B` fill, white text | Hover: `#FFF1E4` tint with `#9A3F14` text |
| Text and icons (`BrandPlumBrush`) | `#2E2240` | Instead of pure black |
| Overlay dim | black at 50% | Area outside the selection while capturing |

> The kickoff mockups used an earlier palette (`#D85A30` accent, gray sidebar). The brand was revised on 2026-10-09
> to look clearly different from Icecream Screen Recorder; the values above replace the mockup colors.

- Follow the system light/dark theme everywhere except the sidebar and the capture overlay.
- Sentence case for every label ("Capture region", not "Capture Region").
- Show shortcuts in monospace badges.
- Only one accent-filled button per view (Copy).

## Screens

### 1. Custom area (region capture, approved 2026-10-09)
Opened by `Ctrl+PrtSc`, the tray, or the Capture button. Inspired by Icecream Screen Recorder's area selection.
- **Freeze:** every display is captured first; one frameless, topmost window per display shows its still image, dimmed.
- **Draw:** drag to draw the area with the cross cursor. The optional round **magnifier** (15×15 px at 8×, with pixel
  coordinates) helps you start on the exact pixel. A click without a drag does nothing.
- **Adjust** (the area stays editable after you release):
  - **8 handles** (4 corners, 4 edge midpoints) resize; dragging **inside** moves it; dragging **outside** starts a new area.
  - Arrow keys nudge by 1 px, `Shift`+arrows by 10 px. Right-click clears the area (right-click again cancels).
- **No crosshair lines.** The magnifier is **off by default** (Settings → General → "Area selection magnifier").
- **Size label** above the top-left corner (`1971 × 1221`, physical pixels), in apricot.
- **Floating toolbar**, light theme, placed below the area and right-aligned; it flips above, then inside, when there's no room:
  `Custom area` label · aspect **Free / 16:9 / 4:3 / 1:1** · **W × H** boxes (type an exact size, Enter applies) ·
  **lock** (keeps the current ratio) · **Save** (opens the Save dialog) · **Copy** (accent) · **Cancel**.
  Edit and Pin are hidden until those features exist (Phase 2 / v1.1).
- **Keys:** `Enter` or double-click inside = Copy (Enter with no area = the whole display). `Esc` = cancel.
- **Copy** = clipboard plus auto-save to the default folder (so it appears in History). **Save** = choose name and folder.

### 1b. Save dialog (approved 2026-10-09)
- Thumbnail plus size in px; **File name** (prefilled from the pattern, editable; `.png` is added automatically).
- **Save to**: the folder path with a **Default** badge when it is the default, and a **Change…** button (system folder picker).
- **Always save here (make this my default folder)** checkbox, shown when the folder isn't already the default.
- A dismissable tip; **Cancel** and **Save** (accent; Enter). Upload/share by URL is left out (candidate for Pro).

**Saving rules**
| Situation | Where the file goes |
|---|---|
| No folder ever chosen | `Pictures\Snapricot` (the default) |
| Folder picked in the dialog | That folder, for this capture only (and suggested next time) |
| Folder picked with "Always save here" | That folder, and it becomes the new default |
| Default folder unavailable (unplugged drive) | `Pictures\Snapricot`, with a note in the dialog |

Names that already exist get ` (2)`, ` (3)`... (nothing is overwritten). Invalid names (`\ / : * ? " < > |`, a trailing dot,
`CON`/`NUL`/...) show an inline error. Settings persist in `%APPDATA%\Snapricot\settings.json` (ADR-0012).

### 2. Annotation editor
- Left vertical toolbar: select, arrow, rectangle, text, highlighter, blur/pixelate, numbered step, crop.
- Center canvas on a neutral background; right properties panel (color swatches, stroke width, shadow, background padding).
- Title bar: logo, file name, undo/redo. Bottom bar: zoom %, image size, then Pin, Save, **Copy**.

### 3. Tray menu and toast
- Tray menu: Capture region `Ctrl+PrtSc`, Capture active window `Alt+PrtSc`, Capture full screen `Shift+PrtSc`,
  then History, Settings, Quit. Items come from modules plus the shell. Default hotkeys use PrtSc combos (ADR-0011).
- After a capture: a small toast with a thumbnail, "Copied to clipboard", size and save location, and an edit shortcut.

### 4. Settings (approved 2026-10-09)
Sidebar bottom group: **Settings**, **About**. Tabs are contributed by modules (see architecture overview, "Settings").

| Tab | Options (default) | Owner |
|---|---|---|
| **General** | Start with Windows (off) · Closing the window keeps Snapricot in the tray (on) | Shell |
| | Area selection magnifier (off) · Show a notification after capture (on) · Play a capture sound (off) | Screenshot |
| **Save to** | Default folder with Change… / Reset (`Pictures\Snapricot`) · File name pattern with live example (`{date}_{time}`) · PNG or JPG (PNG) · Copy also saves automatically (on) · Open the folder after saving (off) | Screenshot |
| **Hotkeys** | Each hotkey with **Change** (press the new combination; Esc cancels) · "In use by another app" warning · Reset to defaults | Shell |

- Left out on purpose: language, theme, auto-update, and usage stats (nothing to configure yet; we never collect stats).
- Started by Windows at login, the app opens hidden in the tray (`--tray`).
- Secondary buttons are white with a light border and an apricot hover; "off" toggles use a warm grey knob.

### 4b. Capture notification
- Bottom-right of the screen; never takes focus; closes after 4 s (stays open while hovered).
- Thumbnail, "Copied and saved" / "Copied to clipboard" / "Saved", file name. Click: show the file in its folder.

### 5. Main window (style of Icecream Screen Recorder)
- **Light sidebar** (warm off-white, apricot selection): logo and name; module pages (History, Screenshot,
  Record screen *Soon*, Copy text, Pinned); and at the bottom Modules, Settings, Help. No color picker (dropped on 2026-10-09).
- **History page**: tabs All / Screenshots / Recordings, search, grid/list toggle, items grouped **Today / Earlier**. List rows: thumbnail, file name,
  resolution, size, relative time, actions (copy, edit, open folder, more). Footer: save folder and capture count.
- Must stay smooth with thousands of items (virtualized list, small cached thumbnails).

## Reference

- Inspiration for the main window: Icecream Screen Recorder (sidebar plus history list). Its responsiveness is the bar we want to meet.
