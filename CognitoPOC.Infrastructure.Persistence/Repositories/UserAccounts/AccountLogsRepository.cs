using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;

[assembly:InternalsVisibleTo("Stu.Cubatel.Infrastructure")]

namespace CognitoPOC.Infrastructure.Persistence.Repositories.UserAccounts;

internal enum AuthActionEnum
{
    SignIn,
    VerifyChallenge,
    RefreshToken,
    UpdatePhone,
    VerifyPhone,
    ForgotPassword,
    ResetPassword,
    ChangePassword,
    Unknown,
    SignOut,
    ConfirmDevice
}
[ElasticsearchType(Name = "authlogdio")]
internal class AccountLogModel
{
    public string? Source { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? AuthCredential { get; init; }
    public string? Username { get; init; }
    public Guid? DeviceId { get; init; }
    public string? DeviceAlias { get; init; }
    public string? DeviceName { get; init; }
    public string? IpAddress { get; init; }
    public IDictionary<string, string>? DeviceAttributes { get; init; }
    public IDictionary<string, string>? OtherInfo { get; init; }
    public DateTime CreatedDate { get; init; } = DateTime.Now;
    public AuthActionEnum Action { get; init; }
    public string? ClientId { get; init; }
}
internal class AccountLogsRepository : BaseElasticRepository<AccountLogModel>, IAccountLogsRepository
{
    private readonly ISessionMetadataService _metadataService;
    protected override string IndexName => "system-auth-logs";

    public AccountLogsRepository(IOptions<ElasticConfiguration> config, ILogger logger, ISessionMetadataService metadataService) : base(config, logger)
    {
        _metadataService = metadataService;
    }

    public async Task PushLog(string? username, OperationResultValue<AuthorizationValue> authorization, Dictionary<string, string>? extraData,
        CancellationToken cancellationToken, string? methodName = "")
    {
        AccountLogModel? data = null;
        try
        {
            var info = new Dictionary<string, string>();
            if(extraData != null)
                foreach (var pair in extraData)
                    info.Add(pair.Key, pair.Value);
            var deviceInfo = new Dictionary<string, string>();
            info.Add("UserAgent", _metadataService.GetUserAgent().ToString());
            deviceInfo.Add("deviceConfirmed", (authorization.Data is { RequestConfirmation: false }).ToString());
            var deviceAlias = _metadataService.GetCurrentDeviceId();
            var deviceMetadata = _metadataService.GetUserMetadata();
            data = new AccountLogModel()
            {
                Source = methodName,
                Success = authorization.Success,
                Message = authorization.Message,
                AuthCredential = username ?? authorization.Data?.Username ?? string.Empty,
                Username = authorization.Data != null ? authorization.Data.Username : string.Empty,
                DeviceAlias = _metadataService.GetCurrentDeviceAlias(),
                DeviceId = deviceAlias,
                DeviceAttributes = deviceInfo,
                DeviceName = deviceMetadata.Name,
                IpAddress = deviceMetadata.IpAddress,
                OtherInfo = info,
                ClientId = _metadataService.GetCurrentClientId(),
                Action = GetAction(methodName)
            };
            await AddAsync(data, cancellationToken);
        }
        catch (Exception exc)
        {
            Logger.LogError(exc, "Unable to log auth : {@Data}", data);
        }
    }

    public Task PushLog(OperationResultValue<AuthorizationValue> authorization, Dictionary<string, string>? extraData,
        CancellationToken cancellationToken, [CallerMemberName]string? methodName = "") 
        => PushLog(authorization.Data?.Username ?? string.Empty, authorization, extraData, cancellationToken, methodName);

    public async Task PushLog(OperationResultValue result, string? alias, Dictionary<string, string>? extraData, CancellationToken cancellationToken,
        string? methodName = "")
    {
        AccountLogModel? data = null;
        try
        {
            var info = new Dictionary<string, string>();
            if(extraData != null)
                foreach (var pair in extraData)
                    info.Add(pair.Key, pair.Value);
            var deviceInfo = new Dictionary<string, string>();
            info.Add("UserAgent", _metadataService.GetUserAgent().ToString());
            var deviceAlias = _metadataService.GetCurrentDeviceId();
            var deviceMetadata = _metadataService.GetUserMetadata();
            var username = _metadataService.GetCurrentUsername();
            data = new AccountLogModel()
            {
                Source = methodName,
                Success = result.Success,
                Message = result.Message,
                AuthCredential = alias,
                Username = username ?? alias,
                DeviceAlias = _metadataService.GetCurrentDeviceAlias(),
                DeviceId = deviceAlias,
                DeviceAttributes = deviceInfo,
                DeviceName = deviceMetadata.Name,
                IpAddress = deviceMetadata.IpAddress,
                OtherInfo = info,
                ClientId = _metadataService.GetCurrentClientId(),
                Action = GetAction(methodName)
            };
            await AddAsync(data, cancellationToken);
        }
        catch (Exception exc)
        {
            Logger.LogError(exc, "Unable to log auth : {@Data}", data);
        }
    }

    private AuthActionEnum GetAction(string? methodName)
    {
        return methodName switch
        {
            nameof(IAuthenticationSignInService.SignInAsync)
                or nameof(IAuthenticationSignInService.VerifyChallengeAsync) => AuthActionEnum.SignIn,
            nameof(IAuthenticationSessionService.RefreshTokenAsync) => AuthActionEnum.RefreshToken,
            nameof(IAuthenticationProfileService.UpdatePhone)
                or nameof(IAuthenticationProfileService.ResendPhoneVerificationCode) => AuthActionEnum.UpdatePhone,
            nameof(IAuthenticationProfileService.VerifyPhone) => AuthActionEnum.VerifyPhone,
            nameof(IAuthenticationPasswordService.ForgotPasswordAsync) => AuthActionEnum.ForgotPassword,
            nameof(IAuthenticationPasswordService.ResetPasswordAsync) => AuthActionEnum.ResetPassword,
            nameof(IAuthenticationPasswordService.ChangePasswordAsync) => AuthActionEnum.ChangePassword,
            nameof(IAuthenticationSignInService.ConfirmDeviceAsync) => AuthActionEnum.ConfirmDevice,
            nameof(IAuthenticationSessionService.SignOutAsync) => AuthActionEnum.SignOut,
            _ => AuthActionEnum.Unknown
        };
    }
}

public interface IAccountLogsRepository
{
    Task PushLog(string? username, OperationResultValue<AuthorizationValue> authorization, Dictionary<string, string>? extraData,
        CancellationToken cancellationToken, [CallerMemberName] string? methodName = "");
    Task PushLog(OperationResultValue<AuthorizationValue> authorization, Dictionary<string, string>? extraData,
        CancellationToken cancellationToken, [CallerMemberName] string? methodName = "");
    Task PushLog(OperationResultValue result, string? alias, Dictionary<string, string>? extraData,
        CancellationToken cancellationToken, [CallerMemberName] string? methodName = "");
}