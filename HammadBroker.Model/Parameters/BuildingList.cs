﻿using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Patterns.Pagination;

namespace HammadBroker.Model.Parameters;

public class BuildingList : SortablePagination
{
	[Display(Name = "مرجع")]
	public string Reference { get; set; }
	[Display(Name = "الحالة")]
	public bool? Enabled { get; set; }
	[Display(Name = "نوع العقار")]
	public BuildingType? BuildingType { get; set; }
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }
	[Display(Name = "الدور")]
	public Floors? Floor { get; set; }
	[Display(Name = "بحد أقصى")]
	public Floors? MaxFloor { get; set; }
	[Display(Name = "الغرف")]
	public byte? Rooms { get; set; }
	[Display(Name = "بحد أقصى")]
	public byte? MaxRooms { get; set; }
	[Display(Name = "الحمامات")]
	public byte? Bathrooms { get; set; }
	[Display(Name = "بحد أقصى")]
	public byte? MaxBathrooms { get; set; }
	[Display(Name = "مساحة المبانى")]
	public long? BuildingArea { get; set; }
	[Display(Name = "بحد أقصى")]
	public long? MaxBuildingArea { get; set; }
	[Display(Name = "مساحة الأرض")]
	public long? Area { get; set; }
	[Display(Name = "بحد أقصى")]
	public long? MaxArea { get; set; }
	[Display(Name = "العنوان")]
	public string Address { get; set; }
	[Display(Name = "الحي")]
	public int DistrictId { get; set; }
	[Display(Name = "المدينة")]
	public int CityId { get; set; }
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType? AdType { get; set; }
	[Display(Name = "التاريخ")]
	public DateTime? Date { get; set; }
	[Display(Name = "بحد أقصى")]
	public DateTime? MaxDate { get; set; }
	[Display(Name = "السعر")]
	public long? Price { get; set; }
	[Display(Name = "بحد أقصى")]
	public long? MaxPrice { get; set; }

	public virtual bool HasSearch => !string.IsNullOrEmpty(Reference)
							|| Enabled.HasValue
							|| BuildingType.HasValue
							|| FinishingType.HasValue
							|| Floor.HasValue
							|| MaxFloor.HasValue
							|| Rooms.HasValue
							|| MaxRooms.HasValue
							|| Bathrooms.HasValue
							|| MaxBathrooms.HasValue
							|| BuildingArea.HasValue
							|| MaxBuildingArea.HasValue
							|| Area.HasValue
							|| MaxArea.HasValue
							|| !string.IsNullOrEmpty(Address)
							|| DistrictId > 0
							|| CityId > 0
							|| AdType.HasValue
							|| Date.HasValue
							|| MaxDate.HasValue
							|| Price.HasValue
							|| MaxPrice.HasValue;
}