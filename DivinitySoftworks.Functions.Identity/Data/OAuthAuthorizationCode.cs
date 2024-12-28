using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Data;

/// <summary>
/// Represents an OAuth authorization code with its associated details.
/// </summary>
public sealed record OAuthAuthorizationCode {
    /// <summary>
    /// Gets the partition key for the authorization code, which is the same as the Token.
    /// </summary>
    [JsonIgnore]
    public string PK => Token;

    /// <summary>
    /// Gets the sort key for the authorization code, which is the same as the TokenType.
    /// </summary>
    [JsonIgnore]
    public string SK => TokenType;

    /// <summary>
    /// Gets or sets the authorization token.
    /// </summary>
    [JsonPropertyName("PK")]
    public string Token { get; init; } = default!;

    /// <summary>
    /// Gets or sets the type of the token.
    /// </summary>
    [JsonPropertyName("SK")]
    public string TokenType { get; init; } = default!;

    /// <summary>
    /// Gets or sets the user identifier associated with the authorization code.
    /// </summary>
    [JsonPropertyName("UserId")]
    public string? UserId { get; init; }

    /// <summary>
    /// Gets or sets the client identifier associated with the authorization code.
    /// </summary>
    [JsonPropertyName("ClientId")]
    public string? ClientId { get; init; }

    /// <summary>
    /// Gets or sets the expiration time of the token in Unix time seconds.
    /// </summary>
    [JsonPropertyName("Expiration")]
    public long Expiration { get; init; } = default!;
}
