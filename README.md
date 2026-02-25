# GoAffPro.Client

Async-first .NET client for the GoAffPro API with build-time Kiota generation and polling/event-based change detection.

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

### Wrapper Surface

`GoAffProClient` intentionally keeps a minimal wrapper surface:

- auth helpers: `LoginAsync`, `SetBearerToken`
- generated client access: `User` and `PublicApi`
- polling detector: `GoAffProEventDetector`

### Access Generated Clients Directly

```csharp
var loginResponse = await client.User.User.Login.PostAsync(new GoAffPro.Client.Generated.User.User.Login.LoginPostRequestBody
{
    Email = "affiliate@example.com",
    Password = "password123",
});

var publicSites = await client.PublicApi.Public.Sites.GetAsync(config =>
{
    config.QueryParameters.Limit = 20;
    config.QueryParameters.Offset = 0;
});
```

## Event Detection

`GoAffProEventDetector` supports both async streams and classic `.NET` events. It uses time-based filtering to fetch only new items since the last poll.
Detected payloads are propagated as generated Kiota feed item types.

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

On build, `GoAffPro.Client.Generated`:

1. Loads the local canonical spec `openapi/goaffpro-canonical.yaml`
2. Runs Kiota generation for `/user/*` and `/public/*`
3. Writes generated files under:
   - `src/GoAffPro.Client.Generated/Generated/User`
   - `src/GoAffPro.Client.Generated/Generated/Public`

Generated output is implementation detail for the wrapper package. Do not manually edit files under `Generated/`.

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
