using System.Collections.Generic;
using HammadBroker.Model.Parameters;

namespace HammadBroker.Model;

public class SeedData
{
	public List<string> Roles { get; set; }
	public List<UserData> Users { get; set; }
	public List<string> BuildingTypes { get; set; }
	public List<string> FloorTypes { get; set; }
	public List<string> FinishingTypes { get; set; }
}