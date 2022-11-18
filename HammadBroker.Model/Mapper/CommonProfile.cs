using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;

namespace HammadBroker.Model.Mapper;

public class CommonProfile : Profile
{
    public CommonProfile()
    {
        CreateMap<SortablePagination, ListSettings>()
            .ReverseMap();
        CreateMap<Country, CountryForList>();
        CreateMap<City, CityForList>();
    }
}