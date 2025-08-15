using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillagerHouseWindowUI : BuildingWindowUI
{
	[Header("Villager Set Up")]
	[SerializeField] private Transform villagerListRoot;
	[SerializeField] private VillagerSlotUI slotPrefab;

	private VillagerHouseLogic _villagerLogic;
	
	public override void Show(BuildingLogic logic)
	{
		_villagerLogic = logic as VillagerHouseLogic;
		base.Show(logic); 
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();

		if (_villagerLogic == null || !_villagerLogic.IsBuilt || _villagerLogic.Villagers == null || _villagerLogic.Villagers.Count == 0)
		{
			villagerListRoot.gameObject.SetActive(false);
			return;
		}

		villagerListRoot.gameObject.SetActive(true);
		
		foreach (Transform child in villagerListRoot)
			Destroy(child.gameObject);

		foreach (var v in _villagerLogic.Villagers)
		{
			var slot = Instantiate(slotPrefab, villagerListRoot);
			slot.Setup(v);
		}
	}
}