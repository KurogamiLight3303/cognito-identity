using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Commands;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Handlers;

public class CommandHandler(IAuthenticationProfileService authenticationService) :
    ICommandHandler<UpdatePhoneCommand>,
    ICommandHandler<VerifyPhoneCommand>,
    ICommandHandler<UpdateMfaPreferenceCommand>,
    ICommandHandler<ResendPhoneVerificationCommand>,
    ICommandHandler<UpdateProfileCommand>
{
    public ValueTask<OperationResultValue> Handle(UpdatePhoneCommand request, CancellationToken cancellationToken) 
        => authenticationService.UpdatePhone(request.Phone, cancellationToken);

    public ValueTask<OperationResultValue> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken) 
        => authenticationService.VerifyPhone(request.Code, cancellationToken);

    public ValueTask<OperationResultValue> Handle(ResendPhoneVerificationCommand request, 
        CancellationToken cancellationToken)
        => authenticationService.ResendPhoneVerificationCode(cancellationToken);
    public ValueTask<OperationResultValue> Handle(UpdateMfaPreferenceCommand request,
        CancellationToken cancellationToken) =>
        authenticationService.SetMfaPreference(request.DefaultMfa, cancellationToken);

    public ValueTask<OperationResultValue> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        => authenticationService.UpdateProfile(request, cancellationToken);
}