using Amazon;
using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Repositories;
using Xunit;

namespace DivinitySoftworks.Functions.Identity.Tests.Repositories;
public class CertificatesRepositoryTests {
    readonly IAmazonDynamoDB _amazonDynamoDB;

    readonly Certificate _certificate = new() {
        Thumbprint = "F2E75EA835FDCE9EC2D6A6CFAA47AFB321DD1CCB",
        Data = @"-----BEGIN CERTIFICATE-----
MIIDWjCCAkKgAwIBAgIQMseVdvPOYZZF3or+wqO4hzANBgkqhkiG9w0BAQsFADAq
MSgwJgYDVQQDDB9pZGVudGl0eS5kaXZpbml0eS1zb2Z0d29ya3MuY29tMB4XDTI1
MDUyNjA3MTMxM1oXDTI2MDUyNjA3MjMxM1owKjEoMCYGA1UEAwwfaWRlbnRpdHku
ZGl2aW5pdHktc29mdHdvcmtzLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBAMfVLBPsfIWbYqjtKvez0uoA/QLoY6hmFH/dOjgwj0Jelh8G2DC4BaZr
hrIIfJDWXcAaosITY4snHNX3b97MRFUyYFPA+zv2vq83jEQUISA8RQpNJvg5mpp1
H7rp98iRhz5waPE0+OKonumbg1baWNN0mE1BTOzs2u7tBliNQsW04dYw0R8ijyva
a0Xx5/T82f57R+k7NPNkeuDNOaFIS+Td2g5G2VFP+ZCuFHHKy1aWHB4BJXEL5Csa
SSS5+6MkMjcrrx7GOcc3RHE18ksSvpKzE2aBuJ+CP3+GRXL0Z9U3tR2DcuCQElDR
6GmXt822KMipCdYOSnaan2uK0AsrZG0CAwEAAaN8MHowDgYDVR0PAQH/BAQDAgWg
MB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATAqBgNVHREEIzAhgh9pZGVu
dGl0eS5kaXZpbml0eS1zb2Z0d29ya3MuY29tMB0GA1UdDgQWBBTaT8f2ZfWfVs9G
vr01UiWGI4yMCzANBgkqhkiG9w0BAQsFAAOCAQEAX90ZXTnKdOL9FP8x8j0RQOr9
ehRL+nURvdXo5G8krcnymDEmBklfxRUidQUxhBluNeN2bgNERf9f5oitM+HNr4Vt
4W/e2b8/Y8E8R1iIHnXH2wQf5DsCv8R3khsK1ggjq2Lq77on7DWLJDJVnxxrR63C
I4tLayq8e4nChROt3QGodq6dBUEmzyxbN42/4Z1bd2etyi7r7eqtsiynEssRJhVV
UIuqyfRY8y5jWtY0exipHhSXxP/8KBeUGYOh5i9zgIoI+OCy+JE1ShQUUyI2bkmb
p2NPCfMuHveGwSPYhy6HCYp1pZZ1/9VMRlujZ6479O0yXB4VbIfBCzVUnj9iCw==
-----END CERTIFICATE-----
-----BEGIN PRIVATE KEY-----
MIIEzAIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDH1SwT7HyFm2Ko
7Sr3s9LqAP0C6GOoZhR/3To4MI9CXpYfBtgwuAWma4ayCHyQ1l3AGqLCE2OLJxzV
92/ezERVMmBTwPs79r6vN4xEFCEgPEUKTSb4OZqadR+66ffIkYc+cGjxNPjiqJ7p
m4NW2ljTdJhNQUzs7Nru7QZYjULFtOHWMNEfIo8r2mtF8ef0/Nn+e0fpOzTzZHrg
zTmhSEvk3doORtlRT/mQrhRxystWlhweASVxC+QrGkkkufujJDI3K68exjnHN0Rx
NfJLEr6SsxNmgbifgj9/hkVy9GfVN7Udg3LgkBJQ0ehpl7fNtijIqQnWDkp2mp9r
itALK2RtAgMBAAECggEAB6NdSPGsHMtArSZLkyY7dJhPE3JghMpKFE0XcZhhkW7e
HBd94fbWuHK5tkpewIaBFNuvxu1Iy8PV9VO84e462mrPhcQFllgrF7Vu1hdS4NGM
7gTe7XP+NdMuQE27G958JeXUDQwySy+Yy2MXVRWJeaOmQqWxVAkFoUZMDMxxde8P
prT98nDnfupfcfSL9duBDHK+aLVVsBk3qq8uyTKRTtQI+g+GXqWNUl/KEy4/C5+g
sarQ4U0lSUhi+LqGkAtl5C2jH8Z/KCCNoQ/4/uMJuB6qC+w22wkdpcZwVgXIP9OH
zBJMfkMyASbUNY2dDzERgEYABlk6pP0kee2aSu1BHQKBgQDM4FXzq80jLtpT6FUP
CruDlKeQXeaAhv1qMUzM0F5OlgBtRljcbuM/ErHnJ3AM/uaLSMKYnLtLuqr7bdyq
/s4mZ+yihYQKyXdrla9e0v9rZ9H/qxanGF8OjmyxFqjpMquZGi3zkd3vOu5ECEbV
/MGWS0H8VoE1fYLTDO0FK+nyGwKBgQD5sqWN5ej/reoVFXaigXUO4Z8Zz2taCcsj
f+TV1ZQyGk0lB6X/hiZrzx31MCjZVeEzSzpoBuYmWFmcsyJ4vx2J3CvON5kosYqG
oRxJZD8ArfT8/BTWiRUEs8YTCTKiIweZpvB4ZycFg/iOKiX/4bPPxbSw35bKjWaL
KV1I0z4sFwKBgQCcaxq0c5KnLfpa3aMzXMpVZ4WXX1nA/08zaHhuVxfpf/TLeU4W
3kJ5wVg6V43hHiv+Y4rO4brN0rMAS5ySoP/bqrSsUKvDYJXgeePuzcFcy2M1g/ZD
lQPeJTcu7VIA2ULSHX6/27pKEAAhyP/sGJHkoYi7k4AKysy7Pb8ol6KByQKBgBcZ
puVEwtR8k25V6P7JuTmiN+TcYpMW2tsy5sm9k5M7Ca4GUh4cnAtXQu8/AFhy8H2d
VQ7wrHQZslij3emLzMDHRKo0TJAONMGVwBcuFgILFeIdtBPQ5MfllcqHDE8hrH9T
iWWqLVr+RY1patCMYeUz5i6C0OF2SWlQb1fgax0LAoGAWkfJ+AwWsHPkLHmvlXyT
TnGAag0Hl1bsLrFYvOJoi32rtImKduJpv+y8sYyaa4p0ePd3+/i3CY5QHcMdkrsm
jXg+YcXhqxm+5BgvH8AbVwDRPyJbDyUg58+uC5EEGMXCtmGWP3dKSn1fQm6rTHvy
anscBk26MJj7959OxqEIORSgDTALBgNVHQ8xBAMCAJA=
-----END PRIVATE KEY-----",
        Expiration = 1779780193
    };

    public CertificatesRepositoryTests() {
        _amazonDynamoDB = new AmazonDynamoDBClient("AKIAZQ3DPZD5G4A5CW4O", "BviEGcimWUIRaVyT4lPgN/qnDpPok+Cw3VssJaCz", RegionEndpoint.EUWest3);
    }

    [Fact]
    public async void CreateAsync() {
        CertificatesRepository repository = new(_amazonDynamoDB);
        bool result = await repository.CreateAsync(_certificate);

        Assert.True(result);
    }

    [Fact]
    public async void ReadAsync() {
        CertificatesRepository repository = new(_amazonDynamoDB);
        Certificate? result = await repository.ReadAsync(_certificate.PK);

        Assert.NotNull(result);
        Assert.Equal(_certificate.PK, result.PK);
        Assert.Equal(_certificate.Thumbprint, result.Thumbprint);
        Assert.Equal(_certificate.Data, result.Data);
        Assert.Equal(_certificate.Expiration, result.Expiration);
    }

    [Fact]
    public async void DeleteAsync() {
        CertificatesRepository repository = new(_amazonDynamoDB);
        bool result = await repository.DeleteAsync(_certificate.PK);

        Assert.True(result);
    }
}
