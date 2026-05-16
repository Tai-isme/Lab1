# Phase 08: Error Handling & Response Consistency - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions captured in CONTEXT.md — this log preserves the discussion.

**Date:** 2026-05-16
**Phase:** 08-them-phase-fix-loi-nay
**Mode:** discuss

## Areas Discussed

### Exception Handling Approach
| Option | Selected |
|--------|----------|
| Middleware (UseExceptionHandler) | Yes |
| IExceptionFilter | No |
| IExceptionHandler (.NET 8) | No |

**Rationale:** Middleware catches everything including non-MVC exceptions, simplest approach, most common in ASP.NET Core.

### Error Detail Exposure
| Option | Selected |
|--------|----------|
| Dev/Prod split | Yes |
| Always minimal | No |
| Always verbose | No |

**Rationale:** Stack traces in dev for debugging, generic message in production for security. Standard practice.

### Delete Response Consistency
| Option | Selected |
|--------|----------|
| ApiResponse<object> | Yes |
| Typed DeleteResponse DTO | No |
| Leave as-is | No |

**Rationale:** Explicit ApiResponse<object>.Ok(...) is consistent with envelope pattern, filter skips wrapping, no new DTO file needed.

### Swagger 500 Documentation
| Option | Selected |
|--------|----------|
| Per-endpoint attribute | Yes |
| Global Swagger config | No |
| Skip documentation | No |

**Rationale:** [ProducesResponseType(500)] on every action is most visible to API consumers in Swagger UI.

## Corrections Applied

No corrections — all recommended options accepted.

## Deferred Ideas

- None — discussion stayed within phase scope
