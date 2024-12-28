using DivinitySoftworks.Functions.Identity.Contracts.Requests;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Converters;

/// <summary>
/// Custom JSON converter for the TokenRequest class to handle case-insensitive property names
/// during serialization and deserialization.
/// </summary>
internal sealed class TokenRequestConverter : JsonConverter<TokenRequest> {

    /// <summary>
    /// Reads and converts the JSON object to a TokenRequest instance.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    /// <returns>A TokenRequest instance.</returns>
    public override TokenRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Dictionary<string, string> dictionary = new (StringComparer.OrdinalIgnoreCase);

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName) {
                string propertyName = reader.GetString()!;
                reader.Read();
                string propertyValue = reader.GetString()!;
                dictionary[propertyName] = propertyValue;
            }
        }

        return new TokenRequest(dictionary);
    }

    /// <summary>
    /// Writes a TokenRequest instance to a JSON object.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The TokenRequest instance to convert.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    public override void Write(Utf8JsonWriter writer, TokenRequest value, JsonSerializerOptions options) {
        writer.WriteStartObject();

        if (value.GrantType is not null)
            writer.WriteString("grant_type", value.GrantType);

        if (value.ClientId is not null)
            writer.WriteString("client_id", value.ClientId);

        if (value.ClientSecret is not null)
            writer.WriteString("client_secret", value.ClientSecret);

        if (value.Code is not null)
            writer.WriteString("code", value.Code);

        if (value.RefreshToken is not null)
            writer.WriteString("refresh_token", value.RefreshToken);

        writer.WriteEndObject();
    }
}
