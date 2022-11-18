using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model;

public enum BuildingType
{
    [Display(Name = "شقة")]
    Apartment,
    [Display(Name = "فيلا")]
    Villa,
    [Display(Name = "محل")]
    Shop,
    [Display(Name = "كافتيريا")]
    Cafeteria
}