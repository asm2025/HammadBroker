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
	}
}