using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Logging;
using CognitoPOC.Domain.Common;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationTotpService(
    ISessionMetadataService metadataService,
    IAmazonCognitoIdentityProvider identityProvider,
    ILogger logger,
    IUnitOfWork unitOfWork,
    IUserAccountCommandRepository repository)
    : IAuthenticationTotpService
{
    private const string AppFriendlyName = "Cubatel ({0})";

    public async ValueTask<OperationResultValue<SoftwareTokenAssociationValue>> AssociateSoftwareToken(CancellationToken cancellationToken)
    {
        OperationResultValue<SoftwareTokenAssociationValue> answer;
        try
        {
            var response = await identityProvider.AssociateSoftwareTokenAsync(new()
            {
                AccessToken = await metadataService.GetAccessToken(cancellationToken)
            }, cancellationToken);
            answer = response.HttpStatusCode == System.Net.HttpStatusCode.OK
                ? new(true,
                    new SoftwareTokenAssociationValue(response.SecretCode))
                : new(false, I18n.UnableToStartTOTPVerification);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to start software token association");
            answer = new(false, I18n.InvalidCredentials);
        }
        return answer;
    }
    public async ValueTask<OperationResultValue> VerifySoftwareToken(string? code, CancellationToken cancellationToken)
    {
        OperationResultValue answer;
        try
        {
            var token = await metadataService.GetAccessToken(cancellationToken);
            var username = metadataService.GetCurrentUsername();
            await identityProvider.VerifySoftwareTokenAsync(new()
            {
                AccessToken = token,
                UserCode = code,
                FriendlyDeviceName = string.Format(AppFriendlyName, username)
            }, cancellationToken);
            var result = await unitOfWork.ExecuteInTransactionAsync(async (cc)
                =>
            {
                var user = await repository.FindByAliasAsync(username, cc);
                if (user == null)
                    return false;
                user.SoftwareTokenLinked = true;
                repository.Update(user);
                return await unitOfWork.CommitAsync(cc);
            }, cancellationToken);
            if(!result)
                logger.LogError("Unable to update user ({Username} after verify TOTP)", username);
            answer = new(true, I18n.SoftwareTokenVerified);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to verify software token association");
            answer = new(false, I18n.UnableToVerifyTOTP);
        }
        return answer;
    }
}