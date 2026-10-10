# Marketing and launch plan

Goals: real users, plus a portfolio piece that shows engineering depth.

## Positioning

- **Tagline:** "Snap it. Mark it. Share it."
- **Pitch:** a fast, free, open-source screenshot and annotation tool, light enough to feel instant.
- **Differentiators:** speed (published startup and memory numbers), clean UI, privacy (no account, no telemetry),
  open source, and cross-platform later.
- **Compared with:** Snipping Tool (more features), ShareX (simpler, cleaner), CleanShot X / Snagit (free, open).

## Before launch (during Phases 1–3): build in public

| Channel | Cadence | Content |
|---|---|---|
| LinkedIn | weekly | Progress plus one lesson ("Week 2: why DPI across monitors is hard") |
| X / Bluesky | 1–2 per week | Short GIFs of new features |
| dev.to / Hashnode | 2 posts | "Building a cross-platform screenshot tool with Avalonia", "Screen capture on Windows with Windows.Graphics.Capture in .NET" |
| GitHub | continuous | Public roadmap (issues plus a project board), good-first-issue labels |

## Assets to prepare

- [ ] README: logo, tagline, **demo GIF at the top**, download button, features, comparison table, badges, architecture diagram
- [x] Landing page on GitHub Pages (`site/`, first version 2026-10-10: features, download, docs, FAQ with SmartScreen and privacy). Still to add: hero GIF and screenshots
- [ ] 30–45 s demo video; 5 screenshots; a social preview image (1280×640)
- [ ] Release notes for v1.0

## Launch week

| Day | Action |
|---|---|
| Mon | GitHub release v1.0, plus a LinkedIn announcement post |
| Tue | **Show HN** around 8–9am US Eastern time; reply to every comment |
| Wed | Reddit: r/windows, r/software, r/dotnet, r/csharp, r/opensource (read each subreddit's self-promotion rules first) |
| Thu | Product Hunt |
| Fri | Avalonia community (GitHub Discussions, Telegram), .NET community channels, and newsletters (e.g. "The Morning Brew") |

## Distribution

- GitHub Releases (Velopack installer), then **winget**, **Scoop**, **Chocolatey**.
- Microsoft Store after v1.1 (also removes the SmartScreen warning).
- Pull requests to awesome-dotnet and awesome-avalonia.

## Metrics

| Metric | 1 month after launch | 3 months |
|---|---|---|
| GitHub stars | 100 | 500 |
| Downloads | 500 | 3,000 |
| Issues opened by people you don't know | 10 | 40 |
| External contributors | 1 | 5 |

## CV entry (fill in the numbers)

> **Snapricot**: open-source screenshot and annotation tool (C#, .NET 10, Avalonia, SkiaSharp). Designed a modular,
> platform-abstracted architecture (Windows/macOS) with an in-process event bus; implemented native screen capture
> with multi-monitor DPI handling; shipped Native AOT builds through CI/CD with performance budgets and auto-update.
> *X stars, Y downloads.*
