using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

[DefaultExecutionOrder(-10)]
public abstract class BuildingLogic : MonoBehaviour
{
    public abstract BuildingData buildingData { get; }

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
    protected Action<bool> onWorkStartedWithSource;

    public event Action OnStartWork { add => onStartWork += value; remove => onStartWork -= value; }
    public event Action OnStopWork { add => onStopWork += value; remove => onStopWork -= value; }
    public event Action<float> OnProgressUpdate { add => onProgressUpdate += value; remove => onProgressUpdate -= value; }
    public event Action OnStateChanged { add => onStateChanged += value; remove => onStateChanged -= value; }
    public event Action<int> OnYield { add => onYield += value; remove => onYield -= value; }
    public event Action<bool> OnWorkStartedWithSource { add => onWorkStartedWithSource += value; remove => onWorkStartedWithSource -= value; }

    public virtual  long BaseBuildCost => 100;
    public virtual  long BaseUpgradeCost => 75;
    public virtual float BaseCycleTime => 5f;
    public virtual int BaseCatchAmount => 2;

    protected bool _isWorking = false;
    public bool IsWorking => _isWorking;

    public virtual float GetCycleTime() => BaseCycleTime / IndexMultiplier;
    public virtual int GetCatchAmount() => BaseCatchAmount * IndexMultiplier * Level;
    public virtual long GetBuildCost() => BaseBuildCost * IndexMultiplier;
    public virtual long GetUpgradeCost() => BaseUpgradeCost * Level * IndexMultiplier;
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

    public virtual void Build(bool bypassCost = false)
    {
        if (IsBuilt) return;
        if (!bypassCost && !CurrencyManager.Instance.TrySpendGold(GetBuildCost())) return;

        IsBuilt = true;
        Level = Mathf.Max(1, Level);
        onStateChanged?.Invoke();
    }

    public void ForceBuild(int level = 1)
    {
        IsBuilt = true;
        Level = Mathf.Clamp(level, 1, MaxLevel);
        onStateChanged?.Invoke();
    }

    public virtual void Upgrade(bool bypassCost = false)
    {
        if (!IsBuilt || Level >= MaxLevel) return;
        if (!bypassCost && !CurrencyManager.Instance.TrySpendGold(GetUpgradeCost())) return;

        Level++;
        onStateChanged?.Invoke();
    }

    public virtual void StartWork(Villager villager, CancellationToken ct)
    {
        StartWork(ct, true); 
    }

    public virtual async void StartWork(CancellationToken ct, bool isVillager = false)
    {
        if (!IsBuilt) return;

        _isWorking = true;
        onWorkStartedWithSource?.Invoke(isVillager);
        onStartWork?.Invoke();
        onProgressUpdate?.Invoke(isVillager ? -1f : 0f);

        float timer = 0f;
        float duration = Mathf.Max(0.01f, GetCycleTime());

        try
        {
            while (!ct.IsCancellationRequested)
            {
                timer += Time.deltaTime;

                if (!isVillager)
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
            _isWorking = false;
        }
    }

    public Task AwaitWorkAsync(CancellationToken ct)
    {
        if (!_isWorking)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();

        void OnStop()
        {
            OnStopWork -= OnStop;
            tcs.TrySetResult(true);
        }

        OnStopWork += OnStop;

        ct.Register(() =>
        {
            OnStopWork -= OnStop;
            tcs.TrySetCanceled();
        });

        return tcs.Task;
    }

    public void SetLevel(int newLevel)
    {
        Level = Mathf.Clamp(newLevel, 1, MaxLevel);
        onStateChanged?.Invoke();
    }

    public virtual string GetWorkAnimation() => null;
}
