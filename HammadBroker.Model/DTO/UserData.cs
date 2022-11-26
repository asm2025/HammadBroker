using System.Collections.Generic;

namespace HammadBroker.Model.DTO;

public class UserData : UserToUpdateCore
{
	public string Password { get; set; }
	public List<string> Roles { get; set; }
}