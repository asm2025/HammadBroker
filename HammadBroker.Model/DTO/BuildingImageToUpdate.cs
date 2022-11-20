using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingImageToUpdate
{
	[Required]
	public int BuildingId { get; set; }
	[Required]
	[StringLength(320)]
	public string ImageUrl { get; set; }
	public bool IsDefault { get; set; }
}