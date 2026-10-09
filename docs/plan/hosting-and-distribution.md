# Hosting, domain, and distribution

Decided on 2026-10-09. Everything below is free except the optional domain.

## GitHub repository: public

**Decision: public, from the first push.**

- A private repo is invisible to recruiters, so it does nothing for the CV.
- "Build in public" is part of the [marketing plan](marketing.md); a public repo's history, issues, and ADRs are the proof.
- Free CI minutes on public repos are unlimited (private repos have a monthly limit, and macOS minutes cost 10× more).
- Open-source code signing programs (e.g. SignPath Foundation) require a public OSS repository.
- A future Pro edition doesn't require the core to be private (see [business-model.md](business-model.md)).

**Where:** the owner's **personal** account, **`github.com/waqasahmaddev/snapricot`**, the same as the `flow` project.
Never use the office account (`Waqas-Ahmad_fust`), which the `gh` CLI on the dev machine is logged in as.

The local repo is already configured like `flow`:

| Setting | Value |
|---|---|
| `user.name` / `user.email` (local) | `Waqas Ahmad` / `waqas.ahmad6253@gmail.com` |
| `origin` | `git@github-personal:waqasahmaddev/snapricot.git` (the `github-personal` SSH alias) |

**First push (the owner does this):** create an empty public repo named `snapricot` on github.com (signed in as
`waqasahmaddev`, with no README or license, since the repo already has them), then:

```bash
git add -A
git commit -m "chore: initial scaffold"
git push -u origin main
```

Website: a project site at `https://waqasahmaddev.github.io/snapricot` (GitHub Pages) until a domain is bought.
A `snapricot` GitHub organization is optional later (for example if a private `snapricot-pro` repo is added).

Repository settings after the push: protect `main` (require PRs and green CI), enable Discussions, add topics
(`screenshot`, `avalonia`, `dotnet`, `csharp`, `windows`, `open-source`), and upload a social preview image (1280×640).

## Domain: not needed yet; buy before the v1.0 launch

| When | What |
|---|---|
| Now to beta | Free: `https://waqasahmaddev.github.io/snapricot` (GitHub Pages) |
| ~2 weeks before v1.0 | Buy a domain and point it to the same site |

- Check availability for **snapricot.app** (good fit; `.app` enforces HTTPS) or **snapricot.com**. Typical cost is
  about $10–20 per year.
- Recommended registrars: **Cloudflare Registrar** (sells at cost, no markup on renewal) or **Porkbun**.
- Optional: once you buy the domain, also register `getsnapricot.com` and keep the GitHub organization name.

## Free hosting

| What | Where | Cost |
|---|---|---|
| Source, issues, discussions | GitHub | Free |
| CI/CD (Windows and macOS builds) | GitHub Actions | Free for public repos |
| Website / landing page | **GitHub Pages** (or Cloudflare Pages) | Free; custom domain and HTTPS included |
| Installers and downloads | **GitHub Releases** | Free; no bandwidth bill |
| Auto-update feed | GitHub Releases, via Velopack (ADR-0009) | Free |
| Package managers | winget, Scoop, Chocolatey; Homebrew later | Free |
| Documentation | The website, or the `docs/` folder rendered on GitHub | Free |
| Code signing (Windows) | SignPath Foundation (OSS program), otherwise Azure Trusted Signing (~$10/month) | Free or low cost |

Total until launch: **$0**. After launch: about **$15 per year** for the domain (plus ~$10/month only if paid signing is needed).

## If a Pro edition happens

Pro downloads and license checks need a payment provider, not a server. A **merchant of record** (Lemon Squeezy or
Paddle) handles checkout, VAT/sales tax, and license keys. The app validates keys offline. See [business-model.md](business-model.md).
