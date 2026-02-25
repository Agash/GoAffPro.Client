using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using GoAffPro.Client;
using GoAffPro.Client.Exceptions;
using Microsoft.Kiota.Abstractions;
using Spectre.Console;
using AggregateField = GoAffPro.Client.Generated.User.Stats.Aggregate.GetFieldsQueryParameterType;
using SiteField = GoAffPro.Client.Generated.User.Sites.GetFieldsQueryParameterType;
using SiteStatus = GoAffPro.Client.Generated.User.Sites.GetStatusQueryParameterType;

ExampleSettings settings = LoadSettings();
var options = CommandLineOptions.Parse(args);

using var client = new GoAffProClient(new GoAffProClientOptions
{
    BaseUrl = settings.BaseUrl,
    BearerToken = settings.BearerToken,
    Timeout = settings.Timeout,
});

if (options.RunTests)
{
    int code = await RunCliSweepAsync(client, settings, options).ConfigureAwait(false);
    Environment.ExitCode = code;
    return;
}

await RunInteractiveAsync(client).ConfigureAwait(false);

static async Task<int> RunCliSweepAsync(GoAffProClient client, ExampleSettings settings, CommandLineOptions options)
{
    string? token = ResolveToken(options.AccessToken) ?? client.BearerToken;
    if (string.IsNullOrWhiteSpace(token) &&
        !string.IsNullOrWhiteSpace(options.Email) &&
        !string.IsNullOrWhiteSpace(options.Password))
    {
        token = await client.LoginAsync(options.Email, options.Password, CancellationToken.None).ConfigureAwait(false);
    }

    if (string.IsNullOrWhiteSpace(token))
    {
        await Console.Error.WriteLineAsync("No access token is available. Use --access_token=... or --email/--password.").ConfigureAwait(false);
        return 2;
    }

    client.SetBearerToken(token);
    string outputPath = string.IsNullOrWhiteSpace(options.OutputPath)
        ? Path.Combine(Environment.CurrentDirectory, $"api-sweep-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json")
        : options.OutputPath;

    TimeSpan productTimeout = options.ProductsTimeoutSeconds > 0
        ? TimeSpan.FromSeconds(options.ProductsTimeoutSeconds)
        : TimeSpan.FromSeconds(90);

    ApiSweepReport report = await ApiSweepRunner.RunAllAsync(client, settings, productTimeout, CancellationToken.None).ConfigureAwait(false);
    string reportJson = JsonSerializer.Serialize(report, JsonOptions.Value);
    await File.WriteAllTextAsync(outputPath, reportJson, CancellationToken.None).ConfigureAwait(false);

    int failures = report.Results.Count(static x => !x.Success);
    await Console.Out.WriteLineAsync($"Sweep completed. Total: {report.Results.Count}, failed: {failures}").ConfigureAwait(false);
    await Console.Out.WriteLineAsync($"Report: {outputPath}").ConfigureAwait(false);
    return failures == 0 ? 0 : 1;
}

