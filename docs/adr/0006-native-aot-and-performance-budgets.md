# ADR-0006: Native AOT and performance budgets

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

The app must feel instant and stay light (the bar is Icecream Screen Recorder's responsiveness). A JIT-compiled .NET
desktop app measured ~1.3 s and ~90 MB at startup in Debug on the dev machine.

## Decision

- Publish `Snapricot.App` with **Native AOT** (`PublishAot=true`). Libraries set `IsAotCompatible=true` so AOT/trim
  analyzers run on every build (warnings are errors).
- Adopt the budgets in [performance.md](../architecture/performance.md) (startup < 300 ms, idle < 60 MB, hotkey to
  overlay < 100 ms, installer < 25 MB).
- Add the `--measure-startup <file>` switch; CI publishes AOT, runs it, and reports the numbers in the job summary.
- Coding rules: compiled bindings only, no reflection-based discovery or serialization, `[LoggerMessage]` logging,
  lazy creation, workstation non-concurrent GC.

## Consequences

- Fast cold start and a smaller footprint; a single native executable.
- Some libraries are off-limits (reflection-heavy ones); every new dependency needs an AOT check.
- Local AOT publish on Windows needs the Visual Studio C++ workload (for `link.exe`).
- Avalonia's previewer and hot reload still work, because they use the normal JIT build.

## Alternatives considered

- **ReadyToRun + trimming:** faster than plain JIT, but still loads the runtime; startup and memory remain higher.
- **No budgets, optimize later:** performance regressions accumulate silently.
