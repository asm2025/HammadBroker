using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuildingLookup
{
	[Display(Name = "العقار")]
	string Name { get; set; }
	[Display(Name = "نوع العقار")]
	BuildingType BuildingType { get; set; }
	string BuildingTypeName { get; }
	[Display(Name = "الصورة")]
	string ImageUrl { get; set; }
	[Display(Name = "نوع التشطيب")]
	FinishingType? FinishingType { get; set; }
	string FinishingTypeName { get; }
	[Display(Name = "البلد")]
	string CountryCode { get; set; }
	[Display(Name = "المدينة")]
	int CityId { get; set; }
}