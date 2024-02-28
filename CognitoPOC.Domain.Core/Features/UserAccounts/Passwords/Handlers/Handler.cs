using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Commands;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Handlers;

public class Handler : 
    ICommandHandler<ChangePasswordCommand>,
    ICommandHandler<ForgotPasswordCommand>,
    ICommandHandler<ResetPasswordCommand>
{
    private readonly IAuthenticationPasswordService _authenticationService;

    public Handler(IAuthenticationPasswordService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public ValueTask<OperationResultValue> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        => _authenticationService.ChangePasswordAsync(request, cancellationToken);

    public ValueTask<OperationResultValue> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        => _authenticationService.ForgotPasswordAsync(request.Alias, cancellationToken);

    public ValueTask<OperationResultValue> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        => _authenticationService.ResetPasswordAsync(request, cancellationToken);
}