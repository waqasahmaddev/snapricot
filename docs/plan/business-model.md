# Business model

**Now:** completely free and open source (MIT).
**Later, if there's traction:** an optional **Pro** edition, using an **open-core** model (proposed in ADR-0010).

## Principles

1. **The free version stays genuinely useful forever.** Everything planned for v1.x (capture, editor, history, OCR,
   pin) stays free. Nothing free gets moved behind a paywall later.
2. **Pro adds new things, mostly ones that cost money to run or take a lot of effort to build.**
3. **No ads, no telemetry, no nagging.** A single "Get Pro" item in the About and Settings pages is enough.

## When to consider Pro

Pro is worth considering when any of these happens. Until then, focus on users and stars.

- About 1,000 monthly active users, or about 1,000 GitHub stars
- Repeated feature requests for things that cost money to run (cloud links, AI)
- Companies asking for team or branding features

## Possible Pro features

| Feature | Why it belongs in Pro |
|---|---|
| Instant share links (upload, short URL, expiry) | Needs storage and bandwidth |
| AI: describe a screenshot, auto-blur personal info, translate text | Each use costs money (API calls) |
| Scrolling capture plus long-page stitching | A lot of engineering effort |
| Advanced recording (MP4 with webcam, mic, and system audio; trimming) | A lot of engineering effort |
| Team branding: watermark, templates, shared presets | Business users |
| Cloud sync of settings and history across devices | Needs servers |

## How it fits the architecture

The module system ([ADR-0004](../adr/0004-modular-architecture.md)) already supports this:

- Pro features are ordinary **modules** that live in a separate **private repo** (`snapricot/snapricot-pro`).
- The public repo builds the **free edition**. A private CI job builds the **Pro edition** by adding the Pro module
  projects to `InstalledModules` (behind a build flag, e.g. `SNAPRICOT_PRO`).
- A small `Licensing` component validates an offline-signed license key (e.g. Ed25519) at startup. Pro modules only
  register if a valid license is present.
- Core stays MIT. Anyone can read and fork the free edition; Pro code is never published.

## Pricing (to validate later)

- Ideas to test: a **one-time purchase of ~$19–29** (popular for utilities), or **$3–5 per month** if Pro includes cloud features.
- Optional "supporter" tier: GitHub Sponsors or Buy Me a Coffee, even before Pro exists.
- Payments go through a **merchant of record** (Lemon Squeezy or Paddle), which handles checkout, VAT/sales tax,
  invoices, and license keys, so no company setup is needed abroad on day one.

## Before launching Pro (checklist)

- [ ] Decide whether the free edition's license stays MIT (default: yes)
- [ ] Register the name or trademark if revenue is meaningful
- [ ] Write a privacy policy and terms (required by payment providers)
- [ ] Accept ADR-0010 (open-core) or write a replacement
