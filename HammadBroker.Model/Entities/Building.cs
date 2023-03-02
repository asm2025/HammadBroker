using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class Building : IEntity<string>
{
	[Key]
	[StringLength(Constants.Buildings.IdentifierLength)]
	public string Id { get; set; }

	[Required]
	public BuildingType BuildingType { get; set; }

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
	public int CityId { get; set; }

	[Required]
	[StringLength(1024)]
	public string ShortDescription { get; set; }

	[Required]
	[StringLength(4096)]
	public string Description { get; set; }

	[Required]
	public BuildingAdType AdType { get; set; }

	public byte? Priority { get; set; }

	public DateTime Date { get; set; }

	public DateTime? Expires { get; set; }

	[Phone]
	public string Phone { get; set; }

	[Phone]
	public string Mobile { get; set; }

	public long Price { get; set; }
}