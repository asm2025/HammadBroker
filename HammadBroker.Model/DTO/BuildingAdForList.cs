using System;

namespace HammadBroker.Model.DTO;

public class BuildingAdForList : BuildingForList
{
	public BuildingAdType Type { get; set; }
	public int BuildingId { get; set; }
	public DateTime Date { get; set; }
	public DateTime? Expires { get; set; }
	public string Phone { get; set; }
	public string Mobile { get; set; }
	public decimal Price { get; set; }
	public long Views { get; set; }
	public long PageViews { get; set; }
	public long Requests { get; set; }
}