using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;

public class UpdatePhoneCommand : CommandBase
{
    public string? Phone { get; set; }
}