static async Task RunInteractiveAsync(GoAffProClient client)
{
    var observer = new ObserverController(client);
    try
    {
        AnsiConsole.MarkupLine("[bold green]GoAffPro Interactive Playground[/]");
        if (!string.IsNullOrWhiteSpace(client.BearerToken))
        {
            AnsiConsole.MarkupLine("[green]Bearer token loaded from appsettings.[/]");
        }

        bool exitRequested = false;
        while (!exitRequested)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Auth: {(string.IsNullOrWhiteSpace(client.BearerToken) ? "[yellow]not authenticated[/]" : "[green]authenticated[/]")}");
            AnsiConsole.MarkupLine($"Observer: {(observer.IsRunning ? "[green]running[/]" : "[grey]stopped[/]")}");

            string action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action")
                    .AddChoices(
                    [
                        "Set bearer token",
                        "Login with email/password",
                        "Call endpoint",
                        "Run full endpoint sweep",
                        "Start observer",
                        "Stop observer",
                        "Exit"
                    ]));

            try
            {
                switch (action)
                {
                    case "Set bearer token":
                        SetBearerToken(client);
                        break;
                    case "Login with email/password":
                        await LoginAsync(client).ConfigureAwait(false);
                        break;
                    case "Call endpoint":
                        await CallEndpointAsync(client).ConfigureAwait(false);
                        break;
                    case "Run full endpoint sweep":
                        await RunSweepFromInteractiveAsync(client).ConfigureAwait(false);
                        break;
                    case "Start observer":
                        await observer.StartAsync().ConfigureAwait(false);
                        break;
                    case "Stop observer":
                        await observer.StopAsync().ConfigureAwait(false);
                        break;
                    case "Exit":
                        exitRequested = true;
                        break;
                }
            }
            catch (GoAffProApiException ex)
            {
                AnsiConsole.MarkupLine($"[red]API error:[/] {Markup.Escape(ex.Message)}");
            }
            catch (ApiException ex)
            {
                AnsiConsole.MarkupLine($"[red]Request failed:[/] {Markup.Escape(ex.Message)}");
            }
            catch (HttpRequestException ex)
            {
                AnsiConsole.MarkupLine($"[red]HTTP error:[/] {Markup.Escape(ex.Message)}");
            }
            catch (TaskCanceledException ex)
            {
                AnsiConsole.MarkupLine($"[red]Timeout/cancelled:[/] {Markup.Escape(ex.Message)}");
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Invalid operation:[/] {Markup.Escape(ex.Message)}");
            }
        }
    }
    finally
    {
        await observer.DisposeAsync().ConfigureAwait(false);
    }
}

static async Task RunSweepFromInteractiveAsync(GoAffProClient client)
{
    if (string.IsNullOrWhiteSpace(client.BearerToken))
    {
        AnsiConsole.MarkupLine("[yellow]Set a bearer token or login first.[/]");
        return;
    }

    string defaultPath = Path.Combine(Environment.CurrentDirectory, $"api-sweep-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json");
    string outputPath = AnsiConsole.Prompt(new TextPrompt<string>("Output path").DefaultValue(defaultPath));

    int timeoutSeconds = AnsiConsole.Prompt(new TextPrompt<int>("Product endpoint timeout (seconds)").DefaultValue(90));
    var productTimeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));

    ApiSweepReport report = await ApiSweepRunner.RunAllAsync(
        client,
        ExampleSettings.Default,
        productTimeout,
        CancellationToken.None).ConfigureAwait(false);
    string reportJson = JsonSerializer.Serialize(report, JsonOptions.Value);
    await File.WriteAllTextAsync(outputPath, reportJson, CancellationToken.None).ConfigureAwait(false);

    int failures = report.Results.Count(static x => !x.Success);
    AnsiConsole.MarkupLine($"[green]Sweep complete.[/] total={report.Results.Count}, failed={failures}");
    AnsiConsole.MarkupLine($"Saved: [grey]{Markup.Escape(outputPath)}[/]");
}

static void SetBearerToken(GoAffProClient client)
{
    string token = AnsiConsole.Prompt(new TextPrompt<string>("Bearer token").Secret());
    client.SetBearerToken(token);
    AnsiConsole.MarkupLine("[green]Bearer token updated.[/]");
}

static async Task LoginAsync(GoAffProClient client)
{
    string email = AnsiConsole.Ask<string>("Email");
    string password = AnsiConsole.Prompt(new TextPrompt<string>("Password").Secret());

    string token = await client.LoginAsync(email, password).ConfigureAwait(false);
    AnsiConsole.MarkupLine($"[green]Login successful.[/] Token: [grey]{Markup.Escape(ShortToken(token))}[/]");
}

