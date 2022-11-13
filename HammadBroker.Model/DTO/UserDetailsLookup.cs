using HammadBroker.Model.Entities;

namespace HammadBroker.Model.DTO;

public class UserDetailsLookup
{
    public ApplicationUser User { get; set; }
    public City City { get; set; }
    public Country Country { get; set; }
}