using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Device.Commands;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Device.Handlers;

public class CommandHandler(IAuthenticationSignInService authenticationSignInService) :
    ICommandHandler<ConfirmDeviceCommand>,
    ICommandHandler<RememberDeviceCommand>
{
    public ValueTask<OperationResultValue> Handle(ConfirmDeviceCommand request, CancellationToken cancellationToken)
        => authenticationSignInService.ConfirmDeviceAsync(request.RememberDevice, cancellationToken);

    public ValueTask<OperationResultValue> Handle(RememberDeviceCommand request, CancellationToken cancellationToken)
        => authenticationSignInService.RememberDeviceAsync(request.RememberDevice, cancellationToken);
}