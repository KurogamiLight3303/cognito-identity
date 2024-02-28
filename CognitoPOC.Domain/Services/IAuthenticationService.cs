using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Models.UserAccounts.Requests;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Services;

public interface IAuthenticationSignInService
{
    ValueTask<OperationResultValue<AuthorizationValue>> SignInAsync(string? alias, string? password, 
        CancellationToken cancellationToken);
    ValueTask<OperationResultValue<AuthorizationValue>> VerifyChallengeAsync(IAuthChallengeResponse response,
        CancellationToken cancellationToken);
    ValueTask<OperationResultValue> ConfirmDeviceAsync(bool rememberDevice, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> RememberDeviceAsync(bool rememberDevice, CancellationToken cancellationToken);
}

public interface IAuthenticationSessionService
{
    ValueTask<OperationResultValue> SignOutAsync(string? refreshToken, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> GlobalSignOutAsync(CancellationToken cancellationToken);
    ValueTask<OperationResultValue<AuthorizationValue>> RefreshTokenAsync(string? refreshToken, 
        CancellationToken cancellationToken);
}

public interface IAuthenticationTotpService
{
    ValueTask<OperationResultValue<SoftwareTokenAssociationValue>> AssociateSoftwareToken(
        CancellationToken cancellationToken);
    ValueTask<OperationResultValue> VerifySoftwareToken(string? code, CancellationToken cancellationToken);
}

public interface IAuthenticationPasswordService
{
    ValueTask<OperationResultValue> ChangePasswordAsync(IPasswordChange password, CancellationToken cancellationToken);

    ValueTask<OperationResultValue> AdminChangePasswordAsync(string username, string? password,
        bool permanent, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> ForgotPasswordAsync(string? alias, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> ResetPasswordAsync(IPasswordReset password, CancellationToken cancellationToken);
}

public interface IAuthenticationProfileService
{
    ValueTask<OperationResultValue> UpdatePhone(string? phone, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> VerifyPhone(string? code, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> ResendPhoneVerificationCode(CancellationToken cancellationToken);
    ValueTask<OperationResultValue> SetMfaPreference(MfaTypesEnum mfa, CancellationToken cancellationToken);
    ValueTask<OperationResultValue> UpdateProfile(IUpdateProfile profile, CancellationToken cancellationToken);
}

public interface IAuthenticationSignUpService
{
    ValueTask<OperationResultValue> Subscribe(UserAccountObject user, string password, CancellationToken cancellationToken);
    Task UnSubscribe(UserAccountObject user, CancellationToken cancellationToken);
}

public interface IAuthenticationRolesService
{
    ValueTask<OperationResultValue> UpdateRoles(UserAccountObject user, string[] roles, CancellationToken cancellationToken);
    ValueTask<string[]> GetRoles(UserAccountObject user, CancellationToken cancellationToken);
}