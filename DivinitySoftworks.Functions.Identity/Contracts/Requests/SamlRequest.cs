using System.IO.Compression;
using System.Text;
using System.Xml;

namespace DivinitySoftworks.Functions.Identity.Contracts.Requests;

/// <summary>
/// Represents a SAML authentication request.
/// </summary>
public sealed record SamlRequest {
    readonly string _id;
    readonly string _issueInstant;
    readonly string _issuer;
    readonly string _assertionConsumerServiceUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="SamlRequest"/> class with the specified issuer and assertion consumer service URL.
    /// </summary>
    /// <param name="issuer">The issuer of the SAML request.</param>
    /// <param name="assertionConsumerServiceUrl">The URL to which the SAML response should be sent.</param>
    public SamlRequest(string issuer, string assertionConsumerServiceUrl) {
        _id = $"_{Guid.NewGuid()}";
        _issueInstant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
        _issuer = issuer;
        _assertionConsumerServiceUrl = assertionConsumerServiceUrl;
    }

    /// <summary>
    /// Generates the SAML authentication request.
    /// </summary>
    /// <param name="format">The format of the authentication request.</param>
    /// <returns>The SAML authentication request in the specified format.</returns>
    public string? GetRequest(AuthRequestFormat format) {
        using StringWriter stringWriter = new();

        using XmlWriter xmlWriter = XmlWriter.Create(stringWriter
            , new XmlWriterSettings() {
                OmitXmlDeclaration = true
            });

        xmlWriter.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
        xmlWriter.WriteAttributeString("ID", _id);
        xmlWriter.WriteAttributeString("Version", "2.0");
        xmlWriter.WriteAttributeString("IssueInstant", _issueInstant);
        xmlWriter.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
        xmlWriter.WriteAttributeString("AssertionConsumerServiceURL", _assertionConsumerServiceUrl);

        xmlWriter.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
        xmlWriter.WriteString(_issuer);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
        xmlWriter.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
        xmlWriter.WriteAttributeString("AllowCreate", "true");
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
        xmlWriter.Flush();

        if (format != AuthRequestFormat.Base64)
            return null;

        using MemoryStream memoryStream = new();
        using StreamWriter streamWriter = new(new DeflateStream(memoryStream, CompressionMode.Compress, true), new UTF8Encoding(false));
        streamWriter.Write(stringWriter.ToString());
        streamWriter.Close();
        return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, Base64FormattingOptions.None);
    }

    /// <summary>
    /// Generates the redirect URL for the SAML authentication request.
    /// </summary>
    /// <param name="samlEndpoint">The SAML endpoint URL.</param>
    /// <param name="relayState">Optional relay state for the SAML request.</param>
    /// <returns>The redirect URL for the SAML authentication request.</returns>
    public string GetRedirectUrl(string samlEndpoint, string? relayState = null) {
        string queryStringSeparator = samlEndpoint.Contains('?') ? "&" : "?";

        string url = $"{samlEndpoint}{queryStringSeparator}SAMLRequest={Uri.EscapeDataString(GetRequest(AuthRequestFormat.Base64) ?? string.Empty)}";

        if (!string.IsNullOrEmpty(relayState))
            url += $"&RelayState={Uri.EscapeDataString(relayState)}";

        return url;
    }
}

/// <summary>
/// Enumerates the possible formats for the SAML authentication request.
/// </summary>
public enum AuthRequestFormat {
    /// <summary>
    /// Represents the Base64 format.
    /// </summary>
    Base64 = 1
}