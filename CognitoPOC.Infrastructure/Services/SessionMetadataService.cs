using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Extensions;

namespace CognitoPOC.Infrastructure.Services;

public class SessionMetadataService(IHttpContextAccessor httpContextAccessor) : ISessionMetadataService
{
    public Guid? GetCurrentDeviceId()
    {
        var deviceId = httpContextAccessor.HttpContext?.Request.GetHeaderValue("Device_Id");

        deviceId = deviceId?.Replace("stu-", string.Empty);
        return !string.IsNullOrEmpty(deviceId) && Guid.TryParse(deviceId, out var result)
            ? result : null;
    }

    public string? GetCurrentHash()
    {
        return httpContextAccessor.HttpContext?.Request.GetHeaderValue("Client_Hash");
    }

    public string? GetCurrentDeviceAlias()
    {
        return httpContextAccessor.HttpContext?.Request.GetHeaderValue("Device_Id");
    }

    public string? GetCurrentClientId()
    {
        return httpContextAccessor.HttpContext?.Request.GetHeaderValue("Client_Id");
    }

    public string? GetCurrentDevicePassword()
    {
        return httpContextAccessor.HttpContext?.Request.GetHeaderValue("Device_Password");
    }

    public string? GetCurrentUsername()
    {
        return httpContextAccessor
            .HttpContext?
            .User
            .Claims
            .Where(p => p.Type == "username")
            .Select(p => p.Value)
            .FirstOrDefault();
    }

    public CurrentDeviceMetadata GetUserMetadata()
    {
        return new()
        {
            Name = GetDeviceName(),
            IpAddress = GetIpAddress(),
            Password = GetCurrentDevicePassword(),
            UserAgent = UserAgentEnum.Web,
            Id = GetCurrentDeviceId()
        };
    }

    public Task<string?> GetAccessToken(CancellationToken cancellationToken) 
        => httpContextAccessor.HttpContext?.GetTokenAsync("access_token") ?? Task.FromResult(string.Empty)!;

    public UserAgentEnum GetUserAgent() 
        => GetPlatform(GetCustomUserAgent() ?? GetDeviceName()) switch 
        {
            "Cubatel.iOS" => UserAgentEnum.IosNative,
            "Cubatel.Android" => UserAgentEnum.AndroidNative,
            "Cubatel.Admin" => UserAgentEnum.Admin,
            _ => httpContextAccessor
                .HttpContext?
                .Request
                .GetHeaderValue("Stu-User-Agent ")?.StartsWith("Cubatel.Admin") == true
            ? UserAgentEnum.Admin
            : UserAgentEnum.Web
        };
    private static string GetPlatform(string? deviceName)
    {
        var result = string.Empty;
        try
        {
            string[] output;
            if (!string.IsNullOrEmpty(deviceName) 
                && deviceName.StartsWith("Cubatel.") 
                && (output = deviceName.Split('/')).Length > 1)
                result = output[0];
        }
        catch
        {
            // ignored
        }

        return result;
    }

    public string? GetCustomUserAgent()
        => httpContextAccessor.HttpContext?.Request.GetCustomUserAgent();

    public bool IsInRole(string? role)
    {
        return !string.IsNullOrEmpty(role) && httpContextAccessor.HttpContext != null &&
            httpContextAccessor.HttpContext.User.IsInRole(role);
    }

    private string? GetDeviceName()
        => httpContextAccessor.HttpContext?.Request.GetUserAgent();
    public string? GetIpAddress()
        => httpContextAccessor.HttpContext?.Request.GetIpAddress();
}