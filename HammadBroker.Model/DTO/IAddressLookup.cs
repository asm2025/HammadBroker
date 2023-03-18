using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IAddressLookup
{
	[Display(Name = "الحي")]
	int? DistrictId { get; set; }
	[Display(Name = "الحي")]
	string DistrictName { get; set; }
	[Display(Name = "المدينة")]
	int CityId { get; set; }
	[Display(Name = "المدينة")]
	string CityName { get; set; }
}