# GoAffPro.Client

Async-first .NET client for the GoAffPro API with build-time NSwag generation and polling/event-based change detection.

## Targets

- `net8.0`
- `net10.0` (working on native-AOT compatibility)

## Install

```bash
dotnet add package GoAffPro.Client
```

## Quick Start

```csharp
using GoAffPro.Client;

// Option 1: create from existing token
await using var client = new GoAffProClient(new GoAffProClientOptions
{
    BearerToken = "your-access-token",
});

// Option 2: login and create
await using var loggedInClient = await GoAffProClient.CreateLoggedInAsync(
    email: "affiliate@example.com",
    password: "password123");
```

### Wrapper Methods (DX Layer)

The wrapper methods are built on top of generated clients:

```csharp
// Fetch orders with optional time filtering
var orders = await client.GetOrdersAsync(from: DateTimeOffset.UtcNow.AddDays(-1), limit: 50);

// Fetch affiliates with time range
var affiliates = await client.GetAffiliatesAsync(from: startDate, toDate: endDate, limit: 50);

// Fetch payouts and products
var payouts = await client.GetPayoutsAsync(limit: 50);
var products = await client.GetProductsAsync(limit: 50);
```

Wrapper methods return typed models:

- `GoAffProOrder` (includes Subtotal, AffiliateId, Status)
- `GoAffProAffiliate` (includes FirstName, LastName, Phone, Country, GroupId)
- `GoAffProReward` (includes AffiliateId, Type, Metadata, Level, Status) - currently disabled
- `GoAffProPayout`
- `GoAffProProduct`

Each model includes strongly typed fields and `RawPayload` (`JsonElement`) for advanced scenarios.

`GetRewardsAsync` is currently disabled because `/user/feed/rewards` is returning `404` (observed on 2026-02-18). The method is marked `[Obsolete]` and returns an empty collection.

### Access Generated Clients Directly

```csharp
var loginResponse = await client.User.UserLoginAsync(new GoAffPro.Client.Generated.User.Body
{
    Email = "affiliate@example.com",
    Password = "password123",
});

var publicSites = await client.PublicApi.PublicSitesAsync(
    site_ids: null,
    currency: null,
    keyword: null,
    limit: 20,
    offset: 0);
```

## Event Detection

`GoAffProEventDetector` supports both async streams and classic `.NET` events. It uses time-based filtering to fetch only new items since the last poll.

### Async Streams

```csharp
using GoAffPro.Client.Events;

var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromSeconds(30));

// Optional: backfill historical data from a specific time
detector.OrderStartTime = DateTimeOffset.UtcNow.AddDays(-7);

await foreach (var order in detector.NewOrdersAsync(cancellationToken))
{
    Console.WriteLine($"New order: {order.Id}");
}
```

### Event Handlers

```csharp
using GoAffPro.Client.Events;

var detector = new GoAffProEventDetector(client, pollingInterval: TimeSpan.FromSeconds(30));

detector.OrderDetected += (_, args) => Console.WriteLine($"Order: {args.Order.Id}");
detector.AffiliateDetected += (_, args) => Console.WriteLine($"Affiliate: {args.Affiliate.Id}");

await detector.StartAsync(cancellationToken);
```

The detector stores the last poll timestamp internally. Use `OrderStartTime` and `AffiliateStartTime` properties to backfill historical data on first run.

## Dependency Injection

```csharp
services.AddGoAffProClient(options =>
{
    options.BaseUrl = new Uri("https://api.goaffpro.com/v1/", UriKind.Absolute);
    options.BearerToken = configuration["GoAffPro:Token"];
    options.Timeout = TimeSpan.FromSeconds(30);
});
```

## Example App

An executable sample is included at:

- `examples/GoAffPro.Client.Example`

Run it with:

```bash
dotnet run --project examples/GoAffPro.Client.Example
```

## Build-Time Generation

On build, `GoAffPro.Client.Generator`:

1. Loads local canonical spec `openapi/goaffpro-canonical.json` by default
   (or uses a configured `GoAffProSwaggerInitPath`; supports both full OpenAPI `.json` and `swagger-ui-init.js`)
2. If `swagger-ui-init.js` is used, extracts embedded OpenAPI JSON and applies legacy normalization
3. Generates:
   - `src/GoAffPro.Client/Generated/GoAffProUserClient.g.cs`
   - `src/GoAffPro.Client/Generated/GoAffProPublicClient.g.cs`

Do not edit `*.g.cs` manually.

## Testing

```bash
dotnet test
```

### Integration Tests

```bash
$env:GOAFFPRO_TEST_TOKEN="your-token"
dotnet test --filter "Category=Integration"
```

### Contract Snapshot Test

The test suite validates generated client method signatures against:

- `tests/GoAffPro.Client.Tests/Snapshots/GeneratedClientSignatures.snapshot`

If generated signatures change, update the snapshot intentionally in the same change.
