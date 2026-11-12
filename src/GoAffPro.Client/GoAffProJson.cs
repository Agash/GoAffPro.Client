using System.Text.Json;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

namespace GoAffPro.Client;

/// <summary>
/// Turns a GoAffPro model back into its JSON. The models are Kiota-generated and carry no System.Text.Json
/// metadata; this writes them with Kiota's own serializer, which needs no reflection, so the result can be stored
/// or forwarded (an event journal, a queue) under trimming and Native AOT.
/// </summary>
public static class GoAffProJson
{
    /// <summary>Serializes <paramref name="model"/> as GoAffPro's JSON shape.</summary>
    public static JsonElement ToJsonElement(IParsable model)
    {
        ArgumentNullException.ThrowIfNull(model);
        using var writer = new JsonSerializationWriter();
        writer.WriteObjectValue(null, model);
        using var content = writer.GetSerializedContent();
        using var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
    }
}
