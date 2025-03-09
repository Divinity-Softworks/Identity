namespace DivinitySoftworks.Functions.Identity.Settings; 
/// <summary>
/// Represents the security settings, including contact information, domain, and subdomain settings.
/// </summary>
public sealed record SecuritySettings {
    /// <summary>
    /// The key name for the security settings.
    /// </summary>
    public const string KeyName = "Security";

    /// <summary>
    /// Gets or sets the notification settings for the security settings.
    /// </summary>
    public string[] Notify { get; set; } = [];

    /// <summary>
    /// Gets or sets the contact information for the security settings.
    /// </summary>
    public string Contact { get; set; } = default!;

    /// <summary>
    /// Gets or sets the contact scheme for the security settings.
    /// </summary>
    public string ContactScheme { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical URLs associated with the security settings.
    /// </summary>
    public string[] Canonical { get; set; } = [];

    /// <summary>
    /// Gets or sets the encryption method for the security settings.
    /// </summary>
    public string Encryption { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the acknowledgment information for the security settings.
    /// </summary>
    public string? Acknowledgment { get; set; }

    /// <summary>
    /// Gets or sets the policy information for the security settings.
    /// </summary>
    public string? Policy { get; set; }

    /// <summary>
    /// Gets or sets the hiring information for the security settings.
    /// </summary>
    public string? Hiring { get; set; }
}