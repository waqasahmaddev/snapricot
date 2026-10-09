# ADR-0005: Modules communicate through an in-process event bus

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

Features react to each other: History records every capture, the Editor opens after a capture, OCR runs on a
selection. Direct references between modules would couple them (ADR-0004 forbids that).

## Decision

- `IEventBus` in Core: `Subscribe<TEvent>(handler)` returns an `IDisposable`; `PublishAsync<TEvent>(event)`.
- Events are **past-tense, immutable records** defined in Core (`CaptureCompleted`, `CaptureSaved`).
- Delivery is in-process, sequential, in subscription order. Handler lists are copy-on-write, so publishing never holds a lock.
- A failing handler is **logged and skipped**; cancellation propagates.
- Request/response between features is not done through the bus: use an extension point or a Core interface instead.

## Consequences

- Publishers don't know their subscribers, so features can be added or removed freely.
- Flows are less explicit than direct calls, so key events and their subscribers are documented in the architecture overview.
- Sequential delivery is simple and deterministic. A slow handler delays the ones after it, so handlers must offload
  heavy work.

## Alternatives considered

- **Direct module references:** coupling (rejected by ADR-0004).
- **MediatR or similar:** an extra dependency, reflection-heavy registration, and more than we need.
- **Parallel delivery:** non-deterministic ordering; not needed yet.
