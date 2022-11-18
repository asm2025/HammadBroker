using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;

namespace HammadBroker.Model.Mapper;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<UserList, ListSettings>()
            .ReverseMap();

        CreateMap<UserToUpdate, ApplicationUser>()
            .ReverseMap();

        CreateMap<ApplicationUser, UserForLoginDisplay>()
            .ReverseMap();
        CreateMap<ApplicationUser, UserForList>()
            .ReverseMap();
        CreateMap<ApplicationUser, UserForDetails>()
            .ReverseMap();
    }
}