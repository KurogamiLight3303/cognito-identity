namespace CognitoPOC.Domain.Common.Models;

public abstract class DomainValue
{
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (DomainValue)obj;
        return GetCompareFields().SequenceEqual(other.GetCompareFields());
    }

    public override int GetHashCode()
        => GetCompareFields().GetHashCode();

    protected abstract IEnumerable<object?> GetCompareFields();
}