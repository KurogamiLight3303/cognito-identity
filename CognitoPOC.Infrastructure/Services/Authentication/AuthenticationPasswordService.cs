using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Models.UserAccounts.Requests;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationPasswordService(
    ISessionMetadataService metadataService,
    IAmazonCognitoIdentityProvider identityProvider,
    IOptions<CognitoConfiguration> configuration,
    IUserAccountCommandRepository repository,
    ILogger logger)
    : IAuthenticationPasswordService
{
    private readonly CognitoConfiguration _configuration = configuration.Value;
    //private readonly IAccountLogsRepository _accountLogsRepository;
    //_accountLogsRepository = accountLogsRepository;

    public async ValueTask<OperationResultValue> ChangePasswordAsync(IPasswordChange password, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        try
        {
            await identityProvider.ChangePasswordAsync(new()
            {
                PreviousPassword = password.OldPassword,
                ProposedPassword = password.NewPassword,
                AccessToken = await metadataService.GetAccessToken(cancellationToken),
            }, cancellationToken);
            answer = new(true,I18n.PasswordUpdatedSuccessfully);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to change password");
            answer = new(false, I18n.UnknownError);
        }

        // await _accountLogsRepository.PushLog(answer,  null, new()
        // {
        //     { "PasswordHash" , Utils.GenerateHash(password.NewPassword, _configuration.PasswordSecret)}
        // }, cancellationToken);
        return answer;
    }

    public async ValueTask<OperationResultValue> AdminChangePasswordAsync(string username, string? password, 
        bool permanent, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        try
        {
            await identityProvider.AdminSetUserPasswordAsync(new()
            {
                Username = username,
                Password = password,
                Permanent = permanent,
                UserPoolId = _configuration.CognitoUserPoolId
            }, cancellationToken);
            answer = new(true,I18n.PasswordUpdatedSuccessfully);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to change password");
            answer = new(false, I18n.UnknownError);
        }
        return answer;
    }

    public async ValueTask<OperationResultValue> ForgotPasswordAsync(string? alias, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        try
        {
            UserAccountObject? user;
            if ((user = await repository.FindByAliasAsync(alias, cancellationToken)) != null)
            {
                await identityProvider.ForgotPasswordAsync(new()
                {
                    Username = user.Username,
                    ClientId = _configuration.CognitoUserPoolAppId,
                    ClientMetadata = new()
                    {
                        {"UserAgent", metadataService.GetUserAgent().ToString("D")}
                    }
                }, cancellationToken);
            }
            answer = new(true, I18n.VerificationSent);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to change password");
            answer = new(true, I18n.VerificationSent);
        }
        //await _accountLogsRepository.PushLog(answer,  alias, null, cancellationToken);
        return answer;
    }

    public async ValueTask<OperationResultValue> ResetPasswordAsync(IPasswordReset password, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        try
        {
            UserAccountObject? user;
            if ((user = await repository.FindByAliasAsync(password.Alias, cancellationToken)) != null)
            {
                await identityProvider.ConfirmForgotPasswordAsync(new()
                {
                    ConfirmationCode = password.Code,
                    Password = password.NewPassword,
                    Username = user.Username,
                    ClientId = _configuration.CognitoUserPoolAppId
                }, cancellationToken);
                answer = new(true, I18n.PasswordUpdatedSuccessfully);
            }
            else
                answer = new(false, I18n.UserNotFound);
        }
        catch (CodeMismatchException)
        {
            answer = new(false, I18n.InvalidVerificationCode);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to change password");
            answer = new(false, I18n.UnknownError);
        }
        // await _accountLogsRepository.PushLog(answer,  password.Alias, new()
        // {
        //     { "PasswordHash" , Utils.GenerateHash(password.NewPassword, _configuration.PasswordSecret)}
        // }, cancellationToken);

        return answer;
    }

}