using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

public static class LighthouseLocator
{
	private static LightHouseLogic _cached;
	private static Transform _sellPointCached;

	public static LightHouseLogic Find()
	{
		if (_cached != null) return _cached;

		_cached = GameObject.FindObjectOfType<LightHouseLogic>();
		return _cached;
	}

	public static Vector3 GetSellPoint()
	{
		if (_sellPointCached != null) return _sellPointCached.position;

		var logic = Find();
		var interactable = logic?.GetComponent<BuildingInteractable>();
		_sellPointCached = interactable?.transform; // можно заменить на специальную точку
		return _sellPointCached != null ? _sellPointCached.position : Vector3.zero;
	}

	public static async Task<Vector3> GetSellPointAsync()
	{
		await UTaskEx.NextFrame(); // вдруг здание ещё не прогрузилось
		return GetSellPoint();
	}

	public static async Task SellFishAsync(int amount)
	{
		var logic = Find();
		if (logic == null || amount <= 0) return;

		bool spent = CurrencyManager.Instance.TrySpendFish(amount);
		if (spent)
		{
			long gold = amount * logic.BuildingData.GetCatchAmount(logic.Level);
			CurrencyManager.Instance.AddGold(gold);
			logic.InvokeYield(gold); // кастомный метод для уведомлений UI и анимаций
		}
	}
}