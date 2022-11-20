using AutoMapper;
using HammadBroker.Extensions;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;

namespace HammadBroker.Model.Mapper;

public class BuildingProfile : Profile
{
	public BuildingProfile()
	{
		CreateMap<Building, BuildingForList>()
			.ReverseMap();
		CreateMap<BuildingImage, BuildingForList>()
			.IgnoreAll()
			.ForMember(e => e.ImageUrl, p => p.MapFrom(e => e.ImageUrl));
		CreateMap<City, BuildingForList>()
			.IgnoreAll()
			.ForMember(e => e.City, p => p.MapFrom(e => e.Name));
		CreateMap<Country, BuildingForList>()
			.IgnoreAll()
			.ForMember(e => e.Country, p => p.MapFrom(e => e.Name));

		CreateMap<Building, BuildingForDetails>()
			.IncludeBase<Building, BuildingForList>()
			.ReverseMap();
		CreateMap<BuildingImage, BuildingForDetails>()
			.IncludeBase<BuildingImage, BuildingForList>();
		CreateMap<City, BuildingForDetails>()
			.IncludeBase<City, BuildingForList>();
		CreateMap<Country, BuildingForDetails>()
			.IncludeBase<Country, BuildingForList>();

		CreateMap<Building, BuildingToUpdate>()
			.ReverseMap();

		CreateMap<BuildingImage, BuildingImageForList>()
			.ReverseMap();
		CreateMap<BuildingImage, BuildingImageToUpdate>()
			.ReverseMap();

		CreateMap<Building, BuildingAdForList>()
			.ForMember(e => e.Id, p => p.Ignore());
		CreateMap<BuildingAd, BuildingAdForList>();
		CreateMap<BuildingImage, BuildingAdForList>()
			.IncludeBase<BuildingImage, BuildingForList>();
		CreateMap<City, BuildingAdForList>()
			.IncludeBase<City, BuildingForList>();
		CreateMap<Country, BuildingAdForList>()
			.IncludeBase<Country, BuildingForList>();

		CreateMap<Building, BuildingAdForDetails>()
			.ForMember(e => e.Id, p => p.Ignore());
		CreateMap<BuildingAd, BuildingAdForDetails>();
		CreateMap<BuildingImage, BuildingAdForDetails>()
			.IncludeBase<BuildingImage, BuildingForDetails>();
		CreateMap<City, BuildingAdForDetails>()
			.IncludeBase<City, BuildingForDetails>();
		CreateMap<Country, BuildingAdForDetails>()
			.IncludeBase<Country, BuildingForDetails>();
	}
}