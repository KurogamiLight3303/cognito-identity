using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Device.Commands;

public class ForgetDeviceCommand : CommandBase
{
    public Guid DeviceId { get; set; }
}