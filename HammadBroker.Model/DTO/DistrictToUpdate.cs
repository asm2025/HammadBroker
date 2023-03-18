using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class DistrictToUpdate
{
	[Display(Name = "المدينة")]
	[Required]
	[Range(1, int.MaxValue)]
	public int CityId { get; set; }

	[Display(Name = "الحي")]
	[Required]
	[StringLength(256)]
	public string Name { get; set; }
}