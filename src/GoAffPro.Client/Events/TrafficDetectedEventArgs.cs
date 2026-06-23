using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.TrafficDetected"/>.
/// </summary>
/// <param name="affiliate">Detected traffic payload.</param>
public sealed class TrafficDetectedEventArgs(UserTrafficFeedItem affiliate) : EventArgs
{
    /// <summary>
    /// Gets the detected traffic payload.
    /// </summary>
    public UserTrafficFeedItem Traffic { get; } = affiliate;
}
