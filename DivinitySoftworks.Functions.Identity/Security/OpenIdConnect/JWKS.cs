using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;

/// <summary>
/// Represents a JSON Web Key Set (JWKS).
/// </summary>
public sealed record JWKS
{
    /// <summary>
    /// Gets or sets the collection of JSON Web Keys.
    /// </summary>
    [JsonPropertyName("keys")]
    public IEnumerable<JWK> Keys { get; init; } = new List<JWK>();
}