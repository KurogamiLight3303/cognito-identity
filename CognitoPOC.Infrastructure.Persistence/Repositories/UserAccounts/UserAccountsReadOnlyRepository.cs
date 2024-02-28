using System.Linq.Expressions;
using AutoMapper;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.Common;
using CognitoPOC.Domain.Models.Pictures;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.Repositories.UserAccounts;

public class UserAccountsReadOnlyRepository : UserAccountsRepository, IUserAccountQueryRepository
{
    private readonly IResourceService _resourceService;
    public UserAccountsReadOnlyRepository(DomainContext context, IResourceService resourceService, IMapper? mapper,
        IQueryFilterTranslator<UserAccountObject, Guid>? filterTranslator) 
        : base(context, mapper, filterTranslator)
    {
        _resourceService = resourceService;
        ReadOnly = true;
    }

    public new async Task<TProjectedValue?> FindAsync<TProjectedValue>(Expression<Func<UserAccountObject, bool>> condition,
        CancellationToken cancellationToken = default) where TProjectedValue : DomainResume<UserAccountObject>
    {
        var data = await base.FindAsync<TProjectedValue>(condition, cancellationToken);
        UpdateResources(data as IPreviewImage);
        return data;
    }
    private void UpdateResources(IPreviewImage? data)
    {
        if(data != null)
            data.PreviewImageUrl = _resourceService.GetPicture(data.PreviewImageId, PictureSizeEnum.Base);
    }
}