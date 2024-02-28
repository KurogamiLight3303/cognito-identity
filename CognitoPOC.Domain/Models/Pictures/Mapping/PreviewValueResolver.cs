using AutoMapper;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Models.Pictures.Mapping;

public class PreviewValueResolver(IResourceService resourceService) :
    IValueConverter<string?, string?>
{
    public string? Convert(string? sourceMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(sourceMember)) return null;
        return resourceService.GetPicture(sourceMember, PictureSizeEnum.Small);
    }
}