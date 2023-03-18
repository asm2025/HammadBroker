using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.Parameters;

public class DistrictList : SearchList
{
	[Display(Name = "المدينة")]
	public int CityId { get; set; }

	public override bool HasSearch => base.HasSearch || CityId > 0;
}