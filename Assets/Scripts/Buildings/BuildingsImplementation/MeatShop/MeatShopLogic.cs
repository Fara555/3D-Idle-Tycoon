using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

/// <summary>
/// Логика мясной лавки. Рабочее место, в котором персонаж "заходит внутрь".
/// </summary>
public class MeatShopLogic : BuildingLogic, IWorkplace
{
    [SerializeField] private MeatShopData data;
    [SerializeField] private Transform workPoint;

    private Villager currentWorker;

    public override int MaxLevel => data.maxLevel;
    public override string BuildingName => data.displayName;
    public override BuildingData BuildingData => data;

    public bool IsOccupied => currentWorker != null;
    public Villager CurrentOccupant => currentWorker;

    public void SetOccupant(Villager villager)
    {
        currentWorker = villager;
    }

    public Vector3 GetWorkPoint()
    {
        return workPoint ? workPoint.position : transform.position;
    }

    public UTask PerformWork(Villager villager, CancellationToken ct)
    {
        return new UTask(PerformVillagerWorkAsync(villager, ct));
    }

    private async Task PerformVillagerWorkAsync(Villager villager, CancellationToken ct)
    {
        currentWorker = villager;

        // Спрятать жителя — он "зашёл внутрь"
        var renderers = villager.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        float timer = 0f;
        float duration = Mathf.Max(0.01f, GetCycleTime());

        while (!ct.IsCancellationRequested && timer < duration)
        {
            timer += Time.deltaTime;
            await UTaskEx.NextFrame(ct);
        }

        if (!ct.IsCancellationRequested)
        {
            int amount = GetCatchAmount();
            currentWorker.CollectFish(amount);
            onYield?.Invoke(amount);
        }

        // Появляется снова
        foreach (var r in renderers)
            r.enabled = true;

        currentWorker = null;
    }

    public override async void StartWork(CancellationToken ct, bool isVillager = false)
    {
        if (!IsBuilt || isVillager) return; // игрокская работа

        _isWorking = true;
        onWorkStartedWithSource?.Invoke(isVillager);
        onStartWork?.Invoke();
        onProgressUpdate?.Invoke(0f);

        var player = FindObjectOfType<СharacterMover>();
        Renderer[] renderers = null;

        if (player != null)
        {
            renderers = player.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.enabled = false;
        }

        float timer = 0f;
        float duration = Mathf.Max(0.01f, GetCycleTime());

        try
        {
            while (!ct.IsCancellationRequested && timer < duration)
            {
                timer += Time.deltaTime;
                onProgressUpdate?.Invoke(timer / duration);
                await UTaskEx.NextFrame(ct);
            }

            if (!ct.IsCancellationRequested)
            {
                int amount = GetCatchAmount();
                CurrencyManager.Instance.AddFish(amount); // Или AddMeat(...) если будет такая валюта
                onYield?.Invoke(amount);
            }
        }
        catch { /* Ignored */ }
        finally
        {
            if (renderers != null)
            {
                foreach (var r in renderers)
                    r.enabled = true;
            }

            onProgressUpdate?.Invoke(0f);
            onStopWork?.Invoke();
            _isWorking = false;
        }
    }

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

    public override int GetBuildCost() => data.GetBuildCost(IndexMultiplier);
    public override int GetUpgradeCost() => data.GetUpgradeCost(Level, IndexMultiplier);
    public override string GetCycleLabel() => "Cycle";
    public override string GetAmountLabel() => "Meat";
    public override string GetWorkAnimation() => null;
}
