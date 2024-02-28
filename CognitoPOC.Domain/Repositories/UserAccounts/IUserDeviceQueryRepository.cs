using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Common.Repositories;

namespace CognitoPOC.Domain.Repositories.UserAccounts;

public interface IUserDeviceQueryRepository : IDomainQueryRepository<UserDevicesObject, Guid>
{
    
}