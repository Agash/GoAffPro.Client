using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed record AffiliateEvent(GoAffProAffiliate Affiliate)
{
    public string Id => Affiliate.Id;
}
