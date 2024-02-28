using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts;

public class UserDevicesObject : PrimaryDomainObject<Guid>, IMetadataContainer
{
    public UserDevicesObject()
    {
        Metadata = new();
    }
    public string? Alias { get; init; }
    public DeviceMetadataValue? DeviceInfo { get; init; }
    public DateTime LastAuth { get; set; }
    public MetadataDomainValue Metadata { get; init; }
    public bool Confirmed { get; private set; }
    public DateTime? ConfirmedDate { get; private set; }
    public bool Remembered { get; private set; }
    
    public virtual UserAccountObject? User { get; set; }

    public void Confirm(bool rememberDevice)
    {
        ConfirmedDate = DateTime.Now;
        Confirmed = true;
        Remember(rememberDevice);
    }

    public void Remember(bool rememberDevice)
    {
        Remembered = rememberDevice;
    }
    public void ResetConfirmation()
    {
        ConfirmedDate = null;
        Confirmed = false;
        Remembered = false;
    }
    public void Forgot()
    {
        IsActive = false;
    }
}