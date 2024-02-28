using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;

public class RefreshSessionCommand : CommandBase<AuthorizationValue>
{
    public string? RefreshToken { get; init; }
}