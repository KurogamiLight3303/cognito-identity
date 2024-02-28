using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts.Resume;

public class UserAccountBaseResume : DomainResume<UserAccountObject>
{
    public bool IsActive { get; init; }
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateTime CreatedDate { get; init; }
}