using AutoMapper;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;

namespace HammadBroker.Model.Mapper;

public class BuildingProfile : Profile
{
	public BuildingProfile()
	{
		CreateMap<Building, BuildingForList>()
			.ForMember(d => d.Date, m => m.MapFrom(s => s.Date.ToLocalTime()))
			.ForMember(d => d.Expires, m =>
			{
				m.PreCondition(c => c.Expires.HasValue);
				m.MapFrom(s => s.Expires.Value.ToLocalTime());
			});
		CreateMap<BuildingForList, Building>()
			.ForMember(d => d.Date, m => m.MapFrom(s => s.Date.ToUniversalTime()))
			.ForMember(d => d.Expires, m =>
			{
				m.PreCondition(c => c.Expires.HasValue);
				m.MapFrom(s => s.Expires.Value.ToUniversalTime());
			});

		CreateMap<Building, BuildingForDisplay>()
			.IncludeBase<Building, BuildingForList>();
		CreateMap<BuildingForDisplay, Building>()
			.IncludeBase<BuildingForList, Building>();

		CreateMap<Building, BuildingForDetails>()
			.IncludeBase<Building, BuildingForDisplay>();
		CreateMap<BuildingForDetails, Building>()
			.IncludeBase<BuildingForDisplay, Building>();

		CreateMap<Building, BuildingToUpdate>()
			.ReverseMap();

		CreateMap<BuildingImage, BuildingImageForList>()
			.ReverseMap();
	}
}