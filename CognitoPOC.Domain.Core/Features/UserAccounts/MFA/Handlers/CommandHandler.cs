using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.MFA.Commands;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.MFA.Handlers;

public class CommandHandler(IAuthenticationTotpService authenticationService) :
    ICommandHandler<AssociateSoftwareTokenCommand, SoftwareTokenAssociationValue>,
    ICommandHandler<VerifySoftwareTokenCommand>
{
    public ValueTask<OperationResultValue<SoftwareTokenAssociationValue>> Handle(AssociateSoftwareTokenCommand request,
        CancellationToken cancellationToken) => authenticationService.AssociateSoftwareToken(cancellationToken);

    public ValueTask<OperationResultValue> Handle(VerifySoftwareTokenCommand request,
        CancellationToken cancellationToken) =>
        authenticationService.VerifySoftwareToken(request.Code, cancellationToken);
}