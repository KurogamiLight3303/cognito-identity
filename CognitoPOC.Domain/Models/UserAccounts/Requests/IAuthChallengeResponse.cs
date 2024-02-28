using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Domain.Models.UserAccounts.Requests;

public interface IAuthChallengeResponse
{
    public ChallengeTypeEnum Type { get;}
    public string? Session { get; }
    public string? Username { get; }
    public string? ChallengeValue { get; }
}