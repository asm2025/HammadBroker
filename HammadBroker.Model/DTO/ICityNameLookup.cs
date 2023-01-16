using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface ICityNameLookup
{
	[Display(Name = "المدينة")]
	int CityId { get; set; }
	[Display(Name = "المدينة")]
	string CityName { get; set; }
}