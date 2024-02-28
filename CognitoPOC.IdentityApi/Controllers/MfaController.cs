using System.Net.Mime;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Features.UserAccounts.MFA.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.IdentityApi.Controllers;

/// <summary>
/// Authentication Workflows
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Main Constructor
    /// </summary>
    /// <param name="mediator">Mediator Service</param>
    public MfaController(IMediator mediator) => _mediator = mediator;
    [HttpPost("AssociateSoftwareToken")]
    public ValueTask<OperationResultValue<SoftwareTokenAssociationValue>> Associate(CancellationToken cancellationToken)
        => _mediator.Send(new AssociateSoftwareTokenCommand(), cancellationToken);

    [HttpPost("VerifySoftwareToken")]
    public ValueTask<OperationResultValue> Verify(VerifySoftwareTokenCommand command,
        CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);

    [HttpPost("UpdateMfaPreference")]
    public ValueTask<OperationResultValue> Update(UpdateMfaPreferenceCommand command,
        CancellationToken cancellationToken)
        => _mediator.Send(command, cancellationToken);
}