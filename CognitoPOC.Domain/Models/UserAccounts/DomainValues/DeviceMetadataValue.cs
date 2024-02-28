namespace CognitoPOC.Domain.Models.UserAccounts.DomainValues;

public class DeviceMetadataValue
{
    public string? Name { get; init; }
    public string? IpAddress { get; init; }
    public UserAgentEnum UserAgent { get; init; }
}

public enum UserAgentEnum : short
{
    Unknown = 0,
    Web = 1,
    Android = 2,
    Ios = 3,
    LandingPage = 4,
    AndroidNative = 5,
    IosNative = 6,
    Admin = 7,
    Background = 8,
    Reseller = 9,
}