using System;
using System.Threading;
using UnityEngine;
using Utilities.UTask;

[DefaultExecutionOrder(-10)]
public abstract class BuildingLogic : MonoBehaviour
{
    public abstract BuildingData BuildingData { get; }
    
    [Header("State")]
    [SerializeField] private bool startBuilt = false;
    
    [Header("Balance")]
    [SerializeField] protected int indexMultiplier = 1;
    
    public int IndexMultiplier => Mathf.Max(1, indexMultiplier);
    public bool IsBuilt { get; protected set; }
    public int Level { get; protected set; } = 1;
    
    public abstract int MaxLevel { get; }
    public abstract string BuildingName { get; }
    
    protected Action onStartWork;
    protected Action onStopWork;
    protected Action<float> onProgressUpdate; // 0..1
    protected Action onStateChanged;
    protected Action<int> onYield;
    
    public event Action OnStartWork { add => onStartWork += value; remove => onStartWork -= value; }
    public event Action OnStopWork { add => onStopWork += value; remove => onStopWork -= value; }
    public event Action<float> OnProgressUpdate { add => onProgressUpdate += value; remove => onProgressUpdate -= value; }
    public event Action OnStateChanged { add => onStateChanged += value; remove => onStateChanged -= value; }
    public event Action<int> OnYield { add => onYield += value; remove => onYield -= value; }
    
    public virtual int BaseBuildCost => 100;
    public virtual int BaseUpgradeCost => 75;
    public virtual float BaseCycleTime => 5f;
    public virtual int BaseCatchAmount => 2;
    
    public virtual float GetCycleTime() => BaseCycleTime / IndexMultiplier;
    public virtual int GetCatchAmount() => BaseCatchAmount * IndexMultiplier * Level;
    public virtual int GetBuildCost() => BaseBuildCost * IndexMultiplier;
    public virtual int GetUpgradeCost() => BaseUpgradeCost * Level * IndexMultiplier;
    public virtual string GetCycleLabel() => "Cycle";
    public virtual string GetAmountLabel() => "Amount";

    
    protected virtual void Awake()
    {
        if (startBuilt)
        {
            IsBuilt = true;
            Level = Mathf.Max(1, Level);
        }
        onStateChanged?.Invoke();
    }

    public virtual void Build()
    {
        if (IsBuilt) return;
        if (!CurrencyManager.Instance.TrySpendGold(GetBuildCost())) return;

        IsBuilt = true;
        Level = Mathf.Max(1, Level);
        onStateChanged?.Invoke();
    }

    public virtual void Upgrade()
    {
        if (!IsBuilt || Level >= MaxLevel) return;
        if (!CurrencyManager.Instance.TrySpendGold(GetUpgradeCost())) return;

        Level++;
        onStateChanged?.Invoke();
    }

    public virtual async void StartWork(CancellationToken ct)
    {
        if (!IsBuilt) return;

        onStartWork?.Invoke();
        float timer = 0f;
        float duration = Mathf.Max(0.01f, GetCycleTime());

        try
        {
            while (!ct.IsCancellationRequested)
            {
                timer += Time.deltaTime;
                onProgressUpdate?.Invoke(Mathf.Clamp01(timer / duration));

                if (timer >= duration)
                {
                    timer -= duration;
                    int amount = GetCatchAmount();
                    CurrencyManager.Instance.AddFish(amount);
                    onYield?.Invoke(amount); 
                }

                await UTaskEx.NextFrame(ct);
            }
        }
        catch { /* cancelled */ }
        finally
        {
            onProgressUpdate?.Invoke(0f);
            onStopWork?.Invoke();
        }
    }
    
    public virtual string GetWorkAnimation() => null;
}
