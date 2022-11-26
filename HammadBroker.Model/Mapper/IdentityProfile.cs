using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;

namespace HammadBroker.Model.Mapper;

public class IdentityProfile : Profile
{
	public IdentityProfile()
	{
		CreateMap<UserList, ListSettings>()
			.ReverseMap();

		CreateMap<UserToUpdateCore, User>()
			.ReverseMap();
		CreateMap<UserToUpdate, User>()
			.IncludeBase<UserToUpdateCore, User>()
			.ReverseMap();
		CreateMap<UserData, User>()
			.IncludeBase<UserToUpdateCore, User>()
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