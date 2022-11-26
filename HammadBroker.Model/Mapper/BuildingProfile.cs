using AutoMapper;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;

namespace HammadBroker.Model.Mapper;

public class BuildingProfile : Profile
{
	public BuildingProfile()
	{
		CreateMap<Building, BuildingForList>()
			.ReverseMap();

		CreateMap<Building, BuildingForDetails>()
			.IncludeBase<Building, BuildingForList>()
			.ReverseMap();

		CreateMap<Building, BuildingToUpdate>()
			.ReverseMap();

		CreateMap<BuildingImage, BuildingImageForList>()
			.ReverseMap();

		CreateMap<Building, BuildingAdForList>()
			.ForMember(e => e.Id, p => p.Ignore());
		CreateMap<BuildingAd, BuildingAdForList>();

		CreateMap<Building, BuildingAdForDetails>()
			.ForMember(e => e.Id, p => p.Ignore());
		CreateMap<BuildingAd, BuildingAdForDetails>();
	}
}