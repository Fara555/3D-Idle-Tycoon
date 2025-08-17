using UnityEngine;

public abstract class BuildingData : ScriptableObject
{
	public string displayName = "Building";
	public int maxLevel = 1;

	public virtual string GetCycleLabel() => "Cycle";
	public virtual string GetAmountLabel() => "Amount";

	public abstract float GetCycleSeconds(int level = 0);
	public abstract int GetCatchAmount(int level);
	public abstract long GetUpgradeCost(int level, int indexMultiplier);
	public abstract long GetBuildCost(int indexMultiplier);
}