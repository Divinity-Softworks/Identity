namespace DivinitySoftworks.Functions.Identity.Security;
/// <summary>
/// This static class defines constants for the various OAuth2 grant types.
/// </summary>
internal static class GrantType {
    /// <summary>
    /// The Authorization Code grant type.
    /// </summary>
    public const string AuthorizationCode = "authorization_code";

    /// <summary>
    /// The Refresh Token grant type.
    /// </summary>
    public const string RefreshToken = "refresh_token";

    /// <summary>
    /// The Client Credentials grant type.
    /// </summary>
    public const string ClientCredentials = "client_credentials";
}
