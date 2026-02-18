using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed record RewardEvent(GoAffProReward Reward)
{
    public string Id => Reward.Id;
}
