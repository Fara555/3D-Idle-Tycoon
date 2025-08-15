using UnityEngine;

[CreateAssetMenu(menuName = "Tycoon/Lighthouse Data", fileName = "LighthouseData")]
public class LighthouseData : BuildingData
{
	[Header("Economy (base values for lighthouse)")]
	public int buildBaseCost = 100;
	public int upgradeBaseCost = 75;
	public float upgradeCostGrowth = 1.2f;

	[Header("Production")]
	public float baseCycleSeconds = 0.2f;
	public int baseGoldPerFish = 2;
	public int goldPerLevelAdd = 1;

	public override float GetCycleSeconds(int level = 0)
	{
		return baseCycleSeconds;
	}

	public override int GetCatchAmount(int level)
	{
		return baseGoldPerFish + (level - 1) * goldPerLevelAdd;
	}

	public override int GetUpgradeCost(int level, int indexMultiplier)
	{
		level = Mathf.Clamp(level, 1, maxLevel - 1);
		double cost = upgradeBaseCost * System.Math.Pow(upgradeCostGrowth, level - 1);
		return Mathf.CeilToInt((float)cost) * indexMultiplier;
	}

	public override int GetBuildCost(int indexMultiplier)
	{
		return buildBaseCost * indexMultiplier;
	}

	public override string GetCycleLabel() => "Speed";
	public override string GetAmountLabel() => "Fish Price";
}