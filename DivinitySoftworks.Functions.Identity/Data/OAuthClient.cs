using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Data; 

/// <summary>
/// Represents an OAuth client with its credentials and configuration.
/// </summary>
public sealed record OAuthClient {
    /// <summary>
    /// Gets the partition key for the client, which is the same as the ClientId.
    /// </summary>
    [JsonIgnore]
    public string PK => ClientId;
    /// <summary>
    /// Gets or sets the unique identifier for the OAuth client.
    /// </summary>
    [JsonPropertyName("PK")]
    public string ClientId { get; init; } = default!;

    /// <summary>
    /// Gets or sets the secret key for the OAuth client.
    /// </summary>
    [JsonPropertyName("Secret")]
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Gets or sets the name of the OAuth client.
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets or sets the URL to which the OAuth server will redirect after authorization.
    /// </summary>
    [JsonPropertyName("ReturnUrl")]
    public string ReturnUrl { get; init; } = default!;
}
