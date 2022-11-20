using System;

namespace HammadBroker.Model.DTO;

public class BuildingAdForList : BuildingForList
{
	public int BuildingId { get; set; }
	public DateTime Date { get; set; }
	public string Phone { get; set; }
	public string Mobile { get; set; }
	public decimal Price { get; set; }
}