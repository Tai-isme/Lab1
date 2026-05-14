# External Integrations

**Analysis Date:** 2026-05-14

## APIs & External Services

**None detected:**
- The codebase consists of 3 standalone ASP.NET Core Web API projects with no outgoing HTTP calls or external API integrations
- No `HttpClient`, `IHttpClientFactory`, or typed client registrations present
- No third-party API SDKs (Stripe, SendGrid, Twilio, etc.) referenced

## Data Storage

**Databases:**
- Not configured — No database connection strings in `appsettings.json`
- No Entity Framework Core, Dapper, or any ORM packages referenced
- No SQL, NoSQL, or in-memory database setup

**File Storage:**
- Local filesystem only — No blob/object storage integration

**Caching:**
- None — No Redis, MemoryCache, or distributed cache setup

## Authentication & Identity

**Auth Provider:**
- None — No authentication middleware configured
- `app.UseAuthorization()` is called in `Program.cs` but no `AddAuthentication()` or `AddIdentity()` services registered
- No JWT, OAuth, OpenID Connect, or cookie auth setup

## Monitoring & Observability

**Error Tracking:**
- None — No Sentry, Application Insights, or similar SDKs

**Logs:**
- `Microsoft.Extensions.Logging` (built-in) — Standard ASP.NET Core logging
- Log level: `Information` (Default), `Warning` (Microsoft.AspNetCore)

## CI/CD & Deployment

**Hosting:**
- Not configured — No Dockerfile, no cloud deployment manifests

**CI Pipeline:**
- None — No GitHub Actions, Azure Pipelines, or other CI config detected

## Environment Configuration

**Required env vars:**
- `ASPNETCORE_ENVIRONMENT` — Set per launch profile (Development/Production)

**Secrets location:**
- No secrets mechanism configured — No User Secrets, Azure Key Vault, or env-file solutions detected

## Webhooks & Callbacks

**Incoming:**
- None detected

**Outgoing:**
- None detected

## External NuGet Dependencies

| Package | Version | Source |
|---------|---------|--------|
| `Swashbuckle.AspNetCore` | 6.6.2 | NuGet.org (implied) |

No other external package dependencies exist across all 3 projects.

---

*Integration audit: 2026-05-14*
