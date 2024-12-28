using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;

/// <summary>
/// Represents a JSON Web Key (JWK).
/// </summary>
public sealed record JWK
{
    /// <summary>
    /// Gets or sets the Key Type (e.g., RSA).
    /// </summary>
    [JsonPropertyName("kty")]
    public string Kty { get; init; } = default!;

    /// <summary>
    /// Gets or sets the intended use of the public key (e.g., sig for signature).
    /// </summary>
    [JsonPropertyName("use")]
    public string Use { get; init; } = default!;

    /// <summary>
    /// Gets or sets the Key ID used to match a specific key.
    /// </summary>
    [JsonPropertyName("kid")]
    public string Kid { get; init; } = default!;

    /// <summary>
    /// Gets or sets the X.509 certificate SHA-1 thumbprint, Base64 URL encoded.
    /// </summary>
    [JsonPropertyName("x5t")]
    public string X5t { get; init; } = default!;

    /// <summary>
    /// Gets or sets the exponent value for RSA keys, Base64 URL encoded.
    /// </summary>
    [JsonPropertyName("e")]
    public string E { get; init; } = default!;

    /// <summary>
    /// Gets or sets the modulus value for RSA keys, Base64 URL encoded.
    /// </summary>
    [JsonPropertyName("n")]
    public string N { get; init; } = default!;

    /// <summary>
    /// Gets or sets the X.509 certificate chain, Base64 encoded.
    /// </summary>
    [JsonPropertyName("x5c")]
    public IEnumerable<string> X5c { get; init; } = new List<string>();

    /// <summary>
    /// Gets or sets the algorithm intended for use with the key.
    /// </summary>
    [JsonPropertyName("alg")]
    public string Alg { get; init; } = default!;
}
