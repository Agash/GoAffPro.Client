# GoAffPro.Client — Architecture Specification

**Version:** 1.0  
**Target:** .NET 8.0 (compatibility), .NET 10.0 (primary)  
**Last Updated:** 2026-02-17

---

## 1. Product Intent

GoAffPro.Client is a **standalone NuGet library** that provides a strongly-typed, async-first C# client for the GoAffPro Affiliate Marketing API. It is designed for:

- Independent use in any .NET application
- Integration into StreamWeaver as `StreamWeaver.Integrations.GoAffPro`'s underlying client (per ADR-006)
- Event-driven reactive consumption via `IAsyncEnumerable<T>` streams

**Core goals:**
- **Zero manual maintenance**: OpenAPI spec is fetched and client code regenerated at build time
- **User-facing endpoints only**: `/user` and `/public` paths; **no `/admin` or `/sdk` endpoints** (requires credentials we don't have)
- **Bearer token authentication**: Supports user login flow; no API key support (admin-only)
- **Polling-based events**: Detects new orders, affiliates, rewards via periodic polling and diff comparison
- **Resilient**: Polly-based retry policies for transient failures
- **Testable**: Full unit + integration test coverage; contract tests validate generated code against spec

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                      GoAffPro.Client (NuGet Package)                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────────┐     ┌────────────────────────────────────┐   │
│  │ GoAffProClient   │────→│ GoAffProUserClient.g.cs (generated)│   │
│  │ (public wrapper) │     │ /user/* endpoints                  │   │
│  └────────┬─────────┘     └────────────────────────────────────┘   │
│           │                                                         │
│           ├────────────────→ GoAffProPublicClient.g.cs (generated) │
│           │                  /public/* endpoints                    │
│           │                                                         │
│           └────────────────→ Polly Retry Policies                  │
│                              (429, 5xx retries)                     │
│                                                                     │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │ GoAffProEventDetector                                      │   │
│  │ - NewOrdersAsync() → IAsyncEnumerable<Order>              │   │
│  │ - NewAffiliatesAsync() → IAsyncEnumerable<Affiliate>      │   │
│  │ - NewRewardsAsync() → IAsyncEnumerable<Reward>            │   │
│  │ (Polls API, diffs IDs, yields new entities)               │   │
│  └────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

           │                              │
           │ Consumed via NuGet           │ Consumed via NuGet
           ↓                              ↓
    ┌──────────────┐            ┌────────────────────────┐
    │ Any .NET App │            │ StreamWeaver           │
    │              │            │ .Integrations.GoAffPro │
    └──────────────┘            └────────────────────────┘
```

---

## 3. Project Structure

```
GoAffPro.Client/                     # Root repo
├── src/
│   ├── GoAffPro.Client/
│   │   ├── GoAffProClient.cs        # Main wrapper + auth logic
│   │   ├── IGoAffProClient.cs       # DI interface
│   │   ├── GoAffProClientOptions.cs # Configuration options
│   │   ├── Generated/               # Auto-generated at build time
│   │   │   ├── GoAffProUserClient.g.cs   # NSwag output for /user/*
│   │   │   └── GoAffProPublicClient.g.cs # NSwag output for /public/*
│   │   ├── Events/
│   │   │   ├── GoAffProEventDetector.cs
│   │   │   └── IAsyncEnumerableExtensions.cs
│   │   ├── Policies/
│   │   │   └── RetryPolicies.cs     # Polly policy definitions
│   │   ├── Exceptions/
│   │   │   └── GoAffProApiException.cs
│   │   └── Models/                  # Optional hand-written extensions
│   │       └── (empty by default; add as needed)
│   │
│   └── GoAffPro.Client.Generator/   # MSBuild task project
│       ├── GoAffProClientGeneratorTask.cs
│       ├── SpecExtractor.cs         # Fetches + parses swagger-ui-init.js
│       ├── NSwagInvoker.cs          # Invokes NSwag programmatically
│       └── build/
│           └── GoAffPro.Client.Generator.targets  # MSBuild integration
│
├── tests/
│   ├── GoAffPro.Client.Tests/       # Unit tests
│   └── GoAffPro.Client.IntegrationTests/  # Integration tests (require token)
│
├── openapi/
│   └── goaffpro.openapi.json        # Cached spec (git-ignored)
│
├── README.md
├── ARCHITECTURE.md                  # This document
├── AGENTS.md
├── SETUP.md
├── LICENSE.txt
├── .editorconfig
├── .gitignore
├── global.json
├── Directory.Build.props
└── Directory.Packages.props
```

---

## 4. Code Generation Pipeline

### 4.1 Build-Time Flow

```
MSBuild BeforeBuild target
    ↓
GoAffProClientGeneratorTask.Execute()
    ↓
1. Fetch https://api.goaffpro.com/docs/admin/swagger-ui-init.js
    ↓
2. Extract embedded swaggerDoc JSON via regex
    ↓
3. Write to openapi/goaffpro.openapi.json
    ↓
4. Filter paths:
       KEEP: /user/*, /public/*
       EXCLUDE: /admin/*, /sdk/*
    ↓
5. Generate two separate specs:
       openapi/goaffpro-user.openapi.json
       openapi/goaffpro-public.openapi.json
    ↓
6. Invoke NSwag CLI (embedded via NSwag.Core NuGet):
       nswag openapi2csclient /input:goaffpro-user.openapi.json
         /namespace:GoAffPro.Client.Generated
         /classname:GoAffProUserClient
         /output:Generated/GoAffProUserClient.g.cs

       nswag openapi2csclient /input:goaffpro-public.openapi.json
         /namespace:GoAffPro.Client.Generated
         /classname:GoAffProPublicClient
         /output:Generated/GoAffProPublicClient.g.cs
    ↓
7. Append header comment to each .g.cs file:
       // <auto-generated>
       // This file was generated by GoAffPro.Client.Generator.
       // Source: https://api.goaffpro.com/docs/admin/swagger-ui-init.js
       // Generated: 2026-02-17T12:34:56Z
       // Do not edit manually.
       // </auto-generated>
    ↓
8. CoreCompile proceeds with generated files
```

### 4.2 Caching Strategy

To avoid regenerating on every build when the spec hasn't changed:

- Compute SHA256 hash of `openapi/goaffpro.openapi.json`
- Store hash in `obj/GoAffPro.Client.Generator.hash`
- On next build: fetch spec, hash it, compare to stored hash
- If unchanged: skip NSwag generation
- If changed or missing: regenerate

### 4.3 Force Regeneration

```bash
# Delete cache
rm -rf openapi/ obj/

# Rebuild
dotnet build
```

---

## 5. Authentication

GoAffPro's API uses **bearer token** authentication for user endpoints. Admin endpoints require API keys (not supported in this library).

### 5.1 Login Flow

```csharp
// POST /user/login
{
  "username": "affiliate_user",
  "password": "password123"
}

// Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": { ... }
}
```

The `GoAffProClient.LoginAsync` method:
1. POSTs to `/user/login`
2. Extracts `token` from response
3. Stores it in `GoAffProClient.BearerToken`
4. All subsequent requests include `Authorization: Bearer {token}`

### 5.2 Token Lifecycle

- **Expiry:** Not documented by GoAffPro. Observed to last ~30 days.
- **Refresh:** No refresh token mechanism. Re-login required after expiry.
- **Storage:** Caller's responsibility. `GoAffProClient` holds the token in-memory only.

If a request returns `401 Unauthorized`, the caller must re-authenticate and create a new client instance.

---

## 6. Event Detection

The `GoAffProEventDetector` provides a polling-based change detection mechanism. It periodically fetches the latest entities and yields only new ones as an `IAsyncEnumerable<T>` stream.

### 6.1 Algorithm

```csharp
var seenOrderIds = new HashSet<string>();

while (!cancellationToken.IsCancellationRequested)
{
    var orders = await client.User.GetOrdersAsync(limit: 100);
    
    foreach (var order in orders)
    {
        if (seenOrderIds.Add(order.Id))  // Returns true if newly added
        {
            yield return order;
        }
    }

    await Task.Delay(pollingInterval, cancellationToken);
}
```

### 6.2 Configuration

```csharp
var detector = new GoAffProEventDetector(
    client,
    pollingInterval: TimeSpan.FromSeconds(30),
    pageSize: 100  // Max entities per poll
);
```

### 6.3 State Persistence

The detector's seen-ID sets are **in-memory only**. Restarting the detector will re-emit all entities on the first poll.

For persistent state, wrap the detector:

```csharp
var persistentSeenIds = await LoadFromDatabaseAsync();

await foreach (var order in detector.NewOrdersAsync(ct))
{
    if (!persistentSeenIds.Add(order.Id)) continue;
    
    ProcessOrder(order);
    await SaveToDatabaseAsync(persistentSeenIds);
}
```

---

## 7. Retry Policies

All HTTP requests pass through Polly policies defined in `RetryPolicies.cs`.

### 7.1 Transient Retry Policy

```csharp
Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => 
        r.StatusCode == HttpStatusCode.TooManyRequests ||
        r.StatusCode >= HttpStatusCode.InternalServerError)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, attempt, context) =>
        {
            Log.Warning($"Retry {attempt} after {timespan.TotalSeconds}s");
        });
```

Retries on: 429, 500, 502, 503, 504, `HttpRequestException`.

### 7.2 Circuit Breaker Policy

```csharp
Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromMinutes(1));
```

After 5 consecutive failures, the circuit opens for 1 minute. Half-opens after 1 minute to test recovery.

---

## 8. Dependency Injection

```csharp
// In Startup.cs or Program.cs
services.AddGoAffProClient(options =>
{
    options.BearerToken = Configuration["GoAffPro:BearerToken"];
    options.BaseUrl = "https://api.goaffpro.com";
    options.Timeout = TimeSpan.FromSeconds(30);
});

// Registers:
// - IGoAffProClient (singleton)
// - IHttpClientFactory for GoAffProClient
// - Polly policies on the named HttpClient
```

---

## 9. Testing Strategy

### 9.1 Unit Tests

- `GoAffProEventDetector` logic (uses fake client returning canned responses)
- `RetryPolicies` behavior (uses Polly test helpers)
- `SpecExtractor` regex parsing (uses fixture JS files)

### 9.2 Integration Tests

Require a valid GoAffPro user bearer token in `appsettings.Test.json`:

```json
{
  "GoAffPro": {
    "BearerToken": "test-token-here"
  }
}
```

Run with:
```bash
dotnet test --filter "Category=Integration"
```

Tests:
- Login flow
- Fetch profile, stats, orders
- Rate limit handling (429 responses)

### 9.3 Contract Tests

Validate that generated client methods match the OpenAPI spec:

- Spec declares `GET /user/profile` → generated client has `GetProfileAsync()`
- Spec declares response schema → generated DTO matches

Uses a snapshot-based approach: generate client, serialize method signatures, compare to committed snapshot. Fails on drift.

---

## 10. NuGet Packaging

### 10.1 Package Metadata

```xml
<PropertyGroup>
  <PackageId>GoAffPro.Client</PackageId>
  <Version>2026.2.17</Version> <!-- Calendar versioning -->
  <Authors>Your Name</Authors>
  <Description>A modern .NET client for the GoAffPro Affiliate Marketing API with automatic code generation and event detection.</Description>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/your-org/GoAffPro.Client</PackageProjectUrl>
  <RepositoryUrl>https://github.com/your-org/GoAffPro.Client</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageTags>goaffpro;affiliate;marketing;api;client;dotnet</PackageTags>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>
```

### 10.2 Build-Time Generation in Consumer Projects

When a consuming project references `GoAffPro.Client`, the MSBuild targets **do not run** in the consumer — only in the `GoAffPro.Client` project itself.

The NuGet package includes:
- Compiled `GoAffPro.Client.dll` (with embedded generated code)
- Dependencies: `NSwag.Core`, `Polly`, `System.Text.Json`

**No source generators** are deployed to consumers. All generation happens at `GoAffPro.Client` build time, before packing.

---

## 11. Versioning

Uses **calendar versioning** (`YYYY.M.D`) to signal the OpenAPI spec generation date.

Example:
- `2026.2.17` — generated from the spec as of Feb 17, 2026
- `2026.2.17.1` — patch release (bug fix, no spec regeneration)

When the spec changes (detected by hash diff), bump to the current date.

---

## 12. CI/CD

### 12.1 GitHub Actions Workflow

```yaml
# .github/workflows/ci.yml
name: CI

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --configuration Release
      - run: dotnet test --configuration Release --no-build
      - run: dotnet pack --configuration Release --no-build --output nupkgs/
      - uses: actions/upload-artifact@v4
        with:
          name: nupkg
          path: nupkgs/*.nupkg

  publish:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: nupkg
      - run: dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.NUGET_API_KEY }}
```

### 12.2 Scheduled Spec Regeneration

```yaml
# .github/workflows/regen-spec.yml
name: Regenerate OpenAPI Spec

on:
  schedule:
    - cron: '0 6 * * 1'  # Weekly Monday 6am UTC
  workflow_dispatch:     # Manual trigger

jobs:
  regen:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: rm -rf openapi/ obj/
      - run: dotnet build
      - run: |
          if git diff --quiet openapi/; then
            echo "No spec changes"
          else
            git config user.name "GitHub Actions"
            git config user.email "actions@github.com"
            git add openapi/
            git commit -m "chore: regenerate OpenAPI spec"
            git push
          fi
```

---

## 13. Open Questions

| # | Question | Status |
|---|----------|--------|
| OQ-1 | Should we support webhook-based events instead of polling? | Deferred (requires public HTTPS endpoint) |
| OQ-2 | Should the event detector persist seen IDs to disk by default? | Deferred (caller's responsibility for now) |
| OQ-3 | Should we multi-target `netstandard2.0` for broader compatibility? | No (requires `net8.0` minimum for `IAsyncEnumerable<T>`) |

---

## 14. References

- [GoAffPro API Docs](https://api.goaffpro.com/docs/admin/)
- [NSwag Documentation](https://github.com/RicoSuter/NSwag/wiki)
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [IAsyncEnumerable<T>](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/async-scenarios#async-streams)
