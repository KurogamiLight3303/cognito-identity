namespace CognitoPOC.Domain.Common.Models;

public abstract  class DomainObject<TKey> : DomainObject
{
    public TKey? Id { get; private set; }
}

public abstract class DomainObject
{
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}