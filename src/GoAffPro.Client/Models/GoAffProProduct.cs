#pragma warning disable CA1056, CA1054
using System.Text.Json;

namespace GoAffPro.Client.Models;

/// <summary>
/// Represents a single product item from <c>/user/feed/products</c>.
/// </summary>
public sealed record GoAffProProduct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProProduct"/> record.
    /// </summary>
    /// <param name="id">Unique product identifier.</param>
    /// <param name="name">Product name.</param>
    /// <param name="description">Product description.</param>
    /// <param name="price">Product price.</param>
    /// <param name="salePrice">Product sale price if on sale.</param>
    /// <param name="imageUrl">Product image URL.</param>
    /// <param name="productUrl">URL to the product page.</param>
    /// <param name="category">Product category.</param>
    /// <param name="sku">Product SKU.</param>
    /// <param name="currency">Currency code associated with the product price.</param>
    /// <param name="rawPayload">Original JSON payload returned by the feed endpoint.</param>
    public GoAffProProduct(
        string id,
        string? name,
        string? description,
        decimal? price,
        decimal? salePrice,
        string? imageUrl,
        string? productUrl,
        string? category,
        string? sku,
        string? currency,
        JsonElement rawPayload)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        SalePrice = salePrice;
        ImageUrl = imageUrl;
        ProductUrl = productUrl;
        Category = category;
        SKU = sku;
        Currency = currency;
        RawPayload = rawPayload;
    }

    /// <summary>
    /// Unique product identifier.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Product price.
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// Product sale price if on sale.
    /// </summary>
    public decimal? SalePrice { get; init; }

    /// <summary>
    /// Product image URL.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// URL to the product page.
    /// </summary>
    public string? ProductUrl { get; init; }

    /// <summary>
    /// Product category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public string? SKU { get; init; }

    /// <summary>
    /// Currency code associated with the product price.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Original JSON payload for advanced scenarios not covered by typed properties.
    /// </summary>
    public JsonElement RawPayload { get; init; }
}
#pragma warning restore CA1056, CA1054
