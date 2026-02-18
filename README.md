# GoAffPro.Client

**A modern .NET 10 client library for the GoAffPro Affiliate Marketing API**

[![NuGet](https://img.shields.io/nuget/v/GoAffPro.Client.svg)](https://www.nuget.org/packages/GoAffPro.Client/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

GoAffPro.Client is a strongly-typed, async-first C# client for interacting with the [GoAffPro API](https://api.goaffpro.com/docs/admin/). It provides:

- **Automatic code generation** from the live OpenAPI specification at build time
- **User (`/user`) and public endpoint support** with bearer token authentication
- **Polling-based event detection** for new orders, affiliates, and rewards
- **`IAsyncEnumerable<T>` event streams** for reactive consumption
- **Polly-based retry policies** for transient failures
- **Strongly-typed DTOs** with nullability annotations

> **Note:** Admin (`/admin`) and SDK (`/sdk`) endpoints are **not included** in this library. Admin endpoints require API keys not accessible to end-user tokens; SDK endpoints require native app headers.

---

## Installation

```bash
dotnet add package GoAffPro.Client
```

Requires: **.NET 8.0 or later** (multi-targets `net8.0` and `net10.0`)

---

## Quick Start

### 1. Authenticate and Create a Client

GoAffPro supports user-level authentication via bearer tokens obtained from the `/user/login` endpoint.

```csharp
using GoAffPro.Client;

// Option 1: Direct token (if you already have one)
var client = new GoAffProClient(bearerToken: "your-user-bearer-token");

// Option 2: Login with username/password (obtains token automatically)
var client = await GoAffProClient.LoginAsync("username", "password");

// Option 3: Use dependency injection
services.AddGoAffProClient(options =>
{
    options.BearerToken = "your-token";
    options.BaseUrl = "https://api.goaffpro.com"; // optional, this is default
});
```

### 2. Fetch Affiliate Data

```csharp
// Get current user's affiliate profile
var profile = await client.User.GetProfileAsync();
Console.WriteLine($"Affiliate: {profile.Name}, Commission: {profile.CommissionRate}%");

// Get referral stats
var stats = await client.User.GetStatsAsync();
Console.WriteLine($"Total Clicks: {stats.TotalClicks}, Conversions: {stats.TotalConversions}");

// List recent orders
var orders = await client.User.GetOrdersAsync(limit: 50);
foreach (var order in orders)
{
    Console.WriteLine($"Order {order.Id}: ${order.Total}, Commission: ${order.Commission}");
}
```

### 3. Poll for New Orders (Event-Driven)

The library includes a polling-based event detector that surfaces new entities as `IAsyncEnumerable<T>`.

```csharp
using GoAffPro.Client.Events;

var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromSeconds(30));

// Subscribe to new orders
await foreach (var order in detector.NewOrdersAsync(cancellationToken))
{
    Console.WriteLine($"🎉 New order detected: {order.Id}, ${order.Total}");
    // Send notification, trigger automation, etc.
}
```

The event detector compares the latest poll results against previously seen IDs and yields only new entities. It tracks state in-memory per detector instance (no persistent storage).

### 4. Use with Reactive Extensions (Rx)

Convert the `IAsyncEnumerable<T>` stream to `IObservable<T>` for use with Rx operators.

```csharp
using System.Reactive.Linq;

var orderObservable = detector.NewOrdersAsync(ct).ToObservable();

orderObservable
    .Throttle(TimeSpan.FromSeconds(5))
    .Subscribe(order => Console.WriteLine($"Order: {order.Id}"));
```

---

## Architecture

```
GoAffPro.Client/
├── Generated/                       # Auto-generated at build time
│   ├── GoAffProUserClient.g.cs      # User endpoint client (from OpenAPI /user paths)
│   └── GoAffProPublicClient.g.cs    # Public endpoint client (from OpenAPI /public paths)
├── GoAffProClient.cs                # Main client wrapper with auth and DI hooks
├── Events/
│   ├── GoAffProEventDetector.cs     # Polling-based change detection
│   └── IAsyncEnumerableExtensions.cs
├── Policies/
│   └── RetryPolicies.cs             # Polly policies (transient retry, circuit breaker)
└── Models/                          # Hand-written domain helpers (optional)
    └── UserProfile.Extensions.cs
```

### Code Generation Flow

1. **Build-time MSBuild task** (`GoAffPro.Client.Generator.targets`) runs before `CoreCompile`.
2. Task fetches `https://api.goaffpro.com/docs/admin/swagger-ui-init.js`.
3. Extracts the embedded `swaggerDoc` JSON object.
4. Filters paths: keeps `/user/*` and `/public/*`; excludes `/admin/*` and `/sdk/*`.
5. Invokes **NSwag** (embedded via `NSwag.Core` NuGet) to generate C# client code.
6. Outputs `GoAffProUserClient.g.cs` and `GoAffProPublicClient.g.cs` to `Generated/`.
7. The generated clients are `partial` classes; customizations go in non-generated files.

Regeneration is automatic on every build if the OpenAPI spec changes. The generator caches the spec hash to skip regeneration when unchanged.

---

## Event Detection

The `GoAffProEventDetector` polls the API at a configurable interval and emits new entities as an `IAsyncEnumerable<T>` stream.

### Supported Event Types

| Event Stream | Detects | Method |
|-------------|---------|--------|
| `NewOrdersAsync` | New orders (by `orderId`) | `IAsyncEnumerable<Order>` |
| `NewAffiliatesAsync` | New affiliate signups (by `affiliateId`) | `IAsyncEnumerable<Affiliate>` |
| `NewRewardsAsync` | New rewards issued (by `rewardId`) | `IAsyncEnumerable<Reward>` |

### Internal State

The detector maintains an in-memory `HashSet<string>` of seen IDs per entity type. On each poll:
1. Fetch the latest page of entities (configurable page size, default 100).
2. Filter out IDs already in the seen set.
3. Yield new entities via `IAsyncEnumerable<T>`.
4. Add new IDs to the seen set.

The seen set is **not persisted**. Restarting the detector will re-emit all entities from the first poll. For persistent state, wrap the detector and track seen IDs externally (e.g., in a database or file).

### Example: Persistent State

```csharp
var seenOrderIds = await LoadSeenOrderIdsFromDatabaseAsync();
var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromSeconds(60));

await foreach (var order in detector.NewOrdersAsync(cancellationToken))
{
    if (seenOrderIds.Contains(order.Id)) continue; // Skip if already processed

    Console.WriteLine($"Processing order {order.Id}");
    seenOrderIds.Add(order.Id);
    await SaveSeenOrderIdsToDatabaseAsync(seenOrderIds);
}
```

---

## Configuration

### Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using GoAffPro.Client;

services.AddGoAffProClient(options =>
{
    options.BearerToken = Configuration["GoAffPro:BearerToken"];
    options.BaseUrl = "https://api.goaffpro.com";  // optional
    options.Timeout = TimeSpan.FromSeconds(30);     // optional, default 30s
});

// Inject IGoAffProClient
public class MyService
{
    private readonly IGoAffProClient _client;
    public MyService(IGoAffProClient client) => _client = client;
}
```

### Retry Policies (Polly)

The client includes built-in retry policies for transient HTTP errors (429, 500, 502, 503, 504).

- **Retry policy:** 3 attempts with exponential backoff (2^attempt seconds, max 30s)
- **Circuit breaker:** Opens after 5 consecutive failures; half-opens after 60s

Configure custom policies:

```csharp
services.AddGoAffProClient(options =>
{
    options.BearerToken = token;
    options.RetryPolicy = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
});
```

---

## Authentication

GoAffPro supports two authentication modes for user endpoints:

### 1. Username/Password Login

```csharp
var client = await GoAffProClient.LoginAsync("username", "password", cancellationToken);
// Token is stored internally and used for all subsequent requests
```

The login flow:
1. POST `/user/login` with `{ username, password }`
2. Response: `{ token: "bearer-token-here", ... }`
3. Token stored in `GoAffProClient.BearerToken`
4. All requests include `Authorization: Bearer {token}`

### 2. Pre-Existing Bearer Token

If you already have a token (e.g., from a previous login or external OAuth flow):

```csharp
var client = new GoAffProClient(bearerToken: "your-token");
```

Token expiry is **not automatically handled**. If a request returns `401 Unauthorized`, you must re-authenticate and create a new client instance.

---

## Rate Limiting

GoAffPro does not publish official rate limits. The library includes:
- **Exponential backoff** on `429 Too Many Requests`
- **Automatic retry** with jitter

If you encounter rate limits frequently, increase the `pollingInterval` on `GoAffProEventDetector`.

---

## Error Handling

All API methods throw `GoAffProApiException` on non-success responses.

```csharp
using GoAffPro.Client.Exceptions;

try
{
    var profile = await client.User.GetProfileAsync();
}
catch (GoAffProApiException ex) when (ex.StatusCode == 401)
{
    Console.WriteLine("Unauthorized: token may be expired");
    // Re-authenticate
}
catch (GoAffProApiException ex)
{
    Console.WriteLine($"API error {ex.StatusCode}: {ex.Message}");
    Console.WriteLine($"Response body: {ex.ResponseBody}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

---

## Building from Source

```bash
git clone https://github.com/your-org/GoAffPro.Client.git
cd GoAffPro.Client
dotnet restore
dotnet build
dotnet test
```

The first build will:
1. Fetch the OpenAPI spec from `api.goaffpro.com`
2. Generate `GoAffProUserClient.g.cs` and `GoAffProPublicClient.g.cs`
3. Compile the library

To force regeneration:

```bash
dotnet clean
dotnet build
```

---

## Testing

The library includes:
- **Unit tests** for `GoAffProEventDetector`, `RetryPolicies`, and custom helpers
- **Integration tests** (require a valid bearer token in `appsettings.Test.json`)
- **Contract tests** (validate generated client methods match OpenAPI spec)

Run tests:

```bash
dotnet test
```

Run integration tests (requires credentials):

```bash
export GOAFFPRO_TEST_TOKEN="your-token"
dotnet test --filter "Category=Integration"
```

---

## Versioning

This library uses **calendar versioning** (`YYYY.M.D`) to signal OpenAPI spec regeneration dates.

Example: `2026.2.15` — generated from the OpenAPI spec as of February 15, 2026.

Patch releases (e.g., `2026.2.15.1`) indicate bug fixes with no spec regeneration.

---

## Contributing

Contributions are welcome! Please:
1. Fork the repo
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit changes with [conventional commits](https://www.conventionalcommits.org/)
4. Open a pull request

Before submitting:
- Run `dotnet format` (code style)
- Run `dotnet test` (all tests pass)
- Update this README if adding new features

---

## License

MIT License. See [LICENSE.txt](LICENSE.txt).

---

## Support

- **Issues:** https://github.com/your-org/GoAffPro.Client/issues
- **GoAffPro Docs:** https://docs.goaffpro.com/
- **API Docs:** https://api.goaffpro.com/docs/admin/

---

## Acknowledgments

- **GoAffPro** for providing the API
- **NSwag** for OpenAPI client generation
- **Polly** for resilience policies
