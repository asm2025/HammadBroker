using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdForDetails : BuildingForDetails, IBuildingAd
{
	[Display(Name = "العقار")]
	public int BuildingId { get; set; }
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType Type { get; set; }
	[Display(Name = "التاريخ")]
	[DisplayFormat(DataFormatString = "{0:D}")]
	public DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	[DisplayFormat(DataFormatString = "{0:D}")]
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