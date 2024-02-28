using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Device.Commands;

public class ConfirmDeviceCommand : CommandBase
{
    public bool RememberDevice { get; init; }
}