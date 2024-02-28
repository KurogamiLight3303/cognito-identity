namespace CognitoPOC.Domain.Models.UserAccounts.DomainValues;

public class CurrentDeviceMetadata : DeviceMetadataValue
{
    public Guid? Id { get; init; }
    public string? Password { get; init; }
}