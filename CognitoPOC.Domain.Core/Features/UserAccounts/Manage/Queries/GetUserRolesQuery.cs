using CognitoPOC.Domain.Core.Common.Queries;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Queries;

public class GetUserRolesQuery : ICollectionQuery<string>
{
    public Guid Id { get; init; }
}