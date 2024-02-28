namespace CognitoPOC.Domain.Models.UserAccounts.DomainValues;

public enum MfaTypesEnum : short
{
    None = 0,
    Sms = 1,
    SoftwareToken = 2,
    SelectAtSignIn = 99
}