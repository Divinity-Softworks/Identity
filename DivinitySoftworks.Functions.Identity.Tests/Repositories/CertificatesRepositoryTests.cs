using Amazon;
using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Repositories;
using Xunit;

namespace DivinitySoftworks.Functions.Identity.Tests.Repositories;
public class CertificatesRepositoryTests {
    readonly IAmazonDynamoDB _amazonDynamoDB;

    readonly Certificate _certificate = new() {
        Thumbprint = "BD050C52727818E718C1AADF7A29CBAEB9C0F7C9",
        Data = @"-----BEGIN CERTIFICATE-----
MIIDWjCCAkKgAwIBAgIQNypfDXZiVqJIotRwwceHXjANBgkqhkiG9w0BAQsFADAq
MSgwJgYDVQQDDB9pZGVudGl0eS5kaXZpbml0eS1zb2Z0d29ya3MuY29tMB4XDTI0
MDUyMDA3NTgyOFoXDTI1MDUyMDA4MDgyOFowKjEoMCYGA1UEAwwfaWRlbnRpdHku
ZGl2aW5pdHktc29mdHdvcmtzLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCC
AQoCggEBANDQlUtabdxMkqR+tf9tnGfrw706TBCnhwP0v46hVw2aBKcPwAXVlmqq
l1OkOLcX2uoqeW/RqIsD0vBRxh7634vsTI+qNf4ak+ps+afd/GhiEnHXOTvQ0r0a
KhxNdu6BSyUDNnPe4KQ8SpzNLhuarPJA2jzSAs5NmVAfr7zf4Dvcd1h2yvmsLTVH
H4S0RvNqbmNktbgWPjPdYpwL0LpuYtuXJ5zlWQZEg/syoNLHf/Ri0i2bRQndJ8ed
VKeYmgc5seAofnKSr5P50daP5I6qeWiCneMuGTmOgWJWx37Lur72rveqx4wot6cO
KX5NNLHmy4UI9Ev82vKTEc3lzf4n+7kCAwEAAaN8MHowDgYDVR0PAQH/BAQDAgWg
MB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATAqBgNVHREEIzAhgh9pZGVu
dGl0eS5kaXZpbml0eS1zb2Z0d29ya3MuY29tMB0GA1UdDgQWBBQJTcSM0mv9Hfzg
G37ZUSr4A2fhdTANBgkqhkiG9w0BAQsFAAOCAQEAQlGq1n7FRC1BpoG8JpiJ3S01
H1k4GcpHvcGZ2ogpkvj46R9UwpTAyYBr763fQkf/jnaEHHp5R2pBlURi34AjIj1D
60z4TgfwfBuIR/KalYMKPULoBXMREB8wA9gRN+YRArDIYAaf72qNsR0p+c4gAHv7
syjhiLnktxLmdmujPwwozwfkHH3Go72q7QyRfxh6/7JYrZTy/qhwPu5530K9Lm95
SLX7XYoZvo3Ygziz7LQ1b1H5wXoiAgoeQS95txmFQQoPsA++UNY11GqX7At2Y8XB
kAGsX08lN25n+UHG3qz5DKEYyIaZPJaphmrUIJTFR4yBjusadtwZ5ohQ9TeZbg==
-----END CERTIFICATE-----
-----BEGIN PRIVATE KEY-----
MIIEzQIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDQ0JVLWm3cTJKk
frX/bZxn68O9OkwQp4cD9L+OoVcNmgSnD8AF1ZZqqpdTpDi3F9rqKnlv0aiLA9Lw
UcYe+t+L7EyPqjX+GpPqbPmn3fxoYhJx1zk70NK9GiocTXbugUslAzZz3uCkPEqc
zS4bmqzyQNo80gLOTZlQH6+83+A73HdYdsr5rC01Rx+EtEbzam5jZLW4Fj4z3WKc
C9C6bmLblyec5VkGRIP7MqDSx3/0YtItm0UJ3SfHnVSnmJoHObHgKH5ykq+T+dHW
j+SOqnlogp3jLhk5joFiVsd+y7q+9q73qseMKLenDil+TTSx5suFCPRL/NrykxHN
5c3+J/u5AgMBAAECggEBAKIrK5b+3s01RrmrkoqRxONMTZLbAJH6mTewT0hk8qek
krJv3/BoBU5HuAiERGulgiVT4UW/LMbxrFNpHpWWvh/73yW/yfjTCUcJZQZCtzEV
4T86J7/VmKS1+jhKzO/Dx+kxyotIb2v9SvDlxEY55NCrHxIXA4jW6Rc51JC5yXqS
gWZXiCtOsTsMzH+/ZiKC40eekU0t8J+kAf26n/bhMhV8F/lGOLXX0ZfQInTvlcmm
qtULZbH7TJ4tDfSxqbBcI6BLKODhuyQXuIqQMekgoHcVYIH3Xm5vwerZwAu4CTZm
3Rzjm+nELnaRmVfkds+VGSKxsjWxQijpQxXWaVd7tCUCgYEA7xWAEuamU2hsfYGs
/0/V4FQs6lvgUZttc536I4SSV8HGIR9Ofq8emPYDINoRercy/vIAGIx3WQNEd+Hv
yCVzPiJUBeGs9nxm6QxQqFsT1zyhEi2eCCYDenjPx6GuXevpJSc+ItQPP1iM+8HN
OP0zlyKI+ezXCu6naypnHO20mrsCgYEA35bSAl1g6sHA0T0T1bCpoCtI6Tu2TMbZ
R2HBg4Qgfl6eJukdWAiw8Ec5A80kmew4aydBKt0CL2Q0bRUXaEq8usoxt4iPWgzu
e6xANWO8VB73I/0bVXojqJNt8lyJ0ycKHtuAV0tHr/PEbp4qmClZzpafzUSO2ZN3
yQbTQuCxXhsCgYAKWm3/Y4mUVxNLpA/vxm9GlZ6UT4I5RnfI5/BrFVSZBEUmLgri
JYc3rH+aLpdPLxvAOo6SgZKxghi28EvH6QdCi44D6y/oRJ7YPLZJdBg52+BFvz/2
rl68MPsec/vvCUZBW5+vNmuqnJUOWegfLafMRayU1hLB4G/TjjSE5i5J/QKBgQCv
3RU6PvBoNhXPflcHkXkaPnGO4iS1pwJoHv/yzg4w14NV363w/IUPuTWjQ8wyNZb6
0Vl0uXlqhUqnMhvDMGXaFgec8JCwp9M8+3NViykqkWCasg654OQDSFMGerr9lotj
UgtUniW4w+gRid6+6Gd3EwqGjhI+GLvzJqcsx0c7WQKBgBNtj47dlCBMwO3xS1sD
SG8G6Hx6Nt1v4CUeBZq2urDeQLD8OsZLWv0jpEAJPo/cyvtnCOE/LFUme35q5WQq
F768cdlX/8ch8ogdL7uLtWVb/rzKVyJloRG6HRVtL+NoF48Ju6xUnydRXASK8kp+
Sr8U0vP6uB8FOl7AykkWeYkXoA0wCwYDVR0PMQQDAgCQ
-----END PRIVATE KEY-----",
        Expiration = 638833325080000000
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
