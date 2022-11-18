using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.Entities;

namespace HammadBroker.Data.Services;

public interface IBuildingService : IService<DataContext, IBuildingRepository, Building, long>
{
}