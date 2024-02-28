namespace CognitoPOC.Infrastructure.Configurations;

public class CognitoConfiguration
{
    public string? Domain { get; init; }
    public string? CognitoUserPoolId { get; init; }
    public string? CognitoUserPoolAppId { get; init; }
    public string? CognitoUserPoolAppSecret { get; init; }
    public string? PasswordSecret { get; init; }
}