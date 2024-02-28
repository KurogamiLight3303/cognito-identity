namespace CognitoPOC.Domain.Models.UserAccounts;

public static class Roles
{
    public const string Administrator = "Administrator";

    public static string[] GetRoles()
    {
        return new[]
        {
            Administrator
        };
    }
}