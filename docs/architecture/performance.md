# Performance budgets

"Lightweight and very responsive" is a requirement, not an aspiration. Budgets apply to the **Release Native AOT
build** on a mid-range Windows 10/11 machine.

| Metric | Budget | Measured by |
|---|---|---|
| Process start to main window open | **< 300 ms** | `--measure-startup` (CI) |
| Idle working set (tray only) | **< 60 MB** | `--measure-startup` (CI) / Task Manager |
| Hotkey to overlay visible | **< 100 ms** | Phase 1: timestamped log around overlay show |
| Release capture to clipboard | **< 150 ms** at 4K | Phase 1: timestamped log |
| Installer download | **< 25 MB** | CI artifact size |
| History scrolling, 5,000 items | 60 fps, no stalls | Manual check plus a virtualization test |

CI runners are noisy, so CI **reports** these numbers in the job summary but doesn't fail on them. Treat a jump of
20% or more as a regression to investigate.

## Measurements so far

| Date | Build | Startup | Working set | Notes |
|---|---|---|---|---|
| 2026-10-09 | Debug, JIT, dev machine | 1,280 ms | 90 MB | Baseline only; AOT not measured yet (no local C++ linker) |

## Techniques we rely on

- **Native AOT** (ADR-0006): no JIT at startup, smaller memory, single native exe.
- **Compiled bindings** everywhere (`AvaloniaUseCompiledBindingsByDefault` plus `x:DataType`).
- **Lazy everything:** pages are created on first open; module services are singletons resolved on first use.
- **Two-layer overlay:** the frozen screenshot is a static, immutable `Image` (uploaded to the GPU once and cached by the
  compositor); the selection, dim, handles, and label are a separate light layer. A pointer move repaints only that layer,
  and the size label's text is reused while the size is unchanged.
- **Pre-created overlay:** the capture overlay window is created hidden at startup and only shown on the hotkey (Phase 1).
- **Workstation, non-concurrent GC** (`ServerGarbageCollector=false`, `ConcurrentGarbageCollection=false`): a smaller
  footprint for a mostly idle desktop app.
- **Clipboard first, disk later:** never make the user wait for PNG encoding or I/O.
- **Thumbnails, not images,** in History; virtualized lists.
- **No reflection-heavy libraries.** Every new dependency must be AOT/trim compatible (check that the publish has no
  IL2xxx/IL3xxx warnings).
