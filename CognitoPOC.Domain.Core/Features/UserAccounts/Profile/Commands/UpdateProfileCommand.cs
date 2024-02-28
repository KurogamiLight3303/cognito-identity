using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.Requests;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;

public class UpdateProfileCommand : CommandBase, IUpdateProfile
{
    public string? Firstname { get; init; }
    public string? LastName { get; init;}
    public Guid? ProfilePicture { get; init;}
    public string? Picture { get; init; }
    public string? PreferredLanguage { get; init;}
}