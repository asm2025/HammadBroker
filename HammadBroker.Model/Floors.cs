using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model;

public enum Floors
{
	[Display(Name = "الأرضي")]
	Ground,
	[Display(Name = "الأول")]
	First,
	[Display(Name = "الثاني")]
	Second,
	[Display(Name = "الثالث")]
	Third,
	[Display(Name = "الرابع")]
	Fourth,
	[Display(Name = "الخامس")]
	Fifth,
	[Display(Name = "السادس")]
	Sixth,
	[Display(Name = "السابع")]
	Seventh,
	[Display(Name = "الثامن")]
	Eighth,
	[Display(Name = "التاسع")]
	Ninth,
	[Display(Name = "العاشر")]
	Tenth,
	[Display(Name = "الحادي عشر")]
	Eleventh,
	[Display(Name = "الثاني عشر")]
	Twelfth,
	[Display(Name = "الثاني عشر+")]
	TwelfthPlus,
	[Display(Name = "الاخير")]
	Last,
}