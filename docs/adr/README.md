# Architecture decision records

An ADR captures one significant decision: the context, what we chose, and what it costs us. ADRs are immutable once
**Accepted**. To change a decision, write a new ADR that supersedes the old one, and set the old one's status to
`Superseded by ADR-XXXX`.

**How to add one:** copy [0000-template.md](0000-template.md) to `NNNN-short-title.md` with the next number, fill it in, and add a row below.

| # | Title | Status | Date |
|---|---|---|---|
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted | 2026-10-09 |
| [0002](0002-dotnet-10-and-avalonia-12.md) | .NET 10 and Avalonia 12 for the UI | Accepted | 2026-10-09 |
| [0003](0003-windows-first-macos-ready.md) | Windows first, macOS-ready | Accepted | 2026-10-09 |
| [0004](0004-modular-architecture.md) | Every feature is a module, registered explicitly | Accepted | 2026-10-09 |
| [0005](0005-in-process-event-bus.md) | Modules communicate through an in-process event bus | Accepted | 2026-10-09 |
| [0006](0006-native-aot-and-performance-budgets.md) | Native AOT and performance budgets | Accepted | 2026-10-09 |
| [0007](0007-platform-abstraction.md) | OS services behind platform interfaces | Accepted | 2026-10-09 |
| [0008](0008-windows-capture-api.md) | GDI BitBlt first, Windows.Graphics.Capture as fallback | Accepted | 2026-10-09 |
| [0009](0009-velopack-packaging.md) | Velopack for installers and auto-update | Proposed | 2026-10-09 |
| [0010](0010-open-core-pro-edition.md) | Open-core model for a possible Pro edition | Proposed | 2026-10-09 |
| [0011](0011-printscreen-default-hotkeys.md) | Default hotkeys use PrintScreen combinations | Accepted | 2026-10-09 |
| [0012](0012-json-settings-store.md) | User settings in one JSON file, one section per module | Accepted | 2026-10-09 |
