using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class DistrictForList
{
	public int Id { get; set; }

	[Display(Name = "الحي")]
	public string Name { get; set; }

	[Display(Name = "المدينة")]
	public int CityId { get; set; }

	[Display(Name = "المدينة")]
	public string CityName { get; set; }
}