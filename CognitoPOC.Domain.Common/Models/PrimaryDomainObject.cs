namespace CognitoPOC.Domain.Common.Models;

public abstract  class PrimaryDomainObject<TKey> : DomainObject<TKey>
{
    public bool IsActive { get; set; }
}

public abstract class PrimaryDomainObject : PrimaryDomainObject<Guid>
{
}

