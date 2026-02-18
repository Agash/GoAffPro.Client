using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed record OrderEvent(GoAffProOrder Order)
{
    public string Id => Order.Id;
}
