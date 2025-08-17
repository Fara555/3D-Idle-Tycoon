using System.Collections.Generic;
using UnityEngine;

public class VillagerHouseLogic : BuildingLogic
{
	[SerializeField] private VillagerHouseData data;
	[SerializeField] private Transform villagerSpawnPoint;
	[SerializeField] private Villager villagerPrefab;

	private List<Villager> villagers = new List<Villager>();
	public override BuildingData BuildingData => data;
	public override string BuildingName => "Villager House";
	public override int MaxLevel => data.maxLevel;
	
	public override long BaseBuildCost => data.buildBaseCost;
	public override long BaseUpgradeCost => data.upgradeBaseCost;

	public IReadOnlyList<Villager> Villagers => villagers;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Build(bool bypassCost = false)
	{
		base.Build();
		SpawnVillagersIfNeeded();
		ApplyStatsToVillagers();
	}

	public override void Upgrade(bool bypassCost = false)
	{
		if (!IsBuilt || Level >= MaxLevel) return;
		if (!CurrencyManager.Instance.TrySpendGold(data.GetUpgradeCost(Level, indexMultiplier))) return;

		Level++;
		onStateChanged?.Invoke();
		SpawnVillagersIfNeeded();
	}

	public void SpawnVillagersIfNeeded(List<VillagerSave> loadedSaves = null)
	{
		if (!IsBuilt) return;

		int desiredCount = Mathf.Clamp(Level, 1, 3);

		while (villagers.Count < desiredCount)
		{
			Villager v = Instantiate(villagerPrefab, villagerSpawnPoint.position, Quaternion.identity);
		
			// 👉 Принудительно генерируем новый ID при создании
			var idComp = v.GetComponent<UniqueId>();
			if (idComp != null)
				idComp.GenerateNewId(); 

			v.Initialize(this);
			villagers.Add(v);
		}

		ApplyStatsToVillagers();

		// если переданы сохранённые ID — маппим их обратно
		if (loadedSaves != null)
		{
			for (int i = 0; i < villagers.Count; i++)
			{
				if (i >= loadedSaves.Count) break;
				var villager = villagers[i];
				var idComp = villager.GetComponent<UniqueId>();
				if (idComp != null)
					idComp.LoadId(loadedSaves[i].villagerId); // 👈 добавим этот метод в UniqueId
			}
		}
	}


	private void ApplyStatsToVillagers()
	{
		var stats = data.GetStats(Level);
		foreach (var v in villagers)
			v.ApplyStats(stats.moveSpeed, stats.carryCapacity);
	}

	public override float GetCycleTime()
	{
		// Время цикла берём напрямую из данных — без множителя
		return data.GetStats(Level).moveSpeed; // или data.GetCycleSeconds(Level), если используешь другой способ
	}

	public override int GetCatchAmount()
	{
		return data.GetStats(Level).carryCapacity;
	}

	public override string GetCycleLabel() => "Speed";
	public override string GetAmountLabel() => "Fish Capacity";
}