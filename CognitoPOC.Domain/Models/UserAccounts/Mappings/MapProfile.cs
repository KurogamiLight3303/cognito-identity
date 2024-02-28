using AutoMapper;
using CognitoPOC.Domain.Models.UserAccounts.DomainValues;
using CognitoPOC.Domain.Models.UserAccounts.Resume;
using CognitoPOC.Domain.Extensions;

namespace CognitoPOC.Domain.Models.UserAccounts.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<UserAccountObject, UserAccountBaseResume>()
            .ForMember(p => p.Email, p
                => p.MapFrom(s => s.Email.Value ?? string.Empty))
            .ForMember(p => p.Phone, p
                => p.MapFrom(s => s.PhoneNumber.Value != null
                    ? s.PhoneNumber.Value.FormatPhone() 
                    : string.Empty))
            ;
        CreateProjection<UserAccountObject, UserAccountResume>()
            .ForMember(p => p.Email, p
                => p.MapFrom(s => s.Email.Value ?? string.Empty))
            .ForMember(p => p.Phone, p
                => p.MapFrom(s => s.PhoneNumber.Value != null
                    ? s.PhoneNumber.Value.FormatPhone() 
                    : string.Empty))
            .ForMember(p => p.PhoneConfirmed, p
                => p.MapFrom(s => s.PhoneNumber.Verified))
            .ForMember(p => p.PreviewImageId, p
                => p.MapFrom(s => s.ProfilePicture))
            ;

        CreateProjection<UserDevicesObject, DeviceResume>()
            .ForMember(p => p.Name, p
                => p.MapFrom(s => s.DeviceInfo != null
                    ? s.DeviceInfo.Name
                    : string.Empty))
            .ForMember(p => p.UserAgent, p
                => p.MapFrom(s => s.DeviceInfo != null
                    ? s.DeviceInfo.UserAgent
                    : UserAgentEnum.Unknown))
            .ForMember(p => p.IpAddress, p
                => p.MapFrom(s => s.DeviceInfo != null ? s.DeviceInfo.IpAddress : string.Empty))
            ;
    }
}