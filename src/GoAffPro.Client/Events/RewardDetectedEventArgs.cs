using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

public sealed class RewardDetectedEventArgs(GoAffProReward reward) : EventArgs
{
    public GoAffProReward Reward { get; } = reward;
}
