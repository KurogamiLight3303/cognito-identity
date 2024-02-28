using CognitoPOC.Domain.Common;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Device.Commands;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Device.Handlers;

public class ForgetCommandHandler(
    IUserDeviceRepository repository,
    IUnitOfWork unitOfWork,
    ISessionMetadataService sessionMetadataService)
    :
        ICommandHandler<ForgetDeviceCommand>
{
    public async ValueTask<OperationResultValue> Handle(ForgetDeviceCommand request, CancellationToken cancellationToken)
    {
        var username = sessionMetadataService.GetCurrentUsername();
        return await unitOfWork.ExecuteInTransactionAsync<OperationResultValue>(async (cc) =>
        {
            var device = await repository.FindAsync(p
                => p.Id == request.DeviceId && p.User!.Username == username, cc);
            if (device == null)
                return new(false, I18n.UnknownError);
            device.Forgot();
            repository.Update(device);
            return await unitOfWork.CommitAsync(cc)
                ? new(true, "")
                : new(false, I18n.UnknownError);
        }, cancellationToken);
        
    }
}