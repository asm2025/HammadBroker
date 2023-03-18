using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;

namespace HammadBroker.Model.Mapper;

public class CommonProfile : Profile
{
	public CommonProfile()
	{
		CreateMap<SortablePagination, ListSettings>()
			.ReverseMap();

		CreateMap<SearchList, SortablePagination>()
			.ReverseMap();

		CreateMap<DistrictList, SortablePagination>()
			.ReverseMap();
		CreateMap<DistrictList, SearchList>()
			.ReverseMap();

		CreateMap<District, DistrictForList>()
			.ReverseMap();
		CreateMap<District, DistrictToUpdate>()
			.ReverseMap();

		CreateMap<City, CityForList>()
			.ReverseMap();
		CreateMap<City, CityToUpdate>()
			.ReverseMap();
	}
}