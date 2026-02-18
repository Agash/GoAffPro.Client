using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProEventDetector.AffiliateDetected"/>.
/// </summary>
public sealed class AffiliateDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AffiliateDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="affiliate">Detected affiliate payload.</param>
    public AffiliateDetectedEventArgs(GoAffProAffiliate affiliate)
    {
        Affiliate = affiliate;
    }

    /// <summary>
    /// Gets the detected affiliate payload.
    /// </summary>
    public GoAffProAffiliate Affiliate { get; }
}
