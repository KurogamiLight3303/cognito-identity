namespace CognitoPOC.Domain.Models.UserAccounts.Requests;

public interface ISignUp
{
    public string? Email { get;}
    public string? FirstName { get; }
    public string? LastName { get; }
    public string? Password { get; }
}