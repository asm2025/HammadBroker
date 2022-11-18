using System.Collections.Generic;
using HammadBroker.Model.Parameters;

namespace HammadBroker.Model;

public class SeedData
{
	public List<string> Roles { get; set; }
	public List<UserData> Users { get; set; }
	public List<string> Floors { get; set; }
}