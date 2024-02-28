using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;

public class SignOutCommand : CommandBase
{
    public string? Token { get; init; }
}