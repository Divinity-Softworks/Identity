using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Data;

/// <summary>
/// Represents a certificate with its thumbprint, expiration, and base64 encoded data.
/// </summary>
public sealed record Certificate {
    /// <summary>
    /// Gets the partition key for the certificate, which is the same as the Thumbprint.
    /// </summary>
    [JsonIgnore]
    public string PK => Thumbprint;

    /// <summary>
    /// Gets or sets the thumbprint of the certificate.
    /// The thumbprint is a unique hexadecimal string that identifies the certificate.
    /// </summary>
    [JsonPropertyName("PK")]
    public string Thumbprint { get; init; } = default!;

    /// <summary>
    /// Gets or sets the expiration time of the certificate in Unix time format.
    /// </summary>
    [JsonPropertyName("Expiration")]
    public long Expiration { get; init; } = default!;

    /// <summary>
    /// Gets or sets the base64 encoded data of the certificate, which may optionally include a private key.
    /// </summary>
    [JsonPropertyName("Data")]
    public string Data { get; init; } = default!;
}