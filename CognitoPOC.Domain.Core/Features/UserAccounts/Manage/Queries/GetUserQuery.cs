using CognitoPOC.Domain.Core.Common.Queries;
using CognitoPOC.Domain.Models.UserAccounts.Resume;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Queries;

public class GetUserQuery : IQuery<UserAccountBaseResume>
{
    public Guid Id { get; init; }
}