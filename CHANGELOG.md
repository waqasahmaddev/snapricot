# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added
- Release automation: pushing a `v*` tag builds the Native AOT Windows zip and publishes a GitHub Release (ADR-0013). `CODEOWNERS` added.
- Settings page (General, Save to, Hotkeys) built from module-contributed tabs and sections; hotkey rebinding with conflict detection.
- Start with Windows (opens in the tray), capture notification, optional shutter sound, PNG or JPG, open folder after saving.
- Custom area editing (8 handles, move, nudge, exact size, aspect presets) and the Save dialog with "Always save here".
- Solution scaffold: Core, Windows and macOS platform projects, Avalonia shell, Screenshot module.
- Module system (`ISnapricotModule`, `IModuleBuilder`, `ModuleLoader`, `ModuleCatalog`).
- In-process event bus with failure isolation.
- Shell with a module-driven sidebar, lazy pages, and a tray icon.
- `--measure-startup` diagnostics switch.
- Unit tests, architecture tests, and CI on Windows and macOS.
- Documentation: vision, design, brand, roadmap, marketing, hosting, business model, architecture, performance budgets, ADR-0001 to ADR-0010.
- Brand: apricot "snapped slice" logo (SVG plus a small mark), Plum and Apricot palette as app resources, and a logo render script.
