using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed class OrderDetectedEventArgs(GoAffProOrder order) : EventArgs
{
    public GoAffProOrder Order { get; } = order;
}
