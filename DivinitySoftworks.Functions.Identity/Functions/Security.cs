using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService.Model;
using DivinitySoftworks.AWS.Core.Net.Storage;
using DivinitySoftworks.AWS.Core.Web.ContentDeliveryNetwork;
using DivinitySoftworks.AWS.Core.Web.Functions;
using DivinitySoftworks.Core.Net.EventBus;
using DivinitySoftworks.Core.Net.Mail;
using DivinitySoftworks.Core.Web.Security;
using DivinitySoftworks.Functions.Identity.Security;
using DivinitySoftworks.Functions.Identity.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace DS.Functions.Identity;

/// <summary>
/// Represents a Lambda function for handling security-related operations,
/// including generating and storing security files and retrieving the security.txt file.
/// </summary>
/// <param name="authorizeService">The authorization service for function execution control.</param>
public sealed class Security([FromServices] IAuthorizeService authorizeService) : ExecutableFunction(authorizeService) {
    const string RootBase = "/.well-known";
    const string RootResourceName = "DSSecurity";
    const string TopicArn = "arn:aws:sns:eu-west-3:654654294266:sns-notification-email";
    const string SenderAddress = "Identity @ Divinity Softworks <identity@divinity-softworks.com>";
    const string Subject = "New security files uploaded.";

    /// <summary>
    /// Generates security files, stores them in the appropriate location,
    /// and notifies configured recipients via email.
    /// </summary>
    /// <param name="context">The Lambda execution context.</param>
    /// <param name="configuration">The configuration service for accessing settings.</param>
    /// <param name="publisher">The event publisher for dispatching notifications.</param>
    /// <param name="securityService">The security service responsible for generating security files.</param>
    /// <param name="storageService">The storage service for persisting security files.</param>
    /// <exception cref="Exception">
    /// Thrown if file generation fails, the signed content or public key is null or empty,
    /// or file storage fails.
    /// </exception>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(CreateAsync)}")]
    public async Task CreateAsync(ILambdaContext context
        , [FromServices] IConfiguration configuration
        , [FromServices] IPublisher publisher
        , [FromServices] ISecurityService securityService
        , [FromServices] IStorageService storageService
        , [FromServices] IContentDeliveryNetworkService contentDeliveryNetworkService) {

        try {
            SecurityFileResult securityFileResult = securityService.CreateSecurityFiles();

            if (securityFileResult.Exception is not null)
                throw securityFileResult.Exception;

            if (string.IsNullOrWhiteSpace(securityFileResult.SignedContent))
                throw new Exception("The generated signed content is either null, or empty.");

            if (string.IsNullOrWhiteSpace(securityFileResult.PublicKey))
                throw new Exception("The generated public key is either null, or empty.");

            if (!await storageService.StoreFileAsync(".well-known/security.txt", securityFileResult.SignedContent, context))
                throw new Exception("Storing '.well-known/security.txt' has failed!");

            string filePath = securityService.Settings.Encryption[(securityService.Settings.Encryption.IndexOf("/.well-known") + 1)..];

            if (!await storageService.StoreFileAsync(filePath, securityFileResult.PublicKey, context))
                throw new Exception($"Storing '{filePath}' has failed!");

            foreach (string email in securityService.Settings.Notify) {
                EmailTemplateMessage emailMessage = new(new(SenderAddress), "identity-new-security") {
                    To = [new MailAddress(email).ToString()],
                    Subject = Subject
                };

                await publisher.PublishAsync<EmailTemplateMessage, PublishResponse>(TopicArn, emailMessage);
            }

            await contentDeliveryNetworkService.CreateInvalidationAsync(["/*"], context);
        }
        catch (Exception exception) {
            context.Logger.LogError(exception.Message);
            EmailTemplateMessage? emailTemplateMessage = EmailTemplateMessage.FromException(
                configuration,
                exception,
                (ex) => context.Logger.LogError(ex.Message));

            if (emailTemplateMessage is not null)
                await publisher.PublishAsync<EmailTemplateMessage, PublishResponse>(TopicArn, emailTemplateMessage);
        }
    }

    /// <summary>
    /// Retrieves the security.txt file from storage and returns its content.
    /// </summary>
    /// <param name="context">The Lambda execution context.</param>
    /// <param name="storageService">The storage service for retrieving the security.txt file.</param>
    /// <returns>
    /// An <see cref="APIGatewayHttpApiV2ProxyResponse"/> containing the file content if found,
    /// or a 404 Not Found response if the file is missing.
    /// </returns>
    [LambdaFunction(ResourceName = $"{RootResourceName}{nameof(GetSecurityTextAsync)}")]
    [HttpApi(LambdaHttpMethod.Get, $"{RootBase}/security.txt")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetSecurityTextAsync(ILambdaContext context
        , [FromServices] IStorageService storageService) {

        APIGatewayHttpApiV2ProxyResponse response = new() {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string> {
                { "Content-Type", "text/plain" }
            }
        };

        try {
            response.Body = await storageService.LoadFileAsync(".well-known/security.txt", context);

            if (string.IsNullOrWhiteSpace(response.Body))
                response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        catch (Exception exception) {
            context.Logger.LogError(exception.Message);

            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        return response;
    }
}
