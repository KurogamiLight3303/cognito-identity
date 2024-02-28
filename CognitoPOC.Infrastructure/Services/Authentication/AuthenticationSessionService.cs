using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using RestSharp;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationSessionService(
    ISessionMetadataService metadataService,
    IAmazonCognitoIdentityProvider identityProvider,
    IOptions<CognitoConfiguration> configuration,
    IUserDeviceRepository deviceRepository,
    ILogger logger)
    : IAuthenticationSessionService
{
    private readonly CognitoConfiguration _configuration = configuration.Value;

    //private readonly IAccountLogsRepository _accountLogsRepository;

    public async ValueTask<OperationResultValue> SignOutAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        OperationResultValue? answer = null;
        string? username = null;
        try
        {
            username = metadataService.GetCurrentUsername();
            var client = new RestClient(_configuration.Domain!);
            var request = new RestRequest("/oauth2/revoke", Method.Post);
            request.AddParameter("token", refreshToken);
            request.AddParameter("client_id", _configuration.CognitoUserPoolAppId);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            var response = await client.ExecuteAsync(request, cancellationToken);
            if (response.IsSuccessful)
                answer = new(true, I18n.SignOutSuccessfully);
            else
            {
                logger.LogError("Unable to sign out because: ({StatusCode}) {Content}",
                    response.StatusCode, response.Content);
                answer = new(false, I18n.UnableToSignOut);
            }
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to sign out");
        }
        finally
        {
            answer ??= new(false, I18n.UnableToSignOut);
            //await _accountLogsRepository.PushLog(answer, username ?? refreshToken ,null,cancellationToken);
        }
        
        return answer;
    }
    

    public async ValueTask<OperationResultValue<AuthorizationValue>> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue>? answer = null;
        try
        {
            string? key;
            var deviceId = metadataService.GetCurrentDeviceId();
            var deviceKeys = new List<string>();
            if (deviceId != null)
                foreach (var d in await deviceRepository.GetConfirmedDevices(deviceId.ToString()!, cancellationToken))
                {
                    key = d.Metadata.GetValue("Cognito_DeviceKey");
                    if (!string.IsNullOrEmpty(key))
                        deviceKeys.Add(key);
                }
            foreach (var k in deviceKeys)
            {
                answer = await TryRefresh(refreshToken!, k, cancellationToken);
                if (answer.Success)
                    break;
            }

            if (answer == null || !answer.Success)
                answer = await TryRefresh(refreshToken!, null, cancellationToken);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Fail to refresh token");
            answer = exc.Message switch
            {
                _ => new(false, I18n.InvalidCredentials)
            };
        }
        finally
        {
            answer ??= new(false, I18n.UnknownError);
            // if(answer.Success)
            //     await _accountLogsRepository.PushLog(answer,null,cancellationToken);
            // else
            //     await _accountLogsRepository.PushLog(refreshToken, answer,null,cancellationToken); 
        }

        return answer;
    }

    private async ValueTask<OperationResultValue<AuthorizationValue>> TryRefresh(string refreshToken, string? deviceKey,
        CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        try
        {
            var authParameters = new Dictionary<string, string>()
            {
                {"REFRESH_TOKEN", refreshToken},
            };
            if(deviceKey != null)
                authParameters.Add("DEVICE_KEY", deviceKey);
            var response = await identityProvider.AdminInitiateAuthAsync(new()
            {
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                UserPoolId = _configuration.CognitoUserPoolId,
                ClientId = _configuration.CognitoUserPoolAppId,
                AuthParameters = authParameters
            }, cancellationToken);
            answer = new(true,
                GetAuthorization(response.AuthenticationResult, refreshToken));
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Fail to refresh token");
            answer = exc.Message switch
            {
                _ => new(false, I18n.InvalidCredentials)
            };
        }

        return answer;
    }
    
    public async ValueTask<OperationResultValue> GlobalSignOutAsync(CancellationToken cancellationToken)
    {
        OperationResultValue answer;
        try
        {
            await identityProvider.GlobalSignOutAsync(new()
            {
                AccessToken = await metadataService.GetAccessToken(cancellationToken)
            }, cancellationToken);
            answer = new(true,I18n.SignOutSuccessfully);
        }
        catch (Exception exc)
        {
            answer = exc.Message switch
            {
                _ => new(false, I18n.UnableToSignOut)
            };
        }

        return answer;
    }
    
    private AuthorizationValue GetAuthorization(AuthenticationResultType result,
        string refreshToken)
    {
        JsonWebToken token = new(result.AccessToken);
        var username = token.Claims.First(p => p.Type == "username").Value;
        Guid? deviceId = null;
        var device = metadataService.GetUserMetadata();
        return new()
        {
            AccessToken = result.AccessToken,
            RefreshToken = refreshToken,
            ExpiresIn = result.ExpiresIn,
            Username = username,
            Device = deviceId ?? device.Id,
            RequestConfirmation = result.NewDeviceMetadata != null,
            Roles = token.Claims.Where(p => p.Type == "cognito:groups").Select(p => p.Value).ToArray()
        };
    }
}