static async Task CallEndpointAsync(GoAffProClient client)
{
    string endpoint = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select endpoint")
            .AddChoices(
            [
                "GET /user",
                "POST /user",
                "GET /user/sites",
                "GET /user/stats/aggregate",
                "GET /user/feed/orders",
                "GET /user/feed/traffic",
                "GET /user/feed/payouts",
                "GET /user/feed/products",
                "GET /user/feed/rewards",
                "GET /user/feed/transactions",
                "GET /user/commissions",
                "GET /user/payouts/pending",
                "GET /public/sites",
                "GET /public/products",
                "Back"
            ]));

    if (endpoint == "Back")
    {
        return;
    }

    int limit = AskInt("Limit", 10);
    int offset = AskInt("Offset", 0);
    string? siteIds = AskOptional("site_ids (optional)");
    string startTime = DateTimeOffset.UtcNow.AddDays(-1).ToString("o", CultureInfo.InvariantCulture);
    string endTime = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);

    object? result = endpoint switch
    {
        "GET /user" => await client.Api.User.GetAsync().ConfigureAwait(false),
        "POST /user" => await client.Api.User.PostAsync().ConfigureAwait(false),
        "GET /user/sites" => await client.Api.User.Sites.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.StatusAsGetStatusQueryParameterType = AskSiteStatus();
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType = [SiteField.Id, SiteField.Name, SiteField.Logo];
        }).ConfigureAwait(false),
        "GET /user/stats/aggregate" => await client.Api.User.Stats.Aggregate.GetAsync(config =>
        {
            config.QueryParameters.SiteIds = siteIds;
            config.QueryParameters.StartTime = AskOptional("start_time (ISO8601, optional)") ?? startTime;
            config.QueryParameters.EndTime = AskOptional("end_time (ISO8601, optional)") ?? endTime;
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType =
            [
                AggregateField.Total_sales,
                AggregateField.Other_commission_earned,
                AggregateField.Revenue_generated,
                AggregateField.Sale_commission_earned,
                AggregateField.Commission_paid,
            ];
        }).ConfigureAwait(false),
        "GET /user/feed/orders" => await client.Api.User.Feed.Orders.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.SiteIds = siteIds;
            config.QueryParameters.CreatedAtMin = AskOptional("created_at_min (ISO8601, optional)") ?? startTime;
            config.QueryParameters.CreatedAtMax = AskOptional("created_at_max (ISO8601, optional)") ?? endTime;
        }).ConfigureAwait(false),
        "GET /user/feed/traffic" => await client.Api.User.Feed.Traffic.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.SiteIds = siteIds;
            config.QueryParameters.StartTime = AskOptional("start_time (ISO8601, optional)") ?? startTime;
            config.QueryParameters.EndTime = AskOptional("end_time (ISO8601, optional)") ?? endTime;
        }).ConfigureAwait(false),
        "GET /user/feed/payouts" => await client.Api.User.Feed.Payouts.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.SiteIds = siteIds;
            config.QueryParameters.StartTime = AskOptional("start_time (ISO8601, optional)") ?? startTime;
            config.QueryParameters.EndTime = AskOptional("end_time (ISO8601, optional)") ?? endTime;
        }).ConfigureAwait(false),
        "GET /user/feed/products" => await client.Api.User.Feed.Products.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
        }).ConfigureAwait(false),
        "GET /user/feed/rewards" => await client.Api.User.Feed.Rewards.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.SiteIds = siteIds;
            config.QueryParameters.StartTime = AskOptional("start_time (ISO8601, optional)") ?? startTime;
            config.QueryParameters.EndTime = AskOptional("end_time (ISO8601, optional)") ?? endTime;
        }).ConfigureAwait(false),
        "GET /user/feed/transactions" => await client.Api.User.Feed.Transactions.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
        }).ConfigureAwait(false),
        "GET /user/commissions" => await client.Api.User.Commissions.GetAsync(config =>
        {
            config.QueryParameters.SiteIds = siteIds;
        }).ConfigureAwait(false),
        "GET /user/payouts/pending" => await client.Api.User.Payouts.Pending.GetAsync().ConfigureAwait(false),
        "GET /public/sites" => await client.Api.Public.Sites.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.SiteIds = siteIds;
            config.QueryParameters.Currency = AskOptional("currency (optional)");
            config.QueryParameters.Keyword = AskOptional("keyword (optional)");
        }).ConfigureAwait(false),
        "GET /public/products" => await client.Api.Public.Products.GetAsync(config =>
        {
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Offset = offset;
            config.QueryParameters.SiteIds = siteIds;
        }).ConfigureAwait(false),
        _ => null,
    };

    RenderJson(endpoint, result);
}

