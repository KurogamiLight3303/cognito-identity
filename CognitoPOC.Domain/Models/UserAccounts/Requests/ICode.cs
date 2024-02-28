namespace CognitoPOC.Domain.Models.UserAccounts.Requests;

public interface ICode
{
    public string? Alias { get; }
    public string? Code { get; }
}