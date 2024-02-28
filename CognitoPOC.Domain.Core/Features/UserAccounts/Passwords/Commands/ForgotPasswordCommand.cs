using CognitoPOC.Domain.Core.Common.Commands;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Commands;

public class ForgotPasswordCommand : CommandBase
{
    public string? Alias { get; init; }
}