using UnityEngine;

[CreateAssetMenu(menuName = "Tycoon/Boat Data", fileName = "BoatData")]
public class BoatData : BuildingData
{
	[Header("Economy (base values for boat)")]
	public int buildBaseCost = 100;
	public int upgradeBaseCost = 50;
	public float upgradeCostGrowth = 1.15f;

	[Header("Production)")]
	public float baseCycleSeconds = 4f;
	public int baseCatchAmount = 3;

	[Header("Level scaling")]
	public float cycleTimePerLevelMult = 0.98f;
	public int catchPerLevelAdd = 1;

	public override float GetCycleSeconds(int level = 0)
	{
		level = Mathf.Clamp(level, 1, maxLevel);
		return baseCycleSeconds * Mathf.Pow(cycleTimePerLevelMult, level - 1);
	}

	public override int GetCatchAmount(int level)
	{
		level = Mathf.Clamp(level, 1, maxLevel);
		return baseCatchAmount + (level - 1) * catchPerLevelAdd;
	}

	public override long GetUpgradeCost(int level, int boatIndexMultiplier)
	{
		int lvl = Mathf.Clamp(level, 1, maxLevel - 1);
		double cost = upgradeBaseCost * System.Math.Pow(upgradeCostGrowth, lvl - 1);
		return Mathf.CeilToInt((float)cost) * boatIndexMultiplier;
	}

	public override long GetBuildCost(int boatIndexMultiplier)
	{
		return buildBaseCost * boatIndexMultiplier;
	}

	public override string GetCycleLabel() => "Cycle";
	public override string GetAmountLabel() => "Catch";
}