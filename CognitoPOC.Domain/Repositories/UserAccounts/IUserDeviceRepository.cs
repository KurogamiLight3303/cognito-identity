using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Common.Repositories;

namespace CognitoPOC.Domain.Repositories.UserAccounts;

public interface IUserDeviceRepository : IDomainCommandRepository<UserDevicesObject, Guid>
{
    public Task AddAsync(Guid userId, UserDevicesObject device, CancellationToken cancellationToken);
    public Task<UserDevicesObject[]> GetConfirmedDevices(string alias, CancellationToken cancellationToken);
}