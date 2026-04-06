# GoAffPro.Client

Async-first .NET client for GoAffPro using Kiota-generated models plus a thin wrapper for auth and polling observers.

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

## API Usage

Use the generated API root from the wrapper:

```csharp
var sites = await client.Api.User.Sites.GetAsync(config =>
{
    config.QueryParameters.Limit = 20;
    config.QueryParameters.Offset = 0;
});
```

Wrapper helpers:

- `LoginAsync(email, password, ct)`
- `SetBearerToken(token)`

## Observer Usage

Stream-based:

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

Project: `examples/GoAffPro.Client.Example`

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

## Build-Time Generation

Generation is handled by `src/GoAffPro.Client.Generated/GoAffPro.Client.Generated.csproj`:

1. Uses local canonical spec: `openapi/goaffpro-canonical.yaml`
2. Runs Kiota during build
3. Writes generated sources under `src/GoAffPro.Client.Generated/Generated`

Do not manually edit generated files.

## Testing

Unit tests:

```bash
dotnet test tests/GoAffPro.Client.Tests/GoAffPro.Client.Tests.csproj
```

Integration tests:

```bash
dotnet test tests/GoAffPro.Client.IntegrationTests/GoAffPro.Client.IntegrationTests.csproj --filter "Category=Integration"
```

Integration auth sources:

- `GOAFFPRO_TEST_TOKEN`
- `GOAFFPRO_TEST_EMAIL` + `GOAFFPRO_TEST_PASSWORD`
- `tests/GoAffPro.Client.IntegrationTests/appsettings.Test.local.json` (template: `appsettings.Test.local.example.json`)

## Acknowledgements

- [**WolfwithSword**](https://github.com/WolfwithSword) — real-world data and testing

## Known Upstream Instability

Tracked in `openapi/goaffpro-canonical.yaml` comments:

- `/user/feed/products` can time out
- `/user/feed/rewards` can return 404/non-JSON
- `/user/feed/transactions` can return 500 with non-JSON payload
- `/user/payouts/pending` tracked as parity endpoint also seen under `/sdk/user/*`
