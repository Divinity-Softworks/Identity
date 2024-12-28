namespace DivinitySoftworks.Functions.Identity.Security;
/// <summary>
/// Represents the result of a token validation process, including subject, email, authentication type, and identifier.
/// </summary>
public sealed record TokenValidationResult {
    /// <summary>
    /// Gets the subject of the token.
    /// </summary>
    public string Subject { get; init; } = default!;

    /// <summary>
    /// Gets the email associated with the token, if available.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the authentication type used for the token.
    /// </summary>
    public string AuthType { get; init; } = default!;

    /// <summary>
    /// Gets the unique identifier of the token.
    /// </summary>
    public string Identifier { get; init; } = default!;
}
