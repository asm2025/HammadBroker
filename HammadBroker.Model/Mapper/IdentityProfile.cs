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

		CreateMap<UserToUpdate, User>()
			.ReverseMap();

		CreateMap<UserData, UserToUpdate>()
			.ReverseMap();
		CreateMap<UserData, User>()
			.IncludeBase<UserToUpdate, User>()
			.ReverseMap();

		CreateMap<User, UserForLogin>()
			.ReverseMap();
		CreateMap<User, UserForList>()
			.IncludeBase<User, UserForLogin>()
			.ReverseMap();
		CreateMap<User, UserForDetails>()
			.IncludeBase<User, UserForList>()
			.ReverseMap();
	}
}