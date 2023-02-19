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
			})
			.IncludeAllDerived();
		CreateMap<BuildingForList, Building>()
			.ForMember(d => d.Date, m => m.MapFrom(s => s.Date.ToUniversalTime()))
			.ForMember(d => d.Expires, m =>
			{
				m.PreCondition(c => c.Expires.HasValue);
				m.MapFrom(s => s.Expires.Value.ToUniversalTime());
			})
			.IncludeAllDerived();

		CreateMap<Building, BuildingForDisplay>()
			.IncludeAllDerived()
			.ReverseMap();

		CreateMap<Building, BuildingForDetails>()
			.IncludeAllDerived()
			.ReverseMap();

		CreateMap<Building, BuildingToUpdate>()
			.ReverseMap();

		CreateMap<BuildingImage, BuildingImageForList>()
			.ReverseMap();
	}
}