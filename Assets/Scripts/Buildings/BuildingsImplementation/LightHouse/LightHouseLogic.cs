using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

public class LightHouseLogic : BuildingLogic
{
    [SerializeField] private LighthouseData data;

    public override int MaxLevel => data.maxLevel;
    public override string BuildingName => data.displayName;
    public override BuildingData BuildingData => data;

    public override float GetCycleTime() => data.GetCycleSeconds();
    public override long GetBuildCost() => data.GetBuildCost(IndexMultiplier);
    public override long GetUpgradeCost() => data.GetUpgradeCost(Level, IndexMultiplier);
    public override int GetCatchAmount() => data.GetCatchAmount(Level);

    // Игрок запускает
    public override void StartWork(CancellationToken ct, bool isVillagerSale = false)
    {
        _ = InternalStartWorkAsync(ct, isVillagerSale);
    }

    // Житель запускает
    public override void StartWork(Villager villager, CancellationToken ct)
    {
        _ = InternalStartWorkAsync(ct, true, villager);
    }

    // ✅ Асинхронная продажа с возвращаемым Task
    public Task StartWorkAsync(CancellationToken ct, bool isVillagerSale, Villager sourceVillager)
    {
        return InternalStartWorkAsync(ct, isVillagerSale, sourceVillager);
    }

    private async Task InternalStartWorkAsync(CancellationToken ct, bool isVillagerSale, Villager sourceVillager = null)
    {
        if (!IsBuilt)
        {
            onStopWork?.Invoke();
            return;
        }

        bool isVillager = sourceVillager != null;
        long carriedFish = isVillager
            ? sourceVillager.CarriedFish
            : Math.Min(CurrencyManager.Instance.Fish, CurrencyManager.Instance.MaxFishCapacity);

        if (carriedFish <= 0)
        {
            onStopWork?.Invoke();
            return;
        }

        long maxFish = CurrencyManager.Instance.MaxFishCapacity;
        float fullCycleTime = data.baseCycleSeconds;
        float cycleTime = isVillager
            ? fullCycleTime                     
            : fullCycleTime * ((float)carriedFish / maxFish);
        onWorkStartedWithSource?.Invoke(isVillagerSale);
        onStartWork?.Invoke();
        onProgressUpdate?.Invoke(isVillagerSale ? -1f : 0f);

        float timer = 0f;

        try
        {
            while (!ct.IsCancellationRequested && timer < cycleTime)
            {
                timer += Time.deltaTime;
                if (!isVillagerSale)
                    onProgressUpdate?.Invoke(Mathf.Clamp01(timer / cycleTime));
                await UTaskEx.NextFrame(ct);
            }
        }
        catch { }

        onProgressUpdate?.Invoke(0f);

        long actualSoldFish = 0;

        if (isVillager)
        {
            actualSoldFish = sourceVillager.CarriedFish;
            sourceVillager.ResetFish();
        }
        else
        {
            actualSoldFish = Math.Min(CurrencyManager.Instance.Fish, CurrencyManager.Instance.MaxFishCapacity);
            bool spent = CurrencyManager.Instance.TrySpendFish(actualSoldFish);
            if (!spent) actualSoldFish = 0;
        }

        long totalGold = actualSoldFish * data.GetCatchAmount(Level);
        CurrencyManager.Instance.AddGold(totalGold);
        onYield?.Invoke((int)totalGold);

        onStopWork?.Invoke();
    }

    public void InvokeYield(long gold)
    {
        onYield?.Invoke((int)gold);
    }

    public override string GetCycleLabel() => "Speed";
    public override string GetAmountLabel() => "Fish Price";
}
