using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Data.Repositories;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Services;

public class BuildingAdService : Service<DataContext, IBuildingAdRepository, BuildingAd, int>, IBuildingAdService
{
	public BuildingAdService([NotNull] IBuildingAdRepository repository, [NotNull] IMapper mapper, [NotNull] ILogger<BuildingAdService> logger)
		: base(repository, mapper, logger)
	{
	}

	/// <inheritdoc />
	public async Task<IPaginated<BuildingAdForList>> ListBuildingsAsync(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingAdForList> entities = Repository.DbSet
															.Join(Context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdForList
															{
																Id = ba.Id,
																BuildingId = ba.BuildingId,
																Date = ba.Date,
																Expires = ba.Expires,
																Phone = ba.Phone,
																Mobile = ba.Mobile,
																Price = ba.Price,
																Name = b.Name,
																BuildingType = b.BuildingType,
																ImageUrl = b.ImageUrl,
																FinishingType = b.FinishingType,
																CityId = b.CityId,
																CountryCode = b.CountryCode,
															})
															.Where(e => e.Expires == null || e.Expires >= DateTime.Today);

		if (settings is { PageSize: > 0 })
		{
			entities = entities.Paginate(settings);
			settings.Count = await Repository.DbSet
											.Where(e => e.Expires == null || e.Expires >= DateTime.Today)
											.CountAsync(token);
			token.ThrowIfCancellationRequested();
		}

		IList<BuildingAdForList> result = await entities.ToListAsync(token).ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new Paginated<BuildingAdForList>(result, settings);
	}

	/// <inheritdoc />
	public async Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		BuildingAdForDetails entity = await Repository.DbSet
													.Join(Context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdForDetails
													{
														Id = ba.Id,
														BuildingId = ba.BuildingId,
														Date = ba.Date,
														Expires = ba.Expires,
														Phone = ba.Phone,
														Mobile = ba.Mobile,
														Price = ba.Price,
														Name = b.Name,
														BuildingType = b.BuildingType,
														ImageUrl = b.ImageUrl,
														FinishingType = b.FinishingType,
														CityId = b.CityId,
														CountryCode = b.CountryCode,
														Floor = b.Floor,
														Location = b.Location,
														Address = b.Address,
														Address2 = b.Address2,
														Description = b.Description,
													})
													.FirstOrDefaultAsync(e => e.Id == id, token);

		token.ThrowIfCancellationRequested();
		return entity;
	}
}