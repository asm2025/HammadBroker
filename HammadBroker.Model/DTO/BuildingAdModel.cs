using JetBrains.Annotations;

namespace HammadBroker.Model.DTO;

public class BuildingAdModel : BuildingModel
{
	public BuildingAdModel(IBuildingAd ad)
		: this(ad, null)
	{
	}

	public BuildingAdModel([NotNull] IBuildingAd ad, IBuilding building)
		: base(building)
	{
		Ad = ad;
	}

	public IBuildingAd Ad { get; set; }
}
