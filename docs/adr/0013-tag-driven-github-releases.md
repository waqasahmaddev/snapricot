# ADR-0013: Tag-driven GitHub Releases

- **Status:** Accepted
- **Date:** 2026-10-10

## Context

Users need a place to download Snapricot. The owner's machine cannot do a Native AOT publish (no C++ linker), but the
GitHub Actions Windows runner can. Releases must be repeatable and must not depend on anyone's local setup.

## Decision

Pushing a tag that starts with `v` runs `.github/workflows/release.yml`, which:

1. runs the tests,
2. publishes the Native AOT `win-x64` build with the version taken from the tag (`-p:Version`),
3. zips it (without `.pdb` files) as `Snapricot-<tag>-win-x64.zip` plus a `.sha256` file,
4. creates the GitHub Release with generated notes and attaches both files.

A tag containing a hyphen (for example `v0.1.0-beta.1`) is marked as a **pre-release** automatically.
Releases live at `https://github.com/waqasahmaddev/snapricot/releases`. Tags matching `v*` are protected by a tag ruleset.

To release: `git tag vX.Y.Z` then `git push origin vX.Y.Z` (the owner does this; agents never tag or push).

## Consequences

- Free hosting and bandwidth, and one repeatable path from tag to download.
- The first release (`v0.1.0-beta.1`, 2026-10-10, about 16 MB zip, build took about 2 minutes) worked on the first run.
- The binary is **unsigned**, so Windows SmartScreen shows "Windows protected your PC" (More info, then Run anyway).
  The README documents this. Code signing is the real fix: apply to the SignPath Foundation OSS programme first,
  Azure Trusted Signing as the fallback. Once approved, add a signing step to `release.yml`.
- There is no installer or auto-update yet; Velopack (ADR-0009) will extend this workflow.

## Alternatives considered

- **Manual releases from the owner's machine:** needs the C++ workload locally and is easy to get wrong.
- **Releasing from every push to `main`:** noisy, and every build would look like a public release.
