using GoAffPro.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoAffPro.Client.Example;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated by host via AddHostedService<Worker>().")]
internal sealed partial class Worker(
    ILogger<Worker> logger,
    IGoAffProClient client,
    IOptions<ExampleOptions> exampleOptions,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    private readonly ExampleOptions _options = exampleOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(client.BearerToken))
            {
                if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
                {
                    LogMissingCredentials(logger);
                    lifetime.StopApplication();
                    return;
                }

                await client.LoginAsync(_options.Email, _options.Password, stoppingToken).ConfigureAwait(false);
                LogAuthenticated(logger);
            }

            IReadOnlyList<global::GoAffPro.Client.Models.GoAffProOrder> orders = await client
                .GetOrdersAsync(limit: _options.PageSize, offset: 0, cancellationToken: stoppingToken)
                .ConfigureAwait(false);

            LogFetchedOrders(logger, orders.Count);

            var detector = new GoAffProEventDetector(
                client,
                pollingInterval: TimeSpan.FromSeconds(Math.Max(1, _options.PollingIntervalSeconds)),
                pageSize: Math.Max(1, _options.PageSize));

            detector.OrderDetected += (_, args) =>
                LogOrderDetected(logger, args.Order.Id);
            detector.AffiliateDetected += (_, args) =>
                LogAffiliateDetected(logger, args.Affiliate.Id);
            detector.RewardDetected += (_, args) =>
                LogRewardDetected(logger, args.Reward.Id);

            LogStartingDetector(logger);
            await detector.StartAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            LogStopping(logger);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "No bearer token is configured. Set GoAffPro:BearerToken or Example:Email/Example:Password in appsettings.json.")]
    private static partial void LogMissingCredentials(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Authenticated with login flow.")]
    private static partial void LogAuthenticated(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Fetched {Count} order(s) from GoAffPro feed.")]
    private static partial void LogFetchedOrders(ILogger logger, int count);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Order detected: {Id}")]
    private static partial void LogOrderDetected(ILogger logger, string id);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Affiliate detected: {Id}")]
    private static partial void LogAffiliateDetected(ILogger logger, string id);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Reward detected: {Id}")]
    private static partial void LogRewardDetected(ILogger logger, string id);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Starting detector loop. Press Ctrl+C to stop.")]
    private static partial void LogStartingDetector(ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Example is stopping.")]
    private static partial void LogStopping(ILogger logger);
}
