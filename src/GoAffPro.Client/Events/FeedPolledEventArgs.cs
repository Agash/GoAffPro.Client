namespace GoAffPro.Client.Events;

/// <summary>Which feed a poll read.</summary>
public enum GoAffProFeed
{
    /// <summary>New orders.</summary>
    Orders,

    /// <summary>Referral traffic.</summary>
    Traffic,

    /// <summary>Payouts.</summary>
    Payouts,

    /// <summary>Products.</summary>
    Products,

    /// <summary>Transactions.</summary>
    Transactions,
}

/// <summary>
/// Event arguments for <see cref="GoAffProClient.FeedPolled"/>: one poll of a feed, which worked when
/// <see cref="Error"/> is null. A failed poll is retried with the same window on the next interval.
/// </summary>
/// <param name="feed">The feed polled.</param>
/// <param name="polledAt">When the poll finished.</param>
/// <param name="error">Why the poll failed, or null when it worked.</param>
public sealed class FeedPolledEventArgs(
    GoAffProFeed feed,
    DateTimeOffset polledAt,
    Exception? error
) : EventArgs
{
    /// <summary>Gets the feed polled.</summary>
    public GoAffProFeed Feed { get; } = feed;

    /// <summary>Gets when the poll finished.</summary>
    public DateTimeOffset PolledAt { get; } = polledAt;

    /// <summary>Gets why the poll failed, or null when it worked.</summary>
    public Exception? Error { get; } = error;

    /// <summary>Gets whether the poll worked.</summary>
    public bool Succeeded => Error is null;
}
