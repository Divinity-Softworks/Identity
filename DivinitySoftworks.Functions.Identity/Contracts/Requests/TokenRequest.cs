using DivinitySoftworks.Core.Web.Errors;
using DivinitySoftworks.Functions.Identity.Converters;
using System.Net;
using System.Text.Json.Serialization;

namespace DivinitySoftworks.Functions.Identity.Contracts.Requests;

/// <summary>
/// Represents the model for the token request payload.
/// </summary>
[JsonConverter(typeof(TokenRequestConverter))]
public sealed record TokenRequest {

    /// <summary>
    /// Represents the model for the token request payload.
    /// </summary>
    public TokenRequest() {

    }

    /// <summary>
    /// Represents the model for the token request payload.
    /// </summary>
    /// <param name="dictionary">The dictionary containing the data.</param>
    public TokenRequest(Dictionary<string, string> dictionary) {
        if (dictionary.TryGetValue("grant_type", out string? grantType))
            GrantType = grantType;
        else if (dictionary.TryGetValue(nameof(GrantType), out string? _GrantType))
            GrantType = _GrantType;
        if (dictionary.TryGetValue("client_id", out string? clientId))
            ClientId = clientId;
        else if (dictionary.TryGetValue(nameof(ClientId), out string? _ClientId))
            ClientId = _ClientId;
        if (dictionary.TryGetValue("client_secret", out string? clientSecret))
            ClientSecret = clientSecret;
        else if (dictionary.TryGetValue(nameof(ClientSecret), out string? _ClientSecret))
            ClientSecret = _ClientSecret;
        if (dictionary.TryGetValue("code", out string? code))
            Code = code;
        else if (dictionary.TryGetValue(nameof(Code), out string? _Code))
            Code = _Code;
        if (dictionary.TryGetValue("refresh_token", out string? refreshToken))
            RefreshToken = refreshToken;
        else if (dictionary.TryGetValue(nameof(RefreshToken), out string? _RefreshToken))
            RefreshToken = _RefreshToken;
    }

    /// <summary>
    /// Gets or sets the grant type.
    /// </summary>
    [JsonPropertyName("grant_type")]
    public string? GrantType { get; set; }

    /// <summary>
    /// Gets or sets the client id.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the authorization code.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Validates the current TokenRequest instance to ensure all required parameters are present
    /// and correct based on the specified grant type.
    /// </summary>
    /// <param name="errorResponse">
    /// Output parameter that will contain an <see cref="ErrorResponse"/> with details if the validation fails.
    /// </param>
    /// <returns>
    /// True if validation succeeds; otherwise, false. If validation fails, <paramref name="errorResponse"/> will be populated with error details.
    /// </returns>
    public bool TryValidate(out ErrorResponse? errorResponse) {
        errorResponse = null;

        if (GrantType is null)
            errorResponse = new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'grant_type' parameter is missing.");
        if (GrantType != Security.GrantType.AuthorizationCode && GrantType != Security.GrantType.RefreshToken && GrantType != Security.GrantType.ClientCredentials)
            errorResponse = new ErrorResponse(HttpStatusCode.BadRequest, "unsupported_grant_type", "The authorization server does not support the 'grant_type' parameter provided.");
        if (GrantType == Security.GrantType.AuthorizationCode && string.IsNullOrEmpty(Code))
            errorResponse = new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'code' parameter is missing.");
        if (GrantType == Security.GrantType.RefreshToken && string.IsNullOrEmpty(RefreshToken))
            errorResponse = new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'refresh_token' parameter is missing.");

        return true;
    }
}
