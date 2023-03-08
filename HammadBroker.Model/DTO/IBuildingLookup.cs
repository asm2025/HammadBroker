using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuildingLookup
{
	[Display(Name = "مرجع")]
	public string Id { get; set; }
	[Display(Name = "الصورة")]
	public string ImageUrl { get; set; }
	[Display(Name = "نوع العقار")]
	BuildingType BuildingType { get; set; }
	string BuildingTypeName { get; }
	[Display(Name = "نوع التشطيب")]
	FinishingType? FinishingType { get; set; }
	string FinishingTypeName { get; }
	[Display(Name = "المدينة")]
	int CityId { get; set; }
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType AdType { get; set; }
	public string AdTypeName { get; }
	[Display(Name = "الاولوية")]
	public byte? Priority { get; set; }
	[Display(Name = "التاريخ")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime? Expires { get; set; }
}