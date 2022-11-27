using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model;

public enum BuildingAdType
{
	[Display(Name = "للبيع")]
	ForSale,
	[Display(Name = "للايجار")]
	ForRent
}