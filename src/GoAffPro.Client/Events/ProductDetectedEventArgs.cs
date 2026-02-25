using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Events;

/// <summary>
/// Event arguments for <see cref="GoAffProClient.ProductDetected"/>.
/// </summary>
public sealed class ProductDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductDetectedEventArgs"/> class.
    /// </summary>
    /// <param name="product">Detected product payload.</param>
    public ProductDetectedEventArgs(UserProductFeedItem product)
    {
        Product = product;
    }

    /// <summary>
    /// Gets the detected product payload.
    /// </summary>
    public UserProductFeedItem Product { get; }
}
