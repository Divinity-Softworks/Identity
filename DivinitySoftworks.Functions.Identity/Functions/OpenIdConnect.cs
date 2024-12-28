using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DivinitySoftworks.AWS.Core.Web.Functions;
using DivinitySoftworks.Core.Web.Security;
using DivinitySoftworks.Functions.Identity.Security.OpenIdConnect;

using static Amazon.Lambda.Annotations.APIGateway.HttpResults;

namespace DS.Functions.Identity;

/// <summary>
/// Represents endpoints for OpenID Connect related operations.
/// </summary>
/// <param name="authorizeService">The service used to handle authorization.</param>
public sealed class OpenIdConnect([FromServices] IAuthorizeService authorizeService) : ExecutableFunction(authorizeService) {
    const string RootBase = "/.well-known/openid-configuration";
    const string RootResourceName = "DSOpenId";

    /// <summary>
    /// Retrieves the OpenID Connect configuration.
    /// </summary>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="request">The API Gateway request.</param>
    /// <param name="openIdConnectService">An instance of the OpenID Connect service.</param>
    /// <returns>An HTTP result containing the OpenID Connect configuration.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetConfiguration)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}")]
    public async Task<IHttpResult> GetConfiguration(ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request
        , [FromServices] IOpenIdConnectService openIdConnectService) {

        return await ExecuteAsync(Authorize.AllowAnonymous, context, request,
           () => {
               return Task.FromResult(Ok(openIdConnectService.OpenIdConnectConfiguration));
           });
    }

    /// <summary>
    /// Retrieves the JSON Web Key Set (JWKS) asynchronously.
    /// </summary>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="request">The API Gateway request.</param>
    /// <param name="openIdConnectService">An instance of the OpenID Connect service.</param>
    /// <returns>An asynchronous task that represents the HTTP result containing the JWKS.</returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetJWSKAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}/jwks")]
    public async Task<IHttpResult> GetJWSKAsync(ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request
        , [FromServices] IOpenIdConnectService openIdConnectService) {

        return await ExecuteAsync(Authorize.AllowAnonymous, context, request,
           async () => {
               JWKS jwks = await openIdConnectService.GetJWKSAsync();

               return Ok(jwks);
           });
    }
}
