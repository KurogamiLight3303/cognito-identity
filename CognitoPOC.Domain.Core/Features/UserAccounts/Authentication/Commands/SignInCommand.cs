using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;

public class SignInCommand : CommandBase<AuthorizationValue>
{
    public string? Alias { get; init; }
    public string? Password { get; init; }
}