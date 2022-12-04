using System;

namespace HammadBroker.Model.Parameters;

public class BuildingAdList : BuildingList
{
	public BuildingAdType? Type { get; set; }
	public DateTime? Date { get; set; }
	public DateTime? MaxDate { get; set; }
	public decimal? Price { get; set; }
	public decimal? MaxPrice { get; set; }
}