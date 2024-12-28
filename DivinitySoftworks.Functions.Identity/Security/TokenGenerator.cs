using DivinitySoftworks.Core.Web.Errors;
using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Repositories;
using DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace DivinitySoftworks.Functions.Identity.Security;

/// <summary>
///<c>ITokenGenerator</c> utility is a C# utility designed to create JWT access tokens and refresh tokens. It encapsulates the logic 
/// required to generate these tokens, ensuring they are securely created and properly formatted. This class typically uses a 
/// secret key to sign the tokens, enhancing security and preventing tampering.
/// </summary>

public interface ITokenGenerator {
    /// <summary>
    /// Generates a new authentication token.
    /// </summary>
    /// <param name="subject">The subject for the token.</param>
    /// <param name="email">The email associated with the token.</param>
    /// <param name="grantType">The authentication flow used for the token.</param>
    /// <returns>The generated token result.</returns>
    Task<TokenGenerationResult> GenerateAsync(string subject, string? email, string authFlow);
    /// <summary>
    /// Validates a given refresh token and returns either a <see cref="TokenValidationResult"/> or an <see cref="ErrorResponse"/>.
    /// </summary>
    /// <param name="refreshToken">The refresh token to be validated.</param>
    /// <returns>The generated token result or an error response.</returns>
    Task<OneOf.OneOf<TokenValidationResult, ErrorResponse>> ValidateRefreshTokenAsync(string refreshToken);
}

/// <summary>
/// <c>TokenGenerator</c> utility is designed to create JWT access tokens and refresh tokens. It encapsulates the logic 
/// required to generate these tokens, ensuring they are securely created and properly formatted. This class typically uses a 
/// secret key to sign the tokens, enhancing security and preventing tampering.
/// </summary>
public sealed class TokenGenerator : ITokenGenerator {
    readonly ICertificatesRepository _certificatesRepository;
    readonly OpenIdConnectConfiguration _openIdConnectConfiguration;

    X509SigningCredentials? _credentials;

    /// <summary>
    /// <c>TokenGenerator</c> utility is designed to create JWT access tokens and refresh tokens. It encapsulates the logic 
    /// required to generate these tokens, ensuring they are securely created and properly formatted. This class typically uses a 
    /// secret key to sign the tokens, enhancing security and preventing tampering.
    /// </summary>
    /// <param name="jwtSettings">Represents the settings required for generating and validating JWT tokens.</param>
    /// <param name="certificatesRepository">Repository that provides methods to interact with the database for storing and 
    /// retrieving certificate data.</param>
    public TokenGenerator(OpenIdConnectConfiguration openIdConnectConfiguration, ICertificatesRepository certificatesRepository) {
        _openIdConnectConfiguration = openIdConnectConfiguration;
        _certificatesRepository = certificatesRepository;
    }

    /// <inheritdoc/>
    public async Task<TokenGenerationResult> GenerateAsync(string subject, string? email, string authFlow) {
        if (_credentials is null) {
            Certificate? certificate = await _certificatesRepository.ReadAsync(_openIdConnectConfiguration.Certificate.Thumbprint)
                ?? throw new KeyNotFoundException($"Certificate was not found for Thumbprint [{_openIdConnectConfiguration.Certificate.Thumbprint}]!");
            _credentials = new X509SigningCredentials(X509Certificate2.CreateFromPem(certificate.Data, certificate.Data));
        }

        return new TokenGenerationResult() {
            AccessToken = GenerateAccessToken(subject, email, authFlow, out int accessTokenExpiresIn),
            Subject = subject,
            ExpiresIn = accessTokenExpiresIn,
            RefreshToken = GenerateRefreshToken(subject, authFlow, out string refreshTokenIdentifier, out int refreshTokenExpiresIn),
            RefreshTokenExpiresIn = refreshTokenExpiresIn,
            RefreshTokenIdentifier = refreshTokenIdentifier,
            TokenType = "Bearer"
        };
    }


