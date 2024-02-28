using System.Net.Mime;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Queries;
using CognitoPOC.Domain.Models.UserAccounts.Resume;

namespace CognitoPOC.IdentityApi.Controllers;

/// <summary>
/// Profile Workflows
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : Controller
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Main Constructor
    /// </summary>
    /// <param name="mediator">Mediator Service</param>
    public ProfileController(IMediator mediator) => _mediator = mediator;
    [HttpGet("Me")]
    public ValueTask<OperationResultValue<UserAccountResume>> Me(CancellationToken cancellationToken = default)
        => _mediator.Send(new GetProfileQuery(), cancellationToken);
    [HttpPost("Phone/Update")]
    public ValueTask<OperationResultValue> UpdatePhone(UpdatePhoneCommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
    [HttpPost("Phone/Resend")]
    public ValueTask<OperationResultValue> UpdatePhone(CancellationToken cancellationToken = default)
        => _mediator.Send(new ResendPhoneVerificationCommand(), cancellationToken);
    [HttpPost("Phone/Verify")]
    public ValueTask<OperationResultValue> VerifyPhone(VerifyPhoneCommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
    [HttpPost("Me/Update")]
    public ValueTask<OperationResultValue> UpdateProfile(UpdateProfileCommand command, CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
}