using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public class DisableMultipleUserCommand : CommandBase
{
    public Guid[]? data { get; init; }
}