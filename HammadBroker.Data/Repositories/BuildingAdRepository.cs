using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Core.Data.Entity.Patterns.Repository;
using essentialMix.Patterns.Pagination;
using HammadBroker.Data.Context;
using HammadBroker.Model.DTO;
using HammadBroker.Model.Entities;
using HammadBroker.Model.Parameters;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HammadBroker.Data.Repositories;

public class BuildingAdRepository : Repository<DataContext, BuildingAd, int>, IBuildingAdRepository
{
	/// <inheritdoc />
	public BuildingAdRepository([NotNull] DataContext context, [NotNull] IConfiguration configuration, [NotNull] ILogger<BuildingAdRepository> logger)
		: base(context, configuration, logger)
	{
	}

	/// <inheritdoc />
	protected override IQueryable<BuildingAd> ListInternal(IPagination settings = null)
	{
		if (settings is not BuildingList buildingList) return base.ListInternal(settings);
		IQueryable<BuildingAdBuilding> q = DbSet.Join(Context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdBuilding
		{
			Ad = ba,
			Building = b
		});

		IQueryable<BuildingAd> queryable = PrepareListQuery(q, buildingList)
			.Select(e => e.Ad);
		return queryable;
	}

	/// <inheritdoc />
	public IQueryable<BuildingAdForList> ListWithBuildings(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingAdForList> queryable = (settings is BuildingList buildingList
														? WithFilter(Context, buildingList)
														: WithNoFilter(Context));
		return queryable;

		[NotNull]
		static IQueryable<BuildingAdForList> WithNoFilter([NotNull] DataContext context)
		{
			return context.BuildingAds
						.Join(context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdForList
						{
							Id = ba.Id,
							BuildingId = ba.BuildingId,
							Type = ba.Type,
							Date = ba.Date,
							Expires = ba.Expires,
							Phone = ba.Phone,
							Mobile = ba.Mobile,
							Price = ba.Price,
							Views = ba.Views,
							PageViews = ba.PageViews,
							Requests = ba.Requests,
							Name = b.Name,
							BuildingType = b.BuildingType,
							FinishingType = b.FinishingType,
							CountryCode = b.CountryCode,
							CityId = b.CityId,
							ImageUrl = b.ImageUrl
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
					Type = e.Ad.Type,
					Date = e.Ad.Date,
					Expires = e.Ad.Expires,
					Phone = e.Ad.Phone,
					Mobile = e.Ad.Mobile,
					Price = e.Ad.Price,
					Views = e.Ad.Views,
					PageViews = e.Ad.PageViews,
					Requests = e.Ad.Requests,
					Name = e.Building.Name,
					BuildingType = e.Building.BuildingType,
					FinishingType = e.Building.FinishingType,
					CountryCode = e.Building.CountryCode,
					CityId = e.Building.CityId,
					ImageUrl = e.Building.ImageUrl
				});
		}
	}