static SiteStatus? AskSiteStatus()
{
    string status = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("status")
            .AddChoices(["(none)", "approved", "pending", "blocked"]));

    return status switch
    {
        "approved" => SiteStatus.Approved,
        "pending" => SiteStatus.Pending,
        "blocked" => SiteStatus.Blocked,
        _ => null,
    };
}

static int AskInt(string label, int defaultValue)
{
    return AnsiConsole.Prompt(new TextPrompt<int>(label).DefaultValue(defaultValue));
}

static string? AskOptional(string label)
{
    string value = AnsiConsole.Prompt(new TextPrompt<string>(label).AllowEmpty());
    return string.IsNullOrWhiteSpace(value) ? null : value;
}

static void RenderJson(string title, object? value)
{
    string json = Serialize(value);

    var panel = new Panel(new Markup(Markup.Escape(json)))
    {
        Header = new PanelHeader(title),
        Expand = true,
    };

    AnsiConsole.Write(panel);
}

static string Serialize(object? value)
{
    if (value is null)
    {
        return "null";
    }

    try
    {
        return JsonSerializer.Serialize(value, JsonOptions.Value);
    }
    catch (JsonException)
    {
        return value.ToString() ?? "<unable to render response>";
    }
    catch (NotSupportedException)
    {
        return value.ToString() ?? "<unable to render response>";
    }
}

static string ShortToken(string token)
{
    return token.Length <= 10 ? token : $"{token[..6]}...{token[^4..]}";
}

static string? ResolveToken(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    const string envPrefix = "env:";
    if (value.StartsWith(envPrefix, StringComparison.OrdinalIgnoreCase))
    {
        string envName = value[envPrefix.Length..];
        return Environment.GetEnvironmentVariable(envName);
    }

    return value;
}

static ExampleSettings LoadSettings()
{
    string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    if (!File.Exists(path))
    {
        return ExampleSettings.Default;
    }

    try
    {
        string json = File.ReadAllText(path);
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("GoAffPro", out JsonElement goAffPro))
        {
            return ExampleSettings.Default;
        }

        string baseUrlText = goAffPro.TryGetProperty("BaseUrl", out JsonElement baseUrlElement)
            ? baseUrlElement.GetString() ?? ExampleSettings.Default.BaseUrl.ToString()
            : ExampleSettings.Default.BaseUrl.ToString();

        string? bearerToken = goAffPro.TryGetProperty("BearerToken", out JsonElement tokenElement)
            ? tokenElement.GetString()
            : null;

        string timeoutText = goAffPro.TryGetProperty("Timeout", out JsonElement timeoutElement)
            ? timeoutElement.GetString() ?? "00:00:30"
            : "00:00:30";

        Uri baseUrl = Uri.TryCreate(baseUrlText, UriKind.Absolute, out Uri? parsedBaseUrl)
            ? parsedBaseUrl
            : ExampleSettings.Default.BaseUrl;

        TimeSpan timeout = TimeSpan.TryParse(timeoutText, out TimeSpan parsedTimeout)
            ? parsedTimeout
            : ExampleSettings.Default.Timeout;

        return new ExampleSettings(baseUrl, bearerToken, timeout);
    }
    catch (IOException)
    {
        return ExampleSettings.Default;
    }
    catch (JsonException)
    {
        return ExampleSettings.Default;
    }
}

