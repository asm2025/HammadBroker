namespace HammadBroker.Model.DTO;

public class BuildingForDetails : BuildingForList
{
	public byte? Floor { get; set; }
	public byte? Rooms { get; set; }
	public byte? Bathrooms { get; set; }
	public decimal? Area { get; set; }
	public string Location { get; set; }
	public string Address { get; set; }
	public string Address2 { get; set; }
	public string VideoUrl { get; set; }
	public string Description { get; set; }
}