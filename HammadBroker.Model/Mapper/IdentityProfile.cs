using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using HammadBroker.Extensions;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;

namespace HammadBroker.Model.Mapper;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<UserList, ListSettings>()
            .IncludeAllDerived()
            .ReverseMap();

        CreateMap<UserToUpdate, ApplicationUser>()
            .IncludeAllDerived()
            .ReverseMap();

        CreateMap<ApplicationUser, UserForLoginDisplay>()
            .IncludeAllDerived()
            .ReverseMap();
        CreateMap<ApplicationUser, UserForList>()
            .IncludeAllDerived()
            .ReverseMap();
        CreateMap<ApplicationUser, UserForDetails>()
            .ReverseMap();
        CreateMap<City, UserForDetails>()
            .IgnoreAll()
            .ForMember(e => e.City, p => p.MapFrom(d => d.Name));
        CreateMap<Country, UserForDetails>()
            .IgnoreAll()
            .ForMember(e => e.Country, p => p.MapFrom(d => d.Name));
    }
}