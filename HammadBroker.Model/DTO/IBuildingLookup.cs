using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public interface IBuildingLookup
{
	[Display(Name = "مرجع")]
	string Reference { get; set; }
	[Display(Name = "نوع العقار")]
	BuildingType BuildingType { get; set; }
	string BuildingTypeName { get; }
	[Display(Name = "نوع التشطيب")]
	FinishingType? FinishingType { get; set; }
	string FinishingTypeName { get; }
	[Display(Name = "الحي")]
	int? DistrictId { get; set; }
	[Display(Name = "المدينة")]
	int CityId { get; set; }
	[Display(Name = "نوع الاعلان")]
	BuildingAdType AdType { get; set; }
	string AdTypeName { get; }
	[Display(Name = "الاولوية")]
	byte? Priority { get; set; }
	[Display(Name = "نشر")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	DateTime CreatedOn { get; set; }
	[Display(Name = "حدث")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
	DateTime UpdatedOn { get; set; }
	[Display(Name = "التاريخ")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
	DateTime? Expires { get; set; }

	[Display(Name = "مفعل")]
	bool Enabled { get; set; }
}