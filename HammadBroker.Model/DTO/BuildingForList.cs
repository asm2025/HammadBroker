using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForList
{
	public int Id { get; set; }
	[Display(Name = "العقار")]
	public string Name { get; set; }
	[Display(Name = "نوع العقار")]
	public BuildingType BuildingType { get; set; }
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	[Display(Name = "المدينة")]
	public int CityId { get; set; }
	[Display(Name = "الصورة")]
	public string ImageUrl { get; set; }
}