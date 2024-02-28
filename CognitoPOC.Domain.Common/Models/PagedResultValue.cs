namespace CognitoPOC.Domain.Common.Models;

public class PagedResultValue<TItem> : QueryResult<TItem>
{
    public PagedResultValue(string? message, PagedRequestValue parameters, long count) : base(message)
    {
        PageNo = parameters.PageNo;
        PageSize = parameters.PageSize;
        Total = count;
    }
    public PagedResultValue(TItem[] items, PagedRequestValue parameters, long count) : base(items)
    {
        PageNo = parameters.PageNo;
        PageSize = parameters.PageSize;
        Total = count;
    }
    public int PageSize { get; }
    public int PageNo { get; }
    public long Total { get; }
    public long TotalPages => Total > 0 ? Total / PageSize + (Total % PageSize > 0 ? 1 : 0) : 0;
}