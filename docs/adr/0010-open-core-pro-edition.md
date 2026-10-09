# ADR-0010: Open-core model for a possible Pro edition

- **Status:** Proposed (revisit when the triggers in docs/plan/business-model.md are met)
- **Date:** 2026-10-09

## Context

Snapricot launches free and open source (MIT). If it gains traction, a paid Pro edition may fund development. The
core must stay public (for the CV, community, CI, and OSS signing) while paid features stay private.

## Decision (proposed)

- **Open core.** The public repo (MIT) is the full free edition. Pro features are separate **modules** in a private
  repo, built into a Pro edition by private CI.
- Editions differ only in which modules `InstalledModules` lists (build flag `SNAPRICOT_PRO`). No Pro code or
  stubs ship in the public repo.
- Licensing: offline-verifiable signed license keys (asymmetric signature, public key in the app). Pro modules
  register only if the license is valid. No always-online check.
- Payments through a merchant of record (Lemon Squeezy or Paddle).
- Free features are never moved into Pro.

## Consequences

- The module architecture already supports this; nothing needs to change in the code until Pro starts.
- MIT means anyone may legally fork and resell the free edition. This is accepted: the value of Pro lies in its
  private modules and any services behind them.
- Two editions to build and test; CI for Pro runs privately.

## Alternatives considered

- **Keep everything private:** destroys the CV and community value; loses free OSS CI and signing.
- **Switch the core to GPL/AGPL:** stops closed-source forks, but discourages contributors and makes some companies avoid it.
- **Donations only:** keep as a complement (GitHub Sponsors), but rarely enough on its own.
- **Ads or telemetry monetization:** contradicts the "private by default" principle.
