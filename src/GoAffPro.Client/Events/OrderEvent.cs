using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Represents a detected order feed event.
/// </summary>
public sealed record OrderEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderEvent"/> record.
    /// </summary>
    /// <param name="order">The detected order payload.</param>
    public OrderEvent(GoAffProOrder order)
    {
        Order = order;
    }

    /// <summary>
    /// Gets the strongly typed order payload.
    /// </summary>
    public GoAffProOrder Order { get; init; }

    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public string Id => Order.Id;
}
