using System.Diagnostics.CodeAnalysis;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using CognitoPOC.Domain.Common;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Extensions;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Models.UserAccounts.Requests;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Infrastructure.Services.Authentication;

public class AuthenticationProfileService(
    ISessionMetadataService metadataService,
    IAmazonCognitoIdentityProvider identityProvider,
    IUserAccountCommandRepository repository,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IAuthenticationProfileService
{
    //private readonly IAccountLogsRepository _accountLogsRepository;


    public async ValueTask<OperationResultValue> UpdatePhone(string? phone, CancellationToken cancellationToken)
    {
        var answer = !string.IsNullOrEmpty(phone) 
            ? await InternalUpdatePhone(phone, cancellationToken) 
            : new(false, I18n.InvalidPhone);
        // await _accountLogsRepository.PushLog(answer,  null, new()
        // {
        //     {"ResendCode", "true"}
        // }, cancellationToken);
        return answer;
    }
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    private async ValueTask<OperationResultValue> InternalUpdatePhone(string phone,
        CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await metadataService.GetAccessToken(cancellationToken);
            var username = metadataService.GetCurrentUsername();
            var user = await repository.FindByAliasAsync(username, cancellationToken);
            if (user == null)
            {
                logger.LogError("Unable find user: {Username}", username);
                return new(false, I18n.InvalidCredentials);
            }

            if (user.PhoneNumber.Value != null 
                && user.PhoneNumber.Value.Equals(phone))
                return new(false, I18n.PhoneNumberAlreadyVerified);
            if (user.NewPhone?.Equals(phone) == true)
                return new(false, I18n.PhoneNumberAlreadyVerified);
            await identityProvider.UpdateUserAttributesAsync(new()
            {
                AccessToken = accessToken,
                UserAttributes = new List<AttributeType>()
                {
                    new()
                    {
                        Name = "phone_number",
                        Value = phone
                    }
                }
            }, cancellationToken);

            if (await unitOfWork.ExecuteInTransactionAsync(async (cc) =>
                {
                    try
                    {
                        user.NewPhone = phone.CleanPhoneNumber();
                        repository.Update(user);
                        return await unitOfWork.CommitAsync(cc);
                    }
                    catch (Exception exc)
                    {
                        logger.LogError(exc, "Unable update user: {Username}", username);
                    }

                    return false;
                }, cancellationToken)) 
                return await SendPhoneVerificationCode(accessToken, phone, cancellationToken);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to update phone to {Phone}", phone);
        }
        return new(false, I18n.UnknownError);
    }
    
    private async ValueTask<OperationResultValue> SendPhoneVerificationCode(string? accessToken, string? phone,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await identityProvider.GetUserAttributeVerificationCodeAsync(
                new()
                {
                    AccessToken = accessToken,
                    AttributeName = "phone_number",
                }, cancellationToken);
            if (result.CodeDeliveryDetails != null)
                return new(true, string.Format(I18n.PhoneMessageChallengeMessage,
                    result.CodeDeliveryDetails.Destination));
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to send verification to phone");
        }
        return new(false, I18n.UnableToUpdateMfaPreference);
    }

    public async ValueTask<OperationResultValue> VerifyPhone(string? code, CancellationToken cancellationToken)
    {
        OperationResultValue answer;
        try
        {
            var data = await metadataService.GetAccessToken(cancellationToken);
            var username = metadataService.GetCurrentUsername();
            await identityProvider.VerifyUserAttributeAsync(new()
            {
                AccessToken = data,
                AttributeName = "phone_number",
                Code = code
            }, cancellationToken);

            answer = await unitOfWork.ExecuteInTransactionAsync<OperationResultValue>(async (cc) =>
            {
                try
                {
                    var user = await repository.FindByAliasAsync(username, cc);
                    if (user != null)
                    {
                        user.PhoneNumber = new VerifiableValue<string>()
                        {
                            Value = user.NewPhone,
                            Verified = true
                        };
                        repository.Update(user);
                        await unitOfWork.CommitAsync(cc);
                    }
                    else
                        logger.LogError("User not found {Username}", username);
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, "Unable to verify phone");
                }
                return new(true, I18n.PhoneNumberUpdated);
            }, cancellationToken);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to verify phone");
            answer = new(false, I18n.UnableToVerifyPhone);
        }
        //await _accountLogsRepository.PushLog(answer,  null, null, cancellationToken);
        return answer;
    }

    public async ValueTask<OperationResultValue> ResendPhoneVerificationCode(CancellationToken cancellationToken)
    {
        var answer = await SendPhoneVerificationCode(
            await metadataService.GetAccessToken(cancellationToken),
            null, cancellationToken);
        // await _accountLogsRepository.PushLog(answer,  null, new()
        // {
        //     {"ResendCode", "true"}
        // }, cancellationToken);
        return answer;
    }

    public async ValueTask<OperationResultValue> SetMfaPreference(MfaTypesEnum mfa, CancellationToken cancellationToken)
    {
        OperationResultValue answer;
        UserAccountObject? user;
        try
        {
            var username = metadataService.GetCurrentUsername();
            var accessToken = await metadataService.GetAccessToken(cancellationToken);
            var enabled = true;
            var request = new SetUserMFAPreferenceRequest()
            {
                AccessToken = accessToken
            };
            switch (mfa)
            {
                case MfaTypesEnum.None:
                    enabled = false;
                    request.SMSMfaSettings = new()
                    {
                        Enabled = false,
                        PreferredMfa = false
                    };
                    request.SoftwareTokenMfaSettings = new()
                    {
                        Enabled = false,
                        PreferredMfa = false
                    };
                    break;
                case MfaTypesEnum.Sms:
                    request.SMSMfaSettings = new()
                    {
                        Enabled = true,
                        PreferredMfa = true
                    };
                    request.SoftwareTokenMfaSettings = new()
                    {
                        Enabled = false,
                        PreferredMfa = false
                    };
                    break;
                case MfaTypesEnum.SoftwareToken:
                    request.SoftwareTokenMfaSettings = new()
                    {
                        Enabled = true,
                        PreferredMfa = true
                    };
                    request.SMSMfaSettings = new()
                    {
                        Enabled = false,
                        PreferredMfa = false
                    };
                    break;
                case MfaTypesEnum.SelectAtSignIn:
                    request.SoftwareTokenMfaSettings = new()
                    {
                        Enabled = true,
                        PreferredMfa = false
                    };
                    request.SMSMfaSettings = new()
                    {
                        Enabled = true,
                        PreferredMfa = false
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mfa), mfa, @"Invalid MFA");
            }

            await identityProvider.SetUserMFAPreferenceAsync(request, cancellationToken);
            answer = await unitOfWork.ExecuteInTransactionAsync<OperationResultValue>(async (cc)
                =>
            {
                user = await repository.FindByAliasAsync(accessToken, cancellationToken);
                if (user != null)
                {
                    user.MfaEnabled = enabled;
                    user.MfaPreference = mfa;
                    repository.Update(user);
                    if (await unitOfWork.CommitAsync(cc))
                        return new(true, I18n.MfaPreferenceSuccessfullyUpdated);
                }

                return new(false, I18n.UnableToUpdateMfaPreference);
            }, cancellationToken);
        }
        catch (InvalidParameterException exc)
        {
            if (exc.Message == "User does not have delivery config set to turn on SOFTWARE_TOKEN_MFA")
            {
                answer = new(false, I18n.UnableToUpdateMfaPreference);
                logger.LogError(exc, "Unable to update MFA Preference {Mfa}", mfa);
            }
            else
            {
                answer = new(false, I18n.UnableToUpdateMfaPreference);
            }
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to update MFA Preference {Mfa}", mfa);
            answer = new(false, I18n.UnableToUpdateMfaPreference);
        }

        return answer;
    }

    public async ValueTask<OperationResultValue> UpdateProfile(IUpdateProfile profile, CancellationToken cancellationToken)
    {
        OperationResultValue answer;
        UserAccountObject? user;
        try
        {
            var accessToken = await metadataService.GetAccessToken(cancellationToken);
            var username = metadataService.GetCurrentUsername();
            if ((user = await repository.FindByAliasAsync(username, cancellationToken)) != null)
            {
                await identityProvider.UpdateUserAttributesAsync(new()
                {
                    AccessToken = accessToken,
                    UserAttributes = new()
                    {
                        new AttributeType()
                        {
                            Name = "given_name",
                            Value = profile.Firstname ?? user.FirstName,
                        },
                        new AttributeType()
                        {
                            Name = "family_name",
                            Value = profile.LastName ?? user.LastName,
                        }
                    }
                }, cancellationToken);
                answer = await unitOfWork.ExecuteInTransactionAsync<OperationResultValue>(async (cc)
                    =>
                {

                    user.FirstName = profile.Firstname ?? user.FirstName;
                    user.LastName = profile.LastName ?? user.LastName;
                    user.PreferredLanguage = profile.PreferredLanguage ?? user.PreferredLanguage;
                    user.ProfilePicture = profile.ProfilePicture ?? user.ProfilePicture;
                    user.Picture = profile.Picture;
                    repository.Update(user);
                    if (await unitOfWork.CommitAsync(cc))
                        return new(true, I18n.ProfileUpdatedSuccessfully);

                    return new(false, I18n.UnableToUpdateMfaPreference);
                }, cancellationToken);
            }
            else
                answer = new(false, I18n.UnknownError);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to update Unable to update profile");
            answer = new(false, I18n.UnableToUpdateMfaPreference);
        }

        return answer;
    }
}