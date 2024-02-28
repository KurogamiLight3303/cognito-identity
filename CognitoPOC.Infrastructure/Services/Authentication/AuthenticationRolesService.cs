using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationRolesService(
    IAmazonCognitoIdentityProvider identityProvider,
    ILogger logger,
    IOptions<CognitoConfiguration> configuration)
    : IAuthenticationRolesService
{
    private readonly CognitoConfiguration _configuration = configuration.Value;

    public async ValueTask<OperationResultValue> UpdateRoles(UserAccountObject user, string[] roles, CancellationToken cancellationToken)
    {
        OperationResultValue result;
        try
        {
            var groups = await GetRoles(user, cancellationToken);
            foreach (var r in groups.Where(p => !roles.Contains(p)))
            {
                await identityProvider.AdminRemoveUserFromGroupAsync(new()
                {
                    Username = user.Username,
                    UserPoolId = _configuration.CognitoUserPoolId,
                    GroupName = r
                }, cancellationToken);
            }
            foreach (var r in roles.Where(p => !groups.Contains(p)))
            {
                await identityProvider.AdminAddUserToGroupAsync(new()
                {
                    Username = user.Username,
                    UserPoolId = _configuration.CognitoUserPoolId,
                    GroupName = r
                }, cancellationToken);
            }

            result = new(true, I18n.RolesUpdated);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to unsubscribe user {Username}", user.Username);
            result = new(false, I18n.UnknownError);
        }

        return result;
    }

    public async ValueTask<string[]> GetRoles(UserAccountObject user, CancellationToken cancellationToken)
    {
        string[] result;
        try
        {
            var u = await identityProvider.AdminListGroupsForUserAsync(new ()
            {
                Username = user.Username,
                UserPoolId = _configuration.CognitoUserPoolId
            }, cancellationToken);
            result = u.Groups.Select(p => p.GroupName).ToArray();

        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to unsubscribe user {Username}", user.Username);
            result = Array.Empty<string>();
        }

        return result;
    }
}