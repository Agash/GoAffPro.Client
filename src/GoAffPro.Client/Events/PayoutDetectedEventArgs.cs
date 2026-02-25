using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.PayoutDetected"/>.
/// </summary>
public sealed class PayoutDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PayoutDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="payout">Detected payout payload.</param>
    public PayoutDetectedEventArgs(UserPayoutFeedItem payout)
    {
        Payout = payout;
    }

    /// <summary>
    /// Gets the detected payout payload.
    /// </summary>
    public UserPayoutFeedItem Payout { get; }
}
