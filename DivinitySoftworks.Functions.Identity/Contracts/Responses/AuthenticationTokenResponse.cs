using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Contracts.Responses;

/// <summary>
/// Represents an OAuth2 authentication token response.
/// </summary>
public sealed record AuthenticationTokenResponse {
    /// <summary>
    /// Gets the access token issued by the authorization server.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = default!;

    /// <summary>
    /// Gets the type of token issued.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = default!;

    /// <summary>
    /// Gets the lifetime in seconds of the access token.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the refresh token, which can be used to obtain a new access token.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = default!;

    /// <summary>
    /// Gets the lifetime in seconds of the refresh token.
    /// </summary>
    [JsonPropertyName("refresh_token_expires_in")]
    public int RefreshTokenExpiresIn { get; init; }

    /// <summary>
    /// Gets the scope of the access token.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; init; } = string.Empty;
}
