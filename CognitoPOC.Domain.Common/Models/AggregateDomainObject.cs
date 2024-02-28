namespace CognitoPOC.Domain.Common.Models;

public class AggregateDomainObject<TParentKey, TChildrenKey, TChildren> : PrimaryDomainObject<TParentKey>
    where TChildren : SecondaryDomainObject<TChildrenKey>
{
    public List<TChildren> Items { get; private set; } = new();
}