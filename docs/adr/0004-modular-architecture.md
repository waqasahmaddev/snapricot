# ADR-0004: Every feature is a module, registered explicitly

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

The app must be easy to extend (OCR, pin, recording, uploads, effects) without rewriting the core or turning into a
tangle of cross-feature dependencies.

## Decision

- Each feature is its own project, `Snapricot.Modules.<Name>`, with one `ISnapricotModule` entry point.
- A module contributes through `IModuleBuilder`: DI services, sidebar pages (`NavItem`), tray items (`TrayItem`), and
  hotkeys (`HotkeyBinding`). More contribution kinds (settings pages, overlay actions, annotation tools) are added to
  Core as they're needed.
- Modules may reference **only Core** and UI packages. Enforced by architecture tests.
- Modules are listed **explicitly** in `InstalledModules.cs`. No assembly scanning or runtime plugin loading.
- Contributions are **lazy**: page views are created on first open.

## Consequences

- Adding a feature means a new project plus three one-line registrations; existing features stay untouched.
- Features can be removed or disabled by removing one line.
- Explicit registration is AOT/trim-safe (ADR-0006), but third-party plugins (DLLs dropped into a folder) are not
  possible yet. If needed later, a separate ADR can add an out-of-process or non-AOT plugin host.

## Alternatives considered

- **Feature folders in one project:** nothing enforces the boundaries, so they erode.
- **Reflection-based discovery (scan assemblies for `ISnapricotModule`):** breaks trimming/AOT and hides the dependency graph.
- **MEF / dynamic plugin loading:** heavy, reflection-based, and unnecessary for first-party features.
