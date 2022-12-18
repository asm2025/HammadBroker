using System.Collections.Generic;

namespace HammadBroker.Model.DTO;

public interface IBuildingLookup
{
	string CountryCode { get; set; }
	IList<CountryForList> Countries { get; set; }
	int CityId { get; set; }
	IList<CityForList> Cities { get; set; }
}