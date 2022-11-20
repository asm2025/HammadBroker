using System.ComponentModel.DataAnnotations;
using essentialMix.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace HammadBroker.Model.Entities;

[Index(nameof(BuildingId), nameof(IsDefault), IsUnique = true)]
public class BuildingImage : IEntity<int>
{
	[Key]
	public int Id { get; set; }
	[Required]
	public int BuildingId { get; set; }
	[Required]
	[StringLength(320)]
	public string ImageUrl { get; set; }
	public bool IsDefault { get; set; }
}