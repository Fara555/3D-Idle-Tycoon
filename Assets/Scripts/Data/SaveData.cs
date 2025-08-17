using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
	public long gold;
	public long fish;
	public long maxFishCapacity;
	
	public List<BuildingSave> buildings = new();
	public List<VillagerSave> villagers = new();
	
	public long lastOnlineBinaryTime;
}

[System.Serializable]
public class VillagerSave
{
	public string villagerId;
	public string assignedWorkplaceId;
	public string originHouseId; 
}

[System.Serializable]
public class BuildingSave
{
	public string buildingId;
	public bool isBuilt;
	public int level;
}