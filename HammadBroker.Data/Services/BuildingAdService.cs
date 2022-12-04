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
using HammadBroker.Model.Parameters;
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
	public override IPaginated<T> List<T>(IQueryable<BuildingAd> queryable, IPagination settings = null)
	{
		ThrowIfDisposed();
		if (settings is not BuildingList buildingList) return base.List<T>(queryable, settings);

		IQueryable<BuildingAdBuilding> q = queryable.Join(Context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdBuilding
		{
			Ad = ba,
			Building = b
		});

		queryable = PrepareBuildingFilters(q, buildingList)
			.Select(e => e.Ad);
		return base.List<T>(queryable, settings);
	}

	/// <inheritdoc />
	public override Task<IPaginated<T>> ListAsync<T>(IQueryable<BuildingAd> queryable, IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		if (settings is not BuildingList buildingList) return base.ListAsync<T>(queryable, settings, token);

		IQueryable<BuildingAdBuilding> q = queryable.Join(Context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdBuilding
		{
			Ad = ba,
			Building = b
		});

		queryable = PrepareBuildingFilters(q, buildingList)
			.Select(e => e.Ad);
		return base.ListAsync<T>(queryable, settings, token);
	}

	/// <inheritdoc />
	public async Task<IPaginated<BuildingAdForList>> ListWithBuildingsAsync(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingAdForList> queryable = (settings is BuildingList buildingList
														? WithFilter(Context, buildingList)
														: WithNoFilter(Context))
			.Where(e => e.Expires == null || e.Expires >= DateTime.Today);

		if (settings is { PageSize: > 0 })
		{
			settings.Count = await queryable.CountAsync(token);
			queryable = queryable.Paginate(settings);
			token.ThrowIfCancellationRequested();
		}

		IList<BuildingAdForList> result = await queryable.ToListAsync(token)
														.ConfigureAwait();
		token.ThrowIfCancellationRequested();
		return new Paginated<BuildingAdForList>(result, settings);

		[NotNull]
		static IQueryable<BuildingAdForList> WithNoFilter([NotNull] DataContext context)
		{
			return context.BuildingAds
						.Join(context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdForList
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
							CityId = b.CityId
						});
		}

		[NotNull]
		static IQueryable<BuildingAdForList> WithFilter([NotNull] DataContext context, [NotNull] BuildingList buildingList)
		{
			IQueryable<BuildingAdBuilding> q = context.BuildingAds
													.Join(context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdBuilding
													{
														Ad = ba,
														Building = b
													});
			return PrepareBuildingFilters(q, buildingList)
				.Select(e => new BuildingAdForList
				{
					Id = e.Ad.Id,
					BuildingId = e.Ad.BuildingId,
					Date = e.Ad.Date,
					Expires = e.Ad.Expires,
					Phone = e.Ad.Phone,
					Mobile = e.Ad.Mobile,
					Price = e.Ad.Price,
					Name = e.Building.Name,
					BuildingType = e.Building.BuildingType,
					ImageUrl = e.Building.ImageUrl,
					FinishingType = e.Building.FinishingType,
					CityId = e.Building.CityId
				});
		}
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
														VideoUrl = b.VideoUrl,
														FinishingType = b.FinishingType,
														CityId = b.CityId,
														Floor = b.Floor,
														Location = b.Location,
														Address = b.Address,
														Address2 = b.Address2,
														Description = b.Description
													})
													.FirstOrDefaultAsync(e => e.Id == id, token);

		token.ThrowIfCancellationRequested();
		return entity;
	}

	[NotNull]
	private static IQueryable<BuildingAdBuilding> PrepareBuildingFilters([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingList buildingList)
	{
		if (buildingList.CityId.HasValue) queryable = queryable.Where(e => e.Building.CityId == buildingList.CityId.Value);
		queryable = PrepareMetaData(queryable, buildingList);
		queryable = PrepareSearch(queryable, buildingList);
		return queryable;

		[NotNull]
		static IQueryable<BuildingAdBuilding> PrepareMetaData([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingList buildingList)
		{
			if (buildingList.BuildingType.HasValue) queryable = queryable.Where(e => e.Building.BuildingType == buildingList.BuildingType.Value);
			if (buildingList.FinishingType.HasValue) queryable = queryable.Where(e => e.Building.FinishingType == buildingList.FinishingType.Value);
			if (buildingList.Floor.HasValue) queryable = queryable.Where(e => e.Building.Floor == buildingList.Floor.Value);

			if (buildingList.Rooms.HasValue)
			{
				if (buildingList.MaxRooms.HasValue && buildingList.MaxRooms > buildingList.Rooms)
					queryable = queryable.Where(e => e.Building.Rooms >= buildingList.Rooms.Value && e.Building.Rooms <= buildingList.MaxRooms.Value);
				else
					queryable = queryable.Where(e => e.Building.Rooms == buildingList.Rooms.Value);
			}

			if (buildingList.Bathrooms.HasValue)
			{
				if (buildingList.MaxBathrooms.HasValue && buildingList.MaxBathrooms > buildingList.Bathrooms)
					queryable = queryable.Where(e => e.Building.Bathrooms >= buildingList.Bathrooms.Value && e.Building.Bathrooms <= buildingList.MaxBathrooms.Value);
				else
					queryable = queryable.Where(e => e.Building.Bathrooms == buildingList.Bathrooms.Value);
			}

			if (buildingList.Area.HasValue)
			{
				if (buildingList.MaxArea.HasValue && buildingList.MaxArea > buildingList.Area)
					queryable = queryable.Where(e => e.Building.Area >= buildingList.Area.Value && e.Building.Area <= buildingList.MaxArea.Value);
				else
					queryable = queryable.Where(e => e.Building.Area == buildingList.Area.Value);
			}

			return queryable;
		}

		[NotNull]
		static IQueryable<BuildingAdBuilding> PrepareSearch([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingList buildingList)
		{
			if (!string.IsNullOrEmpty(buildingList.Search)) queryable = queryable.Where(e => e.Building.Name.Contains(buildingList.Search));
			if (!string.IsNullOrEmpty(buildingList.Address)) queryable = queryable.Where(e => e.Building.Address.Contains(buildingList.Address) || e.Building.Address2.Contains(buildingList.Address));
			return queryable;
		}
	}
}