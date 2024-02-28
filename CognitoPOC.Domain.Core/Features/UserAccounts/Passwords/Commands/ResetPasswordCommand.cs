using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.Requests;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Commands;

public class ResetPasswordCommand : CommandBase, IPasswordReset
{
    public string? NewPassword { get; init; }
    public string? Code { get; init; }
    public string? Alias { get;  init;}
}