using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace DivinitySoftworks.Functions.Identity.Contracts.Responses;

/// <summary>
/// Represents a SAML authentication response.
/// </summary>
public sealed record SamlResponse {
    readonly X509Certificate2 _certificate;
    readonly XmlDocument _xmlDocument;
    readonly XmlNamespaceManager _xmlNameSpaceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SamlResponse"/> class with the provided certificate string and response string.
    /// </summary>
    /// <param name="certificate">The string representation of the certificate.</param>
    /// <param name="response">The string representation of the SAML response.</param>
    public SamlResponse(string certificate, string response) : this(StringToByteArray(certificate), response) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SamlResponse"/> class with the provided certificate bytes and response string.
    /// </summary>
    /// <param name="certificate">The byte array representation of the certificate.</param>
    /// <param name="response">The string representation of the SAML response.</param>
    public SamlResponse(byte[] certificate, string response) {
        _certificate = new X509Certificate2(certificate);

        _xmlDocument = new() {
            PreserveWhitespace = true,
            XmlResolver = null
        };
        _xmlDocument.LoadXml(new UTF8Encoding().GetString(Convert.FromBase64String(response)));

        _xmlNameSpaceManager = GetNamespaceManager();
    }

    /// <summary>
    /// Gets the XML representation of the SAML response.
    /// </summary>
    public string Xml { get { return _xmlDocument.OuterXml; } }

    /// <summary>
    /// Checks if the SAML response is valid.
    /// </summary>
    /// <returns>True if the SAML response is valid; otherwise, false.</returns>
    public bool IsValid() {
        XmlNodeList? xmlNodeList = _xmlDocument.SelectNodes("//ds:Signature", _xmlNameSpaceManager);

        SignedXml signedXml = new(_xmlDocument);

        if (xmlNodeList is null || xmlNodeList.Count == 0) return false;

        signedXml.LoadXml((XmlElement)xmlNodeList[0]!);
        return ValidateSignatureReference(signedXml) && signedXml.CheckSignature(_certificate, true) && !IsExpired();
    }

    /// <summary>
    /// Gets the NameID from the SAML response.
    /// </summary>
    /// <returns>The NameID.</returns>
    public string GetNameID() {
        XmlNode? node = _xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:NameID", _xmlNameSpaceManager);
        return node?.InnerText ?? string.Empty;
    }

    /// <summary>
    /// Gets the user principal name (UPN) from the SAML response.
    /// </summary>
    /// <returns>The user principal name (UPN).</returns>
    public string? GetUpn() {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
    }

    /// <summary>
    /// Gets the object identifier from the SAML response.
    /// </summary>
    /// <returns>The object identifier.</returns>
    public string? GetObjectId() {
        return GetCustomAttribute("http://schemas.microsoft.com/identity/claims/objectidentifier");
    }

    /// <summary>
    /// Gets the email address from the SAML response.
    /// </summary>
    /// <returns>The email address.</returns>
    public string? GetEmail() {
        return GetCustomAttribute("User.email")
            ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
            ?? GetCustomAttribute("mail");
    }

    /// <summary>
    /// Gets the first name from the SAML response.
    /// </summary>
    /// <returns>The first name.</returns>
    public string? GetFirstname() {
        return GetCustomAttribute("first_name")
            ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
            ?? GetCustomAttribute("User.FirstName")
            ?? GetCustomAttribute("givenName");
    }

    /// <summary>
    /// Gets the last name from the SAML response.
    /// </summary>
    /// <returns>The last name.</returns>
    public string? GetLastname() {
        return GetCustomAttribute("last_name")
            ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
            ?? GetCustomAttribute("User.LastName")
            ?? GetCustomAttribute("sn");
    }

    /// <summary>
    /// Gets the department from the SAML response.
    /// </summary>
    /// <returns>The department.</returns>
    public string? GetDepartment() {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/department")
            ?? GetCustomAttribute("department");
    }

    /// <summary>
    /// Gets the phone number from the SAML response.
    /// </summary>
    /// <returns>The phone number.</returns>
    public string? GetPhone() {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone")
            ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/telephonenumber");
    }

    /// <summary>
    /// Gets the company from the SAML response.
    /// </summary>
    /// <returns>The company.</returns>
    public string? GetCompany() {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/companyname")
            ?? GetCustomAttribute("organization")
            ?? GetCustomAttribute("User.CompanyName");
    }

    /// <summary>
    /// Gets the location from the SAML response.
    /// </summary>
    /// <returns>The location.</returns>
    public string? GetLocation() {
        return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/location")
            ?? GetCustomAttribute("physicalDeliveryOfficeName");
    }

    // Retrieves a custom attribute from the SAML response.
    private string? GetCustomAttribute(string attribute) {
        return _xmlDocument.SelectSingleNode($"/samlp:Response/saml:Assertion[1]/saml:AttributeStatement/saml:Attribute[@Name='{attribute}']/saml:AttributeValue", _xmlNameSpaceManager)?.InnerText;            
    }

    // Creates and returns a namespace manager for the XML document.
    private XmlNamespaceManager GetNamespaceManager() {
        XmlNamespaceManager xmlNamespaceManager = new(_xmlDocument.NameTable);
        xmlNamespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        return xmlNamespaceManager;
    }

    // Converts a string to a byte array.
    private static byte[] StringToByteArray(string value) {
        byte[] bytes = new byte[value.Length];
        for (int i = 0; i < value.Length; i++)
            bytes[i] = (byte)value[i];
        return bytes;
    }

    // Validates the reference of the signature in the SAML response.
    private bool ValidateSignatureReference(SignedXml signedXml) {
        if (signedXml.SignedInfo is null || signedXml.SignedInfo.References.Count != 1)
            return false;

        Reference? reference = signedXml.SignedInfo.References[0] as Reference;
        string? id = reference?.Uri?[1..];

        if (id is null) 
            return false;

        XmlElement? xmlElement = signedXml.GetIdElement(_xmlDocument, id);

        if (xmlElement is null) return false; 

        if (xmlElement == _xmlDocument.DocumentElement)
            return true;
        else {
            if (_xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion", _xmlNameSpaceManager) is XmlElement assertionNode && assertionNode != xmlElement)
                return false;
        }

        return true;
    }

    // Checks if the SAML response is expired.
    private bool IsExpired() {
        DateTime expiration = DateTime.MaxValue;
        XmlNode? xmlNode = _xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", _xmlNameSpaceManager);
        if (xmlNode is not null && xmlNode.Attributes?["NotOnOrAfter"] is not null)
            _ = DateTime.TryParse(xmlNode.Attributes?["NotOnOrAfter"]!.Value ?? string.Empty, out expiration);
        return DateTime.UtcNow > expiration.ToUniversalTime();
    }
}
