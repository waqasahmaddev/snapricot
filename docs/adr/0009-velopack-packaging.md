# ADR-0009: Velopack for installers and auto-update

- **Status:** Proposed (to be confirmed in Phase 3)
- **Date:** 2026-10-09

## Context

Users need a one-click installer and silent updates. It has to work on Windows now and macOS later, and publish from
GitHub Releases.

## Decision (proposed)

- Use **Velopack**: `vpk pack` in CI produces a Windows `Setup.exe` (and later a macOS `.pkg`/`.app`), plus delta updates.
- Updates are served from **GitHub Releases**; the app checks in the background at most once a day.
- Code signing through SignPath.io (free for OSS) or Azure Trusted Signing; macOS notarization in v2.

## Consequences

- Cross-platform, fast installs and delta updates; no admin rights needed (per-user install).
- Must confirm that Velopack's startup hook stays within the startup budget, and works with Native AOT.

## Alternatives considered

- **MSIX:** Windows-only, and sideloading is awkward without the Store.
- **Squirrel.Windows:** unmaintained; Velopack is its successor.
- **Inno Setup / WiX + a custom updater:** more work, and Windows-only.
