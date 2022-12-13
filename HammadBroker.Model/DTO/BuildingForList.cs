using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForList
{
	public int Id { get; set; }
	[Display(Name = "المبنى")]
	public string Name { get; set; }
	[Display(Name = "نوع المبنى")]
	public BuildingType BuildingType { get; set; }
	[Display(Name = "نوع التشطيب")]
	public FinishingType FinishingType { get; set; }
	[Display(Name = "المدينة")]
	public int CityId { get; set; }
	[Display(Name = "الصورة")]
	public string ImageUrl { get; set; }
}