using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.RewardDetected"/>.
/// </summary>
/// <remarks>
/// Reward feed detection is currently disabled because <c>/user/feed/rewards</c>
/// is returning HTTP 404 as observed on 2026-02-18.
/// </remarks>
public sealed class RewardDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RewardDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="reward">Detected reward payload.</param>
    public RewardDetectedEventArgs(UserRewardFeedItem reward)
    {
        Reward = reward;
    }

    /// <summary>
    /// Gets the detected reward payload.
    /// </summary>
    public UserRewardFeedItem Reward { get; }
}
