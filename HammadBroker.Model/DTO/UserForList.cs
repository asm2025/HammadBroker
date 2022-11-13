namespace HammadBroker.Model.DTO;

public class UserForList : UserForLoginDisplay
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string CountryCode { get; set; }
	public int? CityId { get; set; }
}