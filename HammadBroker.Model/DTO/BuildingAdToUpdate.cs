using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdToUpdate
{
	[Display(Name = "النوع")]
	public BuildingAdType Type { get; set; }
	[Display(Name = "الاولوية")]
	public byte Priority { get; set; }
	[Required]
	[Display(Name = "المبنى")]
	public int BuildingId { get; set; }
	[Display(Name = "التاريخ")]
	public DateTime Date { get; set; }
	[Display(Name = "تاريخ الانتهاء")]
	public DateTime? Expires { get; set; }
	[Required]
	[Phone]
	[Display(Name = "التليفون")]
	public string Phone { get; set; }
	[Phone]
	[Display(Name = "المحمول")]
	public string Mobile { get; set; }
	[Required]
	[Display(Name = "السعر")]
	public decimal Price { get; set; }
	[Display(Name = "المشاهدات")]
	public long Views { get; set; }
	[Display(Name = "مشاهدات الصفحة")]
	public long PageViews { get; set; }
	[Display(Name = "الطلبات")]
	public long Requests { get; set; }
}