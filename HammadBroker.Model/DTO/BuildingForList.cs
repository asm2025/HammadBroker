using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingForList : IBuildingLookup
{
	public int Id { get; set; }
	/// <inheritdoc />
	[Display(Name = "العقار")]
	public string Name { get; set; }
	/// <inheritdoc />
	[Display(Name = "نوع العقار")]
	public BuildingType BuildingType { get; set; }
	/// <inheritdoc />
	[Display(Name = "الصورة")]
	public string ImageUrl { get; set; }
	/// <inheritdoc />
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }
	/// <inheritdoc />
	[Display(Name = "البلد")]
	public string CountryCode { get; set; }
	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public int CityId { get; set; }
}