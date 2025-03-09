using DivinitySoftworks.Functions.Identity.Security;
using DivinitySoftworks.Functions.Identity.Settings;
using PgpCore;
using System.Text;

namespace DivinitySoftworks.Functions.Identity.Services;

/// <summary>
/// Defines the interface for security services.
/// </summary>
public interface ISecurityService {
    /// <summary>
    /// Gets the security settings.
    /// </summary>
    SecuritySettings Settings { get; }
    /// <summary>
    /// Creates security files.
    /// </summary>
    /// <returns>A result containing the security files.</returns>
    SecurityFileResult CreateSecurityFiles();
}

/// <summary>
/// Provides implementation for security services.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SecurityService"/> class.
/// </remarks>
/// <param name="securitySettings">The security settings.</param>
public sealed class SecurityService(SecuritySettings securitySettings) : ISecurityService {
    readonly SecuritySettings _securitySettings = securitySettings;

    /// <inheritdoc/>
    public SecuritySettings Settings => _securitySettings;

    /// <inheritdoc/>
    public SecurityFileResult CreateSecurityFiles() {
        SecurityFileResult securityFileResult = new();
        try {
            StringBuilder content = new();

            content.AppendLine($"Contact: {_securitySettings.ContactScheme}{_securitySettings.Contact}");

            foreach (string canonical in _securitySettings.Canonical)
                content.AppendLine($"Canonical: {canonical}");

            content.AppendLine($"Encryption: {_securitySettings.Encryption}");
            content.AppendLine($"Expires: {new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(2).AddDays(-1):yyyy-MM-ddTHH:mm:ssZ}");

            if (!string.IsNullOrEmpty(_securitySettings.Acknowledgment))
                content.AppendLine($"Acknowledgments: {_securitySettings.Acknowledgment}");

            if (!string.IsNullOrEmpty(_securitySettings.Policy))
                content.AppendLine($"Policy: {_securitySettings.Policy}");

            if (!string.IsNullOrEmpty(_securitySettings.Hiring))
                content.AppendLine($"Hiring: {_securitySettings.Hiring}");

            SignContent(content, _securitySettings.Contact, out string signedContent, out string publicKey);

            securityFileResult.SignedContent = signedContent;
            securityFileResult.PublicKey = publicKey;
        }
        catch (Exception exception) {
            securityFileResult.Exception = exception;
        }

        return securityFileResult;
    }

    /// <summary>
    /// Signs the content using PGP encryption.
    /// </summary>
    /// <param name="content">The content to be signed.</param>
    /// <param name="contact">The contact information for the signature.</param>
    /// <param name="signedContent">The signed content output.</param>
    /// <param name="publicKey">The public key output.</param>
    static void SignContent(StringBuilder content, string contact, out string signedContent, out string publicKey) {
        using MemoryStream contentMemoryStream = new();
        using StreamWriter writer = new(contentMemoryStream);

        writer.Write(content);
        writer.Flush();
        contentMemoryStream.Position = 0;

        string privateKeyPassword = Guid.NewGuid().ToString();
        GenerateKeys(out publicKey, out string privateKey, contact, privateKeyPassword);

        EncryptionKeys encryptionKeys = new(publicKey, privateKey, privateKeyPassword);

        MemoryStream signedSecurityTxtStream = new();
        using (PGP pgp = new(encryptionKeys))
            pgp.ClearSign(contentMemoryStream, signedSecurityTxtStream);

        signedSecurityTxtStream.Position = 0;
        using StreamReader reader = new(signedSecurityTxtStream);
        signedContent = reader.ReadToEnd();
    }

    /// <summary>
    /// Generates PGP keys.
    /// </summary>
    /// <param name="publicKey">The public key output.</param>
    /// <param name="privateKey">The private key output.</param>
    /// <param name="email">The email associated with the keys.</param>
    /// <param name="passPhrase">The passphrase for the keys.</param>
    static void GenerateKeys(out string publicKey, out string privateKey, string email, string passPhrase) {
        using PGP pgp = new();

        using MemoryStream publicKeyStream = new();
        using MemoryStream privateKeyStream = new();
        pgp.GenerateKey(publicKeyStream, privateKeyStream, email, passPhrase);

        publicKeyStream.Position = 0;
        privateKeyStream.Position = 0;

        using StreamReader publicKeyReader = new(publicKeyStream);
        using StreamReader privateKeyReader = new(privateKeyStream);
        publicKey = publicKeyReader.ReadToEnd();
        privateKey = privateKeyReader.ReadToEnd();
    }
}