using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface ICountryNameLookup
{
	[Display(Name = "البلد")]
	string CountryCode { get; set; }
	[Display(Name = "البلد")]
	string CountryName { get; set; }
}