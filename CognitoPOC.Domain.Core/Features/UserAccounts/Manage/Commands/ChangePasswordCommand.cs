using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public class ChangePasswordCommand : CommandBase
{
    public Guid Id { get; init; }
    public string? Password { get; init; }
    public bool Permanent { get; init; }
}