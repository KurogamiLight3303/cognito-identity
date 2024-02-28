using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using CognitoPOC.Domain.Common;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Models.UserAccounts.Requests;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;
using CognitoPOC.Infrastructure.Extensions;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationSignInService(
    ISessionMetadataService metadataService,
    IAmazonCognitoIdentityProvider identityProvider,
    IUserAccountCommandRepository repository,
    IUserDeviceRepository deviceRepository,
    IUnitOfWork unitOfWork,
    ILogger logger,
    IOptions<CognitoConfiguration> configuration)
    : IAuthenticationSignInService
{
    private readonly CognitoConfiguration _configuration = configuration.Value;
    //protected readonly IAccountLogsRepository AccountLogsRepository;

    private async ValueTask<AuthorizationValue> GetAuthorization(AuthenticationResultType result,
        string? refreshToken, CancellationToken cancellationToken)
    {
        JsonWebToken token = new(result.AccessToken);
        var username = token.Claims.First(p => p.Type == "username").Value;
        var deviceId = metadataService.GetCurrentDeviceId();
        var deviceInfo = metadataService.GetUserMetadata();
        if (result.NewDeviceMetadata != null)
        {
            var device = await GetDevice(deviceId, username, cancellationToken);
            if (device is not null )
            {
                device.Metadata.SetValue("Cognito_DeviceKey", result.NewDeviceMetadata.DeviceKey);
                device.Metadata.SetValue("Cognito_DeviceGroupKey", result.NewDeviceMetadata.DeviceGroupKey);
                device.ResetConfirmation();
                deviceRepository.Update(device);
            }
            else
            {
                device = new()
                {
                    Alias = deviceId != null ? deviceId.ToString() : metadataService.GetCurrentDeviceAlias(),
                    IsActive = true,
                    DeviceInfo = deviceInfo,
                    LastAuth = DateTime.Now,
                    Metadata = new(new KeyValuePair<string, object>[]
                    {
                        new ("Cognito_DeviceKey", result.NewDeviceMetadata.DeviceKey),
                        new ("Cognito_DeviceGroupKey", result.NewDeviceMetadata.DeviceGroupKey),
                        new ("CustomDeviceName", metadataService.GetCustomUserAgent() ?? string.Empty)
                    })
                };
                await deviceRepository.AddAsync(await repository.GetUserId(username,cancellationToken),
                    device, cancellationToken);
            }
            if (await unitOfWork.CommitAsync(cancellationToken))
                deviceId ??= device.Id;
        }
        token = new(result.AccessToken);
        return new()
        {
            AccessToken = result.AccessToken,
            RefreshToken = refreshToken ?? result.RefreshToken,
            ExpiresIn = result.ExpiresIn,
            Username = username,
            Device = deviceId ?? deviceInfo.Id,
            RequestConfirmation = result.NewDeviceMetadata != null,
            Roles = token.Claims.Where(p => p.Type == "cognito:groups").Select(p => p.Value).ToArray()
        };
    }

    private ValueTask<Dictionary<string, string?>> GetChallengeResponseValues(IAuthChallengeResponse response)
    {
        var result = new Dictionary<string, string?>()
        {
            {"USERNAME", response.Username}
        };
        switch (response.Type)
        {
            case ChallengeTypeEnum.NewPasswordRequired:
                result.Add("NEW_PASSWORD", response.ChallengeValue);
                break;
            case ChallengeTypeEnum.SoftwareTokenChallenge:
                result.Add("SOFTWARE_TOKEN_MFA_CODE", response.ChallengeValue);
                break;
            case ChallengeTypeEnum.PhoneMessageChallenge:
                result.Add("SMS_MFA_CODE", response.ChallengeValue);
                break;
            case ChallengeTypeEnum.SelectMfaChallenge:
                if (Enum.TryParse<ChallengeTypeEnum>(response.ChallengeValue, out var type))
                {
                    switch (type)
                    {
                        case ChallengeTypeEnum.PhoneMessageChallenge:
                            result.Add("ANSWER", "SMS_MFA");
                            break;
                        case ChallengeTypeEnum.SoftwareTokenChallenge:
                            result.Add("ANSWER", "SOFTWARE_TOKEN_MFA");
                            break;
                    }
                }
                break;
        }

        return ValueTask.FromResult(result);
    }

    private Task<UserDevicesObject?> GetDevice(Guid? alias, string? username, CancellationToken cancellationToken)
        => deviceRepository.FindAsync(p
            =>
            p.User != null
            && p.User.Username == username
            && (p.Id == alias || p.Alias == alias.ToString()), cancellationToken);

    private ValueTask<OperationResultValue<AuthorizationValue>> PrepareChallengeRequest(string? username,
        string? challenge, string? session, IDictionary<string, string> challengeParameters)
    {
        OperationResultValue<AuthorizationValue>? answer = null;
        var retry = false;
        try
        {
            var challengeType = challenge.GetChallengeType();

            answer = new(true,
                new()
                {
                    Username = username,
                    Challenge = new()
                    {
                        Type = challengeType,
                        Session = session,
                        Options = GetChallengeOptions(challengeType, challengeParameters)
                    }
                },
                retry ? I18n.InvalidVerificationCode :
                challengeType switch
                {
                    ChallengeTypeEnum.PhoneMessageChallenge => string.Format(I18n.SendPhoneVerificationTo, challengeParameters["CODE_DELIVERY_DESTINATION"]),
                    ChallengeTypeEnum.SoftwareTokenChallenge => I18n.CustomTOTPCode,
                    ChallengeTypeEnum.NewPasswordRequired => I18n.NewPasswordRequiredMessage,
                    ChallengeTypeEnum.SelectMfaChallenge => I18n.SelectSignInMfa,
                    _ => throw new NotImplementedException()
                });

        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Fail to prepare challenge");
        }
        finally
        {
            answer ??= new(false);
        }
        return ValueTask.FromResult(answer);
    }

    private static Dictionary<ChallengeTypeEnum, string> GetChallengeOptions(ChallengeTypeEnum type,
        IDictionary<string, string> challengeParameters)
    {
        var answer = new Dictionary<ChallengeTypeEnum, string>();
        if (type == ChallengeTypeEnum.SelectMfaChallenge 
            && challengeParameters.TryGetValue("MFAS_CAN_CHOOSE", out var challengeParameter))
        {
            var options = JsonSerializer.Deserialize<string[]>(challengeParameter, 
                JsonSerializerOptions.Default);
            if (options != null)
            {
                if(options.Contains("SMS_MFA") 
                   && challengeParameters.TryGetValue("CODE_DELIVERY_DESTINATION", out var parameter))
                    answer.Add(ChallengeTypeEnum.PhoneMessageChallenge, 
                        string.Format(I18n.SendPhoneVerificationTo, parameter));
                if(options.Contains("SOFTWARE_TOKEN_MFA"))
                    answer.Add(ChallengeTypeEnum.SoftwareTokenChallenge, I18n.CustomTOTPCode);
            }
        }
        return answer;
    }

    private CognitoUserPool UserPool => new(_configuration.CognitoUserPoolId, 
        _configuration.CognitoUserPoolAppId, identityProvider, _configuration.CognitoUserPoolAppSecret);

    public async ValueTask<OperationResultValue<AuthorizationValue>> SignInAsync(string? alias, string? password, 
        CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        UserAccountObject? user;

        if ((user = await repository.FindByAliasAsync(alias, cancellationToken)) != null)
            answer = user.IsActive 
                ? await AttemptSingIn(user, password!, true, cancellationToken)
                : new(false, I18n.UserIsDisabled);
        else
            answer = new(false, I18n.InvalidCredentials);

        // await AccountLogsRepository.PushLog(alias,answer, new()
        // {
        //     { "PasswordHash" , Utils.GenerateHash(password, Configuration.PasswordSecret)}
        // },cancellationToken);
        return answer;
    }

    private async ValueTask<OperationResultValue<AuthorizationValue>> AttemptSingIn(UserAccountObject user
        , string password, bool useDevice, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        Guid? deviceId;
        var device =
            useDevice && (deviceId = metadataService.GetCurrentDeviceId()).HasValue
                ? await deviceRepository.FindAsync(p 
                    => p.Id == deviceId || p.Alias == deviceId.Value.ToString() && p.User == user, cancellationToken)
                : null;
            
        var cUser = new CognitoUser(user.Username, _configuration.CognitoUserPoolAppId, UserPool, identityProvider);
        var authRequest = new InitiateSrpAuthRequest()
        {
            Password = password,
            ClientMetadata = new Dictionary<string, string>()
            {
                {"ClientApp", metadataService.GetCurrentClientId() ?? string.Empty},
                {"IpAddress", metadataService.GetIpAddress() ?? string.Empty}
            }
        };
        if (device is {Confirmed: true, Remembered: true, Metadata: not null })
        {
            authRequest.DeviceGroupKey = device.Metadata.GetValue("Cognito_DeviceGroupKey");
            authRequest.DevicePass = metadataService.GetCurrentDevicePassword();
            cUser.Device = new(new()
            {
                DeviceKey = device.Metadata.GetValue("Cognito_DeviceKey")
            }, cUser);
        }

        try
        {
            var response = await cUser.StartWithSrpAuthAsync(authRequest, cancellationToken);
            if (response.AuthenticationResult != null)
                answer = new(true,
                    await GetAuthorization(response.AuthenticationResult, null, cancellationToken));
            else
                answer = await PrepareChallengeRequest(user.Username, response.ChallengeName.Value,
                    response.SessionID, response.ChallengeParameters);
        }
        catch (NotAuthorizedException exc)
        {
            if(useDevice && device != null)
                answer = await AttemptSingIn(user, password, false, cancellationToken);
            else
            {
                answer = new(false, I18n.InvalidCredentials);
                logger.LogError(exc, "Unable to authenticate {Username}, Using Device {UseDevice}",
                    user.Username, useDevice);
            }
        }
        catch (ResourceNotFoundException exc)
        {
            if (useDevice && device != null)
                answer = await AttemptSingIn(user, password, false, cancellationToken);
            else
            {
                answer = new(false, I18n.InvalidCredentials);
                logger.LogError(exc, "Unable to authenticate {Username}, Using Device {UseDevice}",
                    user.Username, useDevice);
            }
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to authenticate {Username}", user.Username);
            answer = new(false, I18n.UnknownError);
        }

        return answer;
    }

    public async ValueTask<OperationResultValue<AuthorizationValue>> VerifyChallengeAsync(
        IAuthChallengeResponse response, CancellationToken cancellationToken)
    {
        OperationResultValue<AuthorizationValue> answer;
        try
        {
            var result = await identityProvider.RespondToAuthChallengeAsync(new()
            {
                ClientId = _configuration.CognitoUserPoolAppId,
                ChallengeName = response.Type.GetCognitoChallengeName(),
                Session = response.Session,
                ChallengeResponses = await GetChallengeResponseValues(response)
            }, cancellationToken);
            if (result.AuthenticationResult != null)
                answer = new(true,
                    await GetAuthorization(result.AuthenticationResult, null, cancellationToken));
            else
                answer = await PrepareChallengeRequest(response.Username, result.ChallengeName.Value,
                    result.Session, result.ChallengeParameters);
        }
        catch (Exception exc)
        {
            if (exc is NotAuthorizedException)
                answer = new(true, new()
                {
                    Challenge = new AuthChallengeValue()
                    {
                        Type = ChallengeTypeEnum.SessionExpire,
                        Session = response.Session
                    }
                }, I18n.SessionExpired);
            else if (exc is CodeMismatchException)
            {
                answer = new(false, I18n.InvalidCredentials);
            }
            else
            {
                logger.LogError(exc, "Challenge Verification Failed ({Type})",response.Type);
                answer = new(false, I18n.SessionExpired);
            }
        }
        // await AccountLogsRepository.PushLog(answer, new()
        // {
        //     {"VerificationType", response.Type.ToString()},
        //     {"VerificationValue", response.ChallengeValue ?? string.Empty},
        // },cancellationToken);
        return answer;
    }
    
    public async ValueTask<OperationResultValue> ConfirmDeviceAsync(bool rememberDevice, CancellationToken cancellationToken)
    {
        OperationResultValue? answer = null;
        UserDevicesObject? device;
        var deviceId = metadataService.GetCurrentDeviceId();
        var username = metadataService.GetCurrentUsername();
        var accessToken = await metadataService.GetAccessToken(cancellationToken);
        if (deviceId != null && (device = await GetDevice(deviceId, username, cancellationToken)) != null)
        {
            var deviceKey = device.Metadata.GetValue("Cognito_DeviceKey");
            var devicePassword = metadataService.GetCurrentDevicePassword();
            var deviceGroup = device.Metadata.GetValue("Cognito_DeviceGroupKey");
            try
            {
                await identityProvider.ConfirmDeviceAsync(new()
                {
                    AccessToken = accessToken,
                    DeviceKey = deviceKey,
                    DeviceName = device.DeviceInfo?.Name,
                    DeviceSecretVerifierConfig =
                        new CognitoUser(username, _configuration.CognitoUserPoolAppId, UserPool,
                            identityProvider).GenerateDeviceVerifier(
                            deviceGroup, devicePassword, username)
                }, cancellationToken);
                device.Confirm(rememberDevice);
                deviceRepository.Update(device);
                if (await unitOfWork.CommitAsync(cancellationToken))
                {
                    await identityProvider.UpdateDeviceStatusAsync(new()
                    {
                        AccessToken = accessToken,
                        DeviceKey = device.Metadata.GetValue("Cognito_DeviceKey"),
                        DeviceRememberedStatus = rememberDevice 
                            ? DeviceRememberedStatusType.Remembered
                            : DeviceRememberedStatusType.Not_remembered
                    }, cancellationToken);
                    answer = new(true);
                }
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "Fail to confirm device ({DeviceId}) ({DeviceKey}, {DeviceGroup})", 
                    deviceId, deviceKey, deviceGroup);
            }
        }
        

        answer ??= new(false, I18n.UnknownError);
        // await AccountLogsRepository.PushLog(answer, username.Result,new()
        // {
        //     {"RememberDevice", rememberDevice.ToString()},
        // },cancellationToken);
        return answer;
    }

    public async ValueTask<OperationResultValue> RememberDeviceAsync(bool rememberDevice, CancellationToken cancellationToken)
    {
        OperationResultValue? answer = null;
        UserDevicesObject? device;
        var deviceId = metadataService.GetCurrentDeviceId();
        var username = metadataService.GetCurrentUsername();
        var accessToken = await metadataService.GetAccessToken(cancellationToken);
        if (deviceId != null && (device = await GetDevice(deviceId, username, cancellationToken)) != null)
        {
            try
            {
                device.Remember(rememberDevice);
                deviceRepository.Update(device);
                if (await unitOfWork.CommitAsync(cancellationToken))
                {
                    await identityProvider.UpdateDeviceStatusAsync(new()
                    {
                        AccessToken = accessToken,
                        DeviceKey = device.Metadata.GetValue("Cognito_DeviceKey"),
                        DeviceRememberedStatus = rememberDevice 
                            ? DeviceRememberedStatusType.Remembered
                            : DeviceRememberedStatusType.Not_remembered
                    }, cancellationToken);
                    answer = new(true);
                }
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "Fail to confirm device");
            }
        }

        return answer ?? new(false, I18n.UnknownError);
    }
}