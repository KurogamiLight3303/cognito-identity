using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.MFA.Commands;

public class VerifySoftwareTokenCommand : CommandBase
{
    public string? Code { get; init; }
}