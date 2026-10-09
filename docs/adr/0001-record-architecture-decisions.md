# ADR-0001: Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-10-09

## Context

Work on Snapricot happens across many sessions, often with AI agents that start without memory. Decisions that
live only in chat history get lost and then re-argued.

## Decision

Record every significant decision as an ADR in `docs/adr/`, using the template in `0000-template.md`. Keep
`docs/STATUS.md` as the living "where are we" page, and `AGENTS.md` as the entry point for any new session.

## Consequences

- A new session can get up to speed by reading AGENTS.md, then STATUS.md, then the relevant ADRs.
- Writing a short ADR is a small, recurring cost.
- The repo shows decision-making to reviewers and employers.

## Alternatives considered

- **A wiki or external docs:** drifts from the code and isn't versioned with it.
- **Code comments only:** they can't explain options that were rejected.
