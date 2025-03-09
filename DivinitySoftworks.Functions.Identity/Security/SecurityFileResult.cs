namespace DivinitySoftworks.Functions.Identity.Security;

/// <summary>
/// Represents the result of creating security files, including the signed content, public key, and any exceptions that occurred.
/// </summary>
public sealed record SecurityFileResult {
    /// <summary>
    /// Gets or sets the signed content of the security files.
    /// </summary>
    public string? SignedContent { get; internal set; }

    /// <summary>
    /// Gets or sets the public key used for signing the security files.
    /// </summary>
    public string? PublicKey { get; internal set; }

    /// <summary>
    /// Gets or sets any exception that occurred during the creation of the security files.
    /// </summary>
    public Exception? Exception { get; internal set; }
}