using System.Collections.Generic;

namespace HammadBroker.Model.DTO;

public interface ICountryLookup
{
	ICollection<CountryForList> Countries { get; set; }
}