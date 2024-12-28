namespace DivinitySoftworks.Functions.Identity.Settings;
/// <summary>
/// Represents information about a certificate, including its thumbprint.
/// </summary>
public sealed record CertificateSettings {
    /// <summary>
    /// Gets or sets the thumbprint of the certificate.
    /// The thumbprint is a unique hexadecimal string that identifies the certificate.
    /// </summary>
    public string Thumbprint { get; set; } = default!;
}
