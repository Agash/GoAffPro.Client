using GoAffPro.Client.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Represents a detected reward feed event.
/// </summary>
/// <remarks>
/// Reward feed detection is currently disabled because <c>/user/feed/rewards</c>
/// is returning HTTP 404 as observed on 2026-02-18.
/// </remarks>
public sealed record RewardEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RewardEvent"/> record.
    /// </summary>
    /// <param name="reward">The detected reward payload.</param>
    public RewardEvent(GoAffProReward reward)
    {
        Reward = reward;
    }

    /// <summary>
    /// Gets the strongly typed reward payload.
    /// </summary>
    public GoAffProReward Reward { get; init; }

    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public string Id => Reward.Id;
}
