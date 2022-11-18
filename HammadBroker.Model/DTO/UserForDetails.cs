using System;

namespace HammadBroker.Model.DTO;

public class UserForDetails : UserForList
{
	public string PhoneNumber { get; set; }
	public Genders Gender { get; set; }
	public DateTime DateOfBirth { get; set; }
	public DateTime Created { get; set; }
	public DateTime Modified { get; set; }
}