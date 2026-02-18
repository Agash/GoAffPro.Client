using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProEventDetector.OrderDetected"/>.
/// </summary>
public sealed class OrderDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="order">Detected order payload.</param>
    public OrderDetectedEventArgs(GoAffProOrder order)
    {
        Order = order;
    }

    /// <summary>
    /// Gets the detected order payload.
    /// </summary>
    public GoAffProOrder Order { get; }
}