	/// <inheritdoc />
	public IQueryable<BuildingAdForDisplay> ListActiveWithBuildings(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();

		IQueryable<BuildingAdForDisplay> queryable = (settings is BuildingList buildingList
														? WithFilter(Context, buildingList)
														: WithNoFilter(Context));
		return queryable;

		[NotNull]
		static IQueryable<BuildingAdForDisplay> WithNoFilter([NotNull] DataContext context)
		{
			return context.BuildingAds
						.Where(e => e.Expires == null || e.Expires >= DateTime.UtcNow.Date)
						.Join(context.Buildings, ba => ba.BuildingId, b => b.Id, (Ad, Building) => new
						{
							Ad,
							Building
						})
						.Join(context.Countries, bab => bab.Building.CountryCode, co => co.Id, (bab, Country) => new
						{
							bab.Ad,
							bab.Building,
							Country
						})
						.Join(context.Cities, babco => babco.Building.CityId, ci => ci.Id, (babco, City) => new
						{
							babco.Ad,
							babco.Building,
							babco.Country,
							City
						})
						.Select(e => new BuildingAdForDisplay
						{
							Id = e.Ad.Id,
							BuildingId = e.Ad.BuildingId,
							Type = e.Ad.Type,
							Date = e.Ad.Date,
							Expires = e.Ad.Expires,
							Phone = e.Ad.Phone,
							Mobile = e.Ad.Mobile,
							Price = e.Ad.Price,
							Name = e.Building.Name,
							BuildingType = e.Building.BuildingType,
							FinishingType = e.Building.FinishingType,
							Floor = e.Building.Floor,
							Rooms = e.Building.Rooms,
							Bathrooms = e.Building.Bathrooms,
							Area = e.Building.Area,
							Address = e.Building.Address,
							Address2 = e.Building.Address2,
							CountryCode = e.Building.CountryCode,
							CountryName = e.Country.Name,
							CityId = e.Building.CityId,
							CityName = e.City.Name,
							ImageUrl = e.Building.ImageUrl,
							Description = e.Building.Description,
						});
		}

		[NotNull]
		static IQueryable<BuildingAdForDisplay> WithFilter([NotNull] DataContext context, [NotNull] BuildingList buildingList)
		{
			IQueryable<BuildingAdBuilding> q = context.BuildingAds
													.Where(e => e.Expires == null || e.Expires >= DateTime.UtcNow.Date)
													.Join(context.Buildings, ba => ba.BuildingId, b => b.Id, (Ad, Building) => new BuildingAdBuilding
													{
														Ad = Ad,
														Building = Building
													});
			return PrepareListQuery(q, buildingList)
					.Join(context.Countries, bab => bab.Building.CountryCode, co => co.Id, (bab, Country) => new
					{
						bab.Ad,
						bab.Building,
						Country
					})
					.Join(context.Cities, babco => babco.Building.CityId, ci => ci.Id, (babco, City) => new
					{
						babco.Ad,
						babco.Building,
						babco.Country,
						City
					})
					.Select(e => new BuildingAdForDisplay
					{
						Id = e.Ad.Id,
						BuildingId = e.Ad.BuildingId,
						Type = e.Ad.Type,
						Date = e.Ad.Date,
						Expires = e.Ad.Expires,
						Phone = e.Ad.Phone,
						Mobile = e.Ad.Mobile,
						Price = e.Ad.Price,
						Name = e.Building.Name,
						BuildingType = e.Building.BuildingType,
						FinishingType = e.Building.FinishingType,
						Floor = e.Building.Floor,
						Rooms = e.Building.Rooms,
						Bathrooms = e.Building.Bathrooms,
						Area = e.Building.Area,
						Address = e.Building.Address,
						Address2 = e.Building.Address2,
						CountryCode = e.Building.CountryCode,
						CountryName = e.Country.Name,
						CityId = e.Building.CityId,
						CityName = e.City.Name,
						ImageUrl = e.Building.ImageUrl,
						Description = e.Building.Description,
					});
		}
	}

	public Task<BuildingAdForDetails> GetBuildingAsync(int id, CancellationToken token = default(CancellationToken))
	{
		return DbSet
				.Join(Context.Buildings, ba => ba.BuildingId, b => b.Id, (ba, b) => new BuildingAdForDetails
				{
					Id = ba.Id,
					BuildingId = ba.BuildingId,
					Type = ba.Type,
					Date = ba.Date,
					Expires = ba.Expires,
					Phone = ba.Phone,
					Mobile = ba.Mobile,
					Price = ba.Price,
					Views = ba.Views,
					PageViews = ba.PageViews,
					Requests = ba.Requests,
					Name = b.Name,
					BuildingType = b.BuildingType,
					FinishingType = b.FinishingType,
					CountryCode = b.CountryCode,
					CityId = b.CityId,
					ImageUrl = b.ImageUrl,
					VideoId = b.VideoId,
					Floor = b.Floor,
					Rooms = b.Rooms,
					Bathrooms = b.Bathrooms,
					Area = b.Area,
					Address = b.Address,
					Address2 = b.Address2,
					Description = b.Description
				})
				.FirstOrDefaultAsync(e => e.Id == id, token);
	}

	/// <inheritdoc />
	protected override IQueryable<BuildingAd> PrepareCountQuery(IQueryable<BuildingAd> query, IPagination settings)
	{
		if (settings is not BuildingAdList buildingAdList) return base.PrepareCountQuery(query, settings);
		query = PrepareQuery(query, buildingAdList);
		return base.PrepareCountQuery(query, settings);
	}

	/// <inheritdoc />
	protected override IQueryable<BuildingAd> PrepareListQuery(IQueryable<BuildingAd> query, IPagination settings)
	{
		if (settings is not BuildingAdList buildingAdList) return base.PrepareListQuery(query, settings);
		query = PrepareQuery(query, buildingAdList);
		return base.PrepareListQuery(query, settings);
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