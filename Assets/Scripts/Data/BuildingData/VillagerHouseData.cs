using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VillagerLevelStats
{
	public float moveSpeed = 3f;
	public int carryCapacity = 15;
}

[CreateAssetMenu(fileName = "VillagerHouseData", menuName = "Tycoon/Villager House Data")]
public class VillagerHouseData : BuildingData
{
	public List<VillagerLevelStats> levelStats = new();

	public VillagerLevelStats GetStats(int level)
	{
		if (level < 1) level = 1;
		if (level > levelStats.Count) level = levelStats.Count;
		return levelStats[level - 1];
	}

	public override float GetCycleSeconds(int level = 0) => 0f;
	public override int GetCatchAmount(int level) => 0;
	public override int GetUpgradeCost(int level, int indexMultiplier) => 100 * level * indexMultiplier;
	public override int GetBuildCost(int indexMultiplier) => 150 * indexMultiplier;
}