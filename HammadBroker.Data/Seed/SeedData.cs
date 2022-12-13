using System.Collections.Generic;
using HammadBroker.Model.DTO;

namespace HammadBroker.Data;

public class SeedData
{
	public ICollection<CitiesData> Cities { get; set; }
	public ICollection<string> Roles { get; set; }
	public ICollection<UserData> Users { get; set; }
}