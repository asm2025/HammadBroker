using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using HammadBroker.Data.Context;
using HammadBroker.Model.DTO;
using JetBrains.Annotations;

namespace HammadBroker.Data.Services;

public interface ILookupService : IServiceBase<DataContext>
{
	[NotNull]
	[ItemNotNull]
	Task<IList<CountryForList>> ListCountriesAsync(CancellationToken token = default(CancellationToken));
	[NotNull]
	[ItemNotNull]
	IList<string> ListBuildingTypes();
	[NotNull]
	[ItemNotNull]
	IList<string> ListFinishingTypes();

	[NotNull]
	Task FillCountriesAsync([NotNull] ICountryLookup lookup, CancellationToken token = default(CancellationToken));

	[NotNull]
	Task FillCitiesAsync([NotNull] ICityLookup lookup, CancellationToken token = default(CancellationToken));

	[NotNull]
	Task FillCountryNameAsync([NotNull] ICountryNameLookup lookup, CancellationToken token = default(CancellationToken));

	[NotNull]
	Task FillCityNameAsync([NotNull] ICityNameLookup lookup, CancellationToken token = default(CancellationToken));

	[NotNull]
	Task FillBuildingImagesAsync(int id, [NotNull] IBuildingImagesLookup lookup, int count, CancellationToken token = default(CancellationToken));
}