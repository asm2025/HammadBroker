using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class CityToUpdate
{
	[Display(Name = "المدينة")]
	[Required]
	[StringLength(256)]
	public string Name { get; set; }
}