    /// <inheritdoc/>
    public async Task<OneOf.OneOf<TokenValidationResult, ErrorResponse>> ValidateRefreshTokenAsync(string refreshToken) {
        JwtSecurityTokenHandler tokenHandler = new();

        if (_credentials is null) {
            Certificate? certificate = await _certificatesRepository.ReadAsync(_openIdConnectConfiguration.Certificate.Thumbprint)
                ?? throw new KeyNotFoundException($"Certificate was not found for Thumbprint [{_openIdConnectConfiguration.Certificate.Thumbprint}]!");
            _credentials = new X509SigningCredentials(X509Certificate2.CreateFromPem(certificate.Data, certificate.Data));
        }

        try {
            ClaimsPrincipal principal = tokenHandler.ValidateToken(refreshToken, new() {
                ValidateIssuer = true,
                ValidIssuer = _openIdConnectConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = _openIdConnectConfiguration.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = _credentials.Key,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken _);

            if (!ReadRefreshToken(refreshToken, out string? subject, out string? email, out string? authType, out string? identifier) || subject is null || authType is null || identifier is null)
                return new ErrorResponse {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Error = "invalid_request",
                    Description = "Invalid Refresh Token."
                };


            return new TokenValidationResult() {
                Subject = subject,
                Email = email,
                AuthType = authType,
                Identifier = identifier
            };
        }
        catch (SecurityTokenExpiredException) {
            return new ErrorResponse {
                StatusCode = HttpStatusCode.Unauthorized,
                Error = "invalid_request",
                Description = "Token has expired."
            };
        }
        catch (SecurityTokenInvalidIssuerException) {
            return new ErrorResponse {
                StatusCode = HttpStatusCode.Unauthorized,
                Error = "invalid_request",
                Description = "Invalid token issuer."
            };
        }
        catch (SecurityTokenInvalidAudienceException) {
            return new ErrorResponse {
                StatusCode = HttpStatusCode.Unauthorized,
                Error = "invalid_request",
                Description = "Invalid token audience."
            };
        }
        catch (SecurityTokenInvalidSignatureException) {
            return new ErrorResponse {
                StatusCode = HttpStatusCode.Unauthorized,
                Error = "invalid_request",
                Description = "Invalid token signature."
            };
        }
        catch (Exception) {
            return new ErrorResponse {
                StatusCode = HttpStatusCode.Unauthorized,
                Error = "invalid_request",
                Description = "Invalid Refresh Token."
            };
        }
    }

    /// <summary>
    /// Reads and validates the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to read.</param>
    /// <param name="subject">The subject extracted from the token.</param>
    /// <param name="email">The email extracted from the token.</param>
    /// <param name="authType">The authentication type extracted from the token.</param>
    /// <param name="identifier">The identifier extracted from the token.</param>
    /// <returns><c>true</c> if the token is valid; otherwise, <c>false</c>.</returns>
    private static bool ReadRefreshToken(string refreshToken, out string? subject, out string? email, out string? authType, out string? identifier) {
        subject = email = authType = identifier = null;
        try {
            JwtSecurityTokenHandler handler = new();
            if (handler.CanReadToken(refreshToken)) {
                JwtSecurityToken token = handler.ReadJwtToken(refreshToken);
                Claim? tokenTypeClaim = token.Claims.FirstOrDefault(c => c.Type == "tty");
                if (tokenTypeClaim is null || string.IsNullOrWhiteSpace(tokenTypeClaim.Value) || tokenTypeClaim.Value != "refresh_token")
                    return false;
                Claim? tokenAuthClaim = token.Claims.FirstOrDefault(c => c.Type == "taf");
                if (tokenAuthClaim is null || string.IsNullOrWhiteSpace(tokenAuthClaim.Value))
                    return false;

                subject = token.Subject;
                identifier = token.Id;
                authType = tokenAuthClaim.Value;

                return true;
            }

            return false;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Generates a new access token.
    /// </summary>
    /// <param name="subject">The identifier for the token.</param>
    /// <param name="email">The email associated with the token.</param>
    /// <param name="grantType">The grant type for the token.</param>
    /// <param name="expiresIn">The expiration time of the token in seconds.</param>
    /// <returns>The generated access token.</returns>
    private string GenerateAccessToken(string subject, string? email, string grantType, out int expiresIn) {
        List<Claim> claims = [
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tty", "access_token"),
            new Claim("taf", grantType)
        ];

        if (email is not null)
            claims.Insert(1, new Claim(JwtRegisteredClaimNames.Email, email));

        expiresIn = (grantType == GrantType.ClientCredentials) ? 5 * 60 : 15 * 60;

        DateTime utcNow = DateTime.UtcNow;

        JwtSecurityToken jwtSecurityToken = new(
            _openIdConnectConfiguration.Issuer,
            _openIdConnectConfiguration.Audience,
            claims,
            utcNow,
            utcNow.AddSeconds(expiresIn),
            _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <param name="subject">The subject for the token.</param>
    /// <param name="grantType">The grant type for the token.</param>
    /// <param name="identifier">The unique identifier for the refresh token.</param>
    /// <param name="expiresIn">The expiration time of the token in seconds.</param>
    /// <returns>The generated refresh token.</returns>
    private string GenerateRefreshToken(string subject, string grantType, out string identifier, out int expiresIn) {
        identifier = Guid.NewGuid().ToString();

        Claim[] claims = [
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, identifier),
            new Claim("tty", "refresh_token"),
            new Claim("taf", grantType)
        ];

        expiresIn = (grantType == GrantType.ClientCredentials) ? 60 * 60 : 60 * 60 * 24 * 7;

        DateTime utcNow = DateTime.UtcNow;

        JwtSecurityToken jwtSecurityToken = new(
            _openIdConnectConfiguration.Issuer,
            _openIdConnectConfiguration.Audience,
            claims,
            utcNow,
            utcNow.AddSeconds(expiresIn),
            _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }
}
