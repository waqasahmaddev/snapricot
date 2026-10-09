# ADR-0003: Windows first, macOS-ready

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

The goal is cross-platform, but shipping both at once doubles the native work (capture, hotkeys, permissions,
signing and notarization) before anyone can use the app.

## Decision

- v1.x ships on **Windows 10 (2004+) and 11** only.
- macOS ships in **v2.0**.
- From day one: `Snapricot.Platform.MacOS` exists (empty), the App picks its TFM and platform by build OS, and
  CI builds and tests the whole solution on **macOS** as well as Windows.

## Consequences

- Faster path to a usable v1.
- Windows-only assumptions can't silently leak into Core or modules: the macOS CI job would fail.
- macOS-specific UX (menu bar, Screen Recording permission prompt) is designed later, in v2.

## Alternatives considered

- **Both OSes at once:** delays v1 by an estimated 4–6 weeks.
- **Windows-only with no macOS scaffolding:** cheaper now, but invites a painful refactor later.
