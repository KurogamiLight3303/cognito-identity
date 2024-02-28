using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Handlers;

public class SignInCommandHandler(IAuthenticationSignInService authenticationSignInService) :
    ICommandHandler<SignInCommand, AuthorizationValue>,
    ICommandHandler<VerifyChallengeCommand, AuthorizationValue>
{
    public ValueTask<OperationResultValue<AuthorizationValue>> Handle(SignInCommand request, 
        CancellationToken cancellationToken)
        => authenticationSignInService.SignInAsync(request.Alias, request.Password, cancellationToken);

    public ValueTask<OperationResultValue<AuthorizationValue>> Handle(VerifyChallengeCommand request, CancellationToken cancellationToken)
        => authenticationSignInService.VerifyChallengeAsync(request, cancellationToken);
}