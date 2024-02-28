using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.Resume;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public abstract class BaseUpdateUserCommand : CommandBase<UserAccountBaseResume>
{
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}