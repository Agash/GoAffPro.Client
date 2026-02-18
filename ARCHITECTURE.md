# GoAffPro.Client — Architecture

## 1. Product Scope

`GoAffPro.Client` is a standalone NuGet package that provides:

- Build-time generated API clients for GoAffPro `/user/*` and `/public/*` endpoints.
- A higher-level DX wrapper (`GoAffProClient`) over generated clients.
- Polling-based event detection with both:
  - `IAsyncEnumerable<T>` streams
  - classic `.NET` events (`EventHandler<TEventArgs>`)

Admin and SDK endpoints are intentionally excluded.

## 2. Project Layout

```text
src/
  GoAffPro.Client/
    GoAffProClient.cs
    IGoAffProClient.cs
    GoAffProClientOptions.cs
    ServiceCollectionExtensions.cs
    Policies/RetryPolicies.cs
    Exceptions/GoAffProApiException.cs
    Models/
      GoAffProOrder.cs
      GoAffProAffiliate.cs
      GoAffProReward.cs
    Events/
      GoAffProEventDetector.cs
      OrderEvent.cs
      AffiliateEvent.cs
      RewardEvent.cs
      OrderDetectedEventArgs.cs
      AffiliateDetectedEventArgs.cs
      RewardDetectedEventArgs.cs
    Generated/
      GoAffProUserClient.g.cs
      GoAffProPublicClient.g.cs

  GoAffPro.Client.Generator/
    GoAffProClientGeneratorTask.cs
    GeneratorRunner.cs
    SpecExtractor.cs
    GeneratorOptions.cs
    build/GoAffPro.Client.Generator.targets

tests/
  GoAffPro.Client.Tests/
  GoAffPro.Client.IntegrationTests/
examples/
  GoAffPro.Client.Example/
```

## 3. Runtime Architecture

### 3.1 Wrapper Layer

`GoAffProClient` owns a configured `HttpClient` and exposes:

- `User` (`GoAffProUserClient`) generated client
- `PublicApi` (`GoAffProPublicClient`) generated client
- DX wrapper methods:
  - `LoginAsync`
  - `SetBearerToken`
  - `GetOrdersAsync`
  - `GetAffiliatesAsync`
  - `GetRewardsAsync`

Feed wrapper methods return typed model records:

- `GoAffProOrder`
- `GoAffProAffiliate`
- `GoAffProReward`

Note: `GetRewardsAsync` is temporarily disabled because `/user/feed/rewards` is currently returning `404` (observed on 2026-02-18). The method is marked obsolete and returns an empty list.

Important rule: wrapper methods call generated clients; they do not issue ad-hoc endpoint-specific HTTP requests.

### 3.2 Error Model

Generated client exceptions are mapped to `GoAffProApiException` for a stable top-level exception surface.

### 3.3 Resilience

All HTTP calls flow through Polly policies in `RetryPolicies.cs`:

- transient retry
- circuit breaker

## 4. Event Detector

`GoAffProEventDetector` is stateful in-memory only (no disk persistence):

- Seen IDs tracked in `HashSet<string>`.
- Polling interval and page size are configurable.
- Supports:
  - `NewOrdersAsync`, `NewAffiliatesAsync`, `NewRewardsAsync`
  - `StartAsync` with `OrderDetected`, `AffiliateDetected`, `RewardDetected` events
    (event args expose typed models directly)

Note: reward polling/event emission is temporarily disabled for the same `/user/feed/rewards` `404` issue.

Persistence of seen IDs is caller responsibility.

## 5. Build-Time Generation

Generation is executed by MSBuild task target before compile:

1. Load `openapi/swagger-ui-init.js` (fallback to remote URL).
2. Extract embedded OpenAPI JSON (`swaggerDoc`).
3. Filter to `/user/*` and `/public/*`.
4. Normalize schema gaps (especially endpoints with missing response schemas).
5. Generate NSwag C# clients.
6. Write generated files into `src/GoAffPro.Client/Generated`.

Caching:

- Spec hash stored under `obj/GoAffPro.Client.Generator.hash`.
- Cache key includes generator cache version to force regeneration when generation logic changes.

## 6. Tooling / IDE Behavior

Generated files are real `.g.cs` source files in the client project.
This means Visual Studio/Rider can index symbols after generation and regular builds, and packaged output is a normal compiled `GoAffPro.Client.dll`.

## 7. Testing Strategy

### 7.1 Unit Tests

Cover:

- wrapper login/feed behavior
- detector async streams
- detector event handlers
- contract snapshot drift checks for generated method signatures

### 7.2 Integration Tests

- marked with `[Trait("Category", "Integration")]`
- use `GOAFFPRO_TEST_TOKEN` when available
- executed via:

```bash
dotnet test --filter "Category=Integration"
```

### 7.3 Contract Snapshot

Generated signature snapshot file:

- `tests/GoAffPro.Client.Tests/Snapshots/GeneratedClientSignatures.snapshot`

Build/test fails if generated public method signatures drift without snapshot update.

## 8. Packaging

The distributable package contains compiled `GoAffPro.Client` assemblies for `net8.0` and `net10.0`, with generated code already compiled into the package output.
