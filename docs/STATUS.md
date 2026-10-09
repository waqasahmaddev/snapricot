# Project status

> The single source of truth for "where are we?". Update it at the end of every working session.

**Current phase:** Phase 0 (Foundation): code scaffold and brand done, waiting on the first commit and the GitHub repo
**Next phase:** Phase 1 (Capture)
**Target:** v1.0 (Windows) about 8 weeks after Phase 1 starts, at ~8–10 hours per week

![Phase 0 shell](images/phase0-shell.png)

## Phase 0 checklist

- [x] Name chosen: Snapricot ([vision](product/vision.md))
- [x] UI mockups agreed ([design](product/design.md))
- [x] Brand: logo (apricot with a snapped slice) and palette ([brand](product/brand.md))
- [x] Build and marketing plan written ([roadmap](plan/roadmap.md), [marketing](plan/marketing.md))
- [x] Hosting and business decisions ([hosting](plan/hosting-and-distribution.md), [business model](plan/business-model.md))
- [x] Architecture decided and recorded (ADR-0001 to ADR-0010)
- [x] Solution scaffold: Core, Platform.Windows, Platform.MacOS, App, Screenshot module
- [x] Module system: `ISnapricotModule`, `IModuleBuilder`, `ModuleLoader`, `ModuleCatalog`
- [x] In-process event bus with tests
- [x] Shell: Plum sidebar built from modules, lazy pages, tray icon (closing hides to tray)
- [x] Brand colors as app resources (`BrandApricotDeepBrush`, `BrandPlumBrush`, `BrandTintBrush`, ...)
- [x] Unit tests (13) and architecture tests (6) passing
- [x] `--measure-startup` diagnostics switch
- [x] CI workflow: build + test on Windows and macOS, Native AOT publish and startup measurement on Windows
- [ ] Install the VS "Desktop development with C++" workload locally to verify the AOT build and its startup time
- [x] Local git identity and `origin` set to the personal account (`waqasahmaddev`), the same as `flow`
- [ ] Owner creates the public repo `waqasahmaddev/snapricot`, commits, and pushes (**the owner commits; agents don't**)
- [ ] Confirm CI is green on GitHub; protect `main`
- [ ] Check domain availability (snapricot.app / snapricot.com); buy before the v1.0 launch, not now

## Known issues and notes

- **Local Native AOT publish fails** with "Platform linker not found" until the C++ workload is installed.
  The IL compilation step produced no AOT/trim warnings. CI runners already have the linker.
- Debug (JIT) baseline on the dev machine: **~1.28 s to window open, ~90 MB working set**. This is not the release
  number; budgets apply to the AOT build (see [performance](architecture/performance.md)).
- The kickoff mockups use the old palette; [brand.md](product/brand.md) overrides them.
- **Light theme trial in progress:** the shell now uses a white/off-white light theme with apricot hover and selection
  (`App.axaml`: `Shell*Brush` keys and the `ListBox.nav` styles). The owner is reviewing the colors live. Once they're
  final, update brand.md and design.md (they still describe the Plum sidebar).
- An **About** module was added (`Snapricot.Modules.About`). The 4 extra tests come from it being registered in the architecture tests (23 tests total).

## Session log

Newest first. One entry per session: what changed and what's next.

### 2026-10-09 (later): Brand refresh, hosting, and business plan
- The first logo (orange square with corner brackets) looked too much like Icecream Screen Recorder. Replaced it with an
  **apricot with one slice snapped out inside a dashed selection frame**: no tile, no brackets. SVG source plus a small mark
  for 32 px and below; PNGs come from `tools/brand/Render-Logo.ps1`.
- New palette: Apricot `#F4943C`, Apricot deep `#D9622B` (UI accent), Flesh `#FFC98F`, Leaf `#3E9B6A`, Plum `#2E2240` (sidebar).
  Applied as resource keys in `App.axaml`; the Screenshot module now uses the keys instead of hex values.
- Decided: **public** GitHub repo under a `snapricot` organization; free hosting (GitHub Pages, Releases, Actions); buy a domain
  just before v1.0. Free/MIT now, open-core Pro later (ADR-0010, Proposed).
- **Next:** create the GitHub organization, then the first commit and push, then Phase 1.

### 2026-10-09: Project kickoff
- Picked the name Snapricot. Designed the mockups (capture overlay, editor, tray, settings, main window in the style of Icecream Screen Recorder).
- Decided: Windows first, macOS-ready; modular "everything is a module" architecture; Native AOT and performance budgets.
- Scaffolded the solution, module system, event bus, shell with tray, Screenshot module placeholder, tests, CI, and all docs/ADRs.
