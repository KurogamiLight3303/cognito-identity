using CognitoPOC.Domain.Models.Common;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts.Resume;

public class UserAccountResume : DomainResume<UserAccountObject>, IPreviewImage
{
    public string? Username { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public bool PhoneConfirmed { get; init; }
    public bool MfaEnabled { get; init; }
    public string? ProfilePicture { get; init; }
    public string? Picture { get; init; }
    public string? PreferredLanguage { get; init; }
    public MfaTypesEnum MfaPreference { get; init; }
    public bool SoftwareTokenLinked { get; init; }
    public string? PreviewImageId { get; init;}
    public string? PreviewImageUrl { get; set; }
}