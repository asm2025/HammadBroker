using System.Collections.Generic;

namespace HammadBroker.Model.DTO;

public interface IBuildingImagesLookup
{
	ICollection<string> Images { get; set; }
}