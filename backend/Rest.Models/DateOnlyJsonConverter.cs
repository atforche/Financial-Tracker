using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rest.Models;

/// <summary>
/// JSON converter class to convert a DateOnly object into yyyy-MM-dd format
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    /// <inheritdoc/>
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateOnly.ParseExact(reader.GetString() ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}