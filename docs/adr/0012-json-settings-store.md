# ADR-0012: User settings in one JSON file, one section per module

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

Snapricot must remember choices across restarts, starting with the default save folder and the last folder used
(the Save dialog's "Always save here"). Every module will need settings later, and everything must stay Native AOT safe.

## Decision

- `ISettingsStore` in Core with `Load<T>(section, JsonTypeInfo<T>, defaults)` and `Save<T>(section, value, JsonTypeInfo<T>)`.
- `JsonFileSettingsStore` keeps **one file**, `%APPDATA%\Snapricot\settings.json`, with **one JSON section per module**
  (e.g. `"screenshot"`). Each module owns its settings record and a source-generated `JsonSerializerContext`.
- Writes are atomic (write a temp file, then replace). A missing, corrupt, or wrongly-shaped file or section means
  "use defaults"; the next save repairs it.
- Settings records only add optional properties with sensible defaults, so old files keep working.
- **Settings records use `{ get; set; }` properties, never `init`.** Source-generated System.Text.Json fills missing
  `init`-only properties with `default` (e.g. `false`), silently dropping `= true` initializers when an older file lacks
  the property. Found on 2026-10-09; covered by `OldSettingsFile_MissingNewOptions_KeepsTheirDefaults`.

## Consequences

- No reflection-based serialization (AOT-safe), no database, human-readable, easy to back up or reset (delete the file).
- Not designed for large or high-frequency data. History metadata gets its own storage when the History module arrives.
- One file shared by modules: sections must use the module id prefix to avoid collisions.

## Alternatives considered

- **Microsoft.Extensions.Configuration plus writable options:** read-oriented, writing back is awkward, and it uses reflection binding.
- **SQLite:** overkill for a handful of preferences.
- **Windows registry:** Windows-only; breaks the macOS path (ADR-0003).
