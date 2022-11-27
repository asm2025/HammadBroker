using System.Collections.Generic;

namespace HammadBroker.Model.DTO;

public class UserData : UserToUpdateCore
{
	public string UserName { get; set; }
	public bool EmailConfirmed { get; set; }
	public bool PhoneNumberConfirmed { get; set; }
	public string Password { get; set; }
	public List<string> Roles { get; set; }
}