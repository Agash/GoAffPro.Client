using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.RewardDetected"/>.
/// </summary>
/// <remarks>
/// Reward feed detection is currently disabled because <c>/user/feed/rewards</c>
/// is returning HTTP 404 as observed on 2026-02-18.
/// </remarks>
/// <param name="reward">Detected reward payload.</param>
public sealed class RewardDetectedEventArgs(UserRewardFeedItem reward) : EventArgs
{
    /// <summary>
    /// Gets the detected reward payload.
    /// </summary>
    public UserRewardFeedItem Reward { get; } = reward;
}
