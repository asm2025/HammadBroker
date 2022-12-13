using System.Collections.Generic;

namespace HammadBroker.Data;

public class CitiesData
{
	public string CountryCode { get; set; }
	public ICollection<string> Cities { get; set; }
}