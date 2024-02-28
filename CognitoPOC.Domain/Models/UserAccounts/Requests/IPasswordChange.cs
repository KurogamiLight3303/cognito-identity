namespace CognitoPOC.Domain.Models.UserAccounts.Requests;

public interface IPasswordChange : IPasswordUpdate
{
    public string? OldPassword { get; }
}