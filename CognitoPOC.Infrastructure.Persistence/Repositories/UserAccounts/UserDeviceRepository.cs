using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.Repositories.UserAccounts;

public class UserDeviceRepository(
    DomainContext context,
    IMapper? mapper = null,
    IQueryFilterTranslator<UserDevicesObject, Guid>? filterTranslator = null)
    :
        BaseEfEntityRepository<UserDevicesObject, Guid>(context, mapper, filterTranslator),
        IUserDeviceRepository,
        IUserDeviceQueryRepository
{
    public Task AddAsync(Guid userId, UserDevicesObject device, CancellationToken cancellationToken)
    {
        if (userId == default)
            throw new ArgumentException(userId.ToString());
        Context.Entry(device).Property<Guid>("UserId").CurrentValue = userId;
        return AddAsync(device, cancellationToken);
    }

    public Task<UserDevicesObject[]> GetConfirmedDevices(string alias, CancellationToken cancellationToken)
    {
        return DataSet.AsNoTracking().Where(p => p.Alias == alias && p.IsActive).ToArrayAsync(cancellationToken);
    }
}