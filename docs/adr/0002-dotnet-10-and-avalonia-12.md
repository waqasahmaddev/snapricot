# ADR-0002: .NET 10 and Avalonia 12 for the UI

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

We need one UI codebase for Windows now and macOS later, in C#, that is fast and supports Native AOT.

## Decision

- **.NET 10** (LTS, supported until November 2028), pinned via `global.json` (SDK 10.0.302, `latestFeature` roll-forward).
- **Avalonia 12.1** with the Fluent theme, **CommunityToolkit.Mvvm** for view models (source-generated, AOT-friendly).
- **Microsoft.Extensions.DependencyInjection / Logging** for composition and logging.
- **xUnit v3** on **Microsoft.Testing.Platform** (the .NET 10 default test runner).
- Central package management (`Directory.Packages.props`); warnings are errors.

## Consequences

- One XAML UI renders identically on both OSes (Avalonia draws its own controls with Skia).
- SkiaSharp is already present through Avalonia, so the editor can use it without adding a dependency.
- Native OS features (capture, hotkeys, clipboard images) still need platform code (ADR-0007).
- Avalonia 12 is newer than most online examples (which target 11.x), so check the docs when an API differs.

## Alternatives considered

- **.NET MAUI:** no first-class desktop tray/overlay story, heavier, and weaker on macOS desktop.
- **WinUI 3 / WPF:** Windows-only, which blocks v2.
- **Uno Platform:** viable, but heavier for a small utility; Avalonia's desktop focus fits better.
- **Electron / Tauri:** not .NET; Electron fails the lightweight goal.
