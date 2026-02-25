using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.OrderDetected"/>.
/// </summary>
public sealed class OrderDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="order">Detected order payload.</param>
    public OrderDetectedEventArgs(UserOrderFeedItem order)
    {
        Order = order;
    }

    /// <summary>
    /// Gets the detected order payload.
    /// </summary>
    public UserOrderFeedItem Order { get; }
}
