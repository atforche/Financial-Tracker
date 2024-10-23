using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities.BulkDataUpload;

/// <summary>
/// JSON converter class to convert an entity's name into its associated GUID
/// </summary>
public class NameToGuidConverter : JsonConverter<Guid>
{
    private readonly Dictionary<string, Guid> _conversions;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="conversions">Conversions from the name found in the JSON to a GUID</param>
    public NameToGuidConverter(Dictionary<string, Guid> conversions)
    {
        _conversions = conversions;
    }

    /// <inheritdoc/>
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        _conversions[reader.GetString() ?? ""];

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) =>
        writer.WriteStringValue(_conversions.FirstOrDefault(pair => pair.Value == value).Key);
}