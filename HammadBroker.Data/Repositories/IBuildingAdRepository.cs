using essentialMix.Core.Data.Entity.Patterns.Repository;
using HammadBroker.Data.Context;
using HammadBroker.Model.Entities;

namespace HammadBroker.Data.Repositories;

public interface IBuildingAdRepository : IRepository<DataContext, BuildingAd, int>
{
}