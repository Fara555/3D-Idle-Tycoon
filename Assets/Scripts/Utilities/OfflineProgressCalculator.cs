using System.Collections.Generic;
using UnityEngine;

public static class OfflineProgressCalculator
{
	public static long Apply(List<BuildingLogic> buildings, double seconds)
	{
		double cappedSeconds = Mathf.Min((int)seconds, 6 * 3600); 
		long totalGold = 0;

		foreach (var b in buildings)
		{
			if (!b.IsBuilt) continue;
			if (b is not BoatLogic && b is not MeatShopLogic) continue;

			VillagerHouseLogic house = FindOwnerHouse(b, buildings);
			if (house == null) continue;

			var stats = house.buildingData is VillagerHouseData vData
				? vData.GetStats(house.Level)
				: null;

			if (stats == null) continue;

			float carryCapacity = stats.carryCapacity;
			float cycleTime = Mathf.Max(0.01f, b.GetCycleTime());
			int amountPerCycle = b.GetCatchAmount();
			float fishPerSecond = amountPerCycle / cycleTime;

			float timeToFill = carryCapacity / fishPerSecond;
			float travelTime = 5f;
			float sellTime = 10f;

			float fullTripTime = timeToFill + travelTime + sellTime;
			int trips = Mathf.FloorToInt((float)(cappedSeconds / fullTripTime));
			int totalFish = Mathf.FloorToInt(carryCapacity * trips);

			int fishPrice = GetFishPrice();
			totalGold += totalFish * fishPrice;
		}

		if (totalGold > 0)
		{
			CurrencyManager.Instance.AddGold(totalGold);
			Debug.Log($"[Offline] Gained {totalGold} gold from villagers over {cappedSeconds:F0} seconds");
		}

		return totalGold;
	}

	private static int GetFishPrice()
	{
		var light = LighthouseLocator.Find();
		if (light == null || !light.IsBuilt) return 1;
		return light.buildingData.GetCatchAmount(light.Level);
	}

	private static VillagerHouseLogic FindOwnerHouse(BuildingLogic workplace, List<BuildingLogic> all)
	{
		foreach (var b in all)
		{
			if (b is VillagerHouseLogic house)
			{
				foreach (var v in house.Villagers)
				{
					if (v.CurrentWorkplace == workplace)
						return house;
				}
			}
		}
		return null;
	}
}