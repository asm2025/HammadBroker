using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface ILookupService : IServiceBase<DataContext>
{
	[NotNull]
	Task<IList<string>> ListBuildingImagesAsync(string id, int count, CancellationToken token = default(CancellationToken));

	[NotNull]
	Task<IList<CityForList>> ListCitiesAsync([NotNull] CitiesList settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	[ItemNotNull]
	IList<string> ListBuildingTypes();
	[NotNull]
	[ItemNotNull]
	IList<string> ListFinishingTypes();
	[NotNull]
	Task FillCityNameAsync([NotNull] ICityNameLookup lookup, CancellationToken token = default(CancellationToken));
}