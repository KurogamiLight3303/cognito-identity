using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Models.UserAccounts.QueryFilters;

public class UserAccountQueryFilterTranslator : IQueryFilterTranslator<UserAccountObject, Guid>
{
    public Task<IQueryable<UserAccountObject>> AddFiltersAsync(IQueryable<UserAccountObject> query, 
        IEnumerable<QueryFilterValue> filters, CancellationToken cancellationToken)
    {
        foreach (var f in filters)
        {
            switch (f.Alias)
            {
                case "IsActive":
                    if (!string.IsNullOrEmpty(f.Value))
                    {
                        var v = f.Value.ToLower() == "true";
                        query = query.Where(p => p.IsActive == v);
                    }
                    break;
                case "QuickSearch":
                    if(!string.IsNullOrEmpty(f.Value))
                        query = query.Where(p => (p.Username != null && p.Username.StartsWith(f.Value)) 
                                                 || (p.FirstName != null && p.FirstName.StartsWith(f.Value)));
                    break;
                case "StartCreatedDate":
                    if (!string.IsNullOrEmpty(f.Value) && DateTime.TryParse(f.Value, out var startDate))
                        query = query.Where(p => p.CreatedDate != null && p.CreatedDate >= startDate);
                    break;
                case "EndCreatedDate":
                    if (!string.IsNullOrEmpty(f.Value) && DateTime.TryParse(f.Value, out var endDate))
                        query = query.Where(p => p.CreatedDate != null && p.CreatedDate <= endDate);
                    break;
            }
        }

        return Task.FromResult(query);
    }

    public Task<IQueryable<UserAccountObject>> AddOrderAsync(IQueryable<UserAccountObject> query, IEnumerable<QueryOrderValue> filters, CancellationToken cancellationToken)
    {
        foreach (var f in filters)
        {
            switch (f.Alias)
            {
                case "FirstName":
                    query = f.Order == QueryOrderTypeEnum.Asc
                        ? query.OrderBy(p => p.FirstName)
                        : query.OrderByDescending(p => p.FirstName);
                    break;
                case "LastName":
                    query = f.Order == QueryOrderTypeEnum.Asc
                        ? query.OrderBy(p => p.LastName)
                        : query.OrderByDescending(p => p.LastName);
                    break;
                case "CreatedDate":
                    query = f.Order == QueryOrderTypeEnum.Asc
                        ? query.OrderBy(p => p.CreatedDate)
                        : query.OrderByDescending(p => p.CreatedDate);
                    break;
                case "Email":
                    query = f.Order == QueryOrderTypeEnum.Asc
                        ? query.OrderBy(p => p.Email.Value)
                        : query.OrderByDescending(p => p.Email.Value);
                    break;
                case "Phone":
                    query = f.Order == QueryOrderTypeEnum.Asc
                        ? query.OrderBy(p => p.PhoneNumber.Value)
                        : query.OrderByDescending(p => p.PhoneNumber.Value);
                    break;
            }
        }
        return Task.FromResult(query); 
    }
}