using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Data; 

/// <summary>
/// Represents a user with identifiable and contact information.
/// </summary>
public sealed record User {
    /// <summary>
    /// Gets the partition key for the user, which is the same as the Identifier.
    /// </summary>
    [JsonIgnore]
    public string PK => Identifier.ToUpper();

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    [JsonPropertyName("PK")]
    public string Identifier { get; init; } = default!;

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    [JsonPropertyName("FirstName")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    [JsonPropertyName("LastName")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [JsonPropertyName("Email")]
    public string Email { get; set; } = default!;
}