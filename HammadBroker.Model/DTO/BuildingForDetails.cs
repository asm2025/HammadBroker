using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForDetails : BuildingForList
{
	[Display(Name = "الدور")]
	public byte? Floor { get; set; }
	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }
	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }
	[Display(Name = "المساحة")]
	public decimal? Area { get; set; }
	[Display(Name = "الموقع")]
	public string Location { get; set; }
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	[Display(Name = "الحي")]
	public string Address2 { get; set; }
	[Display(Name = "رابط الفيديو")]
	public string VideoUrl { get; set; }
	[Display(Name = "الوصف")]
	public string Description { get; set; }
}