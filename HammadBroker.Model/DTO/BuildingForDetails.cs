using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForDetails : BuildingForList, IBuilding, ICountryNameLookup, ICityNameLookup
{
	/// <inheritdoc />
	[Display(Name = "معرف Youtube")]
	public string VideoId { get; set; }
	/// <inheritdoc />
	[Display(Name = "الدور")]
	public byte? Floor { get; set; }
	/// <inheritdoc />
	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }
	/// <inheritdoc />
	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }
	/// <inheritdoc />
	[Display(Name = "المساحة")]
	public long? Area { get; set; }
	/// <inheritdoc />
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	/// <inheritdoc />
	[Display(Name = "الحي")]
	public string Address2 { get; set; }
	/// <inheritdoc />
	[Display(Name = "البلد")]
	public string CountryName { get; set; }
	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public string CityName { get; set; }
	/// <inheritdoc />
	[Display(Name = "الوصف")]
	public string Description { get; set; }
}