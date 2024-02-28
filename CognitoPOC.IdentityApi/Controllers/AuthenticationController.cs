using System.Net.Mime;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Infrastructure.Middleware;

namespace CognitoPOC.IdentityApi.Controllers;

/// <summary>
/// Authentication Workflows
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Main Constructor
    /// </summary>
    /// <param name="mediator">Mediator Service</param>
    public AuthenticationController(IMediator mediator) => _mediator = mediator;

    [SwaggerDeviceAuth]
    [HttpPost("SignIn")]
    public ValueTask<OperationResultValue<AuthorizationValue>> SignIn(SignInCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
    [HttpPost("Verify")]
    public ValueTask<OperationResultValue<AuthorizationValue>> Verify(VerifyChallengeCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
    [SwaggerDeviceAuth]
    [HttpPost("Refresh")]
    public ValueTask<OperationResultValue<AuthorizationValue>> Refresh(RefreshSessionCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
    [HttpPost("SignOut")]
    public ValueTask<OperationResultValue> SignOut(SignOutCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
}