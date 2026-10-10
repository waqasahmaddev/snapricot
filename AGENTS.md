# Snapricot: guide for AI agents and new contributors

Snapricot (pronounced **SNAP-ri-kot**) is a lightweight, open-source screenshot and annotation tool built with
**.NET 10 + Avalonia 12**. v1 ships on **Windows only**; macOS follows in v2, and the code is structured for it from day one.

## Start every session here

1. Read **[docs/STATUS.md](docs/STATUS.md)**: current phase, what's done, what's next, and the session log.
2. Skim **[docs/architecture/overview.md](docs/architecture/overview.md)** if you will touch code.
3. Check **[docs/adr/](docs/adr/README.md)** before changing anything that an ADR covers.
4. At the end of a session, **update docs/STATUS.md** (move finished items, add a session log entry).

## Map

| Need | Read |
|---|---|
| What we're building and why | [docs/product/vision.md](docs/product/vision.md) |
| What the UI looks like | [docs/product/design.md](docs/product/design.md) |
| Logo, colors, voice (**use the brand resource keys, never raw hex**) | [docs/product/brand.md](docs/product/brand.md) |
| Phases, milestones, checklists | [docs/plan/roadmap.md](docs/plan/roadmap.md) |
| Launch and marketing plan | [docs/plan/marketing.md](docs/plan/marketing.md) |
| GitHub (public), domain, free hosting | [docs/plan/hosting-and-distribution.md](docs/plan/hosting-and-distribution.md) |
| Free now, Pro later (open core) | [docs/plan/business-model.md](docs/plan/business-model.md) |
| Layers, module system, dependency rules | [docs/architecture/overview.md](docs/architecture/overview.md) |
| How to add a feature module | [docs/architecture/adding-a-module.md](docs/architecture/adding-a-module.md) |
| Performance budgets and how we measure them | [docs/architecture/performance.md](docs/architecture/performance.md) |
| Why a decision was made | [docs/adr/](docs/adr/README.md) |

## Commands

```bash
dotnet build Snapricot.slnx                    # build everything (warnings are errors)
dotnet test --solution Snapricot.slnx          # unit + architecture tests (Microsoft.Testing.Platform)
dotnet run --project src/Snapricot.App         # run the app
dotnet publish src/Snapricot.App -c Release -r win-x64 -o artifacts/publish/win-x64   # Native AOT build
Snapricot.exe --measure-startup result.json    # write startup time + memory, then exit
```

Native AOT on Windows needs the Visual Studio **Desktop development with C++** workload (for `link.exe`).
Without it, `dotnet build`/`test`/`run` all still work; only the AOT publish fails. CI has it.

## Hard rules

1. **Core stays pure.** `Snapricot.Core` never references Avalonia, CommunityToolkit, OS APIs, or another Snapricot project.
2. **Modules are islands.** A module references `Snapricot.Core` (and UI packages) only. Never another module, a
   `Snapricot.Platform.*` project, or `Snapricot.App`. Modules talk through `IEventBus` events. (ADR-0004, ADR-0005)
3. **OS code lives in platform projects** behind `Snapricot.Core.Platform` interfaces. (ADR-0007)
4. **AOT-safe code only.** No reflection-based discovery, no `Activator.CreateInstance`, no runtime-generated
   serializers. Use compiled bindings (`x:DataType`) in every `.axaml`, and source-generated JSON/logging. (ADR-0006)
5. **Respect the performance budgets** in docs/architecture/performance.md. Startup work must be lazy: a page or
   service that isn't used must not be created.
6. **Never block the UI thread.** Capture, encode, and save off-thread; copy to the clipboard first.
7. **Every significant decision gets an ADR** (copy `docs/adr/0000-template.md`, take the next number, add it to the index).
8. Architecture tests in `tests/Snapricot.Architecture.Tests` enforce rules 1–2. Register new modules there.
9. **Settings records use `{ get; set; }`, never `init`** (source-generated JSON drops `init` defaults; ADR-0012).
10. **Agents never commit or push.** The owner commits. The repo belongs to the personal GitHub account `waqasahmaddev`
   (local identity and `origin` are preconfigured); never use any other GitHub account the `gh` CLI may be logged in as.

## Code conventions

- File-scoped namespaces, primary constructors, `sealed` by default, records for immutable data.
- `[LoggerMessage]` source-generated logging only; never call `logger.LogX` directly.
- Async all the way with `CancellationToken`; never `.Result` / `.Wait()`.
- Prefer singleton registrations; register a module's services inside its `Register` method.
- Package versions live only in `Directory.Packages.props` (central package management).
- Tests: xUnit v3 on Microsoft.Testing.Platform. Name tests `Method_Condition_Expectation`.
- Commits: [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `ci:`).

## Repository layout

```
src/
├── Snapricot.Core/               # contracts: module system, event bus, capture models, platform interfaces
├── Snapricot.Platform.Windows/   # Windows implementations (net10.0-windows10.0.19041.0)
├── Snapricot.Platform.MacOS/     # macOS implementations (empty until v2)
├── Snapricot.App/                # Avalonia shell + composition root (the only project that sees everything)
└── Modules/
    └── Snapricot.Modules.Screenshot/
tests/
├── Snapricot.Core.Tests/
└── Snapricot.Architecture.Tests/
docs/                             # STATUS, product, plan, architecture, ADRs
```
