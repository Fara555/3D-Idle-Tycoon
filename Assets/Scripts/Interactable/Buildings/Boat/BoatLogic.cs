using UnityEngine;

public class BoatLogic : BuildingLogic
{
	[SerializeField] private BoatData data;

	public override int MaxLevel => data.maxLevel;
	public override string BuildingName => data.displayName;
	
	public override BuildingData BuildingData => data;


	public override float GetCycleTime()
	{
		float baseTime = data.GetCycleSeconds(Level);
		float reductionFactor = 1f - 0.05f * (IndexMultiplier - 1); 
		return Mathf.Max(0.5f, baseTime * reductionFactor); 
	}

	public override int GetCatchAmount()
	{
		int baseAmount = data.GetCatchAmount(Level);
		return baseAmount * IndexMultiplier;
	}

	public override int GetBuildCost()
	{
		return data.GetBuildCost(IndexMultiplier);
	}

	public override int GetUpgradeCost()
	{
		return data.GetUpgradeCost(Level, IndexMultiplier);
	}

	public override string GetCycleLabel() => "Cycle";
	public override string GetAmountLabel() => "Catch";

	public override string GetWorkAnimation() => "Fishing Cast";
}