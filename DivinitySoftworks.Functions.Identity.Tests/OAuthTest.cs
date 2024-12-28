using Amazon;
using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Repositories;
using DivinitySoftworks.Functions.Identity.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;


namespace DivinitySoftworks.Functions.Identity.Tests;

public sealed class OAuthTest {
    IConfiguration _configuration;
    IOptions<DatabaseSettings> _databaseSettings;
    IAmazonDynamoDB _amazonDynamoDB;
    ICertificatesRepository _certificatesRepository;

    public OAuthTest() {
        var builder = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", true);
        _configuration = builder.Build();

        _amazonDynamoDB = new AmazonDynamoDBClient("AKIAZQ3DPZD5G4A5CW4O", "BviEGcimWUIRaVyT4lPgN/qnDpPok+Cw3VssJaCz", RegionEndpoint.EUWest3);

        _certificatesRepository = new CertificatesRepository(_amazonDynamoDB);
    }

    [Fact]
    public void ConfigureServices_ThrowsException_WhenDatabaseSettingsMissing() {

        var services = new ServiceCollection();
        var startup = new Startup();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => startup.ConfigureServices(services));
        Assert.Equal("Database settings are missing.", ex.Message);
    }
}

