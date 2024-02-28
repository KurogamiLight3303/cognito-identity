using AutoMapper;

namespace CognitoPOC.Domain.Models.Pictures.Mapping;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<PictureObject, PictureResume>()
            .ForMember(p => p.UploadDate, p
                => p.MapFrom(s => s.CreatedDate))
            .ForMember(p => p.PreviewUrl, p
                => p.ConvertUsing<PreviewValueResolver, string?>(s => s.Id.ToString()))
            ;
    }
}