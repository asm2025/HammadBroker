using System;
using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdModel : IBuildingAd
{
	private bool _canEdit;
	private bool _canUpload;

	public BuildingAdModel([NotNull] IBuildingAd ad)
		: this(ad, null)
	{
	}

	public BuildingAdModel([NotNull] IBuildingAd ad, IBuilding building)
	{
		Ad = ad;
		Building = building;
	}

	public int Id { get; set; }

	public bool ReadOnly { get; set; }

	public bool CanEdit
	{
		get => !ReadOnly && _canEdit;
		set => _canEdit = value;
	}

	public bool CanUpload
	{
		get => Id > 0 && _canUpload;
		set => _canUpload = value;
	}

	[NotNull]
	public IBuildingAd Ad { get; set; }

	public IBuilding Building { get; set; }

	/// <inheritdoc />
	public int BuildingId
	{
		get => Ad.BuildingId;
		set => Ad.BuildingId = value;
	}

	/// <inheritdoc />
	public BuildingAdType Type
	{
		get => Ad.Type;
		set => Ad.Type = value;
	}

	/// <inheritdoc />
	public DateTime Date
	{
		get => Ad.Date;
		set => Ad.Date = value;
	}

	/// <inheritdoc />
	public DateTime? Expires
	{
		get => Ad.Expires;
		set => Ad.Expires = value;
	}

	/// <inheritdoc />
	public string Phone
	{
		get => Ad.Phone;
		set => Ad.Phone = value;
	}

	/// <inheritdoc />
	public string Mobile
	{
		get => Ad.Mobile;
		set => Ad.Mobile = value;
	}

	/// <inheritdoc />
	public long Price
	{
		get => Ad.Price;
		set => Ad.Price = value;
	}

	/// <inheritdoc />
	public long Views
	{
		get => Ad.Views;
		set => Ad.Views = value;
	}

	/// <inheritdoc />
	public long PageViews
	{
		get => Ad.PageViews;
		set => Ad.PageViews = value;
	}

	/// <inheritdoc />
	public long Requests
	{
		get => Ad.Requests;
		set => Ad.Requests = value;
	}
}
