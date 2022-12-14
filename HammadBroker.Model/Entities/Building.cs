using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class Building : IEntity<int>
{
	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(256)]
	public string Name { get; set; }

	[Required]
	public BuildingType BuildingType { get; set; }

	[Url]
	[StringLength(320)]
	public string ImageUrl { get; set; }

	[StringLength(128)]
	public string VideoId { get; set; }

	[Required]
	public FinishingType? FinishingType { get; set; }

	public byte? Floor { get; set; }

	public byte? Rooms { get; set; }

	public byte? Bathrooms { get; set; }

	public long? Area { get; set; }

	[StringLength(256)]
	public string Address { get; set; }

	[StringLength(256)]
	public string Address2 { get; set; }

	[Required]
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }

	[Required]
	public int CityId { get; set; }

	[Required]
	[StringLength(2048)]
	public string Description { get; set; }
}