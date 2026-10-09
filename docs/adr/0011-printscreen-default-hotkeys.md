# ADR-0011: Default hotkeys use PrintScreen combinations

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

A global hotkey overrides that shortcut in **every** app. The first draft used `Ctrl+Shift+S` (Save As in most apps),
`Ctrl+Shift+W` (closes browser windows), and `Ctrl+Shift+T` (reopens closed browser tabs), which would silently break
common shortcuts. Windows 11 binds plain `PrtSc` to the Snipping Tool by default.

## Decision

| Action | Default |
|---|---|
| Capture custom area (region) | `Ctrl+PrtSc` |
| Capture active window | `Alt+PrtSc` |
| Capture full screen | `Shift+PrtSc` |
| Copy text from screen (v1.1) | `Ctrl+Alt+PrtSc` (proposed) |

- Plain `PrtSc` stays free for the Snipping Tool.
- Every default is rebindable in Settings (Phase 3). A hotkey another app already owns is logged and skipped at startup;
  it never blocks startup (`HotkeyRegistrar`).
- Gestures are written as platform-neutral strings (`HotkeyGesture`, e.g. `Ctrl+PrtSc`), mapped to native codes by the platform.

## Consequences

- No clashes with common app shortcuts; matches what ShareX users expect.
- Laptops without a PrtSc key need `Fn`, or a rebind.
- macOS (v2) needs its own defaults (e.g. `Cmd+Shift+…`), chosen then.

## Alternatives considered

- **Ctrl+Shift+letter combos:** clash with Save As, close window, reopen tab.
- **Win+Shift+S:** reserved by Windows.
