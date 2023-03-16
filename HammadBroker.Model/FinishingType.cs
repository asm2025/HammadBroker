using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model;

public enum FinishingType
{
	[Display(Name = "عادي")]
	Normal,
	[Display(Name = "قديم")]
	Old,
	[Display(Name = "نصف تشطيب")]
	SemiFinished,
	[Display(Name = "لوكس")]
	Lux,
	[Display(Name = "سوبر لوكس")]
	SuperLux,
	[Display(Name = "الترا لوكس")]
	UltraLux
}