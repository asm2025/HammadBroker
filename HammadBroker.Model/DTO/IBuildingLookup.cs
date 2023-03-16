using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuildingLookup
{
	[Display(Name = "مرجع")]
	public string Reference { get; set; }
	[Display(Name = "نوع العقار")]
	BuildingType BuildingType { get; set; }
	string BuildingTypeName { get; }
	[Display(Name = "نوع التشطيب")]
	FinishingType? FinishingType { get; set; }
	string FinishingTypeName { get; }
	[Display(Name = "الحي")]
	public int? DistrictId { get; set; }
	[Display(Name = "المدينة")]
	int CityId { get; set; }
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType AdType { get; set; }
	public string AdTypeName { get; }
	[Display(Name = "الاولوية")]
	public byte? Priority { get; set; }
	[Display(Name = "نشر")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	public DateTime CreatedOn { get; set; }
	[Display(Name = "حدث")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	public DateTime UpdatedOn { get; set; }
	[Display(Name = "التاريخ")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	public DateTime? Expires { get; set; }
}