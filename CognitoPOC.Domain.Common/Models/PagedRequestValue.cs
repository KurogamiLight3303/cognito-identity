using System.ComponentModel;

namespace CognitoPOC.Domain.Common.Models;

public class PagedRequestValue : DomainValue
{
    private int _pageNo;
    private int _pageSize;

    [DefaultValue(0)]
    public int PageNo
    {
        get => _pageNo;
        set => _pageNo = value > 0 ? value : 0;
    }
    
    [DefaultValue(5)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value >= 5 ? value : 5;
    }
    protected override IEnumerable<object> GetCompareFields()
    {
        yield return _pageNo;
        yield return _pageSize;
    }
}