using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed class AffiliateDetectedEventArgs(GoAffProAffiliate affiliate) : EventArgs
{
    public GoAffProAffiliate Affiliate { get; } = affiliate;
}
