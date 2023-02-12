﻿using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Extensions;

namespace HammadBroker.Model.DTO;

public class BuildingAdToUpdate : IBuildingAd
{
	[Display(Name = "النوع")]
	public BuildingAdType Type { get; set; }
	public string TypeName => Type.GetDisplayName();
	[Display(Name = "الاولوية")]
	public byte Priority { get; set; }
	[Display(Name = "العقار")]
	[Required]
	public int BuildingId { get; set; }
	[Display(Name = "التاريخ")]
	[Required]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime Date { get; set; }
	[Display(Name = "تاريخ الانتهاء")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime? Expires { get; set; }
	[Display(Name = "التليفون")]
	[Required]
	[Phone]
	[DataType(DataType.PhoneNumber)]
	public string Phone { get; set; }
	[Display(Name = "المحمول")]
	[Phone]
	[DataType(DataType.PhoneNumber)]
	public string Mobile { get; set; }
	[Display(Name = "السعر")]
	[DisplayFormat(DataFormatString = "{0:#,#.##}")]
	[Required]
	[Range(0, 1000000000)]
	public long Price { get; set; }
	[Display(Name = "المشاهدات")]
	public long Views { get; set; }
	[Display(Name = "مشاهدات الصفحة")]
	public long PageViews { get; set; }
	[Display(Name = "الطلبات")]
	public long Requests { get; set; }
}