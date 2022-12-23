using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using essentialMix.Core.Data.Entity.AutoMapper.Patterns.Services;
using essentialMix.Extensions;
using essentialMix.Helpers;
using HammadBroker.Data.Context;
using HammadBroker.Model;
using HammadBroker.Model.DTO;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class LookupService : ServiceBase<DataContext>, ILookupService
{
	public LookupService([NotNull] DataContext context, [NotNull] IMapper mapper, [NotNull] ILogger<LookupService> logger)
		: base(context, mapper, logger)
	{
	}

	/// <inheritdoc />
	public Task<IList<CountryForList>> ListCountriesAsync(CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return Context.Countries
					.AsNoTracking()
					.OrderBy(e => e.Name)
					.ProjectTo<CountryForList>(Mapper.ConfigurationProvider)
					.ToListAsync(token)
					.As<List<CountryForList>, IList<CountryForList>>(token);
	}

	/// <inheritdoc />
	public IList<string> ListBuildingTypes(CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return EnumHelper<BuildingType>
				.GetDisplayNames()
				.ToList();
	}

	/// <inheritdoc />
	public IList<string> ListFinishingTypes(CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return EnumHelper<FinishingType>
				.GetDisplayNames()
				.ToList();
	}
}