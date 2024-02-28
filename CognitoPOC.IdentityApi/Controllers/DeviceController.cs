using System.Net.Mime;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Features.UserAccounts.Device.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Device.Queries;
using CognitoPOC.Domain.Models.UserAccounts.Resume;
using CognitoPOC.Infrastructure.Middleware;

namespace CognitoPOC.IdentityApi.Controllers;

/// <summary>
/// Authentication Workflows
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Main Constructor
    /// </summary>
    /// <param name="mediator">Mediator Service</param>
    public DeviceController(IMediator mediator) => _mediator = mediator;
    [SwaggerDeviceAuth]
    [HttpPost("Confirm")]
    public ValueTask<OperationResultValue> Confirm(ConfirmDeviceCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
    [HttpPost("Remember")]
    public ValueTask<OperationResultValue> Remember(RememberDeviceCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
    [HttpGet("GetDevices")]
    public ValueTask<QueryResult<DeviceResume>> GetDevices(CancellationToken cancellationToken)
        => _mediator.Send(new GetDevicesQuery(), cancellationToken);
    
    [HttpPost("Forget")]
    public ValueTask<OperationResultValue> Forget(ForgetDeviceCommand command,CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
}