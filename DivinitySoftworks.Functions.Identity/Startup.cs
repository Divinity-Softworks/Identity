using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using DivinitySoftworks.Core.Web.Security;
using DivinitySoftworks.Functions.Identity.Repositories;
using DivinitySoftworks.Functions.Identity.Security;
using DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;
using DivinitySoftworks.Functions.Identity.Services;
using DivinitySoftworks.Functions.Identity.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DivinitySoftworks.Functions.Identity;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup {

    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services) {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true);

        IConfigurationRoot configuration = builder.Build();
        services.AddSingleton<IConfiguration>(configuration);

        DatabaseSettings? databaseSettings = configuration.GetSection(DatabaseSettings.KeyName).Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("Database settings are missing.");

        if (databaseSettings.HasCredentials)
            services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(databaseSettings.Credentials.Key, databaseSettings.Credentials.Secret, RegionEndpoint.EUWest3));
        else
            services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.EUWest3));

        services.AddSingleton(configuration.GetSection(SecuritySettings.KeyName).Get<SecuritySettings>()
            ?? throw new InvalidOperationException("Security settings are missing."));

        services.AddCloudFront(configuration);
        services.AddS3Bucket(configuration);
        services.AddSimpleNotificationService(configuration);


        services.AddSingleton<ICertificatesRepository, CertificatesRepository>();
        services.AddSingleton<IOAuthAuthorizationCodesRepository, OAuthAuthorizationCodesRepository>();
        services.AddSingleton<IOAuthClientsRepository, OAuthClientsRepository>();
        services.AddSingleton<IUsersRepository, UsersRepository>();

        services.AddSingleton(configuration.GetSection("Authentication:OpenIdConnect").Get<OpenIdConnectConfiguration>() 
            ?? throw new InvalidOperationException("OpenIdConnect configuration is missing."));

        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddSingleton<IOpenIdConnectService, OpenIdConnectService>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.Configure<AuthorizationOptions>(configuration.GetSection(AuthorizationOptions.Authorization));
        services.AddOpenIdConnect();
    }
}