using System;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class CurrencyManager : MonoBehaviour
{
	public static CurrencyManager Instance { get; private set; }
	
	[Header("Current amounts")]
	[SerializeField] private long gold = 500;
	[SerializeField] private long fish = 0;
	[SerializeField] private long maxFishCapacity = 0;
	
	public event Action<long> OnGoldChanged;
	public event Action<long, long> OnFishChanged;
	
	public long Gold => gold;
	public long Fish => fish;
	public long MaxFishCapacity => maxFishCapacity;
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
		OnFishChanged?.Invoke(fish, maxFishCapacity);
	}

	public bool TrySpendGold(long amount)
	{
		if (amount <= 0) return true;
		if (gold < amount) return false;
		gold -= amount;
		OnGoldChanged?.Invoke(gold);
		return true;
	}	
	
	public bool TrySpendFish(long amount)
	{
		if (amount <= 0) return true;
		if (fish < amount) return false;
		fish -= amount;
		OnFishChanged?.Invoke(fish, maxFishCapacity);
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
		fish = Math.Min(fish, maxFishCapacity);

		OnFishChanged?.Invoke(fish, maxFishCapacity);
	}
	
	public void SetGold(long value)
	{
		gold = value;
		OnGoldChanged?.Invoke(gold);
	}

	public void SetFish(long value)
	{
		fish = Math.Min(value, maxFishCapacity);
		OnFishChanged?.Invoke(fish, maxFishCapacity);
	}
	
	public void SetMaxFishCapacity(long value)
	{
		maxFishCapacity = Math.Max(0, value);
		fish = Math.Min(fish, maxFishCapacity); 
		OnFishChanged?.Invoke(fish, maxFishCapacity);
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
			OnFishChanged?.Invoke(fish, maxFishCapacity);
		}
	}
}