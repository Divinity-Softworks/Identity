using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DivinitySoftworks.Core.Web.Errors;
using DivinitySoftworks.AWS.Core.Web.Functions;
using DivinitySoftworks.Core.Web.Security;
using DivinitySoftworks.Functions.Identity.Contracts.Requests;
using DivinitySoftworks.Functions.Identity.Contracts.Responses;
using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Repositories;
using DivinitySoftworks.Functions.Identity.Security;
using Microsoft.Extensions.Configuration;
using OneOf;
using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;

using static Amazon.Lambda.Annotations.APIGateway.HttpResults;

namespace DS.Functions.Identity;

/// <summary>
/// Provides endpoints for handling OAuth 2.0 authorization flows and token management for Amazon Lambda functions.
/// </summary>
/// <param name="authorizeService">The service used to handle authorization.</param>
public sealed class OAuth([FromServices] IAuthorizeService authorizeService) : ExecutableFunction(authorizeService) {
    const string RootBase = "/oauth";
    const string RootResourceName = "DSOAuth";

    /// <summary>
    /// Handles the OAuth 2.0 authorization request, initiating the authorization code flow.
    /// </summary>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="request">The API Gateway request.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="oAuthClientsRepository">The repository for OAuth clients.</param>
    /// <param name="response_type">The response type query parameter.</param>
    /// <param name="client_id">The client ID query parameter.</param>
    /// <param name="authorizationHeader">The authorization header.</param>
    /// <returns>The HTTP result of the authorization request.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetAuthorizeAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}/authorize")]
    public async Task<IHttpResult> GetAuthorizeAsync(ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request
        , [FromServices] IConfiguration configuration
        , [FromServices] IOAuthClientsRepository oAuthClientsRepository
        , [FromQuery] string? response_type, [FromQuery] string? client_id
        , [FromHeader(Name = "Authorization")] string authorizationHeader) {

        return await ExecuteAsync(Authorize.AllowAnonymous, context, request,
            async () => {
                // Validate response_type parameter
                if (response_type is null)
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'response_type' parameter is missing."));
                if (response_type != "code")
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "unsupported_response_type", "The authorization server does not support obtaining an authorization code using this method."));

                // Extract Client ID and Client Secret from Authorization header if present
                string? clientId = null;
                string? clientSecret = null;
                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) {
                    string encodedCredentials = authorizationHeader["Basic ".Length..].Trim();
                    string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':', 2);
                    clientId = credentials[0];
                    clientSecret = credentials[1];
                }

                // Validate client_id parameter
                if (string.IsNullOrEmpty(clientId) && string.IsNullOrWhiteSpace(client_id))
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'client_id' parameter is missing or malformed."));

                // Retrieve the OAuth client from the repository
                OAuthClient? oAuthClient = await oAuthClientsRepository.ReadAsync(clientId ?? client_id!);
                if (oAuthClient is null)
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The client was not found."));

                // Create a SAML request and redirect the user to the login URL
                SamlRequest samlRequest = new(
                    configuration.GetValue<string>("Authentication:Identifier")!,
                    configuration.GetValue<string>("Authentication:URLs:ACS")!);

                return Redirect(samlRequest.GetRedirectUrl($"{configuration.GetValue<string>("Authentication:URLs:Login")!}?RelayState={oAuthClient.ClientId}"));
            });
    }

    /// <summary>
    /// Handles the Assertion Consumer Service (ACS) request, processing SAML responses for OAuth 2.0 authentication.
    /// </summary>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="request">The API Gateway request.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="certificatesRepository">The repository for certificates.</param>
    /// <param name="userRepository">The repository for users.</param>
    /// <param name="oAuthAuthorizationCodesRepository">The repository for OAuth authorization codes.</param>
    /// <param name="oAuthClientsRepository">The repository for OAuth clients.</param>
    /// <returns>The HTTP result of the ACS request.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(PostACSAsync)}")]
    [HttpApi(LambdaHttpMethod.Post, $"{RootBase}/acs")]
    public async Task<IHttpResult> PostACSAsync(ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request
        , [FromServices] IConfiguration configuration
        , [FromServices] ICertificatesRepository certificatesRepository, [FromServices] IUsersRepository userRepository
        , [FromServices] IOAuthAuthorizationCodesRepository oAuthAuthorizationCodesRepository, [FromServices] IOAuthClientsRepository oAuthClientsRepository) {

        return await ExecuteAsync(Authorize.AllowAnonymous, context, request,
            async () => {
                // Retrieve the certificate for SAML response validation
                Certificate? certificate = await certificatesRepository.ReadAsync(configuration.GetValue<string>("Authentication:Certificate:Thumbprint")!)
                    ?? throw new KeyNotFoundException($"Certificate was not found for Thumbprint [{configuration.GetValue<string>("Authentication:Certificate:Thumbprint")!}]!");

                // Validate the SAML response
                SamlResponse samlResponse = new(certificate.Data, request.ToNameValueCollection()["SAMLResponse"]!);
                string? relayState = request.ToNameValueCollection()["RelayState"];
                if (relayState is null)
                    return Redirect(@"https://www.google.com");

                // Retrieve the OAuth client using the relay state
                OAuthClient? oAuthClient = await oAuthClientsRepository.ReadAsync(relayState);
                if (oAuthClient is null)
                    return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_request", "The client was not found for Identifier [{relayState}].").ToHttpResult();

                if (!samlResponse.IsValid())
                    return Redirect(@"https://www.google.com");

                // Retrieve or create the user based on SAML response data
                User? user = await userRepository.ReadAsync(samlResponse.GetObjectId()!);
                if (user is null) {
                    user = new() {
                        Identifier = samlResponse.GetObjectId()!.ToUpper(),
                        FirstName = samlResponse.GetFirstname(),
                        LastName = samlResponse.GetLastname(),
                        Email = samlResponse.GetEmail() ?? string.Empty,
                    };
                    if (!await userRepository.CreateAsync(user))
                        throw new DataException($"Creating a user has failed!");
                }
                else {
                    user.FirstName = samlResponse.GetFirstname();
                    user.LastName = samlResponse.GetLastname();
                    user.Email = samlResponse.GetEmail() ?? string.Empty;
                    if (!await userRepository.PutAsync(user))
                        throw new DataException($"Updating the user has failed!");
                }

                // Create an authorization code and redirect the user
                OAuthAuthorizationCode oAuthAuthorizationCode = new() {
                    Token = Guid.NewGuid().ToString(),
                    TokenType = "authorization_code",
                    UserId = user.Identifier.ToUpper(),
                    Expiration = DateTime.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
                };

                if (!await oAuthAuthorizationCodesRepository.CreateAsync(oAuthAuthorizationCode))
                    throw new DataException($"Creating an authorization code has failed!");

                return Redirect($"{oAuthClient.ReturnUrl}?code={oAuthAuthorizationCode.Token}");
            });
    }

    /// <summary>
    /// Handles the token request, issuing tokens in exchange for authorization codes or refresh tokens.
    /// </summary>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="request">The API Gateway request.</param>
    /// <param name="oAuthClientsRepository">The repository for OAuth clients.</param>
    /// <param name="userRepository">The repository for users.</param>
    /// <param name="oAuthAuthorizationCodesRepository">The repository for OAuth authorization codes.</param>
    /// <param name="tokenGenerator">The service for generating tokens.</param>
    /// <param name="contentType">The content type header.</param>
    /// <param name="authorizationHeader">The authorization header.</param>
    /// <returns>The HTTP result of the token request.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(PostTokenAsync)}")]
    [HttpApi(LambdaHttpMethod.Post, $"{RootBase}/token")]
    public async Task<IHttpResult> PostTokenAsync(ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request
        , [FromServices] IOAuthClientsRepository oAuthClientsRepository
        , [FromServices] IUsersRepository userRepository, [FromServices] IOAuthAuthorizationCodesRepository oAuthAuthorizationCodesRepository
        , [FromServices] ITokenGenerator tokenGenerator
        , [FromHeader(Name = "Content-Type")] string contentType
        , [FromHeader(Name = "Authorization")] string authorizationHeader) {

        return await ExecuteAsync(Authorize.AllowAnonymous, context, request,
            async () => {
                // Validate content-type header
                if (contentType is null)
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "unsupported_content_type", "The content-type was not provided."));
                if (!contentType.Contains("application/json") && !contentType.Contains("application/x-www-form-urlencoded"))
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "unsupported_content_type", $"The content-type provided [{contentType}] is not supported."));

                // Parse the token request based on content-type
                TokenRequest? tokenRequest = null;
                if (contentType.Contains("application/json"))
                    tokenRequest = JsonSerializer.Deserialize<TokenRequest>(request.Body);
                if (contentType.Contains("application/x-www-form-urlencoded"))
                    tokenRequest = new TokenRequest(Encoding.UTF8.GetString(Convert.FromBase64String(request.Body))
                        .Split('&')
                        .Select(part => part.Split('='))
                        .Where(part => part.Length == 2)
                        .ToDictionary(sp => Uri.UnescapeDataString(sp[0]), sp => Uri.UnescapeDataString(sp[1])));

                if (tokenRequest is null)
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The request payload is missing."));
                if (!tokenRequest.TryValidate(out ErrorResponse? errorResponse))
                    return errorResponse!.ToHttpResult();

                // Extract Client ID and Client Secret from Authorization header

                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) {
                    string encodedCredentials = authorizationHeader["Basic ".Length..].Trim();
                    string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':', 2);
                    if (!string.IsNullOrWhiteSpace(tokenRequest.ClientId) && tokenRequest.ClientId != credentials[0])
                        return BadRequest(new ErrorResponse());
                    if (!string.IsNullOrWhiteSpace(tokenRequest.ClientSecret) && tokenRequest.ClientSecret != credentials[1])
                        return BadRequest(new ErrorResponse());

                    tokenRequest.ClientId = credentials[0];
                    tokenRequest.ClientSecret = credentials[1].DecodeIfUrlEncoded();
                }

                // Validate client_id parameter
                if (string.IsNullOrEmpty(tokenRequest.ClientId))
                    return BadRequest(new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'client_id' parameter is missing or malformed."));

                // Retrieve the OAuth client from the repository
                OAuthClient? oAuthClient = await oAuthClientsRepository.ReadAsync(tokenRequest.ClientId);
                if (oAuthClient is null)
                    return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_request", "The client was not found.").ToHttpResult();

                // Handle authorization code or refresh token flow
                OneOf<TokenGenerationResult, ErrorResponse>? result = null;
                if (tokenRequest.GrantType == GrantType.AuthorizationCode)
                    result = await HandleAuthorizationCodeFlow(tokenRequest.Code!, oAuthAuthorizationCodesRepository, userRepository, tokenGenerator);
                if (tokenRequest.GrantType == GrantType.ClientCredentials)
                    result = await HandleClientCredentialFlow(oAuthClient, tokenRequest.ClientSecret, tokenGenerator);
                if (tokenRequest.GrantType == GrantType.RefreshToken)
                    result = await HandleRefreshTokenFlow(tokenRequest.RefreshToken!, oAuthAuthorizationCodesRepository, userRepository, tokenGenerator);
                if (result is null)
                    return new ErrorResponse(HttpStatusCode.BadRequest, "unsupported_grant_type", "The authorization server does not support the 'grant_type' parameter provided.").ToHttpResult();

                if (result.Value.IsT1)
                    return result.Value.AsT1.ToHttpResult();

                TokenGenerationResult authenticationToken = result.Value.AsT0;

                // Create a new refresh token
                if (!await oAuthAuthorizationCodesRepository.CreateAsync(new OAuthAuthorizationCode() {
                    Token = authenticationToken.RefreshTokenIdentifier,
                    TokenType = GrantType.RefreshToken,
                    UserId = (tokenRequest.GrantType == GrantType.ClientCredentials) ? null : authenticationToken.Subject,
                    ClientId = (tokenRequest.GrantType == GrantType.ClientCredentials) ? oAuthClient.ClientId : null,
                    Expiration = DateTime.UtcNow.AddSeconds(authenticationToken.RefreshTokenExpiresIn).ToUnixTimeSeconds(),
                }))
                    throw new DataException($"Creating a refresh token has failed!");

                // Prepare the response
                AuthenticationTokenResponse response = new() {
                    AccessToken = authenticationToken.AccessToken,
                    ExpiresIn = authenticationToken.ExpiresIn,
                    TokenType = authenticationToken.TokenType,
                    RefreshToken = authenticationToken.RefreshToken,
                    RefreshTokenExpiresIn = authenticationToken.RefreshTokenExpiresIn,
                    Scope = authenticationToken.Scope,
                };

                return Ok(response);
            });
    }

    /// <summary>
    /// Handles the authorization code flow for exchanging authorization codes for tokens.
    /// </summary>
    /// <param name="code">The authorization code.</param>
    /// <param name="oAuthAuthorizationCodesRepository">The repository for OAuth authorization codes.</param>
    /// <param name="userRepository">The repository for user data.</param>
    /// <param name="tokenGenerator">The service for generating tokens.</param>
    /// <returns>The result of the authorization code flow.</returns>
    private static async Task<OneOf<TokenGenerationResult, ErrorResponse>> HandleAuthorizationCodeFlow(string code
        , IOAuthAuthorizationCodesRepository oAuthAuthorizationCodesRepository
        , IUsersRepository userRepository
        , ITokenGenerator tokenGenerator) {

        OAuthAuthorizationCode? oAuthAuthorizationCode = await oAuthAuthorizationCodesRepository.ReadAsync(code, GrantType.AuthorizationCode);
        if (oAuthAuthorizationCode is null)
            return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_grant", "The provided authorization code is invalid.");

        // Delete the used authorization code
        await oAuthAuthorizationCodesRepository.DeleteAsync(oAuthAuthorizationCode);

        // Validate the expiration of the authorization code
        if (oAuthAuthorizationCode.Expiration.FromUnixTimeSeconds().Ticks < DateTime.UtcNow.Ticks)
            return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_grant", "The provided authorization code, or refresh token, has expired.");

        // Retrieve the user associated with the authorization code
        User? user = oAuthAuthorizationCode.UserId is not null ? await userRepository.ReadAsync(oAuthAuthorizationCode.UserId) : null;
        if (user is null)
            return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_grant", "The user associated with the authorization code, or refresh token, cannot be found.");

        return await tokenGenerator.GenerateAsync(user.Identifier.ToUpper(), user.Email, GrantType.AuthorizationCode);
    }

    /// <summary>
    /// Handles the client credentials flow for generating access tokens using client credentials.
    /// </summary>
    /// <param name="oAuthClient">The OAuth client information.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="tokenGenerator">The service for generating tokens.</param>
    /// <returns>The result of the client credentials flow.</returns>
    private static async Task<OneOf<TokenGenerationResult, ErrorResponse>?> HandleClientCredentialFlow(OAuthClient oAuthClient, string? clientSecret
        , ITokenGenerator tokenGenerator) {
        // Validate client_secret parameter
        if (string.IsNullOrEmpty(clientSecret))
            return new ErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "The 'client_secret' parameter is missing or malformed.");

        // Retrieve the OAuth client from the repository
        if (oAuthClient is null)
            return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_request", "The client was not found.");

        if (oAuthClient.ClientSecret is null || !BCrypt.Net.BCrypt.Verify(clientSecret, oAuthClient.ClientSecret))
            return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_client", "The client authentication failed.");

        return await tokenGenerator.GenerateAsync(oAuthClient.ClientId, null, GrantType.ClientCredentials);
    }

    /// <summary>
    /// Handles the refresh token flow for exchanging refresh tokens for new access tokens.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="oAuthAuthorizationCodesRepository">The repository for OAuth authorization codes.</param>
    /// <param name="userRepository">The repository for user data.</param>
    /// <param name="tokenGenerator">The service for generating tokens.</param>
    /// <returns>The result of the refresh token flow.</returns>
    private static async Task<OneOf<TokenGenerationResult, ErrorResponse>> HandleRefreshTokenFlow(string refreshToken
        , IOAuthAuthorizationCodesRepository oAuthAuthorizationCodesRepository
        , IUsersRepository userRepository
        , ITokenGenerator tokenGenerator) {

        OneOf<TokenValidationResult, ErrorResponse>? result = await tokenGenerator.ValidateRefreshTokenAsync(refreshToken);

        if (result.Value.IsT1)
            return result.Value.AsT1;

        TokenValidationResult tokenValidationResult = result.Value.AsT0;

        OAuthAuthorizationCode? oAuthAuthorizationCode = await oAuthAuthorizationCodesRepository.ReadAsync(tokenValidationResult.Identifier, GrantType.RefreshToken);
        if (oAuthAuthorizationCode is null)
            return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_grant", "The provided refresh token is invalid.");

        await oAuthAuthorizationCodesRepository.DeleteAsync(tokenValidationResult.Identifier, GrantType.RefreshToken);

        User? user = null;
        switch (tokenValidationResult.AuthType) {
            case GrantType.AuthorizationCode:
                user = await userRepository.ReadAsync(tokenValidationResult.Subject);
                if (user is null || oAuthAuthorizationCode.UserId != tokenValidationResult.Subject)
                    return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_grant", "The user associated with the refresh token cannot be found.");
                break;
            case GrantType.ClientCredentials:
                break;
            default:
                return new ErrorResponse(HttpStatusCode.Unauthorized, "invalid_grant", "The provided refresh token is invalid.");
        }

        return await tokenGenerator.GenerateAsync(tokenValidationResult.Subject, user?.Email, tokenValidationResult.AuthType);
    }
}
