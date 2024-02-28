using System.Net.Mime;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Commands;

namespace CognitoPOC.IdentityApi.Controllers;

/// <summary>
/// Password Workflows
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Main Constructor
    /// </summary>
    /// <param name="mediator">Mediator Service</param>
    public PasswordController(IMediator mediator) => _mediator = mediator;
    [HttpPost("Reset")]
    public ValueTask<OperationResultValue> Reset(ResetPasswordCommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
    [HttpPost("Forgot")]
    public ValueTask<OperationResultValue> Forgot(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
    [Authorize]
    [HttpPost("Change")]
    public ValueTask<OperationResultValue> Change(ChangePasswordCommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
}