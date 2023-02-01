using AutoMapper;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;

namespace HammadBroker.Model.Mapper;

public class BuildingAdProfile : Profile
{
	public BuildingAdProfile()
	{
		CreateMap<Building, BuildingAdForList>()
			.ForMember(e => e.Id, p => p.Ignore());
		CreateMap<BuildingAd, BuildingAdForList>();

		CreateMap<Building, BuildingAdForDetails>()
			.ForMember(e => e.Id, p => p.Ignore());
		CreateMap<BuildingAd, BuildingAdForDetails>();
		CreateMap<BuildingAdForList, BuildingAdForDetails>()
			.ReverseMap();

		CreateMap<BuildingAd, BuildingAdToUpdate>()
			.ReverseMap();
	}
}