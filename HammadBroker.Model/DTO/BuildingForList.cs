namespace HammadBroker.Model.DTO;

public class BuildingForList
{
	public int Id { get; set; }
	public string Name { get; set; }
	public BuildingType BuildingType { get; set; }
	public FinishingType FinishingType { get; set; }
	public int? CityId { get; set; }
	public string City { get; set; }
	public string CountryCode { get; set; }
	public string Country { get; set; }
	public string ImageUrl { get; set; }
}