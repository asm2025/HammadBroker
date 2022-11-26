using System;

namespace HammadBroker.Model.DTO;

public class BuildingAdForDetails : BuildingForDetails
{
	public int BuildingId { get; set; }
	public DateTime Date { get; set; }
	public DateTime? Expires { get; set; }
	public string Phone { get; set; }
	public string Mobile { get; set; }
	public decimal Price { get; set; }
}