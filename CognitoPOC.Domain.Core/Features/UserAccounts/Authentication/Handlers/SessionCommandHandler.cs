using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Services;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Handlers;

public class SessionCommandHandler(IAuthenticationSessionService authenticationService) :
    ICommandHandler<RefreshSessionCommand, AuthorizationValue>,
    ICommandHandler<SignOutCommand>
{
    public ValueTask<OperationResultValue<AuthorizationValue>> Handle(RefreshSessionCommand request,
        CancellationToken cancellationToken)
        => authenticationService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

    public ValueTask<OperationResultValue> Handle(SignOutCommand request, CancellationToken cancellationToken)
        => authenticationService.SignOutAsync(request.Token, cancellationToken);
}