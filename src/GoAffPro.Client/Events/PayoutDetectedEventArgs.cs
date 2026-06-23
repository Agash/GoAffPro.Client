using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.PayoutDetected"/>.
/// </summary>
/// <param name="payout">Detected payout payload.</param>
public sealed class PayoutDetectedEventArgs(UserPayoutFeedItem payout) : EventArgs
{
    /// <summary>
    /// Gets the detected payout payload.
    /// </summary>
    public UserPayoutFeedItem Payout { get; } = payout;
}
