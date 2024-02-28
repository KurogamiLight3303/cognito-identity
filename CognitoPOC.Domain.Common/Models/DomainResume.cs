namespace CognitoPOC.Domain.Common.Models;

public abstract class DomainResume<TDomainSource> where TDomainSource : DomainObject
{
    
}

public abstract class DomainValueResume<TDomainValue> where TDomainValue : DomainValue
{
    
}

public abstract class DomainResume<TDomainSource1, TDomainSource2> : DomainResume<TDomainSource1>
    where TDomainSource1 : DomainObject
    where TDomainSource2 : DomainObject
{
    
}