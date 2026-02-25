# GoAffPro.Client

<<<<<<< Updated upstream
Async-first .NET client for the GoAffPro API with build-time NSwag generation and polling/event-based change detection.
=======
Async-first .NET client for GoAffPro using Kiota-generated API models and a thin wrapper with polling observers.
>>>>>>> Stashed changes

## Targets

- `net9.0`
- `net10.0`

## Install

```bash
dotnet add package GoAffPro.Client
```

## Quick Start

```csharp
using GoAffPro.Client;

await using var client = new GoAffProClient(new GoAffProClientOptions
{
    BearerToken = "your-access-token",
});

// Or login first:
string token = await client.LoginAsync("affiliate@example.com", "password123");
```

<<<<<<< Updated upstream
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
=======
## Client Surface

`GoAffProClient` keeps a minimal surface:

- Auth helpers:
  - `LoginAsync(email, password, ct)`
  - `SetBearerToken(token)`
- Generated API root:
  - `client.Api.User...`
  - `client.Api.Public...`
- Observer streams/events (inside `GoAffProClient`):
  - `NewOrdersAsync`, `NewAffiliatesAsync`, `NewPayoutsAsync`, `NewProductsAsync`, `NewTransactionsAsync`
  - `StartEventObserverAsync(...)`
  - events: `OrderDetected`, `AffiliateDetected`, `PayoutDetected`, `ProductDetected`, `TransactionDetected`
>>>>>>> Stashed changes

Example generated call:

```csharp
<<<<<<< Updated upstream
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
=======
var response = await client.Api.User.Sites.GetAsync(config =>
{
    config.QueryParameters.Limit = 20;
    config.QueryParameters.Offset = 0;
});
>>>>>>> Stashed changes
```

## Observer Usage

<<<<<<< Updated upstream
`GoAffProEventDetector` supports both async streams and classic `.NET` events. It uses time-based filtering to fetch only new items since the last poll.

### Async Streams
=======
Stream-based:
>>>>>>> Stashed changes

```csharp
await foreach (var order in client.NewOrdersAsync(
    pollingInterval: TimeSpan.FromSeconds(30),
    pageSize: 100,
    cancellationToken: cancellationToken))
{
    Console.WriteLine(order.Id?.String);
}
```

Event-based:

```csharp
client.OrderDetected += (_, e) => Console.WriteLine(e.Order.Id?.String);
client.AffiliateDetected += (_, e) => Console.WriteLine(e.Affiliate.AffiliateId?.String);

await client.StartEventObserverAsync(
    pollingInterval: TimeSpan.FromSeconds(30),
    pageSize: 100,
    cancellationToken: cancellationToken);
```

Backfill controls:

- `OrderObserverStartTime`
- `AffiliateObserverStartTime`
- `PayoutObserverStartTime`

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

Project:

- `examples/GoAffPro.Client.Example`

Interactive mode:

```bash
dotnet run --project examples/GoAffPro.Client.Example
```

CLI sweep mode:

```bash
dotnet run --project examples/GoAffPro.Client.Example -- \
  --run-tests \
  --access_token=env:GOAFFPRO_TEST_TOKEN \
  --products-timeout-seconds=90 \
  --output=api-sweep.json
```

The sweep calls every supported endpoint and writes a JSON report with per-endpoint success/failure details.

## Build-Time Generation

<<<<<<< Updated upstream
On build, `GoAffPro.Client.Generator`:

1. Fetches `https://api.goaffpro.com/docs/admin/swagger-ui-init.js`
   (or uses `openapi/swagger-ui-init.js` only if you provide a local override file)
2. Extracts OpenAPI JSON
3. Filters to `/user/*` and `/public/*`
4. Normalizes schema gaps for generation
5. Generates:
   - `src/GoAffPro.Client/Generated/GoAffProUserClient.g.cs`
   - `src/GoAffPro.Client/Generated/GoAffProPublicClient.g.cs`

Do not edit `*.g.cs` manually.
=======
Generation is handled by `src/GoAffPro.Client.Generated/GoAffPro.Client.Generated.csproj`:

1. Uses local canonical spec: `openapi/goaffpro-canonical.yaml`
2. Runs Kiota at build time
3. Writes generated sources under `src/GoAffPro.Client.Generated/Generated`

Do not edit generated files manually.
>>>>>>> Stashed changes

## Testing

Unit tests:

```bash
dotnet test tests/GoAffPro.Client.Tests/GoAffPro.Client.Tests.csproj
```

Integration tests:

```bash
dotnet test tests/GoAffPro.Client.IntegrationTests/GoAffPro.Client.IntegrationTests.csproj --filter "Category=Integration"
```

Integration auth options:

- `GOAFFPRO_TEST_TOKEN`
- or `GOAFFPRO_TEST_EMAIL` + `GOAFFPRO_TEST_PASSWORD`
- or local file `tests/GoAffPro.Client.IntegrationTests/appsettings.Test.local.json`
  - template: `appsettings.Test.local.example.json`

## Known Upstream Endpoint Instability

Tracked in `openapi/goaffpro-canonical.yaml` comments:

- `/user/feed/products` can time out
- `/user/feed/rewards` returns 404/non-JSON in current runtime
- `/user/feed/transactions` can return 500 with non-JSON payload
- `/user/payouts/pending` tracked as parity endpoint also exposed under `/sdk/user/*`
