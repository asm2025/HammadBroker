using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class CityForList
{
	public int Id { get; set; }
	[Display(Name = "المدينة")]
	public string Name { get; set; }
}