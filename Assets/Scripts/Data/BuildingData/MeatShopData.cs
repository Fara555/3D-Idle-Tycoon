using UnityEngine;

[CreateAssetMenu(fileName = "MeatShopData", menuName = "Tycoon/Meat Shop Data")]
public class MeatShopData : BuildingData
{
	[Header("Economy (base values for Meat Shop)")]
	public int buildBaseCost = 120;
	public int upgradeBaseCost = 180;
	public float upgradeCostGrowth = 1.1f;

	[Header("Production")]
	public float baseCycleSeconds = 6f;
	public int baseMeatAmount = 3;
	public int meatPerLevelAdd = 1;

	public override float GetCycleSeconds(int level = 1)
	{
		return Mathf.Max(0.5f, baseCycleSeconds - 0.25f * (level - 1));
	}

	public override int GetCatchAmount(int level)
	{
		return baseMeatAmount + meatPerLevelAdd * (level - 1);
	}

	public override long GetUpgradeCost(int level, int indexMultiplier)
	{
		return Mathf.CeilToInt(upgradeBaseCost * Mathf.Pow(upgradeCostGrowth, level - 1)) * indexMultiplier;
	}

	public override long GetBuildCost(int indexMultiplier)
	{
		return buildBaseCost * indexMultiplier;
	}
}