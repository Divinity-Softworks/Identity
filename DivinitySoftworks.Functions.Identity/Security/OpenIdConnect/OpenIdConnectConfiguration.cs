using DivinitySoftworks.Functions.Identity.Settings;
using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;

public sealed record OpenIdConnectConfiguration {

    /// <summary>
    /// Gets or sets the id of the configuration.
    /// The id is a unique identifier for the open id configuration.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the issuer of the JWT token.
    /// The issuer is a unique identifier for the entity that issues the token.
    /// </summary>
    [JsonPropertyName("issuer")]
    public string Issuer { get; init; } = default!;

    /// <summary>
    /// Gets or sets the audience of the JWT token.
    /// The audience is the intended recipient of the token.
    /// </summary>
    [JsonIgnore]
    public string Audience { get; set; } = default!;

    /// <summary>
    /// Gets the URL to which the client can redirect the user agent to authenticate.
    /// </summary>
    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; init; } = default!;

    /// <summary>
    /// Gets the URL at which the client can obtain a token.
    /// </summary>
    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; init; } = default!;

    /// <summary>
    /// Gets the URL of the JWKS endpoint containing the public keys used for token validation.
    /// </summary>
    [JsonPropertyName("jwks_uri")]
    public string JwksUri { get; init; } = default!;

    /// <summary>
    /// Gets or sets the certificate settings used to sign the JWT token.
    /// </summary>
    [JsonIgnore]
    public CertificateSettings Certificate { get; set; } = default!;
}
