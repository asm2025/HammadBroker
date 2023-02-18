using System.Collections.Generic;
using HammadBroker.Model.DTO;

namespace HammadBroker.Data;

public class SeedData
{
	public ICollection<string> Roles { get; set; }
	public ICollection<UserData> Users { get; set; }
	public ICollection<string> Cities { get; set; }
}