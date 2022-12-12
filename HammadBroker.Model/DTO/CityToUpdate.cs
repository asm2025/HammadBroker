using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class CityToUpdate
{
	[Display(Name = "المدينة")]
	[Required]
	[StringLength(256)]
	public string Name { get; set; }
	[Display(Name = "البلد")]
	[Required]
	[StringLength(3, MinimumLength = 3)]
	public string CountryCode { get; set; }
	public IList<CountryForList> Countries { get; set; }
}