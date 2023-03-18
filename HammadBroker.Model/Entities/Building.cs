using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class Building : IEntity<int>
{
	[Key]
	public int Id { get; set; }

	[Required]
	[StringLength(Constants.Buildings.IdentifierLength)]
	public string Reference { get; set; }

	[Required]
	public BuildingType BuildingType { get; set; }

	[StringLength(128)]
	public string VideoId { get; set; }

	public FinishingType? FinishingType { get; set; }

	public Floors? Floor { get; set; }

	public byte? Rooms { get; set; }

	public byte? Bathrooms { get; set; }

	public long? BuildingArea { get; set; }

	public long? Area { get; set; }

	[StringLength(256)]
	public string Address { get; set; }

	public int? DistrictId { get; set; }

	[Required]
	public int CityId { get; set; }

	[StringLength(1024)]
	public string ShortDescription { get; set; }

	[Required]
	[StringLength(4096)]
	public string Description { get; set; }

	[Required]
	public BuildingAdType AdType { get; set; }

	public byte? Priority { get; set; }

	public DateTime CreatedOn { get; set; }

	public DateTime UpdatedOn { get; set; }

	public DateTime Date { get; set; }

	public DateTime? Expires { get; set; }

	[Phone]
	public string Phone { get; set; }

	[Phone]
	public string Mobile { get; set; }

	public long Price { get; set; }

	public bool Enabled { get; set; } = true;
}