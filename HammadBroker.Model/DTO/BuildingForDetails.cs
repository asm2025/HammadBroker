using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForDetails : BuildingForList, ICountryNameLookup, ICityNameLookup, IBuildingImagesLookup
{
	[Display(Name = "رابط الفيديو")]
	public string VideoId { get; set; }
	[Display(Name = "الدور")]
	public byte? Floor { get; set; }
	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }
	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }
	[Display(Name = "المساحة")]
	public long? Area { get; set; }
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	[Display(Name = "الحي")]
	public string Address2 { get; set; }

	/// <inheritdoc />
	[Display(Name = "البلد")]
	public string CountryName { get; set; }

	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public string CityName { get; set; }
	[Display(Name = "الوصف")]
	public string Description { get; set; }

	/// <inheritdoc />
	[Display(Name = "الصور")]
	public ICollection<string> Images { get; set; }
}