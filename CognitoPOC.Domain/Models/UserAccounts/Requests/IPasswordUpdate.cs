namespace CognitoPOC.Domain.Models.UserAccounts.Requests;

public interface IPasswordUpdate
{
    public string? NewPassword { get; }
}