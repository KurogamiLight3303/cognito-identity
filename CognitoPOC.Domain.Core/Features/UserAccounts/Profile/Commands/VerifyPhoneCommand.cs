using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;

public class VerifyPhoneCommand : CommandBase
{
    public string? Code { get; set; }
}