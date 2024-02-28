using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public class DisableUserCommand : CommandBase
{
    public Guid Id { get; init; }
}