using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.OrderDetected"/>.
/// </summary>
/// <param name="order">Detected order payload.</param>
public sealed class OrderDetectedEventArgs(UserOrderFeedItem order) : EventArgs
{
    /// <summary>
    /// Gets the detected order payload.
    /// </summary>
    public UserOrderFeedItem Order { get; } = order;
}
