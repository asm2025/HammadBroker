using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingModel : IBuilding
{
	private bool _canEdit;
	private bool _canUpload;

	public BuildingModel([NotNull] IBuilding building)
	{
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
	public IBuilding Building { get; }

	/// <inheritdoc />
	public string Name
	{
		get => Building.Name;
		set => Building.Name = value;
	}

	/// <inheritdoc />
	public BuildingType BuildingType
	{
		get => Building.BuildingType;
		set => Building.BuildingType = value;
	}

	/// <inheritdoc />
	public string BuildingTypeName => Building.BuildingTypeName;

	/// <inheritdoc />
	public string ImageUrl
	{
		get => Building.ImageUrl;
		set => Building.ImageUrl = value;
	}

	/// <inheritdoc />
	public FinishingType? FinishingType
	{
		get => Building.FinishingType;
		set => Building.FinishingType = value;
	}

	/// <inheritdoc />
	public string FinishingTypeName => Building.FinishingTypeName;

	/// <inheritdoc />
	public string CountryCode
	{
		get => Building.CountryCode;
		set => Building.CountryCode = value;
	}

	/// <inheritdoc />
	public int CityId
	{
		get => Building.CityId;
		set => Building.CityId = value;
	}

	/// <inheritdoc />
	public string VideoId
	{
		get => Building.VideoId;
		set => Building.VideoId = value;
	}

	/// <inheritdoc />
	public byte? Floor
	{
		get => Building.Floor;
		set => Building.Floor = value;
	}

	/// <inheritdoc />
	public byte? Rooms
	{
		get => Building.Rooms;
		set => Building.Rooms = value;
	}

	/// <inheritdoc />
	public byte? Bathrooms
	{
		get => Building.Bathrooms;
		set => Building.Bathrooms = value;
	}

	/// <inheritdoc />
	public long? Area
	{
		get => Building.Area;
		set => Building.Area = value;
	}

	/// <inheritdoc />
	public string Address
	{
		get => Building.Address;
		set => Building.Address = value;
	}

	/// <inheritdoc />
	public string Address2
	{
		get => Building.Address2;
		set => Building.Address2 = value;
	}

	/// <inheritdoc />
	public string Description
	{
		get => Building.Description;
		set => Building.Description = value;
	}
}