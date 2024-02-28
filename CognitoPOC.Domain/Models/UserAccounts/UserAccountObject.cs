using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts;

public class UserAccountObject : PrimaryDomainObject<Guid>
{
    public string? Username { get; set; }
    public VerifiableValue<string> Email { get; set; } = new();
    public VerifiableValue<string> PhoneNumber { get; set; } = new();
    public string? NewPhone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool MfaEnabled { get; set; }
    public virtual ICollection<UserDevicesObject>? Devices { get; set; }
    public Guid? ProfilePicture { get; set; }
    public string? Picture { get; set; }
    public string? PreferredLanguage { get; set; }
    public MfaTypesEnum MfaPreference { get; set; }
    public bool SoftwareTokenLinked { get; set; }
}