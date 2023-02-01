namespace HammadBroker.Model.DTO;

public class BuildingModel
{
	private bool _canEdit;
	private bool _canUpload;

	public BuildingModel(IBuilding building)
	{
		Building = building;
	}

	public int Id { get; set; }

	public bool ReadOnly { get; set; }

	public bool CanEdit
	{
		get => !ReadOnly && _canEdit;
		set => _canEdit = value;
	}

	public bool CanUpload
	{
		get => Id > 0 && _canUpload;
		set => _canUpload = value;
	}

	public IBuilding Building { get; }
}