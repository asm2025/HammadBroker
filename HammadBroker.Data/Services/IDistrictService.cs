using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;

namespace HammadBroker.Data.Services;

public interface IDistrictService : IService<DataContext, IDistrictRepository, District, int>
{
}