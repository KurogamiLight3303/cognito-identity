namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;

public class CreateUserCommand : BaseUpdateUserCommand
{
    public string? Password { get; set; }
}