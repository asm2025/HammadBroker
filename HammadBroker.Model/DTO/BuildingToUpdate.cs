using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingToUpdate
{
	[Required]
	[StringLength(256)]
	public string Name { get; set; }

	[Required]
	public BuildingType BuildingType { get; set; }

	[Required]
	public FinishingType FinishingType { get; set; }

	[Required]
	[StringLength(32)]
	public string Floor { get; set; }

	[StringLength(2048)]
	public string Location { get; set; }

	[StringLength(512)]
	public string Address { get; set; }

	[StringLength(512)]
	public string Address2 { get; set; }

	public int? CityId { get; set; }

	[Required]
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }

	[Required]
	[StringLength(2048)]
	public string Description { get; set; }
}