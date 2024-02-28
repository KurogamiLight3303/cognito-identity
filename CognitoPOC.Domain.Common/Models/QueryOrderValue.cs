namespace CognitoPOC.Domain.Common.Models;

public class QueryOrderValue
{
    public string? Alias { get; set; }
    public QueryOrderTypeEnum Order { get; set; }
}

public enum QueryOrderTypeEnum
{
    Desc = 0,
    Asc = 1
}