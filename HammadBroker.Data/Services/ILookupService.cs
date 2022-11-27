using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HammadBroker.Data.Context;
using HammadBroker.Model.DTO;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface ILookupService : IServiceBase<DataContext>
{
    [NotNull]
    [ItemNotNull]
    Task<IList<CityForList>> ListCitiesAsync([NotNull] string countryCode, CancellationToken token = default(CancellationToken));
    [NotNull]
    [ItemNotNull]
    Task<IList<CountryForList>> ListCountriesAsync(CancellationToken token = default(CancellationToken));
    [NotNull]
    [ItemNotNull]
    IList<string> ListBuildingTypes(CancellationToken token = default(CancellationToken));
    [NotNull]
    [ItemNotNull]
    IList<string> ListFinishingTypes(CancellationToken token = default(CancellationToken));
}