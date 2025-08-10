using System;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class CurrencyManager : MonoBehaviour
{
	public static CurrencyManager Instance { get; private set; }
	
	[Header("Current amounts")]
	[SerializeField] private long gold = 500;
	[SerializeField] private long fish = 0;
	
	public event Action<long> OnGoldChanged;
	public event Action<long> OnFishChanged;
	
	public long Gold => gold;
	public long Fish => fish;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}
	
	private void Start()
	{
		OnGoldChanged?.Invoke(gold);
		OnFishChanged?.Invoke(fish);
	}

	public bool TrySpendGold(long amount)
	{
		if (amount <= 0) return true;
		if (gold < amount) return false;
		gold -= amount;
		OnGoldChanged?.Invoke(gold);
		return true;
	}

	public void AddGold(long amount)
	{
		if (amount == 0) return;
		gold += amount;
		OnGoldChanged?.Invoke(gold);
	}
	public void AddFish(long amount)
	{
		if (amount == 0) return;
		fish += amount;
		OnFishChanged?.Invoke(fish);
	}
	
	public void SetGold(long value)
	{
		gold = value;
		OnGoldChanged?.Invoke(gold);
	}

	public void SetFish(long value)
	{
		fish = value;
		OnFishChanged?.Invoke(fish);
	} 
	
	private long _prevGold;
	private long _prevFish;
	
	private void OnValidate()
	{
		if (!Application.isPlaying) return;
		if (Instance != this) return;

		if (gold != _prevGold)
		{
			_prevGold = gold;
			OnGoldChanged?.Invoke(gold);
		}

		if (fish != _prevFish)
		{
			_prevFish = fish;
			OnFishChanged?.Invoke(fish);
		}
	}
}