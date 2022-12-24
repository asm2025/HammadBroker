using System.Collections.Generic;

namespace HammadBroker.Model.DTO;

public interface ICityLookup
{
	string CountryCode { get; set; }
	int CityId { get; set; }
	ICollection<CityForList> Cities { get; set; }
}