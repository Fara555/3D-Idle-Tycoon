using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-101)]
public static class SaveManager
{
	private static readonly string savePath = Path.Combine(Application.persistentDataPath, "save.json");
	
	public static bool HasSave() => File.Exists(savePath);
	
	public static bool DeleteSave()
	{
		try
		{
			if (File.Exists(savePath))
			{
				File.Delete(savePath);
				Debug.Log($"[SaveManager] Save deleted: {savePath}");
				return true;
			}
			Debug.Log("[SaveManager] No save file to delete.");
		}
		catch (Exception e)
		{
			Debug.LogError($"[SaveManager] Delete failed: {e.Message}");
		}
		return false;
	}

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
							assignedWorkplaceId = wId,
							originHouseId = bId
						});
					}
				}
			}
		}

		data.lastOnlineBinaryTime = DateTime.Now.ToBinary();

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
						var villagersForHouse = data.villagers
							.FindAll(v => v.originHouseId == buildingSave.buildingId);

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

		// Offline progress
		long earnedGold = 0;

		if (data.lastOnlineBinaryTime != 0)
		{
			try
			{
				var lastOnline = DateTime.FromBinary(data.lastOnlineBinaryTime);
				var timeAway = DateTime.Now - lastOnline;

				var secondsAway = Math.Max(0, timeAway.TotalSeconds);
				if (secondsAway >= 1.0)
					earnedGold = OfflineProgressCalculator.Apply(allBuildings, secondsAway);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[Offline] Failed to parse lastOnlineBinaryTime: {data.lastOnlineBinaryTime}. {e.Message}");
			}
		}

		if (earnedGold > 0)
		{
			OfflinePopupUI.Show(earnedGold);
			Debug.Log($"Earned {earnedGold} gold from offline progress.");
		}

		Debug.Log("Game Loaded.");
	}
}