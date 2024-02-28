using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts.DomainValues;

public class AuthorizationValue : DomainValue
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? Username { get; set; }
    public AuthChallengeValue? Challenge { get; set; }
    public int ExpiresIn { get; set; }
    public Guid? Device { get; set; }
    public bool RequestConfirmation { get; set; }

    public string[]? Roles { get; set; }

    protected override IEnumerable<object?> GetCompareFields()
    {
        yield return Username;
        yield return AccessToken;
        yield return RefreshToken;
        yield return Challenge;
    }
}