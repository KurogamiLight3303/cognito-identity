namespace CognitoPOC.Domain.Models.UserAccounts.DomainValues;

public class AuthChallengeValue
{
    public ChallengeTypeEnum Type { get; init; }
    public string? Session { get; init; }
    public Dictionary<ChallengeTypeEnum, string>? Options { get; init; }
}

public enum ChallengeTypeEnum
{
    SessionExpire = 0,
    NewPasswordRequired = 1,
    SelectMfaChallenge = 2,
    PhoneMessageChallenge = 3,
    SoftwareTokenChallenge = 4,
}