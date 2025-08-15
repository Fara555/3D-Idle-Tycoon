using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-101)]
public static class SaveManager
{
	private static readonly string savePath = Path.Combine(Application.persistentDataPath, "save.json");

	public static void SaveGame(List<BuildingLogic> allBuildings)
	{
		var data = new SaveData
		{
			gold = CurrencyManager.Instance.Gold,
			fish = CurrencyManager.Instance.Fish,
			maxFishCapacity = CurrencyManager.Instance.MaxFishCapacity
		};

		foreach (var b in allBuildings)
		{
			var bId = b.GetComponent<UniqueId>()?.Id;
			if (string.IsNullOrEmpty(bId)) continue;

			data.buildings.Add(new BuildingSave
			{
				buildingId = bId,
				isBuilt = b.IsBuilt,
				level = b.Level
			});

			if (b is VillagerHouseLogic house)
			{
				foreach (var villager in house.Villagers)
				{
					var vId = villager.GetComponent<UniqueId>()?.Id;
					var wId = (villager.CurrentWorkplace as MonoBehaviour)?.GetComponent<UniqueId>()?.Id;

					if (!string.IsNullOrEmpty(vId))
					{
						data.villagers.Add(new VillagerSave
						{
							villagerId = vId,
							assignedWorkplaceId = wId
						});
					}
				}
			}
		}

		string json = JsonUtility.ToJson(data, true);
		File.WriteAllText(savePath, json);
		Debug.Log("Game Saved to: " + savePath);
	}

	public static void LoadGame(List<BuildingLogic> allBuildings)
	{
		if (!File.Exists(savePath))
		{
			Debug.LogWarning("Save file not found.");
			return;
		}

		string json = File.ReadAllText(savePath);
		SaveData data = JsonUtility.FromJson<SaveData>(json);

		CurrencyManager.Instance.SetGold(data.gold);
		CurrencyManager.Instance.SetMaxFishCapacity(data.maxFishCapacity);
		CurrencyManager.Instance.SetFish(data.fish);

		var buildingById = new Dictionary<string, BuildingLogic>();
		var workplaceById = new Dictionary<string, IWorkplace>();

		foreach (var b in allBuildings)
		{
			var id = b.GetComponent<UniqueId>()?.Id;
			if (string.IsNullOrEmpty(id)) continue;

			buildingById[id] = b;
			if (b is IWorkplace wp)
				workplaceById[id] = wp;
		}

		foreach (var buildingSave in data.buildings)
		{
			if (buildingById.TryGetValue(buildingSave.buildingId, out var building))
			{
				if (buildingSave.isBuilt)
				{
					building.ForceBuild(buildingSave.level);

					if (building is VillagerHouseLogic house)
					{
						var villagersForHouse = data.villagers.FindAll(v => !string.IsNullOrEmpty(v.villagerId));
						house.SpawnVillagersIfNeeded(villagersForHouse);
					}
				}
			}
		}

		foreach (var b in allBuildings)
		{
			if (b is VillagerHouseLogic house)
			{
				foreach (var villager in house.Villagers)
				{
					var vId = villager.GetComponent<UniqueId>()?.Id;
					var villagerSave = data.villagers.Find(v => v.villagerId == vId);
					if (villagerSave != null && !string.IsNullOrEmpty(villagerSave.assignedWorkplaceId))
					{
						if (workplaceById.TryGetValue(villagerSave.assignedWorkplaceId, out var wp))
						{
							villager.AssignWorkplace(wp);
							wp.SetOccupant(villager);
						}
					}
				}
			}
		}

		Debug.Log("Game Loaded.");
	}
}
