using System.ComponentModel;

namespace CognitoPOC.Domain.Common.Models;

public class PagedFilteredRequestValue : PagedRequestValue
{
    [DefaultValue(null)] public List<QueryFilterValue>? Filters { get; set; }
    [DefaultValue(null)] public List<QueryOrderValue>? Orders { get; set; }
}