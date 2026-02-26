using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.TrafficDetected"/>.
/// </summary>
public sealed class TrafficDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrafficDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="affiliate">Detected traffic payload.</param>
    public TrafficDetectedEventArgs(UserTrafficFeedItem affiliate)
    {
        Traffic = affiliate;
    }

    /// <summary>
    /// Gets the detected traffic payload.
    /// </summary>
    public UserTrafficFeedItem Traffic { get; }
}
