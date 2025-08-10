using UnityEngine;

[CreateAssetMenu(menuName = "Tycoon/Boat Data", fileName = "BoatData")]
public class BoatData : ScriptableObject
{
	[Header("Base")]
	public string displayName = "Boat";
	public int maxLevel = 30;

	[Header("Economy (base values for 1-й лодки)")]
	public int buildBaseCost = 100;     
	public int upgradeBaseCost = 50;     

	[Tooltip("Степень роста стоимости (например 1.15f)")]
	public float upgradeCostGrowth = 1.15f;

	[Header("Production (за цикл)")]
	public float baseCycleSeconds = 4f;   
	public int   baseCatchAmount  = 3;   

	[Header("Level scaling")]
	[Tooltip("Множитель скорости на уровень (меньше = быстрее). 0.98 = -2% длительности за уровень")]
	public float cycleTimePerLevelMult = 0.98f;
	[Tooltip("Прибавка к улову на уровень")]
	public int catchPerLevelAdd = 1;

	public float GetCycleSeconds(int level)
	{
		level = Mathf.Clamp(level, 1, maxLevel);
		return baseCycleSeconds * Mathf.Pow(cycleTimePerLevelMult, level - 1);
	}
	public int GetCatchAmount(int level)
	{
		level = Mathf.Clamp(level, 1, maxLevel);
		return baseCatchAmount + (level - 1) * catchPerLevelAdd;
	}
	public int GetUpgradeCost(int level, int boatIndexMultiplier)
	{
		// level -> cost to go to next level (уровень N -> апгрейд до N+1)
		int lvl = Mathf.Clamp(level, 1, maxLevel - 1);
		double cost = upgradeBaseCost * System.Math.Pow(upgradeCostGrowth, lvl - 1);
		return Mathf.CeilToInt((float)cost) * boatIndexMultiplier;
	}
	public int GetBuildCost(int boatIndexMultiplier)
	{
		return buildBaseCost * boatIndexMultiplier;
	}
}