internal sealed class ObserverController(IGoAffProClient client) : IAsyncDisposable
{
    private readonly Lock _consoleLock = new();
    private CancellationTokenSource? _cts;
    private Task? _task;

    public bool IsRunning => _task is { IsCompleted: false };

    public Task StartAsync()
    {
        if (IsRunning)
        {
            AnsiConsole.MarkupLine("[yellow]Observer already running.[/]");
            return Task.CompletedTask;
        }

        int pollingSeconds = AnsiConsole.Prompt(new TextPrompt<int>("Polling interval (seconds)").DefaultValue(30));
        int pageSize = AnsiConsole.Prompt(new TextPrompt<int>("Page size").DefaultValue(50));

        client.OrderDetected += (_, args) =>
            WriteLiveEvent("order", args.Order.Id?.String ?? args.Order.OrderId?.String ?? "<unknown>");
        client.AffiliateDetected += (_, args) =>
            WriteLiveEvent("affiliate", args.Affiliate.AffiliateId?.String ?? args.Affiliate.Id?.String ?? args.Affiliate.CustomerId?.String ?? "<unknown>");
        client.PayoutDetected += (_, args) =>
            WriteLiveEvent("payout", args.Payout.Id?.String ?? args.Payout.PayoutId?.String ?? "<unknown>");
        client.ProductDetected += (_, args) =>
            WriteLiveEvent("product", args.Product.ProductId?.String ?? args.Product.Id?.String ?? "<unknown>");
        client.TransactionDetected += (_, args) =>
            WriteLiveEvent("transaction", args.Transaction.TxId?.ToString(CultureInfo.InvariantCulture) ?? args.Transaction.Id ?? "<unknown>");

        _cts = new CancellationTokenSource();
        _task = Task.Run(async () =>
        {
            try
            {
                await client.StartEventObserverAsync(
                    pollingInterval: TimeSpan.FromSeconds(Math.Max(1, pollingSeconds)),
                    pageSize: Math.Max(1, pageSize),
                    cancellationToken: _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when stop is requested.
            }
            catch (GoAffProApiException ex)
            {
                WriteObserverError(ex.Message);
            }
            catch (ApiException ex)
            {
                WriteObserverError(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                WriteObserverError(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                WriteObserverError(ex.Message);
            }
        });

        AnsiConsole.MarkupLine("[green]Observer started.[/]");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is null)
        {
            return;
        }

        await _cts.CancelAsync().ConfigureAwait(false);

        if (_task is not null)
        {
            await _task.ConfigureAwait(false);
        }

        _cts.Dispose();
        _cts = null;
        _task = null;

        AnsiConsole.MarkupLine("[grey]Observer stopped.[/]");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private void WriteLiveEvent(string type, string id)
    {
        lock (_consoleLock)
        {
            string timestamp = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            AnsiConsole.MarkupLine($"[blue]{Markup.Escape(timestamp)}[/] [green]{Markup.Escape(type)}[/] -> {Markup.Escape(id)}");
        }
    }

    private void WriteObserverError(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[red]Observer failed:[/] {Markup.Escape(message)}");
        }
    }
}

internal static class ApiSweepRunner
{
    public static async Task<ApiSweepReport> RunAllAsync(
        GoAffProClient client,
        ExampleSettings settings,
        TimeSpan productTimeout,
        CancellationToken cancellationToken)
    {
        string startTime = DateTimeOffset.UtcNow.AddDays(-1).ToString("o", CultureInfo.InvariantCulture);
        string endTime = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        var results = new List<ApiEndpointResult>();

        await RunEndpointAsync(results, "GET /user", () => client.Api.User.GetAsync(cancellationToken: cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "POST /user", () => client.Api.User.PostAsync(cancellationToken: cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/sites", () => client.Api.User.Sites.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
            config.QueryParameters.StatusAsGetStatusQueryParameterType = SiteStatus.Approved;
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType = [SiteField.Id, SiteField.Name, SiteField.Logo];
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/stats/aggregate", () => client.Api.User.Stats.Aggregate.GetAsync(config =>
        {
            config.QueryParameters.StartTime = startTime;
            config.QueryParameters.EndTime = endTime;
            config.QueryParameters.FieldsAsGetFieldsQueryParameterType =
            [
                AggregateField.Total_sales,
                AggregateField.Other_commission_earned,
                AggregateField.Revenue_generated,
                AggregateField.Sale_commission_earned,
                AggregateField.Commission_paid,
            ];
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/feed/orders", () => client.Api.User.Feed.Orders.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
            config.QueryParameters.CreatedAtMin = startTime;
            config.QueryParameters.CreatedAtMax = endTime;
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/feed/traffic", () => client.Api.User.Feed.Traffic.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
            config.QueryParameters.StartTime = startTime;
            config.QueryParameters.EndTime = endTime;
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/feed/payouts", () => client.Api.User.Feed.Payouts.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
            config.QueryParameters.StartTime = startTime;
            config.QueryParameters.EndTime = endTime;
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(
            results,
            "GET /user/feed/products",
            () => ExecuteProductsCallAsync(client, settings, productTimeout, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/feed/rewards", () => client.Api.User.Feed.Rewards.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
            config.QueryParameters.StartTime = startTime;
            config.QueryParameters.EndTime = endTime;
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/feed/transactions", () => client.Api.User.Feed.Transactions.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/commissions", () => client.Api.User.Commissions.GetAsync(cancellationToken: cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /user/payouts/pending", () => client.Api.User.Payouts.Pending.GetAsync(cancellationToken: cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /public/sites", () => client.Api.Public.Sites.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, cancellationToken)).ConfigureAwait(false);
        await RunEndpointAsync(results, "GET /public/products", () => client.Api.Public.Products.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, cancellationToken)).ConfigureAwait(false);

        return new ApiSweepReport(
            TimestampUtc: DateTimeOffset.UtcNow,
            BaseUrl: settings.BaseUrl.ToString(),
            Total: results.Count,
            Failed: results.Count(static r => !r.Success),
            Results: results);
    }

    private static async Task RunEndpointAsync<TResponse>(
        List<ApiEndpointResult> sink,
        string endpoint,
        Func<Task<TResponse?>> call)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            TResponse? result = await call().ConfigureAwait(false);
            sw.Stop();
            sink.Add(new ApiEndpointResult(
                Endpoint: endpoint,
                Success: true,
                DurationMs: sw.ElapsedMilliseconds,
                StatusCode: null,
                ErrorType: null,
                ErrorMessage: null,
                ResponseJson: SerializeResponse(result)));
        }
        catch (ApiException ex)
        {
            sw.Stop();
            sink.Add(new ApiEndpointResult(
                Endpoint: endpoint,
                Success: false,
                DurationMs: sw.ElapsedMilliseconds,
                StatusCode: ex.ResponseStatusCode,
                ErrorType: ex.GetType().FullName,
                ErrorMessage: ex.Message,
                ResponseJson: null));
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            sink.Add(new ApiEndpointResult(
                Endpoint: endpoint,
                Success: false,
                DurationMs: sw.ElapsedMilliseconds,
                StatusCode: null,
                ErrorType: ex.GetType().FullName,
                ErrorMessage: ex.Message,
                ResponseJson: null));
        }
        catch (TaskCanceledException ex)
        {
            sw.Stop();
            sink.Add(new ApiEndpointResult(
                Endpoint: endpoint,
                Success: false,
                DurationMs: sw.ElapsedMilliseconds,
                StatusCode: null,
                ErrorType: ex.GetType().FullName,
                ErrorMessage: ex.Message,
                ResponseJson: null));
        }
        catch (InvalidOperationException ex)
        {
            sw.Stop();
            sink.Add(new ApiEndpointResult(
                Endpoint: endpoint,
                Success: false,
                DurationMs: sw.ElapsedMilliseconds,
                StatusCode: null,
                ErrorType: ex.GetType().FullName,
                ErrorMessage: ex.Message,
                ResponseJson: null));
        }
        catch (GoAffProApiException ex)
        {
            sw.Stop();
            sink.Add(new ApiEndpointResult(
                Endpoint: endpoint,
                Success: false,
                DurationMs: sw.ElapsedMilliseconds,
                StatusCode: (int)ex.StatusCode,
                ErrorType: ex.GetType().FullName,
                ErrorMessage: ex.Message,
                ResponseJson: null));
        }
    }

    private static string SerializeResponse<TResponse>(TResponse? value)
    {
        if (value is null)
        {
            return "null";
        }

        try
        {
            return JsonSerializer.Serialize(value, JsonOptions.Value);
        }
        catch (JsonException)
        {
            return value.ToString() ?? "<unable to render response>";
        }
        catch (NotSupportedException)
        {
            return value.ToString() ?? "<unable to render response>";
        }
    }

    private static async Task<GoAffPro.Client.Generated.Models.UserProductFeedResponse?> ExecuteProductsCallAsync(
        GoAffProClient client,
        ExampleSettings settings,
        TimeSpan productTimeout,
        CancellationToken cancellationToken)
    {
        if (productTimeout <= TimeSpan.Zero || productTimeout == settings.Timeout)
        {
            return await client.Api.User.Feed.Products.GetAsync(config =>
            {
                config.QueryParameters.Limit = 1;
                config.QueryParameters.Offset = 0;
            }, cancellationToken).ConfigureAwait(false);
        }

        using var timeoutClient = new GoAffProClient(new GoAffProClientOptions
        {
            BaseUrl = settings.BaseUrl,
            BearerToken = client.BearerToken,
            Timeout = productTimeout
        });

        return await timeoutClient.Api.User.Feed.Products.GetAsync(config =>
        {
            config.QueryParameters.Limit = 1;
            config.QueryParameters.Offset = 0;
        }, cancellationToken).ConfigureAwait(false);
    }
}

internal sealed record CommandLineOptions
{
    public bool RunTests { get; init; }
    public string? AccessToken { get; init; }
    public string? OutputPath { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public int ProductsTimeoutSeconds { get; init; }

    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();
        foreach (string arg in args)
        {
            if (arg.Equals("--run-tests", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { RunTests = true };
                continue;
            }

            if (arg.StartsWith("--access_token=", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { AccessToken = arg["--access_token=".Length..] };
                continue;
            }

            if (arg.StartsWith("--output=", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { OutputPath = arg["--output=".Length..] };
                continue;
            }

            if (arg.StartsWith("--email=", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { Email = arg["--email=".Length..] };
                continue;
            }

            if (arg.StartsWith("--password=", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { Password = arg["--password=".Length..] };
                continue;
            }

            if (arg.StartsWith("--products-timeout-seconds=", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(arg["--products-timeout-seconds=".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int timeoutSeconds))
            {
                options = options with { ProductsTimeoutSeconds = timeoutSeconds };
            }
        }

        return options;
    }
}

internal sealed record ExampleSettings(Uri BaseUrl, string? BearerToken, TimeSpan Timeout)
{
    public static ExampleSettings Default { get; } =
        new(new Uri("https://api.goaffpro.com/v1/", UriKind.Absolute), null, TimeSpan.FromSeconds(30));
}

internal sealed record ApiSweepReport(
    DateTimeOffset TimestampUtc,
    string BaseUrl,
    int Total,
    int Failed,
    IReadOnlyList<ApiEndpointResult> Results);

internal sealed record ApiEndpointResult(
    string Endpoint,
    bool Success,
    long DurationMs,
    int? StatusCode,
    string? ErrorType,
    string? ErrorMessage,
    string? ResponseJson);

file static class JsonOptions
{
    public static JsonSerializerOptions Value { get; } = new()
    {
        WriteIndented = true,
    };
}
