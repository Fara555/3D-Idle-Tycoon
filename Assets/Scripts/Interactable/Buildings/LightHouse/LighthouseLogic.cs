using System;
using UnityEngine;

public class LighthouseLogic : BuildingLogic
{
    [SerializeField] private LighthouseData data;

    public override int MaxLevel => data.maxLevel;
    public override string BuildingName => data.displayName;
    public override BuildingData BuildingData => data;

    public override float GetCycleTime() => data.GetCycleSeconds();
    public override int GetBuildCost() => data.GetBuildCost(IndexMultiplier);
    public override int GetUpgradeCost() => data.GetUpgradeCost(Level, IndexMultiplier);
    public override int GetCatchAmount() => 0; // doesnt need 


    public override async void StartWork(System.Threading.CancellationToken ct)
    {
        if (!IsBuilt) return;

        long carriedFish = Math.Min(CurrencyManager.Instance.Fish, CurrencyManager.Instance.MaxFishCapacity);
        if (carriedFish <= 0) return;

        long maxFish = CurrencyManager.Instance.MaxFishCapacity;
        float fullCycleTime = data.baseCycleSeconds;
        float cycleTime = fullCycleTime * ((float)carriedFish / maxFish);

        onStartWork?.Invoke();
        onProgressUpdate?.Invoke(0f);

        float timer = 0f;

        try
        {
            while (!ct.IsCancellationRequested && timer < cycleTime)
            {
                timer += Time.deltaTime;
                onProgressUpdate?.Invoke(timer / cycleTime);
                await Utilities.UTask.UTaskEx.NextFrame(ct);
            }
        }
        catch { }

        float progress = Mathf.Clamp01(timer / cycleTime);
        long actualSoldFish = Mathf.FloorToInt(carriedFish * progress);

        bool spent = CurrencyManager.Instance.TrySpendFish(actualSoldFish);
        if (!spent) actualSoldFish = 0;

        long totalGold = actualSoldFish * data.GetCatchAmount(Level);
        CurrencyManager.Instance.AddGold(totalGold);

        onYield?.Invoke((int)totalGold);
        onProgressUpdate?.Invoke(0f);
        onStopWork?.Invoke();
    }
    
    public override string GetCycleLabel() => "Speed";
    public override string GetAmountLabel() => "Fish Price";
}
