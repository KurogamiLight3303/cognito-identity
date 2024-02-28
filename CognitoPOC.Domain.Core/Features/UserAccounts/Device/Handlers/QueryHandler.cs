using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Queries;
using CognitoPOC.Domain.Core.Features.UserAccounts.Device.Queries;
using CognitoPOC.Domain.Models.UserAccounts.Resume;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Device.Handlers;

public class QueryHandler(IUserDeviceQueryRepository repository, ISessionMetadataService sessionMetadataService)
    : ICollectionQueryHandler<GetDevicesQuery, DeviceResume>
{
    public async ValueTask<QueryResult<DeviceResume>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var username = sessionMetadataService.GetCurrentUsername();
        var results = await repository.AllAsync<DeviceResume>(p
            => p.IsActive && p.User!.Username == username , cancellationToken);
        return new(results.ToArray());
    }
}