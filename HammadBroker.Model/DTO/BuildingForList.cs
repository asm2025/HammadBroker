using System;
using System.ComponentModel.DataAnnotations;
using essentialMix.Extensions;

namespace HammadBroker.Model.DTO;

public class BuildingForList : IBuildingLookup
{
	/// <inheritdoc />
	[Display(Name = "رقم")]
	public string Id { get; set; }
	/// <inheritdoc />
	[Display(Name = "نوع العقار")]
	public BuildingType BuildingType { get; set; }
	public string BuildingTypeName => BuildingType.GetDisplayName();
	/// <inheritdoc />
	[Display(Name = "نوع التشطيب")]
	public FinishingType? FinishingType { get; set; }
	public string FinishingTypeName => FinishingType?.GetDisplayName();
	/// <inheritdoc />
	[Display(Name = "المدينة")]
	public int CityId { get; set; }
	[Display(Name = "نوع الاعلان")]
	public BuildingAdType AdType { get; set; }
	public string AdTypeName => AdType.GetDisplayName();
	[Display(Name = "الاولوية")]
	public byte? Priority { get; set; }
	[Display(Name = "التاريخ")]
	[DisplayFormat(DataFormatString = "{0:D}")]
	public DateTime Date { get; set; }
	[Display(Name = "ينتهي في")]
	[DisplayFormat(DataFormatString = "{0:D}")]
	public DateTime? Expires { get; set; }
}