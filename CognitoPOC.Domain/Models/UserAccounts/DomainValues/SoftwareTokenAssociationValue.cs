using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts.DomainValues;

public class SoftwareTokenAssociationValue : DomainValue
{
    public SoftwareTokenAssociationValue(string code)
    {
        SecretCode = code;
    }
    public string SecretCode { get; }
    protected override IEnumerable<object?> GetCompareFields()
    {
        yield return SecretCode;
    }
}