using Amazon;
using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Data;
using DivinitySoftworks.Functions.Identity.Repositories;
using Xunit;

namespace DivinitySoftworks.Functions.Identity.Tests.Repositories;
public class OAuthAuthorizationCodesRepositoryTests {
    readonly IAmazonDynamoDB _amazonDynamoDB;

    readonly OAuthAuthorizationCode oAuthAuthorizationCode = new() {
        Token = "ab5fb45c-efa1-4a6b-b873-30eaf8516a1a",
        TokenType = "authorization_code",
        Expiration = DateTime.UtcNow.AddMinutes(30).Ticks,
        UserId = "f6e3e8e0-35e0-4309-b0b8-f276420473e4"
    };

    public OAuthAuthorizationCodesRepositoryTests() {
        _amazonDynamoDB = new AmazonDynamoDBClient("AKIAZQ3DPZD5G4A5CW4O", "BviEGcimWUIRaVyT4lPgN/qnDpPok+Cw3VssJaCz", RegionEndpoint.EUWest3);
    }

    [Fact]
    public async void CreateAsync() {
        OAuthAuthorizationCodesRepository repository = new(_amazonDynamoDB);
        bool result = await repository.CreateAsync(oAuthAuthorizationCode);

        Assert.True(result);
    }

    [Fact]
    public async void ReadAsync() {
        OAuthAuthorizationCodesRepository repository = new(_amazonDynamoDB);
        OAuthAuthorizationCode? result = await repository.ReadAsync(oAuthAuthorizationCode.PK, oAuthAuthorizationCode.SK);

        Assert.NotNull(result);
        Assert.Equal(oAuthAuthorizationCode.PK, result.PK);
        Assert.Equal(oAuthAuthorizationCode.SK, result.SK);
        Assert.Equal(oAuthAuthorizationCode.Token, result.Token);
        Assert.Equal(oAuthAuthorizationCode.TokenType, result.TokenType);
        Assert.Equal(oAuthAuthorizationCode.Expiration, result.Expiration);
        Assert.Equal(oAuthAuthorizationCode.UserId, result.UserId);
    }

    [Fact]
    public async void ReadAllAsync() {

        OAuthAuthorizationCodesRepository repository = new(_amazonDynamoDB);
        List<OAuthAuthorizationCode> result = await repository.ReadExpiredCodesAsync();


        if (result.Count() == 0) { }

    }
}