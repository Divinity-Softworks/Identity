namespace DivinitySoftworks.Functions.Identity.Security;

/// <summary>
/// The TokenInfo class is a C# representation designed to encapsulate the essential information related to authentication 
/// tokens, including both access and refresh tokens. This class provides a structured way to handle token-related data 
/// in your application, ensuring consistency and clarity in your authentication logic.
/// </summary>
public sealed record TokenGenerationResult {
    /// <summary>
    /// The JWT access token used for authenticated requests.
    /// </summary>
    public string AccessToken { get; set; } = default!;

    /// <summary>
    /// The type of the token. Typically "bearer" for JWT.
    /// </summary>
    public string TokenType { get; set; } = default!;

    /// <summary>
    /// The time in seconds until the access token expires.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The refresh token token identifer used to validate the refresh token.
    /// </summary>
    public string RefreshTokenIdentifier { get; set; } = default!;

    /// <summary>
    /// The refresh token used to obtain a new access token.
    /// </summary>
    public string RefreshToken { get; set; } = default!;

    /// <summary>
    /// The time in seconds until the refresh token expires.
    /// </summary>
    public int RefreshTokenExpiresIn { get; set; }

    /// <summary>
    /// A space-separated list of scopes associated with the access token.
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// The subject (identifier) of the entity that requests the token.
    /// </summary>
    public string? Subject { get; set; }
}
