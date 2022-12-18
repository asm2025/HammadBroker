using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdForList : BuildingForList
{
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType Type { get; set; }
	[Display(Name = "المبنى")]
	public int BuildingId { get; set; }
	[Display(Name = "التاريخ")]
	public DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	public DateTime? Expires { get; set; }
	[Display(Name = "تليفون")]
	public string Phone { get; set; }
	[Display(Name = "محمول")]
	public string Mobile { get; set; }
	[Display(Name = "السعر")]
	public long Price { get; set; }
	[Display(Name = "المشاهدات")]
	public long Views { get; set; }
	[Display(Name = "مشاهدات الصفحة")]
	public long PageViews { get; set; }
	[Display(Name = "الطلبات")]
	public long Requests { get; set; }
}