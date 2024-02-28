using CognitoPOC.Domain.Models.UserAccounts.DomainValues;

namespace CognitoPOC.Infrastructure.Extensions;

public static class CognitoExtensions
{
    public static string GetCognitoChallengeName(this ChallengeTypeEnum challenge)
    {
        return challenge switch
        {
            ChallengeTypeEnum.NewPasswordRequired => "NEW_PASSWORD_REQUIRED",
            ChallengeTypeEnum.PhoneMessageChallenge => "SMS_MFA",
            ChallengeTypeEnum.SoftwareTokenChallenge => "SOFTWARE_TOKEN_MFA",
            ChallengeTypeEnum.SelectMfaChallenge => "SELECT_MFA_TYPE",
            _ => throw new NotImplementedException()
        };
    }

    public static ChallengeTypeEnum GetChallengeType(this string? challenge)
    {
        return challenge switch
        {
            "SELECT_MFA_TYPE" => ChallengeTypeEnum.SelectMfaChallenge,
            "SMS_MFA" => ChallengeTypeEnum.PhoneMessageChallenge,
            "SOFTWARE_TOKEN_MFA" => ChallengeTypeEnum.SoftwareTokenChallenge,
            "NEW_PASSWORD_REQUIRED" => ChallengeTypeEnum.NewPasswordRequired,
            _ => throw new NotImplementedException()
        };
    }
}