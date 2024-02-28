using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationSignUpService(
    IAmazonCognitoIdentityProvider identityProvider,
    ILogger logger,
    IOptions<CognitoConfiguration> configuration)
    : IAuthenticationSignUpService
{
    private readonly CognitoConfiguration _configuration = configuration.Value;

    public async ValueTask<OperationResultValue> Subscribe(UserAccountObject user, string password, CancellationToken cancellationToken)
    {
        try
        {
            await identityProvider.AdminCreateUserAsync(new AdminCreateUserRequest()
            {
                Username = user.Username,
                MessageAction = MessageActionType.SUPPRESS,
                UserPoolId = _configuration.CognitoUserPoolId,
                UserAttributes = new()
                {
                    new AttributeType()
                    {
                        Name = "email",
                        Value = user.Email.Value
                    },
                    new AttributeType()
                    {
                        Name = "email_verified",
                        Value = user.Email.Verified.ToString()
                    },
                    new AttributeType()
                    {
                        Name = "phone_number",
                        Value = user.PhoneNumber.Value
                    },
                    new AttributeType()
                    {
                        Name = "phone_number_verified",
                        Value = user.PhoneNumber.Verified.ToString()
                    },
                    new AttributeType()
                    {
                        Name = "given_name",
                        Value = user.FirstName
                    },
                    new AttributeType()
                    {
                        Name = "family_name",
                        Value = user.LastName
                    },
                }
            }, cancellationToken);

            await identityProvider.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest()
            {
                Username = user.Username,
                Password = password,
                Permanent = true,
                UserPoolId = _configuration.CognitoUserPoolId
            }, cancellationToken);
            return new(true);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to Subscribe user {Username}", user.Username);
            return new(false, I18n.UnknownError);
        }
    }

    public async Task UnSubscribe(UserAccountObject user, CancellationToken cancellationToken)
    {
        try
        {
            await identityProvider.AdminDeleteUserAsync(new AdminDeleteUserRequest()
            {
                Username = user.Username,
                UserPoolId = _configuration.CognitoUserPoolId
            }, cancellationToken);

        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to unsubscribe user {Username}", user.Username);
        }
    }
}