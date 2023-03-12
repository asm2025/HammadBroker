using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model;

public enum BuildingType
{
	[Display(Name = "شقة")]
	Apartment,
	[Display(Name = "فيلا")]
	Villa,
	[Display(Name = "محل")]
	Shop,
	[Display(Name = "كافتيريا")]
	Cafeteria,
	[Display(Name = "تاون هاوس")]
	TownHouse,
	[Display(Name = "بنتهاوس")]
	Penthouse,
	[Display(Name = "مجمع سكني")]
	ApartmentComplex,
	[Display(Name = "شاليه")]
	Chalet,
	[Display(Name = "منزل مزدوج")]
	DoubleHouse,
	[Display(Name = "دوبلكس")]
	Duplex,
	[Display(Name = "طابق كامل")]
	EntireFloor,
	[Display(Name = "نصف طابق")]
	HalfFloor,
	[Display(Name = "بناية كاملة")]
	Building,
	[Display(Name = "قطعة أرض")]
	Yard,
	[Display(Name = "وحدات مجمعة")]
	CompoundUnits,
	[Display(Name = "بانجلو")]
	Bungalow,
	[Display(Name = "شقق فندقية")]
	HotelCompartments,
	[Display(Name = "مخزن")]
	Storehouse,
	[Display(Name = "مصنع")]
	Factory
}