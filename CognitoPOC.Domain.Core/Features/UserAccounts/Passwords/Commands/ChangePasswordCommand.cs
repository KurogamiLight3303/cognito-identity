using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Models.UserAccounts.Requests;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Commands;

public class ChangePasswordCommand : CommandBase, IPasswordChange
{
    public string? OldPassword { get; init; }
    public string? NewPassword { get; init; }
}