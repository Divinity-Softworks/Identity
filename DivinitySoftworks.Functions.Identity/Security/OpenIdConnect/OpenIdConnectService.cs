using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;

/// <summary>
/// Provides functionality for interacting with OpenID Connect configurations,
/// including fetching JSON Web Key Sets (JWKS) asynchronously.
/// </summary>
public interface IOpenIdConnectService {

    /// <summary>
    /// Gets the OpenID Connect configuration.
    /// </summary>
    OpenIdConnectConfiguration OpenIdConnectConfiguration { get; }

    /// <summary>
    /// Asynchronously retrieves the JSON Web Key Set (JWKS).
    /// </summary>
    /// <returns>The retrieved JWKS.</returns>
    Task<JWKS> GetJWKSAsync();
}

/// <summary>
/// Implementation of the IOpenIdConnectService interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the OpenIdConnectService class with the provided dependencies.
/// </remarks>
/// <param name="openIdConnectConfiguration">The OpenID Connect configuration.</param>
/// <param name="certificatesRepository">The repository for certificates.</param>
public sealed class OpenIdConnectService(OpenIdConnectConfiguration openIdConnectConfiguration, ICertificatesRepository certificatesRepository) : IOpenIdConnectService {
    readonly OpenIdConnectConfiguration _openIdConnectConfiguration = openIdConnectConfiguration;
    readonly ICertificatesRepository _certificatesRepository = certificatesRepository;

    JWKS? jwks = null;

    /// <inheritdoc/>
    public OpenIdConnectConfiguration OpenIdConnectConfiguration => _openIdConnectConfiguration;

    /// <inheritdoc/>
    public async Task<JWKS> GetJWKSAsync() {
        if (jwks is not null)
            return jwks;

        Certificate? certificate = await _certificatesRepository.ReadAsync(_openIdConnectConfiguration.Certificate.Thumbprint)
                ?? throw new KeyNotFoundException($"Certificate was not found for Thumbprint [{_openIdConnectConfiguration.Certificate.Thumbprint}]!");

        X509Certificate2 x509Certificate2 = X509Certificate2.CreateFromPem(certificate.Data, certificate.Data);
        RSA? rsa = x509Certificate2.GetRSAPublicKey()
            ?? throw new InvalidOperationException("RSA public key extraction failed.");

        RSAParameters parameters = rsa.ExportParameters(false);

        return new() {
            Keys = new[] {
                new JWK() {
                    Kty = "RSA",
                    Use = "sig",
                    Kid = certificate.Thumbprint,
                    Alg = "RS256", // Assuming RS256. Update based on your certificate's algorithm.
                    N = Base64UrlEncoder.Encode(parameters.Modulus),
                    E =  Base64UrlEncoder.Encode(parameters.Exponent),
                    X5t = Base64UrlEncoder.Encode(x509Certificate2.GetCertHash(HashAlgorithmName.SHA1)),
                    X5c = new [] {
                        Convert.ToBase64String(x509Certificate2.Export(X509ContentType.Cert))
                    }
                }
            }
        };
    }
}
