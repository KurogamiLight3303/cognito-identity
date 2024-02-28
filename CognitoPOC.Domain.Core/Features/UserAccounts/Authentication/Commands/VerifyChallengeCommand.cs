using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Models.UserAccounts.Requests;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;

public class VerifyChallengeCommand : CommandBase<AuthorizationValue>, IAuthChallengeResponse
{
    public string? Username { get; init; }
    public string? Session { get; init; }
    public ChallengeTypeEnum Type { get; init; }
    public string? ChallengeValue { get; init; }
}