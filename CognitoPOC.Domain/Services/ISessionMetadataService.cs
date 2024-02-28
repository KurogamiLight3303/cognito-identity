using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Domain.Services;

public interface ISessionMetadataService
{
    Guid?GetCurrentDeviceId();
    string?GetCurrentHash();
    string? GetCurrentDevicePassword();
    string? GetCurrentUsername();
    CurrentDeviceMetadata GetUserMetadata();
    Task<string?> GetAccessToken(CancellationToken cancellationToken);
    string? GetIpAddress();
    UserAgentEnum GetUserAgent();
    string? GetCurrentDeviceAlias();
    string? GetCurrentClientId();
    public string? GetCustomUserAgent();
    public bool IsInRole(string? role);
}
