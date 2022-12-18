using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.Parameters;

public class BuildingAdList : BuildingList
{
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType? Type { get; set; }
	[Display(Name = "التاريخ")]
	public DateTime? Date { get; set; }
	[Display(Name = "بحد أقصى")]
	public DateTime? MaxDate { get; set; }
	[Display(Name = "السعر")]
	public long? Price { get; set; }
	[Display(Name = "بحد أقصى")]
	public long? MaxPrice { get; set; }
}