using System.Collections.Generic;

namespace HammadBroker.Model.Parameters;

public class UserData : UserToUpdate
{
	public string Password { get; set; }
	public List<string> Roles { get; set; }
}