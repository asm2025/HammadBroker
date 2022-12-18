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

		queryable = PrepareListQuery(q, buildingList)
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

		queryable = PrepareListQuery(q, buildingList)
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
			return PrepareListQuery(q, buildingList)
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
														Address = b.Address,
														Address2 = b.Address2,
														Description = b.Description
													})
													.FirstOrDefaultAsync(e => e.Id == id, token);

		token.ThrowIfCancellationRequested();
		return entity;
	}

	/// <inheritdoc />
	protected override IQueryable<BuildingAd> PrepareListQuery(IQueryable<BuildingAd> queryable, IPagination settings)
	{
		if (settings is not BuildingAdList buildingAdList) return base.PrepareListQuery(queryable, settings);
		queryable = PrepareQuery(queryable, buildingAdList);
		return base.PrepareListQuery(queryable, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<BuildingAd> PrepareCountQuery(IQueryable<BuildingAd> queryable, IPagination settings)
	{
		if (settings is not BuildingAdList buildingAdList) return base.PrepareCountQuery(queryable, settings);
		queryable = PrepareQuery(queryable, buildingAdList);
		return base.PrepareCountQuery(queryable, settings);
	}

	[NotNull]
	private static IQueryable<BuildingAd> PrepareQuery([NotNull] IQueryable<BuildingAd> queryable, [NotNull] BuildingAdList buildingAdList)
	{
		if (buildingAdList.Type.HasValue) queryable = queryable.Where(e => e.Type == buildingAdList.Type.Value);

		if (buildingAdList.Date.HasValue)
		{
			queryable = buildingAdList.MaxDate.HasValue
							? queryable.Where(e => e.Date >= buildingAdList.Date.Value && e.Date <= buildingAdList.MaxDate.Value)
							: queryable.Where(e => e.Date == buildingAdList.Date.Value);
		}
		else if (buildingAdList.MaxDate.HasValue)
		{
			queryable = queryable.Where(e => e.Date <= buildingAdList.MaxDate.Value);
		}

		if (buildingAdList.Price.HasValue)
		{
			queryable = buildingAdList.MaxPrice.HasValue
							? queryable.Where(e => e.Price >= buildingAdList.Price.Value && e.Price <= buildingAdList.MaxPrice.Value)
							: queryable.Where(e => e.Price == buildingAdList.Price.Value);
		}
		else if (buildingAdList.Price.HasValue)
		{
			queryable = queryable.Where(e => e.Price <= buildingAdList.MaxPrice.Value);
		}

		return queryable;
	}

	[NotNull]
	private static IQueryable<BuildingAdBuilding> PrepareListQuery([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingList buildingList)
	{
		if (buildingList is BuildingAdList buildingAdList) queryable = PrepareAd(queryable, buildingAdList);
		queryable = PrepareLocation(queryable, buildingList);
		queryable = PrepareNumbers(queryable, buildingList);
		queryable = PrepareSearch(queryable, buildingList);
		return queryable;

		[NotNull]
		static IQueryable<BuildingAdBuilding> PrepareAd([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingAdList buildingAdList)
		{
			if (buildingAdList.Type.HasValue) queryable = queryable.Where(e => e.Ad.Type == buildingAdList.Type.Value);

			if (buildingAdList.Date.HasValue)
			{
				queryable = buildingAdList.MaxDate.HasValue
								? queryable.Where(e => e.Ad.Date >= buildingAdList.Date.Value && e.Ad.Date <= buildingAdList.MaxDate.Value)
								: queryable.Where(e => e.Ad.Date == buildingAdList.Date.Value);
			}
			else if (buildingAdList.MaxDate.HasValue)
			{
				queryable = queryable.Where(e => e.Ad.Date <= buildingAdList.MaxDate.Value);
			}

			if (buildingAdList.Price.HasValue)
			{
				queryable = buildingAdList.MaxPrice.HasValue
								? queryable.Where(e => e.Ad.Price >= buildingAdList.Price.Value && e.Ad.Price <= buildingAdList.MaxPrice.Value)
								: queryable.Where(e => e.Ad.Price == buildingAdList.Price.Value);
			}
			else if (buildingAdList.Price.HasValue)
			{
				queryable = queryable.Where(e => e.Ad.Price <= buildingAdList.MaxPrice.Value);
			}

			return queryable;
		}

		[NotNull]
		static IQueryable<BuildingAdBuilding> PrepareLocation([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingList buildingList)
		{
			if (!string.IsNullOrEmpty(buildingList.CountryCode)) queryable = queryable.Where(e => e.Building.CountryCode == buildingList.CountryCode);
			if (buildingList.CityId > 0) queryable = queryable.Where(e => e.Building.CityId == buildingList.CityId);
			return queryable;
		}

		[NotNull]
		static IQueryable<BuildingAdBuilding> PrepareNumbers([NotNull] IQueryable<BuildingAdBuilding> queryable, [NotNull] BuildingList buildingList)
		{
			if (buildingList.BuildingType.HasValue) queryable = queryable.Where(e => e.Building.BuildingType == buildingList.BuildingType.Value);
			if (buildingList.FinishingType.HasValue) queryable = queryable.Where(e => e.Building.FinishingType == buildingList.FinishingType.Value);

			if (buildingList.Floor.HasValue)
			{
				if (buildingList.MaxFloor.HasValue && buildingList.MaxFloor > buildingList.Floor)
					queryable = queryable.Where(e => e.Building.Floor >= buildingList.Floor.Value && e.Building.Floor <= buildingList.MaxFloor.Value);
				else
					queryable = queryable.Where(e => e.Building.Floor == buildingList.Floor.Value);
			}

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