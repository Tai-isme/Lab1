# PRN232 Lab 1 — LMS REST API

## Project Guide

This is a GSD-managed project. See `.planning/` for full context.

**Current state:** Initialized — ready for Phase 1 planning.
**Mode:** YOLO (auto-approve)
**Granularity:** Coarse (4 phases)

## Quick Reference

| Item | Location |
|------|----------|
| Project context | `.planning/PROJECT.md` |
| Requirements | `.planning/REQUIREMENTS.md` |
| Roadmap | `.planning/ROADMAP.md` |
| Config | `.planning/config.json` |
| Research | `.planning/research/` |
| Codebase map | `.planning/codebase/` |
| State | `.planning/STATE.md` |

## Workflow

1. `/gsd-plan-phase 1` — plan Phase 1 (Foundation)
2. `/gsd-execute-phase 1` — execute Phase 1
3. `/gsd-plan-phase 2` — plan Phase 2, etc.

## Key Architecture Rules

- Strict 3-layer: API → Services → Repositories
- Controllers contain NO business logic
- Repositories contain NO business logic
- 4 model types: Entity (Repositories), Business (Services), Request (API), Response (API)
- Never return Entity models in API responses
- Never use Request/Response models in Repository layer
