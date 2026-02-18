using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Represents a detected affiliate/traffic feed event.
/// </summary>
public sealed record AffiliateEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AffiliateEvent"/> record.
    /// </summary>
    /// <param name="affiliate">The detected affiliate payload.</param>
    public AffiliateEvent(GoAffProAffiliate affiliate)
    {
        Affiliate = affiliate;
    }

    /// <summary>
    /// Gets the strongly typed affiliate payload.
    /// </summary>
    public GoAffProAffiliate Affiliate { get; init; }

    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public string Id => Affiliate.Id;
}
