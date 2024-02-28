namespace CognitoPOC.Domain.Models.UserAccounts.Requests;

public interface IUpdateProfile
{
    public string? Firstname { get; }
    public string? LastName { get;  }
    public Guid? ProfilePicture { get; }
    public string? Picture { get; init; }
    public string? PreferredLanguage { get; }
}