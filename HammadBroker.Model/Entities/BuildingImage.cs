using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;

namespace HammadBroker.Model.Entities;

public class BuildingImage : IEntity<int>
{
	[Key]
	public int Id { get; set; }
	[Required]
	public int BuildingId { get; set; }
	[Required]
	[StringLength(320)]
	public string ImageUrl { get; set